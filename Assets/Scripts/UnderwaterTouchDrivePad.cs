using UnityEngine;

public class UnderwaterTouchDrivePad : MonoBehaviour
{
    public UnderwaterCarController targetController;
    public bool showOnDesktop = true;
    [Range(0f, 0.4f)] public float touchDeadZone = 0.15f;

    private GUIStyle buttonStyle;
    private GUIStyle labelStyle;

    private void Awake()
    {
        if (targetController == null)
        {
            targetController = FindAnyObjectByType<UnderwaterCarController>();
        }

        CreateStyles();
    }

    private void OnGUI()
    {
        if (targetController == null)
        {
            return;
        }

        if (!showOnDesktop && !Application.isMobilePlatform)
        {
            return;
        }

        var size = Mathf.Min(Screen.width, Screen.height) * 0.14f;
        var margin = 22f;

        var leftX = margin;
        var rightX = Screen.width - size - margin;
        var bottomY = Screen.height - size - margin;
        var midY = bottomY - size - 12f;

        var leftButton = new Rect(leftX, bottomY, size, size);
        var rightButton = new Rect(leftX + size + 12f, bottomY, size, size);
        var forwardButton = new Rect(rightX, midY, size, size);
        var reverseButton = new Rect(rightX, bottomY, size, size);

        var steer = 0f;
        var throttle = 0f;

        if (GUI.RepeatButton(leftButton, "LEFT", buttonStyle))
        {
            steer -= 1f;
        }

        if (GUI.RepeatButton(rightButton, "RIGHT", buttonStyle))
        {
            steer += 1f;
        }

        if (GUI.RepeatButton(forwardButton, "GO", buttonStyle))
        {
            throttle += 1f;
        }

        if (GUI.RepeatButton(reverseButton, "BACK", buttonStyle))
        {
            throttle -= 1f;
        }

        steer = ApplyDeadZone(steer);
        throttle = ApplyDeadZone(throttle);

        GUI.Label(new Rect(margin, Screen.height - size * 2f - 64f, 260f, 28f), "Touch Drive Pad", labelStyle);

        if (Mathf.Abs(throttle) > 0.01f || Mathf.Abs(steer) > 0.01f)
        {
            targetController.SetTouchInput(throttle, steer);
            return;
        }

        targetController.ClearTouchInput();
    }

    private float ApplyDeadZone(float value)
    {
        return Mathf.Abs(value) < touchDeadZone ? 0f : value;
    }

    private void CreateStyles()
    {
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.85f, 0.97f, 1f, 0.9f) }
        };
    }
}
