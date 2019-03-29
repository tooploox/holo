using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using HoloToolkit.Unity.UX;

public class ModelInstance : MonoBehaviour, IClickHandler
{
    private bool playing = true;

    public TextMesh PlayOrStopText;
    public GameObject Decorations;

    public void Click(GameObject clickObject)
    {
        switch (clickObject.name)
        {
            case "Play": ClickTogglePlay(); break;
            case "Rewind": ClickRewind(); break;
            case "Remove": ClickRemove(); break;
            default: Debug.LogWarning("Click on unknown object " + clickObject.name); break;
        }
    }

    public void ClickTogglePlay()
    {
        playing = !playing;
        PlayOrStopText.text = (playing ? "STOP" : "PLAY");
        if (blendShapeAnimation != null)
        {
            blendShapeAnimation.TogglePlay();
        }
    }

    public void ClickRewind()
    {
        if (blendShapeAnimation != null)
        {
            blendShapeAnimation.CurrentTime = 0f;
        }
    }

    public void ClickRemove()
    {
        ModelsCollection.Singleton.RemoveInstance(this);
    }

    // Only non-null if Template was set.
    private BlendShapeAnimation blendShapeAnimation;
    private GameObject instance;
    private GameObject instanceTransformation;

    public GameObject Instance { get { return instance; } }

    private string template;

    // Currently loaded animated shape.
    // This is a path to an object (prefab, fbx etc. -- anything that can be loaded as GameObject),
    // relative to Assets/Resources/ .
    // Set to "" to unload.
    public string Template
    {
        get { return template; }
        set
        {
            // First release previous instance
            if (instance != null)
            {
                Destroy(instance);
                Destroy(instanceTransformation);
                instance = null;
                instanceTransformation = null;
                blendShapeAnimation = null;
            }

            if (!string.IsNullOrEmpty(value))
            {
                GameObject template = Resources.Load<GameObject>(value);
                if (template == null)
                {
                    throw new System.Exception("Cannot load GameObject from " + value);
                }

                instanceTransformation = new GameObject("InstanceTransformation");

                // TODO: do not do Resources.Load each time, keep a list of loaded GameObjects in ModelsCollection.Templates
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
                instanceTransformation.transform.localPosition = -b.center * scale + new Vector3(0f, 1f, 0f);
                instanceTransformation.transform.parent = transform;

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
                // TODO: The default Speed 1 should be a good default.
                blendShapeAnimation.Speed = 10f;
                // TODO: this should be changed into "throw new...", not a warning.
                // After new import STL->GameObject is ready.
                Animator animator = instance.GetComponent<Animator>();
                if (animator != null)
                {
                    Debug.LogWarning("Animator component found but not wanted inside " + value + ", removing");
                    Destroy(animator);
                }
            }

            template = value;
        }
    }

    private void Update()
    {
        if (playing && blendShapeAnimation != null)
        {
            // TODO: This is called but doesn't update UI, why?
            // Debug.Log("update slider to " + blendShapeAnimation.CurrentTime.ToString());
            // TODO
            //timeSlider.value = blendShapeAnimation.CurrentTime;
        }
    }

    private void TimeSliderValueChanged(float newPosition)
    {
        if (blendShapeAnimation != null)
        {
            // TODO: This is never called, why?
            // Debug.Log("updated time to " + newPosition.ToString());
            blendShapeAnimation.CurrentTime = newPosition;
        }
    }

    public void DetachFromScene()
    {
        if (instance != null)
        {
            // Workaround: make sure that BoundingBoxRig is called, AppBar clone will no longer be updated
            instance.GetComponent<BoundingBoxRig>().DetachAppBar();

            Destroy(gameObject);

            instance = null;
            instanceTransformation = null;
            blendShapeAnimation = null;
        }
    }
}
