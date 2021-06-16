using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallsInteraction : MonoBehaviour
{
    public GameObject CollisionIndicator;
    public Material CollisionMaterial;
    public Material DefautlMaterial;

    private bool InCollision = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
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
    private void OnTriggerEnter(Collider other)
    {
        if (!InCollision)
        {
            InCollision = true;
            ChangeMatterial(CollisionMaterial);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (InCollision)
        {
            InCollision = false;
            ChangeMatterial(DefautlMaterial);
        }
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
