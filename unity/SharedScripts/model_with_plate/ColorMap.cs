using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.UX;
using HoloToolkit.Unity.Buttons;

/* Configure colormap used by DataVisualizationMaterial. */
public class ColorMap : MonoBehaviour, IClickHandler
{
    public Material DataVisualizationMaterial;

    // Map from button instance to the corresponding colormap name (texture name)
    private Dictionary<GameObject, string> colorMapButtons;

    private GameObject FindButton(string name)
    {
        foreach (GameObject interactable in GetComponent<ButtonsClickReceiver>().interactables) {
            if (interactable.name == name) {
                CompoundButton button = interactable.GetComponent<CompoundButton>();
                if (button == null) {
                    throw new Exception("Interactable named " + name + " found, but it is not a button");
                }
                return interactable;
            }
        }
        throw new Exception("Interactable " + name + " not found");
    }

    void Start()
    {
        colorMapButtons = new Dictionary<GameObject, string>()
        {
            { FindButton("ButtonColorMapJet"), "jet" },
            { FindButton("ButtonColorMapViridis"), "viridis" },
            { FindButton("ButtonColorMapMagma"), "magma" },
            { FindButton("ButtonColorMapCividis"), "cividis" },
            { FindButton("ButtonColorMapPlasma"), "plasma" },
            { FindButton("ButtonColorMapInferno"), "inferno" }
        };

        // Load Player Settings
        string initialColorMap = PlayerPrefs.GetString("ColorMap", "jet");
        GameObject initialColorMapButton = ButtonFromColorMapName(initialColorMap);
        ClickSetColorMap(initialColorMap, initialColorMapButton);
    }

    /* Reverse colorMapButtons map. */
    private GameObject ButtonFromColorMapName(string colorMapName)
    {
        foreach (var colorMapInfo in colorMapButtons) {
            if (colorMapInfo.Value == colorMapName) {
                return colorMapInfo.Key;
            }
        }
        throw new Exception("No colormap named " + colorMapName);
    }

    public void Click(GameObject clickObject)
    {
        string colorMapName;
        if (colorMapButtons.TryGetValue(clickObject, out colorMapName)) {
            ClickSetColorMap(colorMapName, clickObject);
        }
    }

    private void ClickSetColorMap(string colorMapName, GameObject currentButton)
    {
        MapName = colorMapName;
        foreach (GameObject button in colorMapButtons.Keys) {
            HoloUtilities.SetButtonStateText(button.GetComponent<CompoundButton>(), currentButton == button);
        }
        PlayerPrefs.SetString("ColorMap", colorMapName);
        PlayerPrefs.Save();
    }

    private string mapName;
    public string MapName
    {
        get
        {
            return mapName;
        }
        set
        {
            if (mapName != value) { 
                mapName = value;
                Texture2D colorMapTexture = Resources.Load<Texture2D>("Colormaps/" + value);
                DataVisualizationMaterial.SetTexture("_ColorMap", colorMapTexture);
            }
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
}
