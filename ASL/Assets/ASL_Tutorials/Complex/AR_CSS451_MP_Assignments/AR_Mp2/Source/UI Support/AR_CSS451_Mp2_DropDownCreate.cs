using UnityEngine;
using UnityEngine.UI; // for GUI elements: Button, Toggle

namespace Mps
{
    public partial class AR_CSS451_Mp2_DropDownCreate : MonoBehaviour
    {

        // reference to all UI elements in the Canvas
        public Dropdown mCreateMenu = null;
        public AR_CSS451_Mp2_TheWorld TheWorld = null;

        // Use this for initialization
        void Start()
        {
            Debug.Assert(mCreateMenu != null);
            Debug.Assert(TheWorld != null);
            mCreateMenu.onValueChanged.AddListener(UserSelection);
        }

        PrimitiveType[] kLoadType = {
        PrimitiveType.Cube,     // this is used as menu label, not used
        PrimitiveType.Cube,
        PrimitiveType.Sphere,
        PrimitiveType.Cylinder };

        void UserSelection(int index)
        {
            if (index == 0)
                return;

            mCreateMenu.value = 0; // always show the menu function: Object to create

            // inform the world of user's action
            TheWorld.CreatePrimitive(kLoadType[index]);
        }
    }
}