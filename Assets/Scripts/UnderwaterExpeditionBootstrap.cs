using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class UnderwaterExpeditionBootstrap : MonoBehaviour
{
    [Preserve]
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<UnderwaterExpeditionBootstrap>() != null)
        {
            return;
        }

        var bootstrapObject = new GameObject("UnderwaterExpeditionBootstrap");
        DontDestroyOnLoad(bootstrapObject);
        bootstrapObject.AddComponent<UnderwaterExpeditionBootstrap>();
        Debug.Log("UnderwaterExpeditionBootstrap initialized.");
    }

    private void Start()
    {
        EnsureRoad();
        var controller = EnsureCar();
        var manager = EnsureGameManager();
        EnsureCheckpoints(manager);
        EnsureCamera(controller.transform);
        EnsureTouchPad(controller);
    }

    private static ExpeditionGameManager EnsureGameManager()
    {
        var existingManager = FindAnyObjectByType<ExpeditionGameManager>();
        if (existingManager != null)
        {
            return existingManager;
        }

        var managerObject = new GameObject("ExpeditionGameManager");
        return managerObject.AddComponent<ExpeditionGameManager>();
    }

    private static void EnsureCheckpoints(ExpeditionGameManager manager)
    {
        var checkpoints = FindObjectsByType<ExpeditionCheckpoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (checkpoints.Length == 0)
        {
            checkpoints = CreateDefaultCheckpointTrack();
        }

        System.Array.Sort(checkpoints, (a, b) => a.CheckpointIndex.CompareTo(b.CheckpointIndex));
        manager.ConfigureCheckpoints(checkpoints);
    }

    private static ExpeditionCheckpoint[] CreateDefaultCheckpointTrack()
    {
        var positions = new[]
        {
            new Vector3(0f, 0.3f, -44f),
            new Vector3(2.4f, 0.3f, -20f),
            new Vector3(-2.7f, 0.3f, 6f),
            new Vector3(1.8f, 0.3f, 32f),
            new Vector3(0f, 0.3f, 58f)
        };

        var root = new GameObject("ExpeditionCheckpoints");
        var created = new ExpeditionCheckpoint[positions.Length];

        for (var i = 0; i < positions.Length; i++)
        {
            var gate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gate.name = "Checkpoint_" + (i + 1);
            gate.transform.SetParent(root.transform, false);
            gate.transform.position = positions[i];
            gate.transform.localScale = new Vector3(3f, 1f, 3f);

            var checkpoint = gate.AddComponent<ExpeditionCheckpoint>();
            checkpoint.SetCheckpointData(i + 1, 0);

            var collider = gate.GetComponent<Collider>();
            collider.isTrigger = true;

            created[i] = checkpoint;
        }

        return created;
    }

    private static void EnsureRoad()
    {
        if (GameObject.Find("ExpeditionRoad") != null)
        {
            return;
        }

        var road = GameObject.CreatePrimitive(PrimitiveType.Plane);
        road.name = "ExpeditionRoad";
        road.transform.position = new Vector3(0f, -1f, 0f);
        road.transform.localScale = new Vector3(4f, 1f, 12f);

        var roadRenderer = road.GetComponent<Renderer>();
        roadRenderer.material.color = new Color(0.08f, 0.17f, 0.21f);

        for (var i = 0; i < 14; i++)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "RoadMarker_" + i;
            marker.transform.position = new Vector3(0f, -0.92f, -70f + i * 10f);
            marker.transform.localScale = new Vector3(0.5f, 0.04f, 3.2f);
            marker.GetComponent<Renderer>().material.color = new Color(0.72f, 0.91f, 0.94f);
        }
    }

    private static UnderwaterCarController EnsureCar()
    {
        var existingController = FindAnyObjectByType<UnderwaterCarController>();
        if (existingController != null)
        {
            return existingController;
        }

        var carRoot = new GameObject("SubmarineCar");
        carRoot.transform.position = new Vector3(0f, -0.15f, -56f);

        var bodyCollider = carRoot.AddComponent<BoxCollider>();
        bodyCollider.center = new Vector3(0f, 0.45f, 0f);
        bodyCollider.size = new Vector3(1.8f, 0.95f, 3.6f);

        var rb = carRoot.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var controller = carRoot.AddComponent<UnderwaterCarController>();

        CreateVisualBody(carRoot.transform);
        CreateWheels(carRoot.transform);

        return controller;
    }

    private static void CreateVisualBody(Transform root)
    {
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "CarBody";
        body.transform.SetParent(root, false);
        body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        body.transform.localScale = new Vector3(1.7f, 0.75f, 3.2f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().material.color = new Color(0.2f, 0.62f, 0.79f);

        var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "CarCabin";
        cabin.transform.SetParent(root, false);
        cabin.transform.localPosition = new Vector3(0f, 0.9f, -0.2f);
        cabin.transform.localScale = new Vector3(1.3f, 0.55f, 1.4f);
        Destroy(cabin.GetComponent<Collider>());
        cabin.GetComponent<Renderer>().material.color = new Color(0.65f, 0.84f, 0.95f);

        var lightBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lightBar.name = "HeadLightBar";
        lightBar.transform.SetParent(root, false);
        lightBar.transform.localPosition = new Vector3(0f, 0.48f, 1.7f);
        lightBar.transform.localScale = new Vector3(1.1f, 0.14f, 0.1f);
        Destroy(lightBar.GetComponent<Collider>());
        lightBar.GetComponent<Renderer>().material.color = new Color(0.9f, 0.96f, 1f);
    }

    private static void CreateWheels(Transform root)
    {
        var wheelOffsets = new[]
        {
            new Vector3(-0.8f, 0.18f, 1.2f),
            new Vector3(0.8f, 0.18f, 1.2f),
            new Vector3(-0.8f, 0.18f, -1.2f),
            new Vector3(0.8f, 0.18f, -1.2f)
        };

        for (var i = 0; i < wheelOffsets.Length; i++)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wheel.name = "Wheel_" + i;
            wheel.transform.SetParent(root, false);
            wheel.transform.localPosition = wheelOffsets[i];
            wheel.transform.localScale = new Vector3(0.48f, 0.48f, 0.48f);
            Destroy(wheel.GetComponent<Collider>());
            wheel.GetComponent<Renderer>().material.color = new Color(0.12f, 0.14f, 0.17f);
        }
    }

    private static void EnsureCamera(Transform target)
    {
        var cam = Camera.main;

        if (cam == null)
        {
            var camObject = new GameObject("Main Camera");
            cam = camObject.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        var follow = cam.GetComponent<UnderwaterFollowCamera>();
        if (follow == null)
        {
            follow = cam.gameObject.AddComponent<UnderwaterFollowCamera>();
        }

        follow.target = target;
    }

    private static void EnsureTouchPad(UnderwaterCarController controller)
    {
        var touchPad = FindAnyObjectByType<UnderwaterTouchDrivePad>();
        if (touchPad == null)
        {
            var touchPadObject = new GameObject("UnderwaterTouchDrivePad");
            touchPad = touchPadObject.AddComponent<UnderwaterTouchDrivePad>();
        }

        touchPad.targetController = controller;
    }
}
