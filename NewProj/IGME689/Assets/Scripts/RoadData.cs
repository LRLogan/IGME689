using System.Collections.Generic;
using UnityEngine;

public class RoadData : MonoBehaviour
{
    [Range(0f, 1f)]
    public float congestionValue; // 0 = green (free), 1 = red (jammed)
    public string roadName;
    public Dictionary<int, float> averageCount;
    public LineRenderer[] lines;

    private void Start()
    {
        averageCount = new Dictionary<int, float>();
        lines = GetComponentsInChildren<LineRenderer>();

        // Assign default hour keys
        for (int i = 1; i < 25; i++)
            averageCount[i] = 0;
    }

    /// <summary>
    /// Updates congestionValue and applies a color gradient across all child LineRenderers.
    /// </summary>
    /// <param name="newVal">Float between 0 and 1 representing congestion intensity.</param>
    public void UpdateCValAndGrad(float newVal)
    {
        congestionValue = Mathf.Clamp01(newVal);

        // Map congestion value (0 = green, 1 = red)
        Color color = Color.Lerp(Color.green, Color.red, congestionValue);

        // Apply to all child LineRenderers
        if (lines == null || lines.Length == 0)
        {
            lines = GetComponentsInChildren<LineRenderer>();
            if (lines.Length == 0) return;
        }

        Debug.Log($"Changing the gradient of {this.gameObject.name}");
        foreach (var lr in lines)
        {
            if (lr == null) continue;

            // Use a gradient so the line looks smooth
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );

            lr.colorGradient = gradient;
        }
    }
}
