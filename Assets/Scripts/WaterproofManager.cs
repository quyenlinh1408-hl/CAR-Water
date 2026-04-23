using UnityEngine;

public class WaterproofManager : MonoBehaviour
{
    public bool isInsideCar = true;
    public KeyCode toggleKey = KeyCode.F;
    [Range(0f, 1f)] public float outsideTintAlpha = 0.16f;
    public Color outsideTintColor = new Color(0.08f, 0.35f, 0.55f, 1f);
    public Color outsideFogColor = new Color(0f, 0.2f, 0.4f, 1f);
    public float outsideFogDensity = 0.055f;

    private bool? previousInsideCar;
    private Camera cachedCamera;
    private CameraClearFlags cachedClearFlags;
    private Color cachedBackgroundColor;
    private bool cameraSettingsCached;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<WaterproofManager>() != null)
        {
            return;
        }

        var managerObject = new GameObject("WaterproofManager");
        DontDestroyOnLoad(managerObject);
        managerObject.AddComponent<WaterproofManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isInsideCar = !isInsideCar;
            Debug.Log($"Mode switched: {(isInsideCar ? "Inside car" : "Outside underwater")}");
        }

        if (previousInsideCar != isInsideCar)
        {
            ApplyEnvironment();
            previousInsideCar = isInsideCar;
        }
    }

    private void OnEnable()
    {
        Debug.Log("WaterproofManager is running. Press F to switch cabin/outside mode.");
        CacheCameraSettings();
        ApplyEnvironment();
        previousInsideCar = isInsideCar;
    }

    private void CacheCameraSettings()
    {
        if (cameraSettingsCached)
        {
            return;
        }

        cachedCamera = Camera.main;
        if (cachedCamera == null)
        {
            return;
        }

        cachedClearFlags = cachedCamera.clearFlags;
        cachedBackgroundColor = cachedCamera.backgroundColor;
        cameraSettingsCached = true;
    }

    private void ApplyEnvironment()
    {
        CacheCameraSettings();

        if (isInsideCar)
        {
            RenderSettings.fog = false;
            RenderSettings.fogDensity = 0f;

            if (cachedCamera != null)
            {
                cachedCamera.clearFlags = cachedClearFlags;
                cachedCamera.backgroundColor = cachedBackgroundColor;
            }

            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = outsideFogDensity;
        RenderSettings.fogColor = outsideFogColor;

        if (cachedCamera != null)
        {
            cachedCamera.clearFlags = CameraClearFlags.SolidColor;
            cachedCamera.backgroundColor = outsideFogColor;
        }
    }

    private void OnGUI()
    {
        if (!isInsideCar)
        {
            var fullScreen = new Rect(0f, 0f, Screen.width, Screen.height);
            GUI.color = new Color(outsideTintColor.r, outsideTintColor.g, outsideTintColor.b, outsideTintAlpha);
            GUI.DrawTexture(fullScreen, Texture2D.whiteTexture);

            GUI.color = new Color(1f, 1f, 1f, outsideTintAlpha * 0.35f);
            GUI.DrawTexture(new Rect(-12f, -12f, Screen.width + 24f, Screen.height + 24f), Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(12f, 12f, 480f, 30f),
            $"WaterproofManager: {(isInsideCar ? "Inside car" : "Outside underwater")} (press {toggleKey})");
    }
}
