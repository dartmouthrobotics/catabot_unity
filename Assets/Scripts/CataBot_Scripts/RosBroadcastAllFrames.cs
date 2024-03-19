using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using UnityEngine;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Nav;
using System.Collections.Generic;

public class RosBroadcastAllFrames : MonoBehaviour {
    // ROS Connector
    ROSConnection m_Ros;

    public bool broadcastROSMessages = true;
    private bool haveRobotBoat = false;
    private bool haveTrafficBoats = false;

    public Transform robotBoat;
    public List<Transform> trafficBoats = new List<Transform>();

    private TFMessageMsg robotBoatPoseMsg = new TFMessageMsg(new TransformStampedMsg[2]);
    private TFMessageMsg trafficBoatsPoseMsg;
    private List<TransformStampedMsg> trafficBoatTransformMsgs = new List<TransformStampedMsg>();

    private OdometryMsg robotBoatOdomMsg = new OdometryMsg();
    private List<OdometryMsg> trafficBoatsOdomMsgs = new List<OdometryMsg>();

    private Vector3 robotBoatPosLast = Vector3.zero;
    private List<Vector3> trafficBoatPosLast = new List<Vector3>();

    private Quaternion robotBoatRotLast = Quaternion.identity;
    private List<Quaternion> trafficBoatRotLast = new List<Quaternion>();

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

            if (trafficBoats != null && trafficBoats.Count > 0) {
                for (int i = 0; i < trafficBoats.Count; i++) {
                    AddTrafficBoat(trafficBoats[i], i + 1, true);
                }
            }
        }
    }

    public void AddTrafficBoat(Transform newBoat, int id, bool alreadyInList=false) {
        haveTrafficBoats = true;
        if (!alreadyInList) {
            trafficBoats.Add(newBoat);
        }

        var timestamp = new TimeStamp(Clock.time);
        HeaderMsg header = new HeaderMsg(0, new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds), rootFrame);
        Vector3<FLU> trafficPos = newBoat.position.To<FLU>();
        TransformMsg trans = new TransformMsg(
            new Vector3Msg(trafficPos.x, trafficPos.y, trafficPos.z),
            newBoat.rotation.To<FLU>()
        );
        trafficBoatTransformMsgs.Add(new TransformStampedMsg(header, robotFrame + "_" + id + "/" + odomFrame, trans));
        trafficBoatsPoseMsg = new TFMessageMsg(trafficBoatTransformMsgs.ToArray());
        m_Ros.Publish(tfTopic, trafficBoatsPoseMsg);

        OdometryMsg odomMsg = new OdometryMsg();
        odomMsg.header = header;
        odomMsg.child_frame_id = robotFrame + "_" + id;
        odomMsg.pose = new PoseWithCovarianceMsg(new PoseMsg(new PointMsg(trafficPos.x, trafficPos.y, trafficPos.z), newBoat.rotation.To<FLU>()), new double[36]);
        odomMsg.twist = new TwistWithCovarianceMsg(new TwistMsg(Vector3.zero.To<FLU>(), Vector3.zero.To<FLU>()), new double[36]);
        m_Ros.RegisterPublisher<OdometryMsg>(robotFrame + "_" + id + "/" + odomFrame);
        m_Ros.Publish(robotFrame + "_" + id + "/" + odomFrame, odomMsg);
        trafficBoatsOdomMsgs.Add(odomMsg);

        trafficBoatPosLast.Add(newBoat.position);
        trafficBoatRotLast.Add(newBoat.rotation);
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
                for(int i = 0; i < trafficBoats.Count; i++) {
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
