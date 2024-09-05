using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

//[RequireComponent(typeof(ParticleSystem))]
public class Radar : MonoBehaviour
{
    ROSConnection m_Ros;

    public bool broadcastROSMessages = true;
    private bool _lidarActive = true;
    private bool _radarActive = true;

    public Transform LidarOrigin = null;
    public Transform RadarOrigin = null;

    public ParticleSystem LidarVisualization = null;
    private ParticleSystem.Particle[] lidarPoints = new ParticleSystem.Particle[numDegreesCircle * numDegreesVertical];
    public bool LidarShowRainHits = false;

    public RectTransform sweepTransform;
    private float rotationSpeed;
    private float radarRange;
    private float radarGraphicRange;
    private float radarGraphicRadius;
    private List<Collider> colliderList;
    public RectTransform radarDot;
    float sigma = 0.2f;
    float mu = 0;

    float unityToMathAngle = 0;
    private float beamAngle = 0;

    public enum HorizontalResolution { _32 = 0, _64 = 1, _128 = 2 }
    public HorizontalResolution HorzResolution = HorizontalResolution._32;
    private int[] numSamplesCircle = { 512, 1024, 2048 };
    private const int numDegreesHalfCircle = numDegreesCircle / 2;
    private const int numDegreesAboveCenter = 5;
    private const int numDegreesVertical = 2 * numDegreesAboveCenter + 1;

    private RaycastHit lidarHit;
    private float lidarRange;
    private float[,] lidarRanges = new float[numDegreesCircle, numDegreesVertical];
    private float[,] lidarIntensities = new float[numDegreesCircle, numDegreesVertical]; // Will never be filled. Just for ROS messages.
    private RectTransform[,] lidarDots = new RectTransform[numDegreesCircle, numDegreesVertical];
    public RectTransform lidarDotPrefab;

    private LaserScanMsg laserScanMsg = new LaserScanMsg();

    private const string scanTopic = "scan";
    private const string baseScanFrame = "base_scan";

    double m_TimeLastScanBeganSeconds = -1;
    double m_TimeNextScanSeconds = -1;

    public float beamRadius = 0.02f; // 5 mm
    public float rateOfRainfall = 0f; // Units of mm / hr

    void Awake() {
        //sweepTransform = transform.Find("Sweep");
        rotationSpeed = 180f;
        radarGraphicRange = 525f;
        radarGraphicRadius = 100f;
        radarRange = 250f;
        lidarRange = 50f;
        colliderList = new List<Collider>();
        for(int i = 0; i < numDegreesCircle; i++) {
            for (int j = 0; j < numDegreesVertical; j++) {
                lidarDots[i,j] = Instantiate<RectTransform>(lidarDotPrefab, sweepTransform.parent);
                lidarDots[i,j].gameObject.SetActive(false);
            }
        }

        if(broadcastROSMessages) {
            // Get ROS connection static instance
            m_Ros = ROSConnection.GetOrCreateInstance();
            m_Ros.RegisterPublisher<LaserScanMsg>(scanTopic);

            // Set up the Lidar and Radar ROS messages
            HeaderMsg robotHeader = new HeaderMsg(0, new TimeMsg(), baseScanFrame);
            laserScanMsg = new LaserScanMsg(robotHeader, -Mathf.PI, Mathf.PI, Mathf.Deg2Rad, Time.deltaTime, Time.deltaTime, 0, lidarRange, new float[0], new float[0]);
        }
    }

    // Update is called once per frame
    void Update() {
        // Get the robot's rotation in the standard mathematical coordinate system
        unityToMathAngle = (float)numDegreesCircle - nfmod(transform.rotation.eulerAngles.y, numDegreesCircle);

        // Update the orientation of the radar circle
        sweepTransform.parent.localRotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.y);

        if (_radarActive) {
            RadarRaycast();
        }
        if (_lidarActive) {
            LidarRaycast();
        }

