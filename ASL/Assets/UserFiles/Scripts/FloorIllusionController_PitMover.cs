using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorIllusionController_PitMover : MonoBehaviour
{
    public float MoveDelay = 5;
    public float LiftSpeed = 0.01f;
    public float MoveSpeed = 0.01f;

    public GameObject Group1;
    public GameObject Group2;
    public GameObject Group3;
    public GameObject Group4;
    public GameObject Group5;
    public GameObject Group6;
    public GameObject Group7;
    public GameObject Group8;
    public GameObject Group9;
    List<GameObject> Groups = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Groups.Add(Group1);
        Groups.Add(Group2);
        Groups.Add(Group3);
        Groups.Add(Group4);
        Groups.Add(Group5);
        Groups.Add(Group6);
        Groups.Add(Group7);
        Groups.Add(Group8);
        Groups.Add(Group9);
    }

    public void StartMovement()
    {
        float i = MoveDelay;
        
        Group9.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group8.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group7.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group6.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group5.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group4.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group3.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group2.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;

        Group1.GetComponentInParent<GroupMovement>().StartMovement(i, LiftSpeed, MoveSpeed);
        i += 1;
    }

    public void StopMoving()
    {
        foreach (GameObject obj in Groups)
        {
            obj.GetComponentInParent<GroupMovement>().StopMoving();
            obj.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
