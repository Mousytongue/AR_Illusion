using System; // for assert
using UnityEngine;
using UnityEngine.UI; // for GUI elements: Button, Toggle

namespace Mps
{
    public partial class CSS451_Mp1_DropDownCreate : MonoBehaviour
    {

        // reference to all UI elements in the Canvas
        public Dropdown mCreateMenu = null;
        public CSS451_Mp1_TheWorld TheWorld = null;

        // Use this for initialization
        void Start()
        {
            Debug.Assert(mCreateMenu != null);
            Debug.Assert(TheWorld != null);

            // Drop down menu
            /* if we were to add options during runtime
                List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
                list.Add(new Dropdown.OptionData("Cube"));          // index = 0
                list.Add(new Dropdown.OptionData("Sphere"));        // index = 1
                list.Add(new Dropdown.OptionData("Cylinder"));      // index = 2
                mCreateMenu.AddOptions(list);
            */
            mCreateMenu.onValueChanged.AddListener(UserSelection);
        }
        PrimitiveType[] kLoadType = {PrimitiveType.Cube, PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder };
        void UserSelection(int index)
        {
            if (index == 0)
                return;

            mCreateMenu.value = 0; // always show the menu function: Object to create

            // inform the world of user's action
            TheWorld.ProcessUserSelection(kLoadType[index]);
        }
    }
}