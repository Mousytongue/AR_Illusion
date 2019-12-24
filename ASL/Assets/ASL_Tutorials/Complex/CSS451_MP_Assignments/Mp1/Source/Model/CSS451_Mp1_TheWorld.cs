using UnityEngine;

namespace Mps
{
    public class CSS451_Mp1_TheWorld : MonoBehaviour
    {

        public GameObject CreationTarget;
        private const float kTargetSize = 0.5f;

        // Use this for initialization
        void Start()
        {
            Debug.Assert(CreationTarget != null);

            CreationTarget.GetComponent<Renderer>().material.color = Color.black;
            CreationTarget.transform.localScale = new Vector3(kTargetSize, kTargetSize, kTargetSize);
            CreationTarget.transform.localPosition = new Vector3(0, kTargetSize / 2, 0);
            CreationTarget.layer = 8; // this layer will not be ray casted!

            GameObject.Find("CreationPlane").GetComponent<Renderer>().material.color =
                new Color(0.4528302f, 0.4528302f, 0.4528302f, 1);
        }

        public void SelectObjectAt(GameObject obj, Vector3 p)
        {
            if (obj.name == "CreationPlane")
            {
                p.y = kTargetSize / 2f;
                //Send CreationTarget position to all players
                CreationTarget.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    CreationTarget.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(p);
                });
            }
            else
            {
                //Delete object selected for all players
                obj.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    obj.GetComponent<ASL.ASLObject>().DeleteObject();
                });
            }
        }

        public void ProcessUserSelection(PrimitiveType objType)
        {
            Vector3 p = CreationTarget.transform.localPosition;
            p.y = 0.5f;
            //Determine which
            //Instantiate network object - While these are ASL Objects, it should be noted that when they are
            //Moved or manipulated by CSS451_Mp1_MovementBehavior.cs they are done so locally. These objects could be instantiated via prefabs
            //And it would involve less effort, but by doing it this way it gives a good example of how to add components on creation and
            //then how to modify them right afterwards.
            ASL.ASLHelper.InstanitateASLObject(objType, p, Quaternion.identity, "", "Mps.CSS451_Mp1_MovementBehavior", OnObjectCreation, null, UpdateMovementBehaviorComponentForAll);
        }

        public static void OnObjectCreation(GameObject _myNewObject)
        {
            //Use the function called upon creation to send floats that will trigger what to do for all users 
            float[] key = { 0, 0, 0, 0 };
            _myNewObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                _myNewObject.GetComponent<ASL.ASLObject>().SendFloat4(key);
            });           
        }

        public static void UpdateMovementBehaviorComponentForAll(string _id, float[] _key)
        {
            //What the float values are don't matter here as this is the only thing we'll ever do with these objects
            //By using the SendFloat4 function we are synchronizing all of these movement behaviors for all users
            //If we find the object that called this operation
            if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject _myObject))
            {
                CSS451_Mp1_MovementBehavior movementBehavior = _myObject.GetComponent<CSS451_Mp1_MovementBehavior>();
                if (_myObject.name == "Cube")
                {
                    movementBehavior.mMovementLimit = 5;
                    movementBehavior.mMovementDirection = new Vector3(0, 1, 0);
                    movementBehavior.mToggleColor = new Color(0.02353489f, 1, 0, 1);
                    movementBehavior.mRotationSpeed = 90;
                    movementBehavior.mLinearSpeed = 1;
                }
                else if (_myObject.name == "Sphere")
                {
                    movementBehavior.mMovementLimit = 5;
                    movementBehavior.mMovementDirection = new Vector3(1, 0, 0);
                    movementBehavior.mToggleColor = new Color(1, 0, 0, 1);
                    movementBehavior.mRotationSpeed = 0;
                    movementBehavior.mLinearSpeed = 1;
                }
                else if (_myObject.name == "Cylinder")
                {
                    movementBehavior.mMovementLimit = 5;
                    movementBehavior.mMovementDirection = new Vector3(0, 0, 1);
                    movementBehavior.mToggleColor = new Color(0, 0.2163107f, 1, 1);
                    movementBehavior.mRotationSpeed = 0;
                    movementBehavior.mLinearSpeed = 1;
                }
            }
        }
    }
}