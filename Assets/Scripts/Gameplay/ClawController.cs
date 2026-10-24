using System;
using System.Collections.Generic;
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
        public bool HasPrize => gripAnchor != null && gripAnchor.HasPrize;
        public bool CanSwitchMachine => IsIdle && !HasPrize;
        public ClawConfiguration Config => config;
        public ClawGripAnchor GripAnchor => gripAnchor;
        public Transform GripSocket => gripSocket;
        public float CarryRemainingSeconds => carryTimer;

        public event Action<ClawState> OnStateChanged;
        public event Action<float> OnCarryTimerUpdated;

        private const float MaxCarryDuration = 10.0f;
        private float carryTimer;
        private int lastCarryTickSecond = -1;

        private Vector3 targetTrolleyPos;
        private float targetArmAngle;
        private float targetClosedAngle;
        private Vector3 targetHoldOffset = Vector3.zero;
        private float resolveTimer;
        private float closingTimer;
        private float releasingTimer;
        private float targetDropY;
        private Prize targetedPrize;
        private float targetedAccuracy;
        private Material magnetRingMat;
        private Material markerMat;
        private Material chuteBeaconMat;
        private Renderer markerRend;
        private Transform magnetRing;
        private Transform chuteBeacon;
        private Transform gantryCrossRail;
        private Transform clawStatusLed;
        private Material ledMaterial;
        private Material railMat;
        private Renderer[] chuteRimRenderers;
        private Material chuteRimMat;
        private float strainTimer;
        private float chuteFlashTimer;

        public bool IsOverChute { get; private set; }
        public void TriggerChuteCelebrationFlash(float duration = 1.6f) => chuteFlashTimer = duration;

        public void Setup(ClawConfiguration cfg, Transform trolleyTr, Transform hoistTr, ClawArm[] clawArms, Transform chutePoint, Transform marker, Transform socket, ClawGripAnchor anchor, ClawPendulumSway sway = null)
        {
            config = cfg;
            trolley = trolleyTr;
            hoist = hoistTr;
            arms = clawArms;
            chuteDropPoint = chutePoint;
            clawMarker = marker;
            markerRend = null;
            magnetRing = null;
            gripSocket = socket;
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
                gripSocket = hoist.Find("GripSocket") ?? hoist.Find("ClawSuspensionHub/GripSocket");
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
            if (magnetRingMat != null)
            {
                Destroy(magnetRingMat);
            }
            if (markerMat != null)
            {
                Destroy(markerMat);
            }
            if (chuteBeaconMat != null)
            {
                Destroy(chuteBeaconMat);
            }
            if (ledMaterial != null)
            {
                Destroy(ledMaterial);
            }
            if (railMat != null)
            {
                Destroy(railMat);
            }
            if (chuteRimMat != null)
            {
                Destroy(chuteRimMat);
            }
        }

        private void HandlePrizeSlipped()
        {
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayPrizeSlip();
            audio?.TriggerCameraShake(0.18f, 0.04f);
            if (hoist != null)
            {
                ClawJuiceEffects.Instance?.PlayPrizeNudgeFX(hoist.position);
            }
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
            EnsureVisualJuice();
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
            UpdateVisualJuice();

            if (gripAnchor != null)
            {
                gripAnchor.UpdateMotion(Time.deltaTime);
            }

            if (pendulumSway != null && hoist != null)
            {
                pendulumSway.CableLength = Mathf.Max(0.4f, (config.homeY - hoist.position.y) + 0.5f);
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
                    if (targetedPrize != null && gripSocket != null)
                    {
                        Vector3 targetH = new Vector3(gripSocket.position.x, targetedPrize.transform.position.y, gripSocket.position.z);
                        targetedPrize.transform.position = Vector3.MoveTowards(targetedPrize.transform.position, targetH, 0.40f * Time.deltaTime);
                    }
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
                case ClawState.EvaluatingGrip:
                    float gripMult = GetGripPowerMultiplier();
                    if (gripAnchor != null && targetedPrize != null)
                    {
                        float massFactor = (targetedPrize.Definition != null) ? Mathf.Clamp(targetedPrize.Definition.mass, 0.3f, 1.2f) : 0.5f;
                        float effectiveQuality = Mathf.Clamp01((targetedAccuracy * gripMult) / massFactor);

                        // Skill + Tension: Clean hold requires >= 0.50 quality.
                        // Between 0.25 and 0.50: Toy is lifted but unstable, teetering and slipping partway!
                        bool isCleanHold = effectiveQuality >= 0.50f;
                        float slipDelaySeconds = isCleanHold ? 99f : UnityEngine.Random.Range(1.2f, 2.8f);

                        GripEvaluation eval = new GripEvaluation
                        {
                            hasPrize = true,
                            candidate = targetedPrize,
                            quality = effectiveQuality,
                            isStable = isCleanHold,
                            slipDelay = slipDelaySeconds
                        };

                        if (effectiveQuality >= 0.25f)
                        {
                            if (gripAnchor.TryAcquire(eval, targetHoldOffset))
                            {
                                PlayAudioGrab();
                            }
                            else
                            {
                                PlayAudioClamp();
                            }
                        }
                        else
                        {
                            // Near-miss nudge: Claw slips right off the toy, nudging it across the pile
                            PlayAudioClamp();
                            ApplyNearMissNudge(targetedPrize);
                        }
                    }
                    else
                    {
                        PlayAudioClamp();
                        // Broad physical nudge around claw closure zone to tumble nearby toys
                        Collider[] nearby = Physics.OverlapSphere(hoist != null ? hoist.position : transform.position, 0.45f);
                        for (int n = 0; n < nearby.Length; n++)
                        {
                            Prize np = nearby[n].GetComponentInParent<Prize>();
                            if (np != null)
                            {
                                ApplyNearMissNudge(np);
                                break;
                            }
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
                    carryTimer -= Time.deltaTime;
                    OnCarryTimerUpdated?.Invoke(Mathf.Max(0f, carryTimer));
                    if (carryTimer <= 5.0f && lastCarryTickSecond != 1)
                    {
                        lastCarryTickSecond = 1;
                        ServiceLocator.Get<IAudioService>()?.PlayCountdownTick();
                    }
                    if (carryTimer <= 0f)
                    {
                        ServiceLocator.Get<IAudioService>()?.PlayPrizeSlip();
                        SetState(ClawState.Releasing);
                    }
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

            // Reset hoist rotation if pendulum sway is not handling it
            if (hoist != null && pendulumSway == null)
            {
                hoist.localRotation = Quaternion.Lerp(hoist.localRotation, Quaternion.identity, Time.deltaTime * 8f);
            }
        }

        private void SetState(ClawState newState)
        {
            if (CurrentState == ClawState.Carrying && newState != ClawState.Carrying)
            {
                ServiceLocator.Get<IAudioService>()?.StopCountdown();
            }

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

                    float aimMult = GetDropSpeedMultiplier();
                    float baseTolerance = config != null ? config.grabToleranceRadius : 0.22f;
                    float grabRadius = baseTolerance * aimMult;

                    // Magnet is a 2nd grab range slightly bigger than grab area (+1% to +10%)
                    float magnetBonus = Mathf.Max(0.01f, GetGripPowerMultiplier() - 1f);
                    float magnetRadius = grabRadius * (1f + magnetBonus);

                    PrizeRarity rarity = closest != null ? closest.Rarity : PrizeRarity.Normal;
                    // Tips are 0.65m below hoist when open. Hoist clamp ensures tips never penetrate floor.
                    float minTipY = floorSurfaceY + tipFloorClearance;
                    float clampMin = Mathf.Max(config != null ? config.descentMinY : 0.78f, minTipY + 0.65f);
                    float clampMax = config != null ? config.descentMaxY : 2.6f;

                    if (closest != null)
                    {
                        if (minHorizDist <= grabRadius)
                        {
                            targetedPrize = closest;
                            targetedAccuracy = Mathf.Clamp01(1f - (minHorizDist / grabRadius));
                        }
                        else if (minHorizDist <= magnetRadius)
                        {
                            targetedPrize = closest;
                            targetedAccuracy = 0.55f;
                            Debug.Log($"[ClawController] 🧲 MAGNET ATTRACTED: {closest.name} ({rarity})! Dist: {minHorizDist:F3}m <= MagnetRadius: {magnetRadius:F3}m (+{magnetBonus * 100f:F0}%)");
                        }
                        else
                        {
                            targetedPrize = null;
                            targetedAccuracy = 0f;
                        }
                    }

                    if (targetedPrize != null)
                    {
                        Bounds pBounds = GetPrizeBounds(targetedPrize);
                        float pRadius = Mathf.Max(pBounds.extents.x, pBounds.extents.z);
                        float pHalfHeight = pBounds.extents.y;

                        // Content-aware closed angle: stopped firmly around toy perimeter (min 6°, max 32°)
                        targetClosedAngle = Mathf.Clamp(pRadius * 80f, 6f, 32f);

                        // Content-aware cradle seating: prize sits nestled in palm basket
                        targetHoldOffset = new Vector3(0f, -0.38f - pHalfHeight * 0.2f, 0f);

                        // Gentle descent: tips land gently flanking the equator of toy, never slamming floor
                        float prizeY = targetedPrize.transform.position.y;
                        float desiredTipY = Mathf.Max(minTipY, prizeY - pHalfHeight * 0.3f);
                        targetDropY = Mathf.Clamp(desiredTipY + 0.65f, clampMin, clampMax);
                        Debug.Log($"[ClawController] Target IN RANGE: {targetedPrize.name} ({rarity}) | Radius: {pRadius:F2}m | ContentAngle: {targetClosedAngle:F1}° | DropToY: {targetDropY:F2}");
                    }
                    else
                    {
                        targetedPrize = null;
                        targetedAccuracy = 0f;
                        targetClosedAngle = 2f; // Minimum angle is 2°, never crossing
                        targetHoldOffset = Vector3.zero;

                        float prizeY = closest != null ? closest.transform.position.y : 0.20f;
                        float desiredTipY = Mathf.Max(minTipY, prizeY - 0.02f);
                        targetDropY = Mathf.Clamp(desiredTipY + 0.65f, clampMin, clampMax);
                        if (closest != null)
                        {
                            Debug.Log($"[ClawController] Target MISSED: {closest.name} ({rarity}) | Dist: {minHorizDist:F2}m > MagnetRadius: {magnetRadius:F2}m");
                        }
                        else
                        {
                            Debug.Log("[ClawController] No prize under reticle. Miss!");
                        }
                    }
                    break;

                case ClawState.Closing:
                    closingTimer = 0f;
                    targetArmAngle = (targetedPrize != null) ? targetClosedAngle : 2f;
                    break;

                case ClawState.EvaluatingGrip:
                    break;

                case ClawState.Lifting:
                    break;

                case ClawState.Carrying:
                    carryTimer = MaxCarryDuration;
                    lastCarryTickSecond = 0;
                    OnCarryTimerUpdated?.Invoke(carryTimer);
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

            if (newState != ClawState.Aiming && newState != ClawState.Carrying && newState != ClawState.Descending && newState != ClawState.Lifting)
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

            SetAudioMotor(true);
            float speed = config != null ? config.dropSpeed : 1.8f;
            Vector3 pos = hoist.position;
            pos.y -= speed * Time.deltaTime;

            if (pos.y <= targetDropY)
            {
                pos.y = targetDropY;
                hoist.position = pos;
                SetAudioMotor(false);
                ClawJuiceEffects.Instance?.PlayDustPuff(hoist.position);
                SetState(ClawState.Closing);
            }
            else
            {
                hoist.position = pos;
            }
        }

        private void AscendHoist()
        {
            if (hoist == null || config == null) return;

            SetAudioMotor(true);
            Vector3 pos = hoist.position;
            pos.y += config.liftSpeed * Time.deltaTime;

            if (gripAnchor != null && gripAnchor.HasPrize)
            {
                strainTimer += Time.deltaTime;
                if (strainTimer >= 0.28f)
                {
                    strainTimer = 0f;
                    Vector3 sparkPos = hoist.position + Vector3.up * 0.15f + UnityEngine.Random.insideUnitSphere * 0.06f;
                    ClawJuiceEffects.Instance?.PlayStrainSparks(sparkPos);
                }
            }
            else
            {
                strainTimer = 0f;
            }

            if (pos.y >= config.homeY)
            {
                pos.y = config.homeY;
                hoist.position = pos;
                SetAudioMotor(false);
                if (gripAnchor != null && gripAnchor.HasPrize)
                {
                    // Manual carry flow: player steers claw to chute and triggers drop
                    SetState(ClawState.Carrying);
                }
                else
                {
                    ServiceLocator.Get<IAudioService>()?.PlayFail();
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

            AnimateArms();
            SetAudioMotor(true);

            Vector3 target = new Vector3(chuteDropPoint.position.x, trolley.position.y, chuteDropPoint.position.z);
            trolley.position = Vector3.MoveTowards(trolley.position, target, config.returnSpeed * Time.deltaTime);

            if (Vector3.Distance(new Vector3(trolley.position.x, 0, trolley.position.z), new Vector3(target.x, 0, target.z)) < 0.05f)
            {
                SetAudioMotor(false);
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
            markerPos.y = 0.11f;
            clawMarker.position = markerPos;
            clawMarker.gameObject.SetActive(CurrentState == ClawState.Aiming || CurrentState == ClawState.Carrying);

            // Shadow diameter visually matches the exact grab tolerance radius
            float baseTolerance = config != null ? config.grabToleranceRadius : 0.22f;
            float currentTolerance = baseTolerance * GetDropSpeedMultiplier();
            clawMarker.localScale = new Vector3(currentTolerance * 2f, 0.005f, currentTolerance * 2f);

            // Highlight reticle green when carrying prize over the chute zone
            IsOverChute = CurrentState == ClawState.Carrying && markerPos.x < -0.35f && markerPos.z < -0.35f;
            if (markerRend == null) markerRend = clawMarker.GetComponent<Renderer>();
            if (markerRend != null)
            {
                Color targetCol = IsOverChute ? new Color(0.2f, 1.0f, 0.35f, 0.75f) : new Color(1.0f, 0.9f, 0.2f, 0.45f);
                if (markerMat == null)
                {
                    markerMat = new Material(markerRend.sharedMaterial);
                    markerRend.sharedMaterial = markerMat;
                }
                markerMat.color = targetCol;
            }

            // Magnet Ring Aura (2nd grab area slightly bigger than grab area: +1% to +10%)
            if (magnetRing == null) magnetRing = clawMarker.Find("MagnetRing");
            float magnetBonus = GetGripPowerMultiplier() - 1f;
            if (magnetBonus > 0.001f)
            {
                if (magnetRing == null)
                {
                    GameObject ringObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    ringObj.name = "MagnetRing";
                    ringObj.transform.parent = clawMarker;
                    ringObj.transform.localPosition = new Vector3(0f, 0.001f, 0f);
                    ringObj.GetComponent<Collider>().enabled = false;
                    magnetRingMat = markerMat != null
                        ? new Material(markerMat)
                        : new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
                    magnetRingMat.color = new Color(0.18f, 0.85f, 1.0f, 0.35f);
                    ringObj.GetComponent<Renderer>().sharedMaterial = magnetRingMat;
                    magnetRing = ringObj.transform;
                }
                magnetRing.gameObject.SetActive(true);
                float magnetScaleRatio = 1f + magnetBonus;
                magnetRing.localScale = new Vector3(magnetScaleRatio, 1.05f, magnetScaleRatio);
            }
            else if (magnetRing != null)
            {
                magnetRing.gameObject.SetActive(false);
            }

            // Pulsing Chute Target Indicator during Carrying state
            if (CurrentState == ClawState.Carrying && chuteDropPoint != null)
            {
                if (chuteBeacon == null)
                {
                    GameObject beaconObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    beaconObj.name = "ChuteDropBeacon";
                    beaconObj.transform.SetParent( clawMarker.parent, false);
                    beaconObj.GetComponent<Collider>().enabled = false;
                    chuteBeaconMat = markerMat != null
                        ? new Material(markerMat)
                        : new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
                    beaconObj.GetComponent<Renderer>().sharedMaterial = chuteBeaconMat;
                    chuteBeacon = beaconObj.transform;
                }
                chuteBeacon.gameObject.SetActive(true);
                chuteBeacon.position = new Vector3(chuteDropPoint.position.x, 0.115f, chuteDropPoint.position.z);
                float pulse = 1f + Mathf.Sin(Time.time * 7f) * 0.12f;
                chuteBeacon.localScale = new Vector3(0.68f * pulse, 0.006f, 0.68f * pulse);
                if (chuteBeaconMat != null)
                {
                    float alpha = 0.45f + Mathf.Sin(Time.time * 7f) * 0.20f;
                    chuteBeaconMat.color = IsOverChute
                        ? new Color(0.2f, 1.0f, 0.4f, alpha + 0.15f)
                        : new Color(1.0f, 0.78f, 0.15f, alpha);
                }
            }
            else if (chuteBeacon != null && chuteBeacon.gameObject.activeSelf)
            {
                chuteBeacon.gameObject.SetActive(false);
            }
        }

        private void EnsureVisualJuice()
        {
            if (clawStatusLed == null && hoist != null)
            {
                Transform hub = hoist.Find("ClawSuspensionHub/ClawHub") ?? hoist;
                GameObject ledGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ledGo.name = "ClawStatusLED";
                ledGo.transform.SetParent(hub, false);
                ledGo.transform.localPosition = new Vector3(0f, 0.12f, 0f);
                ledGo.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
                if (ledGo.TryGetComponent<Collider>(out var col)) Destroy(col);

                var rend = ledGo.GetComponent<Renderer>();
                Shader sh = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
                ledMaterial = new Material(sh);
                ledMaterial.color = new Color(0.2f, 0.85f, 1.0f);
                rend.sharedMaterial = ledMaterial;
                clawStatusLed = ledGo.transform;
            }

            if (gantryCrossRail == null && trolley != null && trolley.parent != null)
            {
                // Gantry cross carriage rail beam spanning X across the ceiling
                GameObject railGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                railGo.name = "GantryCrossRail";
                railGo.transform.SetParent(trolley.parent, false);
                railGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                railGo.transform.localScale = new Vector3(0.06f, 1.25f, 0.06f); // 2.5m span across X
                railGo.transform.localPosition = new Vector3(0f, trolley.localPosition.y, trolley.localPosition.z);
                if (railGo.TryGetComponent<Collider>(out var rcol)) Destroy(rcol);

                var rRend = railGo.GetComponent<Renderer>();
                Shader rsh = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
                railMat = new Material(rsh);
                railMat.color = new Color(0.45f, 0.48f, 0.52f);
                if (railMat.HasProperty("_Metallic")) railMat.SetFloat("_Metallic", 0.85f);
                if (railMat.HasProperty("_Glossiness")) railMat.SetFloat("_Glossiness", 0.75f);
                if (railMat.HasProperty("_Smoothness")) railMat.SetFloat("_Smoothness", 0.75f);
                rRend.sharedMaterial = railMat;
                gantryCrossRail = railGo.transform;
            }

            if (chuteRimMat == null)
            {
                var rims = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
                List<Renderer> found = new List<Renderer>();
                for (int i = 0; i < rims.Length; i++)
                {
                    if (rims[i].name.Contains("ChuteDivider_") && rims[i].name.Contains("_Rim"))
                    {
                        found.Add(rims[i]);
                    }
                }
                if (found.Count > 0)
                {
                    chuteRimRenderers = found.ToArray();
                    Shader sh = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
                    chuteRimMat = new Material(sh);
                    chuteRimMat.color = new Color(0.18f, 0.85f, 1.0f, 0.95f);
                    for (int i = 0; i < chuteRimRenderers.Length; i++)
                    {
                        chuteRimRenderers[i].sharedMaterial = chuteRimMat;
                    }
                }
            }
        }

        private void UpdateVisualJuice()
        {
            if (gantryCrossRail != null && trolley != null)
            {
                Vector3 rPos = gantryCrossRail.localPosition;
                rPos.z = trolley.localPosition.z;
                gantryCrossRail.localPosition = rPos;
            }

            if (ledMaterial != null)
            {
                Color ledCol;
                if (CurrentState == ClawState.Aiming)
                {
                    float breath = 0.75f + Mathf.Sin(Time.time * 4f) * 0.25f;
                    ledCol = new Color(0.15f * breath, 0.85f * breath, 1.0f * breath);
                }
                else if (CurrentState == ClawState.Descending || CurrentState == ClawState.Lifting)
                {
                    bool on = (Mathf.FloorToInt(Time.time * 8f) % 2) == 0;
                    ledCol = on ? new Color(1.0f, 0.65f, 0.1f) : new Color(0.3f, 0.15f, 0.02f);
                }
                else if (CurrentState == ClawState.Carrying)
                {
                    if (HasPrize)
                    {
                        if (IsOverChute)
                        {
                            bool on = (Mathf.FloorToInt(Time.time * 12f) % 2) == 0;
                            ledCol = on ? new Color(0.25f, 1.0f, 0.45f) : new Color(0.05f, 0.35f, 0.1f);
                        }
                        else
                        {
                            ledCol = new Color(0.2f, 1.0f, 0.35f);
                        }
                    }
                    else
                    {
                        ledCol = new Color(1.0f, 0.2f, 0.2f);
                    }
                }
                else
                {
                    ledCol = new Color(0.4f, 0.5f, 0.6f);
                }
                ledMaterial.color = ledCol;
            }

            if (chuteRimMat != null)
            {
                if (chuteFlashTimer > 0f)
                {
                    chuteFlashTimer -= Time.deltaTime;
                    bool flash = (Mathf.FloorToInt(Time.time * 16f) % 2) == 0;
                    chuteRimMat.color = flash ? new Color(1.0f, 0.90f, 0.20f, 1f) : new Color(0.20f, 1.0f, 0.45f, 1f);
                }
                else if (CurrentState == ClawState.Carrying && HasPrize)
                {
                    float pulse = 0.65f + Mathf.Sin(Time.time * 8f) * 0.35f;
                    Color rimCol = IsOverChute
                        ? new Color(0.2f * pulse, 1.0f * pulse, 0.4f * pulse)
                        : new Color(0.18f * pulse, 0.85f * pulse, 1.0f * pulse);
                    chuteRimMat.color = rimCol;
                }
                else
                {
                    chuteRimMat.color = new Color(0.18f, 0.85f, 1.0f, 0.75f);
                }
            }
        }

        public void IgnoreEnvironmentCollisions()
        {
            int clawLayer = LayerMask.NameToLayer("Claw");
            int envLayer = LayerMask.NameToLayer("Environment");
            int chuteLayer = LayerMask.NameToLayer("Chute");

            if (clawLayer >= 0)
            {
                if (envLayer >= 0) Physics.IgnoreLayerCollision(clawLayer, envLayer, true);
                if (chuteLayer >= 0) Physics.IgnoreLayerCollision(clawLayer, chuteLayer, true);
            }

            List<Collider> fingerCols = new List<Collider>();
            if (arms != null)
            {
                for (int i = 0; i < arms.Length; i++)
                {
                    if (arms[i] != null)
                    {
                        var cols = arms[i].GetComponentsInChildren<Collider>(true);
                        fingerCols.AddRange(cols);
                        if (clawLayer >= 0)
                        {
                            for (int c = 0; c < cols.Length; c++) cols[c].gameObject.layer = clawLayer;
                        }
                    }
                }
            }

            if (fingerCols.Count == 0)
            {
                Collider[] allCols = GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < allCols.Length; i++)
                {
                    if (allCols[i] != null && allCols[i].gameObject.name != "ClawHub")
                    {
                        fingerCols.Add(allCols[i]);
                    }
                }
            }

            MachineController mc = FindFirstObjectByType<MachineController>();
            if (mc != null)
            {
                Collider[] mcCols = mc.GetComponentsInChildren<Collider>(true);
                for (int c = 0; c < fingerCols.Count; c++)
                {
                    for (int e = 0; e < mcCols.Length; e++)
                    {
                        if (fingerCols[c] != null && mcCols[e] != null && !mcCols[e].isTrigger && mcCols[e].GetComponentInParent<Prize>() == null)
                        {
                            Physics.IgnoreCollision(fingerCols[c], mcCols[e], true);
                        }
                    }
                }
            }
        }

        private float GetTrolleySpeedMultiplier()
        {
            return ServiceLocator.TryGet<IEconomyService>(out var econ) ? econ.GetTrolleySpeedMultiplier() : 1f;
        }

        private float GetGripPowerMultiplier()
        {
            return ServiceLocator.TryGet<IEconomyService>(out var econ) ? econ.GetGripPowerMultiplier() : 1f;
        }

        private float GetDropSpeedMultiplier()
        {
            return ServiceLocator.TryGet<IEconomyService>(out var econ) ? econ.GetDropSpeedMultiplier() : 1f;
        }

        private void PlayAudioGrab()
        {
            ServiceLocator.Get<IAudioService>()?.PlayGrabSuccess();
        }

        private void PlayAudioClamp()
        {
            ServiceLocator.Get<IAudioService>()?.PlayClamp();
        }

        private void PlayAudioDrop()
        {
            ServiceLocator.Get<IAudioService>()?.PlayDropFloor();
        }

        private void SetAudioMotor(bool moving)
        {
            ServiceLocator.Get<IAudioService>()?.SetMotorMoving(moving);
        }

        private Bounds GetPrizeBounds(Prize prize)
        {
            if (prize == null) return new Bounds(Vector3.zero, Vector3.one * 0.4f);
            Renderer[] rends = prize.GetComponentsInChildren<Renderer>();
            if (rends != null && rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++)
                {
                    b.Encapsulate(rends[i].bounds);
                }
                return b;
            }
            Collider[] cols = prize.GetComponentsInChildren<Collider>();
            if (cols != null && cols.Length > 0)
            {
                Bounds b = cols[0].bounds;
                for (int i = 1; i < cols.Length; i++)
                {
                    b.Encapsulate(cols[i].bounds);
                }
                return b;
            }
            return new Bounds(prize.transform.position, Vector3.one * 0.4f);
        }

        private void ApplyNearMissNudge(Prize prize)
        {
            if (prize == null) return;
            Rigidbody rb = prize.Rigidbody;
            if (rb != null && !rb.isKinematic)
            {
                Vector3 clawCenter = hoist != null ? hoist.position : transform.position;
                Vector3 awayDir = (prize.transform.position - clawCenter).normalized;
                awayDir.y = 0.35f; // Slight upward pop + outward tumble
                rb.AddForce(awayDir * UnityEngine.Random.Range(1.4f, 2.6f), ForceMode.Impulse);
                rb.AddTorque(UnityEngine.Random.insideUnitSphere * 1.5f, ForceMode.Impulse);
                ClawJuiceEffects.Instance?.PlayPrizeNudgeFX(prize.transform.position);
                ServiceLocator.Get<IAudioService>()?.TriggerCameraShake(0.12f, 0.025f);
                Debug.Log($"[ClawController] 🎲 NEAR-MISS NUDGE applied to {prize.name}!");
            }
        }
    }
}
