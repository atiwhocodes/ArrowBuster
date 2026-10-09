using UnityEditor;
using UnityEngine;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// Builds the library prefabs (04 §15 catalogue, 01 §8 naming) from the procedural art kit. Root scale is
    /// always 1 so embedded arrows never inherit distortion; sizes are baked into meshes and colliders.
    /// </summary>
    public static class PrefabFactory
    {
        public const string Root = "Assets/_Project/Prefabs";

        public sealed class Profiles
        {
            public MaterialProfile Straw, Timber, Stone, Ice, Metal, Earth;
        }

        // ------------------------------------------------------------------ helpers

        private static GameObject NewObject(string name, int layer)
        {
            var go = new GameObject(name) { layer = layer };
            return go;
        }

        private static GameObject Visual(Transform parent, string name, Mesh mesh, params Material[] materials)
        {
            var go = new GameObject(name) { layer = parent.gameObject.layer };
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            return go;
        }

        private static GameObject Decor(Transform parent, string name, Mesh mesh, Vector3 position, Vector3 euler, Vector3 scale, params Material[] materials)
        {
            GameObject go = Visual(parent, name, mesh, materials);
            go.layer = 0;
            go.transform.localPosition = position;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        public static GameObject Save(GameObject go, string folder)
        {
            string dir = $"{Root}/{folder}";
            System.IO.Directory.CreateDirectory(dir);
            string path = $"{dir}/{go.name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void Dynamic(GameObject go, MaterialProfile profile, Vector2 colliderSize, Vector3 colliderCenter = default)
        {
            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(colliderSize.x, colliderSize.y, 0.96f);
            collider.center = colliderCenter;
            var body = go.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            go.AddComponent<PlanarBody>();
            go.AddComponent<MaterialBody>().SetProfile(profile);
        }

        private static Breakable Breakable(GameObject go, float hp = 0f, float breakImpulse = 0f, float threshold = 0f)
        {
            var breakable = go.AddComponent<Breakable>();
            breakable.Configure(hp, breakImpulse, threshold);
            go.AddComponent<ImpactDamage>();
            return breakable;
        }

        private static void Static(GameObject go, MaterialProfile profile, Vector3 size, Vector3 center)
        {
            var collider = go.AddComponent<BoxCollider>();
            collider.size = size;
            collider.center = center;
            go.AddComponent<MaterialBody>().SetProfile(profile);
            go.isStatic = true;
        }

        // ------------------------------------------------------------------ structures

        public static GameObject Crate(string name, Vector2 size, Profiles p)
        {
            GameObject go = NewObject(name, PhysicsLayers.Structure);
            Visual(go.transform, "Mesh", ArtKit.Crate(name, size), ArtKit.Lit("Timber", ArtKit.Timber), ArtKit.Lit("TimberDark", ArtKit.TimberDark));
            Dynamic(go, p.Timber, size);
            Breakable(go);
            return Save(go, "Structures");
        }

        /// <summary>Timber post/plank/pole. <paramref name="hpOverride"/> &gt; 0 marks a deliberate weak point (snaps on one solid hit).</summary>
        public static GameObject TimberPiece(string name, Vector2 size, Profiles p, float hpOverride = 0f)
        {
            GameObject go = NewObject(name, PhysicsLayers.Structure);
            Visual(go.transform, "Mesh", ArtKit.Box(name, new Vector3(size.x, size.y, 0.8f), 0.05f), ArtKit.Lit("TimberLight", ArtKit.TimberLight));
            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(size.x, size.y, 0.8f);
            go.AddComponent<Rigidbody>();
            go.AddComponent<PlanarBody>();
            go.AddComponent<MaterialBody>().SetProfile(p.Timber);
            Breakable(go, hp: hpOverride);
            return Save(go, "Structures");
        }

        public static GameObject StoneBlock(string name, Vector2 size, Profiles p)
        {
            GameObject go = NewObject(name, PhysicsLayers.Structure);
            Visual(go.transform, "Mesh", ArtKit.Box(name, new Vector3(size.x, size.y, 0.96f), 0.09f), ArtKit.Lit("Stone", ArtKit.Stone));
            Dynamic(go, p.Stone, size);
            Breakable(go);
            return Save(go, "Structures");
        }

        // ------------------------------------------------------------------ objectives / protected

        public static GameObject CrestTarget(Profiles p)
        {
            GameObject go = NewObject("Obj_CrestTarget", PhysicsLayers.Objective);
            Visual(go.transform, "Mesh", ArtKit.Crest("Obj_CrestTarget"),
                ArtKit.Lit("CrestRed", ArtKit.CrestRed, 0.25f), ArtKit.Lit("CrestCream", ArtKit.CrestCream, 0.2f), ArtKit.Lit("TimberDark", ArtKit.TimberDark));
            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.86f, 0.9f, 0.5f);
            collider.center = new Vector3(0f, 0.0f, 0.02f);
            var body = go.AddComponent<Rigidbody>();
            body.mass = 1.2f;
            go.AddComponent<PlanarBody>();
            var material = go.AddComponent<MaterialBody>();
            material.SetProfile(p.Timber);
            Breakable(go, hp: 4f, breakImpulse: 6f, threshold: 1.5f);
            go.AddComponent<Objective>().SetKind(ObjectiveKind.CrestTarget);
            return Save(go, "Objectives");
        }

        public static GameObject RoyalVase(Profiles p)
        {
            GameObject go = NewObject("Prot_RoyalVase", PhysicsLayers.Protected);
            Visual(go.transform, "Mesh", ArtKit.Vase("Prot_RoyalVase"), ArtKit.Lit("VasePurple", ArtKit.VasePurple, 0.55f), ArtKit.Lit("VaseGold", ArtKit.VaseGold, 0.6f));
            GameObject marker = Visual(go.transform, "ProtectedMarker", ArtKit.Gem("Gem"),
                ArtKit.Lit("ProtectedGlow", ArtKit.VasePurple, 0.6f, emission: ArtKit.VasePurple * 0.9f));
            marker.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            marker.AddComponent<Bobber>();
            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.6f, 0.92f, 0.6f);
            var body = go.AddComponent<Rigidbody>();
            body.mass = 1.5f;
            go.AddComponent<PlanarBody>();
            go.AddComponent<MaterialBody>().SetProfile(p.Stone);
            Breakable(go, hp: 1f, breakImpulse: 4f, threshold: 1.2f);
            go.AddComponent<ProtectedObject>().SetKind(ProtectedKind.RoyalVase);
            return Save(go, "Protected");
        }

        // ------------------------------------------------------------------ props

        public static GameObject Rope()
        {
            GameObject go = NewObject("Prop_Rope", 0);
            Visual(go.transform, "Hook", ArtKit.Cylinder("Hook", 0.09f, 0.16f, 8, axis: 2), ArtKit.Lit("Iron", ArtKit.StoneDark, 0.4f));
            var lineGo = new GameObject("Line");
            lineGo.transform.SetParent(go.transform, false);
            var line = lineGo.AddComponent<LineRenderer>();
            line.sharedMaterial = ArtKit.Unlit("Rope", ArtKit.Rope, transparent: false);
            line.widthMultiplier = 0.07f;
            line.numCapVertices = 2;
            line.useWorldSpace = true;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var rope = go.AddComponent<RopeCuttable>();
            rope.Configure(null, new Vector3(0f, 0.5f, 0f), go.transform, null, line);
            return Save(go, "Props");
        }

        // ------------------------------------------------------------------ environment (Greenwood, D-052 prefixes)

        public static GameObject Ground(Profiles p)
        {
            GameObject go = NewObject("Env_GW_Ground", PhysicsLayers.Environment);
            Visual(go.transform, "Mesh", ArtKit.GrassBlock("Env_GW_Ground", new Vector3(16f, 1.2f, 3f)), ArtKit.Lit("Earth", ArtKit.Earth), ArtKit.Lit("Grass", ArtKit.Grass));
            Static(go, p.Earth, new Vector3(16f, 1.2f, 3f), new Vector3(0f, -0.6f, 0f));
            go.tag = "Ground";
            return Save(go, "Environment");
        }

        /// <summary>Grass-topped ledge, 1 m wide per unit of X scale; pivot at the top-centre.</summary>
        public static GameObject Ledge(Profiles p)
        {
            GameObject go = NewObject("Env_GW_Ledge", PhysicsLayers.Environment);
            Visual(go.transform, "Mesh", ArtKit.GrassBlock("Env_GW_Ledge", new Vector3(1f, 0.6f, 1.6f)), ArtKit.Lit("Earth", ArtKit.Earth), ArtKit.Lit("Grass", ArtKit.Grass));
            Static(go, p.Earth, new Vector3(1f, 0.6f, 1.6f), new Vector3(0f, -0.3f, 0f));
            go.tag = "Ground";
            return Save(go, "Environment");
        }

        public static GameObject Stump(Profiles p)
        {
            GameObject go = NewObject("Env_GW_Stump", PhysicsLayers.Environment);
            Visual(go.transform, "Mesh", ArtKit.Cylinder("Env_GW_Stump", 0.55f, 1f, 9), ArtKit.Lit("Bark", ArtKit.Bark));
            Visual(go.transform, "Rings", ArtKit.Cylinder("StumpTop", 0.5f, 0.04f, 9), ArtKit.Lit("TimberLight", ArtKit.TimberLight)).transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Static(go, p.Timber, new Vector3(1.05f, 1f, 1.05f), Vector3.zero);
            return Save(go, "Environment");
        }

        /// <summary>Overhead timber beam (scale X for length); pivot at centre.</summary>
        public static GameObject Beam(Profiles p)
        {
            GameObject go = NewObject("Env_GW_Beam", PhysicsLayers.Environment);
            Visual(go.transform, "Mesh", ArtKit.Box("Env_GW_Beam", new Vector3(1f, 0.4f, 0.9f), 0.04f), ArtKit.Lit("TimberDark", ArtKit.TimberDark));
            Static(go, p.Timber, new Vector3(1f, 0.4f, 0.9f), Vector3.zero);
            return Save(go, "Environment");
        }

        public static GameObject Pedestal(Profiles p)
        {
            GameObject go = NewObject("Env_GW_Pedestal", PhysicsLayers.Environment);
            Visual(go.transform, "Mesh", ArtKit.Box("Env_GW_Pedestal", new Vector3(0.9f, 0.6f, 0.9f), 0.07f), ArtKit.Lit("Ruin", ArtKit.Ruin));
            Static(go, p.Stone, new Vector3(0.9f, 0.6f, 0.9f), Vector3.zero);
            return Save(go, "Environment");
        }

        /// <summary>Striped stall awning (scale X for length); pivot at centre.</summary>
        public static GameObject Awning(Profiles p)
        {
            GameObject go = NewObject("Env_GW_Awning", PhysicsLayers.Environment);
            Visual(go.transform, "Mesh", ArtKit.Box("Env_GW_Awning", new Vector3(1f, 0.3f, 1.2f), 0.05f), ArtKit.Lit("Awning", ArtKit.Awning));
            Static(go, p.Timber, new Vector3(1f, 0.3f, 1.2f), Vector3.zero);
            return Save(go, "Environment");
        }

        // ------------------------------------------------------------------ hazards / markers

        public static GameObject WaterPit()
        {
            GameObject go = NewObject("Haz_WaterPit", PhysicsLayers.KillZone);
            var collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1f, 1f, 2f);
            collider.center = new Vector3(0f, -0.5f, 0f);
            go.AddComponent<KillZone>().SetKind(KillZoneKind.Water);
            GameObject surface = Visual(go.transform, "Surface", ArtKit.Box("WaterSurface", new Vector3(1f, 1f, 1.6f), 0f), ArtKit.Lit("Water", ArtKit.Water, 0.8f, transparent: true));
            surface.layer = 0;
            surface.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            return Save(go, "Hazards");
        }

        public static GameObject Pit()
        {
            GameObject go = NewObject("Haz_Pit", PhysicsLayers.KillZone);
            var collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1f, 1f, 2f);
            collider.center = new Vector3(0f, -0.5f, 0f);
            go.AddComponent<KillZone>().SetKind(KillZoneKind.Pit);
            return Save(go, "Hazards");
        }

        public static GameObject TutorialAnchor()
        {
            GameObject go = NewObject("Marker_TutorialAnchor", 0);
            go.AddComponent<TutorialAnchor>();
            return Save(go, "Markers");
        }

        // ------------------------------------------------------------------ arrow + bow

        public static ArrowProjectile ArrowOak()
        {
            GameObject go = NewObject("Arrow_Oak", PhysicsLayers.Arrow);
            var visual = new GameObject("Visual") { layer = PhysicsLayers.Arrow };
            visual.transform.SetParent(go.transform, false);
            GameObject mesh = Visual(visual.transform, "Mesh", ArtKit.Arrow("Arrow_Oak"),
                ArtKit.Lit("ArrowShaft", ArtKit.ArrowShaft), ArtKit.Lit("ArrowHead", ArtKit.ArrowHead, 0.6f), ArtKit.Lit("Fletch", ArtKit.Fletch));
            mesh.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            var collider = go.AddComponent<CapsuleCollider>();
            collider.direction = 2;
            collider.radius = 0.04f;
            collider.height = 0.9f;
            collider.center = new Vector3(0f, 0f, -0.45f);
            collider.enabled = false;
            var body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            var arrow = go.AddComponent<ArrowProjectile>();
            var so = new SerializedObject(arrow);
            so.FindProperty("_spentCollider").objectReferenceValue = collider;
            so.FindProperty("_visual").objectReferenceValue = visual.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
            return Save(go, "Arrows").GetComponent<ArrowProjectile>();
        }

        public static GameObject Bow(Material dotMaterial, Material ringMaterial)
        {
            GameObject go = NewObject("Bow_OakRanger", 0);
            var rotator = new GameObject("Rotator");
            rotator.transform.SetParent(go.transform, false);
            Visual(rotator.transform, "Mesh", ArtKit.Bow("Bow_OakRanger"),
                ArtKit.Lit("BowWood", ArtKit.BowWood, 0.35f), ArtKit.Lit("BowGold", ArtKit.Gold, 0.7f), ArtKit.Lit("TimberDark", ArtKit.TimberDark));
            var top = new GameObject("TopTip").transform;
            top.SetParent(rotator.transform, false);
            top.localPosition = new Vector3(-0.78f, -0.04f, -0.05f);
            var bottom = new GameObject("BottomTip").transform;
            bottom.SetParent(rotator.transform, false);
            bottom.localPosition = new Vector3(0.78f, -0.04f, -0.05f);
            var nock = new GameObject("Nock").transform;
            nock.SetParent(rotator.transform, false);
            nock.localPosition = new Vector3(0f, -0.04f, -0.05f);
            GameObject nocked = Visual(nock, "NockedArrow", ArtKit.Arrow("Arrow_Oak"),
                ArtKit.Lit("ArrowShaft", ArtKit.ArrowShaft), ArtKit.Lit("ArrowHead", ArtKit.ArrowHead, 0.6f), ArtKit.Lit("Fletch", ArtKit.Fletch));
            nocked.transform.localPosition = new Vector3(0f, 0.86f, 0f);
            nocked.transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.back);

            var stringGo = new GameObject("String");
            stringGo.transform.SetParent(go.transform, false);
            var line = stringGo.AddComponent<LineRenderer>();
            line.sharedMaterial = ArtKit.Unlit("BowString", Color.white, transparent: false);
            line.widthMultiplier = 0.03f;
            line.positionCount = 3;
            line.useWorldSpace = true;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var previewGo = new GameObject("Preview");
            previewGo.transform.SetParent(go.transform, false);
            var preview = previewGo.AddComponent<TrajectoryPreview>();
            var pso = new SerializedObject(preview);
            pso.FindProperty("_dotMesh").objectReferenceValue = ArtKit.Get("Quad") ?? ArtKit.Quad("Quad");
            pso.FindProperty("_dotMaterial").objectReferenceValue = dotMaterial;
            pso.FindProperty("_ringMaterial").objectReferenceValue = ringMaterial;
            pso.ApplyModifiedPropertiesWithoutUndo();

            var view = go.AddComponent<BowView>();
            var vso = new SerializedObject(view);
            vso.FindProperty("_rotator").objectReferenceValue = rotator.transform;
            vso.FindProperty("_nock").objectReferenceValue = nock;
            vso.FindProperty("_topTip").objectReferenceValue = top;
            vso.FindProperty("_bottomTip").objectReferenceValue = bottom;
            vso.FindProperty("_string").objectReferenceValue = line;
            vso.FindProperty("_nockedArrow").objectReferenceValue = nocked;
            vso.ApplyModifiedPropertiesWithoutUndo();

            var controller = go.AddComponent<BowController>();
            var cso = new SerializedObject(controller);
            cso.FindProperty("_view").objectReferenceValue = view;
            cso.FindProperty("_preview").objectReferenceValue = preview;
            cso.ApplyModifiedPropertiesWithoutUndo();
            return Save(go, "Bow");
        }

        // ------------------------------------------------------------------ backdrop

        /// <summary>Greenwood Range "training valley" diorama behind the play plane (decor only, no colliders).</summary>
        public static GameObject Backdrop(Texture2D skyGradient)
        {
            GameObject go = NewObject("Env_GW_Backdrop_TrainingGrounds", 0);
            Transform t = go.transform;
            Material sky = ArtKit.Unlit("Sky", Color.white, transparent: false, texture: skyGradient);
            Decor(t, "Sky", ArtKit.Get("Quad") ?? ArtKit.Quad("Quad"), new Vector3(0f, 18f, 70f), Vector3.zero, new Vector3(160f, 90f, 1f), sky);

            Material grass = ArtKit.Lit("Grass", ArtKit.Grass);
            Material grassDark = ArtKit.Lit("GrassDark", ArtKit.GrassDark);
            Decor(t, "Meadow", ArtKit.Box("Meadow", new Vector3(1f, 1f, 1f), 0f), new Vector3(0f, -0.6f, 30f), Vector3.zero, new Vector3(140f, 1.2f, 60f), grass);
            Decor(t, "FrontLip", ArtKit.Box("Meadow", new Vector3(1f, 1f, 1f), 0f), new Vector3(0f, -0.62f, -1.5f), Vector3.zero, new Vector3(40f, 1.2f, 3f), grassDark);

            Material hillFar = ArtKit.Lit("HillFar", ArtKit.HillFar);
            Material hillNear = ArtKit.Lit("HillNear", ArtKit.HillNear);
            Mesh mound = ArtKit.Mound("Mound", new Vector3(1f, 1f, 1f));
            Decor(t, "HillFarL", mound, new Vector3(-30f, -1f, 62f), Vector3.zero, new Vector3(34f, 11f, 14f), hillFar);
            Decor(t, "HillFarR", mound, new Vector3(28f, -1f, 64f), Vector3.zero, new Vector3(36f, 13f, 14f), hillFar);
            Decor(t, "HillFarC", mound, new Vector3(2f, -2f, 66f), Vector3.zero, new Vector3(28f, 8f, 12f), hillFar);
            Decor(t, "HillNearL", mound, new Vector3(-24f, -1f, 40f), Vector3.zero, new Vector3(16f, 6f, 10f), hillNear);
            Decor(t, "HillNearR", mound, new Vector3(25f, -1f, 38f), Vector3.zero, new Vector3(18f, 7f, 10f), hillNear);

            Material bark = ArtKit.Lit("Bark", ArtKit.Bark);
            Material leaf = ArtKit.Lit("Leaf", ArtKit.Leaf);
            Material leafLight = ArtKit.Lit("LeafLight", ArtKit.LeafLight);
            Mesh pine = ArtKit.Tree("TreePine", 5f, 0);
            Mesh oak = ArtKit.Tree("TreeOak", 4.2f, 1);
            Vector3[] trees =
            {
                new Vector3(-8.2f, 0f, 6f), new Vector3(-10.5f, 0f, 11f), new Vector3(-7.4f, 0f, 16f), new Vector3(-13f, 0f, 18f),
                new Vector3(8.4f, 0f, 7f), new Vector3(11f, 0f, 12f), new Vector3(7.6f, 0f, 17f), new Vector3(14f, 0f, 20f),
                new Vector3(-9f, 0f, 30f), new Vector3(10.5f, 0f, 31f), new Vector3(-17f, 0f, 27f), new Vector3(18f, 0f, 26f)
            };
            for (int i = 0; i < trees.Length; i++)
            {
                float s = 0.85f + (i * 37 % 10) * 0.05f;
                Decor(t, "Tree" + i, i % 2 == 0 ? pine : oak, trees[i], new Vector3(0f, i * 47f, 0f), Vector3.one * s, bark, i % 3 == 0 ? leafLight : leaf);
            }

            Material ruin = ArtKit.Lit("Ruin", ArtKit.Ruin);
            Mesh pillar = ArtKit.Cylinder("Pillar", 0.45f, 1f, 8);
            Decor(t, "PillarA", pillar, new Vector3(-13.6f, 1.8f, 22f), Vector3.zero, new Vector3(1f, 3.6f, 1f), ruin);
            Decor(t, "PillarB", pillar, new Vector3(-10.4f, 1.3f, 23f), Vector3.zero, new Vector3(1f, 2.6f, 1f), ruin);
            Decor(t, "Lintel", ArtKit.Box("Lintel", new Vector3(1f, 1f, 1f), 0.05f), new Vector3(-12f, 3.8f, 22.5f), new Vector3(0f, 0f, 4f), new Vector3(4.6f, 0.6f, 1f), ruin);

            Mesh crest = ArtKit.Get("Obj_CrestTarget");
            if (crest != null)
            {
                Material[] crestMats = { ArtKit.Lit("CrestRed", ArtKit.CrestRed, 0.25f), ArtKit.Lit("CrestCream", ArtKit.CrestCream, 0.2f), ArtKit.Lit("TimberDark", ArtKit.TimberDark) };
                Decor(t, "RangeTargetL", crest, new Vector3(-7.4f, 1.1f, 9f), new Vector3(0f, 12f, 0f), Vector3.one * 1.6f, crestMats);
                Decor(t, "RangeTargetR", crest, new Vector3(7.6f, 1.1f, 10f), new Vector3(0f, -10f, 0f), Vector3.one * 1.6f, crestMats);
            }

            Material cloud = ArtKit.Lit("Cloud", ArtKit.Cloud, 0f, emission: new Color(0.45f, 0.45f, 0.45f));
            Mesh blob = ArtKit.Blob("Cloud", Vector3.one);
            Vector3[] clouds = { new Vector3(-14f, 26f, 55f), new Vector3(10f, 30f, 60f), new Vector3(22f, 23f, 52f) };
            foreach (Vector3 c in clouds)
            {
                Decor(t, "Cloud", blob, c, Vector3.zero, new Vector3(5f, 2.2f, 2f), cloud);
                Decor(t, "Cloud", blob, c + new Vector3(3f, 0.6f, 0f), Vector3.zero, new Vector3(3.6f, 2.6f, 2f), cloud);
                Decor(t, "Cloud", blob, c + new Vector3(-2.6f, -0.3f, 0f), Vector3.zero, new Vector3(3f, 1.8f, 2f), cloud);
            }

            // Bow platform: a low timber stump at the bottom centre the bow "stands" on.
            Decor(t, "BowStump", ArtKit.Cylinder("BowStump", 0.7f, 1f, 9), new Vector3(0f, 0.25f, 0.6f), Vector3.zero, new Vector3(1f, 0.9f, 1f), bark);
            return Save(go, "Environment");
        }
    }
}
