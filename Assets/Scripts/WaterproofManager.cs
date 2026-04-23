using UnityEngine;

public class WaterproofManager : MonoBehaviour
{
    public bool isInsideCar = true;
    public KeyCode toggleKey = KeyCode.F;

    private bool? previousInsideCar;

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
        ApplyEnvironment();
        previousInsideCar = isInsideCar;
    }

    private void ApplyEnvironment()
    {
        if (isInsideCar)
        {
            RenderSettings.fog = false;
            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.03f;
        RenderSettings.fogColor = new Color(0f, 0.2f, 0.4f);
    }

    private void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(12f, 12f, 480f, 30f),
            $"WaterproofManager: {(isInsideCar ? "Inside car" : "Outside underwater")} (press {toggleKey})");
    }
}
