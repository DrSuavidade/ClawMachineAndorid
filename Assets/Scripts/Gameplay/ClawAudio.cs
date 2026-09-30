using UnityEngine;

namespace ClawMachine.Gameplay
{
    public class ClawAudio : MonoBehaviour
    {
        private static ClawAudio instance;
        public static ClawAudio Instance => instance;

        private AudioSource sfxSource;
        private AudioSource motorSource;

        private AudioClip motorClip;
        private AudioClip clampClip;
        private AudioClip grabClip;
        private AudioClip dropClip;
        private AudioClip winClip;

        private Camera mainCam;
        private Vector3 camOriginalPos;
        private float shakeTime;
        private float shakeMag;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            motorSource = gameObject.AddComponent<AudioSource>();
            motorSource.playOnAwake = false;
            motorSource.loop = true;
            motorSource.volume = 0.22f;

            mainCam = Camera.main;
            if (mainCam != null)
            {
                camOriginalPos = mainCam.transform.position;
            }

            GenerateClips();
        }

        private void GenerateClips()
        {
            motorClip = CreateMotorTone(44100, 0.4f, 105f);
            motorSource.clip = motorClip;

            clampClip = CreateMechanicalClick(44100, 0.12f);
            grabClip = CreateChord(44100, 0.25f, new float[] { 523.25f, 659.25f, 783.99f }); // C Major
            dropClip = CreateDropThud(44100, 0.18f);
            winClip = CreateVictoryArpeggio(44100, 0.55f);
        }

        public void SetMotorMoving(bool moving)
        {
            if (motorSource == null) return;
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
            if (sfxSource != null && clampClip != null)
            {
                sfxSource.PlayOneShot(clampClip, 0.65f);
            }
            TriggerShake(0.08f, 0.03f);
        }

        public void PlayGrab()
        {
            if (sfxSource != null && grabClip != null)
            {
                sfxSource.PlayOneShot(grabClip, 0.8f);
            }
            TriggerShake(0.12f, 0.05f);
            TryVibrate();
        }

        public void PlayDrop()
        {
            if (sfxSource != null && dropClip != null)
            {
                sfxSource.PlayOneShot(dropClip, 0.7f);
            }
            TriggerShake(0.10f, 0.04f);
        }

        public void PlayWin()
        {
            if (sfxSource != null && winClip != null)
            {
                sfxSource.PlayOneShot(winClip, 1.0f);
            }
            TriggerShake(0.25f, 0.08f);
            TryVibrate();
        }

        public void TriggerShake(float duration, float magnitude)
        {
            shakeTime = duration;
            shakeMag = magnitude;
        }

        private void TryVibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (PlayerPrefs.GetInt("Claw_Haptics_Enabled", 1) == 1)
            {
                Handheld.Vibrate();
            }
#endif
        }

        private void LateUpdate()
        {
            if (mainCam == null) return;

            if (shakeTime > 0f)
            {
                shakeTime -= Time.deltaTime;
                Vector3 offset = Random.insideUnitSphere * shakeMag;
                offset.z = 0; // maintain camera distance
                mainCam.transform.position = camOriginalPos + offset;
            }
            else
            {
                mainCam.transform.position = camOriginalPos;
            }
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
