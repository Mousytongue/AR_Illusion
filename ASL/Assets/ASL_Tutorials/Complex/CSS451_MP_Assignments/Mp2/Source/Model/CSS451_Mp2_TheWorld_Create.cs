using UnityEngine;

namespace Mps
{
    public partial class CSS451_Mp2_TheWorld : MonoBehaviour
    {
        private Vector3 kDeltaVector = new Vector3(0.2f, 0.2f, 0.2f);

        public void CreatePrimitive(PrimitiveType type)
        {
            if (mSelected != null)
            {
                ASL.ASLHelper.InstanitateASLObject(type, mSelected.transform.localPosition + kDeltaVector, Quaternion.identity, mSelected.GetComponent<ASL.ASLObject>().m_Id, 
                    "Mps.CSS451_Mp2_ColorSupport",
                    ChangeColorUponCreation, null, MiscBehaviorHandler);
            }
            else // no parent
            {               
                float x = Random.Range(-4f, 4f);
                float y = Random.Range(0f, 2.5f);
                float z = Random.Range(-4f, 4f);

                //Set parent to be our GameController (TheWorld)
                ASL.ASLHelper.InstanitateASLObject(type, new Vector3(x, y, z), Quaternion.identity, "TheWorld", "Mps.CSS451_Mp2_ColorSupport",
                   ChangeColorUponCreation, null, MiscBehaviorHandler);
            }
        }

        public static void ChangeColorUponCreation(GameObject _myGameObject)
        {
            if (_myGameObject.transform.parent.name == "TheWorld")
            {
                _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(Color.black, Color.black);
                    float[] colorCode = {1, 0, 0, 0 }; //1 to trigger case 1 in MiscBehaviorHandler
                    _myGameObject.GetComponent<ASL.ASLObject>().SendFloat4(colorCode);
                });
            }
            else
            {
                _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    Color myColor = GetSiblingsColor(_myGameObject.transform.parent.gameObject);
                    //If no siblings found - just make white
                    if (myColor == (Color)Vector4.zero) { myColor = Color.white; }
                    _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(myColor, myColor);
                    float[] colorCode = {2, myColor.r, myColor.g, myColor.b }; //2 to trigger case 1 in MiscBehaviorHandler
                    _myGameObject.GetComponent<ASL.ASLObject>().SendFloat4(colorCode);
                });
            }

        }

        private static Color GetSiblingsColor(GameObject _parent)
        {
            for (int i = 0; i < _parent.transform.childCount; i++)
            {
                if (_parent.transform.GetChild(i).name != "AxisFrame")
                {
                    Transform sibling = _parent.transform.GetChild(i);                   
                    return sibling.gameObject.GetComponent<Renderer>().material.color; //Found one - get out
                }
            }
            return Vector4.zero;
        }

        //A good example of how flexible the send floats callback can be
        public static void MiscBehaviorHandler(string _id, float [] _key)
        {
            //If we find the object that called this operation
            if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject))
            {
                switch ((int)_key[0]) //Examine the first value to determine what behavior we need to perform
                {
                    case 1: //Set to black
                        myObject.GetComponent<CSS451_Mp2_ColorSupport>().m_OrgObjColor = Color.black; //Set original color for future use
                        break;
                    case 2: //Set to color passed in
                        myObject.GetComponent<CSS451_Mp2_ColorSupport>().m_OrgObjColor = new Color(_key[1], _key[2], _key[3]); //Set original color for future use
                        break;
                    default:
                        Debug.Log("Unaccounted for key passed for MiscBehaviorHandler function.");
                        break;
                }
            }
            
        }
    }
}