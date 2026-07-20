using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class CircularSofaGenerator : MonoBehaviour
{
    private const string SegmentNamePrefix = "SofaSegment_";
    private const string PlaceholderName = "PlaceholderSegment_Template";

    [Header("Sofa Module")]
    [SerializeField] private GameObject sofaPiecePrefab;

    [Header("Layout")]
    [SerializeField, Min(1)] private int segmentCount = 10;
    [SerializeField, Min(0.01f)] private float radius = 3.2f;
    [SerializeField, Range(1f, 360f)] private float arcAngle = 270f;
    [SerializeField] private float startAngle;
    [SerializeField] private float height;

    [Header("Module Transform")]
    [SerializeField] private float rotationOffset;
    [SerializeField, Min(0.01f)] private float segmentScale = 1f;
    [SerializeField] private bool faceCenter = true;

    [Header("Build Options")]
    [SerializeField] private bool clearBeforeBuild = true;

    public GameObject SofaPiecePrefab => sofaPiecePrefab;
    public int SegmentCount => segmentCount;

    public void BuildSofa()
    {
        if (sofaPiecePrefab == null)
        {
            Debug.LogError(
                "CircularSofaGenerator: Sofa Piece Prefab is not assigned. " +
                "Assign a prefab or click 'Create Placeholder Segment' first.",
                this);
            return;
        }

        if (clearBeforeBuild) ClearSofa();

#if UNITY_EDITOR
        int undoGroup = 0;
        if (!Application.isPlaying)
        {
            Undo.IncrementCurrentGroup();
            undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Build Circular Sofa");
        }
#endif

        // A 360-degree arc must not duplicate the first module at its endpoint.
        bool closedCircle = Mathf.Approximately(arcAngle, 360f);
        float divisor = closedCircle ? segmentCount : Mathf.Max(1, segmentCount - 1);

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = startAngle + arcAngle * (i / divisor);
            float radians = angle * Mathf.Deg2Rad;

            // Required circle formula, expressed in the generator's local space.
            Vector3 localPosition = new Vector3(
                Mathf.Sin(radians) * radius,
                height,
                Mathf.Cos(radians) * radius);

            GameObject segment = InstantiateSegment(sofaPiecePrefab);
            if (segment == null) continue;

            segment.name = $"{SegmentNamePrefix}{i + 1:00}";
            segment.transform.SetParent(transform, true);
            segment.transform.localPosition = localPosition;
            segment.transform.localScale *= segmentScale;
            segment.SetActive(true);

            if (faceCenter)
            {
                Vector3 worldPosition = transform.TransformPoint(localPosition);
                Vector3 directionToCenter = transform.position - worldPosition;
                if (directionToCenter.sqrMagnitude > 0.0001f)
                {
                    segment.transform.rotation =
                        Quaternion.LookRotation(directionToCenter, transform.up) *
                        Quaternion.Euler(0f, rotationOffset, 0f);
                }
            }
            else
            {
                segment.transform.localRotation = Quaternion.Euler(0f, angle + rotationOffset, 0f);
            }
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(this);
        }
#endif
    }

    public void RebuildSofa()
    {
        ClearSofa();
        BuildSofa();
    }

    public void ClearSofa()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (!child.name.StartsWith(SegmentNamePrefix)) continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(child);
            else
                Destroy(child);
#else
            Destroy(child);
#endif
        }
    }

    public void CreatePlaceholderSegment()
    {
        Transform existing = transform.Find(PlaceholderName);
        if (existing != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) Undo.RecordObject(this, "Assign Placeholder Segment");
#endif
            sofaPiecePrefab = existing.gameObject;
            return;
        }

        GameObject placeholder = new GameObject(PlaceholderName);
#if UNITY_EDITOR
        if (!Application.isPlaying) Undo.RegisterCreatedObjectUndo(placeholder, "Create Placeholder Sofa Segment");
#endif
        placeholder.transform.SetParent(transform, false);

        CreatePlaceholderCube("Seat Cushion", placeholder.transform,
            new Vector3(0f, 0.28f, 0f), new Vector3(1.4f, 0.45f, 0.9f));
        CreatePlaceholderCube("Back Rest", placeholder.transform,
            new Vector3(0f, 0.78f, 0.37f), new Vector3(1.4f, 0.85f, 0.18f));
        CreatePlaceholderCube("Left Arm", placeholder.transform,
            new Vector3(-0.64f, 0.48f, 0f), new Vector3(0.14f, 0.58f, 0.9f));
        CreatePlaceholderCube("Right Arm", placeholder.transform,
            new Vector3(0.64f, 0.48f, 0f), new Vector3(0.14f, 0.58f, 0.9f));

#if UNITY_EDITOR
        if (!Application.isPlaying) Undo.RecordObject(this, "Assign Placeholder Segment");
#endif
        sofaPiecePrefab = placeholder;
        placeholder.SetActive(false);

#if UNITY_EDITOR
        if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
    }

    private GameObject InstantiateSegment(GameObject source)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject instance = PrefabUtility.IsPartOfPrefabAsset(source)
                ? PrefabUtility.InstantiatePrefab(source, transform) as GameObject
                : Instantiate(source, transform);

            if (instance != null) Undo.RegisterCreatedObjectUndo(instance, "Create Sofa Segment");
            return instance;
        }
#endif
        return Instantiate(source, transform);
    }

    private static void CreatePlaceholderCube(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = localScale;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        const int previewSteps = 64;
        Gizmos.color = new Color(1f, 0.62f, 0.2f, 0.95f);

        Vector3 previous = GetArcPoint(startAngle);
        for (int i = 1; i <= previewSteps; i++)
        {
            float angle = startAngle + arcAngle * (i / (float)previewSteps);
            Vector3 current = GetArcPoint(angle);
            Gizmos.DrawLine(previous, current);
            previous = current;
        }

        Vector3 center = transform.TransformPoint(new Vector3(0f, height, 0f));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, Mathf.Max(0.08f, radius * 0.035f));

        // The entrance is centered in the uncovered part of the circle.
        float gapAngle = startAngle + arcAngle + (360f - arcAngle) * 0.5f;
        float gapRadians = gapAngle * Mathf.Deg2Rad;
        Vector3 localGapDirection = new Vector3(Mathf.Sin(gapRadians), 0f, Mathf.Cos(gapRadians));
        Vector3 gapEnd = transform.TransformPoint(new Vector3(
            localGapDirection.x * radius * 0.8f,
            height,
            localGapDirection.z * radius * 0.8f));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, gapEnd);
        Gizmos.DrawWireSphere(gapEnd, Mathf.Max(0.06f, radius * 0.025f));
    }

    private Vector3 GetArcPoint(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return transform.TransformPoint(new Vector3(
            Mathf.Sin(radians) * radius,
            height,
            Mathf.Cos(radians) * radius));
    }
#endif
}
