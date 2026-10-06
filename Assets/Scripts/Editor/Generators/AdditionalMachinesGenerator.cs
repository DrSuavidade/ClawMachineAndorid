#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.Editor
{
    public static class AdditionalMachinesGenerator
    {
        // ==========================================
        // 1. COSMIC GALAXY MACHINE (Machine 3)
        // ==========================================
        [MenuItem("ClawMachine/Generate Cosmic Galaxy Machine (9 Prizes)")]
        public static MachineDefinition GenerateCosmicGalaxy()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/CosmicGalaxy";
            const string DATA_DIR = "Assets/Data/Prizes/CosmicGalaxy";
            const string MAT_DIR = "Assets/Materials/CosmicGalaxy";
            const string MACHINE_PATH = "Assets/Data/Machine_CosmicGalaxy.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);

            PhysicsMaterial physMat = new PhysicsMaterial("CosmicPhysics")
            {
                dynamicFriction = 0.65f,
                staticFriction = 0.75f,
                bounciness = 0.18f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            Material slateMat = CreateLitMat(MAT_DIR, "Mat_MeteorSlate", new Color(0.25f, 0.26f, 0.30f));
            Material magmaMat = CreateLitMat(MAT_DIR, "Mat_MagmaGlow", new Color(1.0f, 0.40f, 0.08f));
            Material neonLimeMat = CreateLitMat(MAT_DIR, "Mat_NeonLime", new Color(0.20f, 0.95f, 0.35f));
            Material silverMat = CreateMetallicMat(MAT_DIR, "Mat_CosmicSilver", new Color(0.85f, 0.88f, 0.92f), 0.85f, 0.75f);
            Material deepBlueMat = CreateLitMat(MAT_DIR, "Mat_DeepSpaceBlue", new Color(0.12f, 0.18f, 0.38f));
            Material toxicGreenMat = CreateLitMat(MAT_DIR, "Mat_ToxicGreen", new Color(0.45f, 0.92f, 0.22f));
            Material bioPurpleMat = CreateLitMat(MAT_DIR, "Mat_BioPurple", new Color(0.68f, 0.18f, 0.82f));
            Material astroWhiteMat = CreateLitMat(MAT_DIR, "Mat_AstroSuitWhite", new Color(0.95f, 0.95f, 0.98f));
            Material goldVisorMat = CreateMetallicMat(MAT_DIR, "Mat_GoldVisor", new Color(1.0f, 0.82f, 0.15f), 0.95f, 0.90f);
            Material saturnTanMat = CreateLitMat(MAT_DIR, "Mat_SaturnTan", new Color(0.88f, 0.78f, 0.58f));
            Material ringIvoryMat = CreateLitMat(MAT_DIR, "Mat_RingIvory", new Color(0.95f, 0.90f, 0.82f));
            Material rocketRedMat = CreateLitMat(MAT_DIR, "Mat_RocketRed", new Color(0.92f, 0.18f, 0.18f));
            Material crystalCyanMat = CreateLitMat(MAT_DIR, "Mat_NebulaCyan", new Color(0.15f, 0.90f, 0.95f));
            Material crystalPinkMat = CreateLitMat(MAT_DIR, "Mat_NebulaMagenta", new Color(0.95f, 0.25f, 0.75f));

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Meteorite Chunk (Normal, 50 coins)
            {
                GameObject root = new GameObject("Meteorite Chunk");
                GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                core.name = "Core";
                core.transform.SetParent(root.transform, false);
                core.transform.localScale = new Vector3(0.40f, 0.38f, 0.42f);
                core.GetComponent<Renderer>().sharedMaterial = slateMat;
                core.GetComponent<Collider>().material = physMat;

                GameObject bump = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bump.name = "MagmaVein";
                bump.transform.SetParent(root.transform, false);
                bump.transform.localPosition = new Vector3(0.08f, 0.08f, 0.06f);
                bump.transform.localScale = new Vector3(0.18f, 0.16f, 0.20f);
                bump.transform.localRotation = Quaternion.Euler(25f, 40f, 15f);
                bump.GetComponent<Renderer>().sharedMaterial = magmaMat;
                bump.GetComponent<Collider>().material = physMat;
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_meteorite", "Meteorite Chunk", PrizeRarity.Normal, 0.52f, 50);
            }

            // 2. Sci-Fi Raygun (Normal, 50 coins)
            {
                GameObject root = new GameObject("Raygun Blaster");
                GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                barrel.name = "Barrel";
                barrel.transform.SetParent(root.transform, false);
                barrel.transform.localScale = new Vector3(0.12f, 0.24f, 0.12f);
                barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                barrel.GetComponent<Renderer>().sharedMaterial = silverMat;
                barrel.GetComponent<Collider>().material = physMat;

                GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grip.name = "Grip";
                grip.transform.SetParent(root.transform, false);
                grip.transform.localPosition = new Vector3(0f, -0.14f, -0.10f);
                grip.transform.localScale = new Vector3(0.09f, 0.20f, 0.09f);
                grip.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
                grip.GetComponent<Renderer>().sharedMaterial = neonLimeMat;
                grip.GetComponent<Collider>().material = physMat;

                GameObject emitter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                emitter.name = "Emitter";
                emitter.transform.SetParent(root.transform, false);
                emitter.transform.localPosition = new Vector3(0f, 0f, 0.26f);
                emitter.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
                emitter.GetComponent<Renderer>().sharedMaterial = neonLimeMat;
                emitter.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_raygun", "Raygun Blaster", PrizeRarity.Normal, 0.44f, 50);
            }

            // 3. Orbital Satellite (Normal, 55 coins)
            {
                GameObject root = new GameObject("Orbital Satellite");
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Hub";
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
                body.GetComponent<Renderer>().sharedMaterial = silverMat;
                body.GetComponent<Collider>().material = physMat;

                GameObject panelLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panelLeft.name = "PanelL";
                panelLeft.transform.SetParent(root.transform, false);
                panelLeft.transform.localPosition = new Vector3(-0.28f, 0f, 0f);
                panelLeft.transform.localScale = new Vector3(0.30f, 0.03f, 0.18f);
                panelLeft.GetComponent<Renderer>().sharedMaterial = deepBlueMat;
                panelLeft.GetComponent<Collider>().material = physMat;

                GameObject panelRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panelRight.name = "PanelR";
                panelRight.transform.SetParent(root.transform, false);
                panelRight.transform.localPosition = new Vector3(0.28f, 0f, 0f);
                panelRight.transform.localScale = new Vector3(0.30f, 0.03f, 0.18f);
                panelRight.GetComponent<Renderer>().sharedMaterial = deepBlueMat;
                panelRight.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_satellite", "Orbital Satellite", PrizeRarity.Normal, 0.48f, 55);
            }

            // 4. Alien Egg (Normal, 55 coins)
            {
                GameObject root = new GameObject("Alien Egg");
                GameObject egg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                egg.name = "Egg";
                egg.transform.SetParent(root.transform, false);
                egg.transform.localScale = new Vector3(0.36f, 0.48f, 0.36f);
                egg.GetComponent<Renderer>().sharedMaterial = toxicGreenMat;
                egg.GetComponent<Collider>().material = physMat;

                GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                band.name = "BioBand";
                band.transform.SetParent(root.transform, false);
                band.transform.localScale = new Vector3(0.38f, 0.06f, 0.38f);
                band.GetComponent<Renderer>().sharedMaterial = bioPurpleMat;
                band.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_alien_egg", "Glowing Alien Egg", PrizeRarity.Normal, 0.45f, 55);
            }

            // 5. Astro Helmet (Normal, 60 coins)
            {
                GameObject root = new GameObject("Space Helmet");
                GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dome.name = "Dome";
                dome.transform.SetParent(root.transform, false);
                dome.transform.localScale = new Vector3(0.40f, 0.40f, 0.40f);
                dome.GetComponent<Renderer>().sharedMaterial = astroWhiteMat;
                dome.GetComponent<Collider>().material = physMat;

                GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visor.name = "Visor";
                visor.transform.SetParent(root.transform, false);
                visor.transform.localPosition = new Vector3(0f, 0.04f, 0.14f);
                visor.transform.localScale = new Vector3(0.28f, 0.22f, 0.20f);
                visor.GetComponent<Renderer>().sharedMaterial = goldVisorMat;
                visor.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_helmet", "Space Helmet", PrizeRarity.Normal, 0.46f, 60);
            }

            // 6. Ringed Saturn Planet (Rare, 140 coins)
            {
                GameObject root = new GameObject("Saturn Planet");
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "Planet";
                orb.transform.SetParent(root.transform, false);
                orb.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                orb.GetComponent<Renderer>().sharedMaterial = saturnTanMat;
                orb.GetComponent<Collider>().material = physMat;

                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "Rings";
                ring.transform.SetParent(root.transform, false);
                ring.transform.localScale = new Vector3(0.65f, 0.02f, 0.65f);
                ring.transform.localRotation = Quaternion.Euler(22f, 0f, 15f);
                ring.GetComponent<Renderer>().sharedMaterial = ringIvoryMat;
                ring.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_saturn", "Ringed Saturn", PrizeRarity.Rare, 0.58f, 140);
            }

            // 7. Rocket Cruiser (Rare, 150 coins)
            {
                GameObject root = new GameObject("Rocket Cruiser");
                GameObject fus = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fus.name = "Fuselage";
                fus.transform.SetParent(root.transform, false);
                fus.transform.localScale = new Vector3(0.22f, 0.36f, 0.22f);
                fus.GetComponent<Renderer>().sharedMaterial = astroWhiteMat;
                fus.GetComponent<Collider>().material = physMat;

                GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                nose.name = "NoseCone";
                nose.transform.SetParent(root.transform, false);
                nose.transform.localPosition = new Vector3(0f, 0.38f, 0f);
                nose.transform.localScale = new Vector3(0.24f, 0.32f, 0.24f);
                nose.GetComponent<Renderer>().sharedMaterial = rocketRedMat;
                nose.GetComponent<Collider>().material = physMat;

                GameObject finL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                finL.name = "FinL";
                finL.transform.SetParent(root.transform, false);
                finL.transform.localPosition = new Vector3(-0.20f, -0.22f, 0f);
                finL.transform.localScale = new Vector3(0.18f, 0.16f, 0.04f);
                finL.GetComponent<Renderer>().sharedMaterial = rocketRedMat;
                finL.GetComponent<Collider>().material = physMat;

                GameObject finR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                finR.name = "FinR";
                finR.transform.SetParent(root.transform, false);
                finR.transform.localPosition = new Vector3(0.20f, -0.22f, 0f);
                finR.transform.localScale = new Vector3(0.18f, 0.16f, 0.04f);
                finR.GetComponent<Renderer>().sharedMaterial = rocketRedMat;
                finR.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_rocket", "Rocket Cruiser", PrizeRarity.Rare, 0.62f, 150);
            }

            // 8. Nebula Crystal (Rare, 160 coins)
            {
                GameObject root = new GameObject("Nebula Crystal");
                GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spire.name = "Spire";
                spire.transform.SetParent(root.transform, false);
                spire.transform.localScale = new Vector3(0.18f, 0.44f, 0.18f);
                spire.GetComponent<Renderer>().sharedMaterial = crystalCyanMat;
                spire.GetComponent<Collider>().material = physMat;

                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "CrownJewel";
                orb.transform.SetParent(root.transform, false);
                orb.transform.localPosition = new Vector3(0f, 0.44f, 0f);
                orb.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
                orb.GetComponent<Renderer>().sharedMaterial = crystalPinkMat;
                orb.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_nebula_crystal", "Nebula Crystal", PrizeRarity.Rare, 0.55f, 160);
            }

            // 9. Golden Astronaut Statue (Secret, 450 coins)
            {
                GameObject root = new GameObject("Golden Astronaut");
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.28f, 0.32f, 0.24f);
                body.GetComponent<Renderer>().sharedMaterial = goldVisorMat;
                body.GetComponent<Collider>().material = physMat;

                GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                helmet.name = "Helmet";
                helmet.transform.SetParent(root.transform, false);
                helmet.transform.localPosition = new Vector3(0f, 0.36f, 0f);
                helmet.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
                helmet.GetComponent<Renderer>().sharedMaterial = goldVisorMat;
                helmet.GetComponent<Collider>().material = physMat;

                GameObject basePedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                basePedestal.name = "Pedestal";
                basePedestal.transform.SetParent(root.transform, false);
                basePedestal.transform.localPosition = new Vector3(0f, -0.35f, 0f);
                basePedestal.transform.localScale = new Vector3(0.46f, 0.08f, 0.46f);
                basePedestal.GetComponent<Renderer>().sharedMaterial = goldVisorMat;
                basePedestal.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "cosmic_golden_astronaut", "Golden Astronaut", PrizeRarity.Secret, 0.75f, 450);
            }

            return SaveMachineDefinition(MACHINE_PATH, "cosmic_galaxy", "Cosmic Galaxy",
                "Delve into deep space relics, planetary artifacts, and alien tech.",
                400, 20, 30,
                new Color(0.42f, 0.16f, 0.72f), new Color(0.06f, 0.08f, 0.16f), prizes);
        }

        // ==========================================
        // 2. SWEET CANDY MACHINE (Machine 4)
        // ==========================================
        [MenuItem("ClawMachine/Generate Sweet Candy Machine (9 Prizes)")]
        public static MachineDefinition GenerateSweetCandy()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/SweetCandy";
            const string DATA_DIR = "Assets/Data/Prizes/SweetCandy";
            const string MAT_DIR = "Assets/Materials/SweetCandy";
            const string MACHINE_PATH = "Assets/Data/Machine_SweetCandy.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);

            PhysicsMaterial physMat = new PhysicsMaterial("CandyPhysics")
            {
                dynamicFriction = 0.75f,
                staticFriction = 0.85f,
                bounciness = 0.14f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            Material cherryPinkMat = CreateLitMat(MAT_DIR, "Mat_CherryPink", new Color(0.96f, 0.28f, 0.55f));
            Material sugarWhiteMat = CreateLitMat(MAT_DIR, "Mat_SugarWhite", new Color(0.98f, 0.98f, 0.98f));
            Material gummyOrangeMat = CreateLitMat(MAT_DIR, "Mat_GummyOrange", new Color(1.0f, 0.52f, 0.10f));
            Material chocolateBrownMat = CreateLitMat(MAT_DIR, "Mat_ChocoBrown", new Color(0.38f, 0.22f, 0.15f));
            Material foilGoldMat = CreateMetallicMat(MAT_DIR, "Mat_FoilGold", new Color(0.95f, 0.82f, 0.25f), 0.90f, 0.85f);
            Material donutDoughMat = CreateLitMat(MAT_DIR, "Mat_DonutDough", new Color(0.88f, 0.72f, 0.46f));
            Material mintBlueMat = CreateLitMat(MAT_DIR, "Mat_MintBlue", new Color(0.35f, 0.88f, 0.82f));
            Material sprinkleYellowMat = CreateLitMat(MAT_DIR, "Mat_SprinkleYellow", new Color(1.0f, 0.90f, 0.15f));
            Material lavenderMat = CreateLitMat(MAT_DIR, "Mat_MacaronLavender", new Color(0.78f, 0.65f, 0.92f));
            Material lemonMat = CreateLitMat(MAT_DIR, "Mat_MacaronLemon", new Color(0.95f, 0.92f, 0.35f));
            Material diamondSugarMat = CreateMetallicMat(MAT_DIR, "Mat_SugarDiamond", new Color(0.85f, 0.95f, 1.0f), 0.40f, 0.95f);

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Swirl Lollipop (Normal, 80 coins)
            {
                GameObject root = new GameObject("Swirl Lollipop");
                GameObject candy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                candy.name = "Candy";
                candy.transform.SetParent(root.transform, false);
                candy.transform.localScale = new Vector3(0.42f, 0.05f, 0.42f);
                candy.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                candy.GetComponent<Renderer>().sharedMaterial = cherryPinkMat;
                candy.GetComponent<Collider>().material = physMat;

                GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick.name = "Stick";
                stick.transform.SetParent(root.transform, false);
                stick.transform.localPosition = new Vector3(0f, -0.28f, 0f);
                stick.transform.localScale = new Vector3(0.04f, 0.25f, 0.04f);
                stick.GetComponent<Renderer>().sharedMaterial = sugarWhiteMat;
                stick.GetComponent<Collider>().material = physMat;
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_lollipop", "Swirl Lollipop", PrizeRarity.Normal, 0.42f, 80);
            }

            // 2. Gummy Bear (Normal, 80 coins)
            {
                GameObject root = new GameObject("Gummy Bear");
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.32f, 0.28f, 0.26f);
                body.GetComponent<Renderer>().sharedMaterial = gummyOrangeMat;
                body.GetComponent<Collider>().material = physMat;

                GameObject earL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                earL.name = "EarL";
                earL.transform.SetParent(root.transform, false);
                earL.transform.localPosition = new Vector3(-0.12f, 0.24f, 0f);
                earL.transform.localScale = new Vector3(0.12f, 0.12f, 0.10f);
                earL.GetComponent<Renderer>().sharedMaterial = gummyOrangeMat;
                earL.GetComponent<Collider>().material = physMat;

                GameObject earR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                earR.name = "EarR";
                earR.transform.SetParent(root.transform, false);
                earR.transform.localPosition = new Vector3(0.12f, 0.24f, 0f);
                earR.transform.localScale = new Vector3(0.12f, 0.12f, 0.10f);
                earR.GetComponent<Renderer>().sharedMaterial = gummyOrangeMat;
                earR.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_gummy_bear", "Gummy Bear", PrizeRarity.Normal, 0.45f, 80);
            }

            // 3. Chocolate Bar (Normal, 85 coins)
            {
                GameObject root = new GameObject("Chocolate Bar");
                GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = "Slab";
                slab.transform.SetParent(root.transform, false);
                slab.transform.localScale = new Vector3(0.44f, 0.10f, 0.32f);
                slab.GetComponent<Renderer>().sharedMaterial = chocolateBrownMat;
                slab.GetComponent<Collider>().material = physMat;

                GameObject foil = GameObject.CreatePrimitive(PrimitiveType.Cube);
                foil.name = "FoilWrapper";
                foil.transform.SetParent(root.transform, false);
                foil.transform.localPosition = new Vector3(-0.12f, 0.01f, 0f);
                foil.transform.localScale = new Vector3(0.24f, 0.11f, 0.33f);
                foil.GetComponent<Renderer>().sharedMaterial = foilGoldMat;
                foil.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_chocolate", "Chocolate Bar", PrizeRarity.Normal, 0.50f, 85);
            }

            // 4. Glazed Donut (Normal, 85 coins)
            {
                GameObject root = new GameObject("Glazed Donut");
                GameObject dough = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dough.name = "Dough";
                dough.transform.SetParent(root.transform, false);
                dough.transform.localScale = new Vector3(0.42f, 0.10f, 0.42f);
                dough.GetComponent<Renderer>().sharedMaterial = donutDoughMat;
                dough.GetComponent<Collider>().material = physMat;

                GameObject icing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                icing.name = "PinkIcing";
                icing.transform.SetParent(root.transform, false);
                icing.transform.localPosition = new Vector3(0f, 0.06f, 0f);
                icing.transform.localScale = new Vector3(0.38f, 0.04f, 0.38f);
                icing.GetComponent<Renderer>().sharedMaterial = cherryPinkMat;
                icing.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_donut", "Glazed Donut", PrizeRarity.Normal, 0.46f, 85);
            }

            // 5. Candy Cane (Normal, 90 coins)
            {
                GameObject root = new GameObject("Candy Cane");
                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "Stem";
                stem.transform.SetParent(root.transform, false);
                stem.transform.localScale = new Vector3(0.08f, 0.28f, 0.08f);
                stem.GetComponent<Renderer>().sharedMaterial = sugarWhiteMat;
                stem.GetComponent<Collider>().material = physMat;

                GameObject hook = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hook.name = "Hook";
                hook.transform.SetParent(root.transform, false);
                hook.transform.localPosition = new Vector3(0.08f, 0.28f, 0f);
                hook.transform.localScale = new Vector3(0.18f, 0.16f, 0.08f);
                hook.GetComponent<Renderer>().sharedMaterial = cherryPinkMat;
                hook.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_cane", "Candy Cane", PrizeRarity.Normal, 0.40f, 90);
            }

            // 6. Swirl Cupcake (Rare, 220 coins)
            {
                GameObject root = new GameObject("Swirl Cupcake");
                GameObject cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cup.name = "PaperCup";
                cup.transform.SetParent(root.transform, false);
                cup.transform.localScale = new Vector3(0.36f, 0.14f, 0.36f);
                cup.GetComponent<Renderer>().sharedMaterial = donutDoughMat;
                cup.GetComponent<Collider>().material = physMat;

                GameObject frosting = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                frosting.name = "Frosting";
                frosting.transform.SetParent(root.transform, false);
                frosting.transform.localPosition = new Vector3(0f, 0.18f, 0f);
                frosting.transform.localScale = new Vector3(0.34f, 0.24f, 0.34f);
                frosting.GetComponent<Renderer>().sharedMaterial = mintBlueMat;
                frosting.GetComponent<Collider>().material = physMat;

                GameObject cherry = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cherry.name = "Cherry";
                cherry.transform.SetParent(root.transform, false);
                cherry.transform.localPosition = new Vector3(0f, 0.32f, 0f);
                cherry.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                cherry.GetComponent<Renderer>().sharedMaterial = cherryPinkMat;
                cherry.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_cupcake", "Swirl Cupcake", PrizeRarity.Rare, 0.54f, 220);
            }

            // 7. Macaron Tower (Rare, 235 coins)
            {
                GameObject root = new GameObject("Macaron Trio");
                GameObject m1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                m1.name = "Macaron1";
                m1.transform.SetParent(root.transform, false);
                m1.transform.localScale = new Vector3(0.34f, 0.08f, 0.34f);
                m1.GetComponent<Renderer>().sharedMaterial = cherryPinkMat;
                m1.GetComponent<Collider>().material = physMat;

                GameObject m2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                m2.name = "Macaron2";
                m2.transform.SetParent(root.transform, false);
                m2.transform.localPosition = new Vector3(0f, 0.10f, 0f);
                m2.transform.localScale = new Vector3(0.32f, 0.08f, 0.32f);
                m2.GetComponent<Renderer>().sharedMaterial = lavenderMat;
                m2.GetComponent<Collider>().material = physMat;

                GameObject m3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                m3.name = "Macaron3";
                m3.transform.SetParent(root.transform, false);
                m3.transform.localPosition = new Vector3(0f, 0.20f, 0f);
                m3.transform.localScale = new Vector3(0.30f, 0.08f, 0.30f);
                m3.GetComponent<Renderer>().sharedMaterial = lemonMat;
                m3.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_macaron", "Macaron Trio", PrizeRarity.Rare, 0.58f, 235);
            }

            // 8. Ice Cream Cone (Rare, 250 coins)
            {
                GameObject root = new GameObject("Ice Cream Cone");
                GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cone.name = "WaffleCone";
                cone.transform.SetParent(root.transform, false);
                cone.transform.localScale = new Vector3(0.18f, 0.26f, 0.18f);
                cone.GetComponent<Renderer>().sharedMaterial = donutDoughMat;
                cone.GetComponent<Collider>().material = physMat;

                GameObject scoop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                scoop.name = "Scoop";
                scoop.transform.SetParent(root.transform, false);
                scoop.transform.localPosition = new Vector3(0f, 0.30f, 0f);
                scoop.transform.localScale = new Vector3(0.30f, 0.28f, 0.30f);
                scoop.GetComponent<Renderer>().sharedMaterial = cherryPinkMat;
                scoop.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_ice_cream", "Ice Cream Cone", PrizeRarity.Rare, 0.56f, 250);
            }

            // 9. Diamond Sugar Crown (Secret, 750 coins)
            {
                GameObject root = new GameObject("Diamond Sugar Crown");
                GameObject circlet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                circlet.name = "Circlet";
                circlet.transform.SetParent(root.transform, false);
                circlet.transform.localScale = new Vector3(0.42f, 0.12f, 0.42f);
                circlet.GetComponent<Renderer>().sharedMaterial = diamondSugarMat;
                circlet.GetComponent<Collider>().material = physMat;

                GameObject gemFront = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gemFront.name = "CenterRuby";
                gemFront.transform.SetParent(root.transform, false);
                gemFront.transform.localPosition = new Vector3(0f, 0.14f, 0.20f);
                gemFront.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
                gemFront.GetComponent<Renderer>().sharedMaterial = cherryPinkMat;
                gemFront.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "candy_diamond_crown", "Sugar Gem Crown", PrizeRarity.Secret, 0.72f, 750);
            }

            return SaveMachineDefinition(MACHINE_PATH, "sweet_candy", "Sweet Candy",
                "Indulge in sugary confectioneries, pastries, and crystalline desserts.",
                900, 35, 50,
                new Color(0.96f, 0.36f, 0.62f), new Color(0.18f, 0.38f, 0.34f), prizes);
        }

        // ==========================================
        // 3. FANTASY DUNGEON MACHINE (Machine 5)
        // ==========================================
        [MenuItem("ClawMachine/Generate Fantasy Dungeon Machine (9 Prizes)")]
        public static MachineDefinition GenerateFantasyDungeon()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/FantasyDungeon";
            const string DATA_DIR = "Assets/Data/Prizes/FantasyDungeon";
            const string MAT_DIR = "Assets/Materials/FantasyDungeon";
            const string MACHINE_PATH = "Assets/Data/Machine_FantasyDungeon.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);

            PhysicsMaterial physMat = new PhysicsMaterial("DungeonPhysics")
            {
                dynamicFriction = 0.70f,
                staticFriction = 0.80f,
                bounciness = 0.10f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            Material steelMat = CreateMetallicMat(MAT_DIR, "Mat_ForgedSteel", new Color(0.72f, 0.75f, 0.80f), 0.85f, 0.70f);
            Material leatherMat = CreateLitMat(MAT_DIR, "Mat_LeatherBrown", new Color(0.40f, 0.26f, 0.16f));
            Material redPotionMat = CreateLitMat(MAT_DIR, "Mat_ElixirCrimson", new Color(0.92f, 0.12f, 0.20f));
            Material potionGlassMat = CreateMetallicMat(MAT_DIR, "Mat_PotionGlass", new Color(0.85f, 0.95f, 0.95f), 0.20f, 0.95f);
            Material boneMat = CreateLitMat(MAT_DIR, "Mat_BoneIvory", new Color(0.90f, 0.88f, 0.78f));
            Material emeraldScaleMat = CreateMetallicMat(MAT_DIR, "Mat_DragonEmerald", new Color(0.12f, 0.78f, 0.42f), 0.65f, 0.85f);
            Material royalBlueMat = CreateLitMat(MAT_DIR, "Mat_RoyalBlue", new Color(0.18f, 0.32f, 0.78f));
            Material mysticPurpleMat = CreateLitMat(MAT_DIR, "Mat_MysticPurple", new Color(0.48f, 0.16f, 0.72f));
            Material woodMat = CreateLitMat(MAT_DIR, "Mat_AncientWood", new Color(0.42f, 0.28f, 0.14f));
            Material sapphireOrbMat = CreateMetallicMat(MAT_DIR, "Mat_SapphireOrb", new Color(0.15f, 0.65f, 1.0f), 0.50f, 0.95f);
            Material mythrilGoldMat = CreateMetallicMat(MAT_DIR, "Mat_MythrilGold", new Color(1.0f, 0.84f, 0.20f), 0.95f, 0.90f);
            Material stoneMat = CreateLitMat(MAT_DIR, "Mat_AnvilGranite", new Color(0.24f, 0.25f, 0.28f));

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Rogue Iron Dagger (Normal, 120 coins)
            {
                GameObject root = new GameObject("Iron Dagger");
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "Blade";
                blade.transform.SetParent(root.transform, false);
                blade.transform.localScale = new Vector3(0.08f, 0.36f, 0.04f);
                blade.GetComponent<Renderer>().sharedMaterial = steelMat;
                blade.GetComponent<Collider>().material = physMat;

                GameObject hilt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hilt.name = "Crossguard";
                hilt.transform.SetParent(root.transform, false);
                hilt.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                hilt.transform.localScale = new Vector3(0.22f, 0.05f, 0.06f);
                hilt.GetComponent<Renderer>().sharedMaterial = mythrilGoldMat;
                hilt.GetComponent<Collider>().material = physMat;

                GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                grip.name = "Grip";
                grip.transform.SetParent(root.transform, false);
                grip.transform.localPosition = new Vector3(0f, -0.28f, 0f);
                grip.transform.localScale = new Vector3(0.06f, 0.12f, 0.06f);
                grip.GetComponent<Renderer>().sharedMaterial = leatherMat;
                grip.GetComponent<Collider>().material = physMat;
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_dagger", "Iron Dagger", PrizeRarity.Normal, 0.48f, 120);
            }

            // 2. Health Potion (Normal, 120 coins)
            {
                GameObject root = new GameObject("Health Potion");
                GameObject flask = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flask.name = "Flask";
                flask.transform.SetParent(root.transform, false);
                flask.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                flask.GetComponent<Renderer>().sharedMaterial = redPotionMat;
                flask.GetComponent<Collider>().material = physMat;

                GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                neck.name = "Neck";
                neck.transform.SetParent(root.transform, false);
                neck.transform.localPosition = new Vector3(0f, 0.22f, 0f);
                neck.transform.localScale = new Vector3(0.12f, 0.10f, 0.12f);
                neck.GetComponent<Renderer>().sharedMaterial = potionGlassMat;
                neck.GetComponent<Collider>().material = physMat;

                GameObject cork = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cork.name = "Cork";
                cork.transform.SetParent(root.transform, false);
                cork.transform.localPosition = new Vector3(0f, 0.30f, 0f);
                cork.transform.localScale = new Vector3(0.10f, 0.06f, 0.10f);
                cork.GetComponent<Renderer>().sharedMaterial = leatherMat;
                cork.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_potion", "Health Potion", PrizeRarity.Normal, 0.50f, 120);
            }

            // 3. Goblin Skull (Normal, 130 coins)
            {
                GameObject root = new GameObject("Goblin Skull");
                GameObject cranium = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cranium.name = "Cranium";
                cranium.transform.SetParent(root.transform, false);
                cranium.transform.localScale = new Vector3(0.38f, 0.32f, 0.38f);
                cranium.GetComponent<Renderer>().sharedMaterial = boneMat;
                cranium.GetComponent<Collider>().material = physMat;

                GameObject jaw = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jaw.name = "Jaw";
                jaw.transform.SetParent(root.transform, false);
                jaw.transform.localPosition = new Vector3(0f, -0.12f, 0.12f);
                jaw.transform.localScale = new Vector3(0.24f, 0.12f, 0.22f);
                jaw.GetComponent<Renderer>().sharedMaterial = boneMat;
                jaw.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_skull", "Goblin Skull", PrizeRarity.Normal, 0.46f, 130);
            }

            // 4. Dragon Scale (Normal, 130 coins)
            {
                GameObject root = new GameObject("Dragon Scale");
                GameObject scale = GameObject.CreatePrimitive(PrimitiveType.Cube);
                scale.name = "ScaleBody";
                scale.transform.SetParent(root.transform, false);
                scale.transform.localScale = new Vector3(0.34f, 0.44f, 0.08f);
                scale.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                scale.GetComponent<Renderer>().sharedMaterial = emeraldScaleMat;
                scale.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_scale", "Dragon Scale", PrizeRarity.Normal, 0.44f, 130);
            }

            // 5. Knight Shield (Normal, 140 coins)
            {
                GameObject root = new GameObject("Knight Shield");
                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = "ShieldPlate";
                plate.transform.SetParent(root.transform, false);
                plate.transform.localScale = new Vector3(0.36f, 0.48f, 0.06f);
                plate.GetComponent<Renderer>().sharedMaterial = royalBlueMat;
                plate.GetComponent<Collider>().material = physMat;

                GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boss.name = "CenterBoss";
                boss.transform.SetParent(root.transform, false);
                boss.transform.localPosition = new Vector3(0f, 0f, 0.05f);
                boss.transform.localScale = new Vector3(0.14f, 0.14f, 0.08f);
                boss.GetComponent<Renderer>().sharedMaterial = steelMat;
                boss.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_shield", "Knight Shield", PrizeRarity.Normal, 0.52f, 140);
            }

            // 6. Wizard Spellbook (Rare, 350 coins)
            {
                GameObject root = new GameObject("Wizard Spellbook");
                GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cover.name = "Tome";
                cover.transform.SetParent(root.transform, false);
                cover.transform.localScale = new Vector3(0.38f, 0.12f, 0.48f);
                cover.GetComponent<Renderer>().sharedMaterial = mysticPurpleMat;
                cover.GetComponent<Collider>().material = physMat;

                GameObject clasp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                clasp.name = "GoldClasp";
                clasp.transform.SetParent(root.transform, false);
                clasp.transform.localPosition = new Vector3(0.18f, 0f, 0f);
                clasp.transform.localScale = new Vector3(0.06f, 0.14f, 0.12f);
                clasp.GetComponent<Renderer>().sharedMaterial = mythrilGoldMat;
                clasp.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_spellbook", "Wizard Spellbook", PrizeRarity.Rare, 0.60f, 350);
            }

            // 7. Elven Bow (Rare, 375 coins)
            {
                GameObject root = new GameObject("Elven Bow");
                GameObject stave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stave.name = "Stave";
                stave.transform.SetParent(root.transform, false);
                stave.transform.localScale = new Vector3(0.08f, 0.50f, 0.08f);
                stave.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
                stave.GetComponent<Renderer>().sharedMaterial = woodMat;
                stave.GetComponent<Collider>().material = physMat;

                GameObject wrap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wrap.name = "GripWrap";
                wrap.transform.SetParent(root.transform, false);
                wrap.transform.localScale = new Vector3(0.10f, 0.14f, 0.10f);
                wrap.GetComponent<Renderer>().sharedMaterial = leatherMat;
                wrap.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_bow", "Elven Bow", PrizeRarity.Rare, 0.55f, 375);
            }

            // 8. Crystal Staff (Rare, 400 coins)
            {
                GameObject root = new GameObject("Crystal Staff");
                GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shaft.name = "Shaft";
                shaft.transform.SetParent(root.transform, false);
                shaft.transform.localScale = new Vector3(0.08f, 0.46f, 0.08f);
                shaft.GetComponent<Renderer>().sharedMaterial = woodMat;
                shaft.GetComponent<Collider>().material = physMat;

                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "SapphireOrb";
                orb.transform.SetParent(root.transform, false);
                orb.transform.localPosition = new Vector3(0f, 0.50f, 0f);
                orb.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
                orb.GetComponent<Renderer>().sharedMaterial = sapphireOrbMat;
                orb.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_staff", "Crystal Staff", PrizeRarity.Rare, 0.58f, 400);
            }

            // 9. Excalibur in Stone (Secret, 1,200 coins)
            {
                GameObject root = new GameObject("Excalibur in Stone");
                GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boulder.name = "GraniteStone";
                boulder.transform.SetParent(root.transform, false);
                boulder.transform.localScale = new Vector3(0.48f, 0.26f, 0.44f);
                boulder.GetComponent<Renderer>().sharedMaterial = stoneMat;
                boulder.GetComponent<Collider>().material = physMat;

                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "MythrilBlade";
                blade.transform.SetParent(root.transform, false);
                blade.transform.localPosition = new Vector3(0f, 0.26f, 0f);
                blade.transform.localScale = new Vector3(0.08f, 0.36f, 0.04f);
                blade.GetComponent<Renderer>().sharedMaterial = mythrilGoldMat;
                blade.GetComponent<Collider>().material = physMat;

                GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                guard.name = "GoldenGuard";
                guard.transform.SetParent(root.transform, false);
                guard.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                guard.transform.localScale = new Vector3(0.24f, 0.05f, 0.06f);
                guard.GetComponent<Renderer>().sharedMaterial = mythrilGoldMat;
                guard.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "dungeon_excalibur", "Excalibur Sword", PrizeRarity.Secret, 0.85f, 1200);
            }

            return SaveMachineDefinition(MACHINE_PATH, "fantasy_dungeon", "Fantasy Dungeon",
                "Explore legendary medieval treasures, wizard relics, and mythic weapons.",
                1800, 55, 85,
                new Color(0.76f, 0.54f, 0.18f), new Color(0.11f, 0.12f, 0.15f), prizes);
        }

        // ==========================================
        // 4. OCEAN ABYSS MACHINE (Machine 6)
        // ==========================================
        [MenuItem("ClawMachine/Generate Ocean Abyss Machine (9 Prizes)")]
        public static MachineDefinition GenerateOceanAbyss()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/OceanAbyss";
            const string DATA_DIR = "Assets/Data/Prizes/OceanAbyss";
            const string MAT_DIR = "Assets/Materials/OceanAbyss";
            const string MACHINE_PATH = "Assets/Data/Machine_OceanAbyss.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);

            PhysicsMaterial physMat = new PhysicsMaterial("OceanPhysics")
            {
                dynamicFriction = 0.72f,
                staticFriction = 0.82f,
                bounciness = 0.12f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            Material coralPeachMat = CreateLitMat(MAT_DIR, "Mat_CoralPeach", new Color(0.98f, 0.48f, 0.35f));
            Material clamVioletMat = CreateLitMat(MAT_DIR, "Mat_ClamViolet", new Color(0.55f, 0.32f, 0.72f));
            Material pearlWhiteMat = CreateMetallicMat(MAT_DIR, "Mat_PearlIridescent", new Color(0.96f, 0.95f, 1.0f), 0.40f, 0.95f);
            Material rustedIronMat = CreateLitMat(MAT_DIR, "Mat_RustedIron", new Color(0.36f, 0.28f, 0.24f));
            Material seaTurtleOliveMat = CreateLitMat(MAT_DIR, "Mat_TurtleOlive", new Color(0.32f, 0.55f, 0.28f));
            Material shellCreamMat = CreateLitMat(MAT_DIR, "Mat_ShellCream", new Color(0.95f, 0.88f, 0.76f));
            Material bronzeMat = CreateMetallicMat(MAT_DIR, "Mat_NavalBronze", new Color(0.78f, 0.55f, 0.25f), 0.85f, 0.70f);
            Material sharkToothMat = CreateLitMat(MAT_DIR, "Mat_FossilTooth", new Color(0.92f, 0.90f, 0.85f));
            Material chestOakMat = CreateLitMat(MAT_DIR, "Mat_SunkenOak", new Color(0.32f, 0.22f, 0.14f));
            Material pirateGoldMat = CreateMetallicMat(MAT_DIR, "Mat_PirateGold", new Color(1.0f, 0.82f, 0.10f), 0.95f, 0.88f);
            Material seaGemMat = CreateMetallicMat(MAT_DIR, "Mat_AquamarineGem", new Color(0.12f, 0.90f, 0.85f), 0.60f, 0.95f);

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Spiny Starfish (Normal, 180 coins)
            {
                GameObject root = new GameObject("Spiny Starfish");
                GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                center.name = "Center";
                center.transform.SetParent(root.transform, false);
                center.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
                center.GetComponent<Renderer>().sharedMaterial = coralPeachMat;
                center.GetComponent<Collider>().material = physMat;

                for (int i = 0; i < 5; i++)
                {
                    float angle = i * 72f * Mathf.Deg2Rad;
                    GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ray.name = $"Arm_{i}";
                    ray.transform.SetParent(root.transform, false);
                    ray.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.18f, 0f, Mathf.Sin(angle) * 0.18f);
                    ray.transform.localScale = new Vector3(0.10f, 0.08f, 0.20f);
                    ray.transform.localRotation = Quaternion.Euler(0f, -i * 72f, 0f);
                    ray.GetComponent<Renderer>().sharedMaterial = coralPeachMat;
                    ray.GetComponent<Collider>().material = physMat;
                }
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_starfish", "Spiny Starfish", PrizeRarity.Normal, 0.44f, 180);
            }

            // 2. Open Clam with Pearl (Normal, 180 coins)
            {
                GameObject root = new GameObject("Clam with Pearl");
                GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shell.name = "Shell";
                shell.transform.SetParent(root.transform, false);
                shell.transform.localScale = new Vector3(0.40f, 0.18f, 0.36f);
                shell.GetComponent<Renderer>().sharedMaterial = clamVioletMat;
                shell.GetComponent<Collider>().material = physMat;

                GameObject pearl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pearl.name = "Pearl";
                pearl.transform.SetParent(root.transform, false);
                pearl.transform.localPosition = new Vector3(0f, 0.08f, 0f);
                pearl.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                pearl.GetComponent<Renderer>().sharedMaterial = pearlWhiteMat;
                pearl.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_clam_pearl", "Clam with Pearl", PrizeRarity.Normal, 0.48f, 180);
            }

            // 3. Diver Anchor (Normal, 195 coins)
            {
                GameObject root = new GameObject("Diver Anchor");
                GameObject shank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shank.name = "Shank";
                shank.transform.SetParent(root.transform, false);
                shank.transform.localScale = new Vector3(0.10f, 0.40f, 0.10f);
                shank.GetComponent<Renderer>().sharedMaterial = rustedIronMat;
                shank.GetComponent<Collider>().material = physMat;

                GameObject arms = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arms.name = "Flukes";
                arms.transform.SetParent(root.transform, false);
                arms.transform.localPosition = new Vector3(0f, -0.36f, 0f);
                arms.transform.localScale = new Vector3(0.44f, 0.08f, 0.12f);
                arms.GetComponent<Renderer>().sharedMaterial = rustedIronMat;
                arms.GetComponent<Collider>().material = physMat;

                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "AnchorRing";
                ring.transform.SetParent(root.transform, false);
                ring.transform.localPosition = new Vector3(0f, 0.42f, 0f);
                ring.transform.localScale = new Vector3(0.18f, 0.04f, 0.18f);
                ring.GetComponent<Renderer>().sharedMaterial = bronzeMat;
                ring.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_anchor", "Diver Anchor", PrizeRarity.Normal, 0.56f, 195);
            }

            // 4. Sea Turtle (Normal, 195 coins)
            {
                GameObject root = new GameObject("Sea Turtle");
                GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shell.name = "Carapace";
                shell.transform.SetParent(root.transform, false);
                shell.transform.localScale = new Vector3(0.42f, 0.20f, 0.48f);
                shell.GetComponent<Renderer>().sharedMaterial = seaTurtleOliveMat;
                shell.GetComponent<Collider>().material = physMat;

                GameObject flipperL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flipperL.name = "FlipperL";
                flipperL.transform.SetParent(root.transform, false);
                flipperL.transform.localPosition = new Vector3(-0.24f, -0.04f, 0.12f);
                flipperL.transform.localScale = new Vector3(0.18f, 0.03f, 0.12f);
                flipperL.GetComponent<Renderer>().sharedMaterial = seaTurtleOliveMat;
                flipperL.GetComponent<Collider>().material = physMat;

                GameObject flipperR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flipperR.name = "FlipperR";
                flipperR.transform.SetParent(root.transform, false);
                flipperR.transform.localPosition = new Vector3(0.24f, -0.04f, 0.12f);
                flipperR.transform.localScale = new Vector3(0.18f, 0.03f, 0.12f);
                flipperR.GetComponent<Renderer>().sharedMaterial = seaTurtleOliveMat;
                flipperR.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_turtle", "Sea Turtle", PrizeRarity.Normal, 0.50f, 195);
            }

            // 5. Nautilus Shell (Normal, 210 coins)
            {
                GameObject root = new GameObject("Nautilus Shell");
                GameObject spiral = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spiral.name = "Spiral";
                spiral.transform.SetParent(root.transform, false);
                spiral.transform.localScale = new Vector3(0.28f, 0.42f, 0.38f);
                spiral.GetComponent<Renderer>().sharedMaterial = shellCreamMat;
                spiral.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_nautilus", "Nautilus Shell", PrizeRarity.Normal, 0.46f, 210);
            }

            // 6. Pirate Cannon (Rare, 520 coins)
            {
                GameObject root = new GameObject("Pirate Cannon");
                GameObject carriage = GameObject.CreatePrimitive(PrimitiveType.Cube);
                carriage.name = "Carriage";
                carriage.transform.SetParent(root.transform, false);
                carriage.transform.localScale = new Vector3(0.32f, 0.14f, 0.38f);
                carriage.GetComponent<Renderer>().sharedMaterial = chestOakMat;
                carriage.GetComponent<Collider>().material = physMat;

                GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                barrel.name = "BronzeBarrel";
                barrel.transform.SetParent(root.transform, false);
                barrel.transform.localPosition = new Vector3(0f, 0.12f, 0f);
                barrel.transform.localScale = new Vector3(0.16f, 0.36f, 0.16f);
                barrel.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
                barrel.GetComponent<Renderer>().sharedMaterial = bronzeMat;
                barrel.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_cannon", "Pirate Cannon", PrizeRarity.Rare, 0.65f, 520);
            }

            // 7. Megalodon Shark Jaw (Rare, 550 coins)
            {
                GameObject root = new GameObject("Shark Jaw");
                GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arch.name = "Arch";
                arch.transform.SetParent(root.transform, false);
                arch.transform.localScale = new Vector3(0.42f, 0.08f, 0.36f);
                arch.GetComponent<Renderer>().sharedMaterial = sharkToothMat;
                arch.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_shark_jaw", "Shark Jaw", PrizeRarity.Rare, 0.58f, 550);
            }

            // 8. Sunken Treasure Chest (Rare, 600 coins)
            {
                GameObject root = new GameObject("Sunken Treasure Chest");
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = "ChestBox";
                box.transform.SetParent(root.transform, false);
                box.transform.localScale = new Vector3(0.44f, 0.28f, 0.34f);
                box.GetComponent<Renderer>().sharedMaterial = chestOakMat;
                box.GetComponent<Collider>().material = physMat;

                GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lid.name = "DomedLid";
                lid.transform.SetParent(root.transform, false);
                lid.transform.localPosition = new Vector3(0f, 0.16f, 0f);
                lid.transform.localScale = new Vector3(0.34f, 0.22f, 0.34f);
                lid.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                lid.GetComponent<Renderer>().sharedMaterial = bronzeMat;
                lid.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_treasure_chest", "Treasure Chest", PrizeRarity.Rare, 0.68f, 600);
            }

            // 9. Golden Trident of Neptune (Secret, 2,000 coins)
            {
                GameObject root = new GameObject("Golden Trident");
                GameObject staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                staff.name = "Shaft";
                staff.transform.SetParent(root.transform, false);
                staff.transform.localScale = new Vector3(0.08f, 0.44f, 0.08f);
                staff.GetComponent<Renderer>().sharedMaterial = pirateGoldMat;
                staff.GetComponent<Collider>().material = physMat;

                GameObject cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cross.name = "Crossbar";
                cross.transform.SetParent(root.transform, false);
                cross.transform.localPosition = new Vector3(0f, 0.42f, 0f);
                cross.transform.localScale = new Vector3(0.36f, 0.06f, 0.08f);
                cross.GetComponent<Renderer>().sharedMaterial = pirateGoldMat;
                cross.GetComponent<Collider>().material = physMat;

                GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gem.name = "SeaGem";
                gem.transform.SetParent(root.transform, false);
                gem.transform.localPosition = new Vector3(0f, 0.44f, 0.05f);
                gem.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                gem.GetComponent<Renderer>().sharedMaterial = seaGemMat;
                gem.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "ocean_trident", "Golden Trident", PrizeRarity.Secret, 0.78f, 2000);
            }

            return SaveMachineDefinition(MACHINE_PATH, "ocean_abyss", "Ocean Abyss",
                "Dive into abyssal reefs, sunken galleons, and mystical sea relics.",
                3500, 85, 140,
                new Color(0.08f, 0.74f, 0.72f), new Color(0.03f, 0.07f, 0.15f), prizes);
        }

        // ==========================================
        // MASTER GENERATE ALL
        // ==========================================
        [MenuItem("ClawMachine/Generate All 6 Arcade Machines & Catalog")]
        public static void GenerateAllSixMachines()
        {
            ToyBoxAssetGenerator.GenerateToyBox();
            RetroArcadeAssetGenerator.GenerateRetroArcade();
            GenerateCosmicGalaxy();
            GenerateSweetCandy();
            GenerateFantasyDungeon();
            GenerateOceanAbyss();

            // Load all 6 definitions
            MachineDefinition m1 = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_ToyBox.asset");
            MachineDefinition m2 = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_RetroArcade.asset");
            MachineDefinition m3 = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_CosmicGalaxy.asset");
            MachineDefinition m4 = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_SweetCandy.asset");
            MachineDefinition m5 = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_FantasyDungeon.asset");
            MachineDefinition m6 = AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_OceanAbyss.asset");

            MachineDefinition[] allSix = new MachineDefinition[] { m1, m2, m3, m4, m5, m6 };

            string catalogPath = "Assets/Data/MachineCatalog.asset";
            MachineCatalog catalog = AssetDatabase.LoadAssetAtPath<MachineCatalog>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<MachineCatalog>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }
            catalog.SetMachinesInEditor(allSix);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AdditionalMachinesGenerator] Successfully registered all {allSix.Length} machines into MachineCatalog.asset!");
        }

        // ==========================================
        // SHARED HELPERS
        // ==========================================
        private static void EnsureDirs(string prefabDir, string dataDir, string matDir)
        {
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);
        }

        private static Material CreateLitMat(string folder, string name, Color color)
        {
            string path = $"{folder}/{name}.mat";
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

        private static Material CreateMetallicMat(string folder, string name, Color color, float metallic, float smoothness)
        {
            string path = $"{folder}/{name}.mat";
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

        private static PrizeDefinition FinalizePrize(GameObject root, string prefabDir, string dataDir, string id, string name, PrizeRarity rarity, float mass, int coinVal)
        {
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            root.AddComponent<Prize>();

            string prefabPath = $"{prefabDir}/{id}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            GameObject.DestroyImmediate(root);

            string defPath = $"{dataDir}/{id}.asset";
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

        private static MachineDefinition SaveMachineDefinition(string path, string id, string displayName, string desc,
            int unlockCost, int entryCost, int passiveIncome, Color frameColor, Color backdropColor, PrizeDefinition[] prizes)
        {
            MachineDefinition machine = AssetDatabase.LoadAssetAtPath<MachineDefinition>(path);
            if (machine == null)
            {
                machine = ScriptableObject.CreateInstance<MachineDefinition>();
                AssetDatabase.CreateAsset(machine, path);
            }
            machine.machineId = id;
            machine.displayName = displayName;
            machine.description = desc;
            machine.unlockCost = unlockCost;
            machine.entryCost = entryCost;
            machine.passiveIncomePerMinute = passiveIncome;
            machine.cabinetFrameColor = frameColor;
            machine.cabinetBackdropColor = backdropColor;
            machine.prizes = prizes;

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AdditionalMachinesGenerator] Generated {displayName} ({prizes.Length} prizes) successfully!");
            return machine;
        }
    }
}
#endif
