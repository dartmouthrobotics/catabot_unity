using RosMessageTypes.Nav;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class RosPosFromOdom : MonoBehaviour {
    // ROS Connector
    ROSConnection m_Ros;

    private const string robotTopicStart = "robot_";
    private const string odomTopic = "/odom";

    public void ConnectToRobotOdom(int robot_num) {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.Subscribe<OdometryMsg>(robotTopicStart + robot_num + odomTopic, OdomCallback);
    }

    public void OdomCallback(OdometryMsg msg) {
        transform.position = msg.pose.pose.position.From<FLU>();
        transform.rotation = msg.pose.pose.orientation.From<FLU>();
    }
}
