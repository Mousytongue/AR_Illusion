using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    /// <summary>Used to remember what color this object should be when not selected</summary>
    public class AR_CSS451_Mp2_ColorSupport : MonoBehaviour
    {

        public Color m_OrgObjColor = Color.white; // remember obj's original color

        // Start is called before the first frame update
        void Start()
        {
            //Grab color right away
            m_OrgObjColor = transform.GetComponent<Renderer>().material.color; // save a copy
        }
    }
}