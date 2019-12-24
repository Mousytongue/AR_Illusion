using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mps
{
    public class AR_CSS451_Mp2_ButtonManager : MonoBehaviour
    {
        public AR_CSS451_Mp2_TheWorld m_TheWorld;
        public Button[] m_CloudAnchorButtons;
        public Button m_Instructions;
        public Image m_BackgroundPanel;

        // Start is called before the first frame update
        void Start()
        {
            m_Instructions.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void DeactiveCloudAnchorButtons()
        {
            foreach (Button _button in m_CloudAnchorButtons)
            {
                _button.gameObject.SetActive(false);
            }
            m_Instructions.gameObject.SetActive(true);
        }

        public void ResetScene()
        {
            ASL.ASLHelper.SendAndSetNewScene("AR_CSS451_Mp2");
        }

        public void ParentToAnchorNoOrigin()
        {
            m_TheWorld.m_CloudAnchorExecutionType = 0;
            DeactiveCloudAnchorButtons();
        }

        public void ParentToAnchorAsOrigin()
        {
            m_TheWorld.m_CloudAnchorExecutionType = 1;
            DeactiveCloudAnchorButtons();
        }

        public void ParentToAnchorWithOriginBeingDifferentAnchor()
        {
            m_TheWorld.m_CloudAnchorExecutionType = 2;
            DeactiveCloudAnchorButtons();
        }

        public void AnchorNoOrigin()
        {
            m_TheWorld.m_CloudAnchorExecutionType = 3;
            DeactiveCloudAnchorButtons();
        }

        public void AnchorAsOrigin()
        {
            m_TheWorld.m_CloudAnchorExecutionType = 4;
            DeactiveCloudAnchorButtons();
        }

        public void AnchorWithOriginBeingDifferentAnchor()
        {
            m_TheWorld.m_CloudAnchorExecutionType = 5;
            DeactiveCloudAnchorButtons();
        }

        public void Ready()
        {
            m_Instructions.gameObject.SetActive(false);
            m_BackgroundPanel.gameObject.SetActive(false);
        }
    }
}