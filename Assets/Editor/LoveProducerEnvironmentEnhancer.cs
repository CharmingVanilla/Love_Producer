using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class LoveProducerEnvironmentEnhancer
{
    private const string FormalScenePath = "Assets/Scenes/LoveProducerFloorPlan.unity";
    private const string MaterialFolder = "Assets/Generated/FloorPlanMaterials";
    private const string SocialSceneVersionMarker = "SOCIAL SCENE VERSION 7 - COLORFUL LIGHTING";

    [MenuItem("Love Producer/RECOVERY - Restore Detail Upgrades Safely")]
    public static void RestoreDetailUpgradesSafely()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        if (details == null || furniture == null)
        {
            Debug.LogError("Recovery stopped: the rebuilt DETAILS & ENVIRONMENT hierarchy was not found.");
            return;
        }

        // The full rebuild recreates these old sofas. Remove only the known
        // generated versions and preserve the user's custom circular sofa roots.
        string[] obsoleteFurniture =
        {
            "Curved Living Conversation Sofa",
            "Lounge Sofa South",
            "Lounge Sofa West",
            "Lounge Sofa North"
        };
        foreach (string objectName in obsoleteFurniture)
        {
            Transform old = furniture.Find(objectName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        }

        RestoreTwoCircularSofas();

        UpgradeColorfulLivingLightsOnly();
        UpgradeKitchenTableToSixSeats();
        UpgradeTerraceFloorsAndExtraFurniture();
        ApplyCinematicCameraAndNightLook();
        UpgradeMainTerraceLighting();
        AddLifestyleLightingAndTableDetails();
        LoveProducerFloorPlanBuilder.FixSplitBedroomDoorways();
        ApplySafeMaterialDetailUpgrade();
        RefineLivingAndLoungeRugs();
        AddArchitecturalTrimsAndFrames();
        AddLayeredMainTerracePlants();
        AddBedroomSoftFurnishings();
        ReplaceBedroomWindowsWithRealGlass();

        // Deliberately excluded: All Room Ceiling Lights and bedroom grouping.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = details;
        Debug.LogWarning("Recovery completed. Review the scene before saving. All Room Ceiling Lights and bedroom grouping were intentionally skipped.");
    }

    [MenuItem("Love Producer/RECOVERY - Restore Two Circular Sofas")]
    public static void RestoreTwoCircularSofas()
    {
        Material white = AssetDatabase.LoadAssetAtPath<Material>("Assets/CircularSofaSeat_white.mat");
        Material pink = AssetDatabase.LoadAssetAtPath<Material>("Assets/CircularSofaSeat_Pink.mat");
        if (white == null || pink == null)
        {
            Debug.LogError("Circular sofa recovery stopped: the white or pink sofa material is missing from Assets.");
            return;
        }

        Material teal = Mat("Recovered Sofa Teal Cushion", new Color(0.18f, 0.54f, 0.57f), 0.16f);
        Material coral = Mat("Recovered Sofa Coral Cushion", new Color(0.82f, 0.38f, 0.45f), 0.16f);
        Material cream = Mat("Recovered Sofa Cream Cushion", new Color(0.91f, 0.82f, 0.68f), 0.14f);

        GameObject living = GameObject.Find("LivingroomCircularSofa");
        if (living == null)
        {
            living = BuildRecoveredCircularSofa("LivingroomCircularSofa", new Vector3(0f, 0f, -1.0f),
                13, 2.65f, 280f, -140f, 1.0f, white, coral, teal);
            Undo.RegisterCreatedObjectUndo(living, "Restore Living Circular Sofa");
        }
        else if (living.transform.childCount == 0)
        {
            Object.DestroyImmediate(living);
            living = BuildRecoveredCircularSofa("LivingroomCircularSofa", new Vector3(0f, 0f, -1.0f),
                13, 2.65f, 280f, -140f, 1.0f, white, coral, teal);
            Undo.RegisterCreatedObjectUndo(living, "Restore Living Circular Sofa");
        }

        GameObject lounge = GameObject.Find("LoungeCircularSofa");
        if (lounge == null) lounge = GameObject.Find("LoungeCircularSofa ");
        if (lounge == null)
        {
            lounge = BuildRecoveredCircularSofa("LoungeCircularSofa", new Vector3(8.0f, 0f, -5.8f),
                8, 1.65f, 245f, -122.5f, 0.72f, pink, cream, teal);
            Undo.RegisterCreatedObjectUndo(lounge, "Restore Lounge Circular Sofa");
        }
        else if (lounge.transform.childCount == 0)
        {
            Object.DestroyImmediate(lounge);
            lounge = BuildRecoveredCircularSofa("LoungeCircularSofa", new Vector3(8.0f, 0f, -5.8f),
                8, 1.65f, 245f, -122.5f, 0.72f, pink, cream, teal);
            Undo.RegisterCreatedObjectUndo(lounge, "Restore Lounge Circular Sofa");
        }

        Selection.activeGameObject = living;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Two grouped circular sofas restored. Existing non-empty custom sofa roots were preserved.");
    }

    private static GameObject BuildRecoveredCircularSofa(string name, Vector3 center, int segmentCount,
        float radius, float arcAngle, float startAngle, float scale, Material upholstery,
        Material accentA, Material accentB)
    {
        GameObject root = new GameObject(name);
        root.transform.position = center;
        for (int i = 0; i < segmentCount; i++)
        {
            float t = segmentCount == 1 ? 0.5f : i / (float)(segmentCount - 1);
            float angle = startAngle + arcAngle * t;
            float radians = angle * Mathf.Deg2Rad;
            Vector3 localPosition = new Vector3(Mathf.Sin(radians) * radius, 0f, Mathf.Cos(radians) * radius);
            Transform module = Group("SofaSegment_" + (i + 1).ToString("00"), root.transform);
            module.localPosition = localPosition;
            Vector3 towardCenter = -localPosition;
            module.localRotation = Quaternion.LookRotation(towardCenter.sqrMagnitude > 0.001f ? towardCenter : Vector3.forward);
            module.localScale = Vector3.one * scale;

            Box("Seat", new Vector3(1.08f, 0.42f, 0.82f), new Vector3(0f, 0.34f, 0f), upholstery, module, true);
            Box("Back", new Vector3(1.10f, 0.78f, 0.20f), new Vector3(0f, 0.72f, -0.36f), upholstery, module, true);
            if (i == 0 || i == segmentCount - 1)
                Box("End Arm", new Vector3(0.18f, 0.66f, 0.82f), new Vector3(i == 0 ? -0.55f : 0.55f, 0.50f, 0f), upholstery, module, true);
            if (i % 2 == 1)
                Box("Accent Cushion", new Vector3(0.44f, 0.18f, 0.38f), new Vector3(0f, 0.70f, -0.05f),
                    i % 4 == 1 ? accentA : accentB, module, true);
        }
        return root;
    }

    [MenuItem("Love Producer/Convert Both Sofas to Smooth Ring Shape")]
    public static void ConvertBothSofasToSmoothRingShape()
    {
        GameObject living = GameObject.Find("LivingroomCircularSofa");
        GameObject lounge = GameObject.Find("LoungeCircularSofa");
        if (lounge == null) lounge = GameObject.Find("LoungeCircularSofa ");
        if (living == null || lounge == null)
        {
            RestoreTwoCircularSofas();
            living = GameObject.Find("LivingroomCircularSofa");
            lounge = GameObject.Find("LoungeCircularSofa");
        }
        if (living == null || lounge == null)
        {
            Debug.LogError("Smooth sofa conversion stopped: one or both sofa roots could not be found.");
            return;
        }

        Material white = AssetDatabase.LoadAssetAtPath<Material>("Assets/CircularSofaSeat_white.mat");
        Material pink = AssetDatabase.LoadAssetAtPath<Material>("Assets/CircularSofaSeat_Pink.mat");
        Material teal = Mat("Recovered Sofa Teal Cushion", new Color(0.18f, 0.54f, 0.57f), 0.16f);
        Material coral = Mat("Recovered Sofa Coral Cushion", new Color(0.82f, 0.38f, 0.45f), 0.16f);
        Material cream = Mat("Recovered Sofa Cream Cushion", new Color(0.91f, 0.82f, 0.68f), 0.14f);
        if (white == null || pink == null) return;

        ClearSofaChildren(living.transform);
        ClearSofaChildren(lounge.transform);
        BuildSmoothRingChildren(living.transform, "Living", 13, 2.0f, 2.88f, 280f, -140f, white, coral, teal);
        BuildSmoothRingChildren(lounge.transform, "Lounge", 8, 1.05f, 1.72f, 245f, -122.5f, pink, cream, teal);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = living;
        EditorGUIUtility.PingObject(living);
        Debug.Log("Both sofas converted from box modules to contiguous annular-sector meshes.");
    }

    private static void ClearSofaChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
    }

    private static void BuildSmoothRingChildren(Transform root, string assetPrefix, int segmentCount,
        float innerRadius, float outerRadius, float arcAngle, float startAngle, Material upholstery,
        Material accentA, Material accentB)
    {
        EnsureFolder("Assets/Generated/FurnitureMeshes");
        float segmentAngle = arcAngle / segmentCount;
        Mesh seatMesh = CreateOrUpdateRingMesh(
            "Assets/Generated/FurnitureMeshes/" + assetPrefix + "_SmoothSeat.asset",
            innerRadius, outerRadius, 0.18f, 0.54f, arcAngle, segmentCount * 5);
        Mesh backMesh = CreateOrUpdateRingMesh(
            "Assets/Generated/FurnitureMeshes/" + assetPrefix + "_SmoothBack.asset",
            outerRadius - 0.18f, outerRadius + 0.06f, 0.52f, 1.16f, arcAngle, segmentCount * 5);

        float arcCenter = startAngle + arcAngle * 0.5f;
        Transform continuous = Group("CONTINUOUS CURVED SOFA", root);
        continuous.localPosition = Vector3.zero;
        continuous.localRotation = Quaternion.Euler(0f, arcCenter, 0f);

        GameObject seat = new GameObject("Continuous Curved Seat");
        seat.transform.SetParent(continuous, false);
        MeshFilter seatFilter = seat.AddComponent<MeshFilter>();
        seatFilter.sharedMesh = seatMesh;
        seat.AddComponent<MeshRenderer>().sharedMaterial = upholstery;
        seat.AddComponent<MeshCollider>().sharedMesh = seatMesh;

        GameObject back = new GameObject("Continuous Curved Back");
        back.transform.SetParent(continuous, false);
        MeshFilter backFilter = back.AddComponent<MeshFilter>();
        backFilter.sharedMesh = backMesh;
        back.AddComponent<MeshRenderer>().sharedMaterial = upholstery;

        for (int i = 0; i < segmentCount; i++)
        {
            float centerAngle = startAngle + segmentAngle * (i + 0.5f);
            if (i % 2 == 1)
            {
                float cushionRadius = outerRadius - 0.30f;
                Transform cushionAnchor = Group("Cushion Anchor " + (i + 1), root);
                cushionAnchor.localPosition = Vector3.zero;
                cushionAnchor.localRotation = Quaternion.Euler(0f, centerAngle, 0f);
                Box("Accent Cushion", new Vector3(0.42f, 0.20f, 0.34f),
                    new Vector3(0f, 0.73f, cushionRadius), i % 4 == 1 ? accentA : accentB, cushionAnchor, true);
            }
        }
    }

    private static Mesh CreateOrUpdateRingMesh(string path, float innerRadius, float outerRadius,
        float bottomY, float topY, float angleWidth, int subdivisions)
    {
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null)
        {
            mesh = new Mesh { name = Path.GetFileNameWithoutExtension(path) };
            AssetDatabase.CreateAsset(mesh, path);
        }
        else mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        float half = angleWidth * 0.5f;
        for (int i = 0; i < subdivisions; i++)
        {
            float a = Mathf.Lerp(-half, half, i / (float)subdivisions) * Mathf.Deg2Rad;
            float b = Mathf.Lerp(-half, half, (i + 1) / (float)subdivisions) * Mathf.Deg2Rad;
            Vector3 ia0 = RingPoint(innerRadius, bottomY, a);
            Vector3 ia1 = RingPoint(innerRadius, topY, a);
            Vector3 ib0 = RingPoint(innerRadius, bottomY, b);
            Vector3 ib1 = RingPoint(innerRadius, topY, b);
            Vector3 oa0 = RingPoint(outerRadius, bottomY, a);
            Vector3 oa1 = RingPoint(outerRadius, topY, a);
            Vector3 ob0 = RingPoint(outerRadius, bottomY, b);
            Vector3 ob1 = RingPoint(outerRadius, topY, b);
            AddDoubleSidedQuad(vertices, triangles, ia1, ib1, ob1, oa1);
            AddDoubleSidedQuad(vertices, triangles, oa0, ob0, ib0, ia0);
            AddDoubleSidedQuad(vertices, triangles, ia0, ib0, ib1, ia1);
            AddDoubleSidedQuad(vertices, triangles, ob0, oa0, oa1, ob1);
            if (i == 0) AddDoubleSidedQuad(vertices, triangles, oa0, ia0, ia1, oa1);
            if (i == subdivisions - 1) AddDoubleSidedQuad(vertices, triangles, ib0, ob0, ob1, ib1);
        }
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static Vector3 RingPoint(float radius, float y, float radians)
    {
        return new Vector3(Mathf.Sin(radians) * radius, y, Mathf.Cos(radians) * radius);
    }

    private static void AddDoubleSidedQuad(List<Vector3> vertices, List<int> triangles,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int start = vertices.Count;
        vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
        triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);

        // Back faces need their own vertices. Sharing vertices with reversed
        // triangles makes RecalculateNormals cancel the normals and produces
        // incorrect dark or tinted sofa colors under scene lighting.
        int back = vertices.Count;
        vertices.Add(a); vertices.Add(d); vertices.Add(c); vertices.Add(b);
        triangles.Add(back); triangles.Add(back + 1); triangles.Add(back + 2);
        triangles.Add(back); triangles.Add(back + 2); triangles.Add(back + 3);
    }

    [MenuItem("Love Producer/Add Ocean Waves and Sand Texture")]
    public static void AddOceanWavesAndSandTexture()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform environment = details != null ? details.transform.Find("Island Environment") : null;
        if (environment == null)
        {
            Debug.LogError("Ocean texture upgrade stopped: Island Environment was not found.");
            return;
        }

        Transform ocean = environment.Find("Ocean");
        Transform beach = environment.Find("Beach");
        if (ocean == null || beach == null)
        {
            Debug.LogError("Ocean texture upgrade stopped: Ocean or Beach was not found.");
            return;
        }

        Texture2D oceanTexture = CreateCoastalTexture("Ocean_Wave_Texture", true);
        Texture2D sandTexture = CreateCoastalTexture("Beach_Sand_Ripple_Texture", false);
        Material oceanMaterial = DetailedMat("Detailed Tropical Ocean", oceanTexture, 0.82f, new Vector2(3.8f, 5.2f));
        Material sandMaterial = DetailedMat("Detailed Beach Sand", sandTexture, 0.10f, new Vector2(5.0f, 3.5f));
        oceanMaterial.SetFloat("_Metallic", 0.05f);
        sandMaterial.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(oceanMaterial);
        EditorUtility.SetDirty(sandMaterial);

        Renderer oceanRenderer = ocean.GetComponent<Renderer>();
        Renderer beachRenderer = beach.GetComponent<Renderer>();
        if (oceanRenderer != null)
        {
            Undo.RecordObject(oceanRenderer, "Apply Ocean Wave Material");
            oceanRenderer.sharedMaterial = oceanMaterial;
        }
        if (beachRenderer != null)
        {
            Undo.RecordObject(beachRenderer, "Apply Sand Ripple Material");
            beachRenderer.sharedMaterial = sandMaterial;
        }

        Transform oldFoam = environment.Find("Shoreline Foam Detail");
        if (oldFoam != null) Undo.DestroyObjectImmediate(oldFoam.gameObject);
        Transform foamGroup = Group("Shoreline Foam Detail", environment);
        Material foam = EmissiveMat("Soft Shore Foam", new Color(0.82f, 0.94f, 0.92f), 1.25f);
        for (int row = 0; row < 3; row++)
        {
            for (int i = 0; i < 28; i++)
            {
                float x = -16.5f + i * 1.22f;
                float z = 17.0f + row * 0.62f + Mathf.Sin(i * 0.72f + row * 1.4f) * (0.18f + row * 0.04f);
                float yaw = Mathf.Cos(i * 0.72f + row * 1.4f) * 7.5f;
                GameObject foamSegment = Box("Foam Ripple " + row + "-" + i,
                    new Vector3(1.34f, 0.035f, 0.10f + row * 0.025f), new Vector3(x, -0.135f, z), foam, foamGroup);
                foamSegment.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
        }

        Undo.RegisterCreatedObjectUndo(foamGroup.gameObject, "Create Shoreline Foam Detail");
        Selection.activeGameObject = foamGroup.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Ocean wave texture, sand ripples and layered shoreline foam were added.");
    }

    private static Texture2D CreateCoastalTexture(string name, bool ocean)
    {
        const int size = 256;
        string path = MaterialFolder + "/" + name + ".png";
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nx = x / (float)size;
            float ny = y / (float)size;
            float noise = Mathf.PerlinNoise(nx * 9.0f + 2.3f, ny * 9.0f + 6.7f) - 0.5f;
            if (ocean)
            {
                float longWave = Mathf.Sin(ny * Mathf.PI * 18f + Mathf.Sin(nx * Mathf.PI * 6f) * 1.4f);
                float crossWave = Mathf.Sin((nx * 1.3f + ny) * Mathf.PI * 22f) * 0.35f;
                float wave = longWave * 0.085f + crossWave * 0.035f + noise * 0.07f;
                Color deep = new Color(0.045f, 0.30f, 0.42f);
                Color shallow = new Color(0.18f, 0.66f, 0.70f);
                Color color = Color.Lerp(deep, shallow, Mathf.Clamp01(0.52f + wave * 2.2f));
                if (longWave > 0.92f) color = Color.Lerp(color, new Color(0.62f, 0.88f, 0.86f), 0.35f);
                texture.SetPixel(x, y, color);
            }
            else
            {
                float ripple = Mathf.Sin((ny + Mathf.Sin(nx * 11f) * 0.025f) * Mathf.PI * 28f) * 0.025f;
                float speckle = Mathf.PerlinNoise(nx * 48f, ny * 48f) * 0.035f;
                float value = ripple + noise * 0.055f + speckle;
                Color sand = new Color(0.79f + value, 0.67f + value, 0.47f + value * 0.75f);
                texture.SetPixel(x, y, sand);
            }
        }
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    [MenuItem("Love Producer/Add Beach Chairs and Parasols")]
    public static void AddBeachChairsAndParasols()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        if (furniture == null)
        {
            Debug.LogError("Beach furniture upgrade stopped: Detailed Furniture was not found.");
            return;
        }

        Transform old = furniture.Find("Beach Atmosphere Furniture");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group("Beach Atmosphere Furniture", furniture);
        Material wood = Mat("Beach Chair Wood", new Color(0.46f, 0.29f, 0.14f), 0.26f);
        Material cream = Mat("Beach Parasol Cream", new Color(0.94f, 0.82f, 0.60f), 0.16f);
        Material coral = Mat("Beach Parasol Coral", new Color(0.91f, 0.34f, 0.34f), 0.16f);
        Material teal = Mat("Beach Parasol Teal", new Color(0.16f, 0.58f, 0.61f), 0.16f);
        Material pink = Mat("Beach Towel Pink", new Color(0.82f, 0.43f, 0.58f), 0.12f);
        Material blue = Mat("Beach Towel Blue", new Color(0.30f, 0.55f, 0.70f), 0.12f);
        Material yellow = Mat("Beach Towel Yellow", new Color(0.91f, 0.67f, 0.24f), 0.12f);

        AddBeachSet("West Beach Set", new Vector3(-9.5f, -0.12f, 14.2f), -6f, wood, cream, coral, pink, blue, group);
        AddBeachSet("Center Beach Set", new Vector3(0f, -0.12f, 15.0f), 4f, wood, cream, teal, blue, yellow, group);
        AddBeachSet("East Beach Set", new Vector3(9.5f, -0.12f, 14.2f), 7f, wood, cream, coral, yellow, pink, group);

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Add Beach Chairs and Parasols");
        Selection.activeGameObject = group.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Three beach sets with six loungers, parasols and side tables were added.");
    }

    private static void AddBeachSet(string name, Vector3 center, float yaw, Material wood,
        Material stripeA, Material stripeB, Material towelA, Material towelB, Transform parent)
    {
        Transform set = Group(name, parent);
        set.position = center;
        set.rotation = Quaternion.Euler(0f, yaw, 0f);
        BeachParasol(new Vector3(0f, 0f, 0.55f), wood, stripeA, stripeB, set);
        BeachLounger("Left Lounger", new Vector3(-1.25f, 0f, -0.30f), -6f, wood, towelA, set);
        BeachLounger("Right Lounger", new Vector3(1.25f, 0f, -0.30f), 6f, wood, towelB, set);
        Cylinder("Beach Side Table", center, new Vector3(0.34f, 0.28f, 0.34f), wood, set);
        Cylinder("Table Drink", center + new Vector3(0f, 0.64f, 0f), new Vector3(0.07f, 0.10f, 0.07f), stripeB, set);
    }

    private static void BeachParasol(Vector3 localPosition, Material pole, Material stripeA,
        Material stripeB, Transform parent)
    {
        Transform parasol = Group("Striped Beach Parasol", parent);
        parasol.localPosition = localPosition;
        Box("Parasol Pole", new Vector3(0.09f, 2.15f, 0.09f), new Vector3(0f, 1.05f, 0f), pole, parasol, true);
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f;
            float radians = angle * Mathf.Deg2Rad;
            GameObject panel = Box("Canopy Panel " + (i + 1), new Vector3(0.62f, 0.07f, 1.45f),
                new Vector3(Mathf.Sin(radians) * 0.62f, 2.10f, Mathf.Cos(radians) * 0.62f),
                i % 2 == 0 ? stripeA : stripeB, parasol, true);
            panel.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        GameObject top = Sphere("Parasol Top", Vector3.zero, Vector3.one * 0.14f, pole, parasol);
        top.transform.localPosition = new Vector3(0f, 2.17f, 0f);
    }

    private static void BeachLounger(string name, Vector3 localPosition, float yaw,
        Material wood, Material towel, Transform parent)
    {
        Transform chair = Group(name, parent);
        chair.localPosition = localPosition;
        chair.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Box("Lounger Seat", new Vector3(0.76f, 0.14f, 1.46f), new Vector3(0f, 0.34f, 0.20f), wood, chair, true);
        GameObject back = Box("Reclined Back", new Vector3(0.76f, 0.14f, 0.92f), new Vector3(0f, 0.70f, -0.62f), wood, chair, true);
        back.transform.localRotation = Quaternion.Euler(-52f, 0f, 0f);
        Box("Towel", new Vector3(0.58f, 0.035f, 1.05f), new Vector3(0f, 0.43f, 0.30f), towel, chair, true);
        for (int side = -1; side <= 1; side += 2)
        {
            Box("Front Leg", new Vector3(0.09f, 0.34f, 0.09f), new Vector3(side * 0.29f, 0.17f, 0.70f), wood, chair, true);
            Box("Rear Leg", new Vector3(0.09f, 0.34f, 0.09f), new Vector3(side * 0.29f, 0.17f, -0.50f), wood, chair, true);
        }
    }

    [InitializeOnLoadMethod]
    private static void ApplyMissingReferenceDetailsAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorSceneManager.GetActiveScene().path != FormalScenePath) return;
            bool hasCurrentVersion = GameObject.Find(SocialSceneVersionMarker) != null;
            if (hasCurrentVersion) return;

            // Lighting-only upgrades must preserve sofas and materials edited by the user.
            GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
            if (details == null)
            {
                RebuildCompleteSocialVilla();
                return;
            }

            UpgradeColorfulLivingLightsOnly();
            Transform oldMarker = null;
            foreach (Transform child in details.transform)
            {
                if (child.name.StartsWith("SOCIAL SCENE VERSION"))
                {
                    oldMarker = child;
                    break;
                }
            }
            if (oldMarker != null) oldMarker.name = SocialSceneVersionMarker;
            else Group(SocialSceneVersionMarker, details.transform);
            EditorSceneManager.SaveOpenScenes();
        };
    }

    [MenuItem("Love Producer/Apply Details and Day Night to Formal Project")]
    public static void ApplyToFormalProject()
    {
        EditorSceneManager.OpenScene(FormalScenePath, OpenSceneMode.Single);
        Enhance();
    }

    [MenuItem("Love Producer/Rebuild Complete Social Villa")]
    public static void RebuildCompleteSocialVilla()
    {
        LoveProducerFloorPlanBuilder.Build();
        Enhance();
        LoveProducerPlayerBuilder.AddWalker();
        RestoreSocialVillaCamera();
        EditorSceneManager.SaveOpenScenes();
    }

    [MenuItem("Love Producer/Add Details and Environment")]
    public static void Enhance()
    {
        GameObject oldRoot = GameObject.Find("DETAILS & ENVIRONMENT");
        if (oldRoot != null) Object.DestroyImmediate(oldRoot);

        EnsureFolder("Assets/Generated");
        EnsureFolder(MaterialFolder);

        Material grass = Mat("Environment Grass", new Color(0.32f, 0.48f, 0.22f));
        Material sand = Mat("Beach Sand", new Color(0.82f, 0.72f, 0.52f));
        Material water = Mat("Ocean Water", new Color(0.20f, 0.62f, 0.72f), 0.75f);
        Material path = Mat("Stone Path", new Color(0.58f, 0.54f, 0.47f));
        Material wood = Mat("Warm Wood", new Color(0.38f, 0.22f, 0.12f));
        Material darkWood = Mat("Dark Wood", new Color(0.20f, 0.11f, 0.07f));
        Material leaf = Mat("Leaves", new Color(0.18f, 0.43f, 0.16f));
        Material leafLight = Mat("Light Leaves", new Color(0.34f, 0.58f, 0.25f));
        Material ceramic = Mat("Ceramic", new Color(0.91f, 0.91f, 0.86f), 0.5f);
        Material metal = Mat("Metal", new Color(0.32f, 0.34f, 0.36f), 0.75f);
        Material glass = Mat("Window Glass", new Color(0.42f, 0.73f, 0.82f), 0.85f);
        Material cream = Mat("Cream Fabric", new Color(0.78f, 0.72f, 0.62f));
        Material blue = Mat("Blue Fabric", new Color(0.34f, 0.38f, 0.62f));
        Material pink = Mat("Pink Fabric", new Color(0.78f, 0.43f, 0.58f));
        Material teal = Mat("Tropical Teal", new Color(0.16f, 0.55f, 0.58f));
        Material black = Mat("Black", new Color(0.045f, 0.045f, 0.04f));
        Material white = Mat("White", new Color(0.94f, 0.93f, 0.88f));
        Material rug = Mat("Living Rug", new Color(0.68f, 0.55f, 0.40f));
        Material rugDark = Mat("Rug Pattern Dark", new Color(0.35f, 0.20f, 0.14f));
        Material rugCoral = Mat("Rug Pattern Coral", new Color(0.72f, 0.34f, 0.31f));
        Material terraceRug = Mat("Terrace Rug", new Color(0.63f, 0.43f, 0.28f));
        Material cable = Mat("String Light Cable", new Color(0.055f, 0.045f, 0.035f), 0.55f);
        Material bulbGlow = EmissiveMat("String Light Glow", new Color(1.0f, 0.46f, 0.16f), 2.8f);
        Material[] colorfulBulbs = CreateColorfulBulbMaterials();
        Material neonGlow = EmissiveMat("Social Neon Pink", new Color(1.0f, 0.12f, 0.58f), 3.0f);

        GameObject root = new GameObject("DETAILS & ENVIRONMENT");
        Transform environment = Group("Island Environment", root.transform);
        Transform architecture = Group("Doors Windows & Steps", root.transform);
        Transform furniture = Group("Detailed Furniture", root.transform);
        Transform plants = Group("Plants", root.transform);
        Transform lighting = Group("Lighting", root.transform);
        Group(SocialSceneVersionMarker, root.transform);

        // Island setting: lawn around the house, a sandy beach behind it, and ocean.
        Box("Lawn", new Vector3(35, 0.18f, 31), new Vector3(0, -0.36f, 0), grass, environment);
        Box("Beach", new Vector3(35, 0.14f, 7), new Vector3(0, -0.29f, 14), sand, environment);
        Box("Ocean", new Vector3(44, 0.10f, 15), new Vector3(0, -0.25f, 24.5f), water, environment);
        Box("Front Path", new Vector3(2.6f, 0.09f, 5), new Vector3(0, -0.18f, -12.0f), path, environment);
        for (int i = 0; i < 5; i++)
            Box("Stepping Stone " + (i + 1), new Vector3(1.45f, 0.12f, 0.75f),
                new Vector3(0, -0.1f + i * 0.10f, -10.45f - i * 0.7f), path, environment);

        // Front stairs and terrace steps.
        for (int i = 0; i < 3; i++)
        {
            float width = 2.6f + i * 0.45f;
            Box("Entrance Step " + (i + 1), new Vector3(width, 0.18f, 0.65f),
                new Vector3(0, 0.09f + i * 0.16f, -10.35f - i * 0.48f), wood, architecture);
        }
        Box("Terrace Step", new Vector3(7.0f, 0.22f, 0.75f), new Vector3(0, 0.11f, 4.75f), wood, architecture);

        // Door leaves make the openings understandable from the top view.
        Door("Front Door L", new Vector3(-0.78f, 1.05f, -9.92f), new Vector3(1.4f, 2.1f, 0.08f), wood, architecture);
        Door("Front Door R", new Vector3(0.78f, 1.05f, -9.92f), new Vector3(1.4f, 2.1f, 0.08f), wood, architecture);

        // Exterior windows.
        float[] bedroomWindowZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < bedroomWindowZ.Length; i++)
        {
            float z = bedroomWindowZ[i];
            Window("West Bedroom Window " + (i + 1), new Vector3(-12.03f, 1.45f, z), true, glass, darkWood, architecture);
            Window("East Bedroom Window " + (i + 4), new Vector3(12.03f, 1.45f, z), true, glass, darkWood, architecture);
        }
        Window("Kitchen Window", new Vector3(-8.2f, 1.45f, -10.03f), false, glass, darkWood, architecture);
        Window("Lounge Window", new Vector3(8.2f, 1.45f, -10.03f), false, glass, darkWood, architecture);

        // Bedrooms: beds, pillows, bedside tables, wardrobes and lamps.
        float[] bedroomZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            Bedroom(new Vector3(-9.5f, 0, bedroomZ[i]), i == 2 ? rugCoral : blue, wood, white, metal, furniture, false);
            Bedroom(new Vector3(9.5f, 0, bedroomZ[i]), i == 0 ? teal : pink, wood, white, metal, furniture, true);
        }

        Bathroom(new Vector3(-5.9f, 0, 0), ceramic, glass, metal, furniture, false);
        Bathroom(new Vector3(5.9f, 0, 0), ceramic, glass, metal, furniture, true);

        // Kitchen with L-shaped counters, sink, cooker and refrigerator.
        Box("Kitchen Back Counter", new Vector3(4.6f, 0.88f, 0.62f), new Vector3(-8.8f, 0.44f, -2.1f), wood, furniture);
        Box("Kitchen Island", new Vector3(4.8f, 0.92f, 1.05f), new Vector3(-8.2f, 0.46f, -4.2f), ceramic, furniture);
        Box("Kitchen Worktop", new Vector3(4.7f, 0.08f, 0.72f), new Vector3(-8.8f, 0.92f, -2.1f), ceramic, furniture);
        Box("Fridge", new Vector3(0.75f, 1.85f, 0.7f), new Vector3(-11.3f, 0.925f, -2.1f), metal, furniture);
        Box("Sink", new Vector3(0.85f, 0.06f, 0.45f), new Vector3(-8.8f, 0.98f, -2.1f), metal, furniture);
        Box("Cooktop", new Vector3(0.75f, 0.04f, 0.48f), new Vector3(-7.3f, 0.98f, -2.1f), black, furniture);
        for (int i = 0; i < 4; i++)
            Box("Kitchen Stool " + (i + 1), new Vector3(0.55f, 0.68f, 0.55f), new Vector3(-9.65f + i * 0.95f, 0.34f, -5.0f), pink, furniture);

        // Dining and living area.
        SixSeatDiningSet("Kitchen Six Seat Dining", new Vector3(-8.0f, 0, -7.25f), wood, teal, furniture);
        DecoratedRug("Living Woven Rug", new Vector3(0, 0.045f, -1.0f), new Vector2(7.0f, 6.2f), rug, rugDark, rugCoral, furniture);
        ConversationRingSofa(new Vector3(0, 0, -1.0f), cream, rugCoral, teal, furniture);
        Cylinder("Round Living Coffee Table", new Vector3(0, 0.38f, -1.0f), new Vector3(0.72f, 0.14f, 0.72f), wood, furniture);

        // Lounge and game room.
        Sofa("Lounge Sofa South", new Vector3(8.0f, 0, -7.8f), Quaternion.identity, pink, furniture);
        Sofa("Lounge Sofa West", new Vector3(6.1f, 0, -5.8f), Quaternion.Euler(0, 90, 0), pink, furniture);
        Sofa("Lounge Sofa North", new Vector3(8.0f, 0, -3.8f), Quaternion.Euler(0, 180, 0), pink, furniture);
        DecoratedRug("Lounge Round Rug", new Vector3(8.0f, 0.055f, -5.8f), new Vector2(5.4f, 4.8f), rug, rugCoral, rugDark, furniture);
        Cylinder("Lounge Round Table", new Vector3(8.0f, 0.38f, -5.8f), new Vector3(0.65f, 0.14f, 0.65f), wood, furniture);
        NeonLandmark(new Vector3(11.75f, 1.55f, -5.8f), neonGlow, darkWood, lighting);

        // Terrace seating and planters.
        DiningSet(new Vector3(0, 0.10f, 7.25f), wood, cream, furniture);
        DecoratedRug("Terrace Dining Rug", new Vector3(0, 0.065f, 7.25f), new Vector2(5.0f, 3.2f), terraceRug, rugDark, cream, furniture);
        DiningSet(new Vector3(-12.9f, 0.08f, -3.4f), wood, cream, furniture);
        DiningSet(new Vector3(12.9f, 0.08f, -3.4f), wood, cream, furniture);

        // Dense tropical planting similar to the reference illustration.
        Vector3[] plantPositions =
        {
            new Vector3(-14.5f, 0, -8), new Vector3(14.5f, 0, -8), new Vector3(-14.7f, 0, 2),
            new Vector3(14.7f, 0, 2), new Vector3(-13.8f, 0, 10.5f), new Vector3(13.8f, 0, 10.5f),
            new Vector3(-6.0f, 0, 11.0f), new Vector3(6.0f, 0, 11.0f), new Vector3(-5.0f, 0, 4.8f),
            new Vector3(5.0f, 0, 4.8f), new Vector3(-2.8f, 0, -11.2f), new Vector3(3.0f, 0, -11.2f)
        };
        for (int i = 0; i < plantPositions.Length; i++)
        {
            if (i < 6) Palm("Palm " + i, plantPositions[i], wood, i % 2 == 0 ? leaf : leafLight, plants);
            else Bush("Bush " + i, plantPositions[i], i % 2 == 0 ? leaf : leafLight, plants);
        }

        // Warm point lights help the rooms read clearly in perspective view.
        Vector3[] lightPositions =
        {
            new Vector3(0, 2.65f, -1.0f),
            new Vector3(-8.0f, 2.5f, -7.25f),
            new Vector3(-8.2f, 2.5f, -3.5f),
            new Vector3(8.0f, 2.45f, -5.8f),
            new Vector3(0, 2.65f, 7.25f),
            new Vector3(-12.9f, 2.5f, -3.4f),
            new Vector3(12.9f, 2.5f, -3.4f)
        };
        foreach (Vector3 p in lightPositions) PointLight(p, lighting);
        foreach (float z in bedroomZ)
        {
            PointLight(new Vector3(-9.5f, 2.45f, z), lighting, "West Bedroom Light");
            PointLight(new Vector3(9.5f, 2.45f, z), lighting, "East Bedroom Light");
        }

        // Layered rugs and festoon lights echo the warm, intimate resort mood
        // in the visual reference while keeping the main paths unobstructed.
        foreach (float z in bedroomZ)
        {
            DecoratedRug("West Bedroom Runner", new Vector3(-8.6f, 0.035f, z), new Vector2(0.72f, 1.9f), blue, cream, rugDark, furniture);
            DecoratedRug("East Bedroom Runner", new Vector3(8.6f, 0.035f, z), new Vector2(0.72f, 1.9f), pink, cream, rugDark, furniture);
        }

        StringLights("Terrace Festoon A", new Vector3(-5.0f, 2.90f, 5.0f), new Vector3(5.0f, 2.90f, 5.0f), 25, cable, colorfulBulbs, lighting, true);
        StringLights("Terrace Festoon B", new Vector3(-5.0f, 2.95f, 7.0f), new Vector3(5.0f, 2.95f, 7.0f), 25, cable, colorfulBulbs, lighting, true);
        StringLights("Terrace Festoon C", new Vector3(-5.0f, 2.90f, 9.0f), new Vector3(5.0f, 2.90f, 9.0f), 25, cable, colorfulBulbs, lighting, true);
        AddLivingLightUpgrade(lighting, cable, colorfulBulbs);
        Lantern(new Vector3(-1.45f, 0.85f, -10.55f), metal, bulbGlow, lighting);
        Lantern(new Vector3(1.45f, 0.85f, -10.55f), metal, bulbGlow, lighting);

        ConfigureSocialAnchors();

        ImproveSceneLighting();
        ConfigureDayNight(root, lighting);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Selection.activeGameObject = root;
        Debug.Log("Details and island environment added successfully.");
    }

    [MenuItem("Love Producer/Upgrade Colorful Living Lights Only")]
    public static void UpgradeColorfulLivingLightsOnly()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        if (details == null)
        {
            Debug.LogError("Lighting upgrade stopped: DETAILS & ENVIRONMENT was not found in the active scene.");
            return;
        }

        Transform lighting = details.transform.Find("Lighting");
        if (lighting == null) lighting = Group("Lighting", details.transform);

        string[] replaceNames = { "Terrace Festoon A", "Terrace Festoon B", "Terrace Festoon C", "Living Light Upgrade" };
        foreach (string objectName in replaceNames)
        {
            Transform old = lighting.Find(objectName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        }

        Material cable = Mat("String Light Cable", new Color(0.055f, 0.045f, 0.035f), 0.55f);
        Material[] colorfulBulbs = CreateColorfulBulbMaterials();
        StringLights("Terrace Festoon A", new Vector3(-5.0f, 2.90f, 5.0f), new Vector3(5.0f, 2.90f, 5.0f), 25, cable, colorfulBulbs, lighting, true);
        StringLights("Terrace Festoon B", new Vector3(-5.0f, 2.95f, 7.0f), new Vector3(5.0f, 2.95f, 7.0f), 25, cable, colorfulBulbs, lighting, true);
        StringLights("Terrace Festoon C", new Vector3(-5.0f, 2.90f, 9.0f), new Vector3(5.0f, 2.90f, 9.0f), 25, cable, colorfulBulbs, lighting, true);
        AddLivingLightUpgrade(lighting, cable, colorfulBulbs);

        LoveProducerDayNightController controller = Object.FindAnyObjectByType<LoveProducerDayNightController>();
        if (controller != null)
        {
            controller.practicalLights = lighting.GetComponentsInChildren<Light>(true);
            controller.SetNight(true);
            EditorUtility.SetDirty(controller);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Colorful living lights upgraded. Save the scene with Command/Ctrl + S.");
    }

    [MenuItem("Love Producer/Upgrade Kitchen Table to Six Seats")]
    public static void UpgradeKitchenTableToSixSeats()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        if (furniture == null)
        {
            Debug.LogError("Kitchen table upgrade stopped: Detailed Furniture was not found.");
            return;
        }

        Vector3 center = new Vector3(-8.0f, 0, -7.25f);
        Transform existingGroup = furniture.Find("Kitchen Six Seat Dining");
        if (existingGroup != null) Undo.DestroyObjectImmediate(existingGroup.gameObject);

        // Remove only the old dining pieces nearest the kitchen table.
        for (int i = furniture.childCount - 1; i >= 0; i--)
        {
            Transform child = furniture.GetChild(i);
            bool diningPiece = child.name.StartsWith("Dining Table")
                || child.name.StartsWith("Dining Chair")
                || child.name.StartsWith("Table Pedestal");
            if (diningPiece && Vector3.Distance(child.position, center) < 2.2f)
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        Material wood = Mat("Warm Wood", new Color(0.38f, 0.22f, 0.12f));
        Material teal = Mat("Tropical Teal", new Color(0.16f, 0.55f, 0.58f));
        Transform newSet = SixSeatDiningSet("Kitchen Six Seat Dining", center, wood, teal, furniture);
        Undo.RegisterCreatedObjectUndo(newSet.gameObject, "Create Six Seat Kitchen Table");
        Selection.activeGameObject = newSet.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Kitchen dining table upgraded to six seats: three chairs on each long side.");
    }

    [MenuItem("Love Producer/Upgrade Terrace Floors and Extra Furniture")]
    public static void UpgradeTerraceFloorsAndExtraFurniture()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        if (details == null)
        {
            Debug.LogError("Furniture upgrade stopped: DETAILS & ENVIRONMENT was not found.");
            return;
        }

        Transform environment = details.transform.Find("Island Environment");
        Transform furniture = details.transform.Find("Detailed Furniture");
        if (environment == null || furniture == null)
        {
            Debug.LogError("Furniture upgrade stopped: environment or furniture group was not found.");
            return;
        }

        Transform oldEnvironment = environment.Find("Side Terrace Floor Upgrade");
        if (oldEnvironment != null) Undo.DestroyObjectImmediate(oldEnvironment.gameObject);
        Transform oldFurniture = furniture.Find("Extra Villa Furniture");
        if (oldFurniture != null) Undo.DestroyObjectImmediate(oldFurniture.gameObject);

        Material wood = Mat("Warm Wood", new Color(0.38f, 0.22f, 0.12f));
        Material darkWood = Mat("Dark Wood", new Color(0.20f, 0.11f, 0.07f));
        Material ceramic = Mat("Ceramic", new Color(0.91f, 0.91f, 0.86f), 0.5f);
        Material teal = Mat("Tropical Teal", new Color(0.16f, 0.55f, 0.58f));
        Material pink = Mat("Pink Fabric", new Color(0.78f, 0.43f, 0.58f));
        Material leaf = Mat("Leaves", new Color(0.18f, 0.43f, 0.16f));

        Transform terraceUpgrade = Group("Side Terrace Floor Upgrade", environment);
        AddSideTerraceDeck("West Side Terrace Deck", new Vector3(-12.9f, -0.08f, -3.4f), wood, darkWood, terraceUpgrade);
        AddSideTerraceDeck("East Side Terrace Deck", new Vector3(12.9f, -0.08f, -3.4f), wood, darkWood, terraceUpgrade);

        Transform extras = Group("Extra Villa Furniture", furniture);
        // Kitchen storage and serving furniture.
        Box("Kitchen Open Shelf", new Vector3(2.2f, 1.55f, 0.34f), new Vector3(-10.2f, 0.78f, -1.72f), wood, extras);
        for (int i = 0; i < 3; i++)
            Box("Kitchen Shelf " + (i + 1), new Vector3(2.0f, 0.08f, 0.42f), new Vector3(-10.2f, 0.38f + i * 0.48f, -1.50f), ceramic, extras);
        Box("Dining Sideboard", new Vector3(2.8f, 0.78f, 0.48f), new Vector3(-8.0f, 0.39f, -9.55f), wood, extras);

        // Lounge media wall and movable side tables.
        Box("Lounge Media Console", new Vector3(0.48f, 0.62f, 2.6f), new Vector3(11.55f, 0.31f, -5.8f), darkWood, extras);
        Box("Lounge Television", new Vector3(0.10f, 1.25f, 2.15f), new Vector3(11.30f, 1.28f, -5.8f), darkWood, extras);
        Cylinder("Lounge Side Table A", new Vector3(6.15f, 0.34f, -8.3f), new Vector3(0.38f, 0.34f, 0.38f), wood, extras);
        Cylinder("Lounge Side Table B", new Vector3(9.85f, 0.34f, -8.3f), new Vector3(0.38f, 0.34f, 0.38f), wood, extras);

        // A slim entrance console and decorative objects make arrival feel intentional.
        Box("Entrance Console", new Vector3(2.2f, 0.76f, 0.42f), new Vector3(0, 0.38f, -9.45f), wood, extras);
        Cylinder("Entrance Vase", new Vector3(-0.55f, 0.98f, -9.45f), new Vector3(0.20f, 0.34f, 0.20f), teal, extras);
        Sphere("Entrance Flowers", new Vector3(-0.55f, 1.28f, -9.45f), new Vector3(0.42f, 0.36f, 0.42f), pink, extras);

        // Low planting defines the deck edge while the surrounding surface remains lawn.
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 14.45f;
            Box("Side Terrace Planter", new Vector3(0.42f, 0.42f, 3.6f), new Vector3(x, 0.13f, -3.4f), darkWood, extras);
            for (int i = -2; i <= 2; i++)
                Sphere("Side Terrace Plant", new Vector3(x, 0.58f, -3.4f + i * 0.72f), new Vector3(0.52f, 0.58f, 0.52f), leaf, extras);
        }

        Undo.RegisterCreatedObjectUndo(terraceUpgrade.gameObject, "Create Side Terrace Floors");
        Undo.RegisterCreatedObjectUndo(extras.gameObject, "Create Extra Villa Furniture");
        Selection.activeGameObject = terraceUpgrade.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Side terrace decks and extra villa furniture added. Grass remains visible around the decks.");
    }

    [MenuItem("Love Producer/Apply Cinematic Camera and Night Look")]
    public static void ApplyCinematicCameraAndNightLook()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("Cinematic look stopped: no Main Camera was found.");
            return;
        }

        Undo.RecordObject(camera.transform, "Set Cinematic Villa Camera");
        Undo.RecordObject(camera, "Set Cinematic Villa Camera");
        camera.orthographic = false;
        camera.fieldOfView = 46f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 150f;
        camera.transform.position = new Vector3(0f, 32f, -23f);
        camera.transform.LookAt(new Vector3(0f, 0.35f, 0.8f));
        camera.backgroundColor = new Color(0.025f, 0.055f, 0.10f);

        UniversalAdditionalCameraData cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null) cameraData = Undo.AddComponent<UniversalAdditionalCameraData>(camera.gameObject);
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        cameraData.antialiasingQuality = AntialiasingQuality.High;

        const string profilePath = MaterialFolder + "/Love Producer Cinematic Night.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            EnsureFolder("Assets/Generated");
            EnsureFolder(MaterialFolder);
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        ConfigureCinematicProfile(profile);
        GameObject volumeObject = GameObject.Find("Love Producer Cinematic Volume");
        if (volumeObject == null)
        {
            volumeObject = new GameObject("Love Producer Cinematic Volume");
            Undo.RegisterCreatedObjectUndo(volumeObject, "Create Cinematic Volume");
        }
        Volume volume = volumeObject.GetComponent<Volume>();
        if (volume == null) volume = Undo.AddComponent<Volume>(volumeObject);
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.sharedProfile = profile;

        LoveProducerDayNightController controller = Object.FindAnyObjectByType<LoveProducerDayNightController>();
        if (controller != null)
        {
            Undo.RecordObject(controller, "Improve Night Lighting");
            controller.nightAmbient = new Color(0.14f, 0.16f, 0.22f);
            AddCinematicFillLights(controller);
            controller.SetNight(true);
            EditorUtility.SetDirty(controller);
        }

        RenderSettings.reflectionIntensity = 0.72f;
        EditorUtility.SetDirty(camera);
        EditorUtility.SetDirty(cameraData);
        EditorUtility.SetDirty(volume);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Cinematic camera, blue-hour fill light and URP post-processing applied.");
    }

    [MenuItem("Love Producer/Enable Adjustable Day Camera")]
    public static void EnableAdjustableDayCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("Adjustable day camera stopped: no camera tagged MainCamera was found.");
            return;
        }

        Undo.RecordObject(camera, "Enable Adjustable Day Camera");
        Undo.RecordObject(camera.transform, "Enable Adjustable Day Camera");
        camera.orthographic = false;
        camera.fieldOfView = 46f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 150f;

        LoveProducerOrbitCameraController orbit = camera.GetComponent<LoveProducerOrbitCameraController>();
        if (orbit == null)
            orbit = Undo.AddComponent<LoveProducerOrbitCameraController>(camera.gameObject);
        else
            Undo.RecordObject(orbit, "Configure Adjustable Day Camera");

        // Keeps the current villa composition while allowing the player to orbit it in daytime.
        orbit.ConfigureDefaultView(new Vector3(0f, 0.5f, 0.8f), 0f, 55f, 40f);
        orbit.AllowAtNight = false;

        EditorUtility.SetDirty(camera);
        EditorUtility.SetDirty(camera.transform);
        EditorUtility.SetDirty(orbit);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = camera.gameObject;
        EditorGUIUtility.PingObject(camera.gameObject);
        Debug.Log("Adjustable daytime camera enabled. In Play mode: right-drag to orbit, wheel to zoom, Q/E to rotate, R to reset. Night view stays locked by default.");
    }

    private static void AddCinematicFillLights(LoveProducerDayNightController controller)
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform lighting = details != null ? details.transform.Find("Lighting") : null;
        if (lighting == null) return;

        Transform old = lighting.Find("Cinematic Fill Lights");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group("Cinematic Fill Lights", lighting);

        Vector3[] warmPositions =
        {
            new Vector3(0f, 2.6f, -6.2f),
            new Vector3(0f, 2.7f, 2.4f),
            new Vector3(-4.4f, 2.5f, -3.4f),
            new Vector3(4.4f, 2.5f, -3.4f)
        };
        foreach (Vector3 position in warmPositions)
        {
            PointLight(position, group, "Soft Interior Fill");
            Light light = group.GetChild(group.childCount - 1).GetComponent<Light>();
            light.color = new Color(1.0f, 0.72f, 0.50f);
            light.intensity = 0.85f;
            light.range = 6.5f;
        }

        Vector3[] gardenPositions =
        {
            new Vector3(-10.5f, 2.0f, -8.8f),
            new Vector3(10.5f, 2.0f, -8.8f),
            new Vector3(-10.5f, 2.0f, 9.8f),
            new Vector3(10.5f, 2.0f, 9.8f)
        };
        foreach (Vector3 position in gardenPositions)
        {
            PointLight(position, group, "Blue Garden Fill");
            Light light = group.GetChild(group.childCount - 1).GetComponent<Light>();
            light.color = new Color(0.28f, 0.46f, 0.72f);
            light.intensity = 0.48f;
            light.range = 7.5f;
        }

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Create Cinematic Fill Lights");
        controller.practicalLights = lighting.GetComponentsInChildren<Light>(true);
    }

    private static void ConfigureCinematicProfile(VolumeProfile profile)
    {
        if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.intensity.Override(0.48f);
        bloom.threshold.Override(0.85f);
        bloom.scatter.Override(0.68f);

        if (!profile.TryGet(out Tonemapping tonemapping)) tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.active = true;
        tonemapping.mode.Override(TonemappingMode.ACES);

        if (!profile.TryGet(out ColorAdjustments color)) color = profile.Add<ColorAdjustments>(true);
        color.active = true;
        color.postExposure.Override(0.58f);
        color.contrast.Override(12f);
        color.saturation.Override(6f);
        color.colorFilter.Override(new Color(1.0f, 0.96f, 0.90f));

        if (!profile.TryGet(out WhiteBalance whiteBalance)) whiteBalance = profile.Add<WhiteBalance>(true);
        whiteBalance.active = true;
        whiteBalance.temperature.Override(8f);
        whiteBalance.tint.Override(2f);

        if (!profile.TryGet(out Vignette vignette)) vignette = profile.Add<Vignette>(true);
        vignette.active = true;
        vignette.color.Override(new Color(0.015f, 0.025f, 0.05f));
        vignette.intensity.Override(0.18f);
        vignette.smoothness.Override(0.72f);
        EditorUtility.SetDirty(profile);
    }

    [MenuItem("Love Producer/Add Bright Ceiling Lights to Every Room")]
    public static void AddBrightCeilingLightsToEveryRoom()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform lighting = details != null ? details.transform.Find("Lighting") : null;
        if (lighting == null)
        {
            Debug.LogError("Room lighting upgrade stopped: the Lighting group was not found.");
            return;
        }

        Transform old = lighting.Find("All Room Ceiling Lights");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group("All Room Ceiling Lights", lighting);
        Material warmGlow = EmissiveMat("Room Ceiling Warm Glow", new Color(1.0f, 0.72f, 0.42f), 3.8f);
        Material softGlow = EmissiveMat("Room Ceiling Soft Glow", new Color(1.0f, 0.88f, 0.68f), 3.2f);

        float[] bedroomZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            RoomCeilingLight("Bedroom " + (i + 1) + " Ceiling Light", new Vector3(-9.5f, 2.62f, bedroomZ[i]), softGlow, 2.15f, 5.2f, group);
            RoomCeilingLight("Bedroom " + (i + 4) + " Ceiling Light", new Vector3(9.5f, 2.62f, bedroomZ[i]), softGlow, 2.15f, 5.2f, group);
        }

        RoomCeilingLight("West Bathroom Ceiling Light", new Vector3(-5.9f, 2.55f, 0f), softGlow, 1.85f, 4.5f, group);
        RoomCeilingLight("East Bathroom Ceiling Light", new Vector3(5.9f, 2.55f, 0f), softGlow, 1.85f, 4.5f, group);
        RoomCeilingLight("Kitchen Ceiling Light", new Vector3(-8.5f, 2.70f, -3.4f), warmGlow, 2.25f, 6.0f, group);
        RoomCeilingLight("Dining Ceiling Light", new Vector3(-8.0f, 2.70f, -7.25f), warmGlow, 2.15f, 5.8f, group);
        RoomCeilingLight("Living Ceiling Light A", new Vector3(-1.8f, 2.75f, -1.0f), warmGlow, 2.05f, 5.5f, group);
        RoomCeilingLight("Living Ceiling Light B", new Vector3(1.8f, 2.75f, -1.0f), warmGlow, 2.05f, 5.5f, group);
        RoomCeilingLight("Lounge Ceiling Light", new Vector3(8.0f, 2.65f, -5.8f), warmGlow, 2.30f, 6.0f, group);
        RoomCeilingLight("Entrance Hall Ceiling Light", new Vector3(0f, 2.65f, -7.2f), warmGlow, 2.05f, 5.8f, group);
        RoomCeilingLight("North Hall Ceiling Light", new Vector3(0f, 2.65f, 2.5f), softGlow, 1.80f, 5.5f, group);
        RoomCeilingLight("Main Terrace Ceiling Light", new Vector3(0f, 2.85f, 7.25f), warmGlow, 1.85f, 6.0f, group);

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Add Ceiling Lights to Every Room");
        LoveProducerDayNightController controller = Object.FindAnyObjectByType<LoveProducerDayNightController>();
        if (controller != null)
        {
            controller.practicalLights = lighting.GetComponentsInChildren<Light>(true);
            controller.nightAmbient = new Color(0.17f, 0.18f, 0.24f);
            controller.RefreshPracticalLights();
            controller.SetNight(true);
            EditorUtility.SetDirty(controller);
        }

        Selection.activeGameObject = group.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Bright ceiling lights added to every bedroom and shared room.");
    }

    private static void RoomCeilingLight(string name, Vector3 position, Material glow, float intensity, float range, Transform parent)
    {
        Transform fixture = Group(name, parent);
        Cylinder("Ceiling Fixture", position, new Vector3(0.22f, 0.035f, 0.22f), glow, fixture);
        GameObject lamp = new GameObject("Room Light");
        lamp.transform.SetParent(fixture);
        lamp.transform.position = position - Vector3.up * 0.10f;
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.76f, 0.53f);
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    [MenuItem("Love Producer/Upgrade Main Terrace Lighting")]
    public static void UpgradeMainTerraceLighting()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform lighting = details != null ? details.transform.Find("Lighting") : null;
        if (lighting == null)
        {
            Debug.LogError("Terrace lighting upgrade stopped: the Lighting group was not found.");
            return;
        }

        Transform old = lighting.Find("Main Terrace Lighting Upgrade");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group("Main Terrace Lighting Upgrade", lighting);
        Material frame = Mat("Terrace Lantern Frame", new Color(0.16f, 0.10f, 0.07f), 0.65f);
        Material glow = EmissiveMat("Terrace Lantern Glow", new Color(1.0f, 0.52f, 0.18f), 4.2f);

        Vector3[] lanternPositions =
        {
            new Vector3(-4.35f, 0.72f, 5.25f),
            new Vector3(4.35f, 0.72f, 5.25f),
            new Vector3(-4.35f, 0.72f, 9.15f),
            new Vector3(4.35f, 0.72f, 9.15f)
        };
        for (int i = 0; i < lanternPositions.Length; i++)
            TerracePostLantern("Terrace Post Lantern " + (i + 1), lanternPositions[i], frame, glow, group);

        RoomCeilingLight("Terrace Dining Light Left", new Vector3(-1.65f, 2.72f, 7.25f), glow, 1.65f, 5.0f, group);
        RoomCeilingLight("Terrace Dining Light Right", new Vector3(1.65f, 2.72f, 7.25f), glow, 1.65f, 5.0f, group);

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Upgrade Main Terrace Lighting");
        LoveProducerDayNightController controller = Object.FindAnyObjectByType<LoveProducerDayNightController>();
        if (controller != null)
        {
            controller.practicalLights = lighting.GetComponentsInChildren<Light>(true);
            controller.RefreshPracticalLights();
            controller.SetNight(true);
            EditorUtility.SetDirty(controller);
        }

        Selection.activeGameObject = group.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Main terrace upgraded with four post lanterns and two dining lights.");
    }

    private static void TerracePostLantern(string name, Vector3 position, Material frame, Material glow, Transform parent)
    {
        Transform lantern = Group(name, parent);
        Box("Post", new Vector3(0.13f, 0.72f, 0.13f), position - Vector3.up * 0.36f, frame, lantern);
        Box("Lantern Housing", new Vector3(0.30f, 0.40f, 0.30f), position, frame, lantern);
        Sphere("Lantern Bulb", position, Vector3.one * 0.18f, glow, lantern);

        GameObject lamp = new GameObject("Terrace Warm Light");
        lamp.transform.SetParent(lantern);
        lamp.transform.position = position + Vector3.up * 0.05f;
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.62f, 0.32f);
        light.intensity = 1.45f;
        light.range = 4.6f;
        light.shadows = LightShadows.None;
    }

    [MenuItem("Love Producer/Add Lifestyle Lighting and Table Details")]
    public static void AddLifestyleLightingAndTableDetails()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform lighting = details != null ? details.transform.Find("Lighting") : null;
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        if (lighting == null || furniture == null)
        {
            Debug.LogError("Lifestyle detail upgrade stopped: Lighting or Detailed Furniture was not found.");
            return;
        }

        Transform oldLights = lighting.Find("Lifestyle Accent Lights");
        if (oldLights != null) Undo.DestroyObjectImmediate(oldLights.gameObject);
        Transform oldProps = furniture.Find("Lifestyle Table Props");
        if (oldProps != null) Undo.DestroyObjectImmediate(oldProps.gameObject);

        Transform lightGroup = Group("Lifestyle Accent Lights", lighting);
        Transform propGroup = Group("Lifestyle Table Props", furniture);
        Material metal = Mat("Lifestyle Brass", new Color(0.45f, 0.27f, 0.10f), 0.72f);
        Material warmGlow = EmissiveMat("Lifestyle Warm Glow", new Color(1.0f, 0.50f, 0.16f), 4.5f);
        Material ceramic = Mat("Lifestyle Ceramic", new Color(0.92f, 0.88f, 0.78f), 0.42f);
        Material glass = Mat("Lifestyle Glass", new Color(0.48f, 0.76f, 0.82f), 0.82f);
        Material coral = Mat("Lifestyle Coral", new Color(0.82f, 0.30f, 0.24f));
        Material fruit = Mat("Lifestyle Fruit", new Color(0.95f, 0.52f, 0.10f));
        Material green = Mat("Lifestyle Green", new Color(0.20f, 0.48f, 0.18f));

        // Three pendants make the open kitchen island a visual focal point.
        for (int i = 0; i < 3; i++)
            PendantLight("Kitchen Pendant " + (i + 1), new Vector3(-9.35f + i * 1.15f, 2.35f, -4.2f), metal, warmGlow, 1.15f, 3.8f, lightGroup);

        // One practical bedside light per bedroom.
        float[] bedroomZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            TableLamp("West Bedroom " + (i + 1) + " Bedside Light", new Vector3(-8.45f, 0.88f, bedroomZ[i] + 0.55f), metal, warmGlow, lightGroup);
            TableLamp("East Bedroom " + (i + 4) + " Bedside Light", new Vector3(8.45f, 0.88f, bedroomZ[i] + 0.55f), metal, warmGlow, lightGroup);
        }

        FloorLamp("Living Floor Lamp Left", new Vector3(-3.45f, 0f, -2.8f), metal, warmGlow, lightGroup);
        FloorLamp("Living Floor Lamp Right", new Vector3(3.45f, 0f, -2.8f), metal, warmGlow, lightGroup);

        // Low path lights guide the arrival route without washing out the garden.
        for (int i = 0; i < 4; i++)
        {
            float z = -11.0f - i * 0.85f;
            PathLight("West Path Light " + (i + 1), new Vector3(-1.45f, 0.22f, z), metal, warmGlow, lightGroup);
            PathLight("East Path Light " + (i + 1), new Vector3(1.45f, 0.22f, z), metal, warmGlow, lightGroup);
        }

        // Six place settings on the kitchen dining table.
        float[] diningX = { -8.82f, -8.0f, -7.18f };
        int place = 1;
        foreach (float x in diningX)
        {
            PlaceSetting("Dining Place " + place++, new Vector3(x, 0.87f, -6.98f), ceramic, glass, propGroup);
            PlaceSetting("Dining Place " + place++, new Vector3(x, 0.87f, -7.52f), ceramic, glass, propGroup);
        }
        FruitBowl("Dining Fruit Bowl", new Vector3(-8.0f, 0.92f, -7.25f), ceramic, fruit, green, propGroup);

        // Kitchen, living, lounge and terrace tabletop storytelling props.
        FruitBowl("Kitchen Island Fruit", new Vector3(-8.2f, 1.05f, -4.2f), ceramic, fruit, green, propGroup);
        CoffeeTableProps("Living Coffee Table Details", new Vector3(0f, 0.57f, -1.0f), ceramic, glass, green, propGroup);
        CoffeeTableProps("Lounge Coffee Table Details", new Vector3(8.0f, 0.57f, -5.8f), ceramic, glass, coral, propGroup);
        CandleCluster("Terrace Candle Cluster", new Vector3(0f, 0.93f, 7.25f), metal, warmGlow, propGroup);

        Undo.RegisterCreatedObjectUndo(lightGroup.gameObject, "Add Lifestyle Accent Lights");
        Undo.RegisterCreatedObjectUndo(propGroup.gameObject, "Add Lifestyle Table Props");
        LoveProducerDayNightController controller = Object.FindAnyObjectByType<LoveProducerDayNightController>();
        if (controller != null)
        {
            controller.practicalLights = lighting.GetComponentsInChildren<Light>(true);
            controller.RefreshPracticalLights();
            controller.SetNight(true);
            EditorUtility.SetDirty(controller);
        }
        Selection.activeGameObject = propGroup.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Lifestyle lighting and tabletop details added throughout the villa.");
    }

    private static void PendantLight(string name, Vector3 position, Material frame, Material glow, float intensity, float range, Transform parent)
    {
        Transform fixture = Group(name, parent);
        Box("Pendant Cord", new Vector3(0.025f, 0.62f, 0.025f), position + Vector3.up * 0.31f, frame, fixture);
        Cylinder("Pendant Shade", position, new Vector3(0.25f, 0.16f, 0.25f), frame, fixture);
        Sphere("Pendant Bulb", position - Vector3.up * 0.12f, Vector3.one * 0.14f, glow, fixture);
        AddAccentPointLight(position - Vector3.up * 0.18f, intensity, range, fixture);
    }

    private static void TableLamp(string name, Vector3 position, Material frame, Material glow, Transform parent)
    {
        Transform fixture = Group(name, parent);
        Cylinder("Lamp Base", position - Vector3.up * 0.18f, new Vector3(0.14f, 0.18f, 0.14f), frame, fixture);
        Cylinder("Lamp Shade", position + Vector3.up * 0.06f, new Vector3(0.24f, 0.20f, 0.24f), glow, fixture);
        AddAccentPointLight(position + Vector3.up * 0.08f, 0.75f, 2.8f, fixture);
    }

    private static void FloorLamp(string name, Vector3 position, Material frame, Material glow, Transform parent)
    {
        Transform fixture = Group(name, parent);
        Cylinder("Floor Lamp Base", position + Vector3.up * 0.05f, new Vector3(0.25f, 0.05f, 0.25f), frame, fixture);
        Box("Floor Lamp Stem", new Vector3(0.055f, 1.45f, 0.055f), position + Vector3.up * 0.78f, frame, fixture);
        Cylinder("Floor Lamp Shade", position + Vector3.up * 1.52f, new Vector3(0.34f, 0.25f, 0.34f), glow, fixture);
        AddAccentPointLight(position + Vector3.up * 1.48f, 1.05f, 3.6f, fixture);
    }

    private static void PathLight(string name, Vector3 position, Material frame, Material glow, Transform parent)
    {
        Transform fixture = Group(name, parent);
        Box("Path Post", new Vector3(0.09f, 0.42f, 0.09f), position, frame, fixture);
        Sphere("Path Glow", position + Vector3.up * 0.23f, Vector3.one * 0.13f, glow, fixture);
        AddAccentPointLight(position + Vector3.up * 0.24f, 0.42f, 2.2f, fixture);
    }

    private static void AddAccentPointLight(Vector3 position, float intensity, float range, Transform parent)
    {
        GameObject lamp = new GameObject("Accent Light");
        lamp.transform.SetParent(parent);
        lamp.transform.position = position;
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.64f, 0.36f);
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    private static void PlaceSetting(string name, Vector3 position, Material ceramic, Material glass, Transform parent)
    {
        Transform setting = Group(name, parent);
        Cylinder("Plate", position, new Vector3(0.18f, 0.018f, 0.18f), ceramic, setting);
        Cylinder("Glass", position + new Vector3(0.22f, 0.10f, 0), new Vector3(0.055f, 0.10f, 0.055f), glass, setting);
    }

    private static void FruitBowl(string name, Vector3 position, Material bowl, Material fruit, Material leaves, Transform parent)
    {
        Transform group = Group(name, parent);
        Cylinder("Bowl", position, new Vector3(0.24f, 0.07f, 0.24f), bowl, group);
        Sphere("Fruit A", position + new Vector3(-0.10f, 0.12f, 0), Vector3.one * 0.14f, fruit, group);
        Sphere("Fruit B", position + new Vector3(0.10f, 0.13f, 0.04f), Vector3.one * 0.13f, fruit, group);
        Sphere("Fruit C", position + new Vector3(0, 0.15f, -0.08f), Vector3.one * 0.12f, leaves, group);
    }

    private static void CoffeeTableProps(string name, Vector3 position, Material ceramic, Material glass, Material accent, Transform parent)
    {
        Transform group = Group(name, parent);
        Cylinder("Decorative Tray", position, new Vector3(0.28f, 0.025f, 0.28f), ceramic, group);
        Cylinder("Drink Glass", position + new Vector3(0.18f, 0.10f, 0), new Vector3(0.06f, 0.10f, 0.06f), glass, group);
        Sphere("Small Plant", position + new Vector3(-0.13f, 0.15f, 0.03f), new Vector3(0.20f, 0.24f, 0.20f), accent, group);
    }

    private static void CandleCluster(string name, Vector3 position, Material holder, Material glow, Transform parent)
    {
        Transform group = Group(name, parent);
        for (int i = -1; i <= 1; i++)
        {
            Vector3 candle = position + new Vector3(i * 0.18f, Mathf.Abs(i) * 0.03f, 0);
            Cylinder("Candle", candle, new Vector3(0.055f, 0.12f, 0.055f), holder, group);
            Sphere("Candle Flame", candle + Vector3.up * 0.16f, new Vector3(0.055f, 0.09f, 0.055f), glow, group);
        }
    }

    [MenuItem("Love Producer/Apply Safe Material Detail Upgrade")]
    public static void ApplySafeMaterialDetailUpgrade()
    {
        EnsureFolder("Assets/Generated");
        EnsureFolder(MaterialFolder);
        Texture2D plasterTexture = CreateDetailTexture("Warm_Plaster_Texture", TexturePattern.Plaster);
        Texture2D woodTexture = CreateDetailTexture("Warm_Wood_Texture", TexturePattern.Wood);
        Texture2D stoneTexture = CreateDetailTexture("Light_Stone_Texture", TexturePattern.Stone);
        Texture2D quartzTexture = CreateDetailTexture("Quartz_Worktop_Texture", TexturePattern.Quartz);

        Material plaster = DetailedMat("Detailed Warm Plaster", plasterTexture, 0.16f, new Vector2(3.0f, 3.0f));
        Material wood = DetailedMat("Detailed Warm Wood", woodTexture, 0.24f, new Vector2(2.4f, 2.4f));
        Material stone = DetailedMat("Detailed Light Stone", stoneTexture, 0.32f, new Vector2(3.2f, 3.2f));
        Material quartz = DetailedMat("Detailed Quartz Worktop", quartzTexture, 0.48f, new Vector2(2.2f, 2.2f));

        int changed = 0;
        GameObject floorPlan = GameObject.Find("LOVE PRODUCER FLOOR PLAN (24m x 20m)");
        Transform walls = floorPlan != null ? floorPlan.transform.Find("Walls") : null;
        Transform floors = floorPlan != null ? floorPlan.transform.Find("Floors") : null;
        if (walls != null)
        {
            foreach (Renderer renderer in walls.GetComponentsInChildren<Renderer>(true))
            {
                string materialName = renderer.sharedMaterial != null ? renderer.sharedMaterial.name : string.Empty;
                if (!materialName.Contains("Wall")) continue;
                Undo.RecordObject(renderer, "Apply Plaster Material");
                renderer.sharedMaterial = plaster;
                changed++;
            }
        }

        if (floors != null)
        {
            foreach (Renderer renderer in floors.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.name == "Foundation") continue;
                Undo.RecordObject(renderer, "Apply Detailed Floor Material");
                renderer.sharedMaterial = renderer.name.Contains("Terrace") ? wood : stone;
                changed++;
            }
        }

        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        Transform environment = details != null ? details.transform.Find("Island Environment") : null;
        if (furniture != null)
        {
            foreach (Renderer renderer in furniture.GetComponentsInChildren<Renderer>(true))
            {
                string objectName = renderer.name;
                if (IsProtectedSoftFurniture(renderer.transform)) continue;
                bool worktop = objectName.Contains("Worktop") || objectName == "Kitchen Island" || objectName.Contains("Sideboard");
                bool wooden = objectName.Contains("Table") || objectName.Contains("Counter") || objectName.Contains("Console")
                    || objectName.Contains("Shelf") || objectName.Contains("Wardrobe") || objectName.Contains("Bed Frame");
                if (!worktop && !wooden) continue;
                Undo.RecordObject(renderer, "Apply Detailed Furniture Material");
                renderer.sharedMaterial = worktop ? quartz : wood;
                changed++;
            }
        }

        if (environment != null)
        {
            foreach (Renderer renderer in environment.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.name.Contains("Deck")) continue;
                Undo.RecordObject(renderer, "Apply Detailed Deck Material");
                renderer.sharedMaterial = wood;
                changed++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Safe material detail upgrade applied to " + changed + " renderers. Sofas, cushions and rugs were preserved.");
    }

    private enum TexturePattern { Plaster, Wood, Stone, Quartz }

    private static Texture2D CreateDetailTexture(string name, TexturePattern pattern)
    {
        const int size = 128;
        string path = MaterialFolder + "/" + name + ".png";
        Texture2D generated = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color baseColor = pattern == TexturePattern.Plaster ? new Color(0.82f, 0.77f, 0.68f)
            : pattern == TexturePattern.Wood ? new Color(0.40f, 0.235f, 0.12f)
            : pattern == TexturePattern.Quartz ? new Color(0.86f, 0.84f, 0.78f)
            : new Color(0.73f, 0.68f, 0.59f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float noise = Mathf.PerlinNoise(x * 0.075f + 3.1f, y * 0.075f + 7.4f) - 0.5f;
            float detail = Mathf.PerlinNoise(x * 0.24f + 11.2f, y * 0.24f + 2.7f) - 0.5f;
            float variation;
            if (pattern == TexturePattern.Wood)
            {
                float grain = Mathf.Sin((y + noise * 13f) * 0.30f) * 0.5f;
                variation = noise * 0.16f + grain * 0.12f + detail * 0.035f;
            }
            else if (pattern == TexturePattern.Stone)
            {
                float groutX = x % 42 < 2 ? -0.16f : 0f;
                float groutY = y % 42 < 2 ? -0.16f : 0f;
                variation = noise * 0.12f + detail * 0.04f + groutX + groutY;
            }
            else if (pattern == TexturePattern.Quartz)
            {
                float fleck = detail > 0.32f ? -0.18f : 0f;
                variation = noise * 0.055f + fleck;
            }
            else variation = noise * 0.075f + detail * 0.025f;
            generated.SetPixel(x, y, new Color(
                Mathf.Clamp01(baseColor.r + variation),
                Mathf.Clamp01(baseColor.g + variation),
                Mathf.Clamp01(baseColor.b + variation), 1f));
        }
        generated.Apply();
        File.WriteAllBytes(path, generated.EncodeToPNG());
        Object.DestroyImmediate(generated);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Material DetailedMat(string name, Texture2D texture, float smoothness, Vector2 tiling)
    {
        Material material = Mat(name, Color.white, smoothness);
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", tiling);
        }
        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
            material.SetTextureScale("_MainTex", tiling);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static bool IsProtectedSoftFurniture(Transform target)
    {
        Transform current = target;
        while (current != null)
        {
            string name = current.name;
            if (name.Contains("Sofa") || name.Contains("Seat") || name.Contains("Cushion") || name.Contains("Rug")) return true;
            current = current.parent;
        }
        return false;
    }

    [MenuItem("Love Producer/Refine Living and Lounge Rugs")]
    public static void RefineLivingAndLoungeRugs()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        if (furniture == null)
        {
            Debug.LogError("Rug refinement stopped: Detailed Furniture was not found.");
            return;
        }

        Transform livingRug = furniture.Find("Living Woven Rug");
        Transform loungeRug = furniture.Find("Lounge Round Rug");
        if (livingRug == null || loungeRug == null)
        {
            Debug.LogError("Rug refinement stopped: the living or lounge rug group was not found.");
            return;
        }

        // Keep the user's current base color and remove every raised pattern
        // element so the central living rug becomes clean and solid.
        for (int i = livingRug.childCount - 1; i >= 0; i--)
        {
            Transform child = livingRug.GetChild(i);
            if (child.name != "Rug Base") Undo.DestroyObjectImmediate(child.gameObject);
        }

        Renderer loungeBase = null;
        for (int i = loungeRug.childCount - 1; i >= 0; i--)
        {
            Transform child = loungeRug.GetChild(i);
            if (child.name == "Rug Base") loungeBase = child.GetComponent<Renderer>();
            if (child.name.StartsWith("Woven Stripe")) Undo.DestroyObjectImmediate(child.gameObject);
        }

        if (loungeBase != null)
        {
            Color baseColor = loungeBase.sharedMaterial != null ? loungeBase.sharedMaterial.color : new Color(0.72f, 0.58f, 0.56f);
            Texture2D woven = CreateWovenRugTexture("Lounge_Subtle_Woven_Texture", baseColor);
            Material loungeMaterial = DetailedMat("Lounge Subtle Woven Rug", woven, 0.12f, new Vector2(4.5f, 4.5f));
            Undo.RecordObject(loungeBase, "Apply Lounge Woven Material");
            loungeBase.sharedMaterial = loungeMaterial;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = livingRug.gameObject;
        Debug.Log("Living rug changed to its current solid color; lounge rug changed to a subtle woven texture.");
    }

    private static Texture2D CreateWovenRugTexture(string name, Color baseColor)
    {
        const int size = 128;
        string path = MaterialFolder + "/" + name + ".png";
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float horizontalThread = (x % 6 == 0) ? -0.045f : 0f;
            float verticalThread = (y % 6 == 0) ? -0.035f : 0f;
            float noise = (Mathf.PerlinNoise(x * 0.18f, y * 0.18f) - 0.5f) * 0.035f;
            float variation = horizontalThread + verticalThread + noise;
            texture.SetPixel(x, y, new Color(
                Mathf.Clamp01(baseColor.r + variation),
                Mathf.Clamp01(baseColor.g + variation),
                Mathf.Clamp01(baseColor.b + variation), 1f));
        }
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    [MenuItem("Love Producer/Add Architectural Trims and Frames")]
    public static void AddArchitecturalTrimsAndFrames()
    {
        GameObject floorPlan = GameObject.Find("LOVE PRODUCER FLOOR PLAN (24m x 20m)");
        Transform walls = floorPlan != null ? floorPlan.transform.Find("Walls") : null;
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform architecture = details != null ? details.transform.Find("Doors Windows & Steps") : null;
        if (walls == null || architecture == null)
        {
            Debug.LogError("Trim upgrade stopped: Walls or Doors Windows & Steps was not found.");
            return;
        }

        Transform old = architecture.Find("Architectural Trim Upgrade");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group("Architectural Trim Upgrade", architecture);
        Material trim = Mat("Architectural Dark Wood Trim", new Color(0.19f, 0.115f, 0.075f), 0.36f);
        Material brass = Mat("Architectural Brass Detail", new Color(0.58f, 0.36f, 0.13f), 0.72f);

        // Build baseboards and top caps from the actual saved wall segments, so
        // manual wall-position adjustments remain the source of truth.
        foreach (Renderer renderer in walls.GetComponentsInChildren<Renderer>(true))
        {
            Bounds bounds = renderer.bounds;
            if (bounds.size.y < 2.5f) continue;
            bool alongX = bounds.size.x >= bounds.size.z;
            Vector3 baseSize = alongX
                ? new Vector3(bounds.size.x + 0.04f, 0.16f, bounds.size.z + 0.10f)
                : new Vector3(bounds.size.x + 0.10f, 0.16f, bounds.size.z + 0.04f);
            Vector3 topSize = alongX
                ? new Vector3(bounds.size.x + 0.06f, 0.11f, bounds.size.z + 0.08f)
                : new Vector3(bounds.size.x + 0.08f, 0.11f, bounds.size.z + 0.06f);
            Box(renderer.name + " Baseboard", baseSize, new Vector3(bounds.center.x, 0.10f, bounds.center.z), trim, group);
            Box(renderer.name + " Top Cap", topSize, new Vector3(bounds.center.x, 2.94f, bounds.center.z), trim, group);
        }

        float[] bedroomDoorZ = { 1.5f, 4.5f, 7.5f };
        for (int i = 0; i < bedroomDoorZ.Length; i++)
        {
            DoorFrameAlongZ("West Bedroom " + (i + 1) + " Frame", -7f, bedroomDoorZ[i], 1.9f, trim, brass, group);
            DoorFrameAlongZ("East Bedroom " + (i + 4) + " Frame", 7f, bedroomDoorZ[i], 1.9f, trim, brass, group);
        }
        DoorFrameAlongZ("West Bathroom Frame", -4.8f, 0f, 1.3f, trim, brass, group);
        DoorFrameAlongZ("East Bathroom Frame", 4.8f, 0f, 1.3f, trim, brass, group);

        float[] windowZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < windowZ.Length; i++)
        {
            WindowFrameAlongZ("West Bedroom Window Trim " + (i + 1), -12.08f, windowZ[i], trim, group);
            WindowFrameAlongZ("East Bedroom Window Trim " + (i + 4), 12.08f, windowZ[i], trim, group);
        }
        WindowFrameAlongX("Kitchen Window Trim", -8.2f, -10.08f, trim, group);
        WindowFrameAlongX("Lounge Window Trim", 8.2f, -10.08f, trim, group);

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Add Architectural Trims and Frames");
        Selection.activeGameObject = group.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Architectural baseboards, top caps, door frames and window trims added without moving existing walls.");
    }

    private static void DoorFrameAlongZ(string name, float x, float z, float width, Material trim, Material brass, Transform parent)
    {
        Transform frame = Group(name, parent);
        float half = width * 0.5f;
        Box("Jamb A", new Vector3(0.29f, 2.18f, 0.10f), new Vector3(x, 1.09f, z - half), trim, frame);
        Box("Jamb B", new Vector3(0.29f, 2.18f, 0.10f), new Vector3(x, 1.09f, z + half), trim, frame);
        Box("Header", new Vector3(0.29f, 0.14f, width + 0.10f), new Vector3(x, 2.12f, z), trim, frame);
        Sphere("Handle Marker", new Vector3(x - 0.17f, 1.0f, z + half - 0.16f), Vector3.one * 0.055f, brass, frame);
    }

    private static void WindowFrameAlongZ(string name, float x, float z, Material trim, Transform parent)
    {
        Transform frame = Group(name, parent);
        Box("Window Frame Left", new Vector3(0.15f, 1.72f, 0.12f), new Vector3(x, 1.45f, z - 0.92f), trim, frame);
        Box("Window Frame Right", new Vector3(0.15f, 1.72f, 0.12f), new Vector3(x, 1.45f, z + 0.92f), trim, frame);
        Box("Window Frame Top", new Vector3(0.15f, 0.12f, 1.96f), new Vector3(x, 2.31f, z), trim, frame);
        Box("Window Sill", new Vector3(0.22f, 0.13f, 1.96f), new Vector3(x, 0.59f, z), trim, frame);
    }

    private static void WindowFrameAlongX(string name, float x, float z, Material trim, Transform parent)
    {
        Transform frame = Group(name, parent);
        Box("Window Frame Left", new Vector3(0.12f, 1.72f, 0.15f), new Vector3(x - 0.92f, 1.45f, z), trim, frame);
        Box("Window Frame Right", new Vector3(0.12f, 1.72f, 0.15f), new Vector3(x + 0.92f, 1.45f, z), trim, frame);
        Box("Window Frame Top", new Vector3(1.96f, 0.12f, 0.15f), new Vector3(x, 2.31f, z), trim, frame);
        Box("Window Sill", new Vector3(1.96f, 0.13f, 0.22f), new Vector3(x, 0.59f, z), trim, frame);
    }

    [MenuItem("Love Producer/Add Layered Main Terrace Plants")]
    public static void AddLayeredMainTerracePlants()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform plants = details != null ? details.transform.Find("Plants") : null;
        if (plants == null)
        {
            Debug.LogError("Terrace planting stopped: the Plants group was not found.");
            return;
        }

        Transform old = plants.Find("Main Terrace Botanical Upgrade");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group("Main Terrace Botanical Upgrade", plants);
        Material planter = Mat("Terrace Stone Planter", new Color(0.39f, 0.31f, 0.24f), 0.28f);
        Material soil = Mat("Terrace Planter Soil", new Color(0.12f, 0.075f, 0.035f), 0.08f);
        Material darkLeaf = Mat("Terrace Deep Leaves", new Color(0.09f, 0.29f, 0.12f), 0.12f);
        Material lightLeaf = Mat("Terrace Fresh Leaves", new Color(0.28f, 0.53f, 0.20f), 0.14f);
        Material flowerPink = Mat("Terrace Pink Flowers", new Color(0.88f, 0.28f, 0.52f), 0.20f);
        Material flowerCoral = Mat("Terrace Coral Flowers", new Color(0.95f, 0.42f, 0.24f), 0.20f);
        Material flowerCream = Mat("Terrace Cream Flowers", new Color(0.94f, 0.82f, 0.60f), 0.22f);

        AddTerracePlanter("West Terrace Long Planter", -4.55f, planter, soil, darkLeaf, lightLeaf,
            flowerPink, flowerCream, group);
        AddTerracePlanter("East Terrace Long Planter", 4.55f, planter, soil, darkLeaf, lightLeaf,
            flowerCoral, flowerCream, group);

        // Corner clusters visually connect the terrace planting to the garden
        // while leaving the central stairs and dining circulation unobstructed.
        AddTerracePlantCluster("West Lower Cluster", new Vector3(-4.1f, 0.18f, 5.0f), darkLeaf, lightLeaf, flowerPink, group);
        AddTerracePlantCluster("East Lower Cluster", new Vector3(4.1f, 0.18f, 5.0f), darkLeaf, lightLeaf, flowerCoral, group);
        AddTerracePlantCluster("West Ocean Cluster", new Vector3(-4.1f, 0.18f, 9.45f), darkLeaf, lightLeaf, flowerCream, group);
        AddTerracePlantCluster("East Ocean Cluster", new Vector3(4.1f, 0.18f, 9.45f), darkLeaf, lightLeaf, flowerCream, group);

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Add Layered Main Terrace Plants");
        Selection.activeGameObject = group.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Two continuous terrace planters, layered foliage, flowers and trailing vines were added.");
    }

    private static void AddTerracePlanter(string name, float x, Material planter, Material soil,
        Material darkLeaf, Material lightLeaf, Material flowerA, Material flowerB, Transform parent)
    {
        Transform box = Group(name, parent);
        Box("Planter Box", new Vector3(0.62f, 0.52f, 4.15f), new Vector3(x, 0.26f, 7.25f), planter, box);
        Box("Visible Soil", new Vector3(0.50f, 0.055f, 3.95f), new Vector3(x, 0.55f, 7.25f), soil, box);
        for (int i = 0; i < 9; i++)
        {
            float z = 5.55f + i * 0.43f;
            float height = i % 3 == 0 ? 1.05f : (i % 3 == 1 ? 0.72f : 0.48f);
            Material leaf = i % 2 == 0 ? darkLeaf : lightLeaf;
            Cylinder("Plant Stem " + (i + 1), new Vector3(x, 0.57f + height * 0.34f, z),
                new Vector3(0.045f, height * 0.34f, 0.045f), darkLeaf, box);
            Sphere("Foliage " + (i + 1), new Vector3(x, 0.68f + height * 0.62f, z),
                new Vector3(0.46f, height * 0.50f, 0.46f), leaf, box);
            if (i % 2 == 1)
            {
                Material flower = i % 4 == 1 ? flowerA : flowerB;
                Sphere("Flower " + (i + 1), new Vector3(x + (x < 0 ? 0.16f : -0.16f), 0.84f + height * 0.58f, z),
                    Vector3.one * 0.16f, flower, box);
            }
        }

        // Small beads of foliage form readable trailing vines from above.
        for (int vine = 0; vine < 4; vine++)
        {
            float z = 5.9f + vine * 0.95f;
            for (int link = 0; link < 4; link++)
            {
                float inward = x < 0 ? 0.18f : -0.18f;
                Sphere("Trailing Vine", new Vector3(x + inward, 0.55f - link * 0.10f, z + link * 0.10f),
                    new Vector3(0.17f, 0.15f, 0.17f), link % 2 == 0 ? darkLeaf : lightLeaf, box);
            }
        }
    }

    private static void AddTerracePlantCluster(string name, Vector3 center, Material darkLeaf,
        Material lightLeaf, Material flower, Transform parent)
    {
        Transform cluster = Group(name, parent);
        Sphere("Large Leaf Mass", center + Vector3.up * 0.50f, new Vector3(0.70f, 0.78f, 0.70f), darkLeaf, cluster);
        Sphere("Fresh Leaf Mass", center + new Vector3(0.34f, 0.42f, 0.12f), new Vector3(0.52f, 0.58f, 0.52f), lightLeaf, cluster);
        Sphere("Flower Accent A", center + new Vector3(-0.20f, 0.76f, 0.14f), Vector3.one * 0.18f, flower, cluster);
        Sphere("Flower Accent B", center + new Vector3(0.22f, 0.65f, -0.18f), Vector3.one * 0.15f, flower, cluster);
    }

    [MenuItem("Love Producer/Add Bedroom Soft Furnishings")]
    public static void AddBedroomSoftFurnishings()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        if (furniture == null)
        {
            Debug.LogError("Bedroom furnishing stopped: Detailed Furniture was not found.");
            return;
        }

        Transform old = furniture.Find("Bedroom Soft Furnishings Upgrade");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group("Bedroom Soft Furnishings Upgrade", furniture);

        Color westBlue = new Color(0.32f, 0.45f, 0.66f);
        Color westGold = new Color(0.82f, 0.55f, 0.22f);
        Color eastTeal = new Color(0.24f, 0.58f, 0.58f);
        Color eastPink = new Color(0.78f, 0.43f, 0.56f);
        Material cream = Mat("Bedroom Soft Cream", new Color(0.91f, 0.84f, 0.72f), 0.12f);
        Material wood = Mat("Bedroom Bench Wood", new Color(0.34f, 0.20f, 0.11f), 0.25f);
        Material frame = Mat("Bedroom Art Frame", new Color(0.18f, 0.11f, 0.075f), 0.38f);

        Texture2D westWoven = CreateWovenRugTexture("West_Bedroom_Woven_Texture", westBlue);
        Texture2D eastWoven = CreateWovenRugTexture("East_Bedroom_Woven_Texture", new Color(0.72f, 0.50f, 0.54f));
        Material westRug = DetailedMat("West Bedroom Woven Rug", westWoven, 0.10f, new Vector2(3.4f, 3.4f));
        Material eastRug = DetailedMat("East Bedroom Woven Rug", eastWoven, 0.10f, new Vector2(3.4f, 3.4f));

        float[] bedroomZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            Color westAccentColor = i == 2 ? westGold : westBlue;
            Color eastAccentColor = i == 0 ? eastTeal : eastPink;
            Material westAccent = Mat("West Bedroom " + (i + 1) + " Accent", westAccentColor, 0.14f);
            Material eastAccent = Mat("East Bedroom " + (i + 4) + " Accent", eastAccentColor, 0.14f);
            AddBedroomSoftSet("West Bedroom " + (i + 1) + " Soft Set", new Vector3(-9.5f, 0, bedroomZ[i]),
                false, westRug, westAccent, cream, wood, frame, group);
            AddBedroomSoftSet("East Bedroom " + (i + 4) + " Soft Set", new Vector3(9.5f, 0, bedroomZ[i]),
                true, eastRug, eastAccent, cream, wood, frame, group);
        }

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Add Bedroom Soft Furnishings");
        Selection.activeGameObject = group.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Six bedrooms received rugs, curtains, pillows, end benches and framed wall art.");
    }

    private static void AddBedroomSoftSet(string name, Vector3 center, bool eastSide, Material rug,
        Material accent, Material cream, Material wood, Material frame, Transform parent)
    {
        Transform set = Group(name, parent);
        float wallX = eastSide ? 11.80f : -11.80f;
        float curtainInset = eastSide ? -0.04f : 0.04f;

        // A low woven rug peeks out at the foot and sides of the bed.
        Box("Bedside Woven Rug", new Vector3(2.05f, 0.025f, 1.55f), center + new Vector3(0, 0.025f, -0.72f), rug, set);

        // Two smaller decorative pillows layer over the existing main pillow.
        Box("Decorative Pillow Left", new Vector3(0.52f, 0.16f, 0.36f), center + new Vector3(-0.30f, 0.73f, 0.58f), accent, set);
        Box("Decorative Pillow Right", new Vector3(0.52f, 0.16f, 0.36f), center + new Vector3(0.30f, 0.73f, 0.58f), cream, set);

        // A compact upholstered bench stays clear of the corridor doorway.
        Box("Bed End Bench Seat", new Vector3(1.30f, 0.28f, 0.42f), center + new Vector3(0, 0.40f, -1.18f), accent, set);
        Box("Bench Leg Left", new Vector3(0.12f, 0.34f, 0.12f), center + new Vector3(-0.48f, 0.17f, -1.18f), wood, set);
        Box("Bench Leg Right", new Vector3(0.12f, 0.34f, 0.12f), center + new Vector3(0.48f, 0.17f, -1.18f), wood, set);

        // Paired curtains frame the exterior window without covering the glass.
        Box("Curtain Panel A", new Vector3(0.10f, 1.78f, 0.42f), new Vector3(wallX + curtainInset, 1.45f, center.z - 0.78f), accent, set);
        Box("Curtain Panel B", new Vector3(0.10f, 1.78f, 0.42f), new Vector3(wallX + curtainInset, 1.45f, center.z + 0.78f), accent, set);
        Cylinder("Curtain Rod", new Vector3(wallX + curtainInset * 1.4f, 2.38f, center.z),
            new Vector3(0.045f, 1.08f, 0.045f), frame, set).transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Art sits away from the centered doorway on the corridor-facing wall.
        float artX = eastSide ? 7.13f : -7.13f;
        float facingOffset = eastSide ? -0.035f : 0.035f;
        Box("Wall Art Frame", new Vector3(0.10f, 0.82f, 0.92f), new Vector3(artX, 1.55f, center.z - 0.82f), frame, set);
        Box("Wall Art Print", new Vector3(0.075f, 0.62f, 0.72f), new Vector3(artX + facingOffset, 1.55f, center.z - 0.82f), accent, set);
    }

    [MenuItem("Love Producer/Replace Bedroom Windows with Real Glass")]
    public static void ReplaceBedroomWindowsWithRealGlass()
    {
        // A transparent pane only works if the structural wall has a real hole.
        LoveProducerFloorPlanBuilder.FixBedroomExteriorWindowOpenings();

        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform architecture = details != null ? details.transform.Find("Doors Windows & Steps") : null;
        if (architecture == null)
        {
            Debug.LogError("Bedroom window replacement stopped: Doors Windows & Steps was not found.");
            return;
        }

        Transform oldUpgrade = architecture.Find("Bedroom Real Glass Windows");
        if (oldUpgrade != null) Undo.DestroyObjectImmediate(oldUpgrade.gameObject);

        // Remove the original solid-box frames and their hidden glass panes.
        for (int i = architecture.childCount - 1; i >= 0; i--)
        {
            Transform child = architecture.GetChild(i);
            bool oldBedroomWindow = child.name.StartsWith("West Bedroom Window")
                || child.name.StartsWith("East Bedroom Window");
            if (oldBedroomWindow && (child.name.EndsWith(" Frame") || child.name.EndsWith(" Glass")))
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        Transform group = Group("Bedroom Real Glass Windows", architecture);
        Material glass = TransparentGlassMat("Bedroom Clear Blue Glass", new Color(0.78f, 0.90f, 0.94f, 0.12f));
        Material frame = Mat("Bedroom Window Dark Frame", new Color(0.16f, 0.105f, 0.075f), 0.46f);
        float[] bedroomZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            RealGlassWindowAlongZ("West Bedroom " + (i + 1) + " Glass Window", -12.015f, bedroomZ[i], glass, frame, group);
            RealGlassWindowAlongZ("East Bedroom " + (i + 4) + " Glass Window", 12.015f, bedroomZ[i], glass, frame, group);
        }

        Undo.RegisterCreatedObjectUndo(group.gameObject, "Replace Bedroom Windows with Real Glass");
        Selection.activeGameObject = group.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Six solid window blocks were replaced with framed transparent glass windows behind the curtains.");
    }

    private static void RealGlassWindowAlongZ(string name, float x, float z, Material glass, Material frame, Transform parent)
    {
        Transform window = Group(name, parent);
        GameObject pane = Box("Glass Pane", new Vector3(0.035f, 1.42f, 1.62f), new Vector3(x, 1.45f, z), glass, window);
        Renderer paneRenderer = pane.GetComponent<Renderer>();
        paneRenderer.shadowCastingMode = ShadowCastingMode.Off;
        paneRenderer.receiveShadows = false;
        Box("Frame Left", new Vector3(0.13f, 1.62f, 0.11f), new Vector3(x, 1.45f, z - 0.86f), frame, window);
        Box("Frame Right", new Vector3(0.13f, 1.62f, 0.11f), new Vector3(x, 1.45f, z + 0.86f), frame, window);
        Box("Frame Top", new Vector3(0.13f, 0.11f, 1.82f), new Vector3(x, 2.26f, z), frame, window);
        Box("Frame Sill", new Vector3(0.17f, 0.13f, 1.82f), new Vector3(x, 0.64f, z), frame, window);
        Box("Center Mullion", new Vector3(0.13f, 1.48f, 0.075f), new Vector3(x, 1.45f, z), frame, window);
    }

    private static Material TransparentGlassMat(string name, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.92f);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(material);
        return material;
    }

    [MenuItem("Love Producer/Group Each Bedroom Bed Furniture")]
    public static void GroupEachBedroomBedFurniture()
    {
        GameObject details = GameObject.Find("DETAILS & ENVIRONMENT");
        Transform furniture = details != null ? details.transform.Find("Detailed Furniture") : null;
        if (furniture == null)
        {
            Debug.LogError("Bedroom grouping stopped: Detailed Furniture was not found.");
            return;
        }

        GameObject existingObject = GameObject.Find("BEDROOM BED GROUPS - MOVE THESE");
        Transform existing = existingObject != null ? existingObject.transform : null;
        if (existing != null)
        {
            if (existing.parent != null)
            {
                Undo.SetTransformParent(existing, null, "Move Bedroom Groups to Scene Root");
                existing.SetSiblingIndex(0);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            Debug.LogWarning("Bedroom bed groups already exist. They were moved to the scene root and selected without changing bed positions.");
            return;
        }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Group Bedroom Bed Furniture");
        Transform groupRoot = Group("BEDROOM BED GROUPS - MOVE THESE", null);
        groupRoot.SetSiblingIndex(0);
        Undo.RegisterCreatedObjectUndo(groupRoot.gameObject, "Create Bedroom Bed Groups");
        Transform softRoot = furniture.Find("Bedroom Soft Furnishings Upgrade");
        float[] bedroomZ = { 7.5f, 4.5f, 1.5f };

        for (int i = 0; i < bedroomZ.Length; i++)
        {
            GroupBedroomBed("West Bedroom " + (i + 1) + " BED GROUP", new Vector3(-9.5f, 0, bedroomZ[i]),
                furniture, softRoot, groupRoot);
            GroupBedroomBed("East Bedroom " + (i + 4) + " BED GROUP", new Vector3(9.5f, 0, bedroomZ[i]),
                furniture, softRoot, groupRoot);
        }

        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = groupRoot.gameObject;
        EditorGUIUtility.PingObject(groupRoot.gameObject);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Six movable bedroom bed groups created. Curtains, wall art, windows and wardrobes remain fixed.");
    }

    private static void GroupBedroomBed(string groupName, Vector3 center, Transform furniture,
        Transform softRoot, Transform groupRoot)
    {
        Transform bedGroup = Group(groupName, groupRoot);
        bedGroup.position = center;
        Undo.RegisterCreatedObjectUndo(bedGroup.gameObject, "Create " + groupName);

        // Group the original bed pieces by both name and proximity, because all
        // six bedrooms use the same child-object names.
        for (int i = furniture.childCount - 1; i >= 0; i--)
        {
            Transform child = furniture.GetChild(i);
            if (child == groupRoot || child == softRoot) continue;
            string n = child.name;
            bool bedPiece = n == "Bed Frame" || n == "Mattress" || n == "Pillow"
                || n == "Bedside Table" || n == "Bedside Lamp";
            Vector2 childXZ = new Vector2(child.position.x, child.position.z);
            Vector2 centerXZ = new Vector2(center.x, center.z);
            if (bedPiece && Vector2.Distance(childXZ, centerXZ) < 2.0f)
                Undo.SetTransformParent(child, bedGroup, "Move Bed Piece into Bedroom Group");
        }

        if (softRoot == null) return;
        string softName = groupName.Replace(" BED GROUP", " Soft Set");
        Transform softSet = softRoot.Find(softName);
        if (softSet == null) return;
        for (int i = softSet.childCount - 1; i >= 0; i--)
        {
            Transform child = softSet.GetChild(i);
            string n = child.name;
            bool movableSoftPiece = n == "Bedside Woven Rug"
                || n.StartsWith("Decorative Pillow")
                || n.StartsWith("Bed End Bench")
                || n.StartsWith("Bench Leg");
            if (movableSoftPiece)
                Undo.SetTransformParent(child, bedGroup, "Move Soft Furnishing into Bedroom Group");
        }
    }

    private static void Bedroom(Vector3 p, Material fabric, Material wood, Material white, Material metal, Transform parent, bool mirror)
    {
        float side = mirror ? -1 : 1;
        Box("Bed Frame", new Vector3(1.45f, 0.32f, 1.95f), p + new Vector3(0, 0.22f, 0.15f), wood, parent);
        Box("Mattress", new Vector3(1.34f, 0.22f, 1.78f), p + new Vector3(0, 0.49f, 0.12f), fabric, parent);
        Box("Pillow", new Vector3(1.05f, 0.14f, 0.36f), p + new Vector3(0, 0.66f, 0.76f), white, parent);
        Box("Bedside Table", new Vector3(0.48f, 0.48f, 0.48f), p + new Vector3(side * 1.05f, 0.24f, 0.55f), wood, parent);
        Cylinder("Bedside Lamp", p + new Vector3(side * 1.05f, 0.68f, 0.55f), new Vector3(0.17f, 0.38f, 0.17f), metal, parent);
        Box("Wardrobe", new Vector3(0.55f, 1.8f, 1.05f), p + new Vector3(-side * 1.38f, 0.9f, -0.65f), wood, parent);
    }

    private static void Bathroom(Vector3 p, Material ceramic, Material glass, Material metal, Transform parent, bool mirror)
    {
        float side = mirror ? -1 : 1;
        Box("Shower Tray", new Vector3(1.2f, 0.12f, 1.1f), p + new Vector3(side * 0.85f, 0.06f, 0.45f), ceramic, parent);
        Box("Shower Glass", new Vector3(0.06f, 1.65f, 1.1f), p + new Vector3(side * 0.22f, 0.825f, 0.45f), glass, parent);
        Box("Vanity", new Vector3(0.95f, 0.72f, 0.52f), p + new Vector3(-side * 0.9f, 0.36f, 0.55f), ceramic, parent);
        Box("Mirror", new Vector3(0.85f, 0.75f, 0.05f), p + new Vector3(-side * 0.9f, 1.35f, 0.83f), metal, parent);
        Cylinder("Toilet", p + new Vector3(0, 0.25f, -0.65f), new Vector3(0.38f, 0.50f, 0.50f), ceramic, parent);
    }

    private static void DiningSet(Vector3 p, Material wood, Material fabric, Transform parent)
    {
        Box("Dining Table", new Vector3(2.0f, 0.13f, 1.05f), p + new Vector3(0, 0.78f, 0), wood, parent);
        for (int i = 0; i < 4; i++)
        {
            Vector3 offset = i < 2
                ? new Vector3(i == 0 ? -0.65f : 0.65f, 0.32f, 0.85f)
                : new Vector3(i == 2 ? -0.65f : 0.65f, 0.32f, -0.85f);
            Box("Dining Chair " + i, new Vector3(0.52f, 0.64f, 0.52f), p + offset, fabric, parent);
        }
        Box("Table Pedestal", new Vector3(0.24f, 0.72f, 0.24f), p + new Vector3(0, 0.36f, 0), wood, parent);
    }

    private static Transform SixSeatDiningSet(string name, Vector3 p, Material wood, Material fabric, Transform parent)
    {
        Transform set = Group(name, parent);
        Box("Dining Table", new Vector3(2.55f, 0.13f, 1.05f), p + new Vector3(0, 0.78f, 0), wood, set);
        Box("Table Pedestal Left", new Vector3(0.22f, 0.72f, 0.22f), p + new Vector3(-0.72f, 0.36f, 0), wood, set);
        Box("Table Pedestal Right", new Vector3(0.22f, 0.72f, 0.22f), p + new Vector3(0.72f, 0.36f, 0), wood, set);

        float[] chairX = { -0.82f, 0f, 0.82f };
        int chairNumber = 1;
        foreach (float x in chairX)
        {
            Box("Dining Chair " + chairNumber++, new Vector3(0.52f, 0.64f, 0.52f),
                p + new Vector3(x, 0.32f, 0.88f), fabric, set);
            Box("Dining Chair " + chairNumber++, new Vector3(0.52f, 0.64f, 0.52f),
                p + new Vector3(x, 0.32f, -0.88f), fabric, set);
        }
        return set;
    }

    private static void AddSideTerraceDeck(string name, Vector3 center, Material wood, Material border, Transform parent)
    {
        Transform deck = Group(name, parent);
        Box("Deck Base", new Vector3(3.25f, 0.16f, 4.8f), center, wood, deck);
        Box("Outer Border", new Vector3(0.13f, 0.06f, 4.8f), center + new Vector3(center.x < 0 ? -1.56f : 1.56f, 0.11f, 0), border, deck);
        for (int i = -5; i <= 5; i++)
        {
            Box("Deck Board Seam", new Vector3(3.05f, 0.012f, 0.025f),
                center + new Vector3(0, 0.087f, i * 0.40f), border, deck);
        }
    }

    private static void Sofa(string name, Vector3 p, Quaternion rotation, Material material, Transform parent)
    {
        GameObject sofa = new GameObject(name);
        sofa.transform.SetParent(parent);
        sofa.transform.position = p;
        sofa.transform.rotation = rotation;
        Box("Seat", new Vector3(2.15f, 0.42f, 0.82f), new Vector3(0, 0.34f, 0), material, sofa.transform, true);
        Box("Back", new Vector3(2.15f, 0.8f, 0.20f), new Vector3(0, 0.72f, 0.36f), material, sofa.transform, true);
        Box("Arm L", new Vector3(0.20f, 0.68f, 0.82f), new Vector3(-1.08f, 0.50f, 0), material, sofa.transform, true);
        Box("Arm R", new Vector3(0.20f, 0.68f, 0.82f), new Vector3(1.08f, 0.50f, 0), material, sofa.transform, true);
    }

    private static void ConversationRingSofa(Vector3 center, Material upholstery, Material coral, Material teal, Transform parent)
    {
        Transform ring = Group("Curved Living Conversation Sofa", parent);
        const int segmentCount = 13;
        const float radius = 2.65f;
        const float startAngle = -140f;
        const float endAngle = 140f;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 radial = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));

            Transform module = Group("Curved Sofa Module " + (i + 1), ring);
            module.position = center + radial * radius;
            module.rotation = Quaternion.Euler(0, angle, 0);
            Box("Seat", new Vector3(1.08f, 0.42f, 0.82f), new Vector3(0, 0.34f, 0), upholstery, module, true);
            Box("Curved Back", new Vector3(1.10f, 0.78f, 0.20f), new Vector3(0, 0.72f, 0.36f), upholstery, module, true);

            if (i == 0 || i == segmentCount - 1)
                Box("End Arm", new Vector3(0.18f, 0.66f, 0.82f), new Vector3(i == 0 ? -0.55f : 0.55f, 0.50f, 0), upholstery, module, true);

            if (i % 2 == 1)
            {
                Material accent = i % 4 == 1 ? coral : teal;
                Box("Accent Cushion", new Vector3(0.46f, 0.18f, 0.42f), new Vector3(0, 0.72f, 0.08f), accent, module, true);
            }
        }
    }

    private static void PoolTable(Vector3 p, Material wood, Material felt, Transform parent)
    {
        Box("Pool Table", new Vector3(2.35f, 0.20f, 1.30f), p + new Vector3(0, 0.82f, 0), felt, parent);
        Box("Pool Rail", new Vector3(2.55f, 0.18f, 1.50f), p + new Vector3(0, 0.75f, 0), wood, parent);
        float[] xs = { -0.9f, 0.9f };
        float[] zs = { -0.45f, 0.45f };
        foreach (float x in xs) foreach (float z in zs)
            Box("Pool Leg", new Vector3(0.16f, 0.72f, 0.16f), p + new Vector3(x, 0.36f, z), wood, parent);
    }

    private static void Palm(string name, Vector3 p, Material trunk, Material leaves, Transform parent)
    {
        Transform palm = Group(name, parent);
        Cylinder("Trunk", p + Vector3.up * 1.35f, new Vector3(0.28f, 2.7f, 0.28f), trunk, palm);
        for (int i = 0; i < 7; i++)
        {
            GameObject frond = Box("Palm Frond", new Vector3(0.35f, 0.12f, 2.5f),
                p + new Vector3(0, 2.72f, 0), leaves, palm);
            frond.transform.rotation = Quaternion.Euler(12, i * 51.4f, 18);
        }
    }

    private static void Bush(string name, Vector3 p, Material material, Transform parent)
    {
        Transform bush = Group(name, parent);
        Sphere("Bush A", p + new Vector3(-0.35f, 0.45f, 0), new Vector3(1.0f, 0.9f, 1.0f), material, bush);
        Sphere("Bush B", p + new Vector3(0.35f, 0.42f, 0.15f), new Vector3(1.1f, 0.84f, 1.1f), material, bush);
        Sphere("Bush C", p + new Vector3(0, 0.62f, -0.22f), new Vector3(0.9f, 1.1f, 0.9f), material, bush);
    }

    private static void Window(string name, Vector3 p, bool alongZ, Material glass, Material trim, Transform parent)
    {
        Vector3 glassSize = alongZ ? new Vector3(0.07f, 1.15f, 1.55f) : new Vector3(1.55f, 1.15f, 0.07f);
        Vector3 trimSize = alongZ ? new Vector3(0.11f, 1.35f, 1.75f) : new Vector3(1.75f, 1.35f, 0.11f);
        Box(name + " Frame", trimSize, p, trim, parent);
        Box(name + " Glass", glassSize, p, glass, parent);
    }

    private static void Door(string name, Vector3 p, Vector3 size, Material material, Transform parent)
    {
        Box(name, size, p, material, parent);
    }

    private static void PointLight(Vector3 p, Transform parent, string lightName = "Warm Room Light")
    {
        GameObject go = new GameObject(lightName);
        go.transform.SetParent(parent);
        go.transform.position = p;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 7;
        light.intensity = 2.0f;
        light.color = new Color(1.0f, 0.78f, 0.56f);
        light.shadows = LightShadows.None;
    }

    private static void DecoratedRug(string name, Vector3 p, Vector2 size, Material baseMaterial, Material border, Material accent, Transform parent)
    {
        Transform rugRoot = Group(name, parent);
        Box("Rug Base", new Vector3(size.x, 0.025f, size.y), p, baseMaterial, rugRoot);
        float y = p.y + 0.016f;
        Box("Border N", new Vector3(size.x * 0.94f, 0.012f, 0.075f), new Vector3(p.x, y, p.z + size.y * 0.42f), border, rugRoot);
        Box("Border S", new Vector3(size.x * 0.94f, 0.012f, 0.075f), new Vector3(p.x, y, p.z - size.y * 0.42f), border, rugRoot);
        Box("Border E", new Vector3(0.075f, 0.012f, size.y * 0.84f), new Vector3(p.x + size.x * 0.42f, y, p.z), border, rugRoot);
        Box("Border W", new Vector3(0.075f, 0.012f, size.y * 0.84f), new Vector3(p.x - size.x * 0.42f, y, p.z), border, rugRoot);
        for (int i = -2; i <= 2; i++)
            Box("Woven Stripe", new Vector3(size.x * 0.56f, 0.013f, 0.045f), new Vector3(p.x, y + 0.002f, p.z + i * size.y * 0.12f), accent, rugRoot);
    }

    private static Material[] CreateColorfulBulbMaterials()
    {
        return new[]
        {
            EmissiveMat("Festoon Golden Glow", new Color(1.00f, 0.55f, 0.16f), 3.2f),
            EmissiveMat("Festoon Coral Glow", new Color(1.00f, 0.24f, 0.28f), 3.2f),
            EmissiveMat("Festoon Pink Glow", new Color(1.00f, 0.18f, 0.62f), 3.2f),
            EmissiveMat("Festoon Teal Glow", new Color(0.10f, 0.78f, 0.72f), 3.2f),
            EmissiveMat("Festoon Lavender Glow", new Color(0.52f, 0.32f, 1.00f), 3.2f)
        };
    }

    private static void AddLivingLightUpgrade(Transform lighting, Material cable, Material[] colorfulBulbs)
    {
        Transform group = Group("Living Light Upgrade", lighting);

        // Four warm pools of light keep the circular seating readable at night.
        Vector3[] positions =
        {
            new Vector3(-2.35f, 2.55f, -2.65f),
            new Vector3( 2.35f, 2.55f, -2.65f),
            new Vector3(-2.35f, 2.55f,  0.65f),
            new Vector3( 2.35f, 2.55f,  0.65f)
        };
        foreach (Vector3 position in positions)
        {
            PointLight(position, group, "Living Accent Light");
            Light light = group.GetChild(group.childCount - 1).GetComponent<Light>();
            light.intensity = 1.35f;
            light.range = 4.8f;
        }

        // A short colorful strand frames the conversation area without blocking the camera.
        StringLights("Living Color Strand", new Vector3(-3.4f, 2.78f, 1.65f),
            new Vector3(3.4f, 2.78f, 1.65f), 19, cable, colorfulBulbs, group, true);
    }

    private static void StringLights(string name, Vector3 start, Vector3 end, int bulbCount, Material cable, Material[] glows, Transform parent, bool addSharedLight)
    {
        Transform strand = Group(name, parent);
        Vector3 direction = end - start;
        GameObject wire = Box("Cable", new Vector3(0.025f, 0.025f, direction.magnitude), (start + end) * 0.5f, cable, strand);
        wire.transform.rotation = Quaternion.LookRotation(direction.normalized);

        for (int i = 0; i < bulbCount; i++)
        {
            float t = bulbCount == 1 ? 0.5f : i / (float)(bulbCount - 1);
            Vector3 p = Vector3.Lerp(start, end, t);
            p.y -= 0.10f + Mathf.Sin(t * Mathf.PI) * 0.18f;
            Box("Drop", new Vector3(0.018f, 0.18f, 0.018f), p + Vector3.up * 0.09f, cable, strand);
            Material glow = glows != null && glows.Length > 0 ? glows[i % glows.Length] : null;
            Sphere("Color Bulb " + (i + 1), p, Vector3.one * 0.12f, glow, strand);

            // Three lightweight point lights make the emissive bulbs illuminate nearby surfaces.
            bool addLamp = addSharedLight && (i == bulbCount / 4 || i == bulbCount / 2 || i == (bulbCount * 3) / 4);
            if (addLamp)
            {
                GameObject lamp = new GameObject("Festoon Light");
                lamp.transform.SetParent(strand);
                lamp.transform.position = p;
                Light light = lamp.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = glow != null ? glow.color : new Color(1.0f, 0.58f, 0.30f);
                light.intensity = 0.65f;
                light.range = 2.8f;
                light.shadows = LightShadows.None;
            }
        }
    }

    private static void PrivateTerraceSet(Vector3 center, float yaw, Material wood, Material fabric, Material leaves, Transform parent)
    {
        Transform group = Group("Terrace Private Conversation", parent);
        group.position = center;
        group.rotation = Quaternion.Euler(0, yaw, 0);
        Box("Private Chair A", new Vector3(0.72f, 0.65f, 0.72f), new Vector3(-0.62f, 0.33f, 0), fabric, group, true);
        Box("Private Chair B", new Vector3(0.72f, 0.65f, 0.72f), new Vector3(0.62f, 0.33f, 0), fabric, group, true);
        Cylinder("Private Table", center + new Vector3(0, 0.42f, 0.55f), new Vector3(0.30f, 0.42f, 0.30f), wood, group);
        Box("Privacy Planter", new Vector3(1.8f, 0.42f, 0.35f), new Vector3(0, 0.21f, -0.65f), wood, group, true);
        for (int i = -2; i <= 2; i++)
            Sphere("Planter Leaves", center + group.rotation * new Vector3(i * 0.32f, 0.62f, -0.65f), new Vector3(0.42f, 0.55f, 0.42f), leaves, group);
    }

    private static void NeonLandmark(Vector3 p, Material glow, Material backing, Transform parent)
    {
        Transform sign = Group("Right Lounge Neon Landmark", parent);
        Box("Neon Backing", new Vector3(2.2f, 0.95f, 0.06f), p, backing, sign);
        Box("Neon Heart Left", new Vector3(0.08f, 0.48f, 0.08f), p + new Vector3(-0.24f, 0.05f, -0.05f), glow, sign).transform.rotation = Quaternion.Euler(0, 0, -38f);
        Box("Neon Heart Right", new Vector3(0.08f, 0.48f, 0.08f), p + new Vector3(0.24f, 0.05f, -0.05f), glow, sign).transform.rotation = Quaternion.Euler(0, 0, 38f);
        Box("Neon Smile", new Vector3(0.85f, 0.07f, 0.08f), p + new Vector3(0, -0.22f, -0.05f), glow, sign);
    }

    private static void ConfigureSocialAnchors()
    {
        GameObject root = GameObject.Find("SOCIAL GAMEPLAY ANCHORS");
        if (root == null) root = new GameObject("SOCIAL GAMEPLAY ANCHORS");

        SyncAnchor(root.transform, "Central Social Circle", SocialAnchorType.CentralSocialCircle, new Vector3(0, 0, -1.0f), 6, 1.75f);
        SyncAnchor(root.transform, "Dining Shared", SocialAnchorType.DiningShared, new Vector3(-8.0f, 0, -7.25f), 6, 1.35f);
        SyncAnchor(root.transform, "Kitchen Shared", SocialAnchorType.KitchenShared, new Vector3(-8.2f, 0, -4.2f), 4, 1.2f);
        SyncAnchor(root.transform, "Lounge Conversation", SocialAnchorType.LoungeConversation, new Vector3(8.0f, 0, -5.8f), 5, 1.5f);
        SyncAnchor(root.transform, "Main Terrace Shared", SocialAnchorType.DiningShared, new Vector3(0, 0, 7.25f), 4, 1.3f);
        SyncAnchor(root.transform, "West Side Terrace Pair", SocialAnchorType.TerracePrivatePair, new Vector3(-12.9f, 0, -3.4f), 2, 0.75f);
        SyncAnchor(root.transform, "East Side Terrace Pair", SocialAnchorType.TerracePrivatePair, new Vector3(12.9f, 0, -3.4f), 2, 0.75f);
        SyncAnchor(root.transform, "Withdrawal West", SocialAnchorType.WithdrawalSolo, new Vector3(-4.4f, 0, 3.3f), 1, 0.0f);
        SyncAnchor(root.transform, "Withdrawal East", SocialAnchorType.WithdrawalSolo, new Vector3(4.4f, 0, 3.3f), 1, 0.0f);
        EditorUtility.SetDirty(root);
    }

    private static void SyncAnchor(Transform root, string name, SocialAnchorType type, Vector3 center, int capacity, float radius)
    {
        Transform anchorTransform = root.Find(name);
        if (anchorTransform == null) anchorTransform = Group(name, root);
        anchorTransform.position = center;

        Transform facing = anchorTransform.Find("Facing Point");
        if (facing == null) facing = Group("Facing Point", anchorTransform);
        facing.position = center + Vector3.up * 1.2f;

        Transform[] slots = new Transform[capacity];
        for (int i = 0; i < capacity; i++)
        {
            string slotName = "Contestant Slot " + (i + 1);
            Transform slot = anchorTransform.Find(slotName);
            if (slot == null) slot = Group(slotName, anchorTransform);
            float angle = capacity == 1 ? 0f : i * Mathf.PI * 2f / capacity;
            slot.position = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            slot.rotation = Quaternion.LookRotation((center - slot.position).sqrMagnitude > 0.001f ? center - slot.position : Vector3.forward);
            slots[i] = slot;
        }

        SocialInteractionAnchor anchor = anchorTransform.GetComponent<SocialInteractionAnchor>();
        if (anchor == null) anchor = anchorTransform.gameObject.AddComponent<SocialInteractionAnchor>();
        anchor.Configure(type, capacity, facing, slots);
        EditorUtility.SetDirty(anchor);
    }

    private static void Lantern(Vector3 p, Material frame, Material glow, Transform parent)
    {
        Transform lantern = Group("Entrance Lantern", parent);
        Box("Lantern Frame", new Vector3(0.32f, 0.48f, 0.32f), p, frame, lantern);
        Sphere("Lantern Glow", p, Vector3.one * 0.19f, glow, lantern);
        GameObject lamp = new GameObject("Entrance Light");
        lamp.transform.SetParent(lantern);
        lamp.transform.position = p;
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.56f, 0.28f);
        light.intensity = 1.4f;
        light.range = 4.5f;
        light.shadows = LightShadows.Soft;
    }

    private static void ImproveSceneLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.64f, 0.72f, 0.82f);
        RenderSettings.ambientEquatorColor = new Color(0.52f, 0.48f, 0.42f);
        RenderSettings.ambientGroundColor = new Color(0.24f, 0.22f, 0.18f);
        RenderSettings.reflectionIntensity = 0.55f;

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.orthographic = true;
            camera.orthographicSize = 16.5f;
            camera.transform.position = new Vector3(0, 25, -20);
            camera.transform.LookAt(new Vector3(0, 0, 0.5f));
            camera.backgroundColor = new Color(0.58f, 0.76f, 0.83f);
        }
    }

    private static void RestoreSocialVillaCamera()
    {
        FirstPersonHouseWalker[] walkers = Object.FindObjectsByType<FirstPersonHouseWalker>(FindObjectsInactive.Include);
        foreach (FirstPersonHouseWalker walker in walkers) walker.gameObject.SetActive(false);

        GameObject cameraObject = null;
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
        foreach (Camera candidate in cameras)
        {
            if (candidate.name != "Top Down Camera") continue;
            cameraObject = candidate.gameObject;
            break;
        }
        if (cameraObject == null) return;
        cameraObject.SetActive(true);
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        if (camera != null)
        {
            camera.orthographic = true;
            camera.orthographicSize = 16.5f;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void ConfigureDayNight(GameObject detailsRoot, Transform lighting)
    {
        LoveProducerDayNightController controller = Object.FindAnyObjectByType<LoveProducerDayNightController>();
        if (controller == null) controller = detailsRoot.AddComponent<LoveProducerDayNightController>();

        Light sun = null;
        GameObject sunObject = GameObject.Find("Sun");
        if (sunObject != null) sun = sunObject.GetComponent<Light>();

        GameObject moonObject = GameObject.Find("Moon");
        if (moonObject == null)
        {
            moonObject = new GameObject("Moon");
            moonObject.transform.SetParent(detailsRoot.transform);
            moonObject.transform.rotation = Quaternion.Euler(32, 145, 0);
        }

        Light moon = moonObject.GetComponent<Light>();
        if (moon == null) moon = moonObject.AddComponent<Light>();
        moon.type = LightType.Directional;
        moon.color = new Color(0.30f, 0.42f, 0.70f);
        moon.shadows = LightShadows.Soft;

        controller.sun = sun;
        controller.moon = moon;
        controller.practicalLights = lighting.GetComponentsInChildren<Light>(true);
        controller.dayAmbient = new Color(0.58f, 0.55f, 0.50f);
        controller.nightAmbient = new Color(0.055f, 0.075f, 0.13f);
        controller.SetNight(false);
        EditorUtility.SetDirty(controller);
    }

    private static GameObject Box(string name, Vector3 size, Vector3 position, Material material, Transform parent, bool local = false)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        if (local) go.transform.localPosition = position;
        else go.transform.position = position;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private static GameObject Cylinder(string name, Vector3 p, Vector3 size, Material material, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = p;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private static GameObject Sphere(string name, Vector3 p, Vector3 size, Material material, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = p;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private static Transform Group(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.transform;
    }

    private static Material Mat(string name, Color color, float smoothness = 0.18f)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material EmissiveMat(string name, Color color, float intensity)
    {
        Material material = Mat(name, color, 0.5f);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * intensity);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folder = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
