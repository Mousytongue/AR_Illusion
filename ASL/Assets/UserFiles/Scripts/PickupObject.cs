using UnityEngine;
using UnityEngine.UI;

public class PickupObject : MonoBehaviour
{
    public Camera m_Camera;
    public Text UI_Snackbar;

    const float HOLD_DISTANCE = 0.5f;
    static private GameObject m_PickupObj;
    private Vector3 m_HoldPos;
    private bool is_Holding = false;
    private float distance;
    public float RotateSpeed = 2;

    static Vector3 objScale;

    // Start is called before the first frame update
    void Start()
    {
        distance = HOLD_DISTANCE;
    }

    // Update is called once per frame
    void Update()
    {

        if (is_Holding && m_PickupObj != null)
        {
            m_PickupObj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                UI_Snackbar.text = "Attempting to move target.";
                m_PickupObj.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(m_HoldPos);
            });
        }




        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            //Update where to potentially hold the object based on finger position
            Ray distanceRay = m_Camera.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y, 0));
            m_HoldPos = distanceRay.origin + (distanceRay.direction * distance);

            if (Input.touchCount > 1 && is_Holding == true)
            {
                Touch touchOne = Input.GetTouch(1);
                if (touch.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
                {
                    Vector2 touchZeroPrevPos = touch.position - touch.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touch.position - touchOne.position).magnitude;
                    float distanceChange = Mathf.Abs(prevTouchDeltaMag - touchDeltaMag);
                    //pinching out
                    if (prevTouchDeltaMag < touchDeltaMag)
                    {
                        m_PickupObj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                        {
                            UI_Snackbar.text = "Attempting to increase rotatation";
                            //Quaternion to send
                            Quaternion addRot = Quaternion.Euler(new Vector3(0, distanceChange * RotateSpeed, 0));
                            m_PickupObj.GetComponent<ASL.ASLObject>().SendAndIncrementWorldRotation(addRot);
                        });
                    }
                    //pinching in
                    else if (prevTouchDeltaMag > touchDeltaMag)
                    {
                        m_PickupObj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                        {
                            UI_Snackbar.text = "Attempting to decrease rotation";
                            //Quaternion to send
                            Quaternion addRot = Quaternion.Euler(new Vector3(0, -distanceChange * RotateSpeed, 0));
                            m_PickupObj.GetComponent<ASL.ASLObject>().SendAndIncrementWorldRotation(addRot);
                        });
                    }
                }
            }
            //Single touch - moves object around
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //Initial touch login
                    RayCastToPickup(touch.position.x, touch.position.y);
                    break;
                case TouchPhase.Moved:
                    //New obj location
                    break;
                case TouchPhase.Ended:
                    //Let Object go
                    is_Holding = false;                 
                    float[] key = { 0, 0, 0, 87 };
                    m_PickupObj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                    {
                        m_PickupObj.GetComponent<ASL.ASLObject>().SendFloat4(key);
                    });
                    m_PickupObj = null;
                    break;
            }

        }

    }

    void RayCastToPickup(float x, float y)
    {
        int layer_mask = 1 << 21 | 1 << 22;
        Ray ray = m_Camera.ScreenPointToRay(new Vector3(x, y, 0));
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow, 5f);
        RaycastHit hit;
        Vector3 hitPoint = new Vector3(0, 0, 0);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {

            //Static object found - spawn new one in your hands
            if (hit.transform.gameObject.layer == 21)
            {
                Quaternion plankRot = hit.transform.rotation;
                objScale = hit.transform.localScale;

                if (hit.transform.tag == "Plank")
                {
                    ASL.ASLHelper.InstanitateASLObject("MoveablePlank",
                            m_HoldPos, plankRot, "", "UnityEngine.Rigidbody,UnityEngine",
                            MoveablePlankCreatedClientSide,
                            null,
                            PlankFloatsFunction);

                }

                if (hit.transform.tag == "Rock")
                {
                    ASL.ASLHelper.InstanitateASLObject("MoveableRock",
                            m_HoldPos, plankRot, "", "UnityEngine.Rigidbody,UnityEngine",
                            MoveableRockCreatedClientSide,
                            null,
                            RockFloatsFunction);

                }


                distance = Vector3.Distance(m_Camera.transform.position, hit.transform.position) * 0.5f;
                is_Holding = true;

                UI_Snackbar.text = "Hit layer 21 (static) object.";
            }

            //Movable object - pick it up
            if (hit.transform.gameObject.layer == 22)
            {
                UI_Snackbar.text = "Hit layer 22 (moveable) object"; 

                m_PickupObj = hit.transform.gameObject;
                is_Holding = true;
                distance = Vector3.Distance(m_Camera.transform.position, hit.transform.position) * 0.9f;
                float[] key = { 0, 0, 0, 88 };
                m_PickupObj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    m_PickupObj.GetComponent<ASL.ASLObject>().SendFloat4(key);
                });
            } 
        }
        else
            UI_Snackbar.text = "Raycast failed. X: " + x + ", Y: " + y;
    }

    public static void MoveableRockCreatedClientSide(GameObject _myObject)
    {
        m_PickupObj = _myObject;

        float[] key = { 0, 0, 0, 99 };
        _myObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            _myObject.GetComponent<ASL.ASLObject>().SendFloat4(key);
        });
    }

    public static void RockFloatsFunction(string _id, float[] _myFloats)
    {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject))
        {
            if (_myFloats[3] == 99)
            {
                myObject.gameObject.layer = 22;
                myObject.gameObject.transform.localScale = objScale;
            }
            if (_myFloats[3] == 88)
            {
                // GameObject child = myObject.transform.GetChild(0).gameObject;
                myObject.GetComponent<Rigidbody>().useGravity = false;
            }
            if (_myFloats[3] == 87)
            {
                // GameObject child = myObject.transform.GetChild(0).gameObject;
                myObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }

    public static void MoveablePlankCreatedClientSide(GameObject _myObject)
    {
        m_PickupObj = _myObject;
        
        float[] key = { 0, 0, 0, 99 };
        _myObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            _myObject.GetComponent<ASL.ASLObject>().SendFloat4(key);
        });
    }

    public static void PlankFloatsFunction(string _id, float[] _myFloats)
    {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject))
        {
            if (_myFloats[3] == 99)
            {        
                myObject.gameObject.layer = 22;
                myObject.gameObject.transform.localScale = objScale;
            }
            if (_myFloats[3] == 88)
            {
               // GameObject child = myObject.transform.GetChild(0).gameObject;

                myObject.transform.rotation = Quaternion.Euler(new Vector3(0, myObject.transform.eulerAngles.y, 0));
                myObject.GetComponent<Rigidbody>().useGravity = false;
                myObject.GetComponent<Rigidbody>().freezeRotation = true;
                myObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            if (_myFloats[3] == 87)
            {
                myObject.GetComponent<Rigidbody>().useGravity = true;
                myObject.GetComponent<Rigidbody>().freezeRotation = false ;
                myObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
    }
}
