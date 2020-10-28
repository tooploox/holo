using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

public static class HoloUtilities
{
    private static Color ButtonActiveColor = new Color(0f, 0.90f, 0.88f);
    private static Color ButtonInactiveColor = new Color(1f, 1f, 1f);

    public static void SetButtonState(PressableButtonHoloLens2 button, bool active)
    {
        var icon = button.transform.Find("IconAndText/UIButtonSquareIcon").gameObject;
        if (icon == null)
        {
            Debug.LogWarning("Missing UIButtonSquareIcon on " + button.name);
            return;
        }
        MeshRenderer iconRenderer = icon.GetComponent<MeshRenderer>();
        if (iconRenderer == null)
        {
            Debug.LogWarning("Missing MeshRenderer on CompoundButtonIcon.IconMeshFilter attached to " + button.name);
            return;
        }
        // using material, not sharedMaterial, deliberately: we only change color of this material instance
        Color newColor = active ? ButtonActiveColor : ButtonInactiveColor;
        //Debug.Log("changing color of " + button.name + " to " + newColor.ToString());
        // both _EmissiveColor and _Color (Albedo in editor) should be set to make proper effect.
        iconRenderer.material.SetColor("_EmissiveColor", newColor);
        iconRenderer.material.SetColor("_Color", newColor);
    }

    public static void SetButtonStateText(PressableButtonHoloLens2 button, bool active)
    {
        var text = button.transform.Find("IconAndText/Text").gameObject;
        if (text == null)
        {
            Debug.LogWarning("Missing Text GameObject");
            return;
        }
        TextMeshPro textMesh = text.GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogWarning("Missing TextMesh");
            return;
        }
        // using material, not sharedMaterial, deliberately: we only change color of this material instance
        Color newColor = active ? ButtonActiveColor : ButtonInactiveColor;
        //Debug.Log("changing color of " + button.name + " to " + newColor.ToString());
        // both _EmissiveColor and _Color (Albedo in editor) should be set to make proper effect.
        MeshRenderer renderer = textMesh.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_EmissiveColor", newColor);
        renderer.material.SetColor("_Color", newColor);
    }

    public static string SuffixRemove(string suffix, string s)
    {
        if (s.EndsWith(suffix)) {
            return s.Substring(0, s.Length - suffix.Length);
        } else {
            return s;
        }
    }
}
