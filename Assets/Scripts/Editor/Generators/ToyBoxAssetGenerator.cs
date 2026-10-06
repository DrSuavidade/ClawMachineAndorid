#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.Editor
{
    public static class ToyBoxAssetGenerator
    {
        private const string PREFAB_DIR = "Assets/Prefabs/Prizes/ToyBox";
        private const string DATA_DIR = "Assets/Data/Prizes/ToyBox";
        private const string MACHINE_DATA_PATH = "Assets/Data/Machine_ToyBox.asset";

        [MenuItem("ClawMachine/Generate Toy Box Collection (9 Prizes)")]
        public static MachineDefinition GenerateToyBox()
        {
            EnsureDirectories();

            PhysicsMaterial toyPhysMat = new PhysicsMaterial("ToyBoxPhysics")
            {
                dynamicFriction = 0.70f,
                staticFriction = 0.80f,
                bounciness = 0.15f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            // Materials
            Material yellowMat = CreateLitMaterial("Mat_DuckYellow", new Color(1.0f, 0.82f, 0.10f));
            Material orangeMat = CreateLitMaterial("Mat_DuckOrange", new Color(1.0f, 0.45f, 0.05f));
            Material brownMat = CreateLitMaterial("Mat_TeddyBrown", new Color(0.55f, 0.35f, 0.20f));
            Material beigeMat = CreateLitMaterial("Mat_TeddyBeige", new Color(0.85f, 0.72f, 0.58f));
            Material carBlueMat = CreateLitMaterial("Mat_CarBlue", new Color(0.20f, 0.55f, 0.95f));
            Material carCyanMat = CreateLitMaterial("Mat_CarCabin", new Color(0.50f, 0.85f, 0.95f));
            Material wheelMat = CreateLitMaterial("Mat_WheelBlack", new Color(0.15f, 0.15f, 0.18f));
            Material limeMat = CreateLitMaterial("Mat_BallLime", new Color(0.40f, 0.90f, 0.35f));
            Material lilacMat = CreateLitMaterial("Mat_StarLilac", new Color(0.75f, 0.60f, 0.95f));
            Material silverMat = CreateMetallicMaterial("Mat_RobotSilver", new Color(0.80f, 0.82f, 0.85f), 0.75f, 0.65f);
            Material tealMat = CreateLitMaterial("Mat_TealGlow", new Color(0.10f, 0.90f, 0.85f));
            Material whiteMat = CreateLitMaterial("Mat_White", new Color(0.96f, 0.96f, 0.98f));
            Material pinkMat = CreateLitMaterial("Mat_Pink", new Color(0.98f, 0.40f, 0.75f));
            Material goldMat = CreateMetallicMaterial("Mat_GoldLustre", new Color(1.0f, 0.84f, 0.15f), 0.90f, 0.85f);
            Material rubyMat = CreateLitMaterial("Mat_RubyCrown", new Color(0.92f, 0.15f, 0.25f));

            PrizeDefinition[] definitions = new PrizeDefinition[9];

            // 1. Rubber Duck (Normal)
            definitions[0] = BuildDuck("toy_rubber_duck", "Rubber Duck", PrizeRarity.Normal, yellowMat, orangeMat, 0.45f, 15, toyPhysMat);

            // 2. Teddy Bear (Normal)
            definitions[1] = BuildTeddy("toy_teddy_bear", "Teddy Bear", PrizeRarity.Normal, brownMat, beigeMat, 0.50f, 15, toyPhysMat);

            // 3. Toy Car (Normal)
            definitions[2] = BuildCar("toy_car", "Toy Car", PrizeRarity.Normal, carBlueMat, carCyanMat, wheelMat, 0.55f, 20, toyPhysMat);

            // 4. Bouncy Ball (Normal)
            definitions[3] = BuildBall("toy_bouncy_ball", "Bouncy Ball", PrizeRarity.Normal, limeMat, yellowMat, 0.42f, 15, toyPhysMat);

            // 5. Star Pillow (Normal)
            definitions[4] = BuildStarPillow("toy_star_pillow", "Star Pillow", PrizeRarity.Normal, lilacMat, 0.38f, 15, toyPhysMat);

            // 6. Robot (Rare)
            definitions[5] = BuildRobot("toy_robot", "Retro Robot", PrizeRarity.Rare, silverMat, tealMat, rubyMat, 0.65f, 40, toyPhysMat);

            // 7. Unicorn (Rare)
            definitions[6] = BuildUnicorn("toy_unicorn", "Unicorn", PrizeRarity.Rare, whiteMat, pinkMat, goldMat, 0.52f, 40, toyPhysMat);

            // 8. Astronaut (Rare)
            definitions[7] = BuildAstronaut("toy_astronaut", "Astronaut", PrizeRarity.Rare, whiteMat, goldMat, carCyanMat, 0.58f, 45, toyPhysMat);

            // 9. Golden Duck (Secret)
            definitions[8] = BuildGoldenDuck("toy_golden_duck", "Golden Duck", PrizeRarity.Secret, goldMat, rubyMat, 0.70f, 100, toyPhysMat);

            // Machine Definition
            MachineDefinition machine = AssetDatabase.LoadAssetAtPath<MachineDefinition>(MACHINE_DATA_PATH);
            if (machine == null)
            {
                machine = ScriptableObject.CreateInstance<MachineDefinition>();
                AssetDatabase.CreateAsset(machine, MACHINE_DATA_PATH);
            }
            machine.machineId = "toy_box";
            machine.displayName = "Toy Box";
            machine.entryCost = 0;
            machine.passiveIncomePerMinute = 5;
            machine.prizes = definitions;

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ToyBoxAssetGenerator] Generated 9 Toy Box prizes & MachineDefinition successfully!");
            return machine;
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(DATA_DIR)) Directory.CreateDirectory(DATA_DIR);
            if (!Directory.Exists("Assets/Materials/ToyBox")) Directory.CreateDirectory("Assets/Materials/ToyBox");
        }

        private static Material CreateLitMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/ToyBox/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.color = color;
            }
            return mat;
        }

        private static Material CreateMetallicMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = $"Assets/Materials/ToyBox/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static PrizeDefinition BuildDuck(string id, string name, PrizeRarity rarity, Material bodyMat, Material billMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.parent = root.transform;
            body.transform.localScale = new Vector3(0.40f, 0.32f, 0.44f);
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            body.GetComponent<Collider>().material = physMat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.parent = root.transform;
            head.transform.localPosition = new Vector3(0f, 0.18f, 0.12f);
            head.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
            head.GetComponent<Renderer>().sharedMaterial = bodyMat;
            head.GetComponent<Collider>().material = physMat;

            GameObject bill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bill.name = "Bill";
            bill.transform.parent = root.transform;
            bill.transform.localPosition = new Vector3(0f, 0.16f, 0.26f);
            bill.transform.localScale = new Vector3(0.12f, 0.05f, 0.12f);
            bill.GetComponent<Renderer>().sharedMaterial = billMat;
            bill.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildGoldenDuck(string id, string name, PrizeRarity rarity, Material goldMat, Material gemMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.parent = root.transform;
            body.transform.localScale = new Vector3(0.40f, 0.32f, 0.44f);
            body.GetComponent<Renderer>().sharedMaterial = goldMat;
            body.GetComponent<Collider>().material = physMat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.parent = root.transform;
            head.transform.localPosition = new Vector3(0f, 0.18f, 0.12f);
            head.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
            head.GetComponent<Renderer>().sharedMaterial = goldMat;
            head.GetComponent<Collider>().material = physMat;

            GameObject bill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bill.name = "Bill";
            bill.transform.parent = root.transform;
            bill.transform.localPosition = new Vector3(0f, 0.16f, 0.26f);
            bill.transform.localScale = new Vector3(0.12f, 0.05f, 0.12f);
            bill.GetComponent<Renderer>().sharedMaterial = goldMat;
            bill.GetComponent<Collider>().material = physMat;

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.name = "Crown";
            crown.transform.parent = root.transform;
            crown.transform.localPosition = new Vector3(0f, 0.32f, 0.12f);
            crown.transform.localScale = new Vector3(0.10f, 0.06f, 0.10f);
            crown.GetComponent<Renderer>().sharedMaterial = gemMat;
            crown.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildTeddy(string id, string name, PrizeRarity rarity, Material brownMat, Material beigeMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.parent = root.transform;
            body.transform.localScale = new Vector3(0.36f, 0.40f, 0.32f);
            body.GetComponent<Renderer>().sharedMaterial = brownMat;
            body.GetComponent<Collider>().material = physMat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.parent = root.transform;
            head.transform.localPosition = new Vector3(0f, 0.26f, 0f);
            head.transform.localScale = new Vector3(0.30f, 0.28f, 0.28f);
            head.GetComponent<Renderer>().sharedMaterial = brownMat;
            head.GetComponent<Collider>().material = physMat;

            GameObject lEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lEar.name = "LeftEar";
            lEar.transform.parent = root.transform;
            lEar.transform.localPosition = new Vector3(-0.12f, 0.38f, 0f);
            lEar.transform.localScale = Vector3.one * 0.10f;
            lEar.GetComponent<Renderer>().sharedMaterial = brownMat;
            lEar.GetComponent<Collider>().material = physMat;

            GameObject rEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rEar.name = "RightEar";
            rEar.transform.parent = root.transform;
            rEar.transform.localPosition = new Vector3(0.12f, 0.38f, 0f);
            rEar.transform.localScale = Vector3.one * 0.10f;
            rEar.GetComponent<Renderer>().sharedMaterial = brownMat;
            rEar.GetComponent<Collider>().material = physMat;

            GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snout.name = "Snout";
            snout.transform.parent = root.transform;
            snout.transform.localPosition = new Vector3(0f, 0.24f, 0.14f);
            snout.transform.localScale = new Vector3(0.14f, 0.10f, 0.10f);
            snout.GetComponent<Renderer>().sharedMaterial = beigeMat;
            snout.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildCar(string id, string name, PrizeRarity rarity, Material bodyMat, Material cabinMat, Material wheelMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "Chassis";
            chassis.transform.parent = root.transform;
            chassis.transform.localScale = new Vector3(0.32f, 0.14f, 0.52f);
            chassis.GetComponent<Renderer>().sharedMaterial = bodyMat;
            chassis.GetComponent<Collider>().material = physMat;

            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.parent = root.transform;
            cabin.transform.localPosition = new Vector3(0f, 0.12f, -0.04f);
            cabin.transform.localScale = new Vector3(0.24f, 0.14f, 0.26f);
            cabin.GetComponent<Renderer>().sharedMaterial = cabinMat;
            cabin.GetComponent<Collider>().material = physMat;

            Vector3[] wheelOffsets = {
                new Vector3(-0.18f, -0.05f, 0.16f), new Vector3(0.18f, -0.05f, 0.16f),
                new Vector3(-0.18f, -0.05f, -0.16f), new Vector3(0.18f, -0.05f, -0.16f)
            };
            for (int i = 0; i < 4; i++)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                w.name = $"Wheel_{i}";
                w.transform.parent = root.transform;
                w.transform.localPosition = wheelOffsets[i];
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                w.transform.localScale = new Vector3(0.14f, 0.04f, 0.14f);
                w.GetComponent<Renderer>().sharedMaterial = wheelMat;
                w.GetComponent<Collider>().material = physMat;
            }

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildBall(string id, string name, PrizeRarity rarity, Material ballMat, Material ringMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.parent = root.transform;
            ball.transform.localScale = Vector3.one * 0.44f;
            ball.GetComponent<Renderer>().sharedMaterial = ballMat;
            ball.GetComponent<Collider>().material = physMat;

            GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = "EquatorBand";
            band.transform.parent = root.transform;
            band.transform.localScale = new Vector3(0.45f, 0.05f, 0.45f);
            band.GetComponent<Renderer>().sharedMaterial = ringMat;
            band.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildStarPillow(string id, string name, PrizeRarity rarity, Material mat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cube);
            center.name = "PillowCenter";
            center.transform.parent = root.transform;
            center.transform.localScale = new Vector3(0.34f, 0.18f, 0.34f);
            center.GetComponent<Renderer>().sharedMaterial = mat;
            center.GetComponent<Collider>().material = physMat;

            for (int i = 0; i < 4; i++)
            {
                GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                puff.name = $"Puff_{i}";
                puff.transform.parent = root.transform;
                float angle = i * 90f * Mathf.Deg2Rad;
                puff.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.18f, 0f, Mathf.Sin(angle) * 0.18f);
                puff.transform.localScale = new Vector3(0.18f, 0.16f, 0.18f);
                puff.GetComponent<Renderer>().sharedMaterial = mat;
                puff.GetComponent<Collider>().material = physMat;
            }

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildRobot(string id, string name, PrizeRarity rarity, Material silverMat, Material tealMat, Material redMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.parent = root.transform;
            torso.transform.localScale = new Vector3(0.30f, 0.32f, 0.22f);
            torso.GetComponent<Renderer>().sharedMaterial = silverMat;
            torso.GetComponent<Collider>().material = physMat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.parent = root.transform;
            head.transform.localPosition = new Vector3(0f, 0.26f, 0f);
            head.transform.localScale = new Vector3(0.24f, 0.22f, 0.22f);
            head.GetComponent<Renderer>().sharedMaterial = silverMat;
            head.GetComponent<Collider>().material = physMat;

            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor";
            visor.transform.parent = root.transform;
            visor.transform.localPosition = new Vector3(0f, 0.26f, 0.12f);
            visor.transform.localScale = new Vector3(0.18f, 0.08f, 0.04f);
            visor.GetComponent<Renderer>().sharedMaterial = tealMat;
            visor.GetComponent<Collider>().material = physMat;

            GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            antenna.name = "Antenna";
            antenna.transform.parent = root.transform;
            antenna.transform.localPosition = new Vector3(0f, 0.40f, 0f);
            antenna.transform.localScale = Vector3.one * 0.08f;
            antenna.GetComponent<Renderer>().sharedMaterial = redMat;
            antenna.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildUnicorn(string id, string name, PrizeRarity rarity, Material whiteMat, Material pinkMat, Material goldMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.parent = root.transform;
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(0.26f, 0.24f, 0.26f);
            body.GetComponent<Renderer>().sharedMaterial = whiteMat;
            body.GetComponent<Collider>().material = physMat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.parent = root.transform;
            head.transform.localPosition = new Vector3(0f, 0.22f, 0.16f);
            head.transform.localScale = new Vector3(0.20f, 0.24f, 0.26f);
            head.GetComponent<Renderer>().sharedMaterial = whiteMat;
            head.GetComponent<Collider>().material = physMat;

            GameObject horn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            horn.name = "Horn";
            horn.transform.parent = root.transform;
            horn.transform.localPosition = new Vector3(0f, 0.36f, 0.22f);
            horn.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            horn.transform.localScale = new Vector3(0.04f, 0.12f, 0.04f);
            horn.GetComponent<Renderer>().sharedMaterial = goldMat;
            horn.GetComponent<Collider>().material = physMat;

            GameObject mane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mane.name = "Mane";
            mane.transform.parent = root.transform;
            mane.transform.localPosition = new Vector3(0f, 0.24f, 0.02f);
            mane.transform.localScale = new Vector3(0.08f, 0.20f, 0.14f);
            mane.GetComponent<Renderer>().sharedMaterial = pinkMat;
            mane.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition BuildAstronaut(string id, string name, PrizeRarity rarity, Material suitMat, Material visorMat, Material packMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject suit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            suit.name = "Suit";
            suit.transform.parent = root.transform;
            suit.transform.localScale = new Vector3(0.32f, 0.24f, 0.28f);
            suit.GetComponent<Renderer>().sharedMaterial = suitMat;
            suit.GetComponent<Collider>().material = physMat;

            GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helmet.name = "Helmet";
            helmet.transform.parent = root.transform;
            helmet.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            helmet.transform.localScale = Vector3.one * 0.30f;
            helmet.GetComponent<Renderer>().sharedMaterial = suitMat;
            helmet.GetComponent<Collider>().material = physMat;

            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visor.name = "Visor";
            visor.transform.parent = root.transform;
            visor.transform.localPosition = new Vector3(0f, 0.22f, 0.10f);
            visor.transform.localScale = new Vector3(0.20f, 0.16f, 0.16f);
            visor.GetComponent<Renderer>().sharedMaterial = visorMat;
            visor.GetComponent<Collider>().material = physMat;

            GameObject pack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pack.name = "Backpack";
            pack.transform.parent = root.transform;
            pack.transform.localPosition = new Vector3(0f, 0.04f, -0.16f);
            pack.transform.localScale = new Vector3(0.20f, 0.26f, 0.10f);
            pack.GetComponent<Renderer>().sharedMaterial = packMat;
            pack.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        private static PrizeDefinition FinalizePrefab(GameObject root, string id, string name, PrizeRarity rarity, float mass, int coinVal)
        {
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Prize prizeComp = root.AddComponent<Prize>();

            string prefabPath = $"{PREFAB_DIR}/{id}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            GameObject.DestroyImmediate(root);

            // PrizeDefinition
            string defPath = $"{DATA_DIR}/{id}.asset";
            PrizeDefinition def = AssetDatabase.LoadAssetAtPath<PrizeDefinition>(defPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<PrizeDefinition>();
                AssetDatabase.CreateAsset(def, defPath);
            }
            def.id = id;
            def.displayName = name;
            def.rarity = rarity;
            def.prefab = prefab;
            def.mass = mass;
            def.duplicateCoinValue = coinVal;
            EditorUtility.SetDirty(def);

            return def;
        }
    }
}
#endif
