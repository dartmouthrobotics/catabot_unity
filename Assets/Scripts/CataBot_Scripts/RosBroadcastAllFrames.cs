using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using UnityEngine;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Nav;

public class RosBroadcastAllFrames : MonoBehaviour {
    // ROS Connector
    ROSConnection m_Ros;

    public bool broadcastROSMessages = true;
    private bool haveRobotBoat = false;
    private bool haveTrafficBoats = false;

    public Transform robotBoat;
    public Transform[] trafficBoats;

    private TFMessageMsg robotBoatPoseMsg = new TFMessageMsg(new TransformStampedMsg[2]);
    private TFMessageMsg trafficBoatsPoseMsg;

    private OdometryMsg robotBoatOdomMsg = new OdometryMsg();
    private OdometryMsg[] trafficBoatsOdomMsgs;

    private Vector3 robotBoatPosLast = Vector3.zero;
    private Vector3[] trafficBoatPosLast;

    private Quaternion robotBoatRotLast = Quaternion.identity;
    private Quaternion[] trafficBoatRotLast;

    private const string tfTopic = "tf";
    private const string odomFrame = "odom";
    private const string robotFrame = "robot";
    private const string baseScanFrame = "base_scan";

    public string rootFrame = "map";

    private void Awake () {
        if(broadcastROSMessages) {
            // Get ROS connection static instance
            m_Ros = ROSConnection.GetOrCreateInstance();
            m_Ros.RegisterPublisher<TFMessageMsg>(tfTopic);

            var timestamp = new TimeStamp(Clock.time);

            if(robotBoat != null) {
                haveRobotBoat = true;
                
                HeaderMsg robotHeader = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), rootFrame);
                Vector3<FLU> robotPos = robotBoat.position.To<FLU>();
                TransformMsg robotTrans = new TransformMsg(
                    new Vector3Msg(robotPos.x, robotPos.y, robotPos.z),
                    robotBoat.rotation.To<FLU>()
                );
                robotBoatPoseMsg.transforms[0] = new TransformStampedMsg(robotHeader, robotFrame + "_0/" + odomFrame, robotTrans);
                HeaderMsg baseScanHeader = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), robotFrame + "_0/" + odomFrame);
                TransformMsg baseScanTrans = new TransformMsg(
                    new Vector3Msg(0,0,0),
                    Quaternion.identity.To<FLU>()
                );
                robotBoatPoseMsg.transforms[1] = new TransformStampedMsg(baseScanHeader, baseScanFrame, baseScanTrans);
                // Finally send the message to server_endpoint.py running in ROS
                m_Ros.Publish(tfTopic, robotBoatPoseMsg);

                robotBoatOdomMsg.header = robotHeader;
                robotBoatOdomMsg.child_frame_id = robotFrame + "_0";
                robotBoatOdomMsg.pose = new PoseWithCovarianceMsg(new PoseMsg(new PointMsg(robotPos.x, robotPos.y, robotPos.z), robotBoat.rotation.To<FLU>()), new double[36]);
                robotBoatOdomMsg.twist = new TwistWithCovarianceMsg(new TwistMsg(Vector3.zero.To<FLU>(), Vector3.zero.To<FLU>()), new double[36]);
                m_Ros.RegisterPublisher<OdometryMsg>(robotFrame + "_0/" + odomFrame);
                m_Ros.Publish(robotFrame + "_0/" + odomFrame,robotBoatOdomMsg);

                robotBoatPosLast = robotBoat.position;
                robotBoatRotLast = robotBoat.rotation;
            }

            if(trafficBoats != null && trafficBoats.Length > 0) {
                haveTrafficBoats = true;
                trafficBoatsPoseMsg = new TFMessageMsg(new TransformStampedMsg[trafficBoats.Length]);
                trafficBoatsOdomMsgs = new OdometryMsg[trafficBoats.Length];
                trafficBoatPosLast = new Vector3[trafficBoats.Length];
                trafficBoatRotLast = new Quaternion[trafficBoats.Length];

                for(int i = 0; i < trafficBoats.Length; i++) {
                    HeaderMsg header = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), rootFrame);
                    Vector3<FLU> trafficPos = trafficBoats[i].position.To<FLU>();
                    TransformMsg trans = new TransformMsg(
                        new Vector3Msg(trafficPos.x, trafficPos.y, trafficPos.z),
                        trafficBoats[i].rotation.To<FLU>()
                    );
                    trafficBoatsPoseMsg.transforms[i] = new TransformStampedMsg(header, robotFrame + "_" + (i+1) + "/" + odomFrame, trans);

                    trafficBoatsOdomMsgs[i] = new OdometryMsg();
                    trafficBoatsOdomMsgs[i].header = header;
                    trafficBoatsOdomMsgs[i].child_frame_id = robotFrame + "_" + (i + 1);
                    trafficBoatsOdomMsgs[i].pose = new PoseWithCovarianceMsg(new PoseMsg(new PointMsg(trafficPos.x, trafficPos.y, trafficPos.z), trafficBoats[i].rotation.To<FLU>()), new double[36]);
                    trafficBoatsOdomMsgs[i].twist = new TwistWithCovarianceMsg(new TwistMsg(Vector3.zero.To<FLU>(), Vector3.zero.To<FLU>()), new double[36]);
                    m_Ros.RegisterPublisher<OdometryMsg>(robotFrame + "_" + (i+1) + "/" + odomFrame);
                    m_Ros.Publish(robotFrame + "_" + (i+1) + "/" + odomFrame,trafficBoatsOdomMsgs[i]);

                    trafficBoatPosLast[i] = trafficBoats[i].position;
                    trafficBoatRotLast[i] = trafficBoats[i].rotation;
                }
                // Finally send the message to server_endpoint.py running in ROS
                m_Ros.Publish(tfTopic, trafficBoatsPoseMsg);
            }
        }
    }

    private void Update() {
        if(broadcastROSMessages) {
            var timestamp = new TimeStamp(Clock.time);

            float angle;
            Vector3 axes;

            if(haveRobotBoat) {
                Vector3<FLU> pos = robotBoat.position.To<FLU>();

                robotBoatPoseMsg.transforms[0].header = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), rootFrame);
                robotBoatPoseMsg.transforms[1].header = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), robotFrame + "_0/" + odomFrame);
                robotBoatPoseMsg.transforms[0].transform.translation = new Vector3Msg(pos.x, pos.y, pos.z);
                robotBoatPoseMsg.transforms[0].transform.rotation = robotBoat.rotation.To<FLU>();
                m_Ros.Publish(tfTopic, robotBoatPoseMsg);

                robotBoatOdomMsg.header = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), rootFrame);
                robotBoatOdomMsg.pose.pose.position = new PointMsg(pos.x, pos.y, pos.z);
                robotBoatOdomMsg.pose.pose.orientation = robotBoat.rotation.To<FLU>();
                robotBoatOdomMsg.twist.twist.linear = ((robotBoat.position - robotBoatPosLast) / Time.deltaTime) .To<FLU>();
                (robotBoat.rotation * Quaternion.Inverse(robotBoatRotLast)).ToAngleAxis(out angle, out axes);
                robotBoatOdomMsg.twist.twist.angular = (axes * (angle * Mathf.Deg2Rad / Time.deltaTime)).To<FLU>();
                m_Ros.Publish(robotFrame + "_0/" + odomFrame,robotBoatOdomMsg);

                robotBoatPosLast = robotBoat.position;
                robotBoatRotLast = robotBoat.rotation;
            }

            if(haveTrafficBoats) {
                for(int i = 0; i < trafficBoats.Length; i++) {
                    Vector3<FLU> trafficPos = trafficBoats[i].position.To<FLU>();

                    trafficBoatsPoseMsg.transforms[i].header = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), rootFrame);
                    trafficBoatsPoseMsg.transforms[i].transform.translation = new Vector3Msg(trafficPos.x, trafficPos.y, trafficPos.z);
                    trafficBoatsPoseMsg.transforms[i].transform.rotation = trafficBoats[i].rotation.To<FLU>();

                    trafficBoatsOdomMsgs[i].header = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), rootFrame);
                    trafficBoatsOdomMsgs[i].pose.pose.position = new PointMsg(trafficPos.x, trafficPos.y, trafficPos.z);
                    trafficBoatsOdomMsgs[i].pose.pose.orientation = trafficBoats[i].rotation.To<FLU>();
                    trafficBoatsOdomMsgs[i].twist.twist.linear = ((trafficBoats[i].position - trafficBoatPosLast[i]) / Time.deltaTime).To<FLU>();
                    (trafficBoats[i].rotation * Quaternion.Inverse(trafficBoatRotLast[i])).ToAngleAxis(out angle, out axes);
                    trafficBoatsOdomMsgs[i].twist.twist.angular = (axes * (angle * Mathf.Deg2Rad / Time.deltaTime)).To<FLU>();
                    m_Ros.Publish(robotFrame + "_" + (i+1) + "/" + odomFrame,trafficBoatsOdomMsgs[i]);

                    trafficBoatPosLast[i] = trafficBoats[i].position;
                    trafficBoatRotLast[i] = trafficBoats[i].rotation;
                }
                m_Ros.Publish(tfTopic, trafficBoatsPoseMsg);
            }
        }
    }
}
