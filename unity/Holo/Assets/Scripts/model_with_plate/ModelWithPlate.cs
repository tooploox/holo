using System;
using System.Collections.Generic;
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
    public Material DataVisualizationMaterial;
    public Transform InstanceParent;
    public CompoundButton ButtonTogglePlay;
    public CompoundButton ButtonTranslate;
    public CompoundButton ButtonRotate;
    public CompoundButton ButtonScale;
    public CompoundButton ButtonLayerDataFlow;
    public Texture2D ButtonIconPlay;
    public Texture2D ButtonIconPause;
    // Drop here "Prefabs/ModelWithPlateRotationRig"
    public GameObject RotationBoxRigTemplate;

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

    private class LoadedLayer
    {
        public GameObject Instance;
        public BlendShapeAnimation Animation;
    }

    private bool instanceLoaded = false;
    public bool InstanceLoaded { get { return instanceLoaded; } }
    /* All the variables below are non-null 
     * only when instanceLoaded,
     * that is only after LoadInstance call (and before UnloadInstance). */
    private int? instanceIndex; // index to ModelsCollection
    private Dictionary<ModelLayer, LoadedLayer> loadedLayers;
    private GameObject instanceTransformation;
    private bool instanceIsPreview = false;

    // Created only when instance != null, as it initializes bbox in Start and assumes it's not empty
    private GameObject rotationBoxRig;

    private void Start()
    {
        /* Adding components using code, simply because it's more friendly to version control */
        handDraggable = gameObject.AddComponent<HandDraggable>();
        handDraggable.RotationMode = HandDraggable.RotationModeEnum.LockObjectRotation;
        GetComponent<TwoHandManipulatable>().enabled = false;

        ModelClipPlaneCtrl = ModelClipPlane.GetComponentInChildren<ModelClippingPlaneControl>();
        // Turn off the clipping plane on start
        DefaultModelMaterial.DisableKeyword("CLIPPING_ON");
        DataVisualizationMaterial.DisableKeyword("CLIPPING_ON");

        LayersSection.SetActive(false);
        AnimationSubmenu.SetActive(false);

        RefreshUserInterface();
        InitializeAddButtons();

        // This sets proper state of buttons and components like handDraggable
        ClickChangeTransformationState(TransformationState.None);

        // Animation speed slider
        SliderAnimationSpeed.GetComponent<SliderGestureControl>().OnUpdateEvent.AddListener(delegate { UpdateAnimationSpeed(); });
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
            string modelName = ModelsCollection.Singleton.BundleCaption(i);
            GameObject button = FindAddButton(i);
            button.GetComponent<CompoundButtonText>().Text = modelName;
            button.SetActive(true);
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
        switch (clickObject.name)
        {
            case "TogglePlay": ClickTogglePlay(); break;
            case "Rewind": ClickRewind(); break;
            case "Remove": ClickRemove(); break;
            case "ConfirmPreview": ClickConfirmPreview(); break;
            case "CancelPreview": ClickCancelPreview(); break;
            case "ButtonTranslate": ClickChangeTransformationState(TransformationState.Translate); break;
            case "ButtonRotate": ClickChangeTransformationState(TransformationState.Rotate); break;
            case "ButtonScale": ClickChangeTransformationState(TransformationState.Scale); break;
            case "ButtonLayers": ClickToggleLayersState(); break;
            case "ButtonLayerDataFlow": ClickChangeLayerState(); break;
            case "ButtonAnimationSpeed": AnimationSubmenu.SetActive(!AnimationSubmenu.activeSelf); break;

            default:
                {
                    const string addPrefix = "Add";
                    int addInstanceIndex;
                    if (clickObject.name.StartsWith(addPrefix) &&
                        int.TryParse(clickObject.name.Substring(addPrefix.Length), out addInstanceIndex)) {
                        ClickAdd(addInstanceIndex);
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
        if (loadedLayers != null) {
            foreach (LoadedLayer loadedLayer in loadedLayers.Values) {
                loadedLayer.Animation.TogglePlay();
            }
            RefreshUserInterface();
        } else {
            Debug.LogWarning("Play / Stop button clicked, but no model loaded");
        }
    }

    public void ClickRewind()
    {
        if (loadedLayers != null) {
            foreach (LoadedLayer loadedLayer in loadedLayers.Values) {
                loadedLayer.Animation.CurrentTime = 0f;
            }
            RefreshUserInterface();
        } else {
            Debug.LogWarning("Rewind button clicked, but no model loaded");
        }
    }

    private void ClickRemove()
    {
        UnloadInstance();
        RefreshUserInterface();
    }

    private void ClickCancelPreview()
    {
        UnloadInstance();
        RefreshUserInterface();
    }

    private void ClickAdd(int newInstanceIndex)
    {
        LoadInstance(newInstanceIndex, true);
        LoadInitialLayers();
        RefreshUserInterface();
        ModelClipPlaneCtrl.ClippingPlaneState = ModelClippingPlaneControl.ClipPlaneState.Disabled;
    }

    private void ClickConfirmPreview()
    {
        LoadInstance(instanceIndex.Value, false);
        LoadInitialLayers();
        RefreshUserInterface();
    }

    public void ClickToggleLayersState()
    {
        LayersSection.SetActive(!LayersSection.activeSelf);
    }

    private void ClickChangeLayerState()
    {
        // TODO: get layer id (some internal name? or just reference to ModelLayer?)

        if (!instanceLoaded)
        {
            Debug.Log("No model loaded, cannot show layer.");
            return;
        }

        AssetBundleLoader bundleLoader = ModelsCollection.Singleton.BundleLoad(instanceIndex.Value);
        ModelLayer layerSimulation = bundleLoader.Layers.First<ModelLayer>(l => l.Simulation);
        bool addLayer = !loadedLayers.ContainsKey(layerSimulation);
        if (!addLayer) {
            UnloadLayer(layerSimulation);
        } else {
            LoadLayer(layerSimulation);
        }
        HoloUtilities.SetButtonStateText(ButtonLayerDataFlow, addLayer);
    }

    // First LoadedLayer, of any layer is loaded now.
    private LoadedLayer FirstLoadedLayer()
    {
        return loadedLayers != null && loadedLayers.Count != 0 ? loadedLayers.Values.First<LoadedLayer>() : null;
    }

    private void RefreshUserInterface()
    {
        ButtonsModel.SetActive(instanceLoaded && !instanceIsPreview);
        ButtonsModelPreview.SetActive(instanceLoaded && instanceIsPreview);
        PlateVisible = (!instanceLoaded) || instanceIsPreview;

        // update ButtonTogglePlay caption and icon
        LoadedLayer firstLayer = FirstLoadedLayer();
        bool playing = firstLayer != null && firstLayer.Animation.Playing;
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
        if (!loadedLayers.ContainsKey(layer)) {
            Debug.LogWarning("Cannot unload layer " + layer.Caption + ", it is not loaded yet");
            return;
        }
        Destroy(loadedLayers[layer].Instance);
        loadedLayers.Remove(layer);
    }

    /* Unload currently loaded model.
     * May be safely called even when instance is already unloaded.
     * LoadInstance() calls this automatically at the beginning.
     * 
     * After calling this, remember to call RefreshUserInterface at some point.
     */
    private void UnloadInstance()
    {
        if (loadedLayers != null) {
            foreach (LoadedLayer loadedLayer in loadedLayers.Values) {
                Destroy(loadedLayer.Instance);
            }
            loadedLayers = null;
        }

        if (instanceTransformation != null)
        {
            Destroy(instanceTransformation);
            Destroy(rotationBoxRig);
            instanceIndex = null;
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

    /* Load new model.
	 *
	 * newInstanceIndex is an index to ModelsCollection bundle.
	 *
	 * No layer is initially loaded -- you usually want to call
	 * LoadLayer immediately after this.
	 *
     * After calling this, remember to call RefreshUserInterface at some point.
	 */
    private void LoadInstance(int newInstanceIndex, bool newIsPreview)
    {
        UnloadInstance();

        instanceLoaded = true;
        instanceIndex = newInstanceIndex;
        instanceIsPreview = newIsPreview;
        loadedLayers = new Dictionary<ModelLayer, LoadedLayer>();

        /* Instantiate BoundingBoxRig dynamically, as in the next frame (when BoundingBoxRig.Start is run)
         * it will assume that bounding box of children is not empty.
         * So we cannot create BoundingBoxRig (for rotations) earlier.
         * We also cannot create it later, right before calling Activate, as then BoundingBoxRig.Activate would
         * happen before BoundingBoxRig.Start.
         */
        rotationBoxRig = Instantiate<GameObject>(RotationBoxRigTemplate, InstanceParent.transform);

        instanceTransformation = new GameObject("InstanceTransformation");
        instanceTransformation.transform.parent = rotationBoxRig.transform;

        AssetBundleLoader bundleLoader = ModelsCollection.Singleton.BundleLoad(newInstanceIndex);
        // Note that we use bounds, not localBounds, because we want to preserve local rotations
        Bounds b = bundleLoader.Bounds;
        float scale = 1f;
        float maxSize = Mathf.Max(new float[] { b.size.x, b.size.y, b.size.z });
        if (maxSize > Mathf.Epsilon)
        {
            scale = instanceMaxSize / maxSize;
        }
        instanceTransformation.transform.localScale = new Vector3(scale, scale, scale);
        instanceTransformation.transform.localPosition = -b.center * scale + instanceMove;

        // set proper BoxCollider bounds
        BoxCollider rotationBoxCollider = rotationBoxRig.GetComponent<BoxCollider>();
        rotationBoxCollider.center = instanceMove;
        rotationBoxCollider.size = b.size * scale;
        // Disable the component, to not prevent mouse clicking on buttons.
        // It will be taken into account to calculate bbox in BoundingBoxRig anyway,
        // since inside BoundingBox.GetColliderBoundsPoints it looks at all GetComponentsInChildren<Collider>() .
        rotationBoxCollider.enabled = false;

        // reset animation speed slider to value 1
        SliderAnimationSpeed.GetComponent<SliderGestureControl>().SetSliderValue(1f);
    }

    private void LoadInitialLayers()
    {
        if (!instanceLoaded)
        {
            throw new Exception("Cannot call TodoLoadMeshLayer before LoadInstance");
        }

        Assert.IsTrue(instanceIndex.HasValue);
        AssetBundleLoader bundleLoader = ModelsCollection.Singleton.BundleLoad(instanceIndex.Value);
        ModelLayer layer = bundleLoader.Layers.First<ModelLayer>(l => !l.Simulation);
        LoadLayer(layer);
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
        if (loadedLayers.ContainsKey(layer))
        {
            Debug.LogWarning("Layer already loaded");
            return;
        }

        LoadedLayer firstLayer = FirstLoadedLayer();
        bool currentlyPlaying = firstLayer != null ? firstLayer.Animation.Playing : true;
        float currentlyTime = firstLayer != null ? firstLayer.Animation.CurrentTime : 0f;

        LoadedLayer loadedLayer = new LoadedLayer();
        loadedLayer.Instance = layer.InstantiateGameObject(instanceTransformation.transform);

        loadedLayer.Animation = loadedLayer.Instance.GetComponent<BlendShapeAnimation>();
        // It should be already checked and eventually fixed in AssetBundleLoader
        Assert.IsTrue(loadedLayer.Animation != null);
        loadedLayer.Animation.Playing = currentlyPlaying;
        loadedLayer.Animation.CurrentTime = currentlyTime;

        // Assign material
        SkinnedMeshRenderer skinnedMesh = loadedLayer.Instance.GetComponent<SkinnedMeshRenderer>();
        skinnedMesh.sharedMaterial = layer.Simulation ? DataVisualizationMaterial : DefaultModelMaterial;

        // update loadedLayers dictionary
        loadedLayers[layer] = loadedLayer;
    }

    private void UpdateAnimationSpeed()
    {
        float value = SliderAnimationSpeed.GetComponent<SliderGestureControl>().SliderValue;

        if (loadedLayers != null) {
            foreach (LoadedLayer loadedLayer in loadedLayers.Values) {
                loadedLayer.Animation.Speed = value;
                // * loadedLayer.Instance.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount;
            }
            RefreshUserInterface();
        }
        else {
            Debug.LogWarning("Play / Stop button clicked, but no model loaded");
        }
    }
}
