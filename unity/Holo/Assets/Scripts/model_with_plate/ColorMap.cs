using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Configure colormap used by DataVisualizationMaterial. */
public class ColorMap : MonoBehaviour, IClickHandler
{
    public Material DataVisualizationMaterial;

    void Start()
    {
        // Load Player Settings
        ClickSetColorMap(PlayerPrefs.GetString("ColorMap", "jet")); // color map        
    }

    public void Click(GameObject clickObject)
    {
        switch (clickObject.name)
        {
            case "ButtonColorMapJet": ClickSetColorMap("jet"); break;
            case "ButtonColorMapViridis": ClickSetColorMap("viridis"); break;
            case "ButtonColorMapMagma": ClickSetColorMap("magma"); break;
            case "ButtonColorMapCividis": ClickSetColorMap("cividis"); break;
            case "ButtonColorMapPlasma": ClickSetColorMap("plasma"); break;
            case "ButtonColorMapInferno": ClickSetColorMap("inferno"); break;
        }
    }

    public void FocusEnter(GameObject focusEnterObject)
    {
        // does nothing, implemented only to satisfy IClickHandler interface
    }

    public void FocusExit(GameObject focusExitObject)
    {
        // does nothing, implemented only to satisfy IClickHandler interface
    }

    private void ClickSetColorMap(string colorMapName)
    {
        Texture2D colorMap;
        colorMap = Resources.Load<Texture2D>("Colormaps/" + colorMapName);
        DataVisualizationMaterial.SetTexture("_ColorMap", colorMap);
        PlayerPrefs.SetString("ColorMap", colorMapName);
        PlayerPrefs.Save();
    }
}
