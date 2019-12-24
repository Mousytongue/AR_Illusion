using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    public class CSS451_Mp3_LineEndPt : MonoBehaviour
    {

        // One end point of a line segment
        public string RestingWall;

        //public event OnVariableChangeDelegate OnVariableChange;
        // delegate void OnVariableChangeDelegate(Mp3_LineEndPt myEndPt);

        public bool SetPosition(string onWall, Vector3 pos)
        {
            bool hit = (onWall == RestingWall);
            if (hit)
            {
                transform.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
                {
                    transform.GetComponent<ASL.ASLObject>()?.SendAndSetLocalPosition(pos);
                });
            }
            return hit;
        }

        public void SetPosition(Vector3 p)
        {
            transform.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
            {
                transform.GetComponent<ASL.ASLObject>()?.SendAndSetLocalPosition(p);
            });
        }

        public Vector3 GetPosition() { return transform.localPosition; }
    }
}