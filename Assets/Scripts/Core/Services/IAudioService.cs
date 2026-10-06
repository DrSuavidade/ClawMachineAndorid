using UnityEngine;

namespace ClawMachine.Core.Services
{
    public interface IAudioService
    {
        bool IsSFXEnabled { get; set; }
        bool IsHapticsEnabled { get; set; }
        float MasterVolume { get; set; }
        float SFXVolume { get; set; }
        float MotorVolume { get; set; }

        void PlaySFX(AudioClip clip, float volume = 1f);
        void PlayClamp();
        void PlayGrabSuccess();
        void PlayDropFloor();
        void PlayWin();
        void SetMotorMoving(bool moving);

        void TriggerHapticLight();
        void TriggerHapticSuccess();
        void TriggerCameraShake(float duration = 0.2f, float magnitude = 0.04f);
    }
}
