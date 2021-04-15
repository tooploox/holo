using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace MRTKExtensions.Utilities
{
    public static class LookingDirectionHelpers
    {
        //public static Vector3 GetPositionInLookingDirection(float maxDistance = 2)
        //{
        //    var hitPoint = GetPositionOnSpatialMap(maxDistance);

        //    return hitPoint ?? CalculatePositionDeadAhead(maxDistance);
        //}

        //private static int _meshPhysicsLayer = 0;

        //private static int GetSpatialMeshMask()
        //{
        //    if (_meshPhysicsLayer == 0)
        //    {
        //        var spatialMappingConfig = CoreServices.SpatialAwarenessSystem.ConfigurationProfile as
        //            MixedRealitySpatialAwarenessSystemProfile;
        //        if (spatialMappingConfig != null)
        //        {
        //            foreach (var config in spatialMappingConfig.ObserverConfigurations)
        //            {
        //                var observerProfile = config.ObserverProfile
        //                    as MixedRealitySpatialAwarenessMeshObserverProfile;
        //                if (observerProfile != null)
        //                {
        //                    _meshPhysicsLayer |= (1 << observerProfile.MeshPhysicsLayer);
        //                }
        //            }
        //        }
        //    }

        //    return _meshPhysicsLayer;
        //}

        //public static Vector3? GetPositionOnSpatialMap(float maxDistance = 2)
        //{
        //    RaycastHit hitInfo;
        //    var transform = CameraCache.Main.transform;
        //    var headRay = new Ray(transform.position, transform.forward);
        //    if (Physics.Raycast(headRay, out hitInfo, maxDistance, GetSpatialMeshMask()))
        //    {
        //        return hitInfo.point;
        //    }
        //    return null;
        //}

        //public static Vector3 CalculatePositionDeadAhead(float distance = 2)
        //{
        //    return CameraCache.Main.transform.position +
        //           CameraCache.Main.transform.forward.normalized * distance;
        //}
    }
}
