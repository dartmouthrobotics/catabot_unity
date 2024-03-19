using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class RosPositionFromPose : VehicleMovementBase {
    // ROS Connector
    ROSConnection m_Ros;

    private const string robotTopicStart = "robot_";
    private const string poseTopic = "/pose";

    public void ConnectToRobotOdom(int robot_num) {
        if (m_Ros == null) {
            m_Ros = ROSConnection.GetOrCreateInstance();
            m_Ros.Subscribe<PoseStampedMsg>(robotTopicStart + robot_num + poseTopic, PoseCallback);
        }
    }

    public void PoseCallback(PoseStampedMsg msg) {
        if (_movementActive) {
            transform.position = msg.pose.position.From<FLU>();
            transform.rotation = msg.pose.orientation.From<FLU>();
        }
    }

    public override void SetRosId(int id) {
        ConnectToRobotOdom(id);
    }

    public override string DisplayedName { get { return "pose"; } }
}
