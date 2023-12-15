using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInCircles : MonoBehaviour {
    public float forwardSpeed;
    public float rotationSpeed;

    void Update() {
        transform.Translate(0, 0, forwardSpeed * Time.deltaTime);
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
