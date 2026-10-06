#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.Editor
{
    public static class RetroArcadeAssetGenerator
    {
        private const string PREFAB_DIR = "Assets/Prefabs/Prizes/RetroArcade";
        private const string DATA_DIR = "Assets/Data/Prizes/RetroArcade";
        private const string MACHINE_DATA_PATH = "Assets/Data/Machine_RetroArcade.asset";

        [MenuItem("ClawMachine/Generate Retro Arcade Collection (9 Prizes)")]
        public static MachineDefinition GenerateRetroArcade()
        {
            EnsureDirectories();

            PhysicsMaterial physMat = new PhysicsMaterial("RetroArcadePhysics")
            {
                dynamicFriction = 0.70f,
                staticFriction = 0.80f,
                bounciness = 0.12f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            // Palette
            Material cartGrayMat = CreateLitMaterial("Mat_CartGray", new Color(0.28f, 0.28f, 0.32f));
            Material cartLabelMat = CreateLitMaterial("Mat_CartLabel", new Color(0.92f, 0.25f, 0.25f));
            Material cyanNeonMat = CreateLitMaterial("Mat_CyanNeon", new Color(0.12f, 0.85f, 0.95f));
            Material magentaNeonMat = CreateLitMaterial("Mat_MagentaNeon", new Color(0.95f, 0.15f, 0.75f));
            Material swordBlueMat = CreateLitMaterial("Mat_SwordBlade", new Color(0.25f, 0.65f, 0.95f));
            Material swordHiltMat = CreateLitMaterial("Mat_SwordHilt", new Color(0.45f, 0.30f, 0.15f));
            Material floppyBlackMat = CreateLitMaterial("Mat_FloppyBlack", new Color(0.16f, 0.16f, 0.18f));
            Material floppyMetalMat = CreateMetallicMaterial("Mat_FloppyShutter", new Color(0.85f, 0.85f, 0.88f), 0.85f, 0.7f);
            Material crtBeigeMat = CreateLitMaterial("Mat_CRTBeige", new Color(0.82f, 0.80f, 0.74f));
            Material crtScreenMat = CreateLitMaterial("Mat_CRTScreen", new Color(0.10f, 0.28f, 0.20f));
            Material goldCoinMat = CreateMetallicMaterial("Mat_ArcadeTokenGold", new Color(1.0f, 0.82f, 0.15f), 0.90f, 0.85f);
            Material darkPlasticMat = CreateLitMaterial("Mat_DarkPlastic", new Color(0.18f, 0.18f, 0.22f));
            Material keyWhiteMat = CreateLitMaterial("Mat_KeyWhite", new Color(0.95f, 0.95f, 0.95f));

            PrizeDefinition[] definitions = new PrizeDefinition[9];

            // 1. Pixel Game Cartridge (Normal) - Scaled tier duplicate: 30 coins
            definitions[0] = BuildCartridge("retro_cartridge", "Pixel Cartridge", PrizeRarity.Normal, cartGrayMat, cartLabelMat, 0.45f, 30, physMat);

            // 2. 8-Bit Pixel Sword (Normal) - Scaled tier duplicate: 30 coins
            definitions[1] = BuildSword("retro_sword", "8-Bit Sword", PrizeRarity.Normal, swordBlueMat, swordHiltMat, 0.48f, 30, physMat);

            // 3. 3.5\" Floppy Disk (Normal) - Scaled tier duplicate: 30 coins
            definitions[2] = BuildFloppy("retro_floppy", "Floppy Disk", PrizeRarity.Normal, floppyBlackMat, floppyMetalMat, cartLabelMat, 0.40f, 30, physMat);

            // 4. Arcade Token (Normal) - Scaled tier duplicate: 35 coins
            definitions[3] = BuildToken("retro_token", "Arcade Token", PrizeRarity.Normal, goldCoinMat, 0.50f, 35, physMat);

            // 5. Handheld Console (Normal) - Scaled tier duplicate: 35 coins
            definitions[4] = BuildHandheld("retro_handheld", "Handheld Console", PrizeRarity.Normal, crtBeigeMat, crtScreenMat, magentaNeonMat, 0.52f, 35, physMat);

            // 6. Vintage CRT Monitor (Rare) - Scaled tier duplicate: 80 coins
            definitions[5] = BuildCRT("retro_crt", "Retro CRT Monitor", PrizeRarity.Rare, crtBeigeMat, crtScreenMat, 0.65f, 80, physMat);

            // 7. Cyber VR Headset (Rare) - Scaled tier duplicate: 85 coins
            definitions[6] = BuildVR("retro_vr", "Cyber VR Visor", PrizeRarity.Rare, darkPlasticMat, cyanNeonMat, magentaNeonMat, 0.58f, 85, physMat);

            // 8. Synth Keytar (Rare) - Scaled tier duplicate: 90 coins
            definitions[7] = BuildKeytar("retro_keytar", "80s Keytar", PrizeRarity.Rare, magentaNeonMat, keyWhiteMat, darkPlasticMat, 0.60f, 90, physMat);

            // 9. Golden Mini Arcade Cabinet (Secret) - Scaled tier duplicate: 250 coins
            definitions[8] = BuildGoldenCabinet("retro_golden_cabinet", "Golden Arcade Cabinet", PrizeRarity.Secret, goldCoinMat, cyanNeonMat, 0.75f, 250, physMat);

            // Machine Definition Asset
            MachineDefinition machine = AssetDatabase.LoadAssetAtPath<MachineDefinition>(MACHINE_DATA_PATH);
            if (machine == null)
            {
                machine = ScriptableObject.CreateInstance<MachineDefinition>();
                AssetDatabase.CreateAsset(machine, MACHINE_DATA_PATH);
            }
            machine.machineId = "retro_arcade";
            machine.displayName = "Retro Arcade";
            machine.description = "Step into neon nostalgia with pixel relics, vintage tech, and rare arcade collectibles.";
            machine.unlockCost = 150;
            machine.entryCost = 10;
            machine.passiveIncomePerMinute = 15; // Scaled passive income for tier 2
            machine.cabinetFrameColor = new Color(0.78f, 0.15f, 0.88f); // Neon Magenta Frame
            machine.cabinetBackdropColor = new Color(0.10f, 0.12f, 0.22f); // Cyber Navy Wall
            machine.prizes = definitions;

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[RetroArcadeAssetGenerator] Generated 9 Retro Arcade prizes & MachineDefinition successfully!");
            return machine;
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(DATA_DIR)) Directory.CreateDirectory(DATA_DIR);
            if (!Directory.Exists("Assets/Materials/RetroArcade")) Directory.CreateDirectory("Assets/Materials/RetroArcade");
        }

        private static Material CreateLitMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/RetroArcade/{name}.mat";
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
            string path = $"Assets/Materials/RetroArcade/{name}.mat";
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

        // 1. Cartridge
        private static PrizeDefinition BuildCartridge(string id, string name, PrizeRarity rarity, Material bodyMat, Material labelMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.parent = root.transform;
            body.transform.localScale = new Vector3(0.38f, 0.44f, 0.14f);
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            body.GetComponent<Collider>().material = physMat;

            GameObject label = GameObject.CreatePrimitive(PrimitiveType.Cube);
            label.name = "Label";
            label.transform.parent = root.transform;
            label.transform.localPosition = new Vector3(0f, -0.04f, 0.075f);
            label.transform.localScale = new Vector3(0.28f, 0.28f, 0.02f);
            label.GetComponent<Renderer>().sharedMaterial = labelMat;
            label.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 2. Sword
        private static PrizeDefinition BuildSword(string id, string name, PrizeRarity rarity, Material bladeMat, Material hiltMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            // Blade
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.parent = root.transform;
            blade.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            blade.transform.localScale = new Vector3(0.12f, 0.55f, 0.06f);
            blade.GetComponent<Renderer>().sharedMaterial = bladeMat;
            blade.GetComponent<Collider>().material = physMat;

            // Crossguard
            GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.name = "Crossguard";
            guard.transform.parent = root.transform;
            guard.transform.localPosition = new Vector3(0f, -0.14f, 0f);
            guard.transform.localScale = new Vector3(0.32f, 0.06f, 0.09f);
            guard.GetComponent<Renderer>().sharedMaterial = hiltMat;
            guard.GetComponent<Collider>().material = physMat;

            // Hilt
            GameObject hilt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hilt.name = "Hilt";
            hilt.transform.parent = root.transform;
            hilt.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            hilt.transform.localScale = new Vector3(0.08f, 0.10f, 0.08f);
            hilt.GetComponent<Renderer>().sharedMaterial = hiltMat;
            hilt.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 3. Floppy
        private static PrizeDefinition BuildFloppy(string id, string name, PrizeRarity rarity, Material bodyMat, Material shutterMat, Material labelMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            disk.name = "DiskBody";
            disk.transform.parent = root.transform;
            disk.transform.localScale = new Vector3(0.40f, 0.40f, 0.08f);
            disk.GetComponent<Renderer>().sharedMaterial = bodyMat;
            disk.GetComponent<Collider>().material = physMat;

            GameObject shutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shutter.name = "MetalShutter";
            shutter.transform.parent = root.transform;
            shutter.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            shutter.transform.localScale = new Vector3(0.24f, 0.16f, 0.085f);
            shutter.GetComponent<Renderer>().sharedMaterial = shutterMat;
            shutter.GetComponent<Collider>().material = physMat;

            GameObject label = GameObject.CreatePrimitive(PrimitiveType.Cube);
            label.name = "PaperLabel";
            label.transform.parent = root.transform;
            label.transform.localPosition = new Vector3(0f, -0.08f, 0.042f);
            label.transform.localScale = new Vector3(0.32f, 0.20f, 0.01f);
            label.GetComponent<Renderer>().sharedMaterial = labelMat;
            label.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 4. Token
        private static PrizeDefinition BuildToken(string id, string name, PrizeRarity rarity, Material goldMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.parent = root.transform;
            coin.transform.localScale = new Vector3(0.44f, 0.06f, 0.44f);
            coin.GetComponent<Renderer>().sharedMaterial = goldMat;
            coin.GetComponent<Collider>().material = physMat;

            // Inset boss
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            boss.name = "Boss";
            boss.transform.parent = root.transform;
            boss.transform.localScale = new Vector3(0.30f, 0.07f, 0.30f);
            boss.GetComponent<Renderer>().sharedMaterial = goldMat;
            boss.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 5. Handheld
        private static PrizeDefinition BuildHandheld(string id, string name, PrizeRarity rarity, Material bodyMat, Material screenMat, Material btnMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Case";
            body.transform.parent = root.transform;
            body.transform.localScale = new Vector3(0.30f, 0.48f, 0.12f);
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            body.GetComponent<Collider>().material = physMat;

            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Screen";
            screen.transform.parent = root.transform;
            screen.transform.localPosition = new Vector3(0f, 0.10f, 0.062f);
            screen.transform.localScale = new Vector3(0.22f, 0.18f, 0.01f);
            screen.GetComponent<Renderer>().sharedMaterial = screenMat;
            screen.GetComponent<Collider>().material = physMat;

            GameObject dpad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dpad.name = "Dpad";
            dpad.transform.parent = root.transform;
            dpad.transform.localPosition = new Vector3(-0.06f, -0.10f, 0.065f);
            dpad.transform.localScale = new Vector3(0.08f, 0.08f, 0.02f);
            dpad.GetComponent<Renderer>().sharedMaterial = btnMat;
            dpad.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 6. CRT Monitor
        private static PrizeDefinition BuildCRT(string id, string name, PrizeRarity rarity, Material beigeMat, Material screenMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            housing.name = "Housing";
            housing.transform.parent = root.transform;
            housing.transform.localScale = new Vector3(0.40f, 0.36f, 0.38f);
            housing.GetComponent<Renderer>().sharedMaterial = beigeMat;
            housing.GetComponent<Collider>().material = physMat;

            GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tube.name = "CurvedTube";
            tube.transform.parent = root.transform;
            tube.transform.localPosition = new Vector3(0f, 0.02f, 0.14f);
            tube.transform.localScale = new Vector3(0.32f, 0.28f, 0.18f);
            tube.GetComponent<Renderer>().sharedMaterial = screenMat;
            tube.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 7. VR Visor
        private static PrizeDefinition BuildVR(string id, string name, PrizeRarity rarity, Material darkMat, Material glowMat, Material strapMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "VisorBody";
            visor.transform.parent = root.transform;
            visor.transform.localScale = new Vector3(0.42f, 0.22f, 0.26f);
            visor.GetComponent<Renderer>().sharedMaterial = darkMat;
            visor.GetComponent<Collider>().material = physMat;

            GameObject lightBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightBar.name = "NeonBar";
            lightBar.transform.parent = root.transform;
            lightBar.transform.localPosition = new Vector3(0f, 0f, 0.135f);
            lightBar.transform.localScale = new Vector3(0.36f, 0.06f, 0.02f);
            lightBar.GetComponent<Renderer>().sharedMaterial = glowMat;
            lightBar.GetComponent<Collider>().material = physMat;

            GameObject strap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strap.name = "Strap";
            strap.transform.parent = root.transform;
            strap.transform.localPosition = new Vector3(0f, 0f, -0.14f);
            strap.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            strap.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
            strap.GetComponent<Renderer>().sharedMaterial = strapMat;
            strap.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 8. Keytar
        private static PrizeDefinition BuildKeytar(string id, string name, PrizeRarity rarity, Material bodyMat, Material keyMat, Material neckMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
            main.name = "Body";
            main.transform.parent = root.transform;
            main.transform.localPosition = new Vector3(-0.06f, 0f, 0f);
            main.transform.localScale = new Vector3(0.36f, 0.18f, 0.10f);
            main.GetComponent<Renderer>().sharedMaterial = bodyMat;
            main.GetComponent<Collider>().material = physMat;

            GameObject keys = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keys.name = "Keyboard";
            keys.transform.parent = root.transform;
            keys.transform.localPosition = new Vector3(-0.06f, 0.05f, 0.052f);
            keys.transform.localScale = new Vector3(0.30f, 0.06f, 0.02f);
            keys.GetComponent<Renderer>().sharedMaterial = keyMat;
            keys.GetComponent<Collider>().material = physMat;

            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neck.name = "NeckHandle";
            neck.transform.parent = root.transform;
            neck.transform.localPosition = new Vector3(0.24f, 0.08f, 0f);
            neck.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            neck.transform.localScale = new Vector3(0.22f, 0.08f, 0.06f);
            neck.GetComponent<Renderer>().sharedMaterial = neckMat;
            neck.GetComponent<Collider>().material = physMat;

            return FinalizePrefab(root, id, name, rarity, mass, coinVal);
        }

        // 9. Golden Arcade Cabinet
        private static PrizeDefinition BuildGoldenCabinet(string id, string name, PrizeRarity rarity, Material goldMat, Material screenMat, float mass, int coinVal, PhysicsMaterial physMat)
        {
            GameObject root = new GameObject(name);
            // Lower cab
            GameObject cabBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabBase.name = "CabinetBase";
            cabBase.transform.parent = root.transform;
            cabBase.transform.localScale = new Vector3(0.32f, 0.34f, 0.32f);
            cabBase.GetComponent<Renderer>().sharedMaterial = goldMat;
            cabBase.GetComponent<Collider>().material = physMat;

            // Upper marquee / screen
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "MarqueeTop";
            top.transform.parent = root.transform;
            top.transform.localPosition = new Vector3(0f, 0.26f, -0.04f);
            top.transform.localScale = new Vector3(0.32f, 0.22f, 0.24f);
            top.GetComponent<Renderer>().sharedMaterial = goldMat;
            top.GetComponent<Collider>().material = physMat;

            // Inset Screen
            GameObject scr = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scr.name = "Screen";
            scr.transform.parent = root.transform;
            scr.transform.localPosition = new Vector3(0f, 0.16f, 0.10f);
            scr.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            scr.transform.localScale = new Vector3(0.24f, 0.14f, 0.02f);
            scr.GetComponent<Renderer>().sharedMaterial = screenMat;
            scr.GetComponent<Collider>().material = physMat;

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
