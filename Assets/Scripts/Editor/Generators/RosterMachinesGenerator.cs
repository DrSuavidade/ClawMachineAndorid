#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.Editor
{
    public static class RosterMachinesGenerator
    {
        [MenuItem("ClawMachine/Generate All 4 Future Machines (36 Prizes + Themes)")]
        public static void GenerateAllFourMachines()
        {
            GenerateTravelingCircus();
            GenerateHauntedManor();
            GenerateSteampunkFoundry();
            GenerateNeonCasino();
            RegisterAllTenMachinesInCatalog();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RosterMachinesGenerator] Successfully generated all 4 future machines and updated 10-cabinet catalog!");
        }

        // ==========================================
        // 1. TRAVELING CIRCUS (Machine 7)
        // ==========================================
        [MenuItem("ClawMachine/Generate Machine 7: Traveling Circus")]
        public static MachineDefinition GenerateTravelingCircus()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/TravelingCircus";
            const string DATA_DIR = "Assets/Data/Prizes/TravelingCircus";
            const string MAT_DIR = "Assets/Materials/TravelingCircus";
            const string MACHINE_PATH = "Assets/Data/Machine_TravelingCircus.asset";
            const string THEME_PATH = "Assets/Data/Themes/Theme_Circus_TravelingCircus.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);
            PhysicsMaterial physMat = GetPhysMat("CircusPhysics", 0.65f, 0.75f, 0.20f);

            Material goldMat = CreateMetallicMat(MAT_DIR, "Mat_CircusGold", new Color(0.95f, 0.77f, 0.33f), 0.90f, 0.85f);
            Material velvetRedMat = CreateLitMat(MAT_DIR, "Mat_CircusVelvetRed", new Color(0.55f, 0.08f, 0.14f));
            Material emeraldMat = CreateLitMat(MAT_DIR, "Mat_CircusEmerald", new Color(0.08f, 0.32f, 0.18f));
            Material brassMat = CreateMetallicMat(MAT_DIR, "Mat_CircusBrass", new Color(0.85f, 0.65f, 0.25f), 0.80f, 0.70f);
            Material whiteMat = CreateLitMat(MAT_DIR, "Mat_CircusWhite", new Color(0.96f, 0.95f, 0.92f));
            Material stripedMat = CreateLitMat(MAT_DIR, "Mat_CircusYellow", new Color(0.95f, 0.82f, 0.20f));

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Juggling Ring (Normal)
            {
                GameObject root = new GameObject("Juggling Ring");
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.transform.SetParent(root.transform, false);
                ring.transform.localScale = new Vector3(0.40f, 0.04f, 0.40f);
                ring.GetComponent<Renderer>().sharedMaterial = stripedMat;
                ring.GetComponent<Collider>().material = physMat;
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_juggling_ring", "Juggling Ring", PrizeRarity.Normal, 0.35f, 50);
            }

            // 2. Acrobatic Baton (Normal)
            {
                GameObject root = new GameObject("Acrobatic Baton");
                GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick.transform.SetParent(root.transform, false);
                stick.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
                stick.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                stick.GetComponent<Renderer>().sharedMaterial = whiteMat;
                stick.GetComponent<Collider>().material = physMat;

                GameObject ball1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball1.transform.SetParent(root.transform, false);
                ball1.transform.localPosition = new Vector3(0.24f, 0.24f, 0f);
                ball1.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                ball1.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                ball1.GetComponent<Collider>().material = physMat;

                GameObject ball2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball2.transform.SetParent(root.transform, false);
                ball2.transform.localPosition = new Vector3(-0.24f, -0.24f, 0f);
                ball2.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                ball2.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                ball2.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_acrobatic_baton", "Acrobatic Baton", PrizeRarity.Normal, 0.40f, 50);
            }

            // 3. Brass Showman Horn (Normal)
            {
                GameObject root = new GameObject("Showman Horn");
                GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cone.transform.SetParent(root.transform, false);
                cone.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
                cone.GetComponent<Renderer>().sharedMaterial = brassMat;
                cone.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_brass_horn", "Showman Horn", PrizeRarity.Normal, 0.45f, 50);
            }

            // 4. Carnival Ticket Roll (Normal)
            {
                GameObject root = new GameObject("Ticket Roll");
                GameObject roll = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                roll.transform.SetParent(root.transform, false);
                roll.transform.localScale = new Vector3(0.32f, 0.12f, 0.32f);
                roll.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                roll.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                roll.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_ticket_roll", "Ticket Roll", PrizeRarity.Normal, 0.38f, 50);
            }

            // 5. Clown Bowtie (Normal)
            {
                GameObject root = new GameObject("Clown Bowtie");
                GameObject knot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                knot.transform.SetParent(root.transform, false);
                knot.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                knot.GetComponent<Renderer>().sharedMaterial = stripedMat;
                knot.GetComponent<Collider>().material = physMat;

                GameObject wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wingL.transform.SetParent(root.transform, false);
                wingL.transform.localPosition = new Vector3(-0.16f, 0f, 0f);
                wingL.transform.localScale = new Vector3(0.20f, 0.16f, 0.08f);
                wingL.GetComponent<Renderer>().sharedMaterial = emeraldMat;
                wingL.GetComponent<Collider>().material = physMat;

                GameObject wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wingR.transform.SetParent(root.transform, false);
                wingR.transform.localPosition = new Vector3(0.16f, 0f, 0f);
                wingR.transform.localScale = new Vector3(0.20f, 0.16f, 0.08f);
                wingR.GetComponent<Renderer>().sharedMaterial = emeraldMat;
                wingR.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_clown_bowtie", "Clown Bowtie", PrizeRarity.Normal, 0.36f, 50);
            }

            // 6. Ringmaster Top Hat (Rare)
            {
                GameObject root = new GameObject("Ringmaster Top Hat");
                GameObject brim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                brim.transform.SetParent(root.transform, false);
                brim.transform.localScale = new Vector3(0.42f, 0.03f, 0.42f);
                brim.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                brim.GetComponent<Collider>().material = physMat;

                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crown.transform.SetParent(root.transform, false);
                crown.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                crown.transform.localScale = new Vector3(0.28f, 0.14f, 0.28f);
                crown.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                crown.GetComponent<Collider>().material = physMat;

                GameObject ribbon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ribbon.transform.SetParent(root.transform, false);
                ribbon.transform.localPosition = new Vector3(0f, 0.04f, 0f);
                ribbon.transform.localScale = new Vector3(0.29f, 0.03f, 0.29f);
                ribbon.GetComponent<Renderer>().sharedMaterial = goldMat;
                ribbon.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_ringmaster_hat", "Ringmaster Top Hat", PrizeRarity.Rare, 0.55f, 150);
            }

            // 7. Carousel Golden Horse (Rare)
            {
                GameObject root = new GameObject("Carousel Horse");
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.20f, 0.22f, 0.38f);
                body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                body.GetComponent<Renderer>().sharedMaterial = whiteMat;
                body.GetComponent<Collider>().material = physMat;

                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.SetParent(root.transform, false);
                pole.transform.localScale = new Vector3(0.04f, 0.40f, 0.04f);
                pole.GetComponent<Renderer>().sharedMaterial = goldMat;
                pole.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_carousel_horse", "Carousel Horse", PrizeRarity.Rare, 0.60f, 150);
            }

            // 8. Trapeze Swing Trophy (Rare)
            {
                GameObject root = new GameObject("Trapeze Trophy");
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bar.transform.SetParent(root.transform, false);
                bar.transform.localScale = new Vector3(0.06f, 0.35f, 0.06f);
                bar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                bar.GetComponent<Renderer>().sharedMaterial = brassMat;
                bar.GetComponent<Collider>().material = physMat;

                GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.transform.SetParent(root.transform, false);
                star.transform.localPosition = new Vector3(0f, 0.12f, 0f);
                star.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                star.GetComponent<Renderer>().sharedMaterial = goldMat;
                star.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_trapeze_trophy", "Trapeze Trophy", PrizeRarity.Rare, 0.52f, 150);
            }

            // 9. Grand Marquee Lion (Secret)
            {
                GameObject root = new GameObject("Golden Marquee Lion");
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.38f, 0.35f, 0.40f);
                body.GetComponent<Renderer>().sharedMaterial = goldMat;
                body.GetComponent<Collider>().material = physMat;

                GameObject mane = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mane.transform.SetParent(root.transform, false);
                mane.transform.localPosition = new Vector3(0f, 0.10f, 0.14f);
                mane.transform.localScale = new Vector3(0.44f, 0.44f, 0.32f);
                mane.GetComponent<Renderer>().sharedMaterial = brassMat;
                mane.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "circus_golden_lion", "Golden Marquee Lion", PrizeRarity.Secret, 0.70f, 500);
            }

            CabinetTheme theme = CreateOrLoadTheme(THEME_PATH, "circus_wonder", "Grand Traveling Circus",
                new Color(0.43f, 0.09f, 0.14f), new Color(0.07f, 0.16f, 0.12f), new Color(0.95f, 0.77f, 0.33f), new Color(0.09f, 0.07f, 0.07f));

            return SaveMachineDefinition(MACHINE_PATH, "traveling_circus", "Traveling Circus",
                "Grand Victorian wonder circus cabinet draped in royal burgundy velvet and polished brass.",
                10000, 0, 45, new Color(0.55f, 0.12f, 0.16f), new Color(0.12f, 0.22f, 0.16f), prizes, theme);
        }

        // ==========================================
        // 2. HAUNTED MANOR (Machine 8)
        // ==========================================
        [MenuItem("ClawMachine/Generate Machine 8: Haunted Manor")]
        public static MachineDefinition GenerateHauntedManor()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/HauntedManor";
            const string DATA_DIR = "Assets/Data/Prizes/HauntedManor";
            const string MAT_DIR = "Assets/Materials/HauntedManor";
            const string MACHINE_PATH = "Assets/Data/Machine_HauntedManor.asset";
            const string THEME_PATH = "Assets/Data/Themes/Theme_Haunted_Manor.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);
            PhysicsMaterial physMat = GetPhysMat("HauntedPhysics", 0.60f, 0.70f, 0.25f);

            Material ghostGreenMat = CreateLitMat(MAT_DIR, "Mat_GhostGreen", new Color(0.33f, 0.92f, 0.55f));
            Material occultPurpleMat = CreateLitMat(MAT_DIR, "Mat_OccultPurple", new Color(0.35f, 0.14f, 0.45f));
            Material silverMat = CreateMetallicMat(MAT_DIR, "Mat_TarnishedSilver", new Color(0.72f, 0.77f, 0.84f), 0.85f, 0.70f);
            Material ebonyMat = CreateLitMat(MAT_DIR, "Mat_EbonyWood", new Color(0.12f, 0.11f, 0.14f));
            Material boneMat = CreateLitMat(MAT_DIR, "Mat_AncientBone", new Color(0.90f, 0.88f, 0.80f));
            Material candlelightMat = CreateLitMat(MAT_DIR, "Mat_CandleFlame", new Color(1.0f, 0.75f, 0.25f));

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Séance Candle (Normal)
            {
                GameObject root = new GameObject("Séance Candle");
                GameObject wax = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wax.transform.SetParent(root.transform, false);
                wax.transform.localScale = new Vector3(0.14f, 0.25f, 0.14f);
                wax.GetComponent<Renderer>().sharedMaterial = boneMat;
                wax.GetComponent<Collider>().material = physMat;

                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.transform.SetParent(root.transform, false);
                flame.transform.localPosition = new Vector3(0f, 0.28f, 0f);
                flame.transform.localScale = new Vector3(0.08f, 0.14f, 0.08f);
                flame.GetComponent<Renderer>().sharedMaterial = candlelightMat;
                flame.GetComponent<Collider>().material = physMat;
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_seance_candle", "Séance Candle", PrizeRarity.Normal, 0.35f, 50);
            }

            // 2. Ouija Planchette (Normal)
            {
                GameObject root = new GameObject("Spirit Planchette");
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.28f, 0.06f, 0.36f);
                body.GetComponent<Renderer>().sharedMaterial = ebonyMat;
                body.GetComponent<Collider>().material = physMat;

                GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lens.transform.SetParent(root.transform, false);
                lens.transform.localScale = new Vector3(0.12f, 0.07f, 0.12f);
                lens.GetComponent<Renderer>().sharedMaterial = ghostGreenMat;
                lens.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_planchette", "Spirit Planchette", PrizeRarity.Normal, 0.38f, 50);
            }

            // 3. Antique Skeleton Key (Normal)
            {
                GameObject root = new GameObject("Skeleton Key");
                GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shaft.transform.SetParent(root.transform, false);
                shaft.transform.localScale = new Vector3(0.06f, 0.30f, 0.06f);
                shaft.GetComponent<Renderer>().sharedMaterial = silverMat;
                shaft.GetComponent<Collider>().material = physMat;

                GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bow.transform.SetParent(root.transform, false);
                bow.transform.localPosition = new Vector3(0f, 0.30f, 0f);
                bow.transform.localScale = new Vector3(0.16f, 0.16f, 0.06f);
                bow.GetComponent<Renderer>().sharedMaterial = silverMat;
                bow.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_skeleton_key", "Skeleton Key", PrizeRarity.Normal, 0.40f, 50);
            }

            // 4. Ectoplasm Vial (Normal)
            {
                GameObject root = new GameObject("Ectoplasm Vial");
                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                glass.transform.SetParent(root.transform, false);
                glass.transform.localScale = new Vector3(0.16f, 0.22f, 0.16f);
                glass.GetComponent<Renderer>().sharedMaterial = ghostGreenMat;
                glass.GetComponent<Collider>().material = physMat;

                GameObject cork = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cork.transform.SetParent(root.transform, false);
                cork.transform.localPosition = new Vector3(0f, 0.24f, 0f);
                cork.transform.localScale = new Vector3(0.12f, 0.05f, 0.12f);
                cork.GetComponent<Renderer>().sharedMaterial = ebonyMat;
                cork.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_ectoplasm_vial", "Ectoplasm Vial", PrizeRarity.Normal, 0.42f, 50);
            }

            // 5. Tarot Card Deck (Normal)
            {
                GameObject root = new GameObject("Tarot Deck");
                GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
                deck.transform.SetParent(root.transform, false);
                deck.transform.localScale = new Vector3(0.24f, 0.12f, 0.36f);
                deck.GetComponent<Renderer>().sharedMaterial = occultPurpleMat;
                deck.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_tarot_deck", "Tarot Deck", PrizeRarity.Normal, 0.45f, 50);
            }

            // 6. Crystal Ball Orb (Rare)
            {
                GameObject root = new GameObject("Clairvoyant Orb");
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.transform.SetParent(root.transform, false);
                orb.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                orb.transform.localScale = new Vector3(0.34f, 0.34f, 0.34f);
                orb.GetComponent<Renderer>().sharedMaterial = ghostGreenMat;
                orb.GetComponent<Collider>().material = physMat;

                GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stand.transform.SetParent(root.transform, false);
                stand.transform.localScale = new Vector3(0.30f, 0.06f, 0.30f);
                stand.GetComponent<Renderer>().sharedMaterial = silverMat;
                stand.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_crystal_ball", "Clairvoyant Orb", PrizeRarity.Rare, 0.58f, 150);
            }

            // 7. Gargoyle Idol (Rare)
            {
                GameObject root = new GameObject("Stone Gargoyle");
                GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
                torso.transform.SetParent(root.transform, false);
                torso.transform.localScale = new Vector3(0.24f, 0.32f, 0.20f);
                torso.GetComponent<Renderer>().sharedMaterial = ebonyMat;
                torso.GetComponent<Collider>().material = physMat;

                GameObject wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wings.transform.SetParent(root.transform, false);
                wings.transform.localPosition = new Vector3(0f, 0.08f, -0.12f);
                wings.transform.localScale = new Vector3(0.44f, 0.20f, 0.06f);
                wings.GetComponent<Renderer>().sharedMaterial = ebonyMat;
                wings.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_gargoyle", "Stone Gargoyle", PrizeRarity.Rare, 0.62f, 150);
            }

            // 8. Cursed Pocket Mirror (Rare)
            {
                GameObject root = new GameObject("Cursed Mirror");
                GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                frame.transform.SetParent(root.transform, false);
                frame.transform.localScale = new Vector3(0.32f, 0.04f, 0.38f);
                frame.GetComponent<Renderer>().sharedMaterial = silverMat;
                frame.GetComponent<Collider>().material = physMat;

                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                glass.transform.SetParent(root.transform, false);
                glass.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                glass.transform.localScale = new Vector3(0.26f, 0.03f, 0.32f);
                glass.GetComponent<Renderer>().sharedMaterial = occultPurpleMat;
                glass.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_pocket_mirror", "Cursed Mirror", PrizeRarity.Rare, 0.50f, 150);
            }

            // 9. Ghostly Phantom Skull (Secret)
            {
                GameObject root = new GameObject("Phantom Skull");
                GameObject cranium = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cranium.transform.SetParent(root.transform, false);
                cranium.transform.localPosition = new Vector3(0f, 0.10f, 0f);
                cranium.transform.localScale = new Vector3(0.36f, 0.38f, 0.36f);
                cranium.GetComponent<Renderer>().sharedMaterial = boneMat;
                cranium.GetComponent<Collider>().material = physMat;

                GameObject jaw = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jaw.transform.SetParent(root.transform, false);
                jaw.transform.localPosition = new Vector3(0f, -0.10f, 0.08f);
                jaw.transform.localScale = new Vector3(0.22f, 0.16f, 0.20f);
                jaw.GetComponent<Renderer>().sharedMaterial = boneMat;
                jaw.GetComponent<Collider>().material = physMat;

                GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeL.transform.SetParent(root.transform, false);
                eyeL.transform.localPosition = new Vector3(-0.09f, 0.08f, 0.16f);
                eyeL.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                eyeL.GetComponent<Renderer>().sharedMaterial = ghostGreenMat;
                eyeL.GetComponent<Collider>().material = physMat;

                GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeR.transform.SetParent(root.transform, false);
                eyeR.transform.localPosition = new Vector3(0.09f, 0.08f, 0.16f);
                eyeR.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                eyeR.GetComponent<Renderer>().sharedMaterial = ghostGreenMat;
                eyeR.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "haunted_phantom_skull", "Phantom Skull", PrizeRarity.Secret, 0.65f, 500);
            }

            CabinetTheme theme = CreateOrLoadTheme(THEME_PATH, "haunted_parlor", "Spirit Medium Parlor",
                new Color(0.18f, 0.12f, 0.23f), new Color(0.10f, 0.10f, 0.14f), new Color(0.33f, 0.92f, 0.55f), new Color(0.06f, 0.06f, 0.08f));

            return SaveMachineDefinition(MACHINE_PATH, "haunted_manor", "Haunted Manor",
                "Gothic séance parlor cabinet illuminated by eerie green ectoplasm and candlelit spirits.",
                14000, 0, 60, new Color(0.24f, 0.16f, 0.32f), new Color(0.08f, 0.14f, 0.10f), prizes, theme);
        }

        // ==========================================
        // 3. STEAMPUNK FOUNDRY (Machine 9)
        // ==========================================
        [MenuItem("ClawMachine/Generate Machine 9: Steampunk Foundry")]
        public static MachineDefinition GenerateSteampunkFoundry()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/SteampunkFoundry";
            const string DATA_DIR = "Assets/Data/Prizes/SteampunkFoundry";
            const string MAT_DIR = "Assets/Materials/SteampunkFoundry";
            const string MACHINE_PATH = "Assets/Data/Machine_SteampunkFoundry.asset";
            const string THEME_PATH = "Assets/Data/Themes/Theme_Steampunk_Foundry.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);
            PhysicsMaterial physMat = GetPhysMat("SteampunkPhysics", 0.70f, 0.80f, 0.15f);

            Material copperMat = CreateMetallicMat(MAT_DIR, "Mat_SteamCopper", new Color(0.85f, 0.50f, 0.30f), 0.85f, 0.75f);
            Material brassMat = CreateMetallicMat(MAT_DIR, "Mat_SteamBrass", new Color(0.88f, 0.72f, 0.35f), 0.90f, 0.80f);
            Material ironMat = CreateLitMat(MAT_DIR, "Mat_SteamIron", new Color(0.20f, 0.20f, 0.22f));
            Material gaugeWhiteMat = CreateLitMat(MAT_DIR, "Mat_GaugeWhite", new Color(0.92f, 0.90f, 0.84f));
            Material amberGlassMat = CreateLitMat(MAT_DIR, "Mat_SteamAmber", new Color(0.95f, 0.60f, 0.15f));

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Brass Gear Cog (Normal)
            {
                GameObject root = new GameObject("Brass Cog");
                GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hub.transform.SetParent(root.transform, false);
                hub.transform.localScale = new Vector3(0.35f, 0.08f, 0.35f);
                hub.GetComponent<Renderer>().sharedMaterial = brassMat;
                hub.GetComponent<Collider>().material = physMat;
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_brass_cog", "Brass Cog", PrizeRarity.Normal, 0.45f, 50);
            }

            // 2. Pressure Gauge (Normal)
            {
                GameObject root = new GameObject("Pressure Meter");
                GameObject caseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                caseObj.transform.SetParent(root.transform, false);
                caseObj.transform.localScale = new Vector3(0.30f, 0.10f, 0.30f);
                caseObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                caseObj.GetComponent<Renderer>().sharedMaterial = copperMat;
                caseObj.GetComponent<Collider>().material = physMat;

                GameObject dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dial.transform.SetParent(root.transform, false);
                dial.transform.localPosition = new Vector3(0f, 0f, 0.06f);
                dial.transform.localScale = new Vector3(0.24f, 0.03f, 0.24f);
                dial.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                dial.GetComponent<Renderer>().sharedMaterial = gaugeWhiteMat;
                dial.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_pressure_meter", "Pressure Meter", PrizeRarity.Normal, 0.42f, 50);
            }

            // 3. Steam Valve Handle (Normal)
            {
                GameObject root = new GameObject("Valve Handle");
                GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rim.transform.SetParent(root.transform, false);
                rim.transform.localScale = new Vector3(0.34f, 0.05f, 0.34f);
                rim.GetComponent<Renderer>().sharedMaterial = ironMat;
                rim.GetComponent<Collider>().material = physMat;

                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.transform.SetParent(root.transform, false);
                stem.transform.localPosition = new Vector3(0f, -0.12f, 0f);
                stem.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
                stem.GetComponent<Renderer>().sharedMaterial = brassMat;
                stem.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_valve_handle", "Valve Handle", PrizeRarity.Normal, 0.46f, 50);
            }

            // 4. Copper Boiler Canister (Normal)
            {
                GameObject root = new GameObject("Boiler Canister");
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                tank.transform.SetParent(root.transform, false);
                tank.transform.localScale = new Vector3(0.22f, 0.28f, 0.22f);
                tank.GetComponent<Renderer>().sharedMaterial = copperMat;
                tank.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_boiler_canister", "Boiler Canister", PrizeRarity.Normal, 0.48f, 50);
            }

            // 5. Aviator Goggles (Normal)
            {
                GameObject root = new GameObject("Aviator Goggles");
                GameObject lensL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lensL.transform.SetParent(root.transform, false);
                lensL.transform.localPosition = new Vector3(-0.14f, 0f, 0f);
                lensL.transform.localScale = new Vector3(0.16f, 0.06f, 0.16f);
                lensL.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                lensL.GetComponent<Renderer>().sharedMaterial = brassMat;
                lensL.GetComponent<Collider>().material = physMat;

                GameObject lensR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lensR.transform.SetParent(root.transform, false);
                lensR.transform.localPosition = new Vector3(0.14f, 0f, 0f);
                lensR.transform.localScale = new Vector3(0.16f, 0.06f, 0.16f);
                lensR.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                lensR.GetComponent<Renderer>().sharedMaterial = brassMat;
                lensR.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_aviator_goggles", "Aviator Goggles", PrizeRarity.Normal, 0.38f, 50);
            }

            // 6. Clockwork Pocket Watch (Rare)
            {
                GameObject root = new GameObject("Pocket Watch");
                GameObject caseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                caseObj.transform.SetParent(root.transform, false);
                caseObj.transform.localScale = new Vector3(0.32f, 0.07f, 0.32f);
                caseObj.GetComponent<Renderer>().sharedMaterial = goldMatOr(MAT_DIR, "Mat_SteamGold", new Color(0.95f, 0.80f, 0.25f));
                caseObj.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_pocket_watch", "Pocket Watch", PrizeRarity.Rare, 0.52f, 150);
            }

            // 7. Mini Airship Zeppelin (Rare)
            {
                GameObject root = new GameObject("Mini Zeppelin");
                GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                hull.transform.SetParent(root.transform, false);
                hull.transform.localScale = new Vector3(0.24f, 0.42f, 0.24f);
                hull.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                hull.GetComponent<Renderer>().sharedMaterial = copperMat;
                hull.GetComponent<Collider>().material = physMat;

                GameObject gondola = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gondola.transform.SetParent(root.transform, false);
                gondola.transform.localPosition = new Vector3(0f, -0.16f, 0f);
                gondola.transform.localScale = new Vector3(0.12f, 0.08f, 0.22f);
                gondola.GetComponent<Renderer>().sharedMaterial = ironMat;
                gondola.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_mini_zeppelin", "Mini Zeppelin", PrizeRarity.Rare, 0.56f, 150);
            }

            // 8. Vacuum Tube Lamp (Rare)
            {
                GameObject root = new GameObject("Vacuum Tube");
                GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bulb.transform.SetParent(root.transform, false);
                bulb.transform.localScale = new Vector3(0.18f, 0.32f, 0.18f);
                bulb.GetComponent<Renderer>().sharedMaterial = amberGlassMat;
                bulb.GetComponent<Collider>().material = physMat;

                GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseObj.transform.SetParent(root.transform, false);
                baseObj.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                baseObj.transform.localScale = new Vector3(0.22f, 0.08f, 0.22f);
                baseObj.GetComponent<Renderer>().sharedMaterial = brassMat;
                baseObj.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_vacuum_tube", "Vacuum Tube", PrizeRarity.Rare, 0.50f, 150);
            }

            // 9. Clockwork Automaton Head (Secret)
            {
                GameObject root = new GameObject("Clockwork Automaton");
                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
                head.transform.SetParent(root.transform, false);
                head.transform.localScale = new Vector3(0.32f, 0.35f, 0.32f);
                head.GetComponent<Renderer>().sharedMaterial = brassMat;
                head.GetComponent<Collider>().material = physMat;

                GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                eyeL.transform.SetParent(root.transform, false);
                eyeL.transform.localPosition = new Vector3(-0.08f, 0.06f, 0.17f);
                eyeL.transform.localScale = new Vector3(0.08f, 0.04f, 0.08f);
                eyeL.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                eyeL.GetComponent<Renderer>().sharedMaterial = copperMat;
                eyeL.GetComponent<Collider>().material = physMat;

                GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                eyeR.transform.SetParent(root.transform, false);
                eyeR.transform.localPosition = new Vector3(0.08f, 0.06f, 0.17f);
                eyeR.transform.localScale = new Vector3(0.08f, 0.04f, 0.08f);
                eyeR.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                eyeR.GetComponent<Renderer>().sharedMaterial = copperMat;
                eyeR.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "steam_automaton_head", "Clockwork Automaton", PrizeRarity.Secret, 0.72f, 500);
            }

            CabinetTheme theme = CreateOrLoadTheme(THEME_PATH, "steampunk_foundry", "Clockwork Mechanica",
                new Color(0.35f, 0.22f, 0.15f), new Color(0.14f, 0.13f, 0.12f), new Color(0.90f, 0.55f, 0.20f), new Color(0.08f, 0.07f, 0.07f));

            return SaveMachineDefinition(MACHINE_PATH, "steampunk_foundry", "Steampunk Foundry",
                "Industrial boiler-plate mechanical cabinet driven by visible gears, pressure valves, and steam.",
                18000, 0, 75, new Color(0.45f, 0.28f, 0.18f), new Color(0.18f, 0.16f, 0.14f), prizes, theme);
        }

        // ==========================================
        // 4. NEON CASINO (Machine 10)
        // ==========================================
        [MenuItem("ClawMachine/Generate Machine 10: Neon Casino")]
        public static MachineDefinition GenerateNeonCasino()
        {
            const string PREFAB_DIR = "Assets/Prefabs/Prizes/NeonCasino";
            const string DATA_DIR = "Assets/Data/Prizes/NeonCasino";
            const string MAT_DIR = "Assets/Materials/NeonCasino";
            const string MACHINE_PATH = "Assets/Data/Machine_NeonCasino.asset";
            const string THEME_PATH = "Assets/Data/Themes/Theme_Neon_Casino.asset";

            EnsureDirs(PREFAB_DIR, DATA_DIR, MAT_DIR);
            PhysicsMaterial physMat = GetPhysMat("CasinoPhysics", 0.65f, 0.75f, 0.22f);

            Material goldMat = CreateMetallicMat(MAT_DIR, "Mat_CasinoGold", new Color(1.0f, 0.84f, 0.0f), 0.95f, 0.90f);
            Material velvetRedMat = CreateLitMat(MAT_DIR, "Mat_CasinoRed", new Color(0.85f, 0.08f, 0.18f));
            Material feltGreenMat = CreateLitMat(MAT_DIR, "Mat_CasinoGreen", new Color(0.08f, 0.45f, 0.22f));
            Material lacquerBlackMat = CreateLitMat(MAT_DIR, "Mat_CasinoBlack", new Color(0.08f, 0.06f, 0.08f));
            Material pearlWhiteMat = CreateLitMat(MAT_DIR, "Mat_CasinoWhite", new Color(0.96f, 0.96f, 0.98f));
            Material neonBlueMat = CreateLitMat(MAT_DIR, "Mat_CasinoBlue", new Color(0.15f, 0.65f, 1.0f));

            PrizeDefinition[] prizes = new PrizeDefinition[9];

            // 1. Casino Chip Stack (Normal)
            {
                GameObject root = new GameObject("Casino Chip Stack");
                GameObject chip1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                chip1.transform.SetParent(root.transform, false);
                chip1.transform.localScale = new Vector3(0.32f, 0.04f, 0.32f);
                chip1.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                chip1.GetComponent<Collider>().material = physMat;

                GameObject chip2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                chip2.transform.SetParent(root.transform, false);
                chip2.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                chip2.transform.localScale = new Vector3(0.32f, 0.04f, 0.32f);
                chip2.GetComponent<Renderer>().sharedMaterial = pearlWhiteMat;
                chip2.GetComponent<Collider>().material = physMat;

                GameObject chip3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                chip3.transform.SetParent(root.transform, false);
                chip3.transform.localPosition = new Vector3(0f, 0.10f, 0f);
                chip3.transform.localScale = new Vector3(0.32f, 0.04f, 0.32f);
                chip3.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                chip3.GetComponent<Collider>().material = physMat;
                prizes[0] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_chip_stack", "Casino Chip Stack", PrizeRarity.Normal, 0.40f, 50);
            }

            // 2. Pair of Red Dice (Normal)
            {
                GameObject root = new GameObject("Lucky Dice");
                GameObject die1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                die1.transform.SetParent(root.transform, false);
                die1.transform.localPosition = new Vector3(-0.11f, 0f, 0f);
                die1.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                die1.transform.localRotation = Quaternion.Euler(15f, 25f, 0f);
                die1.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                die1.GetComponent<Collider>().material = physMat;

                GameObject die2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                die2.transform.SetParent(root.transform, false);
                die2.transform.localPosition = new Vector3(0.11f, 0f, 0f);
                die2.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                die2.transform.localRotation = Quaternion.Euler(-20f, 15f, 30f);
                die2.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                die2.GetComponent<Collider>().material = physMat;
                prizes[1] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_lucky_dice", "Lucky Dice", PrizeRarity.Normal, 0.38f, 50);
            }

            // 3. Playing Card Case (Normal)
            {
                GameObject root = new GameObject("Playing Card Case");
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.transform.SetParent(root.transform, false);
                box.transform.localScale = new Vector3(0.24f, 0.35f, 0.12f);
                box.GetComponent<Renderer>().sharedMaterial = lacquerBlackMat;
                box.GetComponent<Collider>().material = physMat;
                prizes[2] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_card_case", "Playing Card Case", PrizeRarity.Normal, 0.42f, 50);
            }

            // 4. Roulette Ball Token (Normal)
            {
                GameObject root = new GameObject("Roulette Marker");
                GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                disk.transform.SetParent(root.transform, false);
                disk.transform.localScale = new Vector3(0.30f, 0.08f, 0.30f);
                disk.GetComponent<Renderer>().sharedMaterial = feltGreenMat;
                disk.GetComponent<Collider>().material = physMat;

                GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.transform.SetParent(root.transform, false);
                ball.transform.localPosition = new Vector3(0f, 0.10f, 0f);
                ball.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
                ball.GetComponent<Renderer>().sharedMaterial = pearlWhiteMat;
                ball.GetComponent<Collider>().material = physMat;
                prizes[3] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_roulette_marker", "Roulette Marker", PrizeRarity.Normal, 0.40f, 50);
            }

            // 5. Martini Cocktail Token (Normal)
            {
                GameObject root = new GameObject("Cocktail Glass");
                GameObject cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cup.transform.SetParent(root.transform, false);
                cup.transform.localScale = new Vector3(0.24f, 0.14f, 0.24f);
                cup.GetComponent<Renderer>().sharedMaterial = neonBlueMat;
                cup.GetComponent<Collider>().material = physMat;

                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.transform.SetParent(root.transform, false);
                stem.transform.localPosition = new Vector3(0f, -0.14f, 0f);
                stem.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);
                stem.GetComponent<Renderer>().sharedMaterial = pearlWhiteMat;
                stem.GetComponent<Collider>().material = physMat;
                prizes[4] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_cocktail_token", "Cocktail Glass", PrizeRarity.Normal, 0.36f, 50);
            }

            // 6. Gold Bar Ingot (Rare)
            {
                GameObject root = new GameObject("Gold Bar Ingot");
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.transform.SetParent(root.transform, false);
                bar.transform.localScale = new Vector3(0.38f, 0.14f, 0.20f);
                bar.GetComponent<Renderer>().sharedMaterial = goldMat;
                bar.GetComponent<Collider>().material = physMat;
                prizes[5] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_gold_bar", "Gold Bar Ingot", PrizeRarity.Rare, 0.62f, 150);
            }

            // 7. VIP Diamond Token (Rare)
            {
                GameObject root = new GameObject("VIP Diamond");
                GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gem.transform.SetParent(root.transform, false);
                gem.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
                gem.GetComponent<Renderer>().sharedMaterial = pearlWhiteMat;
                gem.GetComponent<Collider>().material = physMat;

                GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stand.transform.SetParent(root.transform, false);
                stand.transform.localPosition = new Vector3(0f, -0.12f, 0f);
                stand.transform.localScale = new Vector3(0.26f, 0.05f, 0.26f);
                stand.GetComponent<Renderer>().sharedMaterial = goldMat;
                stand.GetComponent<Collider>().material = physMat;
                prizes[6] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_vip_diamond", "VIP Diamond", PrizeRarity.Rare, 0.58f, 150);
            }

            // 8. 777 Jackpot Bar (Rare)
            {
                GameObject root = new GameObject("Jackpot Triple Seven");
                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.transform.SetParent(root.transform, false);
                plate.transform.localScale = new Vector3(0.36f, 0.20f, 0.12f);
                plate.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                plate.GetComponent<Collider>().material = physMat;

                GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rim.transform.SetParent(root.transform, false);
                rim.transform.localPosition = new Vector3(0f, 0f, 0.07f);
                rim.transform.localScale = new Vector3(0.32f, 0.16f, 0.04f);
                rim.GetComponent<Renderer>().sharedMaterial = goldMat;
                rim.GetComponent<Collider>().material = physMat;
                prizes[7] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_jackpot_777", "Jackpot Triple Seven", PrizeRarity.Rare, 0.54f, 150);
            }

            // 9. Sovereign High Roller Crown (Secret)
            {
                GameObject root = new GameObject("High Roller Crown");
                GameObject baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseRing.transform.SetParent(root.transform, false);
                baseRing.transform.localScale = new Vector3(0.38f, 0.08f, 0.38f);
                baseRing.GetComponent<Renderer>().sharedMaterial = goldMat;
                baseRing.GetComponent<Collider>().material = physMat;

                GameObject jewel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                jewel.transform.SetParent(root.transform, false);
                jewel.transform.localPosition = new Vector3(0f, 0.12f, 0f);
                jewel.transform.localScale = new Vector3(0.20f, 0.20f, 0.20f);
                jewel.GetComponent<Renderer>().sharedMaterial = velvetRedMat;
                jewel.GetComponent<Collider>().material = physMat;
                prizes[8] = FinalizePrize(root, PREFAB_DIR, DATA_DIR, "casino_high_roller_crown", "High Roller Crown", PrizeRarity.Secret, 0.70f, 500);
            }

            CabinetTheme theme = CreateOrLoadTheme(THEME_PATH, "neon_casino", "Vegas High Roller",
                new Color(0.10f, 0.04f, 0.06f), new Color(0.18f, 0.06f, 0.12f), new Color(1.0f, 0.84f, 0.0f), new Color(0.05f, 0.03f, 0.04f));

            return SaveMachineDefinition(MACHINE_PATH, "neon_casino", "Neon Casino",
                "High-roller 1970s Vegas casino cabinet gleaming in mirror lacquer, sovereign gold, and jackpot lights.",
                25000, 0, 100, new Color(0.20f, 0.05f, 0.08f), new Color(0.08f, 0.18f, 0.10f), prizes, theme);
        }

        public static void RegisterAllTenMachinesInCatalog()
        {
            string[] paths = new string[]
            {
                "Assets/Data/Machine_ToyBox.asset",
                "Assets/Data/Machine_RetroArcade.asset",
                "Assets/Data/Machine_SweetCandy.asset",
                "Assets/Data/Machine_OceanAbyss.asset",
                "Assets/Data/Machine_CosmicGalaxy.asset",
                "Assets/Data/Machine_FantasyDungeon.asset",
                "Assets/Data/Machine_TravelingCircus.asset",
                "Assets/Data/Machine_HauntedManor.asset",
                "Assets/Data/Machine_SteampunkFoundry.asset",
                "Assets/Data/Machine_NeonCasino.asset"
            };

            MachineDefinition[] allTen = new MachineDefinition[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                allTen[i] = AssetDatabase.LoadAssetAtPath<MachineDefinition>(paths[i]);
            }

            string catalogPath = "Assets/Data/MachineCatalog.asset";
            MachineCatalog catalog = AssetDatabase.LoadAssetAtPath<MachineCatalog>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<MachineCatalog>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }
            catalog.SetMachinesInEditor(allTen);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RosterMachinesGenerator] Successfully updated MachineCatalog.asset with {allTen.Length} machines!");
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private static void EnsureDirs(string p, string d, string m)
        {
            if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            if (!Directory.Exists(m)) Directory.CreateDirectory(m);
        }

        private static PhysicsMaterial GetPhysMat(string name, float df, float sf, float b)
        {
            return new PhysicsMaterial(name)
            {
                dynamicFriction = df,
                staticFriction = sf,
                bounciness = b,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };
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

        private static Material goldMatOr(string folder, string name, Color color)
        {
            return CreateMetallicMat(folder, name, color, 0.90f, 0.85f);
        }

        private static CabinetTheme CreateOrLoadTheme(string path, string id, string name, Color p, Color s, Color a, Color surf)
        {
            CabinetTheme theme = AssetDatabase.LoadAssetAtPath<CabinetTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<CabinetTheme>();
                theme.themeId = id;
                theme.displayName = name;
                theme.primaryColor = p;
                theme.secondaryColor = s;
                theme.accentColor = a;
                theme.surfaceColor = surf;
                theme.textColorHeader = Color.white;
                theme.textColorBody = new Color(0.85f, 0.88f, 0.92f);
                AssetDatabase.CreateAsset(theme, path);
            }
            return theme;
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
            int unlockCost, int entryCost, int passiveIncome, Color frameColor, Color backdropColor, PrizeDefinition[] prizes, CabinetTheme theme)
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
            machine.theme = theme;

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RosterMachinesGenerator] Saved {displayName} with {prizes.Length} prizes!");
            return machine;
        }
    }
}
#endif
