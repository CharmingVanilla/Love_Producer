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

        GameObject root = new GameObject("LOVE PRODUCER FLOOR PLAN (24m x 20m)");
        GameObject floors = Group("Floors", root.transform);
        GameObject walls = Group("Walls", root.transform);
        GameObject props = Group("Furniture & Props", root.transform);
        GameObject labels = Group("Room Labels", root.transform);

        // Reference layout: symmetric bedroom wings around a central living
        // room and ocean terrace, with kitchen/dining and lounge at the front.
        Floor("Entrance Hall", new Vector2(3, 2.2f), new Vector3(0, 0, -8.9f), publicFloor, floors.transform);
        Floor("Living Area", new Vector2(9.6f, 8.2f), new Vector3(0, 0, -1.0f), publicFloor, floors.transform);
        Floor("Kitchen and Dining", new Vector2(7.2f, 7.8f), new Vector3(-8.4f, 0, -5.1f), publicFloor, floors.transform);
        Floor("Lounge", new Vector2(7.2f, 7.8f), new Vector3(8.4f, 0, -5.1f), utilityFloor, floors.transform);
        Floor("Main Terrace", new Vector2(10, 5.5f), new Vector3(0, 0.08f, 7.25f), terraceFloor, floors.transform);
        Floor("West Side Terrace", new Vector2(2.2f, 4.6f), new Vector3(-12.9f, 0.06f, -3.4f), terraceFloor, floors.transform);
        Floor("East Side Terrace", new Vector2(2.2f, 4.6f), new Vector3(12.9f, 0.06f, -3.4f), terraceFloor, floors.transform);

        float[] bedroomZ = { 7.5f, 4.5f, 1.5f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            Floor("West Bedroom " + (i + 1), new Vector2(5, 3), new Vector3(-9.5f, 0, bedroomZ[i]), maleFloor, floors.transform);
            Floor("East Bedroom " + (i + 4), new Vector2(5, 3), new Vector3(9.5f, 0, bedroomZ[i]), femaleFloor, floors.transform);
        }
        Floor("West Shared Bathroom", new Vector2(2.2f, 3.2f), new Vector3(-5.9f, 0, 0), wetFloor, floors.transform);
        Floor("East Shared Bathroom", new Vector2(2.2f, 3.2f), new Vector3(5.9f, 0, 0), wetFloor, floors.transform);

        // Ground slab and outer shell, with an entrance opening in the south wall.
        Box("Foundation", new Vector3(24.4f, 0.2f, 20.4f), new Vector3(0, -0.2f, 0), trim, floors.transform);
        WallX("North Wall West Wing", 7, 10, wall, walls.transform, -8.5f);
        WallX("North Wall East Wing", 7, 10, wall, walls.transform, 8.5f);
        WallZWithDoors("West Exterior Wall", -10, 10, -12, 1.4f, wall, walls.transform, -3.4f);
        WallZWithDoors("East Exterior Wall", -10, 10, 12, 1.4f, wall, walls.transform, -3.4f);
        WallX("South Wall West", 10.5f, -10, wall, walls.transform, -6.75f);
        WallX("South Wall East", 10.5f, -10, wall, walls.transform, 6.75f);
        Box("Entrance Header", new Vector3(3, 0.9f, WallThickness), new Vector3(0, 2.55f, -10), wall, walls.transform);

        // Main circulation and room boundaries.
        // Split the corridor walls at the DoorMarker positions, creating real
        // 1 m wide and 2.1 m high openings for the first-person controller.
        // Three bedrooms per side, opening toward the central circulation.
        WallZWithDoors(
            "West Bedroom Spine", 0f, 10f, -7f, 1.9f,
            wall, walls.transform, 1.5f, 4.5f, 7.5f
        );
        WallZWithDoors(
            "East Bedroom Spine", 0f, 10f, 7f, 1.9f,
            wall, walls.transform, 1.5f, 4.5f, 7.5f
        );
        WallX("West Bedroom Boundary 1", 4.7f, 6f, wall, walls.transform, -9.65f);
        WallX("West Bedroom Boundary 2", 4.7f, 3f, wall, walls.transform, -9.65f);
        WallX("West Bedroom South Wall", 4.7f, 0f, wall, walls.transform, -9.65f);
        WallX("East Bedroom Boundary 1", 4.7f, 6f, wall, walls.transform, 9.65f);
        WallX("East Bedroom Boundary 2", 4.7f, 3f, wall, walls.transform, 9.65f);
        WallX("East Bedroom South Wall", 4.7f, 0f, wall, walls.transform, 9.65f);

        // Shared bathrooms sit between the bedroom wings and living area.
        BathroomWalls("West Shared Bathroom", -5.9f, wall, walls.transform, false);
        BathroomWalls("East Shared Bathroom", 5.9f, wall, walls.transform, true);

        // Wide central opening and steps connect living area to main terrace.
        // Stop short of the bedroom spines so these walls do not cut through
        // the middle bedroom door openings at z = 4.5.
        WallX("Terrace Front West", 3.2f, 4.5f, wall, walls.transform, -5.1f);
        WallX("Terrace Front East", 3.2f, 4.5f, wall, walls.transform, 5.1f);

        // Door lintels visually mark the intended openings without blocking them.
        DoorMarker("Entrance", new Vector3(0, 1.05f, -10), 3f, false, trim, walls.transform);
        DoorMarker("Terrace Opening", new Vector3(0, 1.05f, 4.5f), 7f, false, trim, walls.transform);
        // Bedroom openings already include structural headers. Do not add a
        // second decorative lintel, which reads as a split doorway in perspective.

        // Decorative furniture, rugs, plants and practical lights are owned by
        // LoveProducerEnvironmentEnhancer so rebuilding never creates overlaps.

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
        AddLightingAndCamera(root.transform);
        Selection.activeGameObject = root;
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Love Producer floor plan generated at " + ScenePath);
    }

    [MenuItem("Love Producer/Fix Split Bedroom Doorways")]
    public static void FixSplitBedroomDoorways()
    {
        GameObject floorPlan = GameObject.Find("LOVE PRODUCER FLOOR PLAN (24m x 20m)");
        Transform walls = floorPlan != null ? floorPlan.transform.Find("Walls") : null;
        if (walls == null)
        {
            Debug.LogError("Doorway repair stopped: the floor-plan Walls group was not found.");
            return;
        }

        Material wall = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/Wall.mat");
        if (wall == null)
        {
            Debug.LogError("Doorway repair stopped: the Wall material was not found.");
            return;
        }

        // Remove every previous spine/header/marker so overlapping generations
        // cannot leave a vertical strip inside a doorway.
        for (int i = walls.childCount - 1; i >= 0; i--)
        {
            Transform child = walls.GetChild(i);
            bool isBedroomDoorWall = child.name.StartsWith("West Bedroom Spine")
                || child.name.StartsWith("East Bedroom Spine")
                || child.name.StartsWith("West Bedroom Door")
                || child.name.StartsWith("East Bedroom Door")
                || child.name.StartsWith("West Bedroom Boundary")
                || child.name.StartsWith("East Bedroom Boundary")
                || child.name.StartsWith("West Bedroom South Wall")
                || child.name.StartsWith("East Bedroom South Wall")
                || child.name.StartsWith("West Shared Bathroom North Wall")
                || child.name.StartsWith("East Shared Bathroom North Wall")
                || child.name.StartsWith("Terrace Front West")
                || child.name.StartsWith("Terrace Front East");
            if (isBedroomDoorWall) Undo.DestroyObjectImmediate(child.gameObject);
        }

        const float openingWidth = 1.9f;
        float[] doorZ = { 1.5f, 4.5f, 7.5f };
        WallZWithDoors("West Bedroom Spine", 0f, 10f, -7f, openingWidth, wall, walls, doorZ);
        WallZWithDoors("East Bedroom Spine", 0f, 10f, 7f, openingWidth, wall, walls, doorZ);
        WallX("West Bedroom Boundary 1", 4.7f, 6f, wall, walls, -9.65f);
        WallX("West Bedroom Boundary 2", 4.7f, 3f, wall, walls, -9.65f);
        WallX("West Bedroom South Wall", 4.7f, 0f, wall, walls, -9.65f);
        WallX("East Bedroom Boundary 1", 4.7f, 6f, wall, walls, 9.65f);
        WallX("East Bedroom Boundary 2", 4.7f, 3f, wall, walls, 9.65f);
        WallX("East Bedroom South Wall", 4.7f, 0f, wall, walls, 9.65f);
        // These were the actual intersecting walls: the bathroom north walls
        // cut the lower doors and the terrace-front walls cut the middle doors.
        WallX("West Shared Bathroom North Wall", 1.9f, 1.6f, wall, walls, -5.75f);
        WallX("East Shared Bathroom North Wall", 1.9f, 1.6f, wall, walls, 5.75f);
        WallX("Terrace Front West", 3.2f, 4.5f, wall, walls, -5.1f);
        WallX("Terrace Front East", 3.2f, 4.5f, wall, walls, 5.1f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = walls.gameObject;
        Debug.Log("Bedroom doors repaired, including the bathroom and terrace walls that intersected four openings.");
    }

    [MenuItem("Love Producer/Fix Bedroom Exterior Window Openings")]
    public static void FixBedroomExteriorWindowOpenings()
    {
        GameObject floorPlan = GameObject.Find("LOVE PRODUCER FLOOR PLAN (24m x 20m)");
        Transform walls = floorPlan != null ? floorPlan.transform.Find("Walls") : null;
        if (walls == null)
        {
            Debug.LogError("Window opening repair stopped: the floor-plan Walls group was not found.");
            return;
        }

        Material wall = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/Detailed Warm Plaster.mat");
        if (wall == null) wall = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/Wall.mat");
        if (wall == null)
        {
            Debug.LogError("Window opening repair stopped: no wall material was found.");
            return;
        }

        for (int i = walls.childCount - 1; i >= 0; i--)
        {
            Transform child = walls.GetChild(i);
            if (child.name.StartsWith("West Exterior Wall") || child.name.StartsWith("East Exterior Wall"))
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        ExteriorWallWithBedroomWindows("West Exterior Wall", -12f, wall, walls);
        ExteriorWallWithBedroomWindows("East Exterior Wall", 12f, wall, walls);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Bedroom exterior walls rebuilt with six real window openings and two side-terrace doors.");
    }

    private static void ExteriorWallWithBedroomWindows(string name, float x, Material material, Transform parent)
    {
        const float startZ = -10f;
        const float endZ = 10f;
        const float doorCenter = -3.4f;
        const float doorWidth = 1.4f;
        const float windowWidth = 1.75f;
        const float windowBottom = 0.62f;
        const float windowTop = 2.28f;
        float[] centers = { doorCenter, 1.5f, 4.5f, 7.5f };
        float cursor = startZ;

        for (int i = 0; i < centers.Length; i++)
        {
            bool isDoor = i == 0;
            float width = isDoor ? doorWidth : windowWidth;
            float openingStart = centers[i] - width * 0.5f;
            float openingEnd = centers[i] + width * 0.5f;
            if (openingStart > cursor)
                WallZ(name + " Segment " + i, openingStart - cursor, x, material, parent, (cursor + openingStart) * 0.5f);

            if (isDoor)
            {
                Box(name + " Door Header", new Vector3(WallThickness, WallHeight - 2.1f, width),
                    new Vector3(x, 2.1f + (WallHeight - 2.1f) * 0.5f, centers[i]), material, parent);
            }
            else
            {
                Box(name + " Window Sill Wall " + i, new Vector3(WallThickness, windowBottom, width),
                    new Vector3(x, windowBottom * 0.5f, centers[i]), material, parent);
                float headerHeight = WallHeight - windowTop;
                Box(name + " Window Header " + i, new Vector3(WallThickness, headerHeight, width),
                    new Vector3(x, windowTop + headerHeight * 0.5f, centers[i]), material, parent);
            }
            cursor = openingEnd;
        }

        if (cursor < endZ)
            WallZ(name + " Final Segment", endZ - cursor, x, material, parent, (cursor + endZ) * 0.5f);
    }

    private static void AddLightingAndCamera(Transform parent)
    {
        GameObject lightObject = new GameObject("Sun");
        lightObject.transform.SetParent(parent);
        Light sun = lightObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.25f;
        sun.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(48, -32, 0);

        GameObject moonObject = new GameObject("Moon");
        moonObject.transform.SetParent(parent);
        Light moon = moonObject.AddComponent<Light>();
        moon.type = LightType.Directional;
        moon.color = new Color(0.30f, 0.42f, 0.70f);
        moon.intensity = 0.22f;
        moon.shadows = LightShadows.Soft;
        moonObject.transform.rotation = Quaternion.Euler(32, 145, 0);

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

        LoveProducerDayNightController controller = parent.gameObject.AddComponent<LoveProducerDayNightController>();
        controller.sun = sun;
        controller.moon = moon;
        controller.dayAmbient = new Color(0.58f, 0.55f, 0.50f);
        controller.nightAmbient = new Color(0.055f, 0.075f, 0.13f);
        controller.SetNight(false);
    }

    private static void AddInteriorDetails(
        Transform parent, Material wood, Material cream, Material coral,
        Material teal, Material green, Material brass, Material lampGlow)
    {
        // Living room: large woven rug, accent cushions and side tables.
        Box("Living Woven Rug", new Vector3(5.6f, 0.035f, 3.6f), new Vector3(0, 0.03f, -2.65f), cream, parent);
        Cushion(new Vector3(-2.35f, 0.72f, -2.78f), coral, parent);
        Cushion(new Vector3(-1.35f, 0.72f, -2.78f), teal, parent);
        Cushion(new Vector3(1.35f, 0.72f, -2.78f), coral, parent);
        Cushion(new Vector3(2.35f, 0.72f, -2.78f), teal, parent);
        SideTable(new Vector3(-3.2f, 0, -2.8f), wood, brass, parent);
        SideTable(new Vector3(3.2f, 0, -2.8f), wood, brass, parent);

        // Kitchen island, stools and small countertop props.
        Box("Kitchen Island", new Vector3(2.8f, 0.92f, 0.9f), new Vector3(-7.9f, 0.46f, -3.5f), cream, parent);
        for (int i = 0; i < 3; i++)
            Stool(new Vector3(-8.75f + i * 0.85f, 0, -2.75f), wood, coral, parent);
        Box("Fruit Bowl", new Vector3(0.55f, 0.14f, 0.42f), new Vector3(-7.9f, 1.0f, -3.5f), brass, parent);

        // Each bedroom receives a warm bedside vignette and contrasting runner.
        float[] bedroomZ = { 6.35f, 3.2f, 0.05f };
        for (int i = 0; i < bedroomZ.Length; i++)
        {
            BedroomDetails(new Vector3(-8.25f, 0, bedroomZ[i]), i % 2 == 0 ? teal : coral, wood, brass, lampGlow, parent);
            BedroomDetails(new Vector3(8.25f, 0, bedroomZ[i]), i % 2 == 0 ? coral : teal, wood, brass, lampGlow, parent);
        }

        // Plants frame circulation paths and the terrace without blocking doors.
        Vector3[] plantPositions =
        {
            new Vector3(-5.7f, 0, -1.4f), new Vector3(5.7f, 0, -1.4f),
            new Vector3(-5.7f, 0, 3.0f), new Vector3(5.7f, 0, 3.0f),
            new Vector3(-2.4f, 0, 6.9f), new Vector3(2.4f, 0, 6.9f),
            new Vector3(-9.35f, 0, -6.8f), new Vector3(9.35f, 0, -6.8f)
        };
        foreach (Vector3 position in plantPositions) Plant(position, wood, green, parent);

        // Terrace dining and overhead festoon bulbs for the evening mood.
        for (int i = -2; i <= 2; i++)
            HangingBulb(new Vector3(i * 1.25f, 2.55f, 5.7f), lampGlow, parent);
    }

    private static void BedroomDetails(Vector3 center, Material accent, Material wood, Material brass, Material glow, Transform parent)
    {
        Box("Bed Runner", new Vector3(1.42f, 0.06f, 0.48f), center + new Vector3(0, 0.61f, -0.48f), accent, parent);
        Vector3 table = center + new Vector3(1.05f, 0, 0.58f);
        SideTable(table, wood, brass, parent);
        HangingBulb(table + new Vector3(0, 1.35f, 0), glow, parent);
    }

    private static void Cushion(Vector3 position, Material material, Transform parent)
    {
        Box("Accent Cushion", new Vector3(0.5f, 0.18f, 0.48f), position, material, parent);
    }

    private static void SideTable(Vector3 position, Material wood, Material brass, Transform parent)
    {
        Cylinder("Round Side Table", new Vector3(0.55f, 0.12f, 0.55f), position + Vector3.up * 0.48f, wood, parent);
        Cylinder("Side Table Base", new Vector3(0.10f, 0.45f, 0.10f), position + Vector3.up * 0.23f, brass, parent);
    }

    private static void Stool(Vector3 position, Material wood, Material fabric, Transform parent)
    {
        Cylinder("Bar Stool Seat", new Vector3(0.45f, 0.12f, 0.45f), position + Vector3.up * 0.68f, fabric, parent);
        Cylinder("Bar Stool Base", new Vector3(0.09f, 0.62f, 0.09f), position + Vector3.up * 0.32f, wood, parent);
    }

    private static void Plant(Vector3 position, Material pot, Material leaves, Transform parent)
    {
        Cylinder("Plant Pot", new Vector3(0.48f, 0.45f, 0.48f), position + Vector3.up * 0.22f, pot, parent);
        for (int i = 0; i < 5; i++)
        {
            GameObject leaf = Box("Tropical Leaf", new Vector3(0.12f, 0.75f, 0.28f), position + new Vector3(0, 0.82f, 0), leaves, parent);
            leaf.transform.rotation = Quaternion.Euler(18, i * 72f, i % 2 == 0 ? 24f : -24f);
        }
    }

    private static void HangingBulb(Vector3 position, Material glow, Transform parent)
    {
        GameObject bulb = Sphere("Warm Bulb", new Vector3(0.13f, 0.13f, 0.13f), position, glow, parent);
        Light light = bulb.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.55f, 0.28f);
        light.intensity = 1.5f;
        light.range = 4.2f;
        light.shadows = LightShadows.Soft;
        light.enabled = false;
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

    private static void BathroomWalls(string name, float centerX, Material material, Transform parent, bool eastSide)
    {
        float innerX = centerX + (eastSide ? -1.1f : 1.1f);
        float outerX = centerX + (eastSide ? 1.1f : -1.1f);
        WallZWithDoors(name + " Inner Wall", -1.6f, 1.6f, innerX, 1.3f, material, parent, 0f);
        WallZ(name + " Outer Lower Wall", 1.6f, outerX, material, parent, -0.8f);
        // Leave clearance beside the bedroom spine; otherwise this wall ends
        // inside the lower bedroom doorway centered at z = 1.5.
        float northWallCenterX = centerX + (eastSide ? -0.15f : 0.15f);
        WallX(name + " North Wall", 1.9f, 1.6f, material, parent, northWallCenterX);
        WallX(name + " South Wall", 2.2f, -1.6f, material, parent, centerX);
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
        return Primitive(name, PrimitiveType.Cube, size, position, material, parent);
    }

    private static GameObject Cylinder(string name, Vector3 size, Vector3 position, Material material, Transform parent)
    {
        return Primitive(name, PrimitiveType.Cylinder, size, position, material, parent);
    }

    private static GameObject Sphere(string name, Vector3 size, Vector3 position, Material material, Transform parent)
    {
        return Primitive(name, PrimitiveType.Sphere, size, position, material, parent);
    }

    private static GameObject Primitive(string name, PrimitiveType type, Vector3 size, Vector3 position, Material material, Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
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

    private static Material Material(string name, Color color, bool emissive = false)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.8f);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        material = new Material(shader) { name = name, color = color };
        material.SetFloat("_Smoothness", 0.18f);
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.8f);
        }
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
