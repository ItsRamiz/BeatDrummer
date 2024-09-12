using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{

    public int currentFrame = 0;
    public bool enabledd = false;
    public TMPro.TextMeshProUGUI currentFrameText;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (enabledd == true)
        {
            currentFrame++;
            currentFrameText.text = "FPS: " + currentFrame.ToString();
        }
    }
    
    public void setEnable(bool enabled)
    {
        this.enabledd = enabled;
    }
}
