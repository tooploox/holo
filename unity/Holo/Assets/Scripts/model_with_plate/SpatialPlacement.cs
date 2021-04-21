using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.Extensions;

using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using MRTKExtensions.Utilities;

public class SpatialPlacement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Check raycast hit each time transformation is changed
    }


    public bool validatePosition()
    {
        float _maxDistance = 3;

#if !UNITY_EDITOR
        var foundPosition = LookingDirectionHelpers.GetPositionOnSpatialMap(_maxDistance);
#else
        Vector3? foundPosition = LookingDirectionHelpers.GetPositionInLookingDirection(_maxDistance);
#endif
        Debug.Log("Found position: " + foundPosition.ToString());
        return foundPosition != null;
    }


    void OnEnable()
    {
        if (CoreServices.SpatialAwarenessSystem != null && !IsObserverRunning)
        {
            CoreServices.SpatialAwarenessSystem.ResumeObservers();
            Debug.Log("Validate position result: " + validatePosition());
        }
    }

    private void OnDisable()
    {
        if (CoreServices.SpatialAwarenessSystem != null && IsObserverRunning)
        {
            CoreServices.SpatialAwarenessSystem.SuspendObservers();
            CoreServices.SpatialAwarenessSystem.ClearObservations();
        }
    }

    void OnTransformParentChanged()
    {
        if (!gameObject.activeSelf) return;
    }

    private bool IsObserverRunning
    {
        get
        {
            var providers =
                ((IMixedRealityDataProviderAccess)CoreServices.SpatialAwarenessSystem)
                .GetDataProviders<IMixedRealitySpatialAwarenessObserver>();
            return providers.FirstOrDefault()?.IsRunning == true;
        }
    }
}
