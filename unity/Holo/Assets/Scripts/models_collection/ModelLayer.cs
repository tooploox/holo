using UnityEngine;

public enum DataType
{
    Mesh,
    Volumetric
}

/* Information about layer.
 * It should be present in asset bundle for each GameObject representing a layer.
 */
public class ModelLayer : MonoBehaviour
{
    // Nice name to show to user.
    public string Caption;

    // Data type to be visualized
    public DataType DataType;

    // Is this a simulation layer (using simulation shader etc.)
    public bool Simulation;

    // Each layer in the model can consecutive number, used to calculate ModelWithPlate.LayerMask.
    public int LayerIndex;

    public GameObject InstantiateGameObject(Transform parent)
    {
        GameObject template = gameObject;
        GameObject instance = UnityEngine.Object.Instantiate<GameObject>(template, parent);
        return instance;
    }
}
