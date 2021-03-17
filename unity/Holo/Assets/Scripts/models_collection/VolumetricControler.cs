using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumetricControler : MonoBehaviour
{
    public string DataPath;
    private VolumetricLoader loader;

    // Start is called before the first frame update
    void Start()
    {
        loader = this.gameObject.GetComponent<VolumetricLoader>();
        loader.LoadRawDataFromFile(DataPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
