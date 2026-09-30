using System;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Core.Services;

namespace ClawMachine.Gameplay
{
    public class ClawController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ClawConfiguration config;

        [Header("Hierarchy References")]
        [SerializeField] private Transform trolley; // Moves in X/Z
        [SerializeField] private Transform hoist;   // Moves in Y
        [SerializeField] private ClawArm[] arms;
        [SerializeField] private Transform chuteDropPoint;
        [SerializeField] private Transform clawMarker; // Shadow/reticle projected on floor
        [SerializeField] private Transform gripSocket;
        [SerializeField] private GripCaptureVolume captureVolume;

        [Header("Grip System")]
        [SerializeField] private ClawGripAnchor gripAnchor;
        [SerializeField] private ClawPendulumSway pendulumSway;

        [Header("Ground & Depth Sensor")]
        [SerializeField] private float floorSurfaceY = 0.1f;
        [SerializeField] private float tipFloorClearance = 0.03f; // 3cm above floor

        [Header("Environment")]
        [SerializeField] private Transform cabinetRoot;

        public ClawState CurrentState { get; private set; } = ClawState.Aiming;
        public bool IsIdle => CurrentState == ClawState.Aiming;
        public ClawConfiguration Config => config;
        public ClawGripAnchor GripAnchor => gripAnchor;

        public event Action<ClawState> OnStateChanged;

        private Vector3 targetTrolleyPos;
        private float targetArmAngle;
        private float resolveTimer;
        private float closingTimer;
        private float releasingTimer;
        private float targetDropY;
        private Prize targetedPrize;
        private float targetedAccuracy;

        public void Setup(ClawConfiguration cfg, Transform trolleyTr, Transform hoistTr, ClawArm[] clawArms, Transform chutePoint, Transform marker, Transform socket, GripCaptureVolume volume, ClawGripAnchor anchor, ClawPendulumSway sway = null)
        {
            config = cfg;
            trolley = trolleyTr;
            hoist = hoistTr;
            arms = clawArms;
            chuteDropPoint = chutePoint;
            clawMarker = marker;
            gripSocket = socket;
            captureVolume = volume;
            gripAnchor = anchor;
            pendulumSway = sway;
        }

        private void Awake()
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ClawConfiguration>();
            }

            if (trolley == null) trolley = transform;
            if (hoist == null && trolley.childCount > 0) hoist = trolley.GetChild(0);
            if (pendulumSway == null) pendulumSway = GetComponentInChildren<ClawPendulumSway>();

            // Set reasonable PhysX defaults
            Physics.defaultMaxDepenetrationVelocity = 3.0f;

            targetTrolleyPos = trolley != null ? trolley.position : transform.position;
            targetArmAngle = config.openAngle;

            if (gripSocket == null && hoist != null)
            {
                gripSocket = hoist.Find("GripSocket");
            }
            if (captureVolume == null && hoist != null)
            {
                captureVolume = hoist.GetComponentInChildren<GripCaptureVolume>();
            }
            if (gripAnchor == null && hoist != null)
            {
                gripAnchor = hoist.GetComponentInChildren<ClawGripAnchor>() ?? hoist.gameObject.AddComponent<ClawGripAnchor>();
            }

            if (gripAnchor != null)
            {
                gripAnchor.Initialize(gripSocket != null ? gripSocket : hoist);
                gripAnchor.OnPrizeSlipped += HandlePrizeSlipped;
            }
        }

        private void OnDestroy()
        {
            if (gripAnchor != null)
            {
                gripAnchor.OnPrizeSlipped -= HandlePrizeSlipped;
            }
        }

        private void HandlePrizeSlipped()
        {
            if (CurrentState == ClawState.Carrying)
            {
                SetState(ClawState.Aiming);
            }
        }

        private void Start()
        {
            IgnoreEnvironmentCollisions();
            SetState(ClawState.Aiming);
            ApplyArmAngles(true);
        }

        public void SetInputDelta(Vector2 inputDelta)
        {
            // Horizontal player control enabled during Aiming AND Carrying
            if (CurrentState != ClawState.Aiming && CurrentState != ClawState.Carrying) return;
            if (config == null || trolley == null) return;

            float speedMult = GetTrolleySpeedMultiplier();

            // X = Left/Right, Y = Forward/Back (mapped to 3D Z)
            targetTrolleyPos.x += inputDelta.x * config.moveSpeed * speedMult * Time.deltaTime;
            targetTrolleyPos.z += inputDelta.y * config.moveSpeed * speedMult * Time.deltaTime;

            targetTrolleyPos.x = Mathf.Clamp(targetTrolleyPos.x, config.xBounds.x, config.xBounds.y);
            targetTrolleyPos.z = Mathf.Clamp(targetTrolleyPos.z, config.zBounds.x, config.zBounds.y);
        }

        public void TriggerDrop()
        {
            if (CurrentState == ClawState.Aiming)
            {
                SetState(ClawState.Descending);
            }
            else if (CurrentState == ClawState.Carrying)
            {
                // Player manually chooses when to drop over the chute or anywhere else
                SetState(ClawState.Releasing);
            }
        }

        private void Update()
        {
            UpdateReticle();

            if (gripAnchor != null)
            {
                gripAnchor.UpdateMotion(Time.deltaTime);
            }

            switch (CurrentState)
            {
                case ClawState.Aiming:
                    SmoothMoveTrolley();
                    AnimateArms();
                    break;

                case ClawState.Descending:
                    DescendHoist();
                    break;

                case ClawState.Closing:
                    AnimateArms();
                    closingTimer += Time.deltaTime;
                    if (closingTimer > config.closingDuration)
                    {
                        SetState(ClawState.EvaluatingGrip);
                    }
                    break;

                // Canonical grip flow:
                // 1. Descending state finds target via OverlapCapsule + distance check
                // 2. EvaluatingGrip constructs GripEvaluation from targeting accuracy
                // 3. ClawGripAnchor.TryAcquire() kinematically holds the prize
                // NOTE: GripAssist (spring-damper) and GripEvaluator (static) exist but are
                // not wired into this flow. They're reserved for future physics-based grip.
                case ClawState.EvaluatingGrip:
                    float gripMult = GetGripPowerMultiplier();
                    if (gripAnchor != null && targetedPrize != null)
                    {
                        GripEvaluation eval = new GripEvaluation
                        {
                            hasPrize = true,
                            candidate = targetedPrize,
                            quality = Mathf.Clamp01(targetedAccuracy * gripMult),
                            isStable = true,
                            slipDelay = 10f * gripMult
                        };
                        if (gripAnchor.TryAcquire(eval))
                        {
                            PlayAudioGrab();
                        }
                    }
                    SetState(ClawState.Lifting);
                    break;

                case ClawState.Lifting:
                    AscendHoist();
                    break;

                case ClawState.Carrying:
                    SmoothMoveTrolley();
                    AnimateArms();
                    break;

                case ClawState.Returning:
                    TravelToChute();
                    break;

                case ClawState.Releasing:
                    AnimateArms();
                    releasingTimer += Time.deltaTime;
                    if (releasingTimer > config.releasingDuration || AllArmsReachedTarget(4f))
                    {
                        SetState(ClawState.Resolving);
                    }
                    break;

                case ClawState.Resolving:
                    resolveTimer -= Time.deltaTime;
                    if (resolveTimer <= 0f)
                    {
                        SetState(ClawState.Aiming);
                    }
                    break;
            }

            // Cable tension wobble when carrying or lifting prize
            if (hoist != null)
            {
                if ((CurrentState == ClawState.Lifting || CurrentState == ClawState.Carrying) && gripAnchor != null && gripAnchor.HasPrize)
                {
                    float weight = (targetedPrize != null && targetedPrize.Definition != null) ? targetedPrize.Definition.mass : 0.5f;
                    float swayX = Mathf.Sin(Time.time * 8.5f) * 2.8f * weight;
                    float swayZ = Mathf.Cos(Time.time * 7.2f) * 2.4f * weight;
                    hoist.localRotation = Quaternion.Euler(swayX, 0f, swayZ);
                }
                else
                {
                    hoist.localRotation = Quaternion.Lerp(hoist.localRotation, Quaternion.identity, Time.deltaTime * 8f);
                }
            }
        }

        private void SetState(ClawState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case ClawState.Aiming:
                    targetArmAngle = config != null ? config.openAngle : 38f;
                    if (trolley != null) targetTrolleyPos = trolley.position;
                    if (gripAnchor != null) gripAnchor.Release();
                    AnimateArms();
                    break;

                case ClawState.Descending:
                    Vector3 dropOrigin = (gripSocket != null) ? gripSocket.position : trolley.position;
                    Vector3 rayOrigin = new Vector3(dropOrigin.x, 3.5f, dropOrigin.z);
                    targetedPrize = null;
                    targetedAccuracy = 0f;

                    // Scan zone under reticle
                    Collider[] hits = Physics.OverlapCapsule(
                        new Vector3(rayOrigin.x, 0.15f, rayOrigin.z),
                        new Vector3(rayOrigin.x, 1.60f, rayOrigin.z),
                        config != null ? config.descentScanRadius : 0.60f
                    );
                    Prize closest = null;
                    float minHorizDist = float.MaxValue;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        Prize p = hits[i].GetComponentInParent<Prize>();
                        if (p != null)
                        {
                            float hDist = Vector2.Distance(new Vector2(p.transform.position.x, p.transform.position.z), new Vector2(rayOrigin.x, rayOrigin.z));
                            if (hDist < minHorizDist)
                            {
                                minHorizDist = hDist;
                                closest = p;
                            }
                        }
                    }

                    float tolerance = config != null ? config.grabToleranceRadius : 0.22f;
                    PrizeRarity rarity = closest != null ? closest.Rarity : PrizeRarity.Normal;
                    float socketOffset = (gripSocket != null) ? Mathf.Abs(gripSocket.localPosition.y) : 0.65f;
                    float clampMin = config != null ? config.descentMinY : 0.88f;
                    float clampMax = config != null ? config.descentMaxY : 2.2f;

                    if (closest != null && minHorizDist <= tolerance)
                    {
                        targetedPrize = closest;
                        targetedAccuracy = Mathf.Clamp01(1f - (minHorizDist / tolerance));
                        float prizeY = closest.transform.position.y;
                        targetDropY = Mathf.Clamp(prizeY + socketOffset, clampMin, clampMax);
                        Debug.Log($"[ClawController] Target IN RANGE: {targetedPrize.name} ({rarity}) | Dist: {minHorizDist:F2}m / {tolerance:F2}m | Acc: {targetedAccuracy:P0} | DropToY: {targetDropY:F2}");
                    }
                    else
                    {
                        targetedPrize = null;
                        targetedAccuracy = 0f;
                        float prizeY = closest != null ? closest.transform.position.y : 0.25f;
                        targetDropY = Mathf.Clamp(prizeY + socketOffset, clampMin, clampMax);
                        if (closest != null)
                        {
                            Debug.Log($"[ClawController] Target MISSED: {closest.name} ({rarity}) | Dist: {minHorizDist:F2}m > {tolerance:F2}m");
                        }
                        else
                        {
                            Debug.Log("[ClawController] No prize under reticle. Miss!");
                        }
                    }
                    break;

                case ClawState.Closing:
                    closingTimer = 0f;
                    targetArmAngle = config != null ? config.closedAngle : -52f;
                    PlayAudioClamp();
                    break;

                case ClawState.EvaluatingGrip:
                    break;

                case ClawState.Lifting:
                    break;

                case ClawState.Carrying:
                    break;

                case ClawState.Returning:
                    break;

                case ClawState.Releasing:
                    releasingTimer = 0f;
                    if (gripAnchor != null) gripAnchor.Release();
                    targetArmAngle = config != null ? config.openAngle : 38f;
                    PlayAudioDrop();
                    break;

                case ClawState.Resolving:
                    resolveTimer = config != null ? config.resolveDuration : 1.0f;
                    break;
            }

            if (newState != ClawState.Aiming && newState != ClawState.Carrying)
            {
                SetAudioMotor(false);
            }

            OnStateChanged?.Invoke(newState);
        }

        private void SmoothMoveTrolley()
        {
            if (trolley == null || config == null) return;
            Vector3 prev = trolley.position;
            trolley.position = Vector3.Lerp(trolley.position, targetTrolleyPos, Time.deltaTime * config.moveDamping);
            bool moving = (trolley.position - prev).sqrMagnitude > 0.00002f;
            SetAudioMotor(moving);
        }

        private void DescendHoist()
        {
            if (hoist == null) return;

            float dropMult = GetDropSpeedMultiplier();
            float speed = (config != null ? config.dropSpeed : 1.8f) * dropMult;
            Vector3 pos = hoist.position;
            pos.y -= speed * Time.deltaTime;

            if (pos.y <= targetDropY)
            {
                pos.y = targetDropY;
                hoist.position = pos;
                PlayAudioDrop();
                ClawJuiceEffects.Instance?.PlayDustPuff(hoist.position);
                SetState(ClawState.Closing);
            }
            else
            {
                hoist.position = pos;
            }
        }

        private float GetLowestTipY()
        {
            if (arms == null || arms.Length == 0 || hoist == null) return 0f;
            float lowest = float.MaxValue;
            for (int i = 0; i < arms.Length; i++)
            {
                if (arms[i] != null && arms[i].Tip != null)
                {
                    lowest = Mathf.Min(lowest, arms[i].Tip.position.y);
                }
            }
            return (lowest == float.MaxValue) ? hoist.position.y - 1.25f : lowest;
        }

        private void AscendHoist()
        {
            if (hoist == null || config == null) return;

            float liftMult = GetDropSpeedMultiplier();
            Vector3 pos = hoist.position;
            pos.y += (config.liftSpeed * liftMult) * Time.deltaTime;

            if (pos.y >= config.homeY)
            {
                pos.y = config.homeY;
                hoist.position = pos;
                if (gripAnchor != null && gripAnchor.HasPrize)
                {
                    SetState(ClawState.Carrying);
                }
                else
                {
                    SetState(ClawState.Aiming);
                }
            }
            else
            {
                hoist.position = pos;
            }
        }

        private void TravelToChute()
        {
            if (trolley == null || config == null || chuteDropPoint == null)
            {
                SetState(ClawState.Releasing);
                return;
            }

            Vector3 target = new Vector3(chuteDropPoint.position.x, trolley.position.y, chuteDropPoint.position.z);
            trolley.position = Vector3.MoveTowards(trolley.position, target, config.returnSpeed * Time.deltaTime);

            if (Vector3.Distance(new Vector3(trolley.position.x, 0, trolley.position.z), new Vector3(target.x, 0, target.z)) < 0.05f)
            {
                SetState(ClawState.Releasing);
            }
        }

        private void AnimateArms()
        {
            if (arms == null || arms.Length == 0) return;

            for (int i = 0; i < arms.Length; i++)
            {
                if (arms[i] != null)
                {
                    arms[i].SetTargetAngle(targetArmAngle);
                }
            }
        }

        private bool AllArmsReachedTarget(float threshold)
        {
            if (arms == null || arms.Length == 0) return true;
            for (int i = 0; i < arms.Length; i++)
            {
                if (arms[i] != null && Mathf.Abs(arms[i].CurrentAngle - targetArmAngle) > threshold)
                {
                    return false;
                }
            }
            return true;
        }

        private void ApplyArmAngles(bool immediate)
        {
            if (arms == null) return;
            for (int i = 0; i < arms.Length; i++)
            {
                if (arms[i] != null)
                {
                    arms[i].SetAngleImmediate(targetArmAngle);
                }
            }
        }

        private void UpdateReticle()
        {
            if (clawMarker == null || trolley == null) return;

            Vector3 markerPos = clawMarker.position;
            if (gripSocket != null)
            {
                markerPos.x = gripSocket.position.x;
                markerPos.z = gripSocket.position.z;
            }
            else
            {
                markerPos.x = trolley.position.x;
                markerPos.z = trolley.position.z;
            }
            clawMarker.position = markerPos;
            clawMarker.gameObject.SetActive(CurrentState == ClawState.Aiming || CurrentState == ClawState.Carrying);
        }

        private void IgnoreEnvironmentCollisions()
        {
            Collider[] clawCols = GetComponentsInChildren<Collider>(true);

            // Ignore all non-prize colliders in Cabinet (walls, glass, floor, chute dividers)
            if (cabinetRoot == null)
            {
                GameObject cabinet = GameObject.Find("Cabinet");
                if (cabinet != null) cabinetRoot = cabinet.transform;
            }

            if (cabinetRoot != null)
            {
                Collider[] envCols = cabinetRoot.GetComponentsInChildren<Collider>(true);
                for (int c = 0; c < clawCols.Length; c++)
                {
                    for (int e = 0; e < envCols.Length; e++)
                    {
                        if (clawCols[c] != null && envCols[e] != null && !envCols[e].isTrigger)
                        {
                            if (envCols[e].GetComponentInParent<Prize>() == null)
                            {
                                Physics.IgnoreCollision(clawCols[c], envCols[e], true);
                            }
                        }
                    }
                }
            }

            // Also ignore any barriers under MachineController
            MachineController mc = FindFirstObjectByType<MachineController>();
            if (mc != null)
            {
                Collider[] mcCols = mc.GetComponentsInChildren<Collider>(true);
                for (int c = 0; c < clawCols.Length; c++)
                {
                    for (int e = 0; e < mcCols.Length; e++)
                    {
                        if (clawCols[c] != null && mcCols[e] != null && !mcCols[e].isTrigger)
                        {
                            Physics.IgnoreCollision(clawCols[c], mcCols[e], true);
                        }
                    }
                }
            }
        }

        private float GetTrolleySpeedMultiplier()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ)) return econ.GetTrolleySpeedMultiplier();
            return CollectionManager.Instance != null ? CollectionManager.Instance.GetTrolleySpeedMultiplier() : 1f;
        }

        private float GetGripPowerMultiplier()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ)) return econ.GetGripPowerMultiplier();
            return CollectionManager.Instance != null ? CollectionManager.Instance.GetGripPowerMultiplier() : 1f;
        }

        private float GetDropSpeedMultiplier()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ)) return econ.GetDropSpeedMultiplier();
            return CollectionManager.Instance != null ? CollectionManager.Instance.GetDropSpeedMultiplier() : 1f;
        }

        private void PlayAudioGrab()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayGrabSuccess();
            else ClawAudio.Instance?.PlayGrab();
        }

        private void PlayAudioClamp()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayClamp();
            else ClawAudio.Instance?.PlayClamp();
        }

        private void PlayAudioDrop()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayDropFloor();
            else ClawAudio.Instance?.PlayDrop();
        }

        private void SetAudioMotor(bool moving)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.SetMotorMoving(moving);
            else ClawAudio.Instance?.SetMotorMoving(moving);
        }
    }
}
