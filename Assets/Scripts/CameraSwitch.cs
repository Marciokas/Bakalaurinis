using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraSwitch : MonoBehaviour {

    public Camera firstCamera;
    public Camera secondCamera;
    public Camera thirdCamera;
    public Camera fourthCamera;
    public GameObject indicatorDialog;
    public int cameraIndex = 2;

    void Start ()
    {
        firstCamera.enabled = false;
        thirdCamera.enabled = false;
        fourthCamera.enabled = false;
        secondCamera.enabled = true;
        indicatorDialog.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            firstCamera.enabled = false;
            thirdCamera.enabled = false;
            fourthCamera.enabled = false;
            secondCamera.enabled = true;
            indicatorDialog.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (cameraIndex < 4)
            {
                cameraIndex++;
            }
            else
            {
                cameraIndex = 1;
            }
        
        firstCamera.enabled = false;
        secondCamera.enabled = false;
        thirdCamera.enabled = false;
        fourthCamera.enabled = false;
        indicatorDialog.SetActive(false);
        if (cameraIndex == 1)
        {
            firstCamera.enabled = true;
        }
        if (cameraIndex == 2)
        {
            secondCamera.enabled = true;
            indicatorDialog.SetActive(true);
        }
        if (cameraIndex == 3)
        {
            thirdCamera.enabled = true;
        }
        if (cameraIndex == 4)
        {
            fourthCamera.enabled = true;
        }
    }
    }
}
