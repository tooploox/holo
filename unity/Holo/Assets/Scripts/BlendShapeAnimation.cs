//Using C#

using UnityEngine;
using System.Collections;

public class BlendShapeAnimation : MonoBehaviour
{
    int blendShapeCount;
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;

    float currentIndex = 0f;

    public bool mirrorAnimation = false;
    public float speed = 1f;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    void Start()
    {
        blendShapeCount = skinnedMesh.blendShapeCount;
        Debug.Log("BlendShapes count: " + blendShapeCount);
    }

    private void Update()
    {
        if (mirrorAnimation) UpdateMirror();
        else UpdateCyclic();
    }

    private bool mirrorIncreasing;

    void UpdateMirror()
    {
        float maxCurrentIndex = blendShapeCount - 1f;
        if (mirrorIncreasing)
        {
            currentIndex += Time.deltaTime * speed;
            if (currentIndex > maxCurrentIndex)
            {
                mirrorIncreasing = false;
                currentIndex = maxCurrentIndex - (currentIndex - maxCurrentIndex);
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
        int previousShape = (int)currentIndex;
        float frac = currentIndex - previousShape;
        int nextShape = previousShape + 1;
        if (nextShape > blendShapeCount - 1)
        {
            nextShape = 0;
        }

        for (int i = 0; i < blendShapeCount; i++)
        {
            float weight;
            if (i == previousShape)
            {
                weight = 100f * (1f - frac);
            }
            else
            if (i == nextShape)
            {
                weight = 100f * frac;
            }
            else
            {
                weight = 0f;
            }
            skinnedMeshRenderer.SetBlendShapeWeight(i, weight);
        }
    }
}
