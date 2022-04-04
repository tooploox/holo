using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;


/* Configure colormap used by DataVisualizationMaterial and DataTurbulenceMaterial. */
public class ColorMap : MonoBehaviour, IClickHandler
{
    public Material DataVisualizationMaterial;
    public Material DataTurbulenceMaterial;
    public Material DataDisplacementMaterial;
    public Material DataDisplacementTransparentMaterial;
    public List<GameObject> ColorButtons;

    // must be set if there's a model currently loaded
    public LayersLoaded LayersLoaded;

    // Map from button instance to the corresponding colormap name (texture name)
    private Dictionary<GameObject, string> colorMapButtons;

    private GameObject FindButton(string name)
    {
        foreach (GameObject buttonGameObject in ColorButtons) {
            if (buttonGameObject.name == name) {
                PressableButtonHoloLens2 button = buttonGameObject.GetComponent<PressableButtonHoloLens2>();
                if (button == null) {
                    throw new Exception("Colormap named " + name + " found, but it is not a button");
                }
                return buttonGameObject;
            }
        }
        throw new Exception("Colormap " + name + " not found");
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
            { FindButton("ButtonColorMapCoolwarm"), "coolwarm" }
        };

        // Load Player Settings
        string initialColorMap = PlayerPrefs.GetString("ColorMap", "coolwarm");
        //string initialColorMap = "coolwarm";
        Debug.Log(initialColorMap);
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
            HoloUtilities.SetButtonStateText(button.GetComponent<PressableButtonHoloLens2>(), currentButton == button);
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
                DataTurbulenceMaterial.SetTexture("_ColorMap", colorMapTexture);
                DataDisplacementMaterial.SetTexture("_ColorMap", colorMapTexture);
                DataDisplacementTransparentMaterial.SetTexture("_ColorMap", colorMapTexture);

                /* Change also current instances of materials, because
                   MaterialInstance.cs from Mrtk (required by ClippingPlane of Mrtk)
                   changes shared materials into non-shared. */
                if (LayersLoaded != null) {
                    foreach (var layerPair in LayersLoaded)
                    {
                        if (layerPair.Key.Simulation) {
                            foreach (var renderer in layerPair.Value.Instance.GetComponentsInChildren<Renderer>()) {
                                renderer.material.SetTexture("_ColorMap", colorMapTexture);
                            }
                        }
                        else if (layerPair.Key.Turbulence)
                        {
                            foreach (var renderer in layerPair.Value.Instance.GetComponentsInChildren<Renderer>())
                            {
                                renderer.material.SetTexture("_ColorMap", colorMapTexture);
                            }
                        }
                        else if (layerPair.Key.Displacement)
                        {
                            foreach (var renderer in layerPair.Value.Instance.GetComponentsInChildren<Renderer>())
                            {
                                renderer.material.SetTexture("_ColorMap", colorMapTexture);
                            }
                        }
                    }
                }
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
