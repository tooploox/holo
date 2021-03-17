using System;
using System.Collections.Generic;
using UnityEngine;

/* All layers of a loaded (instantiated) model.
   This connects ModelLayer (constant, derived from model description)
   with LayerLoaded (which contains Unity information specific to given instance
   of the model).
*/
public class LayersLoaded : Dictionary<ModelLayer, LayerLoaded>
{
}
