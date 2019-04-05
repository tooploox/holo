using UnityEngine;
using HoloToolkit.Unity.UX;

public class ModelWithPlate : MonoBehaviour, IClickHandler
{
    /* Public fields that should be set in Unity Editor */
    public TextMesh PlayOrStopText;
    public GameObject Decorations;
    public GameObject ButtonsModel;
    public GameObject ButtonsModelPreview;
    public GameObject PlateAnimated;

    private void Start()
    {
        RefreshUserInterface();
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
    private bool instanceIsPreview;

    private void RefreshUserInterface()
    {
        ButtonsModel.SetActive(instance != null && !instanceIsPreview);
        ButtonsModelPreview.SetActive(instance != null && instanceIsPreview);
        PlayOrStopText.text = (instanceAnimation != null && instanceAnimation.Playing ? "STOP" : "PLAY");
        PlateVisible = instance == null || instanceIsPreview;
    }

    // Unload currently loaded instance.
    // May be safely called even when instance is already unloaded.
    // LoadInstance() calls this automatically at the beginning.
    private void UnloadInstance(bool refreshUi = true)
    {
        // restore Decorations to be child of our GameObject
        Decorations.transform.parent = transform;

        // First release previous instance
        if (instance != null) {
            // Workaround: make sure that BoundingBoxRig.DetachAppBar is called, AppBar clone will no longer be updated
            instance.GetComponent<BoundingBoxRig>().DetachAppBar();

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
        instanceTransformation.transform.localPosition = -b.center * scale + new Vector3(0f, 2f, 0f);

        // This way dragging the animated model will also drag the decorations
        Decorations.transform.parent = instance.transform;

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
