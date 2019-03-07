//Using C#

using UnityEngine;
using System.Collections;

public class BlendShapeAnimation : MonoBehaviour
{
    int blendShapeCount;
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    
    int currentIndex = 0;
    int indexStep = 1;
    public int updateInterval;
    int updateCounter = 0;

    public bool mirrorAnimation = false;

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
        if (updateCounter++ < updateInterval) return;
        updateCounter = 0;

        if (mirrorAnimation) UpdateMirror();
        else UpadteCyclicWithLastZero();
    }

    void UpdateMirror()
    {
        if (currentIndex == blendShapeCount -1)
            indexStep = -1;

        if (currentIndex == 0)
            indexStep = 1;

        int zeroIndex = currentIndex;
        currentIndex += indexStep;

        skinnedMeshRenderer.SetBlendShapeWeight(currentIndex, 100f);
        skinnedMeshRenderer.SetBlendShapeWeight(zeroIndex, 0.0f);
    }

    void UpdateCyclic()
    {
        int zeroIndex = currentIndex;
        currentIndex = (currentIndex + 1) % blendShapeCount;

        skinnedMeshRenderer.SetBlendShapeWeight(currentIndex, 100f);
        skinnedMeshRenderer.SetBlendShapeWeight(zeroIndex, 0.0f);
    }

    void UpadteCyclicWithLastZero()
    {
        int cycleCount = blendShapeCount + 1;
        int zeroIndex = currentIndex;

        currentIndex = (currentIndex + 1) % cycleCount;
        if(currentIndex < blendShapeCount)
            skinnedMeshRenderer.SetBlendShapeWeight(currentIndex, 100f);
        if(zeroIndex < blendShapeCount)
            skinnedMeshRenderer.SetBlendShapeWeight(zeroIndex, 0.0f);
       
    }
}