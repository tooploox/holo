using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class ModelWithPlate : MonoBehaviour, IClickHandler
{
    /* Public fields that should be set in Unity Editor */
    public GameObject SliderAnimationSpeed;
    public GameObject ButtonsModel;
    public GameObject ButtonsModelPreview;
    public GameObject LayerSubmenu;
    public GameObject PlateAnimated;

    public Material DefaultModelMaterial;
    public Material DefaultModelTransparentMaterial;
    public Material DataVisualizationMaterial;
    public Material DefaultVolumetricMaterial;
    public Transform InstanceParent;
    public PressableButtonHoloLens2 ButtonTogglePlay;
    public PressableButtonHoloLens2 ButtonLayerSubmenu;
    public PressableButtonHoloLens2 ButtonTransform;
    public PressableButtonHoloLens2 ButtonTranslate;
    public PressableButtonHoloLens2 ButtonRotate;
    public PressableButtonHoloLens2 ButtonScale;
    public PressableButtonHoloLens2 ButtonAnimationSubmenu;
    public PressableButtonHoloLens2 ButtonTransparency;
    public PressableButtonHoloLens2 ButtonPlateRotation;
    public GameObject PlateButtonCollection;
    public GameObject ButtonLayerTemplate;
    public Texture2D ButtonIconPlay;
    public Texture2D ButtonIconPause;
    // Drop here "Prefabs/ModelWithPlateRotationRig"
    public GameObject RotationBoxRigTemplate;
    public GameObject AddButtonsCollection;
    public ColorMap ColorMap;

    private float SliderSpeedFactor = 5.0f;

    public enum TransformationState
    {
        None,
        Translate,
        Rotate,
        Scale
    }
    private TransformationState transformationState = TransformationState.None;

    public GameObject ModelClipPlane;

    private ModelClippingPlaneControl ModelClipPlaneCtrl;

    public bool InstanceLoaded { get; private set; } = false;
    /* All the variables below are non-null
     * only when instanceLoaded,
     * that is only after LoadInstance call (and before UnloadInstance). */
    private AssetBundleLoader instanceBundle;
    private LayersLoaded layersLoaded;
    private Dictionary<ModelLayer, PressableButtonHoloLens2> layersButtons;
    private GameObject instanceTransformation;
    private bool instanceIsPreview = false;

    // Created only when instance != null, as it initializes bbox in Start and assumes it's not empty
    private GameObject rotationBoxRig;

    /* Currently loaded bundle name, null if none. */
    public string InstanceName
    {
        get
        {
            return instanceBundle != null ? instanceBundle.Name : null;
        }
    }

    public Vector3 ModelPosition
    {
        get
        {
            return rotationBoxRig != null ? rotationBoxRig.transform.localPosition : Vector3.one;
        }
        set
        {
            if (rotationBoxRig != null)
            {
                rotationBoxRig.transform.localPosition = value;
            }
            // TODO: otherwise ignore, we do not synchronize position for unloaded models now
        }
    }

    public Quaternion ModelRotation
    {
        get {
            return rotationBoxRig != null ? rotationBoxRig.transform.localRotation : Quaternion.identity;
        }
        set {
            if (rotationBoxRig != null) {
                rotationBoxRig.transform.localRotation = value;
            }
            // TODO: otherwise ignore, we do not synchronize rotation for unloaded models now
        }
    }

    public Vector3 ModelScale
    {
        get
        {
            return rotationBoxRig != null ? rotationBoxRig.transform.localScale : Vector3.one;
        }
        set
        {
            if (rotationBoxRig != null)
            {
                rotationBoxRig.transform.localScale = value;
            }
            // TODO: otherwise ignore, we do not synchronize scale for unloaded models now
        }
    }

    /* Currently visible layers, expressed as a bitmask. */
    public uint InstanceLayers
    {
        get
        {
            uint result = 0;
            if (layersLoaded != null) {
                foreach (ModelLayer layer in layersLoaded.Keys) {
                    result |= ((uint)1 << layer.LayerIndex);
                }
            }
            return result;
        }
        set
        {
            uint currentLayers = InstanceLayers;
            foreach (ModelLayer layer in instanceBundle.Layers) {
                uint layerMask = (uint)1 << layer.LayerIndex;
                if ((layerMask & value) != 0 && (layerMask & currentLayers) == 0) {
                    LoadLayer(layer);
                } else
                if ((layerMask & value) == 0 && (layerMask & currentLayers) != 0) {
                    UnloadLayer(layer);
                }
            }
        }
    }

    private void Start()
    {
        GetComponent<ObjectManipulator>().enabled = false;

        ModelClipPlaneCtrl = ModelClipPlane.GetComponentInChildren<ModelClippingPlaneControl>();
        // Turn off the clipping plane on start
        DefaultModelMaterial.DisableKeyword("CLIPPING_ON");
        DefaultModelTransparentMaterial.DisableKeyword("CLIPPING_ON");
        DataVisualizationMaterial.DisableKeyword("CLIPPING_ON");

        LayerSubmenuState = false;
        AnimationSpeedSubmenu = false;

        RefreshUserInterface();
        InitializeAddButtons();

        // This sets proper state of buttons and components like handDraggable
        ChangeTransformationState(TransformationState.None);

        SliderAnimationSpeed.GetComponent<PinchSlider>().OnValueUpdated.AddListener(
            delegate
            {
                AnimationSpeed = SliderAnimationSpeed.GetComponent<PinchSlider>().SliderValue * SliderSpeedFactor;
                Debug.Log(SliderAnimationSpeed.GetComponent<PinchSlider>().SliderValue.ToString());
                SliderAnimationSpeed.transform.Find("ThumbRoot/SpeedValue").GetComponent<TextMeshPro>().text = Math.Round(AnimationSpeed, 2).ToString();
            }
        );
    }

    /* Number of "add" buttons we have in the scene. */
    private const int addButtonsCount = 15;

    /* Find the GameObject of some "AddXxx" button. */
    private GameObject FindAddButton(int i)
    {
        // TODO: Search for the real name
        return AddButtonsCollection.transform.Find("Add" + i.ToString()).gameObject;
    }

    /* Initialize "AddXxx" buttons captions and existence. */
    private void InitializeAddButtons()
    {
        if (ModelsCollection.Singleton == null)
        {
            Debug.LogError("ModelsCollection script must be executed before ModelWithPlate. Fix Unity \"Script Execution Order\".");
            return;
        }

        int activeButtonsCount = Mathf.Min(addButtonsCount, ModelsCollection.Singleton.BundlesCount);

        // set add buttons captions and existence, for the buttons that correspond to some bundles
        for (int i = 0; i < activeButtonsCount; i++)
        {
            GameObject button = FindAddButton(i);
            button.SetActive(true);

            string modelName = ModelsCollection.Singleton.BundleCaption(i);
            button.transform.Find("IconAndText/Text").GetComponent<TextMeshPro>().text = modelName;

            Texture2D icon = ModelsCollection.Singleton.BundleIcon(i);
            if (icon != null) {
                button.transform.Find("IconAndText/UIButtonSquareIcon").GetComponent<MeshRenderer>().material.SetTexture("_MainTex", icon);
                button.transform.Find("IconAndText/UIButtonSquareIcon").GetComponent<MeshRenderer>().enabled = true;
            }
        }

        // hide the rest of the buttons, when there are less models than buttons
        for (int i = activeButtonsCount; i < addButtonsCount; i++)
        {
            GameObject button = FindAddButton(i);
            button.SetActive(false);
        }
    }

    /* Handle a click on some button inside. Called by ButtonsClickReceiver. */
    public void Click(GameObject clickObject)
    {
        if (SharingSceneData.Singleton.isClient && !SharingSceneData.Singleton.isServer) 
        {
              return;
        }
        Debug.Log("ModelWithPlate Click: " + clickObject.name);

        switch (clickObject.name)
        {
            case "TogglePlay": ClickTogglePlay(); break;
            case "Rewind": ClickRewind(); break;
            case "Remove": ClickRemove(); break;
            case "Speed": ClickSpeed(); break;
            case "ConfirmPreview": ClickConfirmPreview(); break;
            case "CancelPreview": ClickCancelPreview(); break;
            case "ButtonLayers": ClickToggleLayersState(clickObject.GetComponent<PressableButtonHoloLens2>()); break;
            case "ButtonTransform": ClickTransform(); break;
            case "ButtonTranslate": ChangeTransformationState(TransformationState.Translate); break;
            case "ButtonRotate": ChangeTransformationState(TransformationState.Rotate); break;
            case "ButtonScale": ChangeTransformationState(TransformationState.Scale); break;
            case "ButtonClipping": ClickClipping(); break;
            case "ButtonAnimationSpeed": ClickAnimationSpeed(); break;
            case "ButtonTransparency": ClickTransparency(); break;
            case "ButtonPlateRotation": ClickPlateRotation(); break;


            default:
                {
                    ModelLayer layer = ButtonOfLayer(clickObject);
                    if (layer != null)
                    {
                        ClickChangeLayerState(layer);
                    } else
                    {
                        const string addPrefix = "Add";
                        int addInstanceIndex;
                        if (clickObject.name.StartsWith(addPrefix) && !PlateRotation &&
                            int.TryParse(clickObject.name.Substring(addPrefix.Length), out addInstanceIndex))
                        {
                            ClickAdd(ModelsCollection.Singleton.BundleName(addInstanceIndex));
                            CloseSubmenus();
                        }
                    }
                    break;
                }
        }
    }

    private bool StoreManipulatableModelActiveState, StoreHandDraggableClipPlaneActiveState;
    public void FocusEnter(GameObject focusObject)
    {
        if (focusObject.name == "Slider")
        {
            StoreManipulatableModelActiveState = GetComponent<ObjectManipulator>().enabled;
            StoreHandDraggableClipPlaneActiveState = ModelClipPlane.GetComponent<ObjectManipulator>().enabled;
            GetComponent<ObjectManipulator>().enabled = false;
            ModelClipPlane.GetComponent<ObjectManipulator>().enabled = false;
        }
    }

    public void FocusExit(GameObject focusObject)
    {
        if (focusObject.name == "Slider")
        {
            GetComponent<ObjectManipulator>().enabled = StoreManipulatableModelActiveState;
            ModelClipPlane.GetComponent<ObjectManipulator>().enabled = StoreHandDraggableClipPlaneActiveState;
        }
    }

    public void ClickTogglePlay()
    {
        AnimationPlaying = !AnimationPlaying;
    }

    public void ClickRewind()
    {
        AnimationTime = 0f;
    }

    private void ClickRemove()
    {
        UnloadInstance();
        RefreshUserInterface();
    }

    private void ClickSpeed()
    {
        const float MaxSpeed = 5f;
        float newSpeed = Mathf.Min(MaxSpeed, AnimationSpeed * 2);
        AnimationSpeed = newSpeed;
        SliderAnimationSpeed.GetComponent<PinchSlider>().SliderValue = newSpeed / SliderSpeedFactor;
        Debug.Log(SliderAnimationSpeed.GetComponent<PinchSlider>().SliderValue.ToString());
    }

    private void ClickCancelPreview()
    {
        UnloadInstance();
        RefreshUserInterface();
    }
    public void ClickToggleLayersState(PressableButtonHoloLens2 button)
    {
        var layerState = LayerSubmenuState;
        CloseSubmenus();
        LayerSubmenuState = !layerState;
    }
    private void ClickTransform()
    {
        var modelTransformState = ModelTransform;
        CloseSubmenus();
        ModelTransform = !modelTransformState;
    }
    private void ClickClipping()
    {
        var clipPlaneState = ModelClipPlaneCtrl.ClippingPlaneState;
        CloseSubmenus();
        ModelClipPlaneCtrl.ClippingPlaneState = clipPlaneState;
    }

    private void ClickAnimationSpeed()
    {
        var animationSubmenuState = AnimationSpeedSubmenu;
        CloseSubmenus();
        AnimationSpeedSubmenu = !animationSubmenuState;
    }

    private void ClickTransparency()
    {
        Transparent = !Transparent;
    }

    private void ClickPlateRotation()
    {
        CloseSubmenus();
        PlateRotation = !PlateRotation;
    }

    /* Load new model or unload.
     * newInstanceBundleName must match asset bundle name, returned by AssetBundleLoader.Name.
     * It can also be null or empty to unload a model.
     */
    public void SetInstance(string newInstanceBundleName)
    {
        if (!string.IsNullOrEmpty(newInstanceBundleName))
        {
            LoadInstance(newInstanceBundleName, false);
            LoadInitialLayers();
            ModelClipPlaneCtrl.ClippingPlaneState = ModelClippingPlaneControl.ClipPlaneState.Disabled;
        } else
        {
            UnloadInstance();
        }
        RefreshUserInterface(); // necessary after LoadInstance and UnloadInstance
    }

    private void ClickAdd(string newInstanceBundleName)
    {
        LoadInstance(newInstanceBundleName, true);
        LoadInitialLayers();
        RefreshUserInterface();
        ModelClipPlaneCtrl.ClippingPlaneState = ModelClippingPlaneControl.ClipPlaneState.Disabled;
    }

    private void ClickConfirmPreview()
    {
        LoadInstance(instanceBundle.Name, false);
        LoadInitialLayers();
        RefreshUserInterface();
    }

    private void ClickChangeLayerState(ModelLayer layer)
    {
        if (!InstanceLoaded)
        {
            Debug.Log("No model loaded, cannot show layer.");
            return;
        }

        bool addLayer = !layersLoaded.ContainsKey(layer);
        if (!addLayer) {
            UnloadLayer(layer);
        } else {
            if (layer.DataType == DataType.Volumetric)
            {
                List<ModelLayer> layersToUnload = new List<ModelLayer>();
                foreach (KeyValuePair<ModelLayer, LayerLoaded> entry in layersLoaded)
                    layersToUnload.Add(entry.Key);

                foreach (ModelLayer l in layersToUnload)
                {
                    UnloadLayer(l);
                    HoloUtilities.SetButtonStateText(layersButtons[l], false);
                }
            }
            LoadLayer(layer);
        }
        HoloUtilities.SetButtonStateText(layersButtons[layer], addLayer);
    }

    private void RefreshUserInterface()
    {
        ButtonsModel.SetActive(InstanceLoaded && !instanceIsPreview);
        ButtonsModelPreview.SetActive(InstanceLoaded && instanceIsPreview);
        PlateVisible = (!InstanceLoaded) || instanceIsPreview;

        // update ButtonTogglePlay caption and icon
        bool playing = AnimationPlaying;
        string playOrPauseText = playing ? "PAUSE" : "PLAY";
        ButtonTogglePlay.GetComponent<ButtonConfigHelper>().MainLabelText = playOrPauseText;
        string playOrPauseIconName = playing ? ButtonIconPause.name : ButtonIconPlay.name;
        ButtonTogglePlay.GetComponent<ButtonConfigHelper>().SetQuadIconByName(playOrPauseIconName);
    }

    private void UnloadLayer(ModelLayer layer)
    {
        if (!layersLoaded.ContainsKey(layer)) {
            Debug.LogWarning("Cannot unload layer " + layer.Caption + ", it is not loaded yet");
            return;
        }
        foreach (var renderer in layersLoaded[layer].Instance.GetComponentsInChildren<Renderer>())
        {
            renderer.sharedMaterial = LayerMaterial(layer);
            ModelClipPlane.GetComponent<ClippingPlane>().RemoveRenderer(renderer);
        }

        Destroy(layersLoaded[layer].Instance);
        layersLoaded.Remove(layer);
    }

    /* Unload currently loaded model.
     * May be safely called even when instance is already unloaded.
     * LoadInstance() calls this automatically at the beginning.
     *
     * After calling this, remember to call RefreshUserInterface at some point.
     */
    private void UnloadInstance()
    {
        if (layersLoaded != null) {
            foreach (LayerLoaded l in layersLoaded.Values) {
                Destroy(l.Instance);
            }
            layersLoaded = null;
            ColorMap.LayersLoaded = null;
        }

        if (layersButtons != null) {
            foreach (PressableButtonHoloLens2 button in layersButtons.Values) {
                // remove from ButtonsClickReceiver.interactables
                button.GetComponent<Interactable>().IsEnabled = false;
                Destroy(button.gameObject);
            }
            layersButtons = null;
        }

        if (instanceTransformation != null)
        {
            Destroy(instanceTransformation);
            Destroy(rotationBoxRig);
            instanceBundle = null;
            instanceTransformation = null;
            rotationBoxRig = null;
            instanceIsPreview = false; // value does not matter, but for security better to set it to something well-defined
        }

        InstanceLoaded = false;
    }

    /* Resulting instance will be in box of max size instanceMaxSize,
     * with center in instanceMove above plate origin.
     * This should match the plate designed sizes.
     */
    const float instanceMaxSize = 1.0f;
    const float expandedPlateHeight = 0.4f;
    Vector3 instanceMove = new Vector3(0f, expandedPlateHeight + instanceMaxSize / 2f, 0f); // constant
    const float buttonLayerHeight = 1.6f;

    private bool plateVisible;
    private bool PlateVisible
    {
        get { return plateVisible; }
        set {
            if (plateVisible != value) {
                plateVisible = value;
                PlateAnimated.GetComponent<Animator>().SetBool("expanded", value);
            }
        }
    }

    public void ChangeTransformationState(TransformationState newState)
    {
        bool rotationBoxRigActiveOld = transformationState == TransformationState.Rotate;
        bool scaleBoxRigActiveOld = transformationState == TransformationState.Scale;

        if (newState == transformationState) {
            // clicking again on the same sidebar button just toggles it off
            newState = TransformationState.None;
        }
        transformationState = newState;

        HoloUtilities.SetButtonState(ButtonTranslate, newState == TransformationState.Translate);
        HoloUtilities.SetButtonState(ButtonRotate, newState == TransformationState.Rotate);
        HoloUtilities.SetButtonState(ButtonScale, newState == TransformationState.Scale);

        // if transform the model: disable clipping plane manipulator
        if (newState != TransformationState.None && ModelClipPlaneCtrl.ClippingPlaneState != ModelClippingPlaneControl.ClipPlaneState.Disabled)
            ModelClipPlaneCtrl.ClippingPlaneState = ModelClippingPlaneControl.ClipPlaneState.Active;

        if (newState == TransformationState.Translate)
        {
            GetComponent<ObjectManipulator>().enabled = true;
        }
        else
        {
            GetComponent<ObjectManipulator>().enabled = false;
        }

        bool rotationBoxRigActiveNew = newState == TransformationState.Rotate;
        if (rotationBoxRigActiveOld != rotationBoxRigActiveNew && rotationBoxRig != null)
        {
            if (rotationBoxRigActiveNew) {
                rotationBoxRig.GetComponent<BoundsControl>().Active = true;
                rotationBoxRig.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;
                rotationBoxRig.GetComponent<MinMaxScaleConstraint>().enabled = true;
            } 
        }

        /* As with rotationBoxRig, note that you cannot toggle enabled below.
         * For now, GetComponent<BoundingBoxRig>() is just enabled all the time. */
        bool scaleBoxRigActiveNew = newState == TransformationState.Scale;
        if (scaleBoxRigActiveOld != scaleBoxRigActiveNew)
        {
            if (scaleBoxRigActiveNew) {
                rotationBoxRig.GetComponent<BoundsControl>().Active = true;
                rotationBoxRig.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;
                rotationBoxRig.GetComponent<MinMaxScaleConstraint>().enabled = false;
            }
        }

        if(!rotationBoxRigActiveNew && !scaleBoxRigActiveNew && rotationBoxRig != null)
        {
            rotationBoxRig.GetComponent<BoundsControl>().Active = false;
        }
    }

    /* Does this button toggles some layer (returns null if not), and which one. */
    private ModelLayer ButtonOfLayer(GameObject buttonGameObject)
    {
        if (layersButtons != null) {
            foreach (var pair in layersButtons) {
                if (pair.Value.gameObject == buttonGameObject) {
                    return pair.Key;
                }
            }
        }
        return null;
    }

    /* Load new model.
     *
     * newInstanceBundleName is a bundle name known to the ModelsCollection bundle.
     *
     * No layer is initially loaded -- you usually want to call
     * LoadLayer immediately after this.
     *
     * After calling this, remember to call RefreshUserInterface at some point.
     */
    private void LoadInstance(string newInstanceBundleName, bool newIsPreview)
    {
        if(instanceBundle != null)
        {
            Debug.Log("LoadInstance - currentInstanceName: " + instanceBundle.Name + ", newInstanceName: " + newInstanceBundleName);
            if(instanceIsPreview != newIsPreview && instanceBundle.Layers.All(l => l.DataType == DataType.Volumetric))
            {
                Debug.Log("LoadInstance - preview accepted!");
                instanceIsPreview = newIsPreview;
                return;
            }
        }            

        UnloadInstance();

        InstanceLoaded = true;
        instanceBundle = ModelsCollection.Singleton.BundleLoad(newInstanceBundleName);
        // Load Volumetric Data 
        if(instanceBundle.Layers.All(x => x.GetComponent<ModelLayer>().DataType == DataType.Volumetric))
        {
            instanceBundle.VolumetricMaterial = DefaultVolumetricMaterial;
            instanceBundle.LoadVolumetricData();
        }
        instanceIsPreview = newIsPreview;
        layersLoaded = new LayersLoaded();
        ColorMap.LayersLoaded = layersLoaded;

        rotationBoxRig = Instantiate<GameObject>(RotationBoxRigTemplate, InstanceParent.transform);

        instanceTransformation = new GameObject("InstanceTransformation");
        instanceTransformation.transform.parent = rotationBoxRig.transform;


       
        Vector3 boundsSize = Vector3.one;
        float scale = 1f;
        if (instanceBundle.Bounds.HasValue) { 
            Bounds b = instanceBundle.Bounds.Value;
            boundsSize = b.size;            
            float maxSize = Mathf.Max(new float[] { boundsSize.x, boundsSize.y, boundsSize.z });
            if (maxSize > Mathf.Epsilon) {
                scale = instanceMaxSize / maxSize;
            }
            instanceTransformation.transform.localScale = new Vector3(scale, scale, scale);
            instanceTransformation.transform.localPosition = -b.center * scale + instanceMove;

            //FIXME: this is a hack on instance not rotating properly if we move plate
            instanceTransformation.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        }

        // set proper BoxCollider bounds
        BoxCollider rotationBoxCollider = rotationBoxRig.GetComponent<BoxCollider>();
        rotationBoxCollider.center = instanceMove;
        rotationBoxCollider.size = boundsSize * scale;
        // Disable the component, to not prevent mouse clicking on buttons.
        // It will be taken into account to calculate bbox in BoundingBoxRig anyway,
        // since inside BoundingBox.GetColliderBoundsPoints it looks at all GetComponentsInChildren<Collider>() .

        // reset animation speed slider to value 1
        animationSpeed = 1f;
        SliderAnimationSpeed.GetComponent<PinchSlider>().SliderValue = animationSpeed / SliderSpeedFactor;

        // reset transparency to false
        Transparent = false;

        // add buttons to toggle layers
        layersButtons = new Dictionary<ModelLayer, PressableButtonHoloLens2>();
        int buttonIndex = 0;
        foreach (ModelLayer layer in instanceBundle.Layers)
        {
            // add button to scene
            GameObject buttonGameObject = Instantiate<GameObject>(ButtonLayerTemplate, LayerSubmenu.transform);
            buttonGameObject.transform.localPosition =
                buttonGameObject.transform.localPosition + new Vector3(0f, 0f, buttonLayerHeight * buttonIndex);
            buttonIndex++;

            // configure button text
            PressableButtonHoloLens2 button = buttonGameObject.GetComponent<PressableButtonHoloLens2>();
            if (button == null) {
                Debug.LogWarning("Missing component PressableButtonHoloLens2 in ButtonLayerTemplate");
                continue;
            }
            button.GetComponent<ButtonConfigHelper>().MainLabelText = layer.Caption;
            button.GetComponent<Interactable>().OnClick.AddListener(() => Click(buttonGameObject));

            // update layersButtons dictionary
            layersButtons[layer] = button;
        }
    }

    private void LoadInitialLayers()
    {
        if (!InstanceLoaded)
        {
            throw new Exception("Cannot call TodoLoadMeshLayer before LoadInstance");
        }

        Assert.IsTrue(instanceBundle != null);
        ModelLayer layer = instanceBundle.Layers.First<ModelLayer>(l => !l.Simulation);
        LoadLayer(layer);
        HoloUtilities.SetButtonStateText(layersButtons[layer], true);
    }

    /* Instantiate new animated layer from currently loaded model.
     * If the layer is already instantiated, does nothing
     * (for now it makes a warning, but we can disable the warning if it's useful).
     *
     * This can only be used after LoadInstance (and before corresponding UnloadInstance).
     *
     * After calling this, remember to call RefreshUserInterface at some point.
     */
    private void LoadLayer(ModelLayer layer)
    {
        if (!InstanceLoaded)
        {
            throw new Exception("Cannot call LoadLayer before LoadInstance");
        }
        if (layersLoaded.ContainsKey(layer))
        {
            Debug.LogWarning("Layer already loaded");
            return;
        }

        var layerLoaded = new LayerLoaded();
        layerLoaded.Instance = layer.InstantiateGameObject(instanceTransformation.transform);

        layerLoaded.Animation = layerLoaded.Instance.GetComponent<BlendShapeAnimation>();
        if (layerLoaded.Animation != null)
        { 
            layerLoaded.Animation.InitializeBlendShapes();
            layerLoaded.Animation.Playing = AnimationPlaying;
            layerLoaded.Animation.CurrentTime = AnimationTime;
            layerLoaded.Animation.SpeedNormalized = AnimationSpeed;
        }

        // Assign material to all MeshRenderer and SkinnedMeshRenderer inside
        foreach (var renderer in layerLoaded.Instance.GetComponentsInChildren<Renderer>()) { 
            renderer.sharedMaterial = LayerMaterial(layer);
            ModelClipPlane.GetComponent<ClippingPlane>().AddRenderer(renderer);
        }

        {

            Debug.Log("MeshRenderer Loaded to the Clipping Plane");
        }

       // update layersLoaded dictionary
       layersLoaded[layer] = layerLoaded;
    }

    /* Returns BlendShapeAnimation within any LayerLoaded with Animation component != null 
     * (that is, from any layer animated using out blend shape mechanism).
     * Returns null if no such layer exists (e.g. because there's no model loaded,
     * or it has no layers instantiated, or no layers instantiated with BlendShapeAnimation).
     */
    private BlendShapeAnimation AnyAnimatedLayer()
    {
        if (layersLoaded != null) { 
            foreach (LayerLoaded layerLoaded in layersLoaded.Values) {
                if (layerLoaded.Animation != null) {
                    return layerLoaded.Animation;
                }
            }
        }
        return null;
    }

    /* Is the animation currently playing.
     * When there are no instantiated animated layers 
     * (including when there is no instantiated model at all),
     * this always returns true and setting it is ignored.
     */
    public bool AnimationPlaying
    {
        get {
            BlendShapeAnimation animatedLayer = AnyAnimatedLayer();
            return animatedLayer != null ? animatedLayer.Playing : true;
        }
        set
        {
            if (layersLoaded != null) {
                foreach (LayerLoaded l in layersLoaded.Values) {
                    if (l.Animation != null) { 
                        l.Animation.Playing = value;
                    }
                }
                RefreshUserInterface();
            } else {
                Debug.LogWarning("AnimationPlaying changed, but no animated layer loaded");
            }
        }
    }

    /* Current time within an animation.
     * When there are no instantiated animated layers 
     * (including when there is no instantiated model at all),
     * this always returns 0f and setting it is ignored.
     */
    public float AnimationTime
    {
        get {
            BlendShapeAnimation animatedLayer = AnyAnimatedLayer();
            return animatedLayer != null ? animatedLayer.CurrentTime : 0f;
        }
        set
        {
            if (layersLoaded != null) {
                foreach (LayerLoaded l in layersLoaded.Values) {
                    if (l.Animation != null) { 
                        l.Animation.CurrentTime = value;
                    }
                }
                RefreshUserInterface();
            } else {
                Debug.LogWarning("AnimationTime changed, but no animated layer loaded");
            }
        }
    }

    private float animationSpeed = 1f;
    public float AnimationSpeed
    {
        get { return animationSpeed; }
        set {
            animationSpeed = value;

            if (layersLoaded != null) {
                foreach (LayerLoaded l in layersLoaded.Values) {
                    if (l.Animation != null) { 
                        l.Animation.SpeedNormalized = value;
                    }
                }
                RefreshUserInterface();
            }
        }
    }

    /* Based on layer properties, and current properties like Transparent,
     * determine proper material of the layer.
     */
    private Material LayerMaterial(ModelLayer layer)
    {
        Debug.Log("Getting LayerMaterial for layer: " + layer.name);
        if (layer.DataType == DataType.Volumetric)
        {
            return DefaultVolumetricMaterial;
        } else
        if (layer.Simulation)
        {
            return DataVisualizationMaterial;
        } else
        if (Transparent)
        {
            return DefaultModelTransparentMaterial;
        } else
        {
            return DefaultModelMaterial;
        }
    }

    private bool transparent;
    public bool Transparent {
        get { return transparent; }
        set
        {
            transparent = value;

            // Transparent value changed, so update materials
            if (layersLoaded != null) { 
                foreach (var layerPair in layersLoaded)
                {
                    if (!layerPair.Key.Simulation) {
                        foreach (var renderer in layerPair.Value.Instance.GetComponentsInChildren<Renderer>()) { 
                            renderer.sharedMaterial = LayerMaterial(layerPair.Key);
                        }
                    }
                }
            }
            HoloUtilities.SetButtonState(ButtonTransparency, Transparent);
        }
    }

    private bool layerSubmenuState;
    public bool LayerSubmenuState {
        get { return layerSubmenuState; }
        set
        {
            layerSubmenuState = value;
            HoloUtilities.SetButtonState(ButtonLayerSubmenu, value);
            LayerSubmenu.SetActive(value);

        }

    }

    private bool animationSpeedSubmenu;
    public bool AnimationSpeedSubmenu {
        get { return animationSpeedSubmenu; }
        set
        {
            animationSpeedSubmenu = value;
            HoloUtilities.SetButtonState(ButtonAnimationSubmenu, value);
            ButtonAnimationSubmenu.gameObject.transform.parent.Find("AnimationSpeedSubMenu").gameObject.SetActive(value);

        }
    }

    private bool plateRotation = false;
    public bool PlateRotation{
        get { return plateRotation; }
        set
        {
            plateRotation = value;
            HoloUtilities.SetButtonState(ButtonPlateRotation, value);
            PlateButtonCollection.GetComponent<ObjectManipulator>().enabled = value;
        }
    }

    private void CloseSubmenus()
    {
        LayerSubmenuState =  false;
        ModelTransform = false;
        ModelClipPlaneCtrl.ClippingPlaneState = ModelClippingPlaneControl.ClipPlaneState.Disabled;
        AnimationSpeedSubmenu = false;
    }

    private bool modelTransform;
    public bool ModelTransform {
        get { return modelTransform; }
        set
        {
            modelTransform = value;
            HoloUtilities.SetButtonState(ButtonTransform, value);
            if (value == false)
            {
                ChangeTransformationState(TransformationState.None);
            }
            ButtonTranslate.gameObject.SetActive(modelTransform);
            ButtonRotate.gameObject.SetActive(modelTransform);
            ButtonScale.gameObject.SetActive(modelTransform);
        }
    }
}
