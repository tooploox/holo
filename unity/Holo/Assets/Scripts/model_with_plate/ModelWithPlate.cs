using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

using HoloToolkit.Examples.InteractiveElements;

using HoloToolkit.Unity.UX;
using HoloToolkit.Unity.Buttons;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.InputModule.Utilities.Interactions;


public class ModelWithPlate : MonoBehaviour, IClickHandler
{
    /* Public fields that should be set in Unity Editor */
    public GameObject SliderAnimationSpeed;
    public GameObject ButtonsModel;
    public GameObject ButtonsModelPreview;
    public GameObject PlateAnimated;
    public GameObject LayersSection;
    public GameObject AnimationSubmenu;
    public Material DefaultModelMaterial;
    public Material DefaultModelTransparentMaterial;
    public Material DataVisualizationMaterial;
    public Material DefaultVolumetricMaterial;
    public Transform InstanceParent;
    public CompoundButton ButtonTogglePlay;
    public CompoundButton ButtonTranslate;
    public CompoundButton ButtonRotate;
    public CompoundButton ButtonScale;
    public CompoundButton ButtonTransparency;
    public CompoundButton ButtonPlateTransform;
    public TwoHandManipulatable PlateTransformManipulatable;
    public GameObject ButtonLayerTemplate;
    public Texture2D ButtonIconPlay;
    public Texture2D ButtonIconPause;
    // Drop here "Prefabs/ModelWithPlateRotationRig"
    public GameObject RotationBoxRigTemplate;
    public float IconScale = 1f;
    public float IconShiftY = 0f;

    public enum TransformationState
    {
        None,
        Translate,
        Rotate,
        Scale
    }
    private TransformationState transformationState = TransformationState.None;

    // Constant after Start()
    private HandDraggable handDraggable;

    public GameObject ModelClipPlane;

    private ModelClippingPlaneControl ModelClipPlaneCtrl;

    private class LayerLoaded
    {
        // Instance on scene.
        public GameObject Instance;
        // Shortcut for Instance.GetComponent<BlendShapeAnimation>().
        // May be null for layers not animated using BlendShapeAnimation.
        public BlendShapeAnimation Animation;
    }

    private bool instanceLoaded = false;
    public bool InstanceLoaded { get { return instanceLoaded; } }
    /* All the variables below are non-null
     * only when instanceLoaded,
     * that is only after LoadInstance call (and before UnloadInstance). */
    private AssetBundleLoader instanceBundle;
    private Dictionary<ModelLayer, LayerLoaded> layersLoaded;
    private Dictionary<ModelLayer, CompoundButton> layersButtons;
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
        /* Adding components using code, simply because it's more friendly to version control */
        handDraggable = gameObject.AddComponent<HandDraggable>();
        handDraggable.RotationMode = HandDraggable.RotationModeEnum.LockObjectRotation;
        GetComponent<TwoHandManipulatable>().enabled = false;

        ModelClipPlaneCtrl = ModelClipPlane.GetComponentInChildren<ModelClippingPlaneControl>();
        // Turn off the clipping plane on start
        DefaultModelMaterial.DisableKeyword("CLIPPING_ON");
        DefaultModelTransparentMaterial.DisableKeyword("CLIPPING_ON");
        DataVisualizationMaterial.DisableKeyword("CLIPPING_ON");

        LayersSection.SetActive(false);
        AnimationSubmenu.SetActive(false);

        RefreshUserInterface();
        InitializeAddButtons();

        // This sets proper state of buttons and components like handDraggable
        ClickChangeTransformationState(TransformationState.None);

