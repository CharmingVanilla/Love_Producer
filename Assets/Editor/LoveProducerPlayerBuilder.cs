using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LoveProducerPlayerBuilder
{
    [MenuItem("Love Producer/Add First Person Walker")]
    public static void AddWalker()
    {
        GameObject existing = GameObject.Find("First Person House Walker");
        if (existing != null)
        {
            Selection.activeGameObject = existing;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("First Person House Walker already exists in this scene.");
            return;
        }

        Camera oldMainCamera = Camera.main;
        if (oldMainCamera != null)
        {
            oldMainCamera.tag = "Untagged";
            oldMainCamera.gameObject.SetActive(false);
        }

        GameObject player = new GameObject("First Person House Walker");
        player.transform.position = new Vector3(0, 0.12f, -6.6f);
        player.transform.rotation = Quaternion.Euler(0, 0, 0);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.32f;
        controller.center = new Vector3(0, 0.9f, 0);
        controller.stepOffset = 0.30f;
        controller.slopeLimit = 48f;
        controller.skinWidth = 0.04f;

        GameObject cameraObject = new GameObject("Player Camera");
        cameraObject.transform.SetParent(player.transform);
        cameraObject.transform.localPosition = new Vector3(0, 1.65f, 0);
        cameraObject.transform.localRotation = Quaternion.identity;
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 250f;
        cameraObject.AddComponent<AudioListener>();

        FirstPersonHouseWalker walker = player.AddComponent<FirstPersonHouseWalker>();
        SerializedObject serializedWalker = new SerializedObject(walker);
        serializedWalker.FindProperty("cameraPivot").objectReferenceValue = cameraObject.transform;
        serializedWalker.ApplyModifiedPropertiesWithoutUndo();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body Preview (hidden while playing)";
        body.transform.SetParent(player.transform);
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().enabled = false;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Selection.activeGameObject = player;
        SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log("First person walker added. Press Play, use WASD + mouse, Shift to run, Space to jump, Esc to release cursor.");
    }
}
