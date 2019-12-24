using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class FloorIllusionController_Pit : MonoBehaviour
{
    public GameObject m_PitWallTop;
    public GameObject m_PitWallLeft;
    public GameObject m_PitWallRight;
    public GameObject m_PitWallBot;
    public GameObject m_PitBottom;

    public GameObject m_InvisTop;
    public GameObject m_InvisLeft;
    public GameObject m_InvisRight;
    public GameObject m_InvisBot;

    public float m_PitDepth = 1f; 
    public float m_LavaDepth = 1f;
    public float m_LavaScale = 1f;

    private Vector3 m_Center;
    private Vector3 m_TopLeftPt;
    private Vector3 m_TopRightPt;
    private Vector3 m_BotLeftPt;
    private Vector3 m_BotRightPt;

    static public float m_LargestScale = 1;
    public float m_ObjectSpawnDistance = 1;

    public void SetValues(Vector3 center, Vector3 topLeft, Vector3 topRight, Vector3 botLeft, Vector3 botRight)
    {
            m_Center = center;
            m_TopLeftPt = topLeft;
            m_TopRightPt = topRight;
            m_BotLeftPt = botLeft;
            m_BotRightPt = botRight;
        //Set the max distance to scale the spawned moveable objects
        float MaxDistance = 0;
        float TopDistance = Vector3.Distance(m_TopLeftPt, m_TopRightPt);
        float LeftDistance = Vector3.Distance(m_TopLeftPt, m_BotLeftPt);
        float BotDistance = Vector3.Distance(m_BotLeftPt, m_BotRightPt);
        float RightDistance = Vector3.Distance(m_TopRightPt, m_BotRightPt);
        MaxDistance = TopDistance;
        if (MaxDistance < LeftDistance)
            MaxDistance = LeftDistance;
        if (MaxDistance < BotDistance)
            MaxDistance = BotDistance;
        if (MaxDistance < RightDistance)
            MaxDistance = RightDistance;

        m_LargestScale = MaxDistance;
        Debug.Log("Largest Scale: " + m_LargestScale);
        GameObject.Find("FloorCapture").GetComponent<FloorIllusionController_Chunk>().m_MaxDistance = m_LargestScale;
    }

    static public void PlankSpawnedClient(GameObject _myObj)
    {
        _myObj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() => {
            Vector3 scale = new Vector3(_myObj.transform.localScale.x * m_LargestScale / Random.Range(1.0f, 1.5f), _myObj.transform.localScale.y * m_LargestScale / Random.Range(1.0f, 1.5f), _myObj.transform.localScale.z * m_LargestScale / Random.Range(1.0f, 1.5f));
            _myObj.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(scale);
        });
    }

    static public void RockSpawnedClient(GameObject _myObj)
    {
        _myObj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() => {
            Vector3 scale = new Vector3(_myObj.transform.localScale.x * m_LargestScale / Random.Range(1.5f, 2), _myObj.transform.localScale.y * m_LargestScale / Random.Range(1.5f, 2), _myObj.transform.localScale.z * m_LargestScale / Random.Range(1.5f, 2));
            _myObj.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(scale);
        });
    }

    IEnumerator SpawnObject(Vector3 pos, float delay, bool is_Plank)
    {
        yield return new WaitForSeconds(delay);

        if (is_Plank)
        {
            float rotX = Random.Range(0, 180);
            float rotY = Random.Range(0, 180);
            float rotZ = Random.Range(0, 180);
            Quaternion rot = Quaternion.Euler(rotX, rotY, rotZ);
            ASL.ASLHelper.InstanitateASLObject("StaticPlank",
                        pos, rot, "", "UnityEngine.Rigidbody,UnityEngine",
                        PlankSpawnedClient,
                        null,
                        null);
        }
        else
        {
            float rotX = Random.Range(0, 180);
            float rotY = Random.Range(0, 180);
            float rotZ = Random.Range(0, 180);
            Quaternion rot = Quaternion.Euler(rotX, rotY, rotZ);
            ASL.ASLHelper.InstanitateASLObject("StaticRock",
            pos, rot, "", "UnityEngine.Rigidbody,UnityEngine",
            RockSpawnedClient,
            null,
            null);
        }
    }

    IEnumerator SpawnObjects(Vector3 startPos, float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < 5; i++)
        {
            // Can i Spawn a transform to manipulate, or must it be a game object ...
            GameObject obj = new GameObject();
            obj.transform.position = startPos;
            Quaternion rot = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
            obj.transform.rotation = rot;
            obj.transform.position += (obj.transform.forward * Random.Range(m_ObjectSpawnDistance*0.9f, m_ObjectSpawnDistance)*1.1f);
            obj.transform.position += new Vector3(0, m_LargestScale, 0);
            StartCoroutine(SpawnObject(obj.transform.position, i / 2, true));
            Destroy(obj);
        }

        for (int i = 0; i < 5; i++)
        {
            // Can i Spawn a transform to manipulate, or must it be a game object ...
            GameObject obj = new GameObject();
            obj.transform.position = startPos;
            Quaternion rot = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
            obj.transform.rotation = rot;
            obj.transform.position += (obj.transform.forward * Random.Range(m_ObjectSpawnDistance * 0.9f, m_ObjectSpawnDistance) * 1.1f);
            obj.transform.position += new Vector3(0, m_LargestScale, 0);
            StartCoroutine(SpawnObject(obj.transform.position, i / 2, false));
            Destroy(obj);
        }
    }

    public void SetupPit()
    {


        //Top wall
        //Center point
        Vector3 T_CenterPoint = (m_TopLeftPt + m_TopRightPt) / 2;
        //Angle
        Vector2 tlVec2 = new Vector2(m_TopLeftPt.x, m_TopLeftPt.z);
        Vector2 trVec2 = new Vector2(m_TopRightPt.x, m_TopRightPt.z);
        float angletop = -AngleBetweenTwoVector2(tlVec2, trVec2);

        m_PitWallTop.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 top_pos = new Vector3(T_CenterPoint.x, T_CenterPoint.y - (float)(m_LargestScale * m_PitDepth / 2), T_CenterPoint.z);
            m_PitWallTop.transform.position = top_pos;
            m_PitWallTop.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(top_pos);

            //Rotation
            Quaternion top_rot = Quaternion.identity;
            top_rot.eulerAngles = new Vector3(0, angletop, 0);
            m_PitWallTop.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(top_rot);

            //Scale
            float distanceTop = Vector3.Distance(m_TopLeftPt, (Vector3)this.m_TopRightPt);
            Vector3 top_scale = new Vector3(distanceTop, m_LargestScale * m_PitDepth, 1);
            m_PitWallTop.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(top_scale);
        }));

        m_InvisTop.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            Quaternion top_rot = Quaternion.identity;
            top_rot.eulerAngles = new Vector3(0, angletop, 0);
            m_InvisTop.transform.localRotation = top_rot;
            m_InvisTop.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(top_rot);


            //Scale
            Vector3 invistop_scale = new Vector3(50, 1, 50);
            m_InvisTop.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(invistop_scale);

            //Position
            Vector3 finalPos = T_CenterPoint += m_InvisTop.transform.forward * 25.01f;
            finalPos = new Vector3(finalPos.x, finalPos.y - 0.5f, finalPos.z);
            m_InvisTop.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(finalPos);

           
        });

        //Bot wall
        //Center point
        Vector3 B_CenterPoint = (m_BotLeftPt + m_BotRightPt) / 2;
        //Angle
        Vector2 blVec2 = new Vector2(m_BotLeftPt.x, m_BotLeftPt.z);
        Vector2 brVec2 = new Vector2(m_BotRightPt.x, m_BotRightPt.z);
        float anglebot = -AngleBetweenTwoVector2(blVec2, brVec2);
        anglebot += 180;

        m_PitWallBot.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 Bot_pos = new Vector3(B_CenterPoint.x, B_CenterPoint.y - (float)(m_LargestScale * m_PitDepth / 2), B_CenterPoint.z);
            m_PitWallBot.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(Bot_pos);

            //Rotation
            Quaternion Bot_rot = Quaternion.identity;
            Bot_rot.eulerAngles = new Vector3(0, anglebot, 0);

            m_PitWallBot.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Bot_rot);

            //Scale
            float distanceBot = Vector3.Distance(m_BotLeftPt, (Vector3)this.m_BotRightPt);
            Vector3 Bot_scale = new Vector3(distanceBot, m_LargestScale * m_PitDepth, 1);
            m_PitWallBot.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(Bot_scale);
        }));

        m_InvisBot.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            //Rotation
            Quaternion Bot_rot = Quaternion.identity;
            Bot_rot.eulerAngles = new Vector3(0, anglebot, 0);
            m_InvisBot.transform.localRotation = Bot_rot;
            m_InvisBot.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Bot_rot);

            //Scale
            Vector3 invisBot_scale = new Vector3(50, 1, 50);
            m_InvisBot.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(invisBot_scale);

            //Position
            Vector3 finalPos = B_CenterPoint += m_InvisBot.transform.forward * 25.01f;
            finalPos = new Vector3(finalPos.x, finalPos.y - 0.5f, finalPos.z);
            m_InvisBot.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(finalPos);

      
        });

        //Left wall
        //Center point
        Vector3 L_CenterPoint = (m_TopLeftPt + m_BotLeftPt) / 2;
        //Angle
        Vector2 tlVec2Left = new Vector2(m_TopLeftPt.x, m_TopLeftPt.z);
        Vector2 blVec2Left = new Vector2(m_BotLeftPt.x, m_BotLeftPt.z);
        float angleleft = -AngleBetweenTwoVector2(tlVec2Left, blVec2Left);
        angleleft += 180;

        m_PitWallLeft.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 Left_pos = new Vector3(L_CenterPoint.x, L_CenterPoint.y - (float)(m_LargestScale * m_PitDepth / 2), L_CenterPoint.z);
            m_PitWallLeft.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(Left_pos);

            //Rotation
            Quaternion Left_rot = Quaternion.identity;
            Left_rot.eulerAngles = new Vector3(0, angleleft, 0);
            m_PitWallLeft.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Left_rot);

            //Scale
            float distanceLeft = Vector3.Distance(m_TopLeftPt, (Vector3)this.m_BotLeftPt);
            Vector3 Left_scale = new Vector3(distanceLeft, m_LargestScale * m_PitDepth, 1);
            m_PitWallLeft.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(Left_scale);
        }));

        m_InvisLeft.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            //Rotation
            Quaternion Left_rot = Quaternion.identity;
            Left_rot.eulerAngles = new Vector3(0, angleleft, 0);
            m_InvisLeft.transform.localRotation = Left_rot;
            m_InvisLeft.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Left_rot);

            //Scale
            Vector3 invisBot_scale = new Vector3(50, 1, 50);
            m_InvisLeft.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(invisBot_scale);

            //Position
            Vector3 finalPos = L_CenterPoint += m_InvisLeft.transform.forward * 25.01f;
            finalPos = new Vector3(finalPos.x, finalPos.y - 0.5f, finalPos.z);
            m_InvisLeft.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(finalPos);

           

        });

        //Right wall
        //Center point
        Vector3 R_CenterPoint = (m_TopRightPt + m_BotRightPt) / 2;
        //Angle
        Vector2 trVec2Right = new Vector2(m_TopRightPt.x, m_TopRightPt.z);
        Vector2 brVec2Right = new Vector2(m_BotRightPt.x, m_BotRightPt.z);
        float angleRight = -AngleBetweenTwoVector2(trVec2Right, brVec2Right);

        m_PitWallRight.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Position
            Vector3 Right_pos = new Vector3(R_CenterPoint.x, R_CenterPoint.y - (float)(m_LargestScale *m_PitDepth / 2), R_CenterPoint.z);
            m_PitWallRight.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(Right_pos);

            //Rotation
            Quaternion Right_rot = Quaternion.identity;
            Right_rot.eulerAngles = new Vector3(0, angleRight, 0);
            m_PitWallRight.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Right_rot);

            //Scale
            float distanceRight = Vector3.Distance(m_TopRightPt, (Vector3)this.m_BotRightPt);
            Vector3 Right_scale = new Vector3(distanceRight, m_LargestScale * m_PitDepth, 1);
            m_PitWallRight.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(Right_scale);
        }));

        m_InvisRight.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            //Rotation
            Quaternion Right_rot = Quaternion.identity;
            Right_rot.eulerAngles = new Vector3(0, angleRight, 0);
            m_InvisRight.transform.localRotation = Right_rot;
            m_InvisRight.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(Right_rot);

            //Scale
            Vector3 invisRight_scale = new Vector3(50, 1, 50);
            m_InvisRight.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(invisRight_scale);

            //Position
            Vector3 finalPos = R_CenterPoint += m_InvisRight.transform.forward * 25.01f;
            finalPos = new Vector3(finalPos.x, finalPos.y - 0.5f, finalPos.z);
            m_InvisRight.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(finalPos);

           
        });

        //Pit Floor
        m_PitBottom.GetComponent<ASL.ASLObject>().SendAndSetClaim((ASL.ASLObject.ClaimCallback)(() =>
        {
            //Rotation
            //Worth a shot..
            m_PitBottom.transform.forward = m_PitWallTop.transform.forward;
            m_PitBottom.transform.localEulerAngles += new Vector3(90, 0, 0);
            Quaternion rot = m_PitBottom.transform.rotation;
            m_PitBottom.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(rot);

            //Position
            float distanceRight = Vector3.Distance(m_TopRightPt, m_BotRightPt);
            Vector3 pos = new Vector3(m_Center.x + distanceRight, m_Center.y - (m_LargestScale * m_LavaDepth), m_Center.z);
            //Vector3 pos = new Vector3(m_Center.x, m_Center.y - m_LavaDepth, m_Center.z);
            m_PitBottom.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(pos);

            //Scale
            Vector3 scale = new Vector3(m_LargestScale * m_LavaScale, m_LargestScale * m_LavaScale *0.8f, 1);
            m_PitBottom.GetComponent<ASL.ASLObject>().SendAndIncrementLocalScale(scale);
            //Vector3 scale = new Vector3(m_LavaScale*4, m_LavaScale*3, 1);
        }));



        StartCoroutine(SpawnObjects(m_Center, 10));
    }
    
    float AngleBetweenTwoVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 difference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, difference) * sign;
    }
}
