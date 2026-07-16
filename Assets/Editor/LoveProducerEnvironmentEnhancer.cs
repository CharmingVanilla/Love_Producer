using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class LoveProducerEnvironmentEnhancer
{
    private const string MaterialFolder = "Assets/Generated/FloorPlanMaterials";

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
        Material black = Mat("Black", new Color(0.045f, 0.045f, 0.04f));
        Material white = Mat("White", new Color(0.94f, 0.93f, 0.88f));
        Material rug = Mat("Living Rug", new Color(0.68f, 0.55f, 0.40f));

        GameObject root = new GameObject("DETAILS & ENVIRONMENT");
        Transform environment = Group("Island Environment", root.transform);
        Transform architecture = Group("Doors Windows & Steps", root.transform);
        Transform furniture = Group("Detailed Furniture", root.transform);
        Transform plants = Group("Plants", root.transform);
        Transform lighting = Group("Lighting", root.transform);

        // Island setting: lawn around the house, a sandy beach behind it, and ocean.
        Box("Lawn", new Vector3(29, 0.18f, 25), new Vector3(0, -0.36f, -0.5f), grass, environment);
        Box("Beach", new Vector3(29, 0.14f, 7), new Vector3(0, -0.29f, 12), sand, environment);
        Box("Ocean", new Vector3(38, 0.10f, 14), new Vector3(0, -0.25f, 21.5f), water, environment);
        Box("Front Path", new Vector3(2.1f, 0.09f, 5), new Vector3(0, -0.18f, -10.5f), path, environment);
        for (int i = 0; i < 5; i++)
            Box("Stepping Stone " + (i + 1), new Vector3(1.45f, 0.12f, 0.75f),
                new Vector3(0, -0.1f + i * 0.10f, -8.45f - i * 0.7f), path, environment);

        // Front stairs and terrace steps.
        for (int i = 0; i < 3; i++)
        {
            float width = 2.6f + i * 0.45f;
            Box("Entrance Step " + (i + 1), new Vector3(width, 0.18f, 0.65f),
                new Vector3(0, 0.09f + i * 0.16f, -8.35f - i * 0.48f), wood, architecture);
        }
        Box("Terrace Step", new Vector3(3.6f, 0.22f, 0.75f), new Vector3(0, 0.11f, 8.35f), wood, architecture);

        // Door leaves make the openings understandable from the top view.
        Door("Front Door", new Vector3(-0.72f, 1.05f, -7.92f), new Vector3(1.35f, 2.1f, 0.08f), wood, architecture);
        Door("Terrace Door L", new Vector3(-0.95f, 1.05f, 3.72f), new Vector3(0.85f, 2.1f, 0.07f), glass, architecture);
        Door("Terrace Door R", new Vector3(0.95f, 1.05f, 3.72f), new Vector3(0.85f, 2.1f, 0.07f), glass, architecture);

        // Exterior windows.
        for (int i = 0; i < 4; i++)
        {
            float z = 6.2f - i * 3.0f;
            Window("West Window " + i, new Vector3(-10.03f, 1.45f, z), true, glass, darkWood, architecture);
            Window("East Window " + i, new Vector3(10.03f, 1.45f, z), true, glass, darkWood, architecture);
        }
        Window("Kitchen Window", new Vector3(-8.2f, 1.45f, -8.03f), false, glass, darkWood, architecture);
        Window("Game Window", new Vector3(8.2f, 1.45f, -8.03f), false, glass, darkWood, architecture);

        // Bedrooms: beds, pillows, bedside tables, wardrobes and lamps.
        float[] bedroomZ = { 6.35f, 3.2f, 0.05f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            Bedroom(new Vector3(-8.35f, 0, bedroomZ[i]), blue, wood, white, metal, furniture, false);
            Bedroom(new Vector3(8.35f, 0, bedroomZ[i]), pink, wood, white, metal, furniture, true);
        }

        // Bathrooms.
        Bathroom(new Vector3(-8.25f, 0, -2.75f), ceramic, glass, metal, furniture, false);
        Bathroom(new Vector3(8.25f, 0, -2.75f), ceramic, glass, metal, furniture, true);

        // Kitchen with L-shaped counters, sink, cooker and refrigerator.
        Box("Kitchen Back Counter", new Vector3(3.45f, 0.88f, 0.62f), new Vector3(-8.1f, 0.44f, -5.55f), wood, furniture);
        Box("Kitchen Side Counter", new Vector3(0.62f, 0.88f, 2.5f), new Vector3(-9.55f, 0.44f, -4.25f), wood, furniture);
        Box("Kitchen Worktop A", new Vector3(3.55f, 0.08f, 0.72f), new Vector3(-8.1f, 0.92f, -5.55f), ceramic, furniture);
        Box("Fridge", new Vector3(0.75f, 1.85f, 0.7f), new Vector3(-9.4f, 0.925f, -2.85f), metal, furniture);
        Box("Sink", new Vector3(0.85f, 0.06f, 0.45f), new Vector3(-8.2f, 0.98f, -5.55f), metal, furniture);
        Box("Cooktop", new Vector3(0.75f, 0.04f, 0.48f), new Vector3(-7.0f, 0.98f, -5.55f), black, furniture);

        // Dining and living area.
        DiningSet(new Vector3(-4.15f, 0, -4.15f), wood, cream, furniture);
        Box("Living Rug", new Vector3(4.8f, 0.035f, 3.1f), new Vector3(0, 0.04f, -2.45f), rug, furniture);
        Sofa("Living Sofa Left", new Vector3(-2.15f, 0, -2.55f), Quaternion.Euler(0, 90, 0), cream, furniture);
        Sofa("Living Sofa Right", new Vector3(2.15f, 0, -2.55f), Quaternion.Euler(0, -90, 0), cream, furniture);
        Box("Coffee Table Top", new Vector3(1.65f, 0.14f, 0.9f), new Vector3(0, 0.38f, -2.55f), wood, furniture);
        Cylinder("Coffee Table Base", new Vector3(0, 0.19f, -2.55f), new Vector3(0.25f, 0.38f, 0.25f), darkWood, furniture);
        Box("TV Console", new Vector3(2.4f, 0.55f, 0.42f), new Vector3(0, 0.275f, -0.35f), wood, furniture);
        Box("Television", new Vector3(1.75f, 1.0f, 0.10f), new Vector3(0, 1.2f, -0.12f), black, furniture);

        // Lounge and game room.
        Sofa("Lounge Sofa", new Vector3(5.0f, 0, -4.5f), Quaternion.identity, cream, furniture);
        Box("Lounge Side Table", new Vector3(0.65f, 0.48f, 0.65f), new Vector3(5.0f, 0.24f, -2.9f), wood, furniture);
        PoolTable(new Vector3(8.0f, 0, -4.0f), wood, leaf, furniture);

        // Terrace seating and planters.
        DiningSet(new Vector3(0, 0.10f, 5.75f), wood, cream, furniture);
        Box("Terrace Bench L", new Vector3(1.8f, 0.48f, 0.55f), new Vector3(-2.15f, 0.35f, 6.9f), wood, furniture);
        Box("Terrace Bench R", new Vector3(1.8f, 0.48f, 0.55f), new Vector3(2.15f, 0.35f, 6.9f), wood, furniture);

        // Dense tropical planting similar to the reference illustration.
        Vector3[] plantPositions =
        {
            new Vector3(-12, 0, -7), new Vector3(12, 0, -6), new Vector3(-12.5f, 0, 2),
            new Vector3(12.3f, 0, 3), new Vector3(-11.8f, 0, 8.5f), new Vector3(11.6f, 0, 8.8f),
            new Vector3(-7, 0, 10), new Vector3(7, 0, 10), new Vector3(-3.5f, 0, 9.1f),
            new Vector3(3.8f, 0, 9.3f), new Vector3(-2.8f, 0, -9.2f), new Vector3(3.0f, 0, -9.3f)
        };
        for (int i = 0; i < plantPositions.Length; i++)
        {
            if (i < 6) Palm("Palm " + i, plantPositions[i], wood, i % 2 == 0 ? leaf : leafLight, plants);
            else Bush("Bush " + i, plantPositions[i], i % 2 == 0 ? leaf : leafLight, plants);
        }

        // Warm point lights help the rooms read clearly in perspective view.
        Vector3[] lightPositions =
        {
            new Vector3(0, 2.65f, -2.5f), new Vector3(-4, 2.65f, -4),
            new Vector3(-8, 2.65f, 3), new Vector3(8, 2.65f, 3),
            new Vector3(0, 2.65f, 5.8f)
        };
        foreach (Vector3 p in lightPositions) PointLight(p, lighting);

        ImproveSceneLighting();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Selection.activeGameObject = root;
        Debug.Log("Details and island environment added successfully.");
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

    private static void PointLight(Vector3 p, Transform parent)
    {
        GameObject go = new GameObject("Warm Room Light");
        go.transform.SetParent(parent);
        go.transform.position = p;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 7;
        light.intensity = 2.0f;
        light.color = new Color(1.0f, 0.78f, 0.56f);
        light.shadows = LightShadows.None;
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
            camera.orthographicSize = 16f;
            camera.transform.position = new Vector3(0, 28, 1.5f);
            camera.backgroundColor = new Color(0.58f, 0.76f, 0.83f);
        }
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

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folder = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
