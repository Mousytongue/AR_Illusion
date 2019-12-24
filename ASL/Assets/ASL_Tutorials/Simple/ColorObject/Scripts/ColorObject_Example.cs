using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDemos
{
    /// <summary>Example of how to color an object</summary>
    public class ColorObject_Example : MonoBehaviour
    {
        /// <summary>Provides an easy way to access the object we want to change the color on. </summary>
        public GameObject m_MyObjectToColor;
        /// <summary>The color that the user will set the object for themselves</summary>
        public Color m_MyColor;
        /// <summary>The color that the user will set the object for other users</summary>
        public Color m_OpponentsColor;

        /// <summary>
        /// Bool toggling the color send. It will automatically be set to false after it is set to true to save bandwidth
        /// </summary>
        public bool m_SendColor = false;

        /// <summary>
        /// Initializes our colors to white to match our object
        /// </summary>
        private void Start()
        {
            m_MyColor = Color.white;
            m_OpponentsColor = Color.white;
        }

        /*For more examples go to https://uwb-arsandbox.github.io/ASL/ASLDocumentation/Help/html/fc89a4a7-90ab-c4bb-6faa-0290adc58274.htm */

        /// <summary>
        /// Our game logic
        /// </summary>
        void Update()
        {
            if (m_SendColor)
            {
                m_MyObjectToColor.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    m_MyObjectToColor.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(m_MyColor, m_OpponentsColor);
                });
                m_SendColor = false;
            }
        }
    }
}