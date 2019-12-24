using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomFloorCaptureController : MonoBehaviour
{
    public Text UI_Snackbar;
    //public ASLFloorController floorControl;
    public FloorIllusionController floorControl;
    public GameObject FloorSpawnPrefab;
    public GameObject m_MainFloorPlane;
    public Camera m_Camera;
    //UI to turn off for picture capture
    public GameObject UIone;
    public GameObject UItwo;
   // public GameObject UIthree;
   // public GameObject UIfour;


    private float m_SizeOfCap = 1;
    private Vector3 m_PosOfCap;
    private Quaternion m_RotOfCap;


    void ToggleUI(bool mode)
    {
        UIone.gameObject.SetActive(mode);
        UItwo.gameObject.SetActive(mode);
   //     UIthree.gameObject.SetActive(mode);
   //     UIfour.gameObject.SetActive(mode);
    }

    public void CaptureScreen(float size)
    {
        m_SizeOfCap = size;
        StartCoroutine(CaptureFrameForFloor());
    }

    IEnumerator CaptureFrameForFloor()
    {
        ToggleUI(false);
        yield return new WaitForEndOfFrame();

        float capinv = 1 - m_SizeOfCap;

        m_PosOfCap = m_Camera.transform.position;
        m_RotOfCap = m_Camera.transform.rotation;

        Texture2D m_Image2DProjector = new Texture2D((int)(Screen.width * m_SizeOfCap), (int)(Screen.height * m_SizeOfCap), TextureFormat.ARGB32, false);
        m_Image2DProjector.ReadPixels(new Rect((float)(capinv / 2) * Screen.width, (float)(capinv / 2) * Screen.height, Screen.width * m_SizeOfCap, Screen.height * m_SizeOfCap), 0, 0);

        ToggleUI(true);
        int layer_mask = LayerMask.GetMask("MainPlane");

        //Raycast Center
        Ray rayCenter = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit center_hit;
        GameObject center_hitObj;
        Vector3 center_hitPoint = new Vector3(0, 0, 0);
        float center_distance = 0;
        if (Physics.Raycast(rayCenter, out center_hit, 99, layer_mask))
        {
            center_hitObj = center_hit.transform.gameObject;
            center_hitPoint = center_hit.point;
            center_distance = center_hit.distance;

        }


        //Raycast TopLeft
        Ray rayTopLeft = m_Camera.ViewportPointToRay(new Vector3((float)(capinv / 2), 1 - (float)(capinv / 2), 0));
        RaycastHit topleft_hit;
        GameObject topleft_hitObj;
        Vector3 topleft_hitPoint = new Vector3(0, 0, 0);
        float topleft_distance = 0;
        if (Physics.Raycast(rayTopLeft, out topleft_hit, 99, layer_mask))
        {
            topleft_hitObj = topleft_hit.transform.gameObject;
            topleft_hitPoint = topleft_hit.point;
            topleft_distance = topleft_hit.distance;
        }

        //Raycast TopRight
        Ray rayTopRight = m_Camera.ViewportPointToRay(new Vector3(1 - (float)(capinv / 2), 1 - (float)(capinv / 2), 0));
        RaycastHit topright_hit;
        GameObject topright_hitObj;
        Vector3 topright_hitPoint = new Vector3(0, 0, 0);
        float topright_distance = 0;
        if (Physics.Raycast(rayTopRight, out topright_hit, 99, layer_mask))
        {
            topright_hitObj = topright_hit.transform.gameObject;
            topright_hitPoint = topright_hit.point;
            topright_distance = topright_hit.distance;
        }

        //Raycast BotLeft
        Ray rayBotLeft = m_Camera.ViewportPointToRay(new Vector3((float)(capinv / 2), (float)(capinv / 2), 0));
        RaycastHit botleft_hit;
        GameObject botleft_hitObj;
        Vector3 botleft_hitPoint = new Vector3(0, 0, 0);
        float botleft_distance = 0;
        if (Physics.Raycast(rayBotLeft, out botleft_hit, 99, layer_mask))
        {
            botleft_hitObj = botleft_hit.transform.gameObject;
            botleft_hitPoint = botleft_hit.point;
            botleft_distance = botleft_hit.distance;
        }

        //Raycast BotRight
        Ray rayBotRight = m_Camera.ViewportPointToRay(new Vector3(1 - (float)(capinv / 2), (float)(capinv / 2), 0));
        RaycastHit botright_hit;
        GameObject botright_hitObj;
        Vector3 botright_hitPoint = new Vector3(0, 0, 0);
        float botright_distance = 0;
        if (Physics.Raycast(rayBotRight, out botright_hit, 99, layer_mask))
        {
            botright_hitObj = botright_hit.transform.gameObject;
            botright_hitPoint = botright_hit.point;
            botright_distance = botright_hit.distance;
        }

        //Raycast Left2
        Ray rayLeft2 = m_Camera.ViewportPointToRay(new Vector3(0 - (float)(capinv / 2), .66666f - (float)(capinv / 2), 0));
        RaycastHit Left2_hit;
        GameObject Left2_hitObj;
        Vector3 Left2_hitPoint = new Vector3(0, 0, 0);
        float Left2_distance = 0;
        if (Physics.Raycast(rayLeft2, out Left2_hit, 99, layer_mask))
        {
            Left2_hitObj = Left2_hit.transform.gameObject;
            Left2_hitPoint = Left2_hit.point;
            Left2_distance = Left2_hit.distance;
        }

        //Raycast Left3
        Ray rayLeft3 = m_Camera.ViewportPointToRay(new Vector3(0 - (float)(capinv / 2), .33333f - (float)(capinv / 2), 0));
        RaycastHit Left3_hit;
        GameObject Left3_hitObj;
        Vector3 Left3_hitPoint = new Vector3(0, 0, 0);
        float Left3_distance = 0;
        if (Physics.Raycast(rayLeft3, out Left3_hit, 99, layer_mask))
        {
            Left3_hitObj = Left3_hit.transform.gameObject;
            Left3_hitPoint = Left3_hit.point;
            Left3_distance = Left3_hit.distance;
        }
  
        //Raycast Right2
        Ray rayRight2 = m_Camera.ViewportPointToRay(new Vector3(1 - (float)(capinv / 2), .66666f - (float)(capinv / 2), 0));
        RaycastHit Right2_hit;
        GameObject Right2_hitObj;
        Vector3 Right2_hitPoint = new Vector3(0, 0, 0);
        float Right2_distance = 0;
        if (Physics.Raycast(rayRight2, out Right2_hit, 99, layer_mask))
        {
            Right2_hitObj = Right2_hit.transform.gameObject;
            Right2_hitPoint = Right2_hit.point;
            Right2_distance = Right2_hit.distance;
        }

        //Raycast Right3
        Ray rayRight3 = m_Camera.ViewportPointToRay(new Vector3(1 - (float)(capinv / 2), .33333f - (float)(capinv / 2), 0));
        RaycastHit Right3_hit;
        GameObject Right3_hitObj;
        Vector3 Right3_hitPoint = new Vector3(0, 0, 0);
        float Right3_distance = 0;
        if (Physics.Raycast(rayRight3, out Right3_hit, 99, layer_mask))
        {
            Right3_hitObj = Right3_hit.transform.gameObject;
            Right3_hitPoint = Right3_hit.point;
            Right3_distance = Right3_hit.distance;
        }

        Debug.Log("Device Center: " + center_hitPoint);
        Debug.Log("Device Cam corners: " + topleft_hitPoint + ", " + topright_hitPoint + ", " + botleft_hitPoint + ", " + botright_hitPoint);
        
        floorControl.InitializeObjects();

    }
}
