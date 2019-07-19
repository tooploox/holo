using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Information about layer.
 * TODO: It should be present in asset bundle for each GameObject representing a layer.
 * For now, we "guess" and add it based on layer GameObject name.
 */
public class ModelLayer : MonoBehaviour
{
    // Nice name to show to user.
    public string Caption;

    // Is this a simulation layer (using simulation shader etc.)
    public bool Simulation;

    public GameObject InstantiateGameObject(Transform parent)
    {
        GameObject template = gameObject;
        GameObject instance = UnityEngine.Object.Instantiate<GameObject>(template, parent);
        return instance;
    }
}
