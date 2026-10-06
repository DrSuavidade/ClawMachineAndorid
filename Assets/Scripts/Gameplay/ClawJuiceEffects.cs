using System.Collections.Generic;
using UnityEngine;

namespace ClawMachine.Gameplay
{
    public class ClawJuiceEffects : MonoBehaviour
    {
        private static ClawJuiceEffects instance;
        public static ClawJuiceEffects Instance => instance;

        private ParticleSystem confettiPS;
        private ParticleSystem coinShowerPS;
        private ParticleSystem dustPuffPS;
        private ParticleSystem strainSparksPS;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            BuildConfettiParticleSystem();
            BuildCoinShowerParticleSystem();
            BuildDustPuffParticleSystem();
            BuildStrainSparksParticleSystem();
        }

        public void PlayWinCelebration(Vector3 position)
        {
            if (confettiPS != null)
            {
                confettiPS.transform.position = position + Vector3.up * 0.5f;
                confettiPS.Emit(60);
            }

            if (coinShowerPS != null)
            {
                coinShowerPS.transform.position = position + new Vector3(0f, -0.2f, 0f);
                coinShowerPS.Emit(25);
            }
        }

        public void PlayDustPuff(Vector3 position)
        {
            if (dustPuffPS != null)
            {
                dustPuffPS.transform.position = position;
                dustPuffPS.Emit(12);
            }
        }

        public void PlayStrainSparks(Vector3 position)
        {
            if (strainSparksPS != null)
            {
                strainSparksPS.transform.position = position;
                strainSparksPS.Emit(8);
            }
        }

        private void BuildConfettiParticleSystem()
        {
            GameObject go = new GameObject("ConfettiFX");
            go.transform.parent = transform;
            confettiPS = go.AddComponent<ParticleSystem>();

            var main = confettiPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1.0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.gravityModifier = 0.65f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            // Confetti multi-color palette
            var col = confettiPS.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.2f, 0.4f), 0f), new GradientColorKey(new Color(0.2f, 0.85f, 1f), 0.5f), new GradientColorKey(new Color(1f, 0.9f, 0.2f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var shape = confettiPS.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.35f;

            var emission = confettiPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(Color.white);
        }

        private void BuildCoinShowerParticleSystem()
        {
            GameObject go = new GameObject("CoinShowerFX");
            go.transform.parent = transform;
            coinShowerPS = go.AddComponent<ParticleSystem>();

            var main = coinShowerPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.0f, 6.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            main.gravityModifier = 1.2f;
            main.startColor = new Color(1f, 0.84f, 0.15f); // Golden arcade coin

            var shape = coinShowerPS.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;

            var emission = coinShowerPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(new Color(1f, 0.88f, 0.2f));
        }

        private void BuildDustPuffParticleSystem()
        {
            GameObject go = new GameObject("DustPuffFX");
            go.transform.parent = transform;
            dustPuffPS = go.AddComponent<ParticleSystem>();

            var main = dustPuffPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.gravityModifier = -0.05f; // gently float up
            main.startColor = new Color(0.9f, 0.9f, 0.95f, 0.35f);

            var shape = dustPuffPS.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            var emission = dustPuffPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(new Color(0.9f, 0.9f, 0.95f, 0.35f));
        }

        private void BuildStrainSparksParticleSystem()
        {
            GameObject go = new GameObject("StrainSparksFX");
            go.transform.parent = transform;
            strainSparksPS = go.AddComponent<ParticleSystem>();

            var main = strainSparksPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.3f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
            main.gravityModifier = 0.8f;
            main.startColor = new Color(1.0f, 0.85f, 0.3f);

            var shape = strainSparksPS.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var emission = strainSparksPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(new Color(1f, 0.9f, 0.4f));
        }

        private Material CreateParticleMaterial(Color tint)
        {
            Shader s = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            Material m = new Material(s);
            m.color = tint;
            return m;
        }
    }
}
