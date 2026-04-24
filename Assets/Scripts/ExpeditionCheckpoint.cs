using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ExpeditionCheckpoint : MonoBehaviour
{
    public int checkpointIndex = 1;
    public int scoreValue = 150;

    private ExpeditionGameManager manager;
    private Renderer cachedRenderer;
    private bool completed;

    public int CheckpointIndex => checkpointIndex;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();

        var triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    public void AssignManager(ExpeditionGameManager checkpointManager)
    {
        manager = checkpointManager;
    }

    public void SetCheckpointData(int index, int points)
    {
        checkpointIndex = Mathf.Max(1, index);
        scoreValue = Mathf.Max(0, points);
    }

    public void SetAsNextTarget(bool active)
    {
        if (completed)
        {
            return;
        }

        SetColor(active ? new Color(0.35f, 0.92f, 1f) : new Color(0.2f, 0.45f, 0.58f));
    }

    public void MarkCompleted()
    {
        completed = true;
        SetColor(new Color(0.26f, 0.98f, 0.48f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (completed || manager == null)
        {
            return;
        }

        var controller = other.GetComponentInParent<UnderwaterCarController>();
        if (controller == null)
        {
            return;
        }

        var accepted = manager.TryCompleteCheckpoint(this, scoreValue);
        if (!accepted)
        {
            return;
        }

        MarkCompleted();
    }

    private void SetColor(Color targetColor)
    {
        if (cachedRenderer == null)
        {
            return;
        }

        cachedRenderer.material.color = targetColor;
    }
}
