using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectorFieldScaler : MonoBehaviour
{
    public Slider m_Slider;
    public Text m_Text;

    public GameObject Proj1;
    public GameObject Proj2;
    public GameObject Proj3;
    public GameObject Proj4;
    public GameObject Proj5;
    public GameObject Proj6;
    public GameObject Proj7;
    public GameObject Proj8;
    public GameObject Proj9;

    List<GameObject> Projectors = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Projectors.Add(Proj1);
        Projectors.Add(Proj2);
        Projectors.Add(Proj3);
        Projectors.Add(Proj4);
        Projectors.Add(Proj5);
        Projectors.Add(Proj6);
        Projectors.Add(Proj7);
        Projectors.Add(Proj8);
        Projectors.Add(Proj9);


        m_Slider.onValueChanged.AddListener(delegate { SliderChanged(m_Slider.value); });
    }

    void SliderChanged(float value)
    {
        m_Text.text = "" + value;

        foreach (GameObject obj in Projectors)
        {
            obj.GetComponent<Projector>().fieldOfView = value;
        }
    }
}
