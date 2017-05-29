using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorCameraSwitch : MonoBehaviour
{

    public Camera indicatorCamera;
    public GameObject indicatorDialog;

    void OnMouseDown()
    {
        foreach (Camera c in Camera.allCameras)
        {
            c.enabled = false;
        }
        indicatorCamera.enabled = true;
        indicatorDialog.SetActive(false);
    }
}