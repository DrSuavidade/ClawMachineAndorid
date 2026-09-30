using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClawMachine.Gameplay
{
    public enum GripStabilityState
    {
        Inactive,
        Stable,
        Slipping,
        Broken
    }

    [Serializable]
    public struct GripEvaluationResult
    {
        public Prize candidatePrize;
        public int fingerContacts;
        public float contactScore;
        public float centeringScore;
        public float enclosureScore;
        public float balanceScore;
        public float gripQuality;
        public bool isValid;
        public Vector3 averageContactPoint;
    }

    /// <summary>
    /// Spring-damper grip assist system. NOT currently wired into main claw flow.
    /// ClawGripAnchor (kinematic hold) is the active grip system.
    /// Reserved for future physics-based grip.
    /// </summary>
    public class GripAssist : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform gripSocket;
        [SerializeField] private GripCaptureVolume captureVolume;
        [SerializeField] private ContactSensor[] contactSensors;

        [Header("Tuning - Evaluation")]
        [SerializeField] private float usefulGripRadius = 0.45f;
        [SerializeField] private float maxBalanceDistance = 0.50f;
        [SerializeField] private float contactWeight = 0.35f;
        [SerializeField] private float centeringWeight = 0.20f;
        [SerializeField] private float enclosureWeight = 0.25f;
        [SerializeField] private float balanceWeight = 0.20f;

        [Header("Tuning - Spring Assist")]
        [SerializeField] private float gripSpring = 120f;
        [SerializeField] private float gripDamping = 15f;
        [SerializeField] private float baseMaxGripForceMultiplier = 2.4f;

        [Header("Tuning - Stability & Slipping")]
        [SerializeField] private float slipStartDistance = 0.18f;
        [SerializeField] private float gripBreakDistance = 0.40f;

        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugGui = true;
        [SerializeField] private bool drawGizmos = true;

        public GripStabilityState StabilityState { get; private set; } = GripStabilityState.Inactive;
        public GripEvaluationResult ActiveEvaluation { get; private set; }
        public Prize AssistedPrize { get; private set; }

        private Vector3 localGripPoint;
        private Vector3 currentGripWorld;
        private Vector3 lastAppliedForce;
        private float currentMaxGripForce;
        private float currentGripDistance;

        public void Initialize(Transform socket, GripCaptureVolume volume, ContactSensor[] sensors)
        {
            gripSocket = socket;
            captureVolume = volume;
            contactSensors = sensors;
        }

        public GripEvaluationResult EvaluateAndAcquireGrip()
        {
            ReleaseGrip();

            if (captureVolume == null)
            {
                Debug.LogWarning("[GripAssist] No GripCaptureVolume attached.");
                return default;
            }

            captureVolume.CleanUp();
            GripEvaluationResult bestResult = default;
            float highestQuality = -1f;

            foreach (var candidate in captureVolume.Candidates)
            {
                if (candidate == null || candidate.Rigidbody == null) continue;

                GripEvaluationResult result = EvaluatePrize(candidate);
                if (result.isValid && result.gripQuality > highestQuality)
                {
                    highestQuality = result.gripQuality;
                    bestResult = result;
                }
            }

            if (bestResult.isValid)
            {
                AssistedPrize = bestResult.candidatePrize;
                ActiveEvaluation = bestResult;

                // Store initial local grip point
                Vector3 worldGrip = (bestResult.averageContactPoint + AssistedPrize.Rigidbody.worldCenterOfMass) * 0.5f;
                localGripPoint = AssistedPrize.transform.InverseTransformPoint(worldGrip);
                currentGripWorld = worldGrip;
                StabilityState = GripStabilityState.Stable;

                Debug.Log($"[GripAssist] Acquired Grip on {AssistedPrize.name} | Quality: {bestResult.gripQuality:F2} | Contacts: {bestResult.fingerContacts}");
            }
            else
            {
                StabilityState = GripStabilityState.Inactive;
            }

            return bestResult;
        }

        private GripEvaluationResult EvaluatePrize(Prize prize)
        {
            GripEvaluationResult res = new GripEvaluationResult { candidatePrize = prize };
            Rigidbody rb = prize.Rigidbody;
            Vector3 com = rb.worldCenterOfMass;

            // 1. Contact Score
            int contacts = 0;
            Vector3 sumContactPts = Vector3.zero;

            if (contactSensors != null)
            {
                for (int i = 0; i < contactSensors.Length; i++)
                {
                    if (contactSensors[i] != null && contactSensors[i].IsTouching(prize))
                    {
                        contacts++;
                        sumContactPts += contactSensors[i].GetContactPoint(prize);
                    }
                }
            }

            res.fingerContacts = contacts;
            switch (contacts)
            {
                case 0: res.contactScore = 0f; break;
                case 1: res.contactScore = 0.15f; break;
                case 2: res.contactScore = 0.65f; break;
                default: res.contactScore = 1.0f; break;
            }

            // Hard rule: Fewer than 2 distinct contacts must NEVER receive GripAssist
            if (contacts < 2)
            {
                res.isValid = false;
                res.gripQuality = 0f;
                return res;
            }

            res.averageContactPoint = sumContactPts / contacts;

            // 2. Centering Score
            Vector3 socketPos = gripSocket != null ? gripSocket.position : transform.position;
            float horizDistToAxis = Vector2.Distance(new Vector2(com.x, com.z), new Vector2(socketPos.x, socketPos.z));
            res.centeringScore = Mathf.Clamp01(1f - (horizDistToAxis / usefulGripRadius));

            // 3. Enclosure Score (Point in 2D Finger Triangle)
            res.enclosureScore = CalculateEnclosureScore(com);

            // 4. Balance Score
            float horizDistToAvgContact = Vector2.Distance(new Vector2(com.x, com.z), new Vector2(res.averageContactPoint.x, res.averageContactPoint.z));
            res.balanceScore = Mathf.Clamp01(1f - (horizDistToAvgContact / maxBalanceDistance));

            // Final GripQuality
            res.gripQuality = (res.contactScore * contactWeight) +
                              (res.centeringScore * centeringWeight) +
                              (res.enclosureScore * enclosureWeight) +
                              (res.balanceScore * balanceWeight);

            res.isValid = true;
            return res;
        }

        private float CalculateEnclosureScore(Vector3 com)
        {
            if (contactSensors == null || contactSensors.Length < 3) return 0.5f;

            Vector2 p = new Vector2(com.x, com.z);
            Vector2 a = new Vector2(contactSensors[0].transform.position.x, contactSensors[0].transform.position.z);
            Vector2 b = new Vector2(contactSensors[1].transform.position.x, contactSensors[1].transform.position.z);
            Vector2 c = new Vector2(contactSensors[2].transform.position.x, contactSensors[2].transform.position.z);

            bool inside = IsPointInTriangle(p, a, b, c);
            if (!inside)
            {
                float distToCentroid = Vector2.Distance(p, (a + b + c) / 3f);
                return Mathf.Clamp01(0.2f - distToCentroid * 0.5f);
            }

            // Inside triangle: distance from centroid
            Vector2 centroid = (a + b + c) / 3f;
            float d = Vector2.Distance(p, centroid);
            return Mathf.Clamp01(1f - (d / usefulGripRadius) * 0.8f);
        }

        private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public void ReleaseGrip()
        {
            AssistedPrize = null;
            ActiveEvaluation = default;
            StabilityState = GripStabilityState.Inactive;
            lastAppliedForce = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (AssistedPrize == null || AssistedPrize.Rigidbody == null || gripSocket == null)
            {
                return;
            }

            Rigidbody rb = AssistedPrize.Rigidbody;

            // Compute current world-space grip point
            currentGripWorld = AssistedPrize.transform.TransformPoint(localGripPoint);
            Vector3 positionError = gripSocket.position - currentGripWorld;
            currentGripDistance = positionError.magnitude;

            // Check break distance
            if (currentGripDistance > gripBreakDistance)
            {
                Debug.Log($"[GripAssist] Grip BROKE on {AssistedPrize.name}! Distance: {currentGripDistance:F2}m");
                StabilityState = GripStabilityState.Broken;
                ReleaseGrip();
                return;
            }

            // Check slip threshold
            if (currentGripDistance > slipStartDistance)
            {
                StabilityState = GripStabilityState.Slipping;
            }
            else
            {
                StabilityState = GripStabilityState.Stable;
            }

            // Calculate spring-damper assist force
            Vector3 gripPointVel = rb.GetPointVelocity(currentGripWorld);
            Vector3 assistForce = (positionError * gripSpring) - (gripPointVel * gripDamping);

            // MaxGripForce scales with prize mass and GripQuality
            float objectWeight = rb.mass * Physics.gravity.magnitude;
            currentMaxGripForce = objectWeight * baseMaxGripForceMultiplier * ActiveEvaluation.gripQuality;

            // Clamp force
            assistForce = Vector3.ClampMagnitude(assistForce, currentMaxGripForce);
            lastAppliedForce = assistForce;

            // Apply force at the actual grip point (rotation emerges naturally)
            rb.AddForceAtPosition(assistForce, currentGripWorld, ForceMode.Force);
        }

        private void OnGUI()
        {
            if (!showDebugGui || AssistedPrize == null) return;

            GUI.Box(new Rect(10, 10, 320, 220), "<b>GRIP ASSIST DEBUG</b>");
            int y = 35;
            GUI.Label(new Rect(20, y, 300, 22), $"Candidate: {AssistedPrize.name}"); y += 20;
            GUI.Label(new Rect(20, y, 300, 22), $"State: <b>{StabilityState.ToString().ToUpper()}</b>"); y += 20;
            GUI.Label(new Rect(20, y, 300, 22), $"Contacts: {ActiveEvaluation.fingerContacts} | Score: {ActiveEvaluation.contactScore:F2}"); y += 18;
            GUI.Label(new Rect(20, y, 300, 22), $"Centering: {ActiveEvaluation.centeringScore:F2}"); y += 18;
            GUI.Label(new Rect(20, y, 300, 22), $"Enclosure: {ActiveEvaluation.enclosureScore:F2}"); y += 18;
            GUI.Label(new Rect(20, y, 300, 22), $"Balance: {ActiveEvaluation.balanceScore:F2}"); y += 18;
            GUI.Label(new Rect(20, y, 300, 22), $"GripQuality: <b>{ActiveEvaluation.gripQuality:F2}</b>"); y += 20;
            GUI.Label(new Rect(20, y, 300, 22), $"Distance: {currentGripDistance:F2}m / Slip: {slipStartDistance:F2}m"); y += 18;
            GUI.Label(new Rect(20, y, 300, 22), $"Force: {lastAppliedForce.magnitude:F1}N / Max: {currentMaxGripForce:F1}N");
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            // Draw GripSocket
            if (gripSocket != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(gripSocket.position, 0.06f);

                // Draw Break Distance sphere
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
                Gizmos.DrawWireSphere(gripSocket.position, gripBreakDistance);

                // Draw Slip Distance sphere
                Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.25f);
                Gizmos.DrawWireSphere(gripSocket.position, slipStartDistance);
            }

            if (AssistedPrize != null && AssistedPrize.Rigidbody != null)
            {
                // Prize CoM
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(AssistedPrize.Rigidbody.worldCenterOfMass, 0.04f);

                // Calculated Grip Point
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(currentGripWorld, 0.04f);

                // Line from grip point to socket
                if (gripSocket != null)
                {
                    Gizmos.color = (StabilityState == GripStabilityState.Slipping) ? Color.yellow : Color.green;
                    Gizmos.DrawLine(currentGripWorld, gripSocket.position);
                }
            }
        }
    }
}
