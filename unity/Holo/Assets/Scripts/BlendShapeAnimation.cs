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

    /* Should animation be played */
    private bool playing = true;

    /* Public fields, configurable from Unity Editor */
    public bool mirrorAnimation = false;
    public float speed = 1f;

    public bool GetPlayingStatus() { return playing; }

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    void Start()
    {
        blendShapeCount = skinnedMesh.blendShapeCount;        
        Debug.Log("BlendShapes count: " + blendShapeCount);

        // Make sure all blend shapes start with weight 0, in case user was playing around with them in Unity Editor.
        for (int i = 0; i < blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
        }

        mirrorMaxCurrentIndex = blendShapeCount - 1f;
    }

    public void TogglePlay()
    {
        playing = !playing;
    }

    private void Update()
    {
        if (playing)
        {
            if (mirrorAnimation) UpdateMirror();
            else UpdateCyclic();
        }
    }

    void UpdateMirror()
    {        
        if (mirrorIncreasing)
        {
            currentIndex += Time.deltaTime * speed;
            if (currentIndex > mirrorMaxCurrentIndex)
            {
                mirrorIncreasing = false;
                currentIndex = mirrorMaxCurrentIndex - (currentIndex - mirrorMaxCurrentIndex);
            }
        }
        else
        {
            currentIndex -= Time.deltaTime * speed;
            if (currentIndex < 0f)
            {
                mirrorIncreasing = true;
                currentIndex = -currentIndex;
            }

        }

        UpdateBlendShapes();
    }

    void UpdateCyclic()
    {
        currentIndex += Time.deltaTime * speed;
        currentIndex = Mathf.Repeat(currentIndex, blendShapeCount);

        UpdateBlendShapes();
    }

    /* Update current mesh shape, looking at currentIndex. */
    void UpdateBlendShapes()
    {
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
}
