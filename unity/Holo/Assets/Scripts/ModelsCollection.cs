using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ModelsCollection : MonoBehaviour, IClickHandler
{
    static private ModelsCollection singleton;
    static public ModelsCollection Singleton {
        get {
            return singleton;
        }
    }

    void Start()
    {
        if (singleton != null)
        {
            throw new System.Exception("Only one instance of ModelsCollection should exist");
        }
        singleton = this;
    }

    public void Click(GameObject clickObject)
    {
        switch (clickObject.name)
        {
            case "AddModel": ClickAddModel(); break;
            case "RemoveAllModels": ClickRemoveAllModels(); break;
            default: Debug.LogWarning("Click on unknown object " + clickObject.name); break;
        }
    }

    private List<ModelInstance> modelInstances = new List<ModelInstance>();

    public GameObject ModelInstanceTemplate;

    private void ClickAddModel()
    {
        GameObject parent = GameObject.Find("SceneContent");
        GameObject modelInstanceGameObject = Instantiate<GameObject>(ModelInstanceTemplate, parent.transform);
        ModelInstance modelInstance = modelInstanceGameObject.GetComponent<ModelInstance>();
        modelInstance.Template = "Models/animated-model-with-scripts";
        modelInstance.transform.position = transform.position + new Vector3((modelInstances.Count + 1) * 2f, 0f, 0f);

        modelInstances.Add(modelInstance);
    }

    private void ClickRemoveAllModels()
    {
        foreach (ModelInstance modelInstance in modelInstances)
        {
            Destroy(modelInstance.gameObject);
        }
        modelInstances.Clear();
    }

    public void RemoveInstance(ModelInstance modelInstance)
    {
        Destroy(modelInstance.gameObject);
        modelInstances.Remove(modelInstance);
    }
}
