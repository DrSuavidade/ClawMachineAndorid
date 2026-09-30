using System;

namespace ClawMachine.Core.Services
{
    public interface ISaveService
    {
        bool Exists(string key);
        T Load<T>(string key, int currentVersion, Func<string, int, T> migrationHandler = null) where T : class, new();
        void Save<T>(string key, T data, int version);
        void Delete(string key);
    }
}
