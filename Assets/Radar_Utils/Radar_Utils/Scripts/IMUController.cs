using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class IMUController : MonoBehaviour
{
    Rigidbody rb;
    float IMU_x;
    float IMU_y;
    float IMU_z;
    float sigma = 0.2f;
    float mu = 0;
    string filename = "IMUData_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        IMU_x = rb.angularVelocity.x+ RadarMath.GaussianMath(mu, sigma);
        IMU_y = rb.angularVelocity.y+ RadarMath.GaussianMath(mu, sigma);
        IMU_z = rb.angularVelocity.z+ RadarMath.GaussianMath(mu, sigma);
        File.AppendAllText(filename, "x:"+ IMU_x.ToString("F3") + ",y:"+IMU_y.ToString("F3") + ",z:" + IMU_z.ToString("F3") + "\n");
    }

    void OnGUI() {
        //GUILayout.BeginArea(new Rect(Screen.width - 10, 10, 100, Screen.height));
        GUILayout.BeginArea(new Rect(10, 10, 100, 90));
        //GUI.Label(new Rect(10, 10, 100, 90), rb.velocity.magnitude.ToString());
        GUILayout.Label("x: "+ IMU_x.ToString("F3")+ "\ny: " + IMU_y.ToString("F3") + "\nz: " + IMU_z.ToString("F3"));
        GUILayout.EndArea();
    }

}