        // Animation speed slider
        SliderAnimationSpeed.GetComponent<SliderGestureControl>().OnUpdateEvent.AddListener(
            delegate { 
                AnimationSpeed = SliderAnimationSpeed.GetComponent<SliderGestureControl>().SliderValue;
            }
        );
    }

    /* Number of "add" buttons we have in the scene. */
    private const int addButtonsCount = 15;

    /* Find the GameObject of some "AddXxx" button. */
    private GameObject FindAddButton(int i)
    {
        List<GameObject> interactables = GetComponent<ButtonsClickReceiver>().interactables;
        return interactables.Find(gameObject => gameObject.name == "Add" + i.ToString());
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
            button.GetComponent<CompoundButtonText>().Text = modelName;

            Texture2D icon = ModelsCollection.Singleton.BundleIcon(i);
            if (icon != null) { 
                button.GetComponent<CompoundButtonIcon>().IconScale = IconScale;
                button.GetComponent<CompoundButtonIcon>().IconShiftY = IconShiftY;
                button.GetComponent<CompoundButtonIcon>().SetIconOverride(icon);
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
        if (SharingSceneData.Singleton.isClient &&
		    !SharingSceneData.Singleton.isServer) {
            return;
        }

        switch (clickObject.name)
        {
            case "TogglePlay": ClickTogglePlay(); break;
            case "Rewind": ClickRewind(); break;
            case "Remove": ClickRemove(); break;
            case "Speed": ClickSpeed(); break;
            case "ConfirmPreview": ClickConfirmPreview(); break;
            case "CancelPreview": ClickCancelPreview(); break;
            case "ButtonTranslate": ClickChangeTransformationState(TransformationState.Translate); break;
            case "ButtonRotate": ClickChangeTransformationState(TransformationState.Rotate); break;
            case "ButtonScale": ClickChangeTransformationState(TransformationState.Scale); break;
            case "ButtonLayers": ClickToggleLayersState(); break;
            case "ButtonAnimationSpeed": AnimationSubmenu.SetActive(!AnimationSubmenu.activeSelf); break;
            case "ButtonTransparency": ClickTransparency(); break;
            case "ButtonPlateTransform": ClickPlateTransform(); break;

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
                        if (clickObject.name.StartsWith(addPrefix) &&
                            int.TryParse(clickObject.name.Substring(addPrefix.Length), out addInstanceIndex))
                        {
                            ClickAdd(ModelsCollection.Singleton.BundleName(addInstanceIndex));
                        }
                    }
                    break;
                }
        }
    }

    private bool StoreTwoHandManipulatableModelActiveState, StoreHandDraggableModelActiveState, StoreHandDraggableClipPlaneActiveState;
    public void FocusEnter(GameObject focusObject)
    {
        if (focusObject.name == "Slider")
        {
            StoreTwoHandManipulatableModelActiveState = GetComponent<TwoHandManipulatable>().enabled;
            StoreHandDraggableModelActiveState = GetComponent<HandDraggable>().enabled;
            StoreHandDraggableClipPlaneActiveState = ModelClipPlane.GetComponent<HandDraggable>().enabled;
            GetComponent<TwoHandManipulatable>().enabled = false;
            GetComponent<HandDraggable>().enabled = false;
            ModelClipPlane.GetComponent<HandDraggable>().enabled = false;
        }
    }

    public void FocusExit(GameObject focusObject)
    {
        if (focusObject.name == "Slider")
        {
            GetComponent<TwoHandManipulatable>().enabled = StoreTwoHandManipulatableModelActiveState;
            GetComponent<HandDraggable>().enabled = StoreHandDraggableModelActiveState;
            ModelClipPlane.GetComponent<HandDraggable>().enabled = StoreHandDraggableClipPlaneActiveState;
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
        SliderAnimationSpeed.GetComponent<SliderGestureControl>().SetSliderValue(newSpeed);
    }

    private void ClickCancelPreview()
    {
        UnloadInstance();
        RefreshUserInterface();
    }

    private void ClickTransparency()
    {
        Transparent = !Transparent;
    }

    private void ClickPlateTransform()
    {
        TestAcces();
        PlateTransform = !PlateTransform;
    }

    private void TestAcces()
    {
        //#define ENABLE_WINMD_SUPPORT
#if ENABLE_WINMD_SUPPORT
        string newFileName = DateTime.Now.ToString("ddMMyyyy-HHmmss") + "_test.txt";

        LocalConfig localConfig = Resources.Load<LocalConfig>("LocalConfig");
        string LogFileDir = localConfig.GetBundlesDirectory();


        string text = File.ReadAllText(Path.Combine(LogFileDir, "testFile.txt"));

        newFileName = Path.Combine(LogFileDir, newFileName);
        using (var logFile = File.CreateText(newFileName))
        {
            logFile.WriteLine(" ==== " + DateTime.Now.ToString() + " logging started ==== ");
            logFile.WriteLine(text);
        }
#endif
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

    public void ClickToggleLayersState()
    {
        LayersSection.SetActive(!LayersSection.activeSelf);
    }

    private void ClickChangeLayerState(ModelLayer layer)
    {
        if (!instanceLoaded)
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
        ButtonsModel.SetActive(instanceLoaded && !instanceIsPreview);
        ButtonsModelPreview.SetActive(instanceLoaded && instanceIsPreview);
        PlateVisible = (!instanceLoaded) || instanceIsPreview;

        // update ButtonTogglePlay caption and icon
        bool playing = AnimationPlaying;
        string playOrPauseText = playing ? "PAUSE" : "PLAY";
        ButtonTogglePlay.GetComponent<CompoundButtonText>().Text = playOrPauseText;
        Texture2D playOrPatseIcon = playing ? ButtonIconPause : ButtonIconPlay;
        MeshRenderer iconRenderer = ButtonTogglePlay.GetComponent<CompoundButtonIcon>().IconMeshFilter.GetComponent<MeshRenderer>();
        if (iconRenderer != null) {
            iconRenderer.sharedMaterial.mainTexture = playOrPatseIcon;
        } else {
            Debug.LogWarning("ButtonTogglePlay icon does not have MeshRenderer");
        }
    }

    private void UnloadLayer(ModelLayer layer)
    {
        if (!layersLoaded.ContainsKey(layer)) {
            Debug.LogWarning("Cannot unload layer " + layer.Caption + ", it is not loaded yet");
            return;
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
        }

        if (layersButtons != null) {
            foreach (CompoundButton button in layersButtons.Values) {
                // remove from ButtonsClickReceiver.interactables
                ButtonsClickReceiver clickReceiver = GetComponent<ButtonsClickReceiver>();
                clickReceiver.interactables.Remove(button.gameObject);
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

        instanceLoaded = false;
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

    public void ClickChangeTransformationState(TransformationState newState)
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

        // turn on/off translation manipulation
        handDraggable.enabled = newState == TransformationState.Translate;
        handDraggable.IsDraggingEnabled = newState == TransformationState.Translate;

        // turn on/off rotation manipulation
        /*
         * We do not switch BoundingBoxRig enabled now.
         * It would serve no purpose (calling Activate or Deactivate is enough),
         * and it woud actually break Activate (because you cannot call Activate in the same
         * frame as setting enabled=true for the 1st frame, this causes problems in BoundingBoxRig
         * as private "objectToBound" is only assigned in BoundingBoxRig.Start).
         *
        rotationBoxRig.enabled = newState == TransformationState.Rotate;
        */
        // call rotationBoxRig.Activate or Deactivate
        bool rotationBoxRigActiveNew = newState == TransformationState.Rotate;
        if (rotationBoxRigActiveOld != rotationBoxRigActiveNew && rotationBoxRig != null)
        {
            if (rotationBoxRigActiveNew) {
                rotationBoxRig.GetComponent<BoundingBoxRig>().Activate();
            } else {
                rotationBoxRig.GetComponent<BoundingBoxRig>().Deactivate();
            }
        }

        /* As with rotationBoxRig, note that you cannot toggle enabled below.
         * For now, GetComponent<BoundingBoxRig>() is just enabled all the time. */
        bool scaleBoxRigActiveNew = newState == TransformationState.Scale;
        if (scaleBoxRigActiveOld != scaleBoxRigActiveNew)
        {
            if (scaleBoxRigActiveNew) {
                GetComponent<TwoHandManipulatable>().enabled = true;
            } else {
                GetComponent<TwoHandManipulatable>().enabled = false;
            }
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

        instanceLoaded = true;
        instanceBundle = ModelsCollection.Singleton.BundleLoad(newInstanceBundleName);
        // Load Volumetric Data 
        if(instanceBundle.Layers.All(x => x.GetComponent<ModelLayer>().DataType == DataType.Volumetric))
        {
            instanceBundle.VolumetricMaterial = DefaultVolumetricMaterial;
            instanceBundle.LoadVolumetricData();
        }
        instanceIsPreview = newIsPreview;
        layersLoaded = new Dictionary<ModelLayer, LayerLoaded>();

        /* Instantiate BoundingBoxRig dynamically, as in the next frame (when BoundingBoxRig.Start is run)
         * it will assume that bounding box of children is not empty.
         * So we cannot create BoundingBoxRig (for rotations) earlier.
         * We also cannot create it later, right before calling Activate, as then BoundingBoxRig.Activate would
         * happen before BoundingBoxRig.Start.
         */
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
        }

        // set proper BoxCollider bounds
        BoxCollider rotationBoxCollider = rotationBoxRig.GetComponent<BoxCollider>();
        rotationBoxCollider.center = instanceMove;
        rotationBoxCollider.size = boundsSize * scale;
        // Disable the component, to not prevent mouse clicking on buttons.
        // It will be taken into account to calculate bbox in BoundingBoxRig anyway,
        // since inside BoundingBox.GetColliderBoundsPoints it looks at all GetComponentsInChildren<Collider>() .
        rotationBoxCollider.enabled = false;

        // reset animation speed slider to value 1
        animationSpeed = 1f;
        SliderAnimationSpeed.GetComponent<SliderGestureControl>().SetSliderValue(animationSpeed);

        // reset transparency to false
        Transparent = false;

        // add buttons to toggle layers
        layersButtons = new Dictionary<ModelLayer, CompoundButton>();
        int buttonIndex = 0;
        foreach (ModelLayer layer in instanceBundle.Layers)
        {
            // add button to scene
            GameObject buttonGameObject = Instantiate<GameObject>(ButtonLayerTemplate, LayersSection.transform);
            buttonGameObject.transform.localPosition =
                buttonGameObject.transform.localPosition + new Vector3(0f, 0f, buttonLayerHeight * buttonIndex);
            buttonIndex++;

            // configure button text
            CompoundButton button = buttonGameObject.GetComponent<CompoundButton>();
            if (button == null) {
                Debug.LogWarning("Missing component CompoundButton in ButtonLayerTemplate");
                continue;
            }
            button.GetComponent<CompoundButtonText>().Text = layer.Caption;

            // extend ButtonsClickReceiver.interactables
            ButtonsClickReceiver clickReceiver = GetComponent<ButtonsClickReceiver>();
            clickReceiver.interactables.Add(buttonGameObject);

            // update layersButtons dictionary
            layersButtons[layer] = button;
        }
    }

    private void LoadInitialLayers()
    {
        if (!instanceLoaded)
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
        if (!instanceLoaded)
        {
            throw new Exception("Cannot call LoadLayer before LoadInstance");
        }
        if (layersLoaded.ContainsKey(layer))
        {
            Debug.LogWarning("Layer already loaded");
            return;
        }

        LayerLoaded l = new LayerLoaded();
        l.Instance = layer.InstantiateGameObject(instanceTransformation.transform);

        l.Animation = l.Instance.GetComponent<BlendShapeAnimation>();
        if (l.Animation != null)
        { 
            l.Animation.InitializeBlendShapes();
            l.Animation.Playing = AnimationPlaying;
            l.Animation.CurrentTime = AnimationTime;
            l.Animation.SpeedNormalized = AnimationSpeed;
        }

        // Assign material to all MeshRenderer and SkinnedMeshRenderer inside
        foreach (var renderer in l.Instance.GetComponentsInChildren<Renderer>()) { 
            renderer.sharedMaterial = LayerMaterial(layer);
        }

        // update layersLoaded dictionary
        layersLoaded[layer] = l;
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

    private bool plateTransform;
    public bool PlateTransform {
        get { return plateTransform; }
        set
        {
            plateTransform = value;
            PlateTransformManipulatable.GetComponent<Collider>().enabled = plateTransform;
            PlateTransformManipulatable.enabled = plateTransform;
            HoloUtilities.SetButtonState(ButtonPlateTransform, PlateTransform);
        }
    }
}
