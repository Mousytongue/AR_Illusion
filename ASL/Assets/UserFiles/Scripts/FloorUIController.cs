using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorUIController : MonoBehaviour
{
    public Slider CaptureSizeSlider;
    public Text CaptureSizeText;
    public Button FloorProjButton;
    public FloorCaptureController FloorCaptureControl;

    public Slider RightAdjustSlider;
    public Slider LeftAdjustSlider;
    public Slider TopAdjustSlider;
    public Slider BotAdjustSlider;
    public Text RightText;
    public Text LeftText;
    public Text TopText;
    public Text BotText;    
    public float m_TopAdj = 1;
    public float m_BotAdj = 1;
    public float m_RightAdj = 1;
    public float m_LeftAdj = 1;

    private float m_SizeOfCap = 1;

    // Start is called before the first frame update
    void Start()
    {
        FloorProjButton.onClick.AddListener(delegate { CaptureScreen(); });
        CaptureSizeSlider.onValueChanged.AddListener(delegate { UpdateCaptureSize(CaptureSizeSlider.value); });
        RightAdjustSlider.onValueChanged.AddListener(delegate { UpdateRightAdjust(RightAdjustSlider.value); });
        LeftAdjustSlider.onValueChanged.AddListener(delegate { UpdateLeftAdjust(LeftAdjustSlider.value); });
        TopAdjustSlider.onValueChanged.AddListener(delegate { UpdateTopAdjust(TopAdjustSlider.value); });
        BotAdjustSlider.onValueChanged.AddListener(delegate { UpdateBotAdjust(BotAdjustSlider.value); });
    }

    void UpdateLeftAdjust(float val)
    {
        m_LeftAdj = val;
        LeftText.text = m_LeftAdj.ToString();
    }

    void UpdateBotAdjust(float val)
    {
        m_BotAdj = val;
        BotText.text = m_BotAdj.ToString();
    }

    void UpdateRightAdjust(float val)
    {
        m_RightAdj = val;
        RightText.text = m_RightAdj.ToString();
    }

    void UpdateTopAdjust(float val)
    {
        m_TopAdj = val;
        TopText.text = m_TopAdj.ToString();
    }

    void UpdateCaptureSize(float size)
    {
        m_SizeOfCap = size;
        CaptureSizeText.text = "" + (int)(size * 100) + "%";
    }

    void CaptureScreen()
    {
        FloorCaptureControl.CaptureScreen(m_SizeOfCap);
    }


}
