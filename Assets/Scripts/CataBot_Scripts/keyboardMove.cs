using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyboardMove : VehicleMovementBase {

	public float ForceMultiplier = 0.5f;
	public float TorqueMultiplier = 0.9f;
    public GameObject vehicle;
	public Rigidbody rb;

    // Use keyboard inputs to move the robot
    void FixedUpdate () {
		if (_movementActive) {
			float leftPropeller = Input.GetAxis("LeftPropeller");
			float rightPropeller = Input.GetAxis("RightPropeller");

			float forwardVel = leftPropeller + rightPropeller;
			float angularVel = leftPropeller - rightPropeller;

			Vector3 movement = new Vector3(0, 0, forwardVel);
			rb.AddRelativeForce(movement * ForceMultiplier);
			rb.AddRelativeTorque(transform.up * angularVel * TorqueMultiplier);

			if (transform.position.y > 0) {
				transform.position = new Vector3(transform.position.x, 0, transform.position.z);
			}
		}
	}

	public override string DisplayedName { get { return "Keyboard"; } }
}
