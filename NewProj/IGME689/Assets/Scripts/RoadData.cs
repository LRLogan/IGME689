using System.Collections.Generic;
using System.Drawing;
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
        // Only create dictionary if not already assigned
        if (averageCount == null)
        {
            averageCount = new Dictionary<int, float>();

            // Assign default hour keys (set to 0)
            for (int i = 1; i <= 24; i++)
                averageCount[i] = 0;
        }

        lines = GetComponentsInChildren<LineRenderer>();
    }

    /// <summary>
    /// Updates congestionValue and applies a color gradient across all child LineRenderers.
    /// </summary>
    /// <param name="newVal">Float between 0 and 1 representing congestion intensity.</param>
    public void UpdateCValAndGrad(float newVal)
    {

        if(newVal != 0)
        {

            // Normalize traffic count (1–500) into a 0–1 range
            float normalizedVal = Mathf.InverseLerp(1f, 500f, newVal);
            congestionValue = Mathf.Clamp01(normalizedVal);

            // Choose color: green (free) -> red (jammed)
            UnityEngine.Color color = UnityEngine.Color.Lerp(UnityEngine.Color.green, UnityEngine.Color.red, congestionValue);


            // Apply to all child LineRenderers
            if (lines == null || lines.Length == 0)
            {
                lines = GetComponentsInChildren<LineRenderer>();
                if (lines.Length == 0) return;
            }

            Debug.Log($"Changing the gradient of {this.gameObject.name} with val of {normalizedVal}");
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
        else
        {
            UnityEngine.Color color = UnityEngine.Color.white;

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
}
