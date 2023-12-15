using RosMessageTypes.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class rosMove : MonoBehaviour {
    public bool connectOnAwake = true;

    // ROS Connector
    ROSConnection m_Ros;

	public float mSpeed;
    public GameObject vehicle;
	public Rigidbody rb;

    private const string cmdVelTopic = "cmd_vel";

    float forwardVel = 0;
    float verticalVel = 0;
    float angularVel = 0;

    private void Awake() {
        if(connectOnAwake) {
            ConnectToCmdVel();
        }
    }

    public void ConnectToCmdVel() {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.Subscribe<TwistMsg>(cmdVelTopic, CmdVelCallback);
    }

	// Use this for initialization
	void Start () {
		mSpeed = 5f;
		// rb = GetComponent<Rigidbody>();
	}

    // Use ROS Twist message inputs to move the robot
	void FixedUpdate () {
        float leftPropeller = (angularVel + forwardVel) / 2f;
        float rightPropeller = forwardVel - leftPropeller;

		Vector3 movement = new Vector3 (0, verticalVel, forwardVel);
        rb.AddRelativeForce (movement * 0.5f);
		rb.AddRelativeTorque(transform.up * angularVel * 0.005f);

		if(transform.position.y > 0) {
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		}
	}

    public void CmdVelCallback(TwistMsg msg) {
        Debug.Log(msg);
        forwardVel = (float) msg.linear.x;
        verticalVel = (float) msg.linear.z;
        angularVel = (float) -msg.angular.z;
    }
}
