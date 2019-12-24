using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StressTesting
{
    /// <summary>
    /// Used to hold the starting color of the object this script is attached to - allowing switch back to this color when no one is selecting it
    /// </summary>
    public class StressTest_150_ASLObjects_ObjectColorSupport : MonoBehaviour
    {
        /// <summary>The original color of the object this class gets assigned to</summary>
        public Color m_MyObjectOriginalColor;
        // Start is called before the first frame update
        void Start()
        {
            m_MyObjectOriginalColor = transform.GetComponent<Renderer>().material.color;
        }

    }
}