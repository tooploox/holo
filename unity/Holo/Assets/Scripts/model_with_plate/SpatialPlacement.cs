using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;

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

    void OnEnable()
    {
        if (CoreServices.SpatialAwarenessSystem != null && !IsObserverRunning)
        {
            CoreServices.SpatialAwarenessSystem.ResumeObservers();
        }
    }

    private void OnDisable()
    {
        if(CoreServices.SpatialAwarenessSystem != null && IsObserverRunning)
        {
            CoreServices.SpatialAwarenessSystem.SuspendObservers();
            CoreServices.SpatialAwarenessSystem.ClearObservations();
        }
    }

    void OnTransformParentChanged()
    {
        if(!gameObject.activeSelf) return;
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
