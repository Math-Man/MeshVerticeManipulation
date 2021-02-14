using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsDisplay;

    void Update()
    {
        float fps = 1 / Time.unscaledDeltaTime;
        fpsDisplay.text = "" + fps;
    }
}
