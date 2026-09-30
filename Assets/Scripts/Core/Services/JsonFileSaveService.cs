using System;
using System.IO;
using UnityEngine;

namespace ClawMachine.Core.Services
{
    [Serializable]
    public class SaveEnvelope<T>
    {
        public int version;
        public long timestamp;
        public T payload;
    }

    public class JsonFileSaveService : ISaveService
    {
        private readonly string baseDirectory;

        public JsonFileSaveService(string customDirectory = null)
        {
            baseDirectory = string.IsNullOrEmpty(customDirectory)
                ? Application.persistentDataPath
                : customDirectory;

            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }
        }

        private string GetFilePath(string key) => Path.Combine(baseDirectory, $"{key}.json");
        private string GetTempFilePath(string key) => Path.Combine(baseDirectory, $"{key}.tmp");

        public bool Exists(string key)
        {
            return File.Exists(GetFilePath(key)) || PlayerPrefs.HasKey(key);
        }

        public T Load<T>(string key, int currentVersion, Func<string, int, T> migrationHandler = null) where T : class, new()
        {
            string filePath = GetFilePath(key);

            // 1. Try loading from persistent disk file
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    SaveEnvelope<T> envelope = JsonUtility.FromJson<SaveEnvelope<T>>(json);

                    if (envelope != null && envelope.payload != null)
                    {
                        if (envelope.version < currentVersion && migrationHandler != null)
                        {
                            T migrated = migrationHandler(json, envelope.version);
                            Save(key, migrated, currentVersion);
                            return migrated;
                        }
                        return envelope.payload;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonFileSaveService] Failed to load save file at {filePath}: {ex.Message}");
                }
            }

            // 2. Legacy fallback to PlayerPrefs
            if (PlayerPrefs.HasKey(key))
            {
                try
                {
                    string legacyJson = PlayerPrefs.GetString(key);
                    T legacyData = JsonUtility.FromJson<T>(legacyJson);
                    if (legacyData != null)
                    {
                        Debug.Log($"[JsonFileSaveService] Migrated legacy PlayerPrefs data '{key}' to disk file.");
                        Save(key, legacyData, currentVersion);
                        return legacyData;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[JsonFileSaveService] Legacy PlayerPrefs read failed for '{key}': {ex.Message}");
                }
            }

            // 3. Brand new save
            T freshData = new T();
            Save(key, freshData, currentVersion);
            return freshData;
        }

        public void Save<T>(string key, T data, int version)
        {
            if (data == null) return;

            string filePath = GetFilePath(key);
            string tempPath = GetTempFilePath(key);

            try
            {
                SaveEnvelope<T> envelope = new SaveEnvelope<T>
                {
                    version = version,
                    timestamp = DateTime.UtcNow.Ticks,
                    payload = data
                };

                string json = JsonUtility.ToJson(envelope, true);

                // Atomic write pattern: Write to temp, then replace destination
                File.WriteAllText(tempPath, json);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonFileSaveService] Failed to save '{key}': {ex.Message}");
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        public void Delete(string key)
        {
            string filePath = GetFilePath(key);
            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); } catch { }
            }

            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }
    }
}
