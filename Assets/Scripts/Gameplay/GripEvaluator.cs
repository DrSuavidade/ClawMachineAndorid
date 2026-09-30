using UnityEngine;

namespace ClawMachine.Gameplay
{
    public struct GripEvaluation
    {
        public bool hasPrize;
        public Prize candidate;
        public float quality;      // 0.0 to 1.0 (aim accuracy)
        public bool isStable;      // quality >= stableThreshold -> makes it to chute
        public float slipDelay;    // seconds after lift starts before slipping (if unstable)
    }

    /// <summary>
    /// Static grip evaluator. NOT currently used in the main claw flow.
    /// ClawController uses its own OverlapCapsule-based targeting instead.
    /// Reserved for future physics-based grip evaluation.
    /// </summary>
    public static class GripEvaluator
    {
        public const float UsefulGripRadius = 0.65f;
        public const float StableThreshold = 0.40f;

        public static GripEvaluation Evaluate(Vector3 grabPoint, float searchRadius = UsefulGripRadius)
        {
            GripEvaluation result = default;

            // Sphere overlap centered just below the hub where the fingers wrap
            Collider[] hits = Physics.OverlapSphere(grabPoint, searchRadius);
            if (hits == null || hits.Length == 0) return result;

            Prize bestPrize = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<Prize>(out var prize))
                {
                    if (prize.Rigidbody == null) continue;

                    Vector3 com = prize.Rigidbody.worldCenterOfMass;
                    // Horizontal distance from central grab axis
                    float horizDist = Vector2.Distance(new Vector2(com.x, com.z), new Vector2(grabPoint.x, grabPoint.z));
                    float vertDist = Mathf.Abs(com.y - grabPoint.y);

                    float totalScoreDist = horizDist + (vertDist * 0.5f);

                    if (totalScoreDist < bestDistance)
                    {
                        bestDistance = totalScoreDist;
                        bestPrize = prize;
                    }
                }
            }

            if (bestPrize != null)
            {
                Vector3 prizeCom = bestPrize.Rigidbody.worldCenterOfMass;
                float horizOffset = Vector2.Distance(new Vector2(prizeCom.x, prizeCom.z), new Vector2(grabPoint.x, grabPoint.z));
                float accuracy = Mathf.Clamp01(1f - (horizOffset / searchRadius));

                result.hasPrize = true;
                result.candidate = bestPrize;
                result.quality = accuracy;
                result.isStable = accuracy >= StableThreshold;

                if (!result.isStable)
                {
                    // Poor aim: slips out mid-flight (between 0.6s and 1.8s)
                    result.slipDelay = Mathf.Lerp(0.6f, 1.8f, accuracy / StableThreshold);
                }
            }

            return result;
        }
    }
}
