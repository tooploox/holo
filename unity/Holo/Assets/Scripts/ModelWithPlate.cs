using UnityEngine;
using HoloToolkit.Unity.UX;

public class ModelWithPlate : MonoBehaviour, IClickHandler
{
    private bool playing = true;

    public TextMesh PlayOrStopText;
    public GameObject Decorations;
    public GameObject ModelButtons;

    private void Start()
    {
        ModelButtons.SetActive(instance != null); // always false at Start
    }

    public void Click(GameObject clickObject)
    {
        switch (clickObject.name)
        {
            case "TogglePlay": ClickTogglePlay(); break;
            case "Rewind": ClickRewind(); break;
            case "Remove":
                {
                    InstancePath = null;
                    break;
                }
            default:
                {
                    const string addPrefix = "Add_";
                    if (clickObject.name.StartsWith(addPrefix)) {
                        InstancePath = "Models/" + clickObject.name.Substring(addPrefix.Length);
                    } else {
                        Debug.LogWarning("Click on unknown object " + clickObject.name); 
                    }
                    break;
                }
        }
    }

    public void ClickTogglePlay()
    {
        playing = !playing;
        PlayOrStopText.text = (playing ? "STOP" : "PLAY");
        if (blendShapeAnimation != null)
        {
            blendShapeAnimation.Playing = playing;
        }
    }

    public void ClickRewind()
    {
        if (blendShapeAnimation != null)
        {
            blendShapeAnimation.CurrentTime = 0f;
        }
    }

    // All variables below are non-null if InstancePath was set.
    private BlendShapeAnimation blendShapeAnimation;
    private GameObject instance;
    private GameObject instanceTransformation;

    public GameObject Instance { get { return instance; } }

    private string instancePath;

    // Currently loaded animated shape.
    // This is a path to an object (prefab, fbx etc. -- anything that can be loaded as GameObject),
    // relative to Assets/Resources/ .
    // Set to "" to unload.
    public string InstancePath
    {
        get { return instancePath; }
        set
        {
            // restore Decorations to be child of our GameObject
            Decorations.transform.parent = transform;

            // First release previous instance
            if (instance != null)
            {
                // Workaround: make sure that BoundingBoxRig is called, AppBar clone will no longer be updated
                instance.GetComponent<BoundingBoxRig>().DetachAppBar();

                Destroy(instance);
                Destroy(instanceTransformation);

                instance = null;
                instanceTransformation = null;
                blendShapeAnimation = null;
            }

            if (!string.IsNullOrEmpty(value))
            {
                // TODO: first Resources.Load takes a bit
                GameObject template = Resources.Load<GameObject>(value);
                if (template == null)
                {
                    throw new System.Exception("Cannot load GameObject from " + value);
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
                if (maxSize > Mathf.Epsilon)
                {
                    scale = 2 / maxSize;
                }
                instanceTransformation.transform.localScale = new Vector3(scale, scale, scale);
                instanceTransformation.transform.localPosition = -b.center * scale + new Vector3(0f, 2f, 0f);

                // This way dragging the animated model will also drag the decorations
                Decorations.transform.parent = instance.transform;

                blendShapeAnimation = instance.GetComponent<BlendShapeAnimation>();
                if (blendShapeAnimation == null)
                {
                    // TODO: this should be changed into "throw new...", not a warning.
                    // We should settle whether the stored object has or has not BlendShapeAnimation (I vote not,
                    // but any decision is fine, just stick to it).
                    // After new import STL->GameObject is ready.
                    Debug.LogWarning("BlendShapeAnimation component not found inside " + value + ", adding");
                    blendShapeAnimation = instance.AddComponent<BlendShapeAnimation>();
                }
                // default speed to play in 1 second
                int blendShapesCount = skinnedMesh.sharedMesh.blendShapeCount;
                if (blendShapesCount != 0) {
                    blendShapeAnimation.Speed = skinnedMesh.sharedMesh.blendShapeCount;
                } else {
                    Debug.LogWarning("Model has no blend shapes " + value);
                }
                blendShapeAnimation.Playing = playing;
                // TODO: this should be changed into "throw new...", not a warning.
                // After new import STL->GameObject is ready.
                Animator animator = instance.GetComponent<Animator>();
                if (animator != null)
                {
                    Debug.LogWarning("Animator component found but not wanted inside " + value + ", removing");
                    Destroy(animator);
                }
            }

            ModelButtons.SetActive(instance != null);
            instancePath = value;
        }
    }

    // TODO update time slider now
    /*
    private void Update()
    {
        if (playing && blendShapeAnimation != null)
        {
            timeSlider.value = blendShapeAnimation.CurrentTime;
        }
    }
    */

    // TODO: read time slider now
    /*
    private void TimeSliderValueChanged(float newPosition)
    {
        if (blendShapeAnimation != null)
        {
            blendShapeAnimation.CurrentTime = newPosition;
        }
    }
    */
}
