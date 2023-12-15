using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

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

    private const string tfTopic = "tf";
    private const string odomFrame = "odom";
    private const string robotFrame = "robot";
    private const string baseScanFrame = "base_scan";

    private void Awake () {
        if(broadcastROSMessages) {
            // Get ROS connection static instance
            m_Ros = ROSConnection.GetOrCreateInstance();
            m_Ros.RegisterPublisher<TFMessageMsg>(tfTopic);

            if(robotBoat != null) {
                haveRobotBoat = true;
                HeaderMsg robotHeader = new HeaderMsg(0, new TimeMsg(), odomFrame);
                Vector3<FLU> robotPos = robotBoat.position.To<FLU>();
                TransformMsg robotTrans = new TransformMsg(
                    new Vector3Msg(robotPos.x, robotPos.y, robotPos.z),
                    robotBoat.rotation.To<FLU>()
                );
                robotBoatPoseMsg.transforms[0] = new TransformStampedMsg(robotHeader, robotFrame, robotTrans);
                HeaderMsg baseScanHeader = new HeaderMsg(0, new TimeMsg(), robotFrame);
                TransformMsg baseScanTrans = new TransformMsg(
                    new Vector3Msg(0,0,0),
                    Quaternion.identity.To<FLU>()
                );
                robotBoatPoseMsg.transforms[1] = new TransformStampedMsg(baseScanHeader, baseScanFrame, baseScanTrans);
                // Finally send the message to server_endpoint.py running in ROS
                m_Ros.Publish(tfTopic, robotBoatPoseMsg);
            }

            if(trafficBoats != null && trafficBoats.Length > 0) {
                haveTrafficBoats = true;
                trafficBoatsPoseMsg = new TFMessageMsg(new TransformStampedMsg[trafficBoats.Length]);

                for(int i = 0; i < trafficBoats.Length; i++) {
                    HeaderMsg header = new HeaderMsg(0, new TimeMsg(), odomFrame);
                    Vector3<FLU> trafficPos = trafficBoats[i].position.To<FLU>();
                    TransformMsg trans = new TransformMsg(
                        new Vector3Msg(trafficPos.x, trafficPos.y, trafficPos.z),
                        trafficBoats[i].rotation.To<FLU>()
                    );
                    trafficBoatsPoseMsg.transforms[i] = new TransformStampedMsg(header, trafficBoats[i].name, trans);
                }
                // Finally send the message to server_endpoint.py running in ROS
                m_Ros.Publish(tfTopic, trafficBoatsPoseMsg);
            }
        }
    }

    private void FixedUpdate() {
        if(broadcastROSMessages) {
            if(haveRobotBoat) {
                Vector3<FLU> pos = robotBoat.position.To<FLU>();
                robotBoatPoseMsg.transforms[0].transform.translation = new Vector3Msg(pos.x, pos.y, pos.z);
                robotBoatPoseMsg.transforms[0].transform.rotation = robotBoat.rotation.To<FLU>();
                m_Ros.Publish(tfTopic, robotBoatPoseMsg);
            }

            if(haveTrafficBoats) {
                for(int i = 0; i < trafficBoats.Length; i++) {
                    Vector3<FLU> trafficPos = trafficBoats[i].position.To<FLU>();
                    trafficBoatsPoseMsg.transforms[i].transform.translation = new Vector3Msg(trafficPos.x, trafficPos.y, trafficPos.z);
                    trafficBoatsPoseMsg.transforms[i].transform.rotation = trafficBoats[i].rotation.To<FLU>();
                }
                m_Ros.Publish(tfTopic, trafficBoatsPoseMsg);
            }
        }
    }
}
