using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UISliderManager : MonoBehaviour
{
    public string SliderName;
    public TMP_Text sliderValueText;
    public Slider slider;
    public string rounding;
    
    void Start(){
        if (PlayerPrefs.GetFloat(SliderName) != 0.0f)
        {
            slider.value = PlayerPrefs.GetFloat(SliderName);
            updateSliderAttributes(slider.value);
        }
    }

    public void updateSliderAttributes(float value)
    {
        PlayerPrefs.SetFloat(SliderName, value);
        sliderValueText.text = value.ToString(rounding);
    }
}
