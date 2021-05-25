using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.Extensions;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using MRTKExtensions.Utilities;

public class SpatialPlacement
{
    private TapToPlace tapToPlace;

    public SpatialPlacement(TapToPlace tapToPlace_)
    {
        tapToPlace = tapToPlace_;
    }

    public void StartAnchoring()
    {
        Enable();
        tapToPlace.enabled = true;
        tapToPlace.StartPlacement();
    }

    public void FinishAnchoring()
    {
        Disable();
        tapToPlace.StopPlacement();
        tapToPlace.enabled = false;
    }
    public Vector3? findPosition()
    {
        float _maxDistance = 3;

#if !UNITY_EDITOR
        var foundPosition = LookingDirectionHelpers.GetPositionOnSpatialMap(_maxDistance);
#else
        Vector3? foundPosition = LookingDirectionHelpers.GetPositionInLookingDirection(_maxDistance);
#endif
        Debug.Log("Found position: " + foundPosition.ToString());
        return foundPosition;
    }


    public void Enable()
    {
        if (CoreServices.SpatialAwarenessSystem != null && !IsObserverRunning)
        {
            CoreServices.SpatialAwarenessSystem.ResumeObservers();
            Debug.Log("Found position result: " + findPosition());
        }
    }

    public void Disable()
    {
        if (CoreServices.SpatialAwarenessSystem != null && IsObserverRunning)
        {
            CoreServices.SpatialAwarenessSystem.SuspendObservers();
            CoreServices.SpatialAwarenessSystem.ClearObservations();
        }
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
