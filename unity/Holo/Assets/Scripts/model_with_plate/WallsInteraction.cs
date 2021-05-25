using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallsInteraction : MonoBehaviour
{
    public GameObject CollisionIndicator;
    public Material CollisionMaterial;
    public Material DefautlMaterial;
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
        Material currentMat = CollisionIndicator.GetComponent<Renderer>().material;
        if (currentMat != DefautlMaterial) 
            CollisionIndicator.GetComponent<Renderer>().material = DefautlMaterial;
    }

    private void OnCollisionEnter(Collision collision)
    { 
        Debug.Log("PLATE collision enttred / Collision pooints: " + collision.contactCount);

        if(collision.contactCount > 0)
        {
            Material currentMat = CollisionIndicator.GetComponent<Renderer>().material;
            if(currentMat != CollisionMaterial) 
                CollisionIndicator.GetComponent<Renderer>().material = CollisionMaterial;
        }

        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
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
