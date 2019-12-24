﻿using UnityEngine;

namespace SimpleDemos
{
    /// <summary>
    /// Controls the ASLSlidersWithEcho objects in your scene. Usually you want to attach this script to your canvas object 
    /// and then manually assign each slider to it via the editor.
    /// </summary>
    public class SliderController : MonoBehaviour
    {
        /// <summary>
        /// An example of how to use the ASLSliderWithEcho. Feel free to change the name of this to whatever suits your application
        /// and to add as many as you want.
        /// </summary>
        public ASL.ASLSliderWithEcho m_ExampleSlider;

        /// <summary>
        /// Initializes the ASL sliders in the scene
        /// </summary>
        void Start()
        {
            Debug.Assert(m_ExampleSlider != null, "Example slider is null. Please attach in editor.");
            m_ExampleSlider.InitilizeSlider("Example", 0, 1, .5f, FunctionToCallAfterChangingSlider);
        }

        /// <summary>
        /// This is the function that gets called after the slider value is changed AND that new value has been received from 
        /// the GameSparks relay server. You cannot have more or less parameters than the ones shown here when you make your 
        /// own function due to this function being a ASLObject.FloatCallback function.
        /// </summary>
        /// <param name="_idOfTheASLObjectThatSentTheseValues">The id of the ASL object that sent value - can be used to determine
        /// what GameObject was used if necessary</param>
        /// <param name="_theFloatsThatWereSent">The four floats that were sent - in this case, since we are using a slider,
        /// only 1 value was really sent: [0].</param>
        public void FunctionToCallAfterChangingSlider(string _idOfTheASLObjectThatSentTheseValues, float[] _theFloatsThatWereSent)
        {
            /*Do whatever you want to do with the new slider value here*/
            Debug.Log(m_ExampleSlider.TheLabel.text + " Value: " + _theFloatsThatWereSent[0]);
        }

    }
}