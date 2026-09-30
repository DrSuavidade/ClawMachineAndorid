#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Data;
using ClawMachine.Gameplay;
using ClawMachine.UI;

namespace ClawMachine.Editor
{
    [InitializeOnLoad]
    public static class Phase0SceneSetup
    {
        static Phase0SceneSetup()
        {
            EditorApplication.delayCall += CheckAndAutoBuild;
        }

        private static void CheckAndAutoBuild()
        {
            const string marker = "Library/.phase0_built_v2";
            if (!File.Exists(marker) || !File.Exists("Assets/Textures/UI/Sprite_Circle.png") || !File.Exists("Assets/Data/MachineCatalog.asset"))
            {
                File.WriteAllText(marker, "v2");
                Debug.Log("[Phase0SceneSetup] Auto-generating UI sprites and building updated Phase 0 scene with marquee & modals...");
                BuildPrototype();
            }
        }

        [MenuItem("ClawMachine/Build Phase 0 Prototype Scene")]
        public static void BuildPrototype()
        {
            EnsureDirectories();

            // 1. New Scene first so loaded asset handles are not invalidated
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Create or load Config & Definitions
            ClawConfiguration config = CreateOrLoadClawConfig();
            ToyBoxAssetGenerator.GenerateToyBox();
            RetroArcadeAssetGenerator.GenerateRetroArcade();

            MachineDefinition toyBoxDef = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_ToyBox.asset");
            MachineDefinition retroArcadeDef = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_RetroArcade.asset");
            MachineDefinition[] allMachines = new MachineDefinition[] { toyBoxDef, retroArcadeDef };

            // Modular Machine Catalog Asset
            string catalogPath = "Assets/Data/MachineCatalog.asset";
            MachineCatalog catalog = AssetDatabase.LoadAssetAtPath<MachineCatalog>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<MachineCatalog>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }
            catalog.SetMachinesInEditor(allMachines);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            PrizeDefinition[] prizeDefs = (toyBoxDef != null && toyBoxDef.prizes != null && toyBoxDef.prizes.Length > 0)
                ? toyBoxDef.prizes
                : CreateOrLoadPrizeDefinitions();

            // 3. Environment & Light
            SetupLightingAndCamera();

            // 4. Cabinet & Chute
            GameObject cabinet = BuildCabinet(out Transform chuteDropPoint, out ChuteDetector chuteDetector, out Transform spawnCenter, out Transform refillPoint, out MeshRenderer[] frameRenderers, out MeshRenderer backdropRenderer);

            // 5. Claw Assembly
            GameObject clawObj = BuildClaw(config, chuteDropPoint, out ClawController clawController);

            // 6. Spawner & Machine Controller
            GameObject machineManager = new GameObject("MachineController");
            MachineController machineController = machineManager.AddComponent<MachineController>();
            PrizeSpawner spawner = machineManager.AddComponent<PrizeSpawner>();
            machineManager.AddComponent<ClawAudio>();

            CollectionManager collectionMgr = machineManager.AddComponent<CollectionManager>();
            SetSerializedProperty(collectionMgr, "catalog", catalog);
            if (toyBoxDef != null)
            {
                collectionMgr.SetCurrentMachine(toyBoxDef);
                SetSerializedProperty(collectionMgr, "currentMachine", toyBoxDef);
            }

            SetSerializedProperty(spawner, "machineDefinition", toyBoxDef);
            SetSerializedProperty(spawner, "prizePool", prizeDefs);
            SetSerializedProperty(spawner, "spawnAreaCenter", spawnCenter);
            SetSerializedProperty(spawner, "refillDropPoint", refillPoint);
            SetSerializedProperty(spawner, "initialPileCount", 20);

            SetSerializedProperty(machineController, "claw", clawController);
            SetSerializedProperty(machineController, "chuteDetector", chuteDetector);
            SetSerializedProperty(machineController, "prizeSpawner", spawner);
            SetSerializedProperty(machineController, "cabinetFrameRenderers", frameRenderers);
            SetSerializedProperty(machineController, "cabinetBackdropRenderer", backdropRenderer);

            // 7. Juice & Particle Systems
            GameObject juiceObj = new GameObject("JuiceEffects");
            juiceObj.AddComponent<ClawJuiceEffects>();

            // 8. UI Setup
            BuildUI(machineController, catalog);

            // 8. Save Scene
            string scenePath = "Assets/Scenes/Phase0_Prototype.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Phase 0 Prototype Scene successfully built and saved to: {scenePath}");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
            if (!Directory.Exists("Assets/Data")) Directory.CreateDirectory("Assets/Data");
            if (!Directory.Exists("Assets/Prefabs/Prizes")) Directory.CreateDirectory("Assets/Prefabs/Prizes");
            if (!Directory.Exists("Assets/Materials")) Directory.CreateDirectory("Assets/Materials");
            if (!Directory.Exists("Assets/Textures/UI")) Directory.CreateDirectory("Assets/Textures/UI");
            AssetDatabase.Refresh();
        }

        private static Sprite GetOrCreateCircleSprite()
        {
            string path = "Assets/Textures/UI/Sprite_Circle.png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null) return sp;

            EnsureDirectories();
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.48f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float a = Mathf.Clamp01((radius - dist) + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite GetOrCreateRingSprite()
        {
            string path = "Assets/Textures/UI/Sprite_Ring.png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null) return sp;

            EnsureDirectories();
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outerR = size * 0.48f;
            float innerR = size * 0.36f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float aOuter = Mathf.Clamp01((outerR - dist) + 0.5f);
                    float aInner = Mathf.Clamp01((dist - innerR) + 0.5f);
                    float a = Mathf.Min(aOuter, aInner);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static ClawConfiguration CreateOrLoadClawConfig()
        {
            string path = "Assets/Data/ClawConfig_Default.asset";
            ClawConfiguration config = AssetDatabase.LoadAssetAtPath<ClawConfiguration>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ClawConfiguration>();
                config.moveSpeed = 2.4f;
                config.moveDamping = 10f;
                config.xBounds = new Vector2(-1.05f, 1.05f);
                config.zBounds = new Vector2(-1.05f, 1.05f);
                config.dropSpeed = 1.8f;
                config.liftSpeed = 1.4f;
                config.returnSpeed = 2.2f;
                config.dropMinY = 0.5f;
                config.homeY = 3.6f;
                config.openAngle = 38f;
                config.closedAngle = -52f;
                config.armRotateSpeed = 70f;
                config.minContactsForAssist = 2;
                config.gripSupportStrength = 2.2f;
                config.grabToleranceRadius = 0.22f;
                config.slipSensitivity = 1.0f;

                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
            }
            return config;
        }

        private static PrizeDefinition[] CreateOrLoadPrizeDefinitions()
        {
            PhysicsMaterial bouncyMat = new PhysicsMaterial("ToyPhysics")
            {
                dynamicFriction = 0.65f,
                staticFriction = 0.75f,
                bounciness = 0.15f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            string[] names = { "Ball", "CubeToy", "PillToy", "CapsuleDuo" };
            PrimitiveType[] types = { PrimitiveType.Sphere, PrimitiveType.Cube, PrimitiveType.Capsule, PrimitiveType.Cylinder };
            Color[] colors = { new Color(0.95f, 0.35f, 0.35f), new Color(0.35f, 0.65f, 0.95f), new Color(0.35f, 0.85f, 0.45f), new Color(0.95f, 0.85f, 0.25f) };
            Vector3[] scales = { Vector3.one * 0.42f, Vector3.one * 0.40f, new Vector3(0.36f, 0.5f, 0.36f), new Vector3(0.44f, 0.28f, 0.44f) };

            PrizeDefinition[] defs = new PrizeDefinition[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                string defPath = $"Assets/Data/Prize_{names[i]}.asset";
                PrizeDefinition def = AssetDatabase.LoadAssetAtPath<PrizeDefinition>(defPath);
                if (def == null)
                {
                    def = ScriptableObject.CreateInstance<PrizeDefinition>();
                    def.id = names[i].ToLower();
                    def.displayName = names[i];
                    def.rarity = (i == 3) ? PrizeRarity.Rare : PrizeRarity.Normal;
                    def.mass = 0.4f + i * 0.1f;

                    // Prefab
                    string prefabPath = $"Assets/Prefabs/Prizes/Prize_{names[i]}.prefab";
                    GameObject go = GameObject.CreatePrimitive(types[i]);
                    go.name = $"Prize_{names[i]}";
                    go.transform.localScale = scales[i];

                    Collider col = go.GetComponent<Collider>();
                    col.material = bouncyMat;

                    Rigidbody rb = go.AddComponent<Rigidbody>();
                    rb.mass = def.mass;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                    Prize prize = go.AddComponent<Prize>();

                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    mat.color = colors[i];
                    string matPath = $"Assets/Materials/Mat_{names[i]}.mat";
                    AssetDatabase.CreateAsset(mat, matPath);
                    go.GetComponent<Renderer>().sharedMaterial = mat;

                    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                    GameObject.DestroyImmediate(go);

                    def.prefab = prefab;
                    AssetDatabase.CreateAsset(def, defPath);
                }
                defs[i] = def;
            }

            AssetDatabase.SaveAssets();
            return defs;
        }

        private static void SetupLightingAndCamera()
        {
            // Light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.98f, 0.94f);
            light.intensity = 1.35f;
            lightObj.transform.rotation = Quaternion.Euler(48f, -25f, 0f);

            // Camera (Portrait view requested at position 0, 5, -7)
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.fieldOfView = 45f;
            cam.backgroundColor = new Color(0.14f, 0.15f, 0.18f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.transform.position = new Vector3(0f, 5f, -7f);
            camObj.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            camObj.AddComponent<AudioListener>();
        }

        private static GameObject BuildCabinet(out Transform chuteDropPoint, out ChuteDetector chuteDetector, out Transform spawnCenter, out Transform refillPoint, out MeshRenderer[] frameRenderers, out MeshRenderer backdropRenderer)
        {
            GameObject cabinet = new GameObject("Cabinet");

            // Materials
            Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            floorMat.color = new Color(0.22f, 0.23f, 0.28f);

            Material backWallMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            backWallMat.color = new Color(0.40f, 0.48f, 0.62f);

            Material frameMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            frameMat.color = new Color(0.85f, 0.25f, 0.25f); // Red arcade frame

            Material chuteMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            chuteMat.color = new Color(0.18f, 0.20f, 0.25f);

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = cabinet.transform;
            floor.transform.position = new Vector3(0f, 0f, 0f);
            floor.transform.localScale = new Vector3(3.2f, 0.2f, 3.2f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Back Wall (Solid backdrop behind toys)
            GameObject backWall = CreateWall(cabinet.transform, "Wall_Back", new Vector3(0f, 2f, 1.6f), new Vector3(3.2f, 4f, 0.1f), backWallMat);
            backdropRenderer = backWall.GetComponent<MeshRenderer>();

            // Invisible Glass Boundaries (Colliders with MeshRenderer disabled so view is 100% unobstructed)
            GameObject frontWall = CreateWall(cabinet.transform, "Wall_Front_Glass", new Vector3(0f, 2f, -1.6f), new Vector3(3.2f, 4f, 0.1f), null);
            frontWall.GetComponent<MeshRenderer>().enabled = false;

            GameObject leftWall = CreateWall(cabinet.transform, "Wall_Left_Glass", new Vector3(-1.6f, 2f, 0f), new Vector3(0.1f, 4f, 3.2f), null);
            leftWall.GetComponent<MeshRenderer>().enabled = false;

            GameObject rightWall = CreateWall(cabinet.transform, "Wall_Right_Glass", new Vector3(1.6f, 2f, 0f), new Vector3(0.1f, 4f, 3.2f), null);
            rightWall.GetComponent<MeshRenderer>().enabled = false;

            // Arcade corner pillars for visual cabinet definition
            frameRenderers = new MeshRenderer[4];
            frameRenderers[0] = CreatePillar(cabinet.transform, "Pillar_FL", new Vector3(-1.6f, 2f, -1.6f), frameMat);
            frameRenderers[1] = CreatePillar(cabinet.transform, "Pillar_FR", new Vector3(1.6f, 2f, -1.6f), frameMat);
            frameRenderers[2] = CreatePillar(cabinet.transform, "Pillar_BL", new Vector3(-1.6f, 2f, 1.6f), frameMat);
            frameRenderers[3] = CreatePillar(cabinet.transform, "Pillar_BR", new Vector3(1.6f, 2f, 1.6f), frameMat);

            // Prize Chute in front-left corner
            GameObject chuteObj = new GameObject("PrizeChute");
            chuteObj.transform.parent = cabinet.transform;
            chuteObj.transform.position = new Vector3(-1.05f, 0.1f, -1.05f);

            // Chute Wall dividers
            GameObject chuteDividerX = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chuteDividerX.name = "ChuteDivider_X";
            chuteDividerX.transform.parent = chuteObj.transform;
            chuteDividerX.transform.localPosition = new Vector3(0.45f, 0.45f, 0f);
            chuteDividerX.transform.localScale = new Vector3(0.08f, 0.9f, 0.95f);
            chuteDividerX.GetComponent<Renderer>().sharedMaterial = chuteMat;

            GameObject chuteDividerZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chuteDividerZ.name = "ChuteDivider_Z";
            chuteDividerZ.transform.parent = chuteObj.transform;
            chuteDividerZ.transform.localPosition = new Vector3(0f, 0.45f, 0.45f);
            chuteDividerZ.transform.localScale = new Vector3(0.95f, 0.9f, 0.08f);
            chuteDividerZ.GetComponent<Renderer>().sharedMaterial = chuteMat;

            // Chute Detector Trigger
            GameObject triggerObj = new GameObject("ChuteSensor");
            triggerObj.transform.parent = chuteObj.transform;
            triggerObj.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            BoxCollider triggerCol = triggerObj.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(0.75f, 0.4f, 0.75f);
            chuteDetector = triggerObj.AddComponent<ChuteDetector>();

            // Chute Drop Point (Where claw carriage aligns to release)
            GameObject dropPoint = new GameObject("ChuteDropPoint");
            dropPoint.transform.parent = chuteObj.transform;
            dropPoint.transform.position = new Vector3(-1.05f, 3.6f, -1.05f);
            chuteDropPoint = dropPoint.transform;

            // Spawn Center & Refill Point
            GameObject spawnCenterObj = new GameObject("SpawnAreaCenter");
            spawnCenterObj.transform.parent = cabinet.transform;
            spawnCenterObj.transform.position = new Vector3(0.2f, 0.25f, 0.1f);
            spawnCenter = spawnCenterObj.transform;

            GameObject refillPointObj = new GameObject("RefillPoint");
            refillPointObj.transform.parent = cabinet.transform;
            refillPointObj.transform.position = new Vector3(0.2f, 3.2f, 0.1f);
            refillPoint = refillPointObj.transform;

            return cabinet;
        }

        private static GameObject CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            if (mat != null)
            {
                wall.GetComponent<Renderer>().sharedMaterial = mat;
            }
            return wall;
        }

        private static MeshRenderer CreatePillar(Transform parent, string name, Vector3 pos, Material mat)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = name;
            pillar.transform.parent = parent;
            pillar.transform.position = pos;
            pillar.transform.localScale = new Vector3(0.12f, 4f, 0.12f);
            MeshRenderer mr = pillar.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            return mr;
        }

        private static GameObject BuildClaw(ClawConfiguration config, Transform chuteDropPoint, out ClawController controller)
        {
            GameObject clawRoot = new GameObject("ClawAssembly");
            controller = clawRoot.AddComponent<ClawController>();

            // 1. Trolley (Moves X/Z) - NO Rigidbody (plain transform carriage)
            GameObject trolley = new GameObject("Trolley");
            trolley.transform.parent = clawRoot.transform;
            trolley.transform.position = new Vector3(0f, 3.6f, 0f);

            // 2. Hoist (Moves Y relative to trolley) - NO Rigidbody (plain transform carriage)
            GameObject hoist = new GameObject("Hoist");
            hoist.transform.parent = trolley.transform;
            hoist.transform.position = trolley.transform.position;

            // Visual Hub/Body of claw
            GameObject clawBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            clawBody.name = "ClawHub";
            clawBody.transform.parent = hoist.transform;
            clawBody.transform.localPosition = Vector3.zero;
            clawBody.transform.localScale = new Vector3(0.42f, 0.14f, 0.42f);
            clawBody.GetComponent<Collider>().enabled = false;

            Material chromeMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            chromeMat.color = new Color(0.88f, 0.88f, 0.92f);
            clawBody.GetComponent<Renderer>().sharedMaterial = chromeMat;

            // Reticle / Shadow marker on floor (diameter matches 0.22m grab tolerance radius)
            GameObject reticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reticle.name = "FloorReticle";
            reticle.transform.parent = clawRoot.transform;
            reticle.transform.position = new Vector3(0f, 0.11f, 0f);
            reticle.transform.localScale = new Vector3(0.44f, 0.005f, 0.44f);
            reticle.GetComponent<Collider>().enabled = false;
            Material reticleMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            reticleMat.color = new Color(1f, 0.85f, 0.2f, 0.6f);
            reticle.GetComponent<Renderer>().sharedMaterial = reticleMat;

            // Kinematic Rigidbody on hoist to anchor the HingeJoints
            Rigidbody hoistRb = hoist.AddComponent<Rigidbody>();
            hoistRb.isKinematic = true;

            // 3 Prongs (120 deg apart)
            ClawArm[] arms = new ClawArm[3];
            float[] angles = { 0f, 120f, 240f };

            // High-friction arcade prong physics material (firm grip, no slip, zero bounce)
            PhysicsMaterial prongPhysMat = new PhysicsMaterial("ProngPhysicsMat")
            {
                dynamicFriction = 0.95f,
                staticFriction = 1.0f,
                bounciness = 0.0f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };

            for (int i = 0; i < 3; i++)
            {
                GameObject armPivot = new GameObject($"ArmPivot_{i}");
                armPivot.transform.parent = hoist.transform;
                armPivot.transform.localPosition = Vector3.zero;
                armPivot.transform.localRotation = Quaternion.Euler(0f, angles[i], 0f);

                // Arm Rigidbody (sufficient mass for firm grip)
                Rigidbody armRb = armPivot.AddComponent<Rigidbody>();
                armRb.mass = 1.2f;
                armRb.linearDamping = 1.0f;
                armRb.angularDamping = 2.0f;
                armRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // Arm Hinge Joint anchored to Hoist
                HingeJoint hinge = armPivot.AddComponent<HingeJoint>();
                hinge.connectedBody = hoistRb;
                hinge.axis = Vector3.forward;
                hinge.useLimits = true;
                JointLimits limits = hinge.limits;
                limits.min = -56f; // Allows tips to meet in center when empty
                limits.max = 44f;
                hinge.limits = limits;

                JointSpring spring = hinge.spring;
                spring.spring = 250f;
                spring.damper = 25f;
                spring.targetPosition = config != null ? config.openAngle : 38f;
                hinge.spring = spring;
                hinge.useSpring = true;

                // Solid physical arm geometry
                GameObject armSegment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                armSegment.name = $"ArmSegment_{i}";
                armSegment.transform.parent = armPivot.transform;
                armSegment.transform.localPosition = new Vector3(0.24f, -0.28f, 0f);
                armSegment.transform.localRotation = Quaternion.Euler(0f, 0f, 20f);
                armSegment.transform.localScale = new Vector3(0.08f, 0.32f, 0.08f);
                armSegment.GetComponent<Renderer>().sharedMaterial = chromeMat;
                armSegment.GetComponent<Collider>().material = prongPhysMat;

                // Inward curved talon scoop (solid)
                GameObject talon = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                talon.name = $"Talon_{i}";
                talon.transform.parent = armSegment.transform;
                talon.transform.localPosition = new Vector3(-0.45f, -0.85f, 0f);
                talon.transform.localRotation = Quaternion.Euler(0f, 0f, -50f);
                talon.transform.localScale = new Vector3(0.85f, 0.70f, 0.85f);
                talon.GetComponent<Renderer>().sharedMaterial = chromeMat;
                talon.GetComponent<Collider>().material = prongPhysMat;

                GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = $"Tip_{i}";
                tip.transform.parent = talon.transform;
                tip.transform.localPosition = new Vector3(0f, -0.9f, 0f);
                tip.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                tip.GetComponent<Renderer>().sharedMaterial = chromeMat;
                tip.GetComponent<Collider>().material = prongPhysMat;

                ClawArm armComp = armPivot.AddComponent<ClawArm>();
                armComp.SetupHinge(hinge, armRb, tip.transform);
                SetSerializedProperty(armComp, "hinge", hinge);
                SetSerializedProperty(armComp, "armRigidbody", armRb);
                SetSerializedProperty(armComp, "tipTransform", tip.transform);

                arms[i] = armComp;
            }

            // Grip Socket (central basket target where fingers hold prizes)
            GameObject socketObj = new GameObject("GripSocket");
            socketObj.transform.parent = hoist.transform;
            socketObj.transform.localPosition = new Vector3(0f, -0.65f, 0f);

            // Grip Capture Volume (trigger zone under hub)
            GameObject captureObj = new GameObject("GripCaptureVolume");
            captureObj.transform.parent = hoist.transform;
            captureObj.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            BoxCollider captureBox = captureObj.AddComponent<BoxCollider>();
            captureBox.isTrigger = true;
            captureBox.size = new Vector3(1.1f, 1.2f, 1.1f);
            GripCaptureVolume captureVolume = captureObj.AddComponent<GripCaptureVolume>();

            // Grip Anchor (kinematic holding + sway + detach)
            ClawGripAnchor anchor = hoist.AddComponent<ClawGripAnchor>();
            anchor.Initialize(socketObj.transform);

            // Setup Controller references directly
            controller.Setup(config, trolley.transform, hoist.transform, arms, chuteDropPoint, reticle.transform, socketObj.transform, captureVolume, anchor);
            SetSerializedProperty(controller, "config", config);
            SetSerializedProperty(controller, "trolley", trolley.transform);
            SetSerializedProperty(controller, "hoist", hoist.transform);
            SetSerializedProperty(controller, "arms", arms);
            SetSerializedProperty(controller, "chuteDropPoint", chuteDropPoint);
            SetSerializedProperty(controller, "clawMarker", reticle.transform);
            SetSerializedProperty(controller, "gripSocket", socketObj.transform);
            SetSerializedProperty(controller, "captureVolume", captureVolume);
            SetSerializedProperty(controller, "gripAnchor", anchor);

            return clawRoot;
        }

        private static void BuildUI(MachineController machineController, MachineCatalog catalog)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Canvas
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // 1. Playfield Drag Area (From top of console deck up to status bar: 16% to 88%)
            GameObject dragArea = new GameObject("DragArea");
            dragArea.transform.parent = canvasObj.transform;
            RectTransform dragRect = dragArea.AddComponent<RectTransform>();
            dragRect.anchorMin = new Vector2(0f, 0.16f);
            dragRect.anchorMax = new Vector2(1f, 0.88f);
            dragRect.offsetMin = Vector2.zero;
            dragRect.offsetMax = Vector2.zero;

            Image dragImg = dragArea.AddComponent<Image>();
            dragImg.color = new Color(0f, 0f, 0f, 0.001f); // Invisible raycast target

            MachineControlsUI controlsUI = dragArea.AddComponent<MachineControlsUI>();
            SetSerializedProperty(controlsUI, "machine", machineController);

            // 2. Slim Skeuomorphic Arcade Console Deck (Physical Machine Lower Panel - 16% screen height)
            GameObject consoleDeck = new GameObject("ConsoleDeck");
            consoleDeck.transform.parent = canvasObj.transform;
            RectTransform deckRect = consoleDeck.AddComponent<RectTransform>();
            deckRect.anchorMin = new Vector2(0f, 0f);
            deckRect.anchorMax = new Vector2(1f, 0.16f);
            deckRect.offsetMin = Vector2.zero;
            deckRect.offsetMax = Vector2.zero;

            Image deckImg = consoleDeck.AddComponent<Image>();
            deckImg.color = new Color(0.14f, 0.15f, 0.19f, 0.99f); // Dark brushed metallic plate

            // Top Metallic Beveled Edge
            GameObject rimObj = new GameObject("ConsoleRim");
            rimObj.transform.parent = consoleDeck.transform;
            RectTransform rimRect = rimObj.AddComponent<RectTransform>();
            rimRect.anchorMin = new Vector2(0f, 0.97f);
            rimRect.anchorMax = new Vector2(1f, 1f);
            rimRect.offsetMin = Vector2.zero;
            rimRect.offsetMax = Vector2.zero;
            Image rimImg = rimObj.AddComponent<Image>();
            rimImg.color = new Color(0.38f, 0.42f, 0.54f);

            // 4 Corner Bolts/Rivets for industrial arcade feel
            CreateDeckRivet(consoleDeck.transform, new Vector2(0.012f, 0.88f));
            CreateDeckRivet(consoleDeck.transform, new Vector2(0.988f, 0.88f));
            CreateDeckRivet(consoleDeck.transform, new Vector2(0.012f, 0.08f));
            CreateDeckRivet(consoleDeck.transform, new Vector2(0.988f, 0.08f));

            Sprite circleSprite = GetOrCreateCircleSprite();
            Sprite ringSprite = GetOrCreateRingSprite();

            // A. Left: Skeuomorphic Ball-Top Joystick
            GameObject stickRoot = new GameObject("ArcadeJoystick", typeof(RectTransform));
            stickRoot.transform.SetParent(consoleDeck.transform, false);
            RectTransform stickRect = stickRoot.GetComponent<RectTransform>();
            stickRect.anchorMin = new Vector2(0.02f, 0.06f);
            stickRect.anchorMax = new Vector2(0.27f, 0.94f);
            stickRect.offsetMin = Vector2.zero;
            stickRect.offsetMax = Vector2.zero;
            stickRect.anchoredPosition = Vector2.zero;

            // Outer Base Collar (Metal washer dish)
            GameObject collarObj = new GameObject("CollarBase", typeof(RectTransform));
            collarObj.transform.SetParent(stickRoot.transform, false);
            RectTransform collarRect = collarObj.GetComponent<RectTransform>();
            collarRect.anchorMin = new Vector2(0.5f, 0.5f);
            collarRect.anchorMax = new Vector2(0.5f, 0.5f);
            collarRect.anchoredPosition = Vector2.zero;
            collarRect.sizeDelta = new Vector2(165, 165);
            Image collarImg = collarObj.AddComponent<Image>();
            collarImg.sprite = circleSprite;
            collarImg.color = new Color(0.20f, 0.22f, 0.28f);

            // Armed Glow Outline Ring
            GameObject outlineObj = new GameObject("OutlineRing", typeof(RectTransform));
            outlineObj.transform.SetParent(stickRoot.transform, false);
            RectTransform outRect = outlineObj.GetComponent<RectTransform>();
            outRect.anchorMin = new Vector2(0.5f, 0.5f);
            outRect.anchorMax = new Vector2(0.5f, 0.5f);
            outRect.anchoredPosition = Vector2.zero;
            outRect.sizeDelta = new Vector2(185, 185);
            Image outImg = outlineObj.AddComponent<Image>();
            outImg.sprite = ringSprite;
            outImg.color = new Color(0.2f, 0.95f, 1f, 0.90f);
            outlineObj.SetActive(false);

            // Shaft & Ball Knob
            GameObject knobObj = new GameObject("BallKnob", typeof(RectTransform));
            knobObj.transform.SetParent(stickRoot.transform, false);
            RectTransform knobRect = knobObj.GetComponent<RectTransform>();
            knobRect.anchorMin = new Vector2(0.5f, 0.5f);
            knobRect.anchorMax = new Vector2(0.5f, 0.5f);
            knobRect.anchoredPosition = Vector2.zero;
            knobRect.sizeDelta = new Vector2(105, 105);
            Image knobImg = knobObj.AddComponent<Image>();
            knobImg.sprite = circleSprite;
            knobImg.color = new Color(0.95f, 0.18f, 0.22f); // Cherry arcade ball

            // Knob Highlight Specular Dot
            GameObject specObj = new GameObject("SpecularDot", typeof(RectTransform));
            specObj.transform.SetParent(knobObj.transform, false);
            RectTransform specRect = specObj.GetComponent<RectTransform>();
            specRect.anchorMin = new Vector2(0.5f, 0.5f);
            specRect.anchorMax = new Vector2(0.5f, 0.5f);
            specRect.anchoredPosition = new Vector2(20, 20);
            specRect.sizeDelta = new Vector2(22, 22);
            Image specImg = specObj.AddComponent<Image>();
            specImg.sprite = circleSprite;
            specImg.color = new Color(1f, 1f, 1f, 0.65f);

            ArcadeJoystickUI joystickUI = stickRoot.AddComponent<ArcadeJoystickUI>();
            SetSerializedProperty(joystickUI, "machineController", machineController);
            SetSerializedProperty(joystickUI, "knobTransform", knobRect);
            SetSerializedProperty(joystickUI, "outlineRing", outImg);
            SetSerializedProperty(joystickUI, "ballKnobImage", knobImg);

            // B. Center: Machine Switcher & Info Panel
            GameObject carouselStrip = new GameObject("CarouselStrip", typeof(RectTransform));
            carouselStrip.transform.SetParent(consoleDeck.transform, false);
            RectTransform cStripRect = carouselStrip.GetComponent<RectTransform>();
            cStripRect.anchorMin = new Vector2(0.28f, 0.08f);
            cStripRect.anchorMax = new Vector2(0.71f, 0.92f);
            cStripRect.offsetMin = Vector2.zero;
            cStripRect.offsetMax = Vector2.zero;
            cStripRect.anchoredPosition = Vector2.zero;
            Image cStripImg = carouselStrip.AddComponent<Image>();
            cStripImg.color = new Color(0.08f, 0.09f, 0.13f, 0.94f);

            // Prev Button (<)
            GameObject prevBtnObj = new GameObject("Btn_PrevMachine", typeof(RectTransform));
            prevBtnObj.transform.SetParent(carouselStrip.transform, false);
            RectTransform pbRect = prevBtnObj.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0f, 0f);
            pbRect.anchorMax = new Vector2(0.20f, 1f);
            pbRect.offsetMin = Vector2.zero;
            pbRect.offsetMax = Vector2.zero;
            pbRect.anchoredPosition = Vector2.zero;
            Image pbImg = prevBtnObj.AddComponent<Image>();
            pbImg.color = new Color(0.18f, 0.22f, 0.32f);
            Button prevBtn = prevBtnObj.AddComponent<Button>();

            GameObject pbTextObj = new GameObject("Text", typeof(RectTransform));
            pbTextObj.transform.SetParent(prevBtnObj.transform, false);
            RectTransform pbtRect = pbTextObj.GetComponent<RectTransform>();
            pbtRect.anchorMin = Vector2.zero;
            pbtRect.anchorMax = Vector2.one;
            pbtRect.offsetMin = Vector2.zero;
            pbtRect.offsetMax = Vector2.zero;
            pbtRect.anchoredPosition = Vector2.zero;
            Text pbText = pbTextObj.AddComponent<Text>();
            pbText.text = "◀";
            pbText.font = font;
            pbText.fontSize = 38;
            pbText.alignment = TextAnchor.MiddleCenter;
            pbText.color = Color.white;

            // Next Button (>)
            GameObject nextBtnObj = new GameObject("Btn_NextMachine", typeof(RectTransform));
            nextBtnObj.transform.SetParent(carouselStrip.transform, false);
            RectTransform nbRect = nextBtnObj.GetComponent<RectTransform>();
            nbRect.anchorMin = new Vector2(0.80f, 0f);
            nbRect.anchorMax = new Vector2(1f, 1f);
            nbRect.offsetMin = Vector2.zero;
            nbRect.offsetMax = Vector2.zero;
            nbRect.anchoredPosition = Vector2.zero;
            Image nbImg = nextBtnObj.AddComponent<Image>();
            nbImg.color = new Color(0.18f, 0.22f, 0.32f);
            Button nextBtn = nextBtnObj.AddComponent<Button>();

            GameObject nbTextObj = new GameObject("Text", typeof(RectTransform));
            nbTextObj.transform.SetParent(nextBtnObj.transform, false);
            RectTransform nbtRect = nbTextObj.GetComponent<RectTransform>();
            nbtRect.anchorMin = Vector2.zero;
            nbtRect.anchorMax = Vector2.one;
            nbtRect.offsetMin = Vector2.zero;
            nbtRect.offsetMax = Vector2.zero;
            nbtRect.anchoredPosition = Vector2.zero;
            Text nbText = nbTextObj.AddComponent<Text>();
            nbText.text = "▶";
            nbText.font = font;
            nbText.fontSize = 38;
            nbText.alignment = TextAnchor.MiddleCenter;
            nbText.color = Color.white;

            // Machine Name Text (Middle Top)
            GameObject mNameObj = new GameObject("Text_MachineName", typeof(RectTransform));
            mNameObj.transform.SetParent(carouselStrip.transform, false);
            RectTransform mnRect = mNameObj.GetComponent<RectTransform>();
            mnRect.anchorMin = new Vector2(0.20f, 0.48f);
            mnRect.anchorMax = new Vector2(0.80f, 0.95f);
            mnRect.offsetMin = Vector2.zero;
            mnRect.offsetMax = Vector2.zero;
            mnRect.anchoredPosition = Vector2.zero;
            Text mnText = mNameObj.AddComponent<Text>();
            mnText.text = "TOY BOX";
            mnText.font = font;
            mnText.fontSize = 28;
            mnText.fontStyle = FontStyle.Bold;
            mnText.alignment = TextAnchor.MiddleCenter;
            mnText.color = new Color(1f, 0.88f, 0.3f);

            // Machine Status / Info Text (Middle Bottom)
            GameObject mStatusObj = new GameObject("Text_MachineStatus", typeof(RectTransform));
            mStatusObj.transform.SetParent(carouselStrip.transform, false);
            RectTransform msRect = mStatusObj.GetComponent<RectTransform>();
            msRect.anchorMin = new Vector2(0.20f, 0.05f);
            msRect.anchorMax = new Vector2(0.80f, 0.48f);
            msRect.offsetMin = Vector2.zero;
            msRect.offsetMax = Vector2.zero;
            msRect.anchoredPosition = Vector2.zero;
            Text msText = mStatusObj.AddComponent<Text>();
            msText.text = "FREE ENTRY";
            msText.font = font;
            msText.fontSize = 20;
            msText.alignment = TextAnchor.MiddleCenter;
            msText.color = new Color(0.80f, 0.85f, 0.95f);

            // C. Right: Skeuomorphic Arcade DROP Button
            GameObject pushBtnRoot = new GameObject("ArcadePushButton", typeof(RectTransform));
            pushBtnRoot.transform.SetParent(consoleDeck.transform, false);
            RectTransform pbRootRect = pushBtnRoot.GetComponent<RectTransform>();
            pbRootRect.anchorMin = new Vector2(0.72f, 0.06f);
            pbRootRect.anchorMax = new Vector2(0.98f, 0.94f);
            pbRootRect.offsetMin = Vector2.zero;
            pbRootRect.offsetMax = Vector2.zero;
            pbRootRect.anchoredPosition = Vector2.zero;

            // Outer Bezel Ring
            GameObject bezelObj = new GameObject("Bezel", typeof(RectTransform));
            bezelObj.transform.SetParent(pushBtnRoot.transform, false);
            RectTransform bzRect = bezelObj.GetComponent<RectTransform>();
            bzRect.anchorMin = new Vector2(0.5f, 0.5f);
            bzRect.anchorMax = new Vector2(0.5f, 0.5f);
            bzRect.anchoredPosition = Vector2.zero;
            bzRect.sizeDelta = new Vector2(165, 165);
            Image bzImg = bezelObj.AddComponent<Image>();
            bzImg.sprite = circleSprite;
            bzImg.color = new Color(0.24f, 0.26f, 0.34f);

            // Inner Plunger (moves down on press)
            GameObject plungerObj = new GameObject("Plunger", typeof(RectTransform));
            plungerObj.transform.SetParent(pushBtnRoot.transform, false);
            RectTransform plgRect = plungerObj.GetComponent<RectTransform>();
            plgRect.anchorMin = new Vector2(0.5f, 0.5f);
            plgRect.anchorMax = new Vector2(0.5f, 0.5f);
            plgRect.anchoredPosition = Vector2.zero;
            plgRect.sizeDelta = new Vector2(132, 132);
            Image plgImg = plungerObj.AddComponent<Image>();
            plgImg.sprite = circleSprite;
            plgImg.color = new Color(0.92f, 0.22f, 0.22f);

            GameObject btnTextObj = new GameObject("Text", typeof(RectTransform));
            btnTextObj.transform.SetParent(plungerObj.transform, false);
            RectTransform textRect = btnTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = "DROP";
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.fontSize = 38;
            btnText.fontStyle = FontStyle.Bold;
            btnText.color = Color.white;
            btnText.font = font;

            ArcadePushButtonUI pushBtnUI = pushBtnRoot.AddComponent<ArcadePushButtonUI>();
            SetSerializedProperty(pushBtnUI, "machineController", machineController);
            SetSerializedProperty(pushBtnUI, "plungerTransform", plgRect);
            SetSerializedProperty(pushBtnUI, "plungerImage", plgImg);
            SetSerializedProperty(pushBtnUI, "labelText", btnText);

            Button dropBtn = pushBtnRoot.AddComponent<Button>();
            SetSerializedProperty(controlsUI, "dropButton", dropBtn);

            // Status Text (Top)
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.parent = canvasObj.transform;
            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.05f, 0.88f);
            statusRect.anchorMax = new Vector2(0.95f, 0.93f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;

            Text statusText = statusObj.AddComponent<Text>();
            statusText.text = "STATUS: READY";
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.fontSize = 38;
            statusText.fontStyle = FontStyle.Bold;
            statusText.color = Color.white;
            statusText.font = font;
            SetSerializedProperty(controlsUI, "stateText", statusText);

            // Instructions text
            GameObject hintObj = new GameObject("HintText");
            hintObj.transform.parent = canvasObj.transform;
            RectTransform hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.05f, 0.84f);
            hintRect.anchorMax = new Vector2(0.95f, 0.88f);
            hintRect.offsetMin = Vector2.zero;
            hintRect.offsetMax = Vector2.zero;

            Text hintText = hintObj.AddComponent<Text>();
            hintText.text = "Tap joystick to outline / drag screen | Swipe console or ◀ ▶ to switch machines";
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.fontSize = 24;
            hintText.color = new Color(0.75f, 0.8f, 0.9f);
            hintText.font = font;

            // Wins Text
            GameObject winsObj = new GameObject("WinsText");
            winsObj.transform.parent = canvasObj.transform;
            RectTransform winsRect = winsObj.AddComponent<RectTransform>();
            winsRect.anchorMin = new Vector2(0.05f, 0.78f);
            winsRect.anchorMax = new Vector2(0.95f, 0.84f);
            winsRect.offsetMin = Vector2.zero;
            winsRect.offsetMax = Vector2.zero;

            Text winsText = winsObj.AddComponent<Text>();
            winsText.text = "WON: None";
            winsText.alignment = TextAnchor.MiddleCenter;
            winsText.fontSize = 34;
            winsText.color = new Color(1f, 0.85f, 0.2f);
            winsText.font = font;
            SetSerializedProperty(controlsUI, "winsText", winsText);

            // 3. Unlock Modal
            GameObject unlockModalObj = new GameObject("UnlockModal");
            unlockModalObj.transform.parent = canvasObj.transform;
            RectTransform umRect = unlockModalObj.AddComponent<RectTransform>();
            umRect.anchorMin = new Vector2(0.08f, 0.32f);
            umRect.anchorMax = new Vector2(0.92f, 0.68f);
            umRect.offsetMin = Vector2.zero;
            umRect.offsetMax = Vector2.zero;
            Image umBg = unlockModalObj.AddComponent<Image>();
            umBg.color = new Color(0.09f, 0.10f, 0.15f, 0.98f);

            // Modal Title
            GameObject utObj = new GameObject("Title");
            utObj.transform.parent = unlockModalObj.transform;
            RectTransform utRect = utObj.AddComponent<RectTransform>();
            utRect.anchorMin = new Vector2(0.05f, 0.75f);
            utRect.anchorMax = new Vector2(0.95f, 0.95f);
            utRect.offsetMin = Vector2.zero;
            utRect.offsetMax = Vector2.zero;
            Text utText = utObj.AddComponent<Text>();
            utText.text = "UNLOCK NEW MACHINE";
            utText.font = font;
            utText.fontSize = 40;
            utText.fontStyle = FontStyle.Bold;
            utText.alignment = TextAnchor.MiddleCenter;
            utText.color = new Color(1f, 0.85f, 0.2f);

            // Modal Description
            GameObject udObj = new GameObject("Description");
            udObj.transform.parent = unlockModalObj.transform;
            RectTransform udRect = udObj.AddComponent<RectTransform>();
            udRect.anchorMin = new Vector2(0.08f, 0.48f);
            udRect.anchorMax = new Vector2(0.92f, 0.74f);
            udRect.offsetMin = Vector2.zero;
            udRect.offsetMax = Vector2.zero;
            Text udText = udObj.AddComponent<Text>();
            udText.text = "Unlock the Retro Arcade cabinet with 9 exclusive retro collectibles!";
            udText.font = font;
            udText.fontSize = 28;
            udText.alignment = TextAnchor.MiddleCenter;
            udText.color = Color.white;

            // Modal Balance
            GameObject ubObj = new GameObject("Balance");
            ubObj.transform.parent = unlockModalObj.transform;
            RectTransform ubRect = ubObj.AddComponent<RectTransform>();
            ubRect.anchorMin = new Vector2(0.08f, 0.30f);
            ubRect.anchorMax = new Vector2(0.92f, 0.46f);
            ubRect.offsetMin = Vector2.zero;
            ubRect.offsetMax = Vector2.zero;
            Text ubText = ubObj.AddComponent<Text>();
            ubText.text = "Cost: 150 🪙  |  Your Coins: 100 🪙";
            ubText.font = font;
            ubText.fontSize = 30;
            ubText.fontStyle = FontStyle.Bold;
            ubText.alignment = TextAnchor.MiddleCenter;
            ubText.color = new Color(1f, 0.85f, 0.2f);

            // Confirm Unlock Button
            GameObject ucBtnObj = new GameObject("Btn_ConfirmUnlock");
            ucBtnObj.transform.parent = unlockModalObj.transform;
            RectTransform ucbRect = ucBtnObj.AddComponent<RectTransform>();
            ucbRect.anchorMin = new Vector2(0.08f, 0.08f);
            ucbRect.anchorMax = new Vector2(0.48f, 0.26f);
            ucbRect.offsetMin = Vector2.zero;
            ucbRect.offsetMax = Vector2.zero;
            Image ucbImg = ucBtnObj.AddComponent<Image>();
            ucbImg.color = new Color(0.25f, 0.75f, 0.35f);
            Button ucBtn = ucBtnObj.AddComponent<Button>();

            GameObject ucbTextObj = new GameObject("Text");
            ucbTextObj.transform.parent = ucBtnObj.transform;
            RectTransform ucbtRect = ucbTextObj.AddComponent<RectTransform>();
            ucbtRect.anchorMin = Vector2.zero;
            ucbtRect.anchorMax = Vector2.one;
            ucbtRect.offsetMin = Vector2.zero;
            ucbtRect.offsetMax = Vector2.zero;
            Text ucbText = ucbTextObj.AddComponent<Text>();
            ucbText.text = "UNLOCK";
            ucbText.font = font;
            ucbText.fontSize = 32;
            ucbText.fontStyle = FontStyle.Bold;
            ucbText.alignment = TextAnchor.MiddleCenter;
            ucbText.color = Color.white;

            // Cancel Button
            GameObject uclBtnObj = new GameObject("Btn_CancelUnlock");
            uclBtnObj.transform.parent = unlockModalObj.transform;
            RectTransform uclbRect = uclBtnObj.AddComponent<RectTransform>();
            uclbRect.anchorMin = new Vector2(0.52f, 0.08f);
            uclbRect.anchorMax = new Vector2(0.92f, 0.26f);
            uclbRect.offsetMin = Vector2.zero;
            uclbRect.offsetMax = Vector2.zero;
            Image uclbImg = uclBtnObj.AddComponent<Image>();
            uclbImg.color = new Color(0.65f, 0.25f, 0.25f);
            Button uclBtn = uclBtnObj.AddComponent<Button>();

            GameObject uclbTextObj = new GameObject("Text");
            uclbTextObj.transform.parent = uclBtnObj.transform;
            RectTransform uclbtRect = uclbTextObj.AddComponent<RectTransform>();
            uclbtRect.anchorMin = Vector2.zero;
            uclbtRect.anchorMax = Vector2.one;
            uclbtRect.offsetMin = Vector2.zero;
            uclbtRect.offsetMax = Vector2.zero;
            Text uclbText = uclbTextObj.AddComponent<Text>();
            uclbText.text = "CANCEL";
            uclbText.font = font;
            uclbText.fontSize = 32;
            uclbText.fontStyle = FontStyle.Bold;
            uclbText.alignment = TextAnchor.MiddleCenter;
            uclbText.color = Color.white;

            unlockModalObj.SetActive(false);

            // Hook up ArcadeConsoleUI on canvas
            ArcadeConsoleUI consoleUI = canvasObj.AddComponent<ArcadeConsoleUI>();
            SetSerializedProperty(consoleUI, "machineController", machineController);
            SetSerializedProperty(consoleUI, "catalog", catalog);
            SetSerializedProperty(consoleUI, "availableMachines", catalog != null ? catalog.Machines : null);
            SetSerializedProperty(consoleUI, "prevMachineButton", prevBtn);
            SetSerializedProperty(consoleUI, "nextMachineButton", nextBtn);
            SetSerializedProperty(consoleUI, "machineNameText", mnText);
            SetSerializedProperty(consoleUI, "machineStatusText", msText);
            SetSerializedProperty(consoleUI, "unlockModal", unlockModalObj);
            SetSerializedProperty(consoleUI, "unlockTitleText", utText);
            SetSerializedProperty(consoleUI, "unlockDescriptionText", udText);
            SetSerializedProperty(consoleUI, "unlockCoinsBalanceText", ubText);
            SetSerializedProperty(consoleUI, "unlockConfirmButton", ucBtn);
            SetSerializedProperty(consoleUI, "unlockConfirmButtonText", ucbText);
            SetSerializedProperty(consoleUI, "unlockCancelButton", uclBtn);

            // 4. Skeuomorphic Machine Marquee Canopy (Top Header with 3 buttons)
            BuildMarqueeHeader(canvasObj, out Button collBtn, out Button upgBtn, out Button setBtn, out Text coinsText);

            // 5. Modals
            BuildCollectionUI(canvasObj, collBtn, coinsText);
            BuildUpgradesUI(canvasObj, upgBtn, coinsText);
            BuildSettingsUI(canvasObj, setBtn);
        }

        private static void CreateDeckRivet(Transform parent, Vector2 anchor)
        {
            GameObject rivet = new GameObject("Rivet");
            rivet.transform.parent = parent;
            RectTransform rt = rivet.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(14, 14);
            Image img = rivet.AddComponent<Image>();
            img.color = new Color(0.58f, 0.62f, 0.70f);
        }

        private static void BuildMarqueeHeader(GameObject canvasObj, out Button collBtn, out Button upgBtn, out Button setBtn, out Text coinsText)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Marquee Canopy (Cabinet Top Crown - 8.5% screen height)
            GameObject marquee = new GameObject("MachineMarqueeCanopy", typeof(RectTransform));
            marquee.transform.SetParent(canvasObj.transform, false);
            RectTransform mRect = marquee.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0f, 0.915f);
            mRect.anchorMax = new Vector2(1f, 1f);
            mRect.offsetMin = Vector2.zero;
            mRect.offsetMax = Vector2.zero;
            mRect.anchoredPosition = Vector2.zero;

            Image mImg = marquee.AddComponent<Image>();
            mImg.color = new Color(0.12f, 0.13f, 0.17f, 0.99f); // Dark industrial chassis

            // Lower Metallic Beveled Edge
            GameObject rimObj = new GameObject("MarqueeRim", typeof(RectTransform));
            rimObj.transform.SetParent(marquee.transform, false);
            RectTransform rimRect = rimObj.GetComponent<RectTransform>();
            rimRect.anchorMin = new Vector2(0f, 0f);
            rimRect.anchorMax = new Vector2(1f, 0.05f);
            rimRect.offsetMin = Vector2.zero;
            rimRect.offsetMax = Vector2.zero;
            rimRect.anchoredPosition = Vector2.zero;
            Image rimImg = rimObj.AddComponent<Image>();
            rimImg.color = new Color(0.38f, 0.44f, 0.56f);

            // Neon Glow Runner Strip
            GameObject neonObj = new GameObject("NeonRunner", typeof(RectTransform));
            neonObj.transform.SetParent(marquee.transform, false);
            RectTransform neonRect = neonObj.GetComponent<RectTransform>();
            neonRect.anchorMin = new Vector2(0f, 0.05f);
            neonRect.anchorMax = new Vector2(1f, 0.09f);
            neonRect.offsetMin = Vector2.zero;
            neonRect.offsetMax = Vector2.zero;
            neonRect.anchoredPosition = Vector2.zero;
            Image neonImg = neonObj.AddComponent<Image>();
            neonImg.color = new Color(0.20f, 0.88f, 1f, 0.85f); // Neon Cyan

            // 4 Corner Bolts
            CreateDeckRivet(marquee.transform, new Vector2(0.012f, 0.88f));
            CreateDeckRivet(marquee.transform, new Vector2(0.988f, 0.88f));
            CreateDeckRivet(marquee.transform, new Vector2(0.012f, 0.16f));
            CreateDeckRivet(marquee.transform, new Vector2(0.988f, 0.16f));

            // 1. Collection Button (Left)
            GameObject collBtnObj = new GameObject("Btn_Collection", typeof(RectTransform));
            collBtnObj.transform.SetParent(marquee.transform, false);
            RectTransform collBtnRect = collBtnObj.GetComponent<RectTransform>();
            collBtnRect.anchorMin = new Vector2(0.025f, 0.18f);
            collBtnRect.anchorMax = new Vector2(0.275f, 0.84f);
            collBtnRect.offsetMin = Vector2.zero;
            collBtnRect.offsetMax = Vector2.zero;
            collBtnRect.anchoredPosition = Vector2.zero;

            Image collBtnImg = collBtnObj.AddComponent<Image>();
            collBtnImg.color = new Color(0.18f, 0.38f, 0.78f);
            collBtn = collBtnObj.AddComponent<Button>();

            GameObject collTextObj = new GameObject("Text", typeof(RectTransform));
            collTextObj.transform.SetParent(collBtnObj.transform, false);
            RectTransform collTextRect = collTextObj.GetComponent<RectTransform>();
            collTextRect.anchorMin = Vector2.zero;
            collTextRect.anchorMax = Vector2.one;
            collTextRect.offsetMin = Vector2.zero;
            collTextRect.offsetMax = Vector2.zero;
            collTextRect.anchoredPosition = Vector2.zero;
            Text collText = collTextObj.AddComponent<Text>();
            collText.text = "COLLECTION";
            collText.font = font;
            collText.fontSize = 24;
            collText.fontStyle = FontStyle.Bold;
            collText.alignment = TextAnchor.MiddleCenter;
            collText.color = Color.white;

            // 2. Upgrades Button (Middle-Left)
            GameObject upgBtnObj = new GameObject("Btn_Upgrades", typeof(RectTransform));
            upgBtnObj.transform.SetParent(marquee.transform, false);
            RectTransform upgBtnRect = upgBtnObj.GetComponent<RectTransform>();
            upgBtnRect.anchorMin = new Vector2(0.290f, 0.18f);
            upgBtnRect.anchorMax = new Vector2(0.550f, 0.84f);
            upgBtnRect.offsetMin = Vector2.zero;
            upgBtnRect.offsetMax = Vector2.zero;
            upgBtnRect.anchoredPosition = Vector2.zero;

            Image upgBtnImg = upgBtnObj.AddComponent<Image>();
            upgBtnImg.color = new Color(0.85f, 0.52f, 0.12f);
            upgBtn = upgBtnObj.AddComponent<Button>();

            GameObject upgTextObj = new GameObject("Text", typeof(RectTransform));
            upgTextObj.transform.SetParent(upgBtnObj.transform, false);
            RectTransform upgTextRect = upgTextObj.GetComponent<RectTransform>();
            upgTextRect.anchorMin = Vector2.zero;
            upgTextRect.anchorMax = Vector2.one;
            upgTextRect.offsetMin = Vector2.zero;
            upgTextRect.offsetMax = Vector2.zero;
            upgTextRect.anchoredPosition = Vector2.zero;
            Text upgText = upgTextObj.AddComponent<Text>();
            upgText.text = "⚡ UPGRADES";
            upgText.font = font;
            upgText.fontSize = 24;
            upgText.fontStyle = FontStyle.Bold;
            upgText.alignment = TextAnchor.MiddleCenter;
            upgText.color = Color.white;

            // 3. Settings Button (Middle-Right)
            GameObject setBtnObj = new GameObject("Btn_Settings", typeof(RectTransform));
            setBtnObj.transform.SetParent(marquee.transform, false);
            RectTransform setBtnRect = setBtnObj.GetComponent<RectTransform>();
            setBtnRect.anchorMin = new Vector2(0.565f, 0.18f);
            setBtnRect.anchorMax = new Vector2(0.715f, 0.84f);
            setBtnRect.offsetMin = Vector2.zero;
            setBtnRect.offsetMax = Vector2.zero;
            setBtnRect.anchoredPosition = Vector2.zero;

            Image setBtnImg = setBtnObj.AddComponent<Image>();
            setBtnImg.color = new Color(0.24f, 0.27f, 0.35f);
            setBtn = setBtnObj.AddComponent<Button>();

            GameObject setTextObj = new GameObject("Text", typeof(RectTransform));
            setTextObj.transform.SetParent(setBtnObj.transform, false);
            RectTransform setTextRect = setTextObj.GetComponent<RectTransform>();
            setTextRect.anchorMin = Vector2.zero;
            setTextRect.anchorMax = Vector2.one;
            setTextRect.offsetMin = Vector2.zero;
            setTextRect.offsetMax = Vector2.zero;
            setTextRect.anchoredPosition = Vector2.zero;
            Text setText = setTextObj.AddComponent<Text>();
            setText.text = "⚙";
            setText.font = font;
            setText.fontSize = 32;
            setText.fontStyle = FontStyle.Bold;
            setText.alignment = TextAnchor.MiddleCenter;
            setText.color = Color.white;

            // 4. Inset Coins Readout Display (Right)
            GameObject coinBadgeObj = new GameObject("Badge_Coins", typeof(RectTransform));
            coinBadgeObj.transform.SetParent(marquee.transform, false);
            RectTransform cbRect = coinBadgeObj.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.730f, 0.18f);
            cbRect.anchorMax = new Vector2(0.975f, 0.84f);
            cbRect.offsetMin = Vector2.zero;
            cbRect.offsetMax = Vector2.zero;
            cbRect.anchoredPosition = Vector2.zero;

            Image cbImg = coinBadgeObj.AddComponent<Image>();
            cbImg.color = new Color(0.06f, 0.07f, 0.10f, 0.95f); // Inset digital counter plate

            GameObject coinsObj = new GameObject("Text_Coins", typeof(RectTransform));
            coinsObj.transform.SetParent(coinBadgeObj.transform, false);
            RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
            coinsRect.anchorMin = Vector2.zero;
            coinsRect.anchorMax = Vector2.one;
            coinsRect.offsetMin = Vector2.zero;
            coinsRect.offsetMax = Vector2.zero;
            coinsRect.anchoredPosition = Vector2.zero;
            coinsText = coinsObj.AddComponent<Text>();
            coinsText.text = "100 🪙";
            coinsText.font = font;
            coinsText.fontSize = 28;
            coinsText.fontStyle = FontStyle.Bold;
            coinsText.alignment = TextAnchor.MiddleCenter;
            coinsText.color = new Color(1f, 0.84f, 0.15f);
        }

        private static void BuildCollectionUI(GameObject canvasObj, Button collBtn, Text coinsText)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Collection Modal Panel (Full Screen Overlay)

            // 3. Collection Modal Panel (Full Screen Overlay)
            GameObject modalObj = new GameObject("CollectionModal");
            modalObj.transform.parent = canvasObj.transform;
            RectTransform modalRect = modalObj.AddComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;
            Image modalBg = modalObj.AddComponent<Image>();
            modalBg.color = new Color(0.08f, 0.09f, 0.12f, 0.96f);

            // Modal Header Panel
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.parent = modalObj.transform;
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.85f);
            headerRect.anchorMax = new Vector2(0.95f, 0.96f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.parent = headerObj.transform;
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.55f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "TOY BOX";
            titleText.font = font;
            titleText.fontSize = 54;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            // Progress & Ownership
            GameObject progObj = new GameObject("Progress");
            progObj.transform.parent = headerObj.transform;
            RectTransform progRect = progObj.AddComponent<RectTransform>();
            progRect.anchorMin = new Vector2(0f, 0.25f);
            progRect.anchorMax = new Vector2(1f, 0.55f);
            progRect.offsetMin = Vector2.zero;
            progRect.offsetMax = Vector2.zero;
            Text progText = progObj.AddComponent<Text>();
            progText.text = "COLLECTION: 0 / 9 (0%)";
            progText.font = font;
            progText.fontSize = 32;
            progText.alignment = TextAnchor.MiddleCenter;
            progText.color = new Color(0.85f, 0.85f, 0.9f);

            GameObject ownerObj = new GameObject("Ownership");
            ownerObj.transform.parent = headerObj.transform;
            RectTransform ownerRect = ownerObj.AddComponent<RectTransform>();
            ownerRect.anchorMin = new Vector2(0f, 0f);
            ownerRect.anchorMax = new Vector2(1f, 0.25f);
            ownerRect.offsetMin = Vector2.zero;
            ownerRect.offsetMax = Vector2.zero;
            Text ownerText = ownerObj.AddComponent<Text>();
            ownerText.text = "UNOWNED (ENTRY: FREE)";
            ownerText.font = font;
            ownerText.fontSize = 26;
            ownerText.fontStyle = FontStyle.Bold;
            ownerText.alignment = TextAnchor.MiddleCenter;
            ownerText.color = new Color(0.3f, 0.9f, 0.4f);

            // 4. Grid Container (3x3)
            GameObject gridObj = new GameObject("GridContainer");
            gridObj.transform.parent = modalObj.transform;
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.06f, 0.16f);
            gridRect.anchorMax = new Vector2(0.94f, 0.84f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(285, 340);
            gridLayout.spacing = new Vector2(30, 30);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            // 9 Cards
            for (int i = 0; i < 9; i++)
            {
                GameObject card = new GameObject($"Card_{i}");
                card.transform.parent = gridObj.transform;
                RectTransform cardRect = card.AddComponent<RectTransform>();
                Image cardBg = card.AddComponent<Image>();
                cardBg.color = new Color(0.14f, 0.16f, 0.22f);

                // Card Name
                GameObject cName = new GameObject("Name");
                cName.transform.parent = card.transform;
                RectTransform cnRect = cName.AddComponent<RectTransform>();
                cnRect.anchorMin = new Vector2(0.05f, 0.55f);
                cnRect.anchorMax = new Vector2(0.95f, 0.95f);
                cnRect.offsetMin = Vector2.zero;
                cnRect.offsetMax = Vector2.zero;
                Text cnText = cName.AddComponent<Text>();
                cnText.text = "???";
                cnText.font = font;
                cnText.fontSize = 30;
                cnText.fontStyle = FontStyle.Bold;
                cnText.alignment = TextAnchor.MiddleCenter;
                cnText.color = Color.white;

                // Card Rarity
                GameObject cRarity = new GameObject("Rarity");
                cRarity.transform.parent = card.transform;
                RectTransform crRect = cRarity.AddComponent<RectTransform>();
                crRect.anchorMin = new Vector2(0.05f, 0.28f);
                crRect.anchorMax = new Vector2(0.95f, 0.55f);
                crRect.offsetMin = Vector2.zero;
                crRect.offsetMax = Vector2.zero;
                Text crText = cRarity.AddComponent<Text>();
                crText.text = "NORMAL";
                crText.font = font;
                crText.fontSize = 24;
                crText.fontStyle = FontStyle.Bold;
                crText.alignment = TextAnchor.MiddleCenter;
                crText.color = Color.gray;

                // Card Status
                GameObject cStatus = new GameObject("Status");
                cStatus.transform.parent = card.transform;
                RectTransform csRect = cStatus.AddComponent<RectTransform>();
                csRect.anchorMin = new Vector2(0.05f, 0.05f);
                csRect.anchorMax = new Vector2(0.95f, 0.28f);
                csRect.offsetMin = Vector2.zero;
                csRect.offsetMax = Vector2.zero;
                Text csText = cStatus.AddComponent<Text>();
                csText.text = "LOCKED";
                csText.font = font;
                csText.fontSize = 22;
                csText.alignment = TextAnchor.MiddleCenter;
                csText.color = new Color(0.5f, 0.5f, 0.5f);
            }

            // Close Button
            GameObject closeBtnObj = new GameObject("Btn_Close");
            closeBtnObj.transform.parent = modalObj.transform;
            RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.25f, 0.05f);
            closeRect.anchorMax = new Vector2(0.75f, 0.12f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            Image closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = new Color(0.85f, 0.25f, 0.25f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.parent = closeBtnObj.transform;
            RectTransform cltRect = closeTextObj.AddComponent<RectTransform>();
            cltRect.anchorMin = Vector2.zero;
            cltRect.anchorMax = Vector2.one;
            cltRect.offsetMin = Vector2.zero;
            cltRect.offsetMax = Vector2.zero;
            Text cltText = closeTextObj.AddComponent<Text>();
            cltText.text = "✕ CLOSE";
            cltText.font = font;
            cltText.fontSize = 44;
            cltText.fontStyle = FontStyle.Bold;
            cltText.alignment = TextAnchor.MiddleCenter;
            cltText.color = Color.white;

            // 5. Celebration Modal
            GameObject celebModal = new GameObject("CelebrationModal");
            celebModal.transform.parent = canvasObj.transform;
            RectTransform celebRect = celebModal.AddComponent<RectTransform>();
            celebRect.anchorMin = new Vector2(0.1f, 0.30f);
            celebRect.anchorMax = new Vector2(0.9f, 0.70f);
            celebRect.offsetMin = Vector2.zero;
            celebRect.offsetMax = Vector2.zero;
            Image celebBg = celebModal.AddComponent<Image>();
            celebBg.color = new Color(0.12f, 0.14f, 0.20f, 0.98f);

            GameObject celebTextObj = new GameObject("Text");
            celebTextObj.transform.parent = celebModal.transform;
            RectTransform ctxRect = celebTextObj.AddComponent<RectTransform>();
            ctxRect.anchorMin = new Vector2(0.05f, 0.25f);
            ctxRect.anchorMax = new Vector2(0.95f, 0.95f);
            ctxRect.offsetMin = Vector2.zero;
            ctxRect.offsetMax = Vector2.zero;
            Text celebText = celebTextObj.AddComponent<Text>();
            celebText.text = "★ CONGRATULATIONS! ★\n\nTOY BOX COMPLETED!";
            celebText.font = font;
            celebText.fontSize = 38;
            celebText.fontStyle = FontStyle.Bold;
            celebText.alignment = TextAnchor.MiddleCenter;
            celebText.color = new Color(1f, 0.85f, 0.2f);

            GameObject celebCloseObj = new GameObject("Btn_Awesome");
            celebCloseObj.transform.parent = celebModal.transform;
            RectTransform cclRect = celebCloseObj.AddComponent<RectTransform>();
            cclRect.anchorMin = new Vector2(0.25f, 0.06f);
            cclRect.anchorMax = new Vector2(0.75f, 0.22f);
            cclRect.offsetMin = Vector2.zero;
            cclRect.offsetMax = Vector2.zero;
            Image cclImg = celebCloseObj.AddComponent<Image>();
            cclImg.color = new Color(0.25f, 0.75f, 0.35f);
            Button cclBtn = celebCloseObj.AddComponent<Button>();

            GameObject cclTextObj = new GameObject("Text");
            cclTextObj.transform.parent = celebCloseObj.transform;
            RectTransform ccltRect = cclTextObj.AddComponent<RectTransform>();
            ccltRect.anchorMin = Vector2.zero;
            ccltRect.anchorMax = Vector2.one;
            ccltRect.offsetMin = Vector2.zero;
            ccltRect.offsetMax = Vector2.zero;
            Text ccltText = cclTextObj.AddComponent<Text>();
            ccltText.text = "AWESOME!";
            ccltText.font = font;
            ccltText.fontSize = 36;
            ccltText.fontStyle = FontStyle.Bold;
            ccltText.alignment = TextAnchor.MiddleCenter;
            ccltText.color = Color.white;

            // Hook up CollectionUI
            CollectionUI ui = canvasObj.AddComponent<CollectionUI>();
            SetSerializedProperty(ui, "modalPanel", modalObj);
            SetSerializedProperty(ui, "toggleButton", collBtn);
            SetSerializedProperty(ui, "closeButton", closeBtn);
            SetSerializedProperty(ui, "coinsText", coinsText);
            SetSerializedProperty(ui, "titleText", titleText);
            SetSerializedProperty(ui, "progressText", progText);
            SetSerializedProperty(ui, "ownershipText", ownerText);
            SetSerializedProperty(ui, "gridContainer", gridObj.transform);
            SetSerializedProperty(ui, "completionModal", celebModal);
            SetSerializedProperty(ui, "completionText", celebText);
            SetSerializedProperty(ui, "completionCloseButton", cclBtn);
        }

        private static void BuildUpgradesUI(GameObject canvasObj, Button toggleBtn, Text coinsText)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Modal Scrim
            GameObject modalObj = new GameObject("UpgradesModal", typeof(RectTransform));
            modalObj.transform.SetParent(canvasObj.transform, false);
            RectTransform modalRect = modalObj.GetComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;
            Image modalBg = modalObj.AddComponent<Image>();
            modalBg.color = new Color(0.08f, 0.09f, 0.13f, 0.97f);

            // Header Container
            GameObject headerObj = new GameObject("Header", typeof(RectTransform));
            headerObj.transform.SetParent(modalObj.transform, false);
            RectTransform hRect = headerObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.06f, 0.82f);
            hRect.anchorMax = new Vector2(0.94f, 0.96f);
            hRect.offsetMin = Vector2.zero;
            hRect.offsetMax = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.50f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "ARCADE WORKSHOP";
            titleText.font = font;
            titleText.fontSize = 50;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.88f, 0.30f);

            // Set Bonus / Gold Claw Perk Subtitle
            GameObject bonusObj = new GameObject("SetBonusText", typeof(RectTransform));
            bonusObj.transform.SetParent(headerObj.transform, false);
            RectTransform bRect = bonusObj.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 0f);
            bRect.anchorMax = new Vector2(1f, 0.50f);
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            Text bonusText = bonusObj.AddComponent<Text>();
            bonusText.text = "COLLECT ALL 9 PRIZES IN ANY CABINET FOR SET BONUS & GOLD CLAW";
            bonusText.font = font;
            bonusText.fontSize = 24;
            bonusText.fontStyle = FontStyle.Bold;
            bonusText.alignment = TextAnchor.MiddleCenter;
            bonusText.color = new Color(0.65f, 0.70f, 0.80f);

            // Rows Container (Vertical List)
            GameObject rowsContainer = new GameObject("RowsContainer", typeof(RectTransform));
            rowsContainer.transform.SetParent(modalObj.transform, false);
            RectTransform rcRect = rowsContainer.GetComponent<RectTransform>();
            rcRect.anchorMin = new Vector2(0.06f, 0.20f);
            rcRect.anchorMax = new Vector2(0.94f, 0.80f);
            rcRect.offsetMin = Vector2.zero;
            rcRect.offsetMax = Vector2.zero;

            // Row 1: Trolley Speed
            CreateUpgradeRow(rowsContainer.transform, font, 0, "TROLLEY SPEED", "Faster horizontal claw carriage aiming",
                out Text tLvl, out Text tCost, out Button tBuy);

            // Row 2: Grip Power
            CreateUpgradeRow(rowsContainer.transform, font, 1, "GRIP POWER", "Stronger grip grasp & reduced toy slip",
                out Text gLvl, out Text gCost, out Button gBuy);

            // Row 3: Drop Precision
            CreateUpgradeRow(rowsContainer.transform, font, 2, "DROP PRECISION", "Faster cable plunge and hoist lift",
                out Text dLvl, out Text dCost, out Button dBuy);

            // Close Button
            GameObject closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform));
            closeBtnObj.transform.SetParent(modalObj.transform, false);
            RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.25f, 0.06f);
            closeRect.anchorMax = new Vector2(0.75f, 0.14f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            Image closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = new Color(0.85f, 0.25f, 0.25f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("Text", typeof(RectTransform));
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform cltRect = closeTextObj.GetComponent<RectTransform>();
            cltRect.anchorMin = Vector2.zero;
            cltRect.anchorMax = Vector2.one;
            cltRect.offsetMin = Vector2.zero;
            cltRect.offsetMax = Vector2.zero;
            Text cltText = closeTextObj.AddComponent<Text>();
            cltText.text = "✕ CLOSE";
            cltText.font = font;
            cltText.fontSize = 44;
            cltText.fontStyle = FontStyle.Bold;
            cltText.alignment = TextAnchor.MiddleCenter;
            cltText.color = Color.white;

            // Hook up UpgradesModalUI
            UpgradesModalUI upgUI = canvasObj.AddComponent<UpgradesModalUI>();
            SetSerializedProperty(upgUI, "modalPanel", modalObj);
            SetSerializedProperty(upgUI, "toggleButton", toggleBtn);
            SetSerializedProperty(upgUI, "closeButton", closeBtn);
            SetSerializedProperty(upgUI, "coinsText", coinsText);
            SetSerializedProperty(upgUI, "setBonusText", bonusText);

            SetSerializedProperty(upgUI, "trolleyLevelText", tLvl);
            SetSerializedProperty(upgUI, "trolleyCostText", tCost);
            SetSerializedProperty(upgUI, "trolleyBuyBtn", tBuy);

            SetSerializedProperty(upgUI, "gripLevelText", gLvl);
            SetSerializedProperty(upgUI, "gripCostText", gCost);
            SetSerializedProperty(upgUI, "gripBuyBtn", gBuy);

            SetSerializedProperty(upgUI, "dropLevelText", dLvl);
            SetSerializedProperty(upgUI, "dropCostText", dCost);
            SetSerializedProperty(upgUI, "dropBuyBtn", dBuy);

            modalObj.SetActive(false);
        }

        private static void CreateUpgradeRow(Transform parent, Font font, int index, string title, string subtitle,
            out Text lvlText, out Text costText, out Button buyBtn)
        {
            float rowHeight = 0.30f;
            float topY = 1.0f - index * 0.34f;
            float bottomY = topY - rowHeight;

            GameObject rowObj = new GameObject($"Row_{index}", typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            RectTransform rRect = rowObj.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0f, bottomY);
            rRect.anchorMax = new Vector2(1f, topY);
            rRect.offsetMin = Vector2.zero;
            rRect.offsetMax = Vector2.zero;
            Image rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.14f, 0.16f, 0.22f);

            // Left info (Title + Subtitle + Level)
            GameObject infoObj = new GameObject("Info", typeof(RectTransform));
            infoObj.transform.SetParent(rowObj.transform, false);
            RectTransform iRect = infoObj.GetComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0.04f, 0.08f);
            iRect.anchorMax = new Vector2(0.66f, 0.92f);
            iRect.offsetMin = Vector2.zero;
            iRect.offsetMax = Vector2.zero;

            GameObject tObj = new GameObject("Title", typeof(RectTransform));
            tObj.transform.SetParent(infoObj.transform, false);
            RectTransform tr = tObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0f, 0.62f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            Text tt = tObj.AddComponent<Text>();
            tt.text = title;
            tt.font = font;
            tt.fontSize = 32;
            tt.fontStyle = FontStyle.Bold;
            tt.alignment = TextAnchor.MiddleLeft;
            tt.color = Color.white;

            GameObject subObj = new GameObject("Subtitle", typeof(RectTransform));
            subObj.transform.SetParent(infoObj.transform, false);
            RectTransform sr = subObj.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0f, 0.32f);
            sr.anchorMax = new Vector2(1f, 0.62f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;
            Text st = subObj.AddComponent<Text>();
            st.text = subtitle;
            st.font = font;
            st.fontSize = 20;
            st.alignment = TextAnchor.MiddleLeft;
            st.color = new Color(0.75f, 0.78f, 0.85f);

            GameObject lObj = new GameObject("Level", typeof(RectTransform));
            lObj.transform.SetParent(infoObj.transform, false);
            RectTransform lr = lObj.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(1f, 0.32f);
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;
            lvlText = lObj.AddComponent<Text>();
            lvlText.text = "LVL 1/5  ■ □ □ □ □";
            lvlText.font = font;
            lvlText.fontSize = 22;
            lvlText.fontStyle = FontStyle.Bold;
            lvlText.alignment = TextAnchor.MiddleLeft;
            lvlText.color = new Color(0.3f, 0.9f, 0.5f);

            // Right Buy Button
            GameObject btnObj = new GameObject("Btn_Buy", typeof(RectTransform));
            btnObj.transform.SetParent(rowObj.transform, false);
            RectTransform bRect = btnObj.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.68f, 0.18f);
            bRect.anchorMax = new Vector2(0.96f, 0.82f);
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.22f, 0.75f, 0.35f);
            buyBtn = btnObj.AddComponent<Button>();

            GameObject bTextObj = new GameObject("CostText", typeof(RectTransform));
            bTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btr = bTextObj.GetComponent<RectTransform>();
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            btr.offsetMin = Vector2.zero;
            btr.offsetMax = Vector2.zero;
            costText = bTextObj.AddComponent<Text>();
            costText.text = "45 🪙";
            costText.font = font;
            costText.fontSize = 30;
            costText.fontStyle = FontStyle.Bold;
            costText.alignment = TextAnchor.MiddleCenter;
            costText.color = Color.white;
        }

        private static void BuildSettingsUI(GameObject canvasObj, Button toggleBtn)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Modal Scrim
            GameObject modalObj = new GameObject("SettingsModal", typeof(RectTransform));
            modalObj.transform.SetParent(canvasObj.transform, false);
            RectTransform modalRect = modalObj.GetComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;
            Image modalBg = modalObj.AddComponent<Image>();
            modalBg.color = new Color(0.08f, 0.09f, 0.13f, 0.97f);

            // Header Container
            GameObject headerObj = new GameObject("Header", typeof(RectTransform));
            headerObj.transform.SetParent(modalObj.transform, false);
            RectTransform hRect = headerObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.06f, 0.82f);
            hRect.anchorMax = new Vector2(0.94f, 0.96f);
            hRect.offsetMin = Vector2.zero;
            hRect.offsetMax = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "SETTINGS";
            titleText.font = font;
            titleText.fontSize = 50;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            // Container for Settings Rows
            GameObject listContainer = new GameObject("ListContainer", typeof(RectTransform));
            listContainer.transform.SetParent(modalObj.transform, false);
            RectTransform lcRect = listContainer.GetComponent<RectTransform>();
            lcRect.anchorMin = new Vector2(0.08f, 0.26f);
            lcRect.anchorMax = new Vector2(0.92f, 0.80f);
            lcRect.offsetMin = Vector2.zero;
            lcRect.offsetMax = Vector2.zero;

            // Row 1: Audio SFX Toggle
            CreateSettingsRow(listContainer.transform, font, 0, "SOUND EFFECTS", "SFX: ON", out Button sfxBtn, out Text sfxText);

            // Row 2: Vibration Toggle
            CreateSettingsRow(listContainer.transform, font, 1, "HAPTIC VIBRATION", "VIBRATION: ON", out Button hapBtn, out Text hapText);

            // Row 3: Reset Progress
            CreateSettingsRow(listContainer.transform, font, 2, "DATA MANAGEMENT", "RESET PROGRESS", out Button rstBtn, out Text rstText);
            rstBtn.GetComponent<Image>().color = new Color(0.55f, 0.20f, 0.20f);

            // Close Button
            GameObject closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform));
            closeBtnObj.transform.SetParent(modalObj.transform, false);
            RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.25f, 0.08f);
            closeRect.anchorMax = new Vector2(0.75f, 0.16f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            Image closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = new Color(0.85f, 0.25f, 0.25f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();

            GameObject closeTextObj = new GameObject("Text", typeof(RectTransform));
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform cltRect = closeTextObj.GetComponent<RectTransform>();
            cltRect.anchorMin = Vector2.zero;
            cltRect.anchorMax = Vector2.one;
            cltRect.offsetMin = Vector2.zero;
            cltRect.offsetMax = Vector2.zero;
            Text cltText = closeTextObj.AddComponent<Text>();
            cltText.text = "✕ CLOSE";
            cltText.font = font;
            cltText.fontSize = 44;
            cltText.fontStyle = FontStyle.Bold;
            cltText.alignment = TextAnchor.MiddleCenter;
            cltText.color = Color.white;

            // Hook up SettingsModalUI
            SettingsModalUI setUI = canvasObj.AddComponent<SettingsModalUI>();
            SetSerializedProperty(setUI, "modalPanel", modalObj);
            SetSerializedProperty(setUI, "toggleButton", toggleBtn);
            SetSerializedProperty(setUI, "closeButton", closeBtn);
            SetSerializedProperty(setUI, "sfxToggleButton", sfxBtn);
            SetSerializedProperty(setUI, "sfxStatusText", sfxText);
            SetSerializedProperty(setUI, "hapticsToggleButton", hapBtn);
            SetSerializedProperty(setUI, "hapticsStatusText", hapText);
            SetSerializedProperty(setUI, "resetButton", rstBtn);
            SetSerializedProperty(setUI, "resetText", rstText);

            modalObj.SetActive(false);
        }

        private static void CreateSettingsRow(Transform parent, Font font, int index, string title, string defaultButtonText,
            out Button actionBtn, out Text actionText)
        {
            float rowHeight = 0.28f;
            float topY = 1.0f - index * 0.35f;
            float bottomY = topY - rowHeight;

            GameObject rowObj = new GameObject($"Row_{index}", typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            RectTransform rRect = rowObj.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0f, bottomY);
            rRect.anchorMax = new Vector2(1f, topY);
            rRect.offsetMin = Vector2.zero;
            rRect.offsetMax = Vector2.zero;
            Image rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.14f, 0.16f, 0.22f);

            // Title
            GameObject tObj = new GameObject("Title", typeof(RectTransform));
            tObj.transform.SetParent(rowObj.transform, false);
            RectTransform tr = tObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.05f, 0f);
            tr.anchorMax = new Vector2(0.55f, 1f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            Text tt = tObj.AddComponent<Text>();
            tt.text = title;
            tt.font = font;
            tt.fontSize = 28;
            tt.fontStyle = FontStyle.Bold;
            tt.alignment = TextAnchor.MiddleLeft;
            tt.color = Color.white;

            // Action Button
            GameObject btnObj = new GameObject("Btn_Action", typeof(RectTransform));
            btnObj.transform.SetParent(rowObj.transform, false);
            RectTransform bRect = btnObj.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.58f, 0.15f);
            bRect.anchorMax = new Vector2(0.95f, 0.85f);
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.20f, 0.24f, 0.34f);
            actionBtn = btnObj.AddComponent<Button>();

            GameObject bTextObj = new GameObject("Text", typeof(RectTransform));
            bTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btr = bTextObj.GetComponent<RectTransform>();
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            btr.offsetMin = Vector2.zero;
            btr.offsetMax = Vector2.zero;
            actionText = bTextObj.AddComponent<Text>();
            actionText.text = defaultButtonText;
            actionText.font = font;
            actionText.fontSize = 24;
            actionText.fontStyle = FontStyle.Bold;
            actionText.alignment = TextAnchor.MiddleCenter;
            actionText.color = Color.white;
        }

        private static void SetSerializedProperty(Object target, string propertyName, object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"Property {propertyName} not found on {target.name}");
                return;
            }

            if (value is System.Array sysArray)
            {
                prop.arraySize = sysArray.Length;
                for (int i = 0; i < sysArray.Length; i++)
                {
                    object elem = sysArray.GetValue(i);
                    if (elem is Object uElem)
                    {
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = uElem;
                    }
                }
            }
            else if (value is Object unityObj)
            {
                prop.objectReferenceValue = unityObj;
            }
            else if (value is int intVal)
            {
                prop.intValue = intVal;
            }
            else if (value is float floatVal)
            {
                prop.floatValue = floatVal;
            }
            else if (value is Vector3 v3)
            {
                prop.vector3Value = v3;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
