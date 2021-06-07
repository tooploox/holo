using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallsInteraction : MonoBehaviour
{
    public GameObject CollisionIndicator;
    public Material CollisionMaterial;
    public Material DefautlMaterial;
    public Material CurrentMaterial;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionExit(Collision collision)
    {
        ChangeMatterial(DefautlMaterial);
    }

    private void ChangeMatterial(Material mat)
    {
        var renderer = CollisionIndicator.GetComponent<Renderer>();

        if(renderer.materials.Length > 1 && renderer.materials[0] != mat) {
            var cubeMaterials = renderer.materials;
            cubeMaterials[0] = CollisionMaterial;
            renderer.materials = cubeMaterials;
        }
        else if(renderer.material != mat)
        {
            renderer.material = mat;
        }
    }

    private void OnCollisionEnter(Collision collision)
    { 
        //Debug.Log("PLATE collision enttred / Collision pooints: " + collision.contactCount);
        ChangeMatterial(CollisionMaterial);
        // GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
    }

    private void OnEnable()
    {
        CollisionIndicator.SetActive(true);
    }

    void OnDisable()
    {
        CollisionIndicator.SetActive(false);
    }
}
