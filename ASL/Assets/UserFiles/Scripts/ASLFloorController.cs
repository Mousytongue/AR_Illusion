using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ASLFloorController : MonoBehaviour
{
    //Debugg
    public static Text UI_Snackbar;

    //Proj initial setting
    public static float FieldOfView = 63.5f;
    public static float AspectRatio = 1.6f;

    public static Texture2D m_CurrImage;
    public static GameObject m_CurrProj;

    //Values about the capture
    public static float m_CapSize;
    public static Vector3 m_Pos;
    public static Quaternion m_Rot;
    public static Vector3 m_Center;
    public static Vector3 m_TL;
    public static Vector3 m_TR;
    public static Vector3 m_BL;
    public static Vector3 m_BR;

    //Does the pit need adjusting?
    public static bool m_Adjust;

    //counter
   // public static int m_counter = -1;

    //Reference to the projectors / planes 
   // private static List<GameObject> m_ProjectorObjects = new List<GameObject>();
    //private static List<GameObject> m_ProjPlanes = new List<GameObject>();
   // private static List<Texture2D> m_Images = new List<Texture2D>();
   // private static List<Texture2D> m_recievedImages = new List<Texture2D>();

    //  private static int m_ProjCount = 0;
    // private static int m_ImageCount = 0;
    // public static List<Texture2D> m_Images = new List<Texture2D>();
    //  public static List<Texture2D> m_AlterImages = new List<Texture2D>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValues(Texture2D image, float size, Vector3 pos, Quaternion rot, Vector3 center, Vector3 topLeft, Vector3 topRight, Vector3 botLeft, Vector3 botRight, bool adjust)
    {
       // m_Images.Add(image);

        m_CurrImage = image;

        m_CapSize = size;
        m_Pos = pos;
        m_Rot = rot;
        m_Center = center;
        m_TL = topLeft;
        m_TR = topRight;
        m_BL = botLeft;
        m_BR = botRight;
        m_Adjust = adjust;

        Debug.Log("" + size + pos + rot + center + adjust);
    }

    public void SpawnSingleProjector()
    {
       // m_counter++;
        ASL.ASLHelper.InstanitateASLObject("ProjPlane", m_Center, Quaternion.identity, "", "", SetupProjectorPlane, null, SingleProjPlaneFloat);
        ASL.ASLHelper.InstanitateASLObject("Projector", m_Pos, m_Rot, "", "", SetupSingleProjector, null, SingleProjectorFloatFunction);
       // Invoke("FinishProjectorPlacement", 2f);
    }

    void FinishProjectorPlacement()
    {
        Debug.Log("Finish place called..");

        //First key to change the aspect ratio based on capture size
        float[] key = { m_CapSize, 0, 0, 99 };
        m_CurrProj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            m_CurrProj.GetComponent<ASL.ASLObject>().SendFloat4(key);
        });

        //Sets the render size to the limits of the image
        //Actually .. set it to zero initially, change the limits after the image is spawned .. right? .. hmm
        float xS = 1;
        float xE = m_CurrImage.width;
        float yS = 1;
        float yE = m_CurrImage.height;
        float[] key2 = { xS, xE, yS, yE };
        Debug.Log("Key2: " + xS + ", " + xE + ", " + yS + ", " + yE);
        m_CurrProj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            m_CurrProj.GetComponent<ASL.ASLObject>().SendFloat4(key2);
        });

        
    }

    //ASL function - runs on "caller" only
    public static void SetupProjectorPlane(GameObject _myGameObject)
    {
       // m_ProjPlanes.Add(_myGameObject);
    }

    //ASL function - This will run on all clients when Send4Float is called
    public static void SingleProjPlaneFloat(string _id, float[] _myFloats)
    {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject _myObject))
        {
            //Set Layer of plane
        }
    }

    //ASL function - runs on "caller only"
    public void SetupSingleProjector(GameObject _myGameObject)
    {
        // m_ProjectorObjects.Add(_myGameObject);
        m_CurrProj = _myGameObject;
        SendAndChangeTexture();
    }


    public static void SingleProjectorFloatFunction(string _id, float[] _myFloats)
    {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject _myObject))
        {
            //Invert the projector to hide projection untill texture loads
            if (_myFloats[2] == 88)
            {
                _myObject.transform.forward = -_myObject.transform.forward;
            }
            if (_myFloats[3] == 99)
            {
                Projector Proj = _myObject.GetComponent<Projector>();
               // Material mat = new Material(Shader.Find("Projector/AlphaBlend")) as Material;
               // Proj.material = mat;              
                Proj.fieldOfView = FieldOfView * _myFloats[0];
                Proj.aspectRatio = AspectRatio;
                //Set Projector ignore layers
                Proj.ignoreLayers = 1 << 10;
            }
            else
            {
                //Set shader boundaries here
                Projector Proj = _myObject.GetComponent<Projector>();
                Proj.material.SetInt("_CutoutXStart", (int)_myFloats[0]);
                Proj.material.SetInt("_CutoutXEnd", (int)_myFloats[1]);
                Proj.material.SetInt("_CutoutYStart", (int)_myFloats[2]);
                Proj.material.SetInt("_CutoutYEnd", (int)_myFloats[3]);
            } 
        }
    }

    public void SendAndChangeTexture()
    {
        Debug.Log("Send and Change texture called..");
        m_CurrProj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            m_CurrProj.GetComponent<ASL.ASLObject>().SendAndSetTexture2D(m_CurrImage, ChangeProjectedTexture, false, true);
        });

        Debug.Log("Send and Change texture finsished..");
    }

    //ASL function - runs on all clients.
    public static void ChangeProjectedTexture( GameObject _myGameObject, Texture2D m_CapturedImage)
    {
        //Test

        // Texture2D m_RecievedImage = new Texture2D(m_CapturedImage.width, m_CapturedImage.height);
        // Graphics.CopyTexture(m_CapturedImage, m_RecievedImage);
        //Texture2D m_RecievedImage = m_CapturedImage;

       // m_RecievedImage.wrapMode = TextureWrapMode.Clamp;
        //m_RecievedImage.Apply();
        //m_recievedImages.Add(m_RecievedImage);


       // Material mat = new Material(Shader.Find("Projector/AlphaBlend"));       
        Projector Proj = _myGameObject.GetComponent<Projector>();
        //Proj.material = mat;
        
        Proj.material.SetTexture("_ShadowTex", m_CapturedImage);
        Proj.material.SetInt("_TextureWidth", m_CapturedImage.width);
        Proj.material.SetInt("_TextureHeight", m_CapturedImage.height);
        Proj.material.SetInt("_CutoutXStart", 1);
        Proj.material.SetInt("_CutoutXEnd", 1);
        Proj.material.SetInt("_CutoutYStart", 1);
        Proj.material.SetInt("_CutoutYEnd", 1);
    }
}
