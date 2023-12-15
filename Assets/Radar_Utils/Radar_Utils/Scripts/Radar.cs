using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class Radar : MonoBehaviour
{
    ROSConnection m_Ros;

    public bool broadcastROSMessages = true;

    public RectTransform sweepTransform;
    private float rotationSpeed;
    private float radarRange;
    private float radarDistance;
    private float radarRadius;
    private List<Collider> colliderList;
    public RectTransform radarDot;
    float sigma = 0.2f;
    float mu = 0;

    float unityToMathAngle = 0;
    private float beamAngle = 0;

    private RectTransform[] lidarDots = new RectTransform[360];
    public RectTransform lidarDotPrefab;
    private RaycastHit lidarHit;
    private float lidarRange;
    private float[] lidarRanges = new float[360];

    private LaserScanMsg laserScanMsg = new LaserScanMsg();

    private const string scanTopic = "scan";
    private const string baseScanFrame = "base_scan";

    void Awake() {

        //sweepTransform = transform.Find("Sweep");
        rotationSpeed = 180f;
        radarDistance = 525f;
        radarRadius = 100f;
        radarRange = 250f;
        lidarRange = 50f;
        colliderList = new List<Collider>();
        for(int i = 0; i < 360; i++) {
            lidarDots[i] = Instantiate<RectTransform>(lidarDotPrefab, sweepTransform.parent);
            lidarDots[i].gameObject.SetActive(false);
        }

        if(broadcastROSMessages) {
            // Get ROS connection static instance
            m_Ros = ROSConnection.GetOrCreateInstance();
            m_Ros.RegisterPublisher<LaserScanMsg>(scanTopic);

            // Set up the Lidar and Radar ROS messages
            if(broadcastROSMessages) {
                HeaderMsg robotHeader = new HeaderMsg(0, new TimeMsg(), baseScanFrame);
                laserScanMsg = new LaserScanMsg(robotHeader, -Mathf.PI, Mathf.PI, Mathf.Deg2Rad, Time.deltaTime, Time.deltaTime, 0, lidarRange, new float[0], new float[0]);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        // Get the robot's rotation in the standard mathematical coordinate system
        unityToMathAngle = 360f - nfmod(transform.rotation.eulerAngles.y, 360);

        // Update the orientation of the radar circle
        sweepTransform.parent.localRotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.y);

        RadarRaycast();
        LidarRaycast();

        if(broadcastROSMessages) {
            laserScanMsg.time_increment = Time.deltaTime / 360f;
            laserScanMsg.scan_time = Time.deltaTime;
            laserScanMsg.ranges = lidarRanges;
            m_Ros.Publish(scanTopic, laserScanMsg);
        }
    }

    float nfmod(float a, float b) {
        return a - b * Mathf.Floor(a / b);
    }

    private void RadarRaycast() {
        // Update the angle of the spinning radar beam
        float previousRotation = beamAngle - 180f;
        beamAngle = nfmod((beamAngle - (rotationSpeed * Time.deltaTime)), 360f);
        if (previousRotation < 0 && (beamAngle - 180f) >= 0) {
            print("CLEAR");
            colliderList.Clear();
        }
        // print(beamAngle);

        // Update the graphic of the radar beam
        sweepTransform.eulerAngles = new Vector3(0, 0, beamAngle);

        // Detect if anything is in the radar beam
        RaycastHit raycastHit;
        Vector3 radarDirection = RadarMath.GetRadarVectorFromAngle(unityToMathAngle + beamAngle);
        // print((unityToMathAngle + beamAngle) + " | " + radarDirection);
        Debug.DrawRay(transform.position, radarDirection.normalized * radarRange, Color.green);
        if (Physics.Raycast(transform.position, radarDirection, out raycastHit, radarRange)) {
            //print(radarDirection);
            //print(raycastHit.transform.name);
            colliderList.Add(raycastHit.collider);

            // Create a radar dot if the radar hit something
            RectTransform dot = Instantiate<RectTransform>(radarDot, sweepTransform.parent);
            Vector3 direction = RadarMath.GetVectorFromAngle(unityToMathAngle + beamAngle).normalized;
            dot.anchoredPosition = (direction) * ((raycastHit.distance + RadarMath.GaussianMath(mu, sigma)) * radarRadius / radarDistance);
            Destroy(dot.gameObject, 1);
        }

        //float previousRotation = (sweepTransform.eulerAngles.z % 360) - 180;
        //sweepTransform.eulerAngles -= new Vector3(0, 0, rotationSpeed * Time.deltaTime);
        //float currentRotation = (sweepTransform.eulerAngles.z % 360) - 180;

        //if (previousRotation < 0 && currentRotation >= 0) {
        //    print("CLEAR");
        //    colliderList.Clear();
        //}

        //RaycastHit raycastHit;
        //Vector3 radarDirection = RadarMath.GetRadarVectorFromAngle(transform.rotation.eulerAngles.y + sweepTransform.eulerAngles.z);
        //Debug.DrawRay(transform.position, radarDirection.normalized * 100f, Color.red);
        //if (Physics.Raycast(transform.position, radarDirection, out raycastHit, radarDistance)) {
        //    print(radarDirection);
        //    print(raycastHit.transform.name);
        //    colliderList.Add(raycastHit.collider);
        //    Vector3 direction = RadarMath.GetVectorFromAngle(sweepTransform.eulerAngles.z + transform.rotation.eulerAngles.y); 
        //    Destroy(Instantiate(reddot, sweepTransform.position + (direction) * ((raycastHit.distance + RadarMath.GaussianMath(mu, sigma)) * radarRadius / radarDistance), Quaternion.identity, sweepTransform.parent), 1);
        //}
    }
        
    void LidarRaycast()
    {
        for(int i = 0; i < 360; i++){
            Vector3 radarDirection = RadarMath.GetRadarVectorFromAngle(unityToMathAngle + (float) i);
            if(Physics.Raycast(transform.position, radarDirection, out lidarHit, lidarRange)) {
                Debug.DrawRay(transform.position, lidarHit.point - transform.position, Color.red);
                lidarDots[i].gameObject.SetActive(true);
                Vector3 direction = RadarMath.GetVectorFromAngle(unityToMathAngle + (float) i).normalized;
                lidarDots[i].anchoredPosition = direction * ((lidarHit.distance + RadarMath.GaussianMath(mu, sigma)) * radarRadius / radarDistance);
                lidarRanges[(i + 180) % 360] = lidarHit.distance;
            } else {
                lidarDots[i].gameObject.SetActive(false);
                lidarRanges[(i + 180) % 360] = 0;
            }
        }
    }
}
