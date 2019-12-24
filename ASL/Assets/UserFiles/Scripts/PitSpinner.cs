using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitSpinner : MonoBehaviour
{
    public float speed = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles += new Vector3 (0,Time.deltaTime * speed,0);
    }
}
