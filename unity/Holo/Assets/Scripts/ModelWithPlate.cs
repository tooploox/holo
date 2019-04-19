using System.IO;
using System.Collections.Generic;

using UnityEngine;

using HoloToolkit.Unity;
using HoloToolkit.Unity.UX;
using HoloToolkit.Unity.Buttons;

public class ModelWithPlate : MonoBehaviour, IClickHandler
{
    /* Public fields that should be set in Unity Editor */
    public CompoundButtonText PlayOrStopText;
    public GameObject ButtonsModel;
    public GameObject ButtonsModelPreview;
    public GameObject PlateAnimated;

    private void Start()
    {
        RefreshUserInterface();
        LoadBundles();
    }

    /* Absolute filenames to asset bundles with models. */
    private string[] bundlesFiles;

    /* Number of "add" buttons we have in the scene. */
    private const int addButtonsCount = 15;

    /* Suffix to recognize bundle filename. May be an extension (with dot) or a normal filename suffix. */
    private const string bundleFileSuffix = "_bundle";

    /* Initialize bundlesFiles */
    private void LoadBundles()
    {
        bundlesFiles = new string[] { };
        LocalConfig localConfig = Resources.Load<LocalConfig>("localConfig");
        if (localConfig != null && !string.IsNullOrEmpty(localConfig.BundlesDirectory))
        {
            string dir = localConfig.BundlesDirectory;
            bundlesFiles = Directory.GetFiles(dir, "*" + bundleFileSuffix);
            if (bundlesFiles.Length == 0)
            {
                Debug.LogWarning("No asset bundles found in directory \"" + dir + "\". Make sure to set correct BundlesDirectory in LocalConfig in Assets/Resources/LocalConfig.asset.");
            }
            else
            {
                Debug.Log("Found " + bundlesFiles.Length.ToString() + " asset bundles in \"" + dir + "\".");

                // set add buttons captions
                List<GameObject> interactables = GetComponent<ButtonsClickReceiver>().interactables;
                for (int i = 0; i < Mathf.Min(addButtonsCount, bundlesFiles.Length); i++)
                {
                    GameObject addButton = interactables.Find(gameObject => gameObject.name == "Add" + i.ToString());
                    string modelName = Path.GetFileName(bundlesFiles[i]);
                    modelName = modelName.Substring(0, modelName.Length - bundleFileSuffix.Length);
                    addButton.GetComponent<CompoundButtonText>().Text = modelName;
                }
            }
        }
        else
        {
            Debug.LogWarning("No Assets/Resources/LocalConfig.asset. Create it from Unity Editor by \"Holo -> Create Local Configuration\"");
        }
    }

    public void Click(GameObject clickObject)
    {
        switch (clickObject.name)
        {
            case "TogglePlay": ClickTogglePlay(); break;
            case "Rewind": ClickRewind(); break;
            case "Remove": ClickRemove(); break;
            case "ConfirmPreview": ClickConfirmPreview(); break;
            case "CancelPreview": ClickCancelPreview(); break;
            default:
                {
                    const string addPrefix = "Add_";
                    if (clickObject.name.StartsWith(addPrefix)) {
                        ClickAdd(clickObject.name.Substring(addPrefix.Length));
                    } else {
                        Debug.LogWarning("Click on unknown object " + clickObject.name);
                    }
                    break;
                }
        }
    }

