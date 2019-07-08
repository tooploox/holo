using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using HoloToolkit.Examples.InteractiveElements;

using HoloToolkit.Unity;
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
    public Color ButtonActiveColor = new Color(0f, 0.90f, 0.88f);
    public Color ButtonInactiveColor = new Color(1f, 1f, 1f);
    public Texture2D ButtonIconPlay;
    public Texture2D ButtonIconPause;
    // Drop here "Prefabs/ModelWithPlateRotationRig"
    public GameObject RotationBoxRigTemplate;
    public GameObject Instance { get { return instance; } }

    private enum TransformationState
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

    ModelClippingPlaneControl ModelClipPlaneCtrl;

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

        // Load Player Settings
        ClickSetColorMap(PlayerPrefs.GetString("ColorMap", "jet")); // color map
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
            case "Layer1": ClickChangeLayerState(); break;
            case "ButtonColorMapJet": ClickSetColorMap("jet"); break;
            case "ButtonColorMapViridis": ClickSetColorMap("viridis"); break;
            case "ButtonColorMapMagma": ClickSetColorMap("magma"); break;
            case "ButtonColorMapCividis": ClickSetColorMap("cividis"); break;
            case "ButtonColorMapPlasma": ClickSetColorMap("plasma"); break;
            case "ButtonColorMapInferno": ClickSetColorMap("inferno"); break;
            case "ButtonAnimationSpeed": AnimationSubmenu.SetActive(!AnimationSubmenu.activeSelf); break;
            case "Slider": break;

            default:
                {
                    const string addPrefix = "Add";
                    int addInstanceIndex;
                    if (clickObject.name.StartsWith(addPrefix) &&
                        int.TryParse(clickObject.name.Substring(addPrefix.Length), out addInstanceIndex)) {
                        ClickAdd(addInstanceIndex);
                    } else {
                        Debug.LogWarning("Click on unknown object " + clickObject.name);
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
        if (instanceAnimation != null) {
            instanceAnimation.TogglePlay();
            if(dataLayerInstanceAnimation != null) {
                dataLayerInstanceAnimation.TogglePlay();
            }
            RefreshUserInterface();
        }
        else {
            Debug.LogWarning("Play / Stop button clicked, but no model loaded");
        }
    }

    public void ClickRewind()
    {
        if (instanceAnimation != null) {
            instanceAnimation.CurrentTime = 0f;
            if(dataLayerInstanceAnimation != null)
            {
                dataLayerInstanceAnimation.CurrentTime = 0f;
            }
        } else {
            Debug.LogWarning("Rewind button clicked, but no model loaded");
        }
    }

    private void ClickRemove()
    {
        UnloadInstance();
    }

    private void ClickCancelPreview()
    {
        UnloadInstance();
    }

    private void ClickAdd(int newInstanceIndex)
    {
        LoadInstance(newInstanceIndex, true);
        ModelClipPlaneCtrl.ResetState();
    }

    private void ClickConfirmPreview()
    {
        LoadInstance(instanceIndex.Value, false);
    }

    public void ClickToggleLayersState()
    {
        LayersSection.SetActive(!LayersSection.activeSelf);
        if (!LayersSection.activeSelf) {
            UnloadDataLayerInstance();
        }

        CompoundButtonText text = LayersSection.GetComponentInChildren<CompoundButtonText>();
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
        Color newColor = dataLayerInstance != null ? ButtonActiveColor : ButtonInactiveColor;
        //Debug.Log("changing color of " + button.name + " to " + newColor.ToString());
        // both _EmissiveColor and _Color (Albedo in editor) should be set to make proper effect.
        textMesh.color = newColor;
    }

    private void ClickChangeLayerState()
    {
        if (instance == null)
        {
            // This is normal if you try to turn on dataflow layer before a model is loaded
            Debug.Log("No model loaded, cannot show dataflow.");
            return;
        }

        //TODO: "dataflow" should be extracted from internal state / data / index of instance
        if (dataLayerInstance != null)
            UnloadDataLayerInstance();
        else
            LoadDataLayerInstance(instanceIndex.Value, "dataflow");
        
        CompoundButtonText text = LayersSection.GetComponentInChildren<CompoundButtonText>();
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
        Color newColor = dataLayerInstance != null ? ButtonActiveColor : ButtonInactiveColor;
        //Debug.Log("changing color of " + button.name + " to " + newColor.ToString());
        // both _EmissiveColor and _Color (Albedo in editor) should be set to make proper effect.
        textMesh.color = newColor;
    }

    private void ClickSetColorMap(string colorMapName)
    {
        Texture2D colorMap;
        colorMap = Resources.Load<Texture2D>("Colormaps/" + colorMapName);
        DataVisualizationMaterial.SetTexture("_ColorMap", colorMap);
        PlayerPrefs.SetString("ColorMap", colorMapName);
        PlayerPrefs.Save();
    }

    /* All the variables below are non-null if and only if after 
     * LoadInstance call (and before UnloadInstance). */
    private int? instanceIndex; // index to ModelsCollection
    private BlendShapeAnimation instanceAnimation;
    private BlendShapeAnimation dataLayerInstanceAnimation;
    private GameObject instance;
    private GameObject dataLayerInstance;
    private GameObject instanceTransformation;
    private bool instanceIsPreview = false;

    // Created only when instance != null, as it initializes bbox in Start and assumes it's not empty
    private GameObject rotationBoxRig;

    private void RefreshUserInterface()
    {
        ButtonsModel.SetActive(instance != null && !instanceIsPreview);
        ButtonsModelPreview.SetActive(instance != null && instanceIsPreview);
        PlateVisible = instance == null || instanceIsPreview;

        // update ButtonTogglePlay caption and icon
        bool playing = instanceAnimation != null && instanceAnimation.Playing;
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

    // Unload currently loaded instance.
    // May be safely called even when instance is already unloaded.
    // LoadInstance() calls this automatically at the beginning.
    private void UnloadInstance(bool refreshUi = true)
    {
        // First release previous instance
        UnloadDataLayerInstance();

        if (instance != null) {
            Destroy(instance);
            Destroy(instanceTransformation);
            Destroy(rotationBoxRig);

            instanceIndex = null;
            dataLayerInstance = null;
            instance = null;
            instanceTransformation = null;
            instanceAnimation = null;
            rotationBoxRig = null;
            instanceIsPreview = false; // value does not matter, but for security better to set it to something well-defined

            if (refreshUi) {
                RefreshUserInterface();
            }
        }
    }

    private void UnloadDataLayerInstance()
    {
        if (dataLayerInstance != null)
            Destroy(dataLayerInstance);

        dataLayerInstance = null;
        dataLayerInstanceAnimation = null;
    }

    /* Resulting instance will be in box of max size instanceMaxSize, 
     * with center in instanceMove above plate origin.
     * This should match the plate designed sizes.
     */
    const float instanceMaxSize = 1.3f;
    const float expandedPlateHeight = 0.4f;
    Vector3 instanceMove = new Vector3(0f, expandedPlateHeight + instanceMaxSize / 2f, 0f); // constant

    private void LoadDataLayerInstance(int currentInstanceIndex, string dataLayerSufix)
    {
        UnloadDataLayerInstance();

        GameObject template = ModelsCollection.Singleton.BundleLoadDataLayer(currentInstanceIndex, dataLayerSufix);

        dataLayerInstance = Instantiate<GameObject>(template, instance.transform);

        dataLayerInstanceAnimation = dataLayerInstance.GetComponent<BlendShapeAnimation>();
        if (dataLayerInstanceAnimation == null)
        {
            // TODO: this should be changed into "throw new...", not a warning.
            // We should settle whether the stored object has or has not BlendShapeAnimation (I vote not,
            // but any decision is fine, just stick to it).
            // After new import STL->GameObject is ready.
            Debug.LogWarning("BlendShapeAnimation component not found inside " + currentInstanceIndex + ", adding");
            dataLayerInstanceAnimation = dataLayerInstance.AddComponent<BlendShapeAnimation>();
        }

        SkinnedMeshRenderer skinnedMesh = dataLayerInstance.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer mainInstanceSkinnedMesh = instance.GetComponent<SkinnedMeshRenderer>();


        int blendShapesCount = skinnedMesh.sharedMesh.blendShapeCount;
        int mainBlendShapesCount = mainInstanceSkinnedMesh.sharedMesh.blendShapeCount;
        if (blendShapesCount != mainBlendShapesCount)
        {
            Debug.LogWarningFormat("Data layer has different number of BlendShapes [{0}] than main model [{1}]",
                blendShapesCount, mainBlendShapesCount);
        }

        if (blendShapesCount != 0)
        {
            dataLayerInstanceAnimation.Speed = instanceAnimation.Speed;
        }
        else
        {
            Debug.LogWarning("Model has no blend shapes " + currentInstanceIndex);
        }

        Animator animator = instance.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.LogWarning("Animator component found but not wanted inside " + currentInstanceIndex + ", removing");
            Destroy(animator);
        }

        // Assign material
        skinnedMesh.sharedMaterial = DataVisualizationMaterial;

        bool playingState = instanceAnimation.Playing;

        dataLayerInstanceAnimation.Playing = false;
        instanceAnimation.Playing = false;

        instanceAnimation.CurrentTime = 0.0f;
        dataLayerInstanceAnimation.CurrentTime = instanceAnimation.CurrentTime;
        instanceAnimation.Playing = playingState;
        dataLayerInstanceAnimation.Playing = playingState;
        RefreshUserInterface();
    }

    // Load new animated shape.
    // newInstanceIndex is an index to ModelsCollection.
    private void LoadInstance(int newInstanceIndex, bool newIsPreview)
    {
        UnloadInstance(false);

        GameObject template = ModelsCollection.Singleton.BundleLoad(newInstanceIndex, newIsPreview);

        /* Instantiate BoundingBoxRig dynamically, as in the next frame (when BoundingBoxRig.Start is run)
         * it will assume that bounding box of children is not empty.
         * So we cannot create BoundingBoxRig (for rotations) earlier.
         * We also cannot create it later, right before calling Activate, as then BoundingBoxRig.Activate would
         * happen before BoundingBoxRig.Start.
         */
        rotationBoxRig = Instantiate<GameObject>(RotationBoxRigTemplate, InstanceParent.transform);

        instanceTransformation = new GameObject("InstanceTransformation");
        instanceTransformation.transform.parent = rotationBoxRig.transform;

        instance = Instantiate<GameObject>(template, instanceTransformation.transform);

        SkinnedMeshRenderer skinnedMesh = instance.GetComponent<SkinnedMeshRenderer>();
        // Note that we use bounds, not localBounds, because we want to preserve local rotations
        Bounds b = skinnedMesh.bounds;
        float scale = 1f;
        float maxSize = Mathf.Max(new float[] { b.size.x, b.size.y, b.size.z });
        if (maxSize > Mathf.Epsilon) {
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

        instanceAnimation = instance.GetComponent<BlendShapeAnimation>();
        if (instanceAnimation == null) {
            // TODO: this should be changed into "throw new...", not a warning.
            // We should settle whether the stored object has or has not BlendShapeAnimation (I vote not,
            // but any decision is fine, just stick to it).
            // After new import STL->GameObject is ready.
            Debug.LogWarning("BlendShapeAnimation component not found inside " + newInstanceIndex + ", adding");
            instanceAnimation = instance.AddComponent<BlendShapeAnimation>();
        }
        // default speed to play in 1 second
        int blendShapesCount = skinnedMesh.sharedMesh.blendShapeCount;
        if (blendShapesCount != 0) {
            instanceAnimation.Speed = skinnedMesh.sharedMesh.blendShapeCount;
        } else {
            Debug.LogWarning("Model has no blend shapes " + newInstanceIndex);
        }
        instanceAnimation.Playing = true;
        // TODO: this should be changed into "throw new...", not a warning.
        // After new import STL->GameObject is ready.
        Animator animator = instance.GetComponent<Animator>();
        if (animator != null) {
            Debug.LogWarning("Animator component found but not wanted inside " + newInstanceIndex + ", removing");
            Destroy(animator);
        }

        // Assign material
        skinnedMesh.sharedMaterial = DefaultModelMaterial;

        instanceIndex = newInstanceIndex;
        instanceIsPreview = newIsPreview;
        RefreshUserInterface();        
    }

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

    public void SetButtonState(CompoundButton button, bool active)
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

    private void ClickChangeTransformationState(TransformationState newState)
    {
        bool rotationBoxRigActiveOld = transformationState == TransformationState.Rotate;
        bool scaleBoxRigActiveOld = transformationState == TransformationState.Scale;

        if (newState == transformationState) {
            // clicking again on the same sidebar button just toggles it off
            newState = TransformationState.None;
        }
        transformationState = newState;

        SetButtonState(ButtonTranslate, newState == TransformationState.Translate);
        SetButtonState(ButtonRotate, newState == TransformationState.Rotate);
        SetButtonState(ButtonScale, newState == TransformationState.Scale);

        // turn on/off translation manipulation
        handDraggable.enabled = newState == TransformationState.Translate;

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

    private void UpdateAnimationSpeed()
    {
        float value = SliderAnimationSpeed.GetComponent<SliderGestureControl>().SliderValue;
        if (instanceAnimation != null)
            instanceAnimation.Speed = value * instance.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount;
        if (dataLayerInstanceAnimation != null)
            dataLayerInstanceAnimation.Speed = instanceAnimation.Speed;
    }
}
