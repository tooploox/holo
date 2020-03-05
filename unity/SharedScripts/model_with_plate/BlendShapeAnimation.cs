/* Add this to a GameObject with SkinnedMeshRenderer having some blend shapes,
 * with each shape representing consecutive desired look of the mesh.
 * This script will perform a smooth looping animation between all blend shapes.
 */

using UnityEngine;
using System.Collections;

public class BlendShapeAnimation : MonoBehaviour
{
    /* Private fields constant after Start() */
    private int blendShapeCount;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Mesh skinnedMesh;
    private bool mirrorIncreasing;
    private float mirrorMaxCurrentIndex;
    // At any time, only these two blend shapes may have non-zero weights.
    private int lastPreviousShape, lastNextShape;

    /* Current state, may change in each Update() */
    private float currentIndex = 0f;

    /* Public fields, configurable from Unity Editor */
    public bool MirrorAnimation = false;
    // Speed equal 1.0 means that we advance 1 frame (1 blend shape) in 1 second.
    public float Speed = 1f;

    // Speed normalized such that 1.0 means that we make complete animation in 1 second.
    public float SpeedNormalized {
        get
        {
            return blendShapeCount != 0 ? Speed / blendShapeCount : 0f;
        }
        set
        {
            Speed = value * blendShapeCount;
        }
    }

    /* Public fields, but not serialized/configurable from Unity Editor */
    [System.NonSerialized]
    public bool Playing = true;

    /* Call this after creation, before using anything that depends on blend shape count,
     * like CurrentTime or SpeedNormalized.
     * This makes sure that internal blendShapeCount is initialized.
     * Waiting until Start() is called on this component is sometimes not comfortable.
     */
    public void InitializeBlendShapes()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

        blendShapeCount = skinnedMesh.blendShapeCount;
        if (blendShapeCount == 0) {
            Debug.LogWarning("No blendShapes, will not animate");
        }   

        // Make sure all blend shapes start with weight 0, in case user was playing around with them in Unity Editor.
        for (int i = 0; i < blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
        }

        mirrorMaxCurrentIndex = blendShapeCount - 1f;
    }

    private void Update()
    {
        if (Playing) {
            if (MirrorAnimation) {
                UpdateMirror();
            } else {
                UpdateCyclic();
            }
        }
    }

    private void UpdateMirror()
    {        
        if (mirrorIncreasing)
        {
            currentIndex += Time.deltaTime * Speed;
            if (currentIndex > mirrorMaxCurrentIndex)
            {
                mirrorIncreasing = false;
                currentIndex = mirrorMaxCurrentIndex - (currentIndex - mirrorMaxCurrentIndex);
            }
        }
        else
        {
            currentIndex -= Time.deltaTime * Speed;
            if (currentIndex < 0f)
            {
                mirrorIncreasing = true;
                currentIndex = -currentIndex;
            }

        }

        UpdateBlendShapes();
    }

    private void UpdateCyclic()
    {
        currentIndex += Time.deltaTime * Speed;
        currentIndex = Mathf.Repeat(currentIndex, blendShapeCount);

        UpdateBlendShapes();
    }

    /* Update current mesh shape, looking at currentIndex. */
    private void UpdateBlendShapes()
    {
        if (blendShapeCount == 0) {
            return;
        }

        skinnedMeshRenderer.SetBlendShapeWeight(lastPreviousShape, 0f);
        skinnedMeshRenderer.SetBlendShapeWeight(lastNextShape, 0f);

        int previousShape = (int)currentIndex;
        float frac = currentIndex - previousShape;
        int nextShape = previousShape + 1;
        if (nextShape > blendShapeCount - 1)
        {
            nextShape = 0;
        }

        skinnedMeshRenderer.SetBlendShapeWeight(previousShape, 100f * (1f - frac));
        skinnedMeshRenderer.SetBlendShapeWeight(nextShape, 100f * frac);

        lastPreviousShape = previousShape;
        lastNextShape = nextShape;
    }

    private float MaxCurrentIndex()
    {
        return MirrorAnimation ? mirrorMaxCurrentIndex : blendShapeCount;
    }

    // Current time in animation, in range 0..1.
    public float CurrentTime
    {
        get
        {
            return currentIndex / MaxCurrentIndex();
        }
        set
        {
            currentIndex = MaxCurrentIndex() * value;
            if (!Playing) { // otherwise, newly set currentIndex would not be visible
                UpdateBlendShapes();
            }
        }
    }

    public void TogglePlay()
    {
        Playing = !Playing;
    }
}
