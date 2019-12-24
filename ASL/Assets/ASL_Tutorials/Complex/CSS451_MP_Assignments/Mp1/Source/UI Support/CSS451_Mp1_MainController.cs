using UnityEngine;

namespace Mps
{
    public partial class CSS451_Mp1_MainController : MonoBehaviour
    {

        // reference to all UI elements in the Canvas
        public Camera MainCamera = null;
        public CSS451_Mp1_TheWorld TheWorld = null;

        private const float kTaretSize = 0.5f;

        // Use this for initialization
        void Start()
        {
            Debug.Assert(MainCamera != null);
            Debug.Assert(TheWorld != null);
        }

        // Update is called once per frame
        void Update()
        {
            CheckMouseClick();
        }
    }
}