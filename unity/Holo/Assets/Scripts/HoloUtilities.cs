using System;

using UnityEngine;

using HoloToolkit.Unity;
using HoloToolkit.Unity.UX;
using HoloToolkit.Unity.Buttons;

public static class HoloUtilities
{
    private static Color ButtonActiveColor = new Color(0f, 0.90f, 0.88f);
    private static Color ButtonInactiveColor = new Color(1f, 1f, 1f);

    public static void SetButtonState(CompoundButton button, bool active)
    {
        CompoundButtonIcon icon = button.GetComponent<CompoundButtonIcon>();
        if (icon == null)
        {
            Debug.LogWarning("Missing CompoundButtonIcon on " + button.name);
            return;
        }
        MeshRenderer iconRenderer = icon.IconMeshFilter.GetComponent<MeshRenderer>();
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

    public static void SetButtonStateText(CompoundButton button, bool active)
    {
        CompoundButtonText text = button.GetComponent<CompoundButtonText>();
        if (text == null)
        {
            Debug.LogWarning("Missing CompoundButtonText");
            return;
        }
        TextMesh textMesh = text.TextMesh;
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
}
