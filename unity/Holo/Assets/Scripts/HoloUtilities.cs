using UnityEngine;

using UnityEngine.UI;

public static class HoloUtilities
{
    private static Color ButtonActiveColor = new Color(0f, 0.90f, 0.88f);
    private static Color ButtonInactiveColor = new Color(1f, 1f, 1f);
   
    public static void SetButtonState(Button button, bool active)
    {    
        Image image = button.GetComponent<Image>();
        if (image == null)
        {
            Debug.LogWarning("Missing Image on " + button.name);
            return;
        }
        Color newColor = active ? ButtonActiveColor : ButtonInactiveColor;
        image.color = newColor;
    }

    public static void SetButtonStateText(Button button, bool active)
    {
        Text text = button.GetComponentInChildren<Text>();
        if (text == null)
        {
            Debug.LogWarning("Missing Text");
            return;
        }
        Color newColor = active ? ButtonActiveColor : ButtonInactiveColor;
        text.color = newColor;
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
