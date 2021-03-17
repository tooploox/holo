using UnityEngine;

namespace HoloToolkit.Examples.SharingWithUNET
{ 
    public class ModelWithPlateAnchor : MonoBehaviour
    {
        public static ModelWithPlateAnchor Instance;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance == this) return;
                Destroy(Instance.gameObject);
                Instance = this;
            }
        }
    }
}

