using UnityEngine;

/** Information about layer.
 * It should be present in asset bundle for each GameObject representing a layer.
 * The script is also present in the holo-preprocessing app with the same name.
 */

public enum DataType
{
    Mesh,
    Volumetric
}

public class ModelLayer : MonoBehaviour
{
    // Nice name to show to user.
    public string Caption;

    // Type of data to be visualized
    public DataType DataType;

    // Is this a simulation layer (using simulation shader etc.)
    public bool Simulation;

    // Is this a turbulence layer (using turbulence shader etc.)
    public bool Turbulence;

    // Is this a surface displacement layer (using displacement shader etc.)
    public bool Displacement;

    public bool RGB;

    // Each layer in the model can consecutive number, used to calculate ModelWithPlate.LayerMask.
    public int LayerIndex;

    public GameObject InstantiateGameObject(Transform parent)
    {
        GameObject template = gameObject;
        GameObject instance = UnityEngine.Object.Instantiate<GameObject>(template, parent);
        return instance;
    }
}
