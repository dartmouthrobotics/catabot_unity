using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class IMUController : MonoBehaviour
{
    Rigidbody rb;
    private Quaternion lastOrientation;
    Vector3 angularVelocity;
    float sigma = 0.2f;
    float mu = 0;
    string filename = "IMUData_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";

    public bool ImuActive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastOrientation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (ImuActive) {
            angularVelocity = GetAngularVelocity(lastOrientation, transform.rotation);
            lastOrientation = transform.rotation;
            File.AppendAllText(filename, Time.realtimeSinceStartup + "," + angularVelocity.x.ToString("F3") + "," + angularVelocity.y.ToString("F3") + "," + angularVelocity.z.ToString("F3") + "\n");
        }
    }

    void OnGUI() {
        if (ImuActive) {
            //GUILayout.BeginArea(new Rect(Screen.width - 10, 10, 100, Screen.height));
            GUILayout.BeginArea(new Rect(Screen.width - 120, 10, 100, 90));
            //GUI.Label(new Rect(10, 10, 100, 90), rb.velocity.magnitude.ToString());
            GUILayout.Label("x: " + angularVelocity.x.ToString("F3") + "\ny: " + angularVelocity.y.ToString("F3") + "\nz: " + angularVelocity.z.ToString("F3"));
            GUILayout.EndArea();
        }
    }

    // https://forum.unity.com/threads/manually-calculate-angular-velocity-of-gameobject.289462/
    Vector3 GetAngularVelocity(Quaternion previousRotation, Quaternion currentRotation) {
        var deltaRot = currentRotation * Quaternion.Inverse(previousRotation);
        var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));

        return eulerRot / Time.deltaTime;
    }


}
