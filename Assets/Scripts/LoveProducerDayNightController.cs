using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[ExecuteAlways]
public sealed class LoveProducerDayNightController : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool nightMode;
    [SerializeField] private KeyCode toggleKey = KeyCode.N;
    [SerializeField, Min(0f)] private float transitionDuration = 2f;
    [SerializeField] private bool reduceMotion;

    [Header("Scene lights")]
    public Light sun;
    public Light moon;
    [Tooltip("Warm lamps and practical lights that are active during Blue Hour Night.")]
    public Light[] practicalLights;
    public Color dayAmbient = new Color(0.58f, 0.55f, 0.50f);
    public Color nightAmbient = new Color(0.17f, 0.18f, 0.24f);

    [Header("Accessible state indicator")]
    [SerializeField] private bool showStateIndicator = true;

    private Coroutine _transition;
    private float _blend;
    private float[] _practicalIntensities;
    private GUIStyle _indicatorStyle;

    public bool IsNight => nightMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstalled()
    {
        if (FindAnyObjectByType<LoveProducerDayNightController>() != null) return;

        Light foundSun = null;
        Light[] sceneLights = FindObjectsByType<Light>(FindObjectsInactive.Include);
        foreach (Light light in sceneLights)
        {
            if (light.type == LightType.Directional && light.name == "Sun")
            {
                foundSun = light;
                break;
            }
        }

        GameObject system = new GameObject("Day Night System");
        LoveProducerDayNightController controller = system.AddComponent<LoveProducerDayNightController>();
        controller.sun = foundSun;

        GameObject moonObject = new GameObject("Moon");
        moonObject.transform.SetParent(system.transform);
        moonObject.transform.rotation = Quaternion.Euler(32, 145, 0);
        Light createdMoon = moonObject.AddComponent<Light>();
        createdMoon.type = LightType.Directional;
        createdMoon.color = new Color(0.30f, 0.42f, 0.70f);
        createdMoon.shadows = LightShadows.Soft;
        controller.moon = createdMoon;
        controller.practicalLights = System.Array.FindAll(
            sceneLights,
            light => light != null && light != foundSun && light.type != LightType.Directional);
        controller.SetNight(false);
    }

    private void OnEnable()
    {
        CachePracticalIntensities();
        _blend = nightMode ? 1f : 0f;
        ApplyBlend(_blend);
    }

    private void OnValidate()
    {
        transitionDuration = Mathf.Max(0f, transitionDuration);
        if (!Application.isPlaying)
        {
            CachePracticalIntensities();
            _blend = nightMode ? 1f : 0f;
            ApplyBlend(_blend);
        }
    }

    private void Update()
    {
        if (Application.isPlaying && TogglePressed()) SetNight(!nightMode);
    }

    private bool TogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(toggleKey);
#endif
    }

    public void SetNight(bool enabled)
    {
        nightMode = enabled;
        float target = enabled ? 1f : 0f;

        if (!Application.isPlaying || reduceMotion || transitionDuration <= 0f)
        {
            if (_transition != null) StopCoroutine(_transition);
            _transition = null;
            _blend = target;
            ApplyBlend(_blend);
            return;
        }

        if (_transition != null) StopCoroutine(_transition);
        _transition = StartCoroutine(TransitionTo(target));
    }

    public void RefreshPracticalLights()
    {
        _practicalIntensities = null;
        CachePracticalIntensities();
    }

    private IEnumerator TransitionTo(float target)
    {
        CachePracticalIntensities();
        float start = _blend;
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
            _blend = Mathf.Lerp(start, target, t);
            ApplyBlend(_blend);
            yield return null;
        }

        _blend = target;
        ApplyBlend(_blend);
        _transition = null;
    }

    private void CachePracticalIntensities()
    {
        if (practicalLights == null) practicalLights = new Light[0];
        if (_practicalIntensities != null && _practicalIntensities.Length == practicalLights.Length) return;

        _practicalIntensities = new float[practicalLights.Length];
        for (int i = 0; i < practicalLights.Length; i++)
            _practicalIntensities[i] = practicalLights[i] != null ? Mathf.Max(0.01f, practicalLights[i].intensity) : 0f;
    }

    private void ApplyBlend(float blend)
    {
        CachePracticalIntensities();
        if (sun != null)
        {
            sun.enabled = blend < 0.999f;
            sun.intensity = 1.25f * (1f - blend);
        }

        if (moon != null)
        {
            moon.enabled = blend > 0.001f;
            // Keep architectural silhouettes readable while practical lights
            // provide the warm focal points of the blue-hour scene.
            moon.intensity = 0.62f * blend;
        }

        for (int i = 0; i < practicalLights.Length; i++)
        {
            Light sceneLight = practicalLights[i];
            if (sceneLight == null || sceneLight == sun || sceneLight == moon) continue;
            sceneLight.enabled = blend > 0.001f;
            // In edit mode retain the authored intensity while disabling the
            // light. Otherwise saving the daytime scene serializes intensity 0
            // and there is nothing meaningful to restore when Play begins.
            sceneLight.intensity = Application.isPlaying
                ? _practicalIntensities[i] * blend
                : _practicalIntensities[i];
        }

        Color daySky = new Color(0.64f, 0.72f, 0.82f);
        Color dayEquator = new Color(0.52f, 0.48f, 0.42f);
        Color dayGround = new Color(0.24f, 0.22f, 0.18f);
        RenderSettings.ambientLight = Color.Lerp(dayAmbient, nightAmbient, blend);
        RenderSettings.ambientSkyColor = Color.Lerp(daySky, nightAmbient, blend);
        RenderSettings.ambientEquatorColor = Color.Lerp(dayEquator, nightAmbient * 0.8f, blend);
        RenderSettings.ambientGroundColor = Color.Lerp(dayGround, nightAmbient * 0.55f, blend);
        RenderSettings.fog = blend > 0.01f;
        RenderSettings.fogColor = Color.Lerp(daySky, new Color(0.025f, 0.045f, 0.08f), blend);
        RenderSettings.fogDensity = 0.0045f * blend;
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !showStateIndicator) return;
        if (_indicatorStyle == null)
        {
            _indicatorStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        string stateLabel = nightMode ? "[MOON]  BLUE HOUR NIGHT" : "[SUN]  DAY SOCIAL";
        GUI.Box(new Rect(20, 20, 245, 48), stateLabel, _indicatorStyle);
    }
}
