using System;
using System.Collections.Generic;
using UnityEngine;

/* A single layer of loaded (instantiated) model. */
public class LayerLoaded
{
    // Instance on scene.
    public GameObject Instance;
    // Shortcut for Instance.GetComponent<BlendShapeAnimation>().
    // May be null for layers not animated using BlendShapeAnimation.
    public BlendShapeAnimation Animation;
}
