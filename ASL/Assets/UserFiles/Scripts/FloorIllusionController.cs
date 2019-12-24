using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class FloorIllusionController : MonoBehaviour
{
    public GameObject m_Proj1;
    public GameObject m_Proj2;
    public GameObject m_Proj3;
    public GameObject m_Proj4;
    public GameObject m_Proj5;
    public GameObject m_Proj6;
    public GameObject m_Proj7;
    public GameObject m_Proj8;
    public GameObject m_Proj9;

    public GameObject m_Plane1;
    public GameObject m_Plane2;
    public GameObject m_Plane3;
    public GameObject m_Plane4;
    public GameObject m_Plane5;
    public GameObject m_Plane6;
    public GameObject m_Plane7;
    public GameObject m_Plane8;
    public GameObject m_Plane9;

    public Text UI_Snackbar;
    public FloorIllusionController_Chunk ChunkControl;
    public FloorIllusionController_Pit PitControl;
    public FloorIllusionController_PitMover PitMover;
    static public FloorIllusionController SelfControl;

    public static float FieldOfView = 43.6f;
    public static float AspectRatio = 1.6f;

    private Texture2D m_CurrImage;
    private float m_CapSize;
    private Vector3 m_Pos;
    private Quaternion m_Rot;
    private Vector3 m_Center;

    private int num_ProjWidth = 3;
    private int num_ProjHeight = 3;

    private List<GameObject> m_Projectors = new List<GameObject>();
    private List<GameObject> m_Planes = new List<GameObject>();

    private bool is_Caller = false;



    // Start is called before the first frame update
    void Start()
    {
        SelfControl = this.GetComponent<FloorIllusionController>();

        m_Projectors.Add(m_Proj1);
        m_Projectors.Add(m_Proj2);
        m_Projectors.Add(m_Proj3);
        m_Projectors.Add(m_Proj4);
        m_Projectors.Add(m_Proj5);
        m_Projectors.Add(m_Proj6);
        m_Projectors.Add(m_Proj7);
        m_Projectors.Add(m_Proj8);
        m_Projectors.Add(m_Proj9);

        m_Planes.Add(m_Plane1);
        m_Planes.Add(m_Plane2);
        m_Planes.Add(m_Plane3);
        m_Planes.Add(m_Plane4);
        m_Planes.Add(m_Plane5);
        m_Planes.Add(m_Plane6);
        m_Planes.Add(m_Plane7);
        m_Planes.Add(m_Plane8);
        m_Planes.Add(m_Plane9);

        //Local float callbacks
        for (int i = 0; i < m_Projectors.Count; i++)
        {
            m_Projectors[i].GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(ProjectorFloatFunction);
        }

    }

    public void SetValues(Texture2D image, float size, Vector3 pos, Quaternion rot, Vector3 center)
    {
        m_CurrImage = image;
        m_CapSize = size;
        m_Pos = pos;
        m_Rot = rot;
        m_Center = center;
    }

    public void InitializeObjects()
    {
        PitMover.StopMoving();
        PrepPlanes();
        PrepProjectors();
        SendTexture();
    }

    void PrepPlanes()
    {
        foreach (GameObject obj in m_Planes)
        {
            obj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                //  Debug.Log("Prep plane inside claim");
                obj.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(m_Center);
                // Debug.Log("Prep plane post set");
            });
        }
    }

    void SendTexture()
    {
        SetProjectorTexture(m_Projectors[0]);
        is_Caller = true;
    }

    void PrepProjectors()
    {
        foreach (GameObject obj in m_Projectors)
        {
            float[] key = { 1, 1, 1, 1 };
            obj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                obj.GetComponent<ASL.ASLObject>().SendFloat4(key);
                obj.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(m_Pos);
                obj.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(m_Rot);
            });
        }
    }

    public void SetupProjectors()
    {
        //At this point the projectors are already in their positions
        //Now we limit the field in which the shader is allowed to render


        //This is so only the "picture taker" tells the pit to move
        //Setup Projectors is run locally on every client, 
   
        if (is_Caller)
        {
            PitControl.SetupPit();
            ChunkControl.SetupChunks();
        }

        List<int[]> ListOfInts = new List<int[]>();
        int texWidth = m_CurrImage.width / num_ProjWidth;
        int texHeight = m_CurrImage.height / num_ProjHeight;
        for (int i = 0; i < num_ProjWidth; i++)
        {
            for (int j = 0; j < num_ProjHeight; j++)
            {
                int sX = j * texWidth;
                int eX = (j * texWidth) + texWidth;
                int sY = i * texHeight;
                int eY = (i * texHeight) + texHeight;

                int[] newSizes = { sX, eX, sY, eY };
                ListOfInts.Add(newSizes);
            }
        }

        for (int i = 0; i < 9; i++)
        {
            int spot = 0;
            //Projector Spot is the render order, 8 you move first
            if (i == 0)
                spot = 8;
            else if (i == 1)
                spot = 5;
            else if (i == 2)
                spot = 2;
            else if (i == 3)
                spot = 1;
            else if (i == 4)
                spot = 0;
            else if (i == 5)
                spot = 3;
            else if (i == 6)
                spot = 6;
            else if (i == 7)
                spot = 7;
            else if (i == 8)
                spot = 4;

            Projector Proj = m_Projectors[i].GetComponent<Projector>();
            Material mat = new Material(Shader.Find("Projector/AlphaBlend"));
            Proj.material = mat;
            int[] size = ListOfInts[spot];
            Proj.material.SetTexture("_ShadowTex", m_CurrImage);
            Proj.material.SetInt("_TextureWidth", m_CurrImage.width);
            Proj.material.SetInt("_TextureHeight", m_CurrImage.height);
            Proj.material.SetInt("_CutoutXStart", size[0]);
            Proj.material.SetInt("_CutoutXEnd", size[1]);
            Proj.material.SetInt("_CutoutYStart", size[2]);
            Proj.material.SetInt("_CutoutYEnd", size[3]);
        }

        if (is_Caller)
        {
            foreach (GameObject obj in m_Projectors)
            {
                obj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    float[] key = { m_CapSize, 0, 88, 99 };
                    obj.GetComponent<ASL.ASLObject>().SendFloat4(key);
                });
            }
            SendRaycasts();
        }


        is_Caller = false;
    }

    public void SendRaycasts()
    {
        //Ensure the projector is in the settings;
        Projector proj = m_Proj1.GetComponent<Projector>();
        proj.fieldOfView = FieldOfView * m_CapSize;
        proj.aspectRatio = AspectRatio;


        int layer_mask = LayerMask.GetMask("MainPlane");

        //Raycast Center
        Ray rayCenter = new Ray(m_Proj1.transform.position, m_Proj1.transform.forward);
        RaycastHit Center_hit;
        GameObject Center_hitObj;
        Vector3 Center_hitPoint = new Vector3(0, 0, 0);
        float Center_distance = 0;
        if (Physics.Raycast(rayCenter, out Center_hit, 99, layer_mask))
        {
            Center_hitObj = Center_hit.transform.gameObject;
            Center_hitPoint = Center_hit.point;
            Center_distance = Center_hit.distance;
        }
    }

    public static void ProjectorFloatFunction(string _id, float[] _myFloats)
    {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject _myObject))
        {
            Projector Proj = _myObject.GetComponent<Projector>();
            if (_myFloats[2] == 88)
            {
                SelfControl.PitMover.StartMovement();
            }
            if (_myFloats[3] == 99)
            {
                Proj.fieldOfView = FieldOfView * _myFloats[0];
                Proj.aspectRatio = AspectRatio;
           
            }
            else
            {
                Proj.material.SetInt("_CutoutXStart", (int)_myFloats[0]);
                Proj.material.SetInt("_CutoutXEnd", (int)_myFloats[1]);
                Proj.material.SetInt("_CutoutYStart", (int)_myFloats[2]);
                Proj.material.SetInt("_CutoutYEnd", (int)_myFloats[3]);
            }
        }      
    }


    public void SetProjectorTexture(GameObject _myObject)
    {
        _myObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            _myObject.GetComponent<ASL.ASLObject>().SendAndSetTexture2D(m_CurrImage, ChangeProjectedTexture, false, true);
            UI_Snackbar.text = "Capture screen sent";
        });
       
    }

    public static void ChangeProjectedTexture(GameObject _myObject, Texture2D m_image)
    {
        SelfControl.m_CurrImage = m_image;
        SelfControl.SetupProjectors();
        SelfControl.UI_Snackbar.text = "Capture screen Recieved";
    }
}
