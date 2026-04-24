using System;
using UnityEngine;

public class ExpeditionGameManager : MonoBehaviour
{
    [Header("Scoring")]
    public float targetCompletionTime = 110f;
    public int scorePerCheckpoint = 150;
    public int finishBonus = 500;
    public int timeBonusPerSecond = 8;

    private ExpeditionCheckpoint[] checkpoints = Array.Empty<ExpeditionCheckpoint>();
    private int nextCheckpointArrayIndex;
    private int score;
    private bool runCompleted;
    private float startTime;
    private float completionTime;
    private int timeBonusAwarded;
    private string statusText = "Reach checkpoint 1";

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;

    public void ConfigureCheckpoints(ExpeditionCheckpoint[] sequence)
    {
        checkpoints = sequence ?? Array.Empty<ExpeditionCheckpoint>();
        nextCheckpointArrayIndex = 0;
        score = 0;
        runCompleted = false;
        completionTime = 0f;
        timeBonusAwarded = 0;
        statusText = checkpoints.Length > 0 ? "Reach checkpoint 1" : "No checkpoints configured";
        startTime = Time.time;

        for (var i = 0; i < checkpoints.Length; i++)
        {
            var points = i == checkpoints.Length - 1
                ? scorePerCheckpoint + Mathf.RoundToInt(scorePerCheckpoint * 0.5f)
                : scorePerCheckpoint;

            checkpoints[i].AssignManager(this);
            checkpoints[i].SetCheckpointData(i + 1, points);
            checkpoints[i].SetAsNextTarget(i == 0);
        }
    }

    public bool TryCompleteCheckpoint(ExpeditionCheckpoint checkpoint, int points)
    {
        if (runCompleted || checkpoints.Length == 0)
        {
            return false;
        }

        if (nextCheckpointArrayIndex >= checkpoints.Length)
        {
            return false;
        }

        var expectedCheckpoint = checkpoints[nextCheckpointArrayIndex];
        if (checkpoint != expectedCheckpoint)
        {
            statusText = "Wrong checkpoint order";
            return false;
        }

        score += points;
        nextCheckpointArrayIndex++;

        if (nextCheckpointArrayIndex < checkpoints.Length)
        {
            checkpoints[nextCheckpointArrayIndex].SetAsNextTarget(true);
            statusText = "Checkpoint " + nextCheckpointArrayIndex + "/" + checkpoints.Length + " cleared";
            return true;
        }

        CompleteRun();
        return true;
    }

    private void CompleteRun()
    {
        runCompleted = true;
        completionTime = Time.time - startTime;

        var timeDelta = targetCompletionTime - completionTime;
        timeBonusAwarded = Mathf.Max(0, Mathf.RoundToInt(timeDelta * timeBonusPerSecond));

        score += finishBonus;
        score += timeBonusAwarded;

        statusText = "Mission complete";
    }

    private void OnGUI()
    {
        EnsureStyles();

        var elapsed = runCompleted ? completionTime : Time.time - startTime;
        var nextCheckpointText = runCompleted
            ? "All checkpoints done"
            : "Checkpoint " + (nextCheckpointArrayIndex + 1) + "/" + Mathf.Max(1, checkpoints.Length);

        GUI.Box(new Rect(14f, 14f, 340f, 146f), string.Empty, panelStyle);
        GUI.Label(new Rect(28f, 22f, 300f, 30f), "ABYSS EXPEDITION", titleStyle);
        GUI.Label(new Rect(28f, 54f, 300f, 24f), nextCheckpointText, bodyStyle);
        GUI.Label(new Rect(28f, 78f, 300f, 24f), "Time: " + elapsed.ToString("0.0") + "s", bodyStyle);
        GUI.Label(new Rect(28f, 102f, 300f, 24f), "Score: " + score, bodyStyle);
        GUI.Label(new Rect(28f, 126f, 310f, 24f), statusText, bodyStyle);

        if (!runCompleted)
        {
            return;
        }

        var panelWidth = 400f;
        var panelHeight = 190f;
        var panelX = (Screen.width - panelWidth) * 0.5f;
        var panelY = (Screen.height - panelHeight) * 0.5f;

        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), string.Empty, panelStyle);
        GUI.Label(new Rect(panelX + 20f, panelY + 16f, panelWidth - 40f, 30f), "MISSION COMPLETE", titleStyle);
        GUI.Label(new Rect(panelX + 20f, panelY + 56f, panelWidth - 40f, 26f), "Final time: " + completionTime.ToString("0.0") + "s", bodyStyle);
        GUI.Label(new Rect(panelX + 20f, panelY + 84f, panelWidth - 40f, 26f), "Finish bonus: +" + finishBonus, bodyStyle);
        GUI.Label(new Rect(panelX + 20f, panelY + 112f, panelWidth - 40f, 26f), "Time bonus: +" + timeBonusAwarded, bodyStyle);
        GUI.Label(new Rect(panelX + 20f, panelY + 140f, panelWidth - 40f, 30f), "Total score: " + score, titleStyle);
    }

    private void EnsureStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { textColor = Color.white }
        };

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.84f, 0.97f, 1f, 1f) }
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.76f, 0.94f, 1f, 0.95f) }
        };
    }
}
