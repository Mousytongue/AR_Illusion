using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupMovement : MonoBehaviour
{
    public GameObject Proj;
    public GameObject Plane;
    public GameObject Wall1;
    public GameObject Wall2;
    public GameObject Wall3;
    public GameObject Wall4;

    private Vector3 Direction;
    private float Delay;
    private float LiftSpeed;
    private float MoveSpeed;
    private bool is_Moving = false;

    // Update is called once per frame
    float timer = 0;
    void Update()
    {
        if (!is_Moving)
            return;

        if (timer <= Delay)
        {
            timer += Time.deltaTime;
            return;
        }

        transform.localPosition += transform.up * Time.deltaTime * LiftSpeed;
    }

    public void StartMovement(float delaytime, float lift_speed, float move_speed)
    {
        LiftSpeed = lift_speed;
        MoveSpeed = move_speed;
        Delay = delaytime;
        is_Moving = true;
    }

    public void StopMoving()
    {
        is_Moving = false;
        timer = 0;
    }
}
