using System.IO;
using UnityEditor;
using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Editor
{
    public static class EnvironmentGenerator
    {
        private const string PrefabDir = "Assets/Prefabs/Environments";
        private const string MatDir = "Assets/Materials/Environments";

        [MenuItem("ClawMachine/Generate All Machine Environments")]
        public static void GenerateAllEnvironments()
        {
            EnsureDirectories();

            Shader toonShader = Shader.Find("ClawMachine/ToonOutline") ?? Shader.Find("Standard");
            Shader unlitShader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
            Material particleMat = GetOrCreateParticleMaterial();

            // 1. Toy Box
            GameObject toyBoxEnv = BuildToyBoxEnvironment(toonShader, particleMat);
            SaveEnvironmentPrefab(toyBoxEnv, "Env_ToyBox");

            // 2. Retro Arcade
            GameObject retroEnv = BuildRetroArcadeEnvironment(toonShader, unlitShader, particleMat);
            SaveEnvironmentPrefab(retroEnv, "Env_RetroArcade");

            // 3. Cosmic Galaxy
            GameObject cosmicEnv = BuildCosmicGalaxyEnvironment(toonShader, unlitShader, particleMat);
            SaveEnvironmentPrefab(cosmicEnv, "Env_CosmicGalaxy");

            // 4. Sweet Candy
            GameObject candyEnv = BuildSweetCandyEnvironment(toonShader, particleMat);
            SaveEnvironmentPrefab(candyEnv, "Env_SweetCandy");

            // 5. Fantasy Dungeon
            GameObject dungeonEnv = BuildFantasyDungeonEnvironment(toonShader, particleMat);
            SaveEnvironmentPrefab(dungeonEnv, "Env_FantasyDungeon");

            // 6. Ocean Abyss
            GameObject oceanEnv = BuildOceanAbyssEnvironment(toonShader, particleMat);
            SaveEnvironmentPrefab(oceanEnv, "Env_OceanAbyss");

            // Wire prefabs to MachineDefinitions
            WireEnvironmentsToDefinitions();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnvironmentGenerator] Successfully generated all 6 themed machine environments!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);
            if (!Directory.Exists(MatDir)) Directory.CreateDirectory(MatDir);
        }

        private static Material CreateThemedMaterial(string name, Color color, Shader shader, float gloss = 0.2f, Color? emission = null)
        {
            string path = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);

            if (emission.HasValue && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material GetOrCreateParticleMaterial()
        {
            string path = $"{MatDir}/Mat_EnvParticle.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader s = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                mat = new Material(s);
                mat.color = Color.white;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void StripColliders(GameObject go)
        {
            Collider[] cols = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                Object.DestroyImmediate(cols[i]);
            }
        }

        private static void SaveEnvironmentPrefab(GameObject root, string name)
        {
            StripColliders(root);
            string path = $"{PrefabDir}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        // ==========================================
        // 1. TOY BOX PLAYROOM
        // ==========================================
        private static GameObject BuildToyBoxEnvironment(Shader toonShader, Material partMat)
        {
            GameObject root = new GameObject("Environment_ToyBox");

            Material woodFloorMat = CreateThemedMaterial("Mat_Env_ToyBox_Floor", new Color(0.74f, 0.54f, 0.36f), toonShader, 0.35f);
            Material pastelWallMat = CreateThemedMaterial("Mat_Env_ToyBox_Wall", new Color(0.84f, 0.90f, 0.95f), toonShader, 0.1f);
            Material moldingMat = CreateThemedMaterial("Mat_Env_ToyBox_Molding", new Color(0.96f, 0.96f, 0.96f), toonShader, 0.2f);
            Material shelfMat = CreateThemedMaterial("Mat_Env_ToyBox_Shelf", new Color(0.88f, 0.45f, 0.25f), toonShader, 0.3f);
            Material blockMat1 = CreateThemedMaterial("Mat_Env_ToyBox_BlockRed", new Color(0.90f, 0.22f, 0.22f), toonShader);
            Material blockMat2 = CreateThemedMaterial("Mat_Env_ToyBox_BlockYellow", new Color(0.96f, 0.82f, 0.15f), toonShader);
            Material blockMat3 = CreateThemedMaterial("Mat_Env_ToyBox_BlockBlue", new Color(0.20f, 0.55f, 0.88f), toonShader);

            // Floor & Walls
            CreateBox("Floor", root.transform, new Vector3(0f, -0.06f, 1.0f), new Vector3(8.5f, 0.12f, 8.5f), woodFloorMat);
            CreateBox("BackWall", root.transform, new Vector3(0f, 3.4f, 4.0f), new Vector3(8.5f, 7.0f, 0.2f), pastelWallMat);
            CreateBox("Wainscoting", root.transform, new Vector3(0f, 0.8f, 3.88f), new Vector3(8.5f, 1.6f, 0.1f), moldingMat);
            CreateBox("LeftWall", root.transform, new Vector3(-4.2f, 3.4f, 1.0f), new Vector3(0.2f, 7.0f, 8.5f), pastelWallMat);
            CreateBox("RightWall", root.transform, new Vector3(4.2f, 3.4f, 1.0f), new Vector3(0.2f, 7.0f, 8.5f), pastelWallMat);

            // Left Side: Wooden Toy Shelving
            GameObject shelf = new GameObject("ToyShelves");
            shelf.transform.parent = root.transform;
            shelf.transform.localPosition = new Vector3(-2.8f, 0f, 2.8f);
            CreateBox("ShelfUprightL", shelf.transform, new Vector3(-0.6f, 1.5f, 0f), new Vector3(0.08f, 3.0f, 0.8f), shelfMat);
            CreateBox("ShelfUprightR", shelf.transform, new Vector3(0.6f, 1.5f, 0f), new Vector3(0.08f, 3.0f, 0.8f), shelfMat);
            CreateBox("Plank1", shelf.transform, new Vector3(0f, 0.4f, 0f), new Vector3(1.28f, 0.06f, 0.8f), shelfMat);
            CreateBox("Plank2", shelf.transform, new Vector3(0f, 1.4f, 0f), new Vector3(1.28f, 0.06f, 0.8f), shelfMat);
            CreateBox("Plank3", shelf.transform, new Vector3(0f, 2.4f, 0f), new Vector3(1.28f, 0.06f, 0.8f), shelfMat);
            // Cubby boxes on shelf
            CreateBox("CubbyRed", shelf.transform, new Vector3(-0.25f, 0.70f, 0f), new Vector3(0.5f, 0.5f, 0.6f), blockMat1);
            CreateBox("CubbyYellow", shelf.transform, new Vector3(0.25f, 0.70f, 0f), new Vector3(0.45f, 0.45f, 0.55f), blockMat2);
            CreateBox("CubbyBlue", shelf.transform, new Vector3(0f, 1.70f, 0f), new Vector3(0.65f, 0.45f, 0.6f), blockMat3);

            // Right Side: Giant Playroom ABC Alphabet Blocks
            GameObject blocks = new GameObject("GiantToyBlocks");
            blocks.transform.parent = root.transform;
            blocks.transform.localPosition = new Vector3(2.8f, 0f, 2.6f);
            CreateBox("Block_A", blocks.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.9f), blockMat1);
            CreateBox("Block_B", blocks.transform, new Vector3(0.1f, 1.25f, 0.05f), new Vector3(0.7f, 0.7f, 0.7f), blockMat2, Quaternion.Euler(0f, 18f, 0f));
            CreateBox("Block_C", blocks.transform, new Vector3(-0.75f, 0.35f, -0.2f), new Vector3(0.7f, 0.7f, 0.7f), blockMat3, Quaternion.Euler(0f, -12f, 0f));

            // Atmospheric Lighting
            GameObject lightObj = new GameObject("RoomWarmSpot");
            lightObj.transform.parent = root.transform;
            lightObj.transform.localPosition = new Vector3(0f, 5.5f, -1.0f);
            Light spot = lightObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 14f;
            spot.spotAngle = 65f;
            spot.color = new Color(1.0f, 0.95f, 0.85f);
            spot.intensity = 1.35f;
            spot.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);

            // Particle System: Dust Motes
            AddAmbientParticles(root, partMat, new Color(1.0f, 0.95f, 0.85f, 0.35f), 18, 0.05f, 0.12f, 0.15f);

            return root;
        }

        // ==========================================
        // 2. RETRO ARCADE (80s Neon)
        // ==========================================
        private static GameObject BuildRetroArcadeEnvironment(Shader toonShader, Shader unlitShader, Material partMat)
        {
            GameObject root = new GameObject("Environment_RetroArcade");

            Material floorMat = CreateThemedMaterial("Mat_Env_Arcade_Floor", new Color(0.06f, 0.06f, 0.09f), toonShader, 0.85f);
            Material wallMat = CreateThemedMaterial("Mat_Env_Arcade_Wall", new Color(0.10f, 0.11f, 0.15f), toonShader, 0.2f);
            Material neonMagenta = CreateThemedMaterial("Mat_Env_Arcade_NeonM", new Color(1.0f, 0.12f, 0.65f), unlitShader, 0f, new Color(1.0f, 0.15f, 0.7f) * 2.2f);
            Material neonCyan = CreateThemedMaterial("Mat_Env_Arcade_NeonC", new Color(0.12f, 0.88f, 1.0f), unlitShader, 0f, new Color(0.15f, 0.9f, 1.0f) * 2.2f);
            Material cabBlack = CreateThemedMaterial("Mat_Env_Arcade_Cabinet", new Color(0.14f, 0.15f, 0.18f), toonShader, 0.5f);

            // Floor & Walls
            CreateBox("Floor", root.transform, new Vector3(0f, -0.06f, 1.0f), new Vector3(8.5f, 0.12f, 8.5f), floorMat);
            CreateBox("BackWall", root.transform, new Vector3(0f, 3.4f, 4.0f), new Vector3(8.5f, 7.0f, 0.2f), wallMat);
            CreateBox("LeftWall", root.transform, new Vector3(-4.2f, 3.4f, 1.0f), new Vector3(0.2f, 7.0f, 8.5f), wallMat);
            CreateBox("RightWall", root.transform, new Vector3(4.2f, 3.4f, 1.0f), new Vector3(0.2f, 7.0f, 8.5f), wallMat);

            // Glowing Neon Grid Floor Strips
            for (int z = 0; z < 4; z++)
            {
                CreateBox($"GridH_{z}", root.transform, new Vector3(0f, 0.005f, -0.5f + z * 1.4f), new Vector3(8.2f, 0.01f, 0.04f), neonMagenta);
            }
            CreateBox("GridV_L", root.transform, new Vector3(-2.2f, 0.005f, 1.5f), new Vector3(0.04f, 0.01f, 5.0f), neonCyan);
            CreateBox("GridV_R", root.transform, new Vector3(2.2f, 0.005f, 1.5f), new Vector3(0.04f, 0.01f, 5.0f), neonCyan);

            // Wall Neon Arcade Sign Framing
            CreateBox("NeonArchTop", root.transform, new Vector3(0f, 5.4f, 3.86f), new Vector3(4.2f, 0.08f, 0.05f), neonCyan);
            CreateBox("NeonArchL", root.transform, new Vector3(-2.1f, 3.8f, 3.86f), new Vector3(0.08f, 3.2f, 0.05f), neonMagenta);
            CreateBox("NeonArchR", root.transform, new Vector3(2.1f, 3.8f, 3.86f), new Vector3(0.08f, 3.2f, 0.05f), neonMagenta);

            // Flanking Arcade Cabinets Silhouettes
            GameObject cabL = CreateArcadeCabinetSilhouette("Cabinet_Left", root.transform, new Vector3(-3.0f, 0f, 2.2f), cabBlack, neonMagenta);
            GameObject cabR = CreateArcadeCabinetSilhouette("Cabinet_Right", root.transform, new Vector3(3.0f, 0f, 2.2f), cabBlack, neonCyan);
            cabL.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            cabR.transform.localRotation = Quaternion.Euler(0f, -30f, 0f);

            // Dual-tone Accent Lights (Magenta on Left, Cyan on Right)
            GameObject lightL = new GameObject("Light_NeonMagenta");
            lightL.transform.parent = root.transform;
            lightL.transform.localPosition = new Vector3(-2.6f, 3.0f, 0f);
            Light pLightL = lightL.AddComponent<Light>();
            pLightL.type = LightType.Point;
            pLightL.range = 8f;
            pLightL.color = new Color(1.0f, 0.15f, 0.7f);
            pLightL.intensity = 2.2f;

            GameObject lightR = new GameObject("Light_NeonCyan");
            lightR.transform.parent = root.transform;
            lightR.transform.localPosition = new Vector3(2.6f, 3.0f, 0f);
            Light pLightR = lightR.AddComponent<Light>();
            pLightR.type = LightType.Point;
            pLightR.range = 8f;
            pLightR.color = new Color(0.15f, 0.9f, 1.0f);
            pLightR.intensity = 2.2f;

            AddAmbientParticles(root, partMat, new Color(0.4f, 0.8f, 1.0f, 0.45f), 24, 0.04f, 0.08f, 0.2f);

            return root;
        }

        private static GameObject CreateArcadeCabinetSilhouette(string name, Transform parent, Vector3 pos, Material bodyMat, Material marqueeMat)
        {
            GameObject cab = new GameObject(name);
            cab.transform.parent = parent;
            cab.transform.localPosition = pos;

            CreateBox("Body", cab.transform, new Vector3(0f, 1.8f, 0f), new Vector3(1.1f, 3.6f, 1.2f), bodyMat);
            CreateBox("Marquee", cab.transform, new Vector3(0f, 3.2f, -0.45f), new Vector3(0.95f, 0.35f, 0.1f), marqueeMat);
            CreateBox("Screen", cab.transform, new Vector3(0f, 2.2f, -0.42f), new Vector3(0.85f, 0.85f, 0.1f), marqueeMat);
            return cab;
        }

        // ==========================================
        // 3. COSMIC GALAXY (Deep Space Bay)
        // ==========================================
        private static GameObject BuildCosmicGalaxyEnvironment(Shader toonShader, Shader unlitShader, Material partMat)
        {
            GameObject root = new GameObject("Environment_CosmicGalaxy");

            Material deckMat = CreateThemedMaterial("Mat_Env_Cosmic_Deck", new Color(0.12f, 0.14f, 0.18f), toonShader, 0.4f);
            Material bulkheadMat = CreateThemedMaterial("Mat_Env_Cosmic_Bulkhead", new Color(0.20f, 0.22f, 0.28f), toonShader, 0.6f);
            Material nebulaSkyMat = CreateThemedMaterial("Mat_Env_Cosmic_Nebula", new Color(0.04f, 0.05f, 0.12f), unlitShader);
            Material starGlowMat = CreateThemedMaterial("Mat_Env_Cosmic_StarCyan", new Color(0.2f, 0.85f, 1.0f), unlitShader, 0f, new Color(0.2f, 0.9f, 1.0f) * 2.5f);
            Material amberEnergyMat = CreateThemedMaterial("Mat_Env_Cosmic_AmberGlow", new Color(1.0f, 0.65f, 0.2f), unlitShader, 0f, new Color(1.0f, 0.65f, 0.2f) * 2.0f);

            // Floor & Viewport Backdrop
            CreateBox("Floor", root.transform, new Vector3(0f, -0.06f, 1.0f), new Vector3(8.5f, 0.12f, 8.5f), deckMat);
            CreateBox("StarBackdrop", root.transform, new Vector3(0f, 3.5f, 4.2f), new Vector3(9.0f, 7.5f, 0.1f), nebulaSkyMat);

            // Airlock Structural Bulkheads Framing Viewport
            CreateBox("ArchTop", root.transform, new Vector3(0f, 5.8f, 3.8f), new Vector3(7.5f, 0.6f, 0.4f), bulkheadMat);
            CreateBox("ArchPillarL", root.transform, new Vector3(-3.2f, 3.0f, 3.8f), new Vector3(0.7f, 5.5f, 0.4f), bulkheadMat);
            CreateBox("ArchPillarR", root.transform, new Vector3(3.2f, 3.0f, 3.8f), new Vector3(0.7f, 5.5f, 0.4f), bulkheadMat);

            // Energy Conduits
            CreateBox("ConduitL", root.transform, new Vector3(-3.2f, 3.0f, 3.55f), new Vector3(0.12f, 5.2f, 0.08f), starGlowMat);
            CreateBox("ConduitR", root.transform, new Vector3(3.2f, 3.0f, 3.55f), new Vector3(0.12f, 5.2f, 0.08f), starGlowMat);

            // Sci-Fi Console Pylons on Sides
            GameObject pylonL = CreateCosmicPylon("PylonL", root.transform, new Vector3(-2.8f, 0f, 1.8f), bulkheadMat, starGlowMat);
            GameObject pylonR = CreateCosmicPylon("PylonR", root.transform, new Vector3(2.8f, 0f, 1.8f), bulkheadMat, amberEnergyMat);

            // Atmospheric Lighting
            GameObject lightObj = new GameObject("Light_CosmicAmber");
            lightObj.transform.parent = root.transform;
            lightObj.transform.localPosition = new Vector3(0f, 5.0f, -1.0f);
            Light spot = lightObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 14f;
            spot.spotAngle = 60f;
            spot.color = new Color(0.85f, 0.95f, 1.0f);
            spot.intensity = 1.6f;
            spot.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);

            AddAmbientParticles(root, partMat, new Color(0.4f, 0.9f, 1.0f, 0.65f), 30, 0.03f, 0.07f, 0.25f);

            return root;
        }

        private static GameObject CreateCosmicPylon(string name, Transform parent, Vector3 pos, Material bodyMat, Material coreMat)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent;
            go.transform.localPosition = pos;
            CreateBox("PylonBase", go.transform, new Vector3(0f, 0.3f, 0f), new Vector3(0.8f, 0.6f, 0.8f), bodyMat);
            CreateBox("PylonCore", go.transform, new Vector3(0f, 1.2f, 0f), new Vector3(0.35f, 1.4f, 0.35f), coreMat);
            CreateBox("PylonCap", go.transform, new Vector3(0f, 2.0f, 0f), new Vector3(0.7f, 0.3f, 0.7f), bodyMat);
            return go;
        }

        // ==========================================
        // 4. SWEET CANDY (Pastel Kingdom)
        // ==========================================
        private static GameObject BuildSweetCandyEnvironment(Shader toonShader, Material partMat)
        {
            GameObject root = new GameObject("Environment_SweetCandy");

            Material floorMat = CreateThemedMaterial("Mat_Env_Candy_Floor", new Color(0.96f, 0.82f, 0.87f), toonShader, 0.3f);
            Material wallMat = CreateThemedMaterial("Mat_Env_Candy_Wall", new Color(0.98f, 0.94f, 0.90f), toonShader, 0.1f);
            Material pinkFrosting = CreateThemedMaterial("Mat_Env_Candy_Frosting", new Color(0.94f, 0.40f, 0.62f), toonShader, 0.5f);
            Material candyWhite = CreateThemedMaterial("Mat_Env_Candy_White", new Color(0.98f, 0.98f, 0.98f), toonShader, 0.4f);
            Material mintMat = CreateThemedMaterial("Mat_Env_Candy_Mint", new Color(0.35f, 0.85f, 0.75f), toonShader, 0.4f);

            CreateBox("Floor", root.transform, new Vector3(0f, -0.06f, 1.0f), new Vector3(8.5f, 0.12f, 8.5f), floorMat);
            CreateBox("BackWall", root.transform, new Vector3(0f, 3.4f, 4.0f), new Vector3(8.5f, 7.0f, 0.2f), wallMat);

            // Frosting Drip Scallop on Back Wall
            CreateBox("FrostingTrim", root.transform, new Vector3(0f, 5.2f, 3.86f), new Vector3(8.5f, 0.6f, 0.12f), pinkFrosting);
            for (int d = -3; d <= 3; d++)
            {
                float h = (d % 2 == 0) ? 0.75f : 0.45f;
                CreateBox($"Drip_{d}", root.transform, new Vector3(d * 1.15f, 4.6f - h * 0.5f, 3.84f), new Vector3(0.45f, h, 0.08f), pinkFrosting);
            }

            // Flanking Giant Candy Canes (Striped Pillars)
            CreateCandyCane("CandyCaneL", root.transform, new Vector3(-2.8f, 0f, 2.4f), pinkFrosting, candyWhite);
            CreateCandyCane("CandyCaneR", root.transform, new Vector3(2.8f, 0f, 2.4f), pinkFrosting, candyWhite);

            // Giant Lollipop Decor
            GameObject lollipop = new GameObject("GiantLollipop");
            lollipop.transform.parent = root.transform;
            lollipop.transform.localPosition = new Vector3(-3.2f, 0f, 1.2f);
            CreateBox("Stick", lollipop.transform, new Vector3(0f, 1.4f, 0f), new Vector3(0.1f, 2.8f, 0.1f), candyWhite);
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            head.name = "SwirlHead";
            head.transform.parent = lollipop.transform;
            head.transform.localPosition = new Vector3(0f, 2.8f, 0f);
            head.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            head.transform.localScale = new Vector3(1.2f, 0.12f, 1.2f);
            head.GetComponent<Renderer>().sharedMaterial = mintMat;

            // Warm Pastel Lighting
            GameObject lightObj = new GameObject("Light_CandyPink");
            lightObj.transform.parent = root.transform;
            lightObj.transform.localPosition = new Vector3(0f, 5.2f, -1.0f);
            Light spot = lightObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 14f;
            spot.spotAngle = 65f;
            spot.color = new Color(1.0f, 0.88f, 0.94f);
            spot.intensity = 1.4f;
            spot.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);

            AddAmbientParticles(root, partMat, new Color(1.0f, 0.7f, 0.85f, 0.55f), 22, 0.05f, 0.10f, 0.18f);

            return root;
        }

        private static void CreateCandyCane(string name, Transform parent, Vector3 pos, Material redMat, Material whiteMat)
        {
            GameObject cc = new GameObject(name);
            cc.transform.parent = parent;
            cc.transform.localPosition = pos;
            for (int s = 0; s < 7; s++)
            {
                Material m = (s % 2 == 0) ? redMat : whiteMat;
                CreateBox($"Segment_{s}", cc.transform, new Vector3(0f, 0.35f + s * 0.6f, 0f), new Vector3(0.55f, 0.58f, 0.55f), m);
            }
        }

        // ==========================================
        // 5. FANTASY DUNGEON (Medieval Stone Vault)
        // ==========================================
        private static GameObject BuildFantasyDungeonEnvironment(Shader toonShader, Material partMat)
        {
            GameObject root = new GameObject("Environment_FantasyDungeon");

            Material stoneFloor = CreateThemedMaterial("Mat_Env_Dungeon_Floor", new Color(0.18f, 0.19f, 0.22f), toonShader, 0.15f);
            Material castleWall = CreateThemedMaterial("Mat_Env_Dungeon_Wall", new Color(0.24f, 0.25f, 0.28f), toonShader, 0.1f);
            Material ironMat = CreateThemedMaterial("Mat_Env_Dungeon_Iron", new Color(0.12f, 0.12f, 0.14f), toonShader, 0.7f);
            Material torchGlow = CreateThemedMaterial("Mat_Env_Dungeon_Fire", new Color(1.0f, 0.45f, 0.1f), toonShader, 0f, new Color(1.0f, 0.55f, 0.15f) * 2.5f);

            CreateBox("Floor", root.transform, new Vector3(0f, -0.06f, 1.0f), new Vector3(8.5f, 0.12f, 8.5f), stoneFloor);
            CreateBox("BackWall", root.transform, new Vector3(0f, 3.4f, 4.0f), new Vector3(8.5f, 7.0f, 0.2f), castleWall);

            // Heavy Archway & Portcullis Grate on Back Wall
            CreateBox("ArchwayL", root.transform, new Vector3(-1.8f, 3.0f, 3.86f), new Vector3(0.6f, 5.5f, 0.25f), castleWall);
            CreateBox("ArchwayR", root.transform, new Vector3(1.8f, 3.0f, 3.86f), new Vector3(0.6f, 5.5f, 0.25f), castleWall);
            CreateBox("ArchwayTop", root.transform, new Vector3(0f, 5.4f, 3.86f), new Vector3(4.0f, 0.6f, 0.25f), castleWall);
            for (int b = -2; b <= 2; b++)
            {
                CreateBox($"IronBar_{b}", root.transform, new Vector3(b * 0.6f, 3.2f, 3.92f), new Vector3(0.06f, 4.0f, 0.06f), ironMat);
            }

            // Flanking Torches with Flickering Amber Lights
            CreateTorchSconce("TorchL", root.transform, new Vector3(-2.6f, 3.0f, 3.4f), ironMat, torchGlow, new Color(1.0f, 0.55f, 0.15f));
            CreateTorchSconce("TorchR", root.transform, new Vector3(2.6f, 3.0f, 3.4f), ironMat, torchGlow, new Color(1.0f, 0.55f, 0.15f));

            AddAmbientParticles(root, partMat, new Color(1.0f, 0.50f, 0.15f, 0.70f), 28, 0.04f, 0.09f, 0.35f);

            return root;
        }

        private static void CreateTorchSconce(string name, Transform parent, Vector3 pos, Material ironMat, Material fireMat, Color lightColor)
        {
            GameObject t = new GameObject(name);
            t.transform.parent = parent;
            t.transform.localPosition = pos;

            CreateBox("Bracket", t.transform, new Vector3(0f, 0f, 0f), new Vector3(0.12f, 0.5f, 0.35f), ironMat);
            CreateBox("Cup", t.transform, new Vector3(0f, 0.22f, -0.2f), new Vector3(0.3f, 0.2f, 0.3f), ironMat);
            CreateBox("Flame", t.transform, new Vector3(0f, 0.42f, -0.2f), new Vector3(0.22f, 0.32f, 0.22f), fireMat);

            GameObject ltObj = new GameObject("TorchLight");
            ltObj.transform.parent = t.transform;
            ltObj.transform.localPosition = new Vector3(0f, 0.45f, -0.25f);
            Light lt = ltObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 7f;
            lt.color = lightColor;
            lt.intensity = 2.4f;
        }

        // ==========================================
        // 6. OCEAN ABYSS (Deep Trench)
        // ==========================================
        private static GameObject BuildOceanAbyssEnvironment(Shader toonShader, Material partMat)
        {
            GameObject root = new GameObject("Environment_OceanAbyss");

            Material seabedMat = CreateThemedMaterial("Mat_Env_Ocean_Floor", new Color(0.10f, 0.18f, 0.22f), toonShader, 0.3f);
            Material abyssWall = CreateThemedMaterial("Mat_Env_Ocean_Wall", new Color(0.04f, 0.08f, 0.14f), toonShader, 0.1f);
            Material coralGlow = CreateThemedMaterial("Mat_Env_Ocean_CoralGlow", new Color(0.15f, 0.85f, 0.75f), toonShader, 0f, new Color(0.15f, 0.85f, 0.75f) * 2.2f);
            Material kelpMat = CreateThemedMaterial("Mat_Env_Ocean_Kelp", new Color(0.14f, 0.48f, 0.32f), toonShader, 0.4f);

            CreateBox("Floor", root.transform, new Vector3(0f, -0.06f, 1.0f), new Vector3(8.5f, 0.12f, 8.5f), seabedMat);
            CreateBox("BackWall", root.transform, new Vector3(0f, 3.4f, 4.0f), new Vector3(8.5f, 7.0f, 0.2f), abyssWall);

            // Left Side: Coral Towers
            GameObject coral = new GameObject("BioluminescentCoral");
            coral.transform.parent = root.transform;
            coral.transform.localPosition = new Vector3(-2.8f, 0f, 2.4f);
            CreateBox("Trunk1", coral.transform, new Vector3(0f, 1.0f, 0f), new Vector3(0.5f, 2.0f, 0.5f), coralGlow);
            CreateBox("Branch1", coral.transform, new Vector3(0.4f, 1.8f, 0f), new Vector3(0.35f, 1.2f, 0.35f), coralGlow, Quaternion.Euler(0f, 0f, -25f));
            CreateBox("Branch2", coral.transform, new Vector3(-0.3f, 1.5f, 0.2f), new Vector3(0.3f, 1.0f, 0.3f), coralGlow, Quaternion.Euler(20f, 0f, 30f));

            // Right Side: Giant Kelp Strands
            GameObject kelp = new GameObject("SeaKelpStrands");
            kelp.transform.parent = root.transform;
            kelp.transform.localPosition = new Vector3(2.8f, 0f, 2.2f);
            for (int k = 0; k < 4; k++)
            {
                CreateBox($"Kelp_{k}", kelp.transform, new Vector3(-0.4f + k * 0.3f, 2.0f + (k % 2) * 0.5f, (k % 2) * 0.2f), new Vector3(0.18f, 4.2f, 0.08f), kelpMat, Quaternion.Euler(0f, 0f, (k - 1.5f) * 6f));
            }

            // Subsea Teal Spotlight
            GameObject lightObj = new GameObject("Light_OceanTeal");
            lightObj.transform.parent = root.transform;
            lightObj.transform.localPosition = new Vector3(0f, 5.5f, -1.0f);
            Light spot = lightObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 14f;
            spot.spotAngle = 65f;
            spot.color = new Color(0.2f, 0.75f, 0.85f);
            spot.intensity = 1.8f;
            spot.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);

            // Rising Air Bubbles
            AddAmbientParticles(root, partMat, new Color(0.3f, 0.85f, 1.0f, 0.6f), 24, 0.06f, 0.14f, 0.45f);

            return root;
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private static GameObject CreateBox(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Quaternion? localRot = null)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.parent = parent;
            box.transform.localPosition = localPos;
            box.transform.localScale = localScale;
            if (localRot.HasValue) box.transform.localRotation = localRot.Value;
            if (mat != null) box.GetComponent<Renderer>().sharedMaterial = mat;

            Collider col = box.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            return box;
        }

        private static void AddAmbientParticles(GameObject parent, Material partMat, Color col, int maxCount, float minSize, float maxSize, float speed)
        {
            GameObject pObj = new GameObject("AmbientVFX");
            pObj.transform.parent = parent.transform;
            pObj.transform.localPosition = new Vector3(0f, 1.8f, 1.5f);

            ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
            ParticleSystemRenderer psr = pObj.GetComponent<ParticleSystemRenderer>();
            if (partMat != null) psr.sharedMaterial = partMat;

            var main = ps.main;
            main.maxParticles = maxCount;
            main.startColor = col;
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startSpeed = speed;
            main.startLifetime = 4.0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.loop = true;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(4.5f, 2.5f, 3.5f);

            var emission = ps.emission;
            emission.rateOverTime = maxCount * 0.4f;
        }

        private static void WireEnvironmentsToDefinitions()
        {
            string[] defPaths = {
                "Assets/Data/Machine_ToyBox.asset",
                "Assets/Data/Machine_RetroArcade.asset",
                "Assets/Data/Machine_CosmicGalaxy.asset",
                "Assets/Data/Machine_SweetCandy.asset",
                "Assets/Data/Machine_FantasyDungeon.asset",
                "Assets/Data/Machine_OceanAbyss.asset"
            };

            string[] envPrefabs = {
                "Env_ToyBox",
                "Env_RetroArcade",
                "Env_CosmicGalaxy",
                "Env_SweetCandy",
                "Env_FantasyDungeon",
                "Env_OceanAbyss"
            };

            Color[] ambientColors = {
                new Color(0.28f, 0.26f, 0.25f), // ToyBox: warm daylight
                new Color(0.12f, 0.10f, 0.20f), // RetroArcade: neon dark
                new Color(0.08f, 0.12f, 0.24f), // Cosmic: deep space navy
                new Color(0.32f, 0.25f, 0.28f), // Candy: pastel pink
                new Color(0.18f, 0.14f, 0.12f), // Dungeon: torch ambient
                new Color(0.06f, 0.18f, 0.24f)  // Ocean: deep sea teal
            };

            Color[] dirLightColors = {
                new Color(1.0f, 0.95f, 0.88f),
                new Color(0.85f, 0.40f, 1.0f),
                new Color(0.50f, 0.85f, 1.0f),
                new Color(1.0f, 0.85f, 0.90f),
                new Color(1.0f, 0.65f, 0.35f),
                new Color(0.30f, 0.80f, 0.90f)
            };

            float[] fovs = { 45f, 43f, 46f, 45f, 44f, 45f };

            for (int i = 0; i < defPaths.Length; i++)
            {
                MachineDefinition def = AssetDatabase.LoadAssetAtPath<MachineDefinition>(defPaths[i]);
                if (def == null) continue;

                GameObject env = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{envPrefabs[i]}.prefab");
                def.environmentPrefab = env;
                def.ambientLightColor = ambientColors[i];
                def.directionalLightColor = dirLightColors[i];
                def.cameraFieldOfView = fovs[i];
                EditorUtility.SetDirty(def);
            }
        }
    }
}
