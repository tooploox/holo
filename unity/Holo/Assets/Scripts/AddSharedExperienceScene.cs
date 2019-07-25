using UnityEngine;
using UnityEngine.SceneManagement;

public class AddSharedExperienceScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("SharedExperienceAdditions", LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
