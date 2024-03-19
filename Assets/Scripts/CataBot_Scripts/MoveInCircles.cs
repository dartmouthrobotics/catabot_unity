using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInCircles : VehicleMovementBase {
    public float forwardSpeed;
    public float rotationSpeed;

    void Update() {
        if (_movementActive) {
            transform.Translate(0, 0, forwardSpeed * Time.deltaTime);
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    public override string DisplayedName { get { return "Circles"; } }
}
