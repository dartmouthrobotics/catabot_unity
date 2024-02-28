using RosMessageTypes.Nav;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.VisualScripting;

public class RosInitializeOdomFollowers : MonoBehaviour {
    public bool connectOnAwake = true;
    public int numBoats = 2;

    public GameObject robotOdomFollowerPrefab = null;
    public GameObject boatOdomFollowerPrefab = null;

    // ROS Connector
    ROSConnection m_Ros;

    private const string robotTopicStart = "/robot_";
    private const string odomTopic = "/odom";

    private void Awake() {
        if(connectOnAwake) {
            m_Ros = ROSConnection.GetOrCreateInstance();

            // Always instantiate the robot in addition to however many boats there are
            for(int i = 0; i < numBoats+1; i++) {
                Instantiate<GameObject>(i < 1 ? robotOdomFollowerPrefab : boatOdomFollowerPrefab).GetComponent<RosPosFromOdom>().ConnectToRobotOdom(i);
            }
        }
    }
}
