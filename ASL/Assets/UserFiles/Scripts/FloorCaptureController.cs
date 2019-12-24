using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorCaptureController : MonoBehaviour
{
    //UI element for Feedback to user - currently unused
    public Text UI_Snackbar;

    //controllers that handles the projector/planes/ect
    public FloorIllusionController FloorControl;
    public FloorIllusionController_Pit PitControl;
    public FloorIllusionController_Chunk ChunkControl;
    public FloorUIController FloorUIControl;
    public GameObject MainPlane;

    //camera to capture image from
    public Camera m_Camera;

    //UI elements to turn off temporarly while picture is captured
    public GameObject UIone;
    public GameObject UItwo;

    //% of screenspace to be captured - 1 = 100%
    private float m_SizeOfCap = 1;

    //Position and Rotation of camera at the moment of capture
    private Vector3 m_PosOfCap;
    private Quaternion m_RotOfCap;

    //MainPlain Floor y
    float m_FloorY;

    //Toggles UI elements
    void ToggleUI(bool mode)
    {
        UIone.gameObject.SetActive(mode);
        UItwo.gameObject.SetActive(mode);

    }

    public void SetFloorY(float val)
    {
        m_FloorY = val;
    }

    //Called from Initial controller
    public void CaptureScreen(float size)
    {
       // SetMainPlane(true);
        m_SizeOfCap = size;
        ResetObjectsPositions();
        StartCoroutine(CaptureFrameForFloor());
    }

    void ResetObjectsPositions()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("PitObjs");
        foreach (GameObject obj in objects)
        {
            obj.transform.position = new Vector3(0, 100, 0);
        }
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
        //m_Image2DProjector.ReadPixels(new Rect((float)(capinv / 2) * Screen.width, (float)(capinv / 2) * Screen.height, Screen.width - ((float)(capinv / 2) * Screen.width), Screen.height - ((float)(capinv / 2) * Screen.height)), 0, 0);
        ToggleUI(true);
        int layer_mask = LayerMask.GetMask("MainPlane");

        //      1--2--3--4
        //      | 7| 8| 9|
        //      5--6--7--8
        //      | 4| 5| 6|
        //      9-10-11-12
        //      | 1| 2| 3|
        //     13-14-15-16

        float centerX = 0.5f;
        float centerY = 0.5f;
        Vector3 CenterPt = GetRaycastPosition(centerX, centerY);

        float Pt1X = (float)(capinv / 2);
        float Pt1Y = (1 - (float)(capinv / 2));
        Vector3 Pt1 = GetRaycastPosition(Pt1X, Pt1Y);

        float Pt13X = (float)(capinv / 2);
        float Pt13Y = (float)(capinv / 2);
        Vector3 Pt13 = GetRaycastPosition(Pt13X, Pt13Y);

        float Pt5X = (float)(capinv / 2);
        float Pt5Y = ((Pt1Y - Pt13Y) * .66666f) + Pt13Y;
        Vector3 Pt5 = GetRaycastPosition(Pt5X, Pt5Y);

        float Pt9X = (float)(capinv / 2);
        float Pt9Y = ((Pt1Y - Pt13Y) * .33333f) + Pt13Y;
        Vector3 Pt9 = GetRaycastPosition(Pt9X, Pt9Y);

        float Pt4X = (1 - (float)(capinv / 2));
        float Pt4Y = (1 - (float)(capinv / 2));
        Vector3 Pt4 = GetRaycastPosition(Pt4X, Pt4Y);

        float Pt16X = (1 - (float)(capinv / 2));
        float Pt16Y = (float)(capinv / 2);
        Vector3 Pt16 = GetRaycastPosition(Pt16X, Pt16Y);

        float Pt8X = (1 - (float)(capinv / 2));
        float Pt8Y = ((Pt4Y - Pt16Y) * .66666f) + Pt16Y;
        Vector3 Pt8 = GetRaycastPosition(Pt8X, Pt8Y);

        float Pt12X = (1 - (float)(capinv / 2));
        float Pt12Y = ((Pt4Y - Pt16Y) * .33333f) + Pt16Y;
        Vector3 Pt12 = GetRaycastPosition(Pt12X, Pt12Y);

        float Pt2X = ((Pt4X - Pt1X) * 0.33333f) + Pt1X;
        float Pt2Y = (1 - (float)(capinv / 2));
        Vector3 Pt2 = GetRaycastPosition(Pt2X, Pt2Y);

        float Pt3X = ((Pt4X - Pt1X) * 0.66666f) + Pt1X;
        float Pt3Y = (1 - (float)(capinv / 2));
        Vector3 Pt3 = GetRaycastPosition(Pt3X, Pt3Y);

        float Pt14X = ((Pt16X - Pt13X) * 0.33333f) + Pt13X;
        float Pt14Y = (float)(capinv / 2);
        Vector3 Pt14 = GetRaycastPosition(Pt14X, Pt14Y);

        float Pt15X = ((Pt16X - Pt13X) * 0.66666f) + Pt13X;
        float Pt15Y = (float)(capinv / 2);
        Vector3 Pt15 = GetRaycastPosition(Pt15X, Pt15Y);

        float Pt6X = ((Pt8X - Pt5X) * 0.33333f) + Pt5X;
        float Pt6Y = ((Pt2Y - Pt14Y) * 0.66666f) + Pt14Y;
        Vector3 Pt6 = GetRaycastPosition(Pt6X, Pt6Y);

        float Pt7X = ((Pt8X - Pt5X) * 0.66666f) + Pt5X;
        float Pt7Y = ((Pt3Y - Pt15Y) * 0.66666f) + Pt15Y;
        Vector3 Pt7 = GetRaycastPosition(Pt7X, Pt7Y);

        float Pt10X = ((Pt12X - Pt9X) * 0.33333f) + Pt9X;
        float Pt10Y = ((Pt2Y - Pt14Y) * 0.33333f) + Pt14Y;
        Vector3 Pt10 = GetRaycastPosition(Pt10X, Pt10Y);

        float Pt11X = ((Pt12X - Pt9X) * 0.66666f) + Pt9X;
        float Pt11Y = ((Pt3Y - Pt15Y) * 0.33333f) + Pt15Y;
        Vector3 Pt11 = GetRaycastPosition(Pt11X, Pt11Y);

        //      1--2--3--4
        //      | 7| 8| 9|
        //      5--6--7--8
        //      | 4| 5| 6|
        //      9-10-11-12
        //      | 1| 2| 3|
        //     13-14-15-16

        //Jerry rigged adjustment of points
        float t_distance;
        float RightAdj = 1 - FloorUIControl.m_RightAdj;
        float TopAdj = 1 - FloorUIControl.m_TopAdj;
        float LeftAdj = 1 - FloorUIControl.m_LeftAdj;
        float BotAdj = 1 - FloorUIControl.m_BotAdj;

        Vector3 AdjLeft1Pt, AdjLeft2Pt, AdjLeft3Pt, AdjLeft4Pt;
        Vector3 AdjRight1Pt, AdjRight2Pt, AdjRight3Pt, AdjRight4Pt;

        //crunch top to bot
        t_distance = Vector3.Distance(Pt1, Pt13);
        AdjLeft1Pt = Vector3.MoveTowards(Pt1, Pt13, t_distance * TopAdj);

        t_distance = Vector3.Distance(Pt5, Pt13);
        AdjLeft2Pt = Vector3.MoveTowards(Pt5, Pt13, t_distance * TopAdj);

        t_distance = Vector3.Distance(Pt9, Pt13);
        AdjLeft3Pt = Vector3.MoveTowards(Pt9, Pt13, t_distance * TopAdj);

        t_distance = Vector3.Distance(Pt4, Pt16);
        AdjRight1Pt = Vector3.MoveTowards(Pt4, Pt16, t_distance * TopAdj);

        t_distance = Vector3.Distance(Pt8, Pt16);
        AdjRight2Pt = Vector3.MoveTowards(Pt8, Pt16, t_distance * TopAdj);

        t_distance = Vector3.Distance(Pt12, Pt16);
        AdjRight3Pt = Vector3.MoveTowards(Pt12, Pt16, t_distance * TopAdj);

        //crunch bot to top
        t_distance = Vector3.Distance(AdjLeft2Pt, AdjLeft1Pt);
        AdjLeft2Pt = Vector3.MoveTowards(AdjLeft2Pt, AdjLeft1Pt, t_distance * BotAdj);

        t_distance = Vector3.Distance(AdjLeft3Pt, AdjLeft1Pt);
        AdjLeft3Pt = Vector3.MoveTowards(AdjLeft3Pt, AdjLeft1Pt, t_distance * BotAdj);

        t_distance = Vector3.Distance(Pt13, AdjLeft1Pt);
        AdjLeft4Pt = Vector3.MoveTowards(Pt13, AdjLeft1Pt, t_distance * BotAdj);

        t_distance = Vector3.Distance(AdjRight2Pt, AdjRight1Pt);
        AdjRight2Pt = Vector3.MoveTowards(AdjRight2Pt, AdjRight1Pt, t_distance * BotAdj);

        t_distance = Vector3.Distance(AdjRight3Pt, AdjRight1Pt);
        AdjRight3Pt = Vector3.MoveTowards(AdjRight3Pt, AdjRight1Pt, t_distance * BotAdj);

        t_distance = Vector3.Distance(Pt16, AdjRight1Pt);
        AdjRight4Pt = Vector3.MoveTowards(Pt16, AdjRight1Pt, t_distance * BotAdj);

        //crunch right to left
        t_distance = Vector3.Distance(AdjLeft1Pt, AdjRight1Pt);
        AdjRight1Pt = Vector3.MoveTowards(AdjRight1Pt, AdjLeft1Pt, t_distance * RightAdj);

        t_distance = Vector3.Distance(AdjLeft2Pt, AdjRight2Pt);
        AdjRight2Pt = Vector3.MoveTowards(AdjRight2Pt, AdjLeft2Pt, t_distance * RightAdj);

        t_distance = Vector3.Distance(AdjLeft3Pt, AdjRight3Pt);
        AdjRight3Pt = Vector3.MoveTowards(AdjRight3Pt, AdjLeft3Pt, t_distance * RightAdj);

        t_distance = Vector3.Distance(AdjLeft4Pt, AdjRight4Pt);
        AdjRight4Pt = Vector3.MoveTowards(AdjRight4Pt, AdjLeft4Pt, t_distance * RightAdj);

        //chunk left to right
        t_distance = Vector3.Distance(AdjLeft1Pt, AdjRight1Pt);
        AdjLeft1Pt = Vector3.MoveTowards(AdjLeft1Pt, AdjRight1Pt, t_distance * LeftAdj);

        t_distance = Vector3.Distance(AdjLeft2Pt, AdjRight2Pt);
        AdjLeft2Pt = Vector3.MoveTowards(AdjLeft2Pt, AdjRight2Pt, t_distance * LeftAdj);

        t_distance = Vector3.Distance(AdjLeft3Pt, AdjRight3Pt);
        AdjLeft3Pt = Vector3.MoveTowards(AdjLeft3Pt, AdjRight3Pt, t_distance * LeftAdj);

        t_distance = Vector3.Distance(AdjLeft4Pt, AdjRight4Pt);
        AdjLeft4Pt = Vector3.MoveTowards(AdjLeft4Pt, AdjRight4Pt, t_distance * LeftAdj);


        FloorControl.SetValues(m_Image2DProjector, m_SizeOfCap, m_PosOfCap, m_RotOfCap, CenterPt);
        PitControl.SetValues(CenterPt, AdjLeft1Pt, AdjRight1Pt, AdjLeft4Pt, AdjRight4Pt);
        //ChunkControl.SetValues(CenterPt, AdjLeft1Pt, AdjLeft2Pt, AdjLeft3Pt, Pt13, AdjRight1Pt, AdjRight2Pt, AdjRight3Pt, AdjRight4Pt, Pt6, Pt7, Pt10, Pt11);
        ChunkControl.SetValues(CenterPt, Pt1, Pt2, Pt3, Pt4, 
                                         Pt5, Pt6, Pt7, Pt8, 
                                         Pt9, Pt10, Pt11, Pt12,
                                         Pt13, Pt14, Pt15, Pt16);


        //      1--2--3--4
        //      | 7| 8| 9|
        //      5--6--7--8
        //      | 4| 5| 6|
        //      9-10-11-12
        //      | 1| 2| 3|
        //     13-14-15-16

        FloorControl.InitializeObjects();
    }

    void SetMainPlane(bool isDown)
    {
        if (isDown)
        {
            //Move the plane locally first as to gaurentee its postion for raycasts
            MainPlane.transform.position = new Vector3(0, m_FloorY, 0);
            MainPlane.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                MainPlane.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(new Vector3(0, m_FloorY, 0));
            });
        }
        else
        {
            MainPlane.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                MainPlane.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(new Vector3(0, m_FloorY + 100, 0));
            });
        }
    }

    //X and Y are the viewport coords
    Vector3 GetRaycastPosition(float x, float y)
    {
        int layer_mask = LayerMask.GetMask("MainPlane");

        Ray ray = m_Camera.ViewportPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        Vector3 hitPoint = new Vector3(0, 0, 0);
        if (Physics.Raycast(ray, out hit, 99, layer_mask))
        {
            hitPoint = hit.point;
        }

        return hitPoint;
    }

}
