using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorIllusionController_Chunk : MonoBehaviour
{
    public float m_ChunkDepth = 1f;
    public float m_ChunkAdjustBot = 0.05f;
    public float m_ChunkAdjustTop = 0.00f;
    public float m_ChunkAdjustLeft = 0.00f;
    public float m_ChunkAdjustRight = 0.05f;

    public GameObject Set1Top;
    public GameObject Set1Bot;
    public GameObject Set1Left;
    public GameObject Set1Right;
    public GameObject Set2Top;
    public GameObject Set2Bot;
    public GameObject Set2Left;
    public GameObject Set2Right;
    public GameObject Set3Top;
    public GameObject Set3Bot;
    public GameObject Set3Left;
    public GameObject Set3Right;
    public GameObject Set4Top;
    public GameObject Set4Bot;
    public GameObject Set4Left;
    public GameObject Set4Right;
    public GameObject Set5Top;
    public GameObject Set5Bot;
    public GameObject Set5Left;
    public GameObject Set5Right;
    public GameObject Set6Top;
    public GameObject Set6Bot;
    public GameObject Set6Left;
    public GameObject Set6Right;
    public GameObject Set7Top;
    public GameObject Set7Bot;
    public GameObject Set7Left;
    public GameObject Set7Right;
    public GameObject Set8Top;
    public GameObject Set8Bot;
    public GameObject Set8Left;
    public GameObject Set8Right;
    public GameObject Set9Top;
    public GameObject Set9Bot;
    public GameObject Set9Left;
    public GameObject Set9Right;

    private Vector3 m_Center;
    private Vector3 m_Pt1;
    private Vector3 m_Pt2;
    private Vector3 m_Pt3;
    private Vector3 m_Pt4;
    private Vector3 m_Pt5;
    private Vector3 m_Pt6;
    private Vector3 m_Pt7;
    private Vector3 m_Pt8;
    private Vector3 m_Pt9;
    private Vector3 m_Pt10;
    private Vector3 m_Pt11;
    private Vector3 m_Pt12;
    private Vector3 m_Pt13;
    private Vector3 m_Pt14;
    private Vector3 m_Pt15;
    private Vector3 m_Pt16;

    //private int num_Width = 3;
    //private int num_Height = 3;

    public float m_MaxDistance = 1;

    public void SetValues(Vector3 center, Vector3 Pt1, Vector3 Pt2, Vector3 Pt3, Vector3 Pt4, 
                                          Vector3 Pt5, Vector3 Pt6, Vector3 Pt7, Vector3 Pt8,
                                          Vector3 Pt9, Vector3 Pt10, Vector3 Pt11, Vector3 Pt12,
                                          Vector3 Pt13, Vector3 Pt14, Vector3 Pt15, Vector3 Pt16)
    {
        m_Center = center;


        m_Pt1 = Pt1;
        m_Pt2 = Pt2;
        m_Pt3 = Pt3;
        m_Pt4 = Pt4;
        m_Pt5 = Pt5;
        m_Pt6 = Pt6;
        m_Pt7 = Pt7;
        m_Pt8 = Pt8;
        m_Pt9 = Pt9;
        m_Pt10 = Pt10;
        m_Pt11 = Pt11;
        m_Pt12 = Pt12;
        m_Pt13 = Pt13;
        m_Pt14 = Pt14;
        m_Pt15 = Pt15;
        m_Pt16 = Pt16;
    }

    public void SetupChunks()
    {
        //      1--2--3--4
        //      | 7| 8| 1|
        //      5--6--7--8
        //      | 6| 9| 2|
        //      9-10-11-12
        //      | 5| 4| 3|
        //     13-14-15-16

        //       TOP - BOT - LEFT- RIGHT   -------   TL - TR - BL - BR
        MoveChuck(Set1Top, Set1Bot, Set1Left, Set1Right, m_Pt3, m_Pt4, m_Pt7, m_Pt8);
        MoveChuck(Set2Top, Set2Bot, Set2Left, Set2Right, m_Pt7, m_Pt8, m_Pt11, m_Pt12);
        MoveChuck(Set3Top, Set3Bot, Set3Left, Set3Right, m_Pt11, m_Pt12, m_Pt15, m_Pt16);
        MoveChuck(Set4Top, Set4Bot, Set4Left, Set4Right, m_Pt10, m_Pt11, m_Pt14, m_Pt15);
        MoveChuck(Set5Top, Set5Bot, Set5Left, Set5Right, m_Pt9, m_Pt10, m_Pt13, m_Pt14);
        MoveChuck(Set6Top, Set6Bot, Set6Left, Set6Right, m_Pt5, m_Pt6, m_Pt9, m_Pt10);
        MoveChuck(Set7Top, Set7Bot, Set7Left, Set7Right, m_Pt1, m_Pt2, m_Pt5, m_Pt6);
        MoveChuck(Set8Top, Set8Bot, Set8Left, Set8Right, m_Pt2, m_Pt3, m_Pt6, m_Pt7);
        MoveChuck(Set9Top, Set9Bot, Set9Left, Set9Right, m_Pt6, m_Pt7, m_Pt10, m_Pt11);
    }

    void MoveChuck(GameObject m_Top, GameObject m_Bot, GameObject m_Left, GameObject m_Right, Vector3 tTL, Vector3 tTR, Vector3 tBL, Vector3 tBR)
    {

        //Crunch each squares in
        float distanceTop = Vector3.Distance(tTL, tTR);
        Vector3 tempTL = Vector3.MoveTowards(tTL, tTR, distanceTop * m_ChunkAdjustLeft / 2);
        Vector3 tempTR = Vector3.MoveTowards(tTR, tTR, distanceTop * m_ChunkAdjustRight / 2);
        float distanceBot = Vector3.Distance(tBL, tBR);
        Vector3 tempBL = Vector3.MoveTowards(tBL, tBR, distanceBot * m_ChunkAdjustLeft / 2);
        Vector3 tempBR = Vector3.MoveTowards(tBR, tBL, distanceBot * m_ChunkAdjustRight / 2);
        float distanceLeft = Vector3.Distance(tempTL, tempBL);
        Vector3 TL = Vector3.MoveTowards(tempTL, tempBL, distanceLeft * m_ChunkAdjustTop / 2);
        Vector3 BL = Vector3.MoveTowards(tempBL, tempTL, distanceLeft * m_ChunkAdjustBot / 2);
        float distanceRight = Vector3.Distance(tempTR, tempBR);
        Vector3 TR = Vector3.MoveTowards(tempTR, tempBR, distanceRight * m_ChunkAdjustTop / 2);
        Vector3 BR = Vector3.MoveTowards(tempBR, tempTR, distanceRight * m_ChunkAdjustBot / 2);


        //Top wall
        //Center point
        Vector3 T_CenterPoint = (TL + TR) / 2;
        //Angle
        Vector2 tlVec2 = new Vector2(TL.x, TL.z);
        Vector2 trVec2 = new Vector2(TR.x, TR.z);
        float angletop = -AngleBetweenTwoVector2(tlVec2, trVec2);
        angletop += 180;

        m_Top.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 top_pos = new Vector3(T_CenterPoint.x, T_CenterPoint.y - (float)(m_MaxDistance * m_ChunkDepth / 2), T_CenterPoint.z);
            m_Top.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(top_pos);

            //Rotation

            Quaternion top_rot = Quaternion.identity;
            top_rot.eulerAngles = new Vector3(0, angletop, 0);
            m_Top.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(top_rot);

            //Scale
            distanceTop = Vector3.Distance(TL, TR);
            Vector3 top_scale = new Vector3(distanceTop, m_MaxDistance * m_ChunkDepth, 1);
            m_Top.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(top_scale);
        }));

        //Bot wall
        //Center point
        Vector3 B_CenterPoint = (BL + BR) / 2;
        //Angle
        Vector2 blVec2 = new Vector2(BL.x, BL.z);
        Vector2 brVec2 = new Vector2(BR.x, BR.z);
        float anglebot = -AngleBetweenTwoVector2(blVec2, brVec2);

        m_Bot.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 Bot_pos = new Vector3(B_CenterPoint.x, B_CenterPoint.y - (float)(m_MaxDistance * m_ChunkDepth / 2), B_CenterPoint.z);
            m_Bot.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(Bot_pos);

            //Rotation

            Quaternion Bot_rot = Quaternion.identity;
            Bot_rot.eulerAngles = new Vector3(0, anglebot, 0);
            m_Bot.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Bot_rot);

            //Scale
            distanceBot = Vector3.Distance(BL, BR);
            Vector3 Bot_scale = new Vector3(distanceBot, m_MaxDistance * m_ChunkDepth, 1);
            m_Bot.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(Bot_scale);
        }));

        //Left wall
        //Center point
        Vector3 L_CenterPoint = (TL + BL) / 2;
        //Angle
        Vector2 tlVec2Left = new Vector2(TL.x, TL.z);
        Vector2 blVec2Left = new Vector2(BL.x, BL.z);
        float angleleft = -AngleBetweenTwoVector2(tlVec2Left, blVec2Left);

        m_Left.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 Left_pos = new Vector3(L_CenterPoint.x, L_CenterPoint.y - (float)(m_MaxDistance * m_ChunkDepth / 2), L_CenterPoint.z);
            m_Left.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(Left_pos);

            //Rotation
            Quaternion Left_rot = Quaternion.identity;
            Left_rot.eulerAngles = new Vector3(0, angleleft, 0);
            m_Left.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Left_rot);

            //Scale
            distanceLeft = Vector3.Distance(TL, BL);
            Vector3 Left_scale = new Vector3(distanceLeft, m_MaxDistance * m_ChunkDepth, 1);
            m_Left.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(Left_scale);
        }));

        //Right wall
        //Center point
        Vector3 R_CenterPoint = (TR + BR) / 2;
        //Angle
        Vector2 trVec2Right = new Vector2(TR.x, TR.z);
        Vector2 brVec2Right = new Vector2(BR.x, BR.z);
        float angleRight = -AngleBetweenTwoVector2(trVec2Right, brVec2Right);
        angleRight += 180;

        m_Right.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 Right_pos = new Vector3(R_CenterPoint.x, R_CenterPoint.y - (float)(m_MaxDistance * m_ChunkDepth / 2), R_CenterPoint.z);
            m_Right.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(Right_pos);

            //Rotation
            Quaternion Right_rot = Quaternion.identity;
            Right_rot.eulerAngles = new Vector3(0, angleRight, 0);
            m_Right.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Right_rot);

            //Scale
            distanceRight = Vector3.Distance(TR, BR);
            Vector3 Right_scale = new Vector3(distanceRight, m_MaxDistance * m_ChunkDepth, 1);
            m_Right.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(Right_scale);
        }));

    }

    float AngleBetweenTwoVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 difference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, difference) * sign;
    }
}