    public void ClickTogglePlay()
    {
        if (instanceAnimation != null) {
            instanceAnimation.TogglePlay();
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

    private void ClickAdd(string newinstancePath)
    {
        LoadInstance(newinstancePath, true);
    }

    private void ClickConfirmPreview()
    {
        LoadInstance(instancePath, false);
    }

    /* All the variables below are non-null if and only if after 
     * LoadInstance call (and before UnloadInstance). */
    private string instancePath;
    private BlendShapeAnimation instanceAnimation;
    private GameObject instance;
    private GameObject instanceTransformation;
    private bool instanceIsPreview = false;

    private void RefreshUserInterface()
    {
        ButtonsModel.SetActive(instance != null && !instanceIsPreview);
        ButtonsModelPreview.SetActive(instance != null && instanceIsPreview);
        PlayOrStopText.Text = (instanceAnimation != null && instanceAnimation.Playing ? "STOP" : "PLAY");
        PlateVisible = instance == null || instanceIsPreview;
    }

    // Unload currently loaded instance.
    // May be safely called even when instance is already unloaded.
    // LoadInstance() calls this automatically at the beginning.
    private void UnloadInstance(bool refreshUi = true)
    {
        // First release previous instance
        if (instance != null) {
            Destroy(instance);
            Destroy(instanceTransformation);

            instancePath = null;
            instance = null;
            instanceTransformation = null;
            instanceAnimation = null;
            instanceIsPreview = false; // value does not matter, but for security better to set it to something well-defined

            if (refreshUi) {
                RefreshUserInterface();
            }
        }
    }

    // Load new animated shape.
    // newInstancePath is a path to an object (prefab, fbx etc. -- anything that can be loaded as GameObject),
    // relative to "Assets/Resources/Models/" (with addsitional "Preview/" subdirectory if this
    // newIsPreview.
    private void LoadInstance(string newInstancePath, bool newIsPreview)
    {
        UnloadInstance(false);

        string fullInstancePath = "Models/";
        if (newIsPreview) {
            fullInstancePath += "Preview/";
        }
        fullInstancePath += newInstancePath;

        // TODO: first Resources.Load takes a bit
        GameObject template = Resources.Load<GameObject>(fullInstancePath);
        if (template == null) {
            throw new System.Exception("Cannot load GameObject from " + fullInstancePath);
        }

        instanceTransformation = new GameObject("InstanceTransformation");
        instanceTransformation.transform.parent = transform;

        instance = Instantiate<GameObject>(template, instanceTransformation.transform);

        // transform instance to be centered with a box of size (2,2,2)
        SkinnedMeshRenderer skinnedMesh = instance.GetComponent<SkinnedMeshRenderer>();
        // Note that we use bounds, not localBounds, because we want to preserve local rotations
        Bounds b = skinnedMesh.bounds;
        float scale = 1f;
        float maxSize = Mathf.Max(new float[] { b.size.x, b.size.y, b.size.z });
        if (maxSize > Mathf.Epsilon) {
            scale = 2 / maxSize;
        }
        instanceTransformation.transform.localScale = new Vector3(scale, scale, scale);
        instanceTransformation.transform.localPosition = -b.center * scale + new Vector3(0f, 1.5f, 0f);

        instanceAnimation = instance.GetComponent<BlendShapeAnimation>();
        if (instanceAnimation == null) {
            // TODO: this should be changed into "throw new...", not a warning.
            // We should settle whether the stored object has or has not BlendShapeAnimation (I vote not,
            // but any decision is fine, just stick to it).
            // After new import STL->GameObject is ready.
            Debug.LogWarning("BlendShapeAnimation component not found inside " + newInstancePath + ", adding");
            instanceAnimation = instance.AddComponent<BlendShapeAnimation>();
        }
        // default speed to play in 1 second
        int blendShapesCount = skinnedMesh.sharedMesh.blendShapeCount;
        if (blendShapesCount != 0) {
            instanceAnimation.Speed = skinnedMesh.sharedMesh.blendShapeCount;
        } else {
            Debug.LogWarning("Model has no blend shapes " + newInstancePath);
        }
        instanceAnimation.Playing = true;
        // TODO: this should be changed into "throw new...", not a warning.
        // After new import STL->GameObject is ready.
        Animator animator = instance.GetComponent<Animator>();
        if (animator != null) {
            Debug.LogWarning("Animator component found but not wanted inside " + newInstancePath + ", removing");
            Destroy(animator);
        }

        //// Add Direction indicator for loaded model
        //instance.AddComponent<DirectionIndicator>();
        //DirectionIndicator directionInd = instance.GetComponent<DirectionIndicator>();
        //directionInd.Cursor = GameObject.Find("DefaultCursor");
        //directionInd.DirectionIndicatorObject = Resources.Load(
        //    "Assets/GFX/UI/HandDetectedFeedbackMod.prefab", typeof(GameObject)) as GameObject;
        //directionInd.DirectionIndicatorColor.r = 61.0f;
        //directionInd.DirectionIndicatorColor.g = 206.0f;
        //directionInd.DirectionIndicatorColor.b = 200.0f;
        //directionInd.VisibilitySafeFactor = -0.5f;
        //directionInd.MetersFromCursor = 0.1f;
        //directionInd.Awake();

        instancePath = newInstancePath;
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

    // TODO update time slider now
    /*
    private void Update()
    {
        if (playing && blendShapeAnimation != null) {
            timeSlider.value = blendShapeAnimation.CurrentTime;
        }
    }
    */

    // TODO: read time slider now
    /*
    private void TimeSliderValueChanged(float newPosition)
    {
        if (blendShapeAnimation != null) {
            blendShapeAnimation.CurrentTime = newPosition;
        }
    }
    */
}
