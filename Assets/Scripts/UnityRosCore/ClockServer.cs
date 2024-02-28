using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Rosgraph;
using RosMessageTypes.BuiltinInterfaces;

public class ClockServer : MonoBehaviour {
    public bool broadcastROSMessages = true;

    // ROS Connector
    ROSConnection m_Ros;

    private const string clockTopic = "/clock";

    private void Awake() {
        if(broadcastROSMessages) {
            m_Ros = ROSConnection.GetOrCreateInstance();
            m_Ros.RegisterPublisher<ClockMsg>(clockTopic);
        }
    }

    private void Update() {
        if(broadcastROSMessages) {
            m_Ros.Publish(clockTopic, new ClockMsg(new TimeStamp(Clock.Now)));
        }
    }
}
