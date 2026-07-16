using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class LoveProducerFloorPlanBuilder
{
    private const string ScenePath = "Assets/Scenes/LoveProducerFloorPlan.unity";
    private const string MaterialFolder = "Assets/Generated/FloorPlanMaterials";
    private const float WallHeight = 3f;
    private const float WallThickness = 0.2f;

    [MenuItem("Love Producer/Build Reference Floor Plan")]
    public static void Build()
    {
        EnsureFolder("Assets/Generated");
        EnsureFolder(MaterialFolder);
        EnsureFolder("Assets/Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "LoveProducerFloorPlan";

        Material wall = Material("Wall", new Color(0.88f, 0.84f, 0.77f));
        Material trim = Material("Trim", new Color(0.23f, 0.17f, 0.13f));
        Material publicFloor = Material("PublicFloor", new Color(0.78f, 0.63f, 0.46f));
        Material maleFloor = Material("MaleFloor", new Color(0.48f, 0.45f, 0.72f));
        Material femaleFloor = Material("FemaleFloor", new Color(0.88f, 0.55f, 0.67f));
        Material utilityFloor = Material("UtilityFloor", new Color(0.58f, 0.72f, 0.45f));
        Material wetFloor = Material("WetFloor", new Color(0.48f, 0.70f, 0.78f));
        Material terraceFloor = Material("TerraceFloor", new Color(0.38f, 0.25f, 0.16f));
        Material furniture = Material("Furniture", new Color(0.43f, 0.29f, 0.18f));
        Material fabric = Material("Fabric", new Color(0.72f, 0.67f, 0.58f));
        Material green = Material("Green", new Color(0.25f, 0.48f, 0.22f));

        GameObject root = new GameObject("LOVE PRODUCER FLOOR PLAN (20m x 16m)");
        GameObject floors = Group("Floors", root.transform);
        GameObject walls = Group("Walls", root.transform);
        GameObject props = Group("Furniture & Props", root.transform);
        GameObject labels = Group("Room Labels", root.transform);

        // Overall footprint: X = 20m, Z = 16m. Entrance is at Z = -8.
        Floor("Entrance Hall", new Vector2(4, 2.5f), new Vector3(0, 0, -6.75f), publicFloor, floors.transform);
        Floor("Living Room", new Vector2(8, 5.5f), new Vector3(0, 0, -2.75f), publicFloor, floors.transform);
        Floor("Kitchen", new Vector2(4, 4), new Vector3(-8, 0, -4), publicFloor, floors.transform);
        Floor("Dining Room", new Vector2(4, 3.5f), new Vector3(-4, 0, -4.25f), publicFloor, floors.transform);
        Floor("Lounge", new Vector2(4, 4), new Vector3(6, 0, -4), utilityFloor, floors.transform);
        Floor("Game Room", new Vector2(4, 4), new Vector3(8, 0, -4), utilityFloor, floors.transform);
        Floor("Terrace", new Vector2(6, 4), new Vector3(0, 0.08f, 5.7f), terraceFloor, floors.transform);

        Floor("Male Bedroom 1", new Vector2(3.5f, 3.15f), new Vector3(-8.25f, 0, 6.35f), maleFloor, floors.transform);
        Floor("Male Bedroom 2", new Vector2(3.5f, 3.15f), new Vector3(-8.25f, 0, 3.2f), maleFloor, floors.transform);
        Floor("Male Bedroom 3", new Vector2(3.5f, 3.15f), new Vector3(-8.25f, 0, 0.05f), maleFloor, floors.transform);
        Floor("Male Bathroom", new Vector2(3.5f, 2.55f), new Vector3(-8.25f, 0, -2.8f), wetFloor, floors.transform);

        Floor("Female Bedroom 1", new Vector2(3.5f, 3.15f), new Vector3(8.25f, 0, 6.35f), femaleFloor, floors.transform);
        Floor("Female Bedroom 2", new Vector2(3.5f, 3.15f), new Vector3(8.25f, 0, 3.2f), femaleFloor, floors.transform);
        Floor("Female Bedroom 3", new Vector2(3.5f, 3.15f), new Vector3(8.25f, 0, 0.05f), femaleFloor, floors.transform);
        Floor("Female Bathroom", new Vector2(3.5f, 2.55f), new Vector3(8.25f, 0, -2.8f), wetFloor, floors.transform);

        // Ground slab and outer shell, with an entrance opening in the south wall.
        Box("Foundation", new Vector3(20.4f, 0.2f, 16.4f), new Vector3(0, -0.2f, 0), trim, floors.transform);
        // Rear beach exit: split the north wall around a 1.8 m doorway.
        WallX("North Wall Left", 9.1f, 8, wall, walls.transform, -5.45f);
        WallX("North Wall Right", 9.1f, 8, wall, walls.transform, 5.45f);
        Box(
            "Beach Door Header",
            new Vector3(1.8f, 0.9f, WallThickness),
            new Vector3(0, 2.55f, 8),
            wall,
            walls.transform
        );
        WallZ("West Wall", 16, -10, wall, walls.transform);
        WallZ("East Wall", 16, 10, wall, walls.transform);
        WallX("South Wall L", 9.25f, -8, wall, walls.transform, -5.375f);
        WallX("South Wall R", 9.25f, -8, wall, walls.transform, 5.375f);

        // Main circulation and room boundaries.
        // Split the corridor walls at the DoorMarker positions, creating real
        // 1 m wide and 2.1 m high openings for the first-person controller.
        WallZWithDoors(
            "Left Bedroom Spine", -2.8f, 8f, -6.5f, 1.0f,
            wall, walls.transform, -2.8f, 0.25f, 3.3f, 6.35f
        );
        WallZWithDoors(
            "Right Bedroom Spine", -2.8f, 8f, 6.5f, 1.0f,
            wall, walls.transform, -2.8f, 0.25f, 3.3f, 6.35f
        );
        WallX("Male Row 1", 3.5f, 4.78f, wall, walls.transform, -8.25f);
        WallX("Male Row 2", 3.5f, 1.63f, wall, walls.transform, -8.25f);
        WallX("Male Bath Row", 3.5f, -1.53f, wall, walls.transform, -8.25f);
        WallX("Female Row 1", 3.5f, 4.78f, wall, walls.transform, 8.25f);
        WallX("Female Row 2", 3.5f, 1.63f, wall, walls.transform, 8.25f);
        WallX("Female Bath Row", 3.5f, -1.53f, wall, walls.transform, 8.25f);
        WallX("Terrace Front L", 7, 3.7f, wall, walls.transform, -6.5f);
        WallX("Terrace Front R", 7, 3.7f, wall, walls.transform, 6.5f);
        // WallZ("Kitchen/Dining", 4, -6, wall, walls.transform, -4);
        // WallZ("Dining/Living", 4, -2, wall, walls.transform, -4);
        // WallZ("Living/Lounge", 4, 4, wall, walls.transform, -4);
        // WallZ("Lounge/Game", 4, 6, wall, walls.transform, -4);

        // Door lintels visually mark the intended openings without blocking them.
        DoorMarker("Entrance", new Vector3(0, 1.05f, -8), 1.5f, false, trim, walls.transform);
        DoorMarker("Terrace Door", new Vector3(0, 1.05f, 3.7f), 1.8f, false, trim, walls.transform);
        for (int i = 0; i < 4; i++)
        {
            float z = 6.35f - i * 3.05f;
            DoorMarker("Male Door " + (i + 1), new Vector3(-6.5f, 1.05f, z), 1.0f, true, trim, walls.transform);
            DoorMarker("Female Door " + (i + 1), new Vector3(6.5f, 1.05f, z), 1.0f, true, trim, walls.transform);
        }

        // Simple readable furniture placeholders.
        Bed(new Vector3(-8.25f, 0.35f, 6.35f), maleFloor, props.transform);
        Bed(new Vector3(-8.25f, 0.35f, 3.2f), maleFloor, props.transform);
        Bed(new Vector3(-8.25f, 0.35f, 0.05f), maleFloor, props.transform);
        Bed(new Vector3(8.25f, 0.35f, 6.35f), femaleFloor, props.transform);
        Bed(new Vector3(8.25f, 0.35f, 3.2f), femaleFloor, props.transform);
        Bed(new Vector3(8.25f, 0.35f, 0.05f), femaleFloor, props.transform);
        DiningSet(new Vector3(-4.2f, 0, -4.2f), furniture, props.transform);
        Sofa(new Vector3(-1.8f, 0.35f, -2.8f), fabric, props.transform);
        Sofa(new Vector3(1.8f, 0.35f, -2.8f), fabric, props.transform);
        Box("Coffee Table", new Vector3(1.8f, 0.35f, 1.1f), new Vector3(0, 0.25f, -2.6f), furniture, props.transform);
        Box("Kitchen Counter", new Vector3(3.5f, 0.9f, 0.7f), new Vector3(-8, 0.45f, -5.4f), furniture, props.transform);
        Box("Pool Table", new Vector3(2.2f, 0.8f, 1.2f), new Vector3(8, 0.4f, -4), green, props.transform);
        Box("Terrace Table", new Vector3(1.8f, 0.7f, 1.1f), new Vector3(0, 0.43f, 5.7f), furniture, props.transform);

/*
        Label("露台", new Vector3(0, 0.7f, 5.7f), labels.transform);
        Label("客厅", new Vector3(0, 0.7f, -2.7f), labels.transform);
        Label("厨房", new Vector3(-8, 0.7f, -4), labels.transform);
        Label("餐厅", new Vector3(-4, 0.7f, -4), labels.transform);
        Label("休闲区", new Vector3(5, 0.7f, -4), labels.transform);
        Label("游戏室", new Vector3(8, 0.7f, -4), labels.transform);
        Label("男生卧室区", new Vector3(-8.25f, 0.7f, 7.2f), labels.transform);
        Label("女生卧室区", new Vector3(8.25f, 0.7f, 7.2f), labels.transform);
        Label("入口", new Vector3(0, 0.7f, -7), labels.transform);
*/
        AddLightingAndCamera();
        Selection.activeGameObject = root;
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Love Producer floor plan generated at " + ScenePath);
    }

    private static void AddLightingAndCamera()
    {
        GameObject lightObject = new GameObject("Sun");
        Light sun = lightObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.25f;
        sun.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(48, -32, 0);

        GameObject cameraObject = new GameObject("Top Down Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 12.2f;
        camera.clearFlags = CameraClearFlags.Skybox;
        cameraObject.transform.position = new Vector3(0, 24, -0.5f);
        cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<AudioListener>();

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.58f, 0.58f, 0.58f);
    }

    private static void Bed(Vector3 position, Material material, Transform parent)
    {
        Box("Bed", new Vector3(1.5f, 0.45f, 2.0f), position, material, parent);
        Box("Pillow", new Vector3(1.25f, 0.18f, 0.42f), position + new Vector3(0, 0.31f, 0.68f), Material("Pillow", Color.white), parent);
    }

    private static void Sofa(Vector3 position, Material material, Transform parent)
    {
        Box("Sofa Seat", new Vector3(2.3f, 0.45f, 0.85f), position, material, parent);
        Box("Sofa Back", new Vector3(2.3f, 0.85f, 0.22f), position + new Vector3(0, 0.35f, 0.38f), material, parent);
    }

    private static void DiningSet(Vector3 position, Material material, Transform parent)
    {
        Box("Dining Table", new Vector3(2.2f, 0.75f, 1.1f), position + Vector3.up * 0.375f, material, parent);
        Box("Chair N", new Vector3(0.55f, 0.55f, 0.55f), position + new Vector3(0, 0.275f, 1.05f), material, parent);
        Box("Chair S", new Vector3(0.55f, 0.55f, 0.55f), position + new Vector3(0, 0.275f, -1.05f), material, parent);
        Box("Chair E", new Vector3(0.55f, 0.55f, 0.55f), position + new Vector3(1.45f, 0.275f, 0), material, parent);
        Box("Chair W", new Vector3(0.55f, 0.55f, 0.55f), position + new Vector3(-1.45f, 0.275f, 0), material, parent);
    }

    private static void Floor(string name, Vector2 size, Vector3 center, Material material, Transform parent)
    {
        Box(name, new Vector3(size.x, 0.12f, size.y), center - Vector3.up * 0.06f, material, parent);
    }

    private static void WallZWithDoors(
        string name,
        float wallStartZ,
        float wallEndZ,
        float x,
        float doorWidth,
        Material material,
        Transform parent,
        params float[] doorCenters)
    {
        const float doorHeight = 2.1f;
        float cursor = wallStartZ;

        for (int i = 0; i < doorCenters.Length; i++)
        {
            float doorStart = Mathf.Max(wallStartZ, doorCenters[i] - doorWidth / 2f);
            float doorEnd = Mathf.Min(wallEndZ, doorCenters[i] + doorWidth / 2f);

            if (doorStart > cursor)
            {
                float segmentLength = doorStart - cursor;
                float segmentCenterZ = (cursor + doorStart) / 2f;
                WallZ(name + " Segment " + (i + 1), segmentLength, x, material, parent, segmentCenterZ);
            }

            float headerHeight = WallHeight - doorHeight;
            if (headerHeight > 0 && doorEnd > doorStart)
            {
                Box(
                    name + " Door Header " + (i + 1),
                    new Vector3(WallThickness, headerHeight, doorEnd - doorStart),
                    new Vector3(x, doorHeight + headerHeight / 2f, (doorStart + doorEnd) / 2f),
                    material,
                    parent
                );
            }

            cursor = Mathf.Max(cursor, doorEnd);
        }

        if (cursor < wallEndZ)
        {
            float segmentLength = wallEndZ - cursor;
            float segmentCenterZ = (cursor + wallEndZ) / 2f;
            WallZ(name + " Final Segment", segmentLength, x, material, parent, segmentCenterZ);
        }
    }

    private static void WallX(string name, float length, float z, Material material, Transform parent, float x = 0)
    {
        Box(name, new Vector3(length, WallHeight, WallThickness), new Vector3(x, WallHeight / 2, z), material, parent);
    }

    private static void WallZ(string name, float length, float x, Material material, Transform parent, float z = 0)
    {
        Box(name, new Vector3(WallThickness, WallHeight, length), new Vector3(x, WallHeight / 2, z), material, parent);
    }

    private static void DoorMarker(string name, Vector3 center, float width, bool alongZ, Material material, Transform parent)
    {
        Vector3 size = alongZ
            ? new Vector3(WallThickness + 0.05f, 0.18f, width)
            : new Vector3(width, 0.18f, WallThickness + 0.05f);
        Box(name + " Lintel", size, center + Vector3.up * 1.05f, material, parent);
    }

    private static GameObject Box(string name, Vector3 size, Vector3 position, Material material, Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = size;
        gameObject.GetComponent<Renderer>().sharedMaterial = material;
        return gameObject;
    }

    private static void Label(string text, Vector3 position, Transform parent)
    {
        GameObject labelObject = new GameObject("Label - " + text);
        labelObject.transform.SetParent(parent);
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.28f;
        label.fontSize = 42;
        label.color = new Color(0.12f, 0.09f, 0.12f);
    }

    private static GameObject Group(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        return group;
    }

    private static Material Material(string name, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        material = new Material(shader) { name = name, color = color };
        material.SetFloat("_Smoothness", 0.18f);
        AssetDatabase.CreateAsset(material, path);
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
