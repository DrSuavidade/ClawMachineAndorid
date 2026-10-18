using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Core.Services;

namespace ClawMachine.Gameplay
{
    [DefaultExecutionOrder(-90)]
    public class ClawAudio : MonoBehaviour, IAudioService
    {
        private static ClawAudio instance;
        public static ClawAudio Instance => instance;

        [Header("Assigned Audio Clips (Wired to Assets/Audio)")]
        [SerializeField] private AudioClip bgmLoopClip;
        [SerializeField] private AudioClip motorLoopClip;
        [SerializeField] private AudioClip clawClampClip;
        [SerializeField] private AudioClip grabSuccessClip;
        [SerializeField] private AudioClip dropReleaseClip;
        [SerializeField] private AudioClip prizeWinClip;
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip coinInsertClip;
        [SerializeField] private AudioClip prizeSlipClip;
        [SerializeField] private AudioClip carouselSwipeClip;
        [SerializeField] private AudioClip[] swooshClips;
        [SerializeField] private AudioClip countdownTickClip;
        [SerializeField] private AudioClip modalOpenClip;
        [SerializeField] private AudioClip errorBuzzClip;
        [SerializeField] private AudioClip coinRewardClip;
        [SerializeField] private AudioClip unlockFanfareClip;

        [Header("Master & Channel Volumes")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1.0f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.28f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1.0f;
        [SerializeField] [Range(0f, 1f)] private float motorVolume = 0.22f;

        [Header("Individual Sound Volumes (Tweak in Inspector)")]
        [SerializeField] [Range(0f, 2f)] private float clawCloseEmptyVolume = 0.70f;
        [SerializeField] [Range(0f, 2f)] private float clawCloseFullVolume = 0.85f;
        [SerializeField] [Range(0f, 2f)] private float dropFloorVolume = 0.75f;
        [SerializeField] [Range(0f, 2f)] private float prizeWinVolume = 1.00f;
        [SerializeField] [Range(0f, 2f)] private float uiClickVolume = 0.55f;
        [SerializeField] [Range(0f, 2f)] private float coinInsertVolume = 0.85f;
        [SerializeField] [Range(0f, 2f)] private float coinRewardVolume = 0.85f;
        [SerializeField] [Range(0f, 2f)] private float prizeSlipVolume = 0.80f;
        [SerializeField] [Range(0f, 2f)] private float swooshVolume = 0.65f;
        [SerializeField] [Range(0f, 2f)] private float countdownVolume = 0.75f;
        [SerializeField] [Range(0f, 2f)] private float modalOpenVolume = 0.55f;
        [SerializeField] [Range(0f, 2f)] private float errorBuzzVolume = 0.70f;
        [SerializeField] [Range(0f, 2f)] private float unlockFanfareVolume = 1.00f;

        [Header("Audio Ducking")]
        [Tooltip("Volume multiplier applied to BGM during priority SFX (win, fail, unlock). 0 = complete silence, 0.15 = quiet background.")]
        [SerializeField] [Range(0f, 1f)] private float duckedMusicVolume = 0.15f;
        [Tooltip("If true, BGM is fully paused rather than reduced in volume.")]
        [SerializeField] private bool pauseMusicOnPrioritySFX = false;

        private AudioSource sfxSource;
        private AudioSource countdownSource;
        private AudioSource motorSource;
        private AudioSource bgmSource;
        private AudioSource bgmAltSource;
        private Coroutine bgmCrossfadeRoutine;
        private Coroutine duckRoutine;
        private int swooshIndex;

        private readonly List<AudioClip> proceduralClips = new List<AudioClip>();
        private readonly Dictionary<string, AudioClip> machineBgmCache = new Dictionary<string, AudioClip>();

        private ClawCameraController cachedCamController;

        // Structured Audio & Channel Settings
        private bool isSFXEnabled = true;
        private bool isMusicEnabled = true;
        private bool isHapticsEnabled = true;

        private const string PREF_SFX = "Claw_SFX_Enabled";
        private const string PREF_MUSIC = "Claw_Music_Enabled";
        private const string PREF_HAPTICS = "Claw_Haptics_Enabled";
        private const string PREF_MASTER_VOL = "Claw_Master_Volume";
        private const string PREF_SFX_VOL = "Claw_SFX_Volume";
        private const string PREF_MUSIC_VOL = "Claw_Music_Volume";

        public bool IsSFXEnabled
        {
            get => isSFXEnabled;
            set
            {
                isSFXEnabled = value;
                PlayerPrefs.SetInt(PREF_SFX, isSFXEnabled ? 1 : 0);
                PlayerPrefs.Save();
                UpdateChannelVolumes();
            }
        }

        public bool IsMusicEnabled
        {
            get => isMusicEnabled;
            set
            {
                isMusicEnabled = value;
                PlayerPrefs.SetInt(PREF_MUSIC, isMusicEnabled ? 1 : 0);
                PlayerPrefs.Save();
                UpdateChannelVolumes();
            }
        }

        public bool IsHapticsEnabled
        {
            get => isHapticsEnabled;
            set
            {
                isHapticsEnabled = value;
                PlayerPrefs.SetInt(PREF_HAPTICS, isHapticsEnabled ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_MASTER_VOL, masterVolume);
                PlayerPrefs.Save();
                UpdateChannelVolumes();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SFX_VOL, sfxVolume);
                PlayerPrefs.Save();
                UpdateChannelVolumes();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_MUSIC_VOL, musicVolume);
                PlayerPrefs.Save();
                UpdateChannelVolumes();
            }
        }

        public float MotorVolume
        {
            get => motorVolume;
            set
            {
                motorVolume = Mathf.Clamp01(value);
                UpdateChannelVolumes();
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateChannelVolumes();
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            ServiceLocator.Register<IAudioService>(this);

            isSFXEnabled = PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
            isMusicEnabled = PlayerPrefs.GetInt(PREF_MUSIC, 1) == 1;
            isHapticsEnabled = PlayerPrefs.GetInt(PREF_HAPTICS, 1) == 1;
            masterVolume = PlayerPrefs.GetFloat(PREF_MASTER_VOL, masterVolume);
            sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, sfxVolume);
            musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, musicVolume);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            countdownSource = gameObject.AddComponent<AudioSource>();
            countdownSource.playOnAwake = false;
            countdownSource.loop = false;

            motorSource = gameObject.AddComponent<AudioSource>();
            motorSource.playOnAwake = false;
            motorSource.loop = true;

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;

            bgmAltSource = gameObject.AddComponent<AudioSource>();
            bgmAltSource.playOnAwake = false;
            bgmAltSource.loop = true;
            bgmAltSource.volume = 0f;

            cachedCamController = FindFirstObjectByType<ClawCameraController>();

            EnsureClips();
            UpdateChannelVolumes();

            if (bgmSource.clip != null && isMusicEnabled)
            {
                bgmSource.Play();
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                // Pre-cache and preload all machine BGM tracks so switching machines has zero latency
                if (coll is CollectionManager cm && cm.Catalog != null && cm.Catalog.Machines != null)
                {
                    var machines = cm.Catalog.Machines;
                    for (int i = 0; i < machines.Length; i++)
                    {
                        var m = machines[i];
                        if (m != null && m.bgmClip != null)
                        {
                            m.bgmClip.LoadAudioData();
                            machineBgmCache[m.machineId] = m.bgmClip;
                        }
                    }
                }

                coll.OnCurrentMachineChanged -= HandleMachineChanged;
                coll.OnCurrentMachineChanged += HandleMachineChanged;
                if (coll.CurrentMachine != null)
                {
                    SetMachineBgm(coll.CurrentMachine.machineId, coll.CurrentMachine.bgmClip);
                }
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                coll.OnCurrentMachineChanged -= HandleMachineChanged;
            }

            for (int i = 0; i < proceduralClips.Count; i++)
            {
                if (proceduralClips[i] != null) Destroy(proceduralClips[i]);
            }
            proceduralClips.Clear();
            machineBgmCache.Clear();

