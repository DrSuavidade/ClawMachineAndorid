using UnityEngine;

namespace ClawMachine.Core.Services
{
    public interface IAudioService
    {
        bool IsSFXEnabled { get; set; }
        bool IsMusicEnabled { get; set; }
        bool IsHapticsEnabled { get; set; }
        float MasterVolume { get; set; }
        float SFXVolume { get; set; }
        float MusicVolume { get; set; }
        float MotorVolume { get; set; }

        void PlaySFX(AudioClip clip, float volume = 1f);
        void PlayClamp();
        void PlayGrabSuccess();
        void PlayDropFloor();
        void PlayWin();
        void PlayUIClick();
        void PlayCoinInsert();
        void PlayPrizeSlip();
        void PlayFail();
        void DuckMusic(float duration, float targetMultiplier = -1f);
        void PlayCarouselSwipe();
        void PlayCountdownTick();
        void StopCountdown();
        void PlayModalOpen();
        void PlayErrorBuzz();
        void PlayCoinReward();
        void PlayUnlockFanfare();
        void SetMachineBgm(string machineId, AudioClip customClip = null);
        void SetMotorMoving(bool moving);

        void TriggerHapticLight();
        void TriggerHapticSuccess();
        void TriggerCameraShake(float duration = 0.2f, float magnitude = 0.04f);
    }
}