        if(broadcastROSMessages && Clock.NowTimeInSeconds >= m_TimeNextScanSeconds) {
            m_TimeLastScanBeganSeconds = Clock.Now;
            m_TimeNextScanSeconds = m_TimeLastScanBeganSeconds + 0.1;

            var timestamp = new TimeStamp(Clock.time);

            if (_lidarActive) {
                // TODO: Convert to a PointCloud2 Message!
                //var msg = new LaserScanMsg {
                //    header = new HeaderMsg {
                //        frame_id = baseScanFrame,
                //        stamp = new TimeMsg {
                //            sec = (uint)timestamp.Seconds,
                //            nanosec = timestamp.NanoSeconds,
                //        }
                //    },
                //    range_min = 0,
                //    range_max = lidarRange,
                //    angle_min = -Mathf.PI,
                //    angle_max = Mathf.PI,
                //    angle_increment = (Mathf.PI - -Mathf.PI) / (float)numDegreesCircle,
                //    time_increment = 0.01f,
                //    scan_time = 0.1f,
                //    intensities = lidarIntensities,
                //    ranges = lidarRanges,
                //    //angles = angles.ToArray();
                //};

                //m_Ros.Publish(scanTopic, msg);
            }
        }
    }

    float nfmod(float a, float b) {
        return a - b * Mathf.Floor(a / b);
    }

    private void RadarRaycast() {
        // Update the angle of the spinning radar beam
        float previousRotation = beamAngle - (float)numDegreesHalfCircle;
        beamAngle = nfmod((beamAngle - (rotationSpeed * Time.deltaTime)), numDegreesCircle);
        if (previousRotation < 0 && (beamAngle - (float)numDegreesHalfCircle) >= 0) {
            //print("CLEAR");
            colliderList.Clear();
        }
        // print(beamAngle);

        // Update the graphic of the radar beam
        sweepTransform.eulerAngles = new Vector3(0, 0, beamAngle);

        // Detect if anything is in the radar beam
        RaycastHit raycastHit;
        Vector3 radarDirection = RadarMath.GetRadarVectorFromAngle(unityToMathAngle + beamAngle, 0);
        // print((unityToMathAngle + beamAngle) + " | " + radarDirection);
        //Debug.DrawRay(transform.position, radarDirection.normalized * radarRange, Color.green);
        if (Physics.Raycast(RadarOrigin.position, radarDirection, out raycastHit, radarRange)) {
            //print(radarDirection);
            //print(raycastHit.transform.name);
            colliderList.Add(raycastHit.collider);

            // Create a radar dot if the radar hit something
            RectTransform dot = Instantiate<RectTransform>(radarDot, sweepTransform.parent);
            Vector3 direction = RadarMath.GetVectorFromAngle(unityToMathAngle + beamAngle).normalized;
            float noisyDistance = raycastHit.distance + RadarMath.GaussianMath(mu, sigma);
            dot.anchoredPosition = (direction) * (noisyDistance * radarGraphicRadius / radarGraphicRange);
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
        float dropDistribution = LidarMath.DropSizeDistribution(rateOfRainfall);
        float beamVolumeOneMeter = Mathf.PI * Mathf.Pow(beamRadius, 2f);
        bool printFirstPoint = true;
        for (int i = 0; i < numDegreesCircle; i++){
            for (int j = 0; j < numDegreesVertical; j++) {
                Vector3 lidarDirection = RadarMath.GetRadarVectorFromAngle(unityToMathAngle + (float)i, (float)(j - numDegreesAboveCenter));
                bool hitObject = Physics.Raycast(LidarOrigin.position, lidarDirection, out lidarHit, lidarRange);
                float distance = LidarMath.CalculateLidarDistanceWithRain(lidarHit.distance, hitObject, dropDistribution, beamVolumeOneMeter);
                if (distance > 0) {
                    float noisyDistance = distance + RadarMath.GaussianMath(mu, sigma);
                    //Debug.DrawRay(LidarOrigin.position, radarDirection * noisyDistance, distance == lidarHit.distance ? Color.green : Color.red, 0.01f);
                    lidarPoints[i * numDegreesVertical + j].position = LidarOrigin.position + lidarDirection * noisyDistance;
                    if (LidarShowRainHits) {
                        lidarPoints[i * numDegreesVertical + j].startColor = distance == lidarHit.distance ? Color.green : Color.red;
                    } else {
                        lidarPoints[i * numDegreesVertical + j].startColor = Color.HSVToRGB(noisyDistance / lidarRange, 1, 1);
                    }
                    lidarPoints[i * numDegreesVertical + j].startSize = noisyDistance / lidarRange;
                    if(printFirstPoint) {
                        Debug.Log(LidarOrigin.position + ", " + lidarPoints[i * numDegreesVertical + j].position);
                        printFirstPoint = false;
                    }
                    lidarDots[i,j].gameObject.SetActive(true);
                    Vector3 minimapDirection = RadarMath.GetVectorFromAngle(unityToMathAngle + (float)i).normalized;
                    lidarDots[i,j].anchoredPosition = minimapDirection * ((noisyDistance + RadarMath.GaussianMath(mu, sigma)) * radarGraphicRadius / radarGraphicRange);
                    lidarRanges[(i + numDegreesHalfCircle) % numDegreesCircle, j] = noisyDistance;
                } else {
                    lidarDots[i, j].gameObject.SetActive(false);
                    lidarRanges[(i + numDegreesHalfCircle) % numDegreesCircle, j] = 0;
                    lidarPoints[i * numDegreesVertical + j].position = Vector3.zero;
                    lidarPoints[i * numDegreesVertical + j].startSize = 0f;
                }
            }
        }

        LidarVisualization.SetParticles(lidarPoints, lidarPoints.Length);
    }

    public bool LidarActive { set {
            _lidarActive = value;
            if (!value) { // Disable all the visual dots if Lidar is disabled
                for (int i = 0; i < numDegreesCircle; i++) {
                    for (int j = 0; j < numDegreesVertical; j++) {
                        lidarDots[i, j].gameObject.SetActive(false);
                        lidarRanges[i, j] = 0;
                    }
                }
            }
    } }

    public bool RadarActive { set {
            _radarActive = value;
            sweepTransform.gameObject.SetActive(value);
    } }
}