            if (instance == this)
            {
                ServiceLocator.Unregister<IAudioService>();
                instance = null;
            }
        }

        private void HandleMachineChanged(Data.MachineDefinition machine)
        {
            if (machine != null)
            {
                SetMachineBgm(machine.machineId, machine.bgmClip);
            }
        }

        public void SetMachineBgm(string machineId, AudioClip customClip = null)
        {
            if (bgmSource == null) return;

            AudioClip target = customClip;
            if (target == null && !string.IsNullOrEmpty(machineId))
            {
                if (!machineBgmCache.TryGetValue(machineId, out target) || target == null)
                {
                    string fileName = GetMachineBgmFileName(machineId);
                    target = TryLoadAudioAsset($"Music/{fileName}");
                    if (target == null)
                    {
                        target = bgmLoopClip != null ? bgmLoopClip : RegisterProcedural(CreateThemedMachineBgm(22050, machineId));
                    }
                    machineBgmCache[machineId] = target;
                }
            }

            if (target == null) target = bgmLoopClip;
            if (target == null || bgmSource.clip == target) return;

            target.LoadAudioData();

            // Synchronize playback position to the exact sample/time of the previous machine's track
            int currentSamples = bgmSource.isPlaying ? bgmSource.timeSamples : 0;
            float currentTime = bgmSource.isPlaying ? bgmSource.time : 0f;

            if (bgmAltSource != null && isMusicEnabled && bgmSource.isPlaying)
            {
                if (bgmCrossfadeRoutine != null) StopCoroutine(bgmCrossfadeRoutine);
                bgmCrossfadeRoutine = StartCoroutine(CrossfadeBgmRoutine(target, currentSamples, currentTime));
            }
            else
            {
                bgmSource.clip = target;
                if (target.samples > 0 && currentSamples > 0)
                {
                    bgmSource.timeSamples = currentSamples % target.samples;
                }
                else if (target.length > 0f && currentTime > 0f)
                {
                    bgmSource.time = currentTime % target.length;
                }
                if (isMusicEnabled) bgmSource.Play();
            }
        }

        private System.Collections.IEnumerator CrossfadeBgmRoutine(AudioClip nextClip, int syncSamples, float syncTime)
        {
            bgmAltSource.clip = nextClip;
            if (nextClip.samples > 0 && syncSamples > 0)
            {
                bgmAltSource.timeSamples = syncSamples % nextClip.samples;
            }
            else if (nextClip.length > 0f && syncTime > 0f)
            {
                bgmAltSource.time = syncTime % nextClip.length;
            }

            bgmAltSource.volume = 0f;
            bgmAltSource.Play();

            float duration = 0.22f;
            float elapsed = 0f;
            float targetVol = isMusicEnabled ? (masterVolume * musicVolume) : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                targetVol = isMusicEnabled ? (masterVolume * musicVolume) : 0f;
                bgmSource.volume = targetVol * (1f - t);
                bgmAltSource.volume = targetVol * t;
                yield return null;
            }

            bgmSource.Stop();
            // Swap active and alt sources so bgmSource is always the active playing track
            AudioSource temp = bgmSource;
            bgmSource = bgmAltSource;
            bgmAltSource = temp;
            bgmSource.volume = isMusicEnabled ? (masterVolume * musicVolume) : 0f;
            bgmAltSource.volume = 0f;
            bgmCrossfadeRoutine = null;
        }

        private static string GetMachineBgmFileName(string machineId) => machineId switch
        {
            "toy_box" => "Music_Room",
            "retro_arcade" => "Music_Retro",
            "sweet_candy" => "Music_Sweet",
            "ocean_abyss" => "Music_Ocean",
            "cosmic_galaxy" => "Music_Cosmic",
            "fantasy_dungeon" => "Music_Fantasy",
            _ => "Music_Room"
        };

        private void UpdateChannelVolumes()
        {
            float effectiveSFX = isSFXEnabled ? (masterVolume * sfxVolume) : 0f;
            float effectiveMotor = isSFXEnabled ? (masterVolume * motorVolume) : 0f;
            float effectiveMusic = isMusicEnabled ? (masterVolume * musicVolume) : 0f;

            if (sfxSource != null) sfxSource.volume = effectiveSFX;
            if (countdownSource != null)
            {
                countdownSource.volume = effectiveSFX * countdownVolume;
                if (!isSFXEnabled && countdownSource.isPlaying) countdownSource.Stop();
            }
            if (motorSource != null) motorSource.volume = effectiveMotor;
            if (bgmSource != null)
            {
                bgmSource.volume = effectiveMusic;
                if (isMusicEnabled && !bgmSource.isPlaying && bgmSource.clip != null) bgmSource.Play();
                else if (!isMusicEnabled && bgmSource.isPlaying) bgmSource.Pause();
            }
        }

        private AudioClip RegisterProcedural(AudioClip clip)
        {
            if (clip != null) proceduralClips.Add(clip);
            return clip;
        }

        private static AudioClip TryLoadAudioAsset(string subPath)
        {
            var res = Resources.Load<AudioClip>($"Audio/{subPath}");
            if (res != null) return res;
#if UNITY_EDITOR
            var editorClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/{subPath}.wav");
            if (editorClip != null) return editorClip;
#endif
            return null;
        }

        private static AudioClip LoadOrFallback(AudioClip assigned, string subPath, System.Func<AudioClip> fallbackFactory)
        {
            if (assigned != null) return assigned;
            var loaded = TryLoadAudioAsset(subPath);
            return loaded != null ? loaded : fallbackFactory();
        }

        private void EnsureClips()
        {
            motorLoopClip = LoadOrFallback(motorLoopClip, "SFX/Moving", () => RegisterProcedural(CreateMotorTone(44100, 0.4f, 105f)));
            if (motorSource != null) motorSource.clip = motorLoopClip;

            if (bgmLoopClip == null) bgmLoopClip = TryLoadAudioAsset("Music/Music_Room");
            AudioClip initialBgm = bgmLoopClip != null ? bgmLoopClip : RegisterProcedural(CreateThemedMachineBgm(22050, "toy_box"));
            machineBgmCache["toy_box"] = initialBgm;
            if (bgmSource != null) bgmSource.clip = initialBgm;

            clawClampClip = LoadOrFallback(clawClampClip, "SFX/ClawCloseEmpty", () => RegisterProcedural(CreateMechanicalClick(44100, 0.12f)));
            grabSuccessClip = LoadOrFallback(grabSuccessClip, "SFX/ClawCloseFull", () => RegisterProcedural(CreateChord(44100, 0.25f, new float[] { 523.25f, 659.25f, 783.99f })));
            dropReleaseClip = LoadOrFallback(dropReleaseClip, "SFX/DropFloor", () => RegisterProcedural(CreateDropThud(44100, 0.18f)));
            prizeWinClip = LoadOrFallback(prizeWinClip, "SFX/Sucess", () => RegisterProcedural(CreateVictoryArpeggio(44100, 0.55f)));
            uiClickClip = LoadOrFallback(uiClickClip, "SFX/Click", () => RegisterProcedural(CreateChord(44100, 0.045f, new float[] { 880f, 1174.66f })));
            coinInsertClip = LoadOrFallback(coinInsertClip, "SFX/Coin", () => RegisterProcedural(CreateCoinInsertTone(44100)));
            prizeSlipClip = LoadOrFallback(prizeSlipClip, "SFX/Fail", () => RegisterProcedural(CreatePrizeSlipTone(44100)));

            if (swooshClips == null || swooshClips.Length == 0)
            {
                var s1 = TryLoadAudioAsset("SFX/Swoosh1");
                var s2 = TryLoadAudioAsset("SFX/Swoosh2");
                var s3 = TryLoadAudioAsset("SFX/Swoosh3");
                if (s1 != null || s2 != null || s3 != null)
                {
                    var list = new List<AudioClip>(3);
                    if (s1 != null) list.Add(s1);
                    if (s2 != null) list.Add(s2);
                    if (s3 != null) list.Add(s3);
                    swooshClips = list.ToArray();
                }
            }

            carouselSwipeClip = LoadOrFallback(carouselSwipeClip, "SFX/Swoosh1", () => RegisterProcedural(CreateMechanicalClick(44100, 0.09f)));
            countdownTickClip = LoadOrFallback(countdownTickClip, "SFX/Countdown", () => RegisterProcedural(CreateChord(44100, 0.05f, new float[] { 784f })));
            if (countdownSource != null) countdownSource.clip = countdownTickClip;

            modalOpenClip = LoadOrFallback(modalOpenClip, "SFX/Swoosh2", () => RegisterProcedural(CreateModalOpenTone(44100)));
            errorBuzzClip = LoadOrFallback(errorBuzzClip, "SFX/Error", () => RegisterProcedural(CreateErrorBuzzTone(44100)));
            coinRewardClip = LoadOrFallback(coinRewardClip, "SFX/Coin", () => RegisterProcedural(CreateCoinRewardTone(44100)));
            unlockFanfareClip = LoadOrFallback(unlockFanfareClip, "SFX/Unlock", () => RegisterProcedural(CreateUnlockFanfareTone(44100)));
        }

        private AudioClip GetNextSwooshClip(AudioClip fallback)
        {
            if (swooshClips != null && swooshClips.Length > 0)
            {
                for (int i = 0; i < swooshClips.Length; i++)
                {
                    int idx = (swooshIndex + i) % swooshClips.Length;
                    if (swooshClips[idx] != null)
                    {
                        swooshIndex = (idx + 1) % swooshClips.Length;
                        return swooshClips[idx];
                    }
                }
            }
            return fallback;
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (!isSFXEnabled || sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        public void SetMotorMoving(bool moving)
        {
            if (motorSource == null || !isSFXEnabled)
            {
                if (motorSource != null && motorSource.isPlaying) motorSource.Stop();
                return;
            }

            if (moving && !motorSource.isPlaying)
            {
                motorSource.Play();
            }
            else if (!moving && motorSource.isPlaying)
            {
                motorSource.Stop();
            }
        }

        public void PlayClamp()
        {
            PlaySFX(clawClampClip, clawCloseEmptyVolume);
            TriggerCameraShake(0.08f, 0.03f);
        }

        public void PlayGrab()
        {
            PlaySFX(grabSuccessClip, clawCloseFullVolume);
            TriggerCameraShake(0.12f, 0.05f);
            TriggerHapticLight();
        }

        public void PlayGrabSuccess() => PlayGrab();

        public void PlayDrop()
        {
            PlaySFX(dropReleaseClip, dropFloorVolume);
            TriggerCameraShake(0.10f, 0.04f);
        }

        public void PlayDropFloor() => PlayDrop();

        public void DuckMusic(float duration, float targetMultiplier = -1f)
        {
            if (!isMusicEnabled || bgmSource == null || !bgmSource.isPlaying) return;
            if (duckRoutine != null) StopCoroutine(duckRoutine);
            duckRoutine = StartCoroutine(CoDuckMusic(duration, targetMultiplier));
        }

        private System.Collections.IEnumerator CoDuckMusic(float duration, float targetMultiplier)
        {
            if (bgmSource == null) yield break;

            float normalVol = isMusicEnabled ? (masterVolume * musicVolume) : 0f;
            float mult = targetMultiplier >= 0f ? targetMultiplier : duckedMusicVolume;
            float targetVol = normalVol * mult;

            if (pauseMusicOnPrioritySFX)
            {
                bgmSource.Pause();
                yield return new WaitForSecondsRealtime(duration);
                if (isMusicEnabled) bgmSource.UnPause();
                duckRoutine = null;
                yield break;
            }

            // Fast dip down (0.08s)
            float dipDur = 0.08f;
            float elapsed = 0f;
            float startVol = bgmSource.volume;
            while (elapsed < dipDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dipDur);
                bgmSource.volume = Mathf.Lerp(startVol, targetVol, t);
                yield return null;
            }
            bgmSource.volume = targetVol;

            // Hold ducked for duration of priority SFX
            if (duration > dipDur)
            {
                yield return new WaitForSecondsRealtime(duration - dipDur);
            }

            // Smooth fade back up (0.28s)
            float riseDur = 0.28f;
            elapsed = 0f;
            while (elapsed < riseDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / riseDur);
                normalVol = isMusicEnabled ? (masterVolume * musicVolume) : 0f;
                bgmSource.volume = Mathf.Lerp(targetVol, normalVol, t);
                yield return null;
            }

            normalVol = isMusicEnabled ? (masterVolume * musicVolume) : 0f;
            bgmSource.volume = normalVol;
            duckRoutine = null;
        }

        public void PlayWin()
        {
            float dur = (prizeWinClip != null && prizeWinClip.length > 0f) ? prizeWinClip.length : 2.5f;
            DuckMusic(dur);
            PlaySFX(prizeWinClip, prizeWinVolume);
            TriggerCameraShake(0.25f, 0.08f);
            TriggerHapticSuccess();
        }

        public void PlayUIClick()
        {
            PlaySFX(uiClickClip, uiClickVolume);
            TriggerHapticLight();
        }

        public void PlayCoinInsert() => PlaySFX(coinInsertClip, coinInsertVolume);

        public void PlayPrizeSlip()
        {
            float dur = (prizeSlipClip != null && prizeSlipClip.length > 0f) ? prizeSlipClip.length : 0.85f;
            DuckMusic(dur);
            PlaySFX(prizeSlipClip, prizeSlipVolume);
        }

        public void PlayFail() => PlayPrizeSlip();

        public void PlayCarouselSwipe()
        {
            PlaySFX(GetNextSwooshClip(carouselSwipeClip), swooshVolume);
        }

        public void PlayCountdownTick()
        {
            if (!isSFXEnabled || countdownSource == null || countdownTickClip == null) return;
            countdownSource.volume = (masterVolume * sfxVolume) * countdownVolume;
            countdownSource.clip = countdownTickClip;
            countdownSource.time = 0f;
            countdownSource.Play();
        }

        public void StopCountdown()
        {
            if (countdownSource != null && countdownSource.isPlaying)
            {
                countdownSource.Stop();
            }
        }

        public void PlayModalOpen()
        {
            PlaySFX(GetNextSwooshClip(modalOpenClip), modalOpenVolume);
        }

        public void PlayErrorBuzz()
        {
            PlaySFX(errorBuzzClip, errorBuzzVolume);
            TriggerHapticLight();
        }

        public void PlayCoinReward()
        {
            PlaySFX(coinRewardClip, coinRewardVolume);
            TriggerHapticSuccess();
        }

        public void PlayUnlockFanfare()
        {
            float dur = (unlockFanfareClip != null && unlockFanfareClip.length > 0f) ? unlockFanfareClip.length : 1.5f;
            DuckMusic(dur);
            PlaySFX(unlockFanfareClip, unlockFanfareVolume);
            TriggerCameraShake(0.30f, 0.09f);
            TriggerHapticSuccess();
        }

        public void TriggerCameraShake(float duration = 0.2f, float magnitude = 0.04f)
        {
            if (cachedCamController == null) cachedCamController = FindFirstObjectByType<ClawCameraController>();
            if (cachedCamController != null)
            {
                cachedCamController.TriggerShake(duration, magnitude);
            }
        }

        public void TriggerShake(float duration, float magnitude) => TriggerCameraShake(duration, magnitude);

        public void TriggerHapticLight()
        {
            if (!isHapticsEnabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            TriggerAndroidVibrate(20);
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public void TriggerHapticSuccess()
        {
            if (!isHapticsEnabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            TriggerAndroidVibrate(50);
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private void TriggerAndroidVibrate(long milliseconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    if (vibrator != null)
                    {
                        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                        {
                            int sdkInt = version.GetStatic<int>("SDK_INT");
                            if (sdkInt >= 26)
                            {
                                using (var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                                using (var effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1))
                                {
                                    vibrator.Call("vibrate", effect);
                                }
                            }
                            else
                            {
                                vibrator.Call("vibrate", milliseconds);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ClawAudio] Android haptics fallback: {ex.Message}");
            }
#endif
        }

        private AudioClip CreateThemedMachineBgm(int sampleRate, string machineId)
        {
            float[] melody;
            float noteDur = 0.25f;
            float decay = 6.5f;

            switch (machineId)
            {
                case "retro_arcade":
                    noteDur = 0.20f;
                    decay = 8.0f;
                    melody = new float[] { 220f, 261.63f, 329.63f, 440f, 196f, 246.94f, 293.66f, 392f,
                                           174.61f, 220f, 261.63f, 349.23f, 196f, 246.94f, 329.63f, 392f };
                    break;
                case "sweet_candy":
                    noteDur = 0.22f;
                    decay = 5.5f;
                    melody = new float[] { 349.23f, 440f, 523.25f, 698.46f, 392f, 493.88f, 587.33f, 783.99f,
                                           349.23f, 440f, 523.25f, 659.25f, 349.23f, 523.25f, 440f, 349.23f };
                    break;
                case "ocean_abyss":
                    noteDur = 0.34f;
                    decay = 3.5f;
                    melody = new float[] { 146.83f, 220f, 261.63f, 329.63f, 164.81f, 246.94f, 293.66f, 369.99f,
                                           146.83f, 220f, 293.66f, 349.23f, 130.81f, 196f, 246.94f, 293.66f };
                    break;
                case "cosmic_galaxy":
                    noteDur = 0.26f;
                    decay = 4.5f;
                    melody = new float[] { 164.81f, 246.94f, 329.63f, 493.88f, 196f, 293.66f, 392f, 587.33f,
                                           146.83f, 220f, 293.66f, 440f, 164.81f, 246.94f, 329.63f, 392f };
                    break;
                case "fantasy_dungeon":
                    noteDur = 0.28f;
                    decay = 5.0f;
                    melody = new float[] { 220f, 261.63f, 329.63f, 369.99f, 293.66f, 349.23f, 440f, 329.63f,
                                           196f, 246.94f, 293.66f, 392f, 220f, 293.66f, 261.63f, 220f };
                    break;
                default: // toy_box
                    melody = new float[] { 261.63f, 329.63f, 392.00f, 523.25f, 293.66f, 349.23f, 440.00f, 587.33f,
                                           246.94f, 329.63f, 392.00f, 493.88f, 261.63f, 329.63f, 392.00f, 523.25f };
                    break;
            }

            float totalDur = melody.Length * noteDur;
            int samples = (int)(sampleRate * totalDur);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                int idx = Mathf.Clamp((int)(t / noteDur), 0, melody.Length - 1);
                float nt = t - idx * noteDur;
                float env = Mathf.Exp(-nt * decay);
                float wave = Mathf.Sin(2f * Mathf.PI * melody[idx] * nt) * 0.6f
                           + Mathf.Sin(2f * Mathf.PI * (melody[idx] * 0.5f) * nt) * 0.4f;
                data[i] = wave * env * 0.22f;
            }
            AudioClip clip = AudioClip.Create($"Bgm_{machineId}", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCoinInsertTone(int sampleRate)
        {
            float duration = 0.28f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float f = t < 0.09f ? 1174.66f : 1567.98f;
                float localT = t < 0.09f ? t : (t - 0.09f);
                float env = Mathf.Exp(-localT * 16f);
                data[i] = Mathf.Sin(2f * Mathf.PI * f * localT) * env * 0.55f;
            }
            AudioClip clip = AudioClip.Create("CoinInsert", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreatePrizeSlipTone(int sampleRate)
        {
            float duration = 0.45f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float pitch = Mathf.Lerp(420f, 130f, t / duration);
                float env = Mathf.Exp(-t * 5.5f);
                data[i] = Mathf.Sin(2f * Mathf.PI * pitch * t) * env * 0.55f;
            }
            AudioClip clip = AudioClip.Create("PrizeSlip", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateModalOpenTone(int sampleRate)
        {
            float duration = 0.12f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float pitch = Mathf.Lerp(320f, 580f, t / duration);
                float env = Mathf.Exp(-t * 18f);
                data[i] = Mathf.Sin(2f * Mathf.PI * pitch * t) * env * 0.45f;
            }
            AudioClip clip = AudioClip.Create("ModalOpen", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateErrorBuzzTone(int sampleRate)
        {
            float duration = 0.24f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float f = t < 0.11f ? 165f : 130f;
                float localT = t < 0.11f ? t : (t - 0.11f);
                float env = Mathf.Exp(-localT * 14f);
                float sq = Mathf.Sin(2f * Mathf.PI * f * t) >= 0f ? 0.5f : -0.5f;
                data[i] = (sq * 0.5f + Mathf.Sin(2f * Mathf.PI * f * t) * 0.5f) * env * 0.55f;
            }
            AudioClip clip = AudioClip.Create("ErrorBuzz", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCoinRewardTone(int sampleRate)
        {
            float[] notes = { 987.77f, 1174.66f, 1318.51f, 1567.98f, 1975.53f };
            float noteDur = 0.09f;
            float duration = notes.Length * noteDur;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                int idx = Mathf.Clamp((int)(t / noteDur), 0, notes.Length - 1);
                float nt = t - idx * noteDur;
                float env = Mathf.Exp(-nt * 14f);
                data[i] = Mathf.Sin(2f * Mathf.PI * notes[idx] * nt) * env * 0.55f;
            }
            AudioClip clip = AudioClip.Create("CoinReward", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateUnlockFanfareTone(int sampleRate)
        {
            float[] notes = { 392.00f, 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f };
            float noteDur = 0.16f;
            float duration = notes.Length * noteDur;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                int idx = Mathf.Clamp((int)(t / noteDur), 0, notes.Length - 1);
                float nt = t - idx * noteDur;
                float env = Mathf.Exp(-nt * 6.5f);
                float wave = Mathf.Sin(2f * Mathf.PI * notes[idx] * nt) * 0.65f
                           + Mathf.Sin(2f * Mathf.PI * (notes[idx] * 0.5f) * nt) * 0.35f;
                data[i] = wave * env * 0.60f;
            }
            AudioClip clip = AudioClip.Create("UnlockFanfare", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateMotorTone(int sampleRate, float duration, float freq)
        {
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float wave = (Mathf.Repeat(t * freq, 1f) * 2f - 1f) * 0.4f + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.6f;
                data[i] = wave * 0.35f;
            }
            AudioClip clip = AudioClip.Create("MotorTone", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateMechanicalClick(int sampleRate, float duration)
        {
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float decay = Mathf.Exp(-t * 22f);
                float noise = Random.Range(-1f, 1f);
                float impact = Mathf.Sin(2f * Mathf.PI * 340f * (i / (float)sampleRate));
                data[i] = (noise * 0.4f + impact * 0.6f) * decay;
            }
            AudioClip clip = AudioClip.Create("ClawClamp", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateChord(int sampleRate, float duration, float[] freqs)
        {
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Exp(-t * 6f);
                float sum = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    sum += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                }
                data[i] = (sum / freqs.Length) * env * 0.5f;
            }
            AudioClip clip = AudioClip.Create("GrabPing", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateDropThud(int sampleRate, float duration)
        {
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Exp(-t * 12f);
                float pitch = Mathf.Lerp(180f, 60f, t / duration);
                float wave = Mathf.Sin(2f * Mathf.PI * pitch * t);
                data[i] = wave * env * 0.7f;
            }
            AudioClip clip = AudioClip.Create("DropThud", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateVictoryArpeggio(int sampleRate, float duration)
        {
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            float noteDur = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                int noteIdx = Mathf.Clamp((int)(t / noteDur), 0, notes.Length - 1);
                float noteT = t - (noteIdx * noteDur);
                float env = Mathf.Exp(-noteT * 8f);
                float wave = Mathf.Sin(2f * Mathf.PI * notes[noteIdx] * noteT);
                data[i] = wave * env * 0.5f;
            }
            AudioClip clip = AudioClip.Create("WinJingle", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
