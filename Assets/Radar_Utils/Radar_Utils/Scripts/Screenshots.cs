using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenshots : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            ScreenCapture.CaptureScreenshot( "image_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") +".png",4);
            //ScreenCapture.CaptureScreenshot(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) +"/image" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") +".png",4);

        }
    }
}

