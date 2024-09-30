using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.Sensors.Channels;
using Unity.Robotics.ROSTCPConnector;
using static UnityEditor.PlayerSettings;
using Unity.Robotics.Core;

public class LidarPerception : MonoBehaviour {
    private bool _lidarActive = true;

    // Perception Camera
    [SerializeField]
    private Camera _cam;
    [SerializeField]
    private PerceptionCamera _lidarCam;
    [SerializeField]
    private Transform _lidarTargetTransform;
    private PixelPositionChannel _pixelPosition;

    // Old Lidar Resolution stuff...
    // Only keep what is actually used!
    public enum HorizontalResolution { _512 = 512, _1024 = 1024, _2048 = 2048 }
    public HorizontalResolution HorzResolution = HorizontalResolution._512;
    private const float numDegreesCircle = 360f;
    private const float numDegreesHalfCircle = numDegreesCircle / 2f;
    private float degreesBetweenReadingsHorz = 0;
    private int halfResolutionHorz = 0;

    public enum VerticalResolution { _32 = 32, _64 = 64, _128 = 128 }
    public VerticalResolution VertResolution = VerticalResolution._32;
    private const float numDegreesVertical = 45f;
    private const float numDegreesAboveCenter = numDegreesVertical / 2f;
    private float degreesBetweenReadingsVert = 0;
    private int halfResolutionVert = 0;

    // Compute Shader
    [SerializeField]
    private ComputeShader _lidarShader;
    // - Inputs
    private int _randomSeedId;
    private ComputeBuffer _rawLidarPositionsBuffer;
    private int _rawLidarPositionsId;
    private float _dropDistribution;
    private int _dropDistributionId;
    private int _sensorOriginId;
    private int _sensorDirectionId;
    private int _lidarColorRainHitId;
    // - Outputs
    //private ComputeBuffer _rainLidarRangesBuffer;
    //private float[] _rainLidarRanges;
    //private int _rainLidarRangesId;
    private ComputeBuffer _rainLidarPositionsBuffer;
    private float3[] _rainLidarPositions;
    private int _rainLidarPositionsId;
    private ComputeBuffer _rainLidarParticleScalesBuffer;
    private float[] _rainLidarParticleScales;
    private int _rainLidarParticleScalesId;
    private ComputeBuffer _rainLidarParticleColorsBuffer;
    private float3[] _rainLidarParticleColors;
    private int _rainLidarParticleColorsId;
    //private ComputeBuffer _rainLidarMinimapPositionsBuffer;
    //private float2[] _rainLidarMinimapPositions;
    //private int _rainLidarMinimapPositionsId;
    // - Dimensions
    // --- NOTE: This system only works if HorzResolution * VertResolution
    // ---       is evenly divisible by 4
    private int _readingsTotalNum = 0;
    private int _readingsPerQuarterNum = 0;
    private const int kQuarterSides = 4;
    private const int kThreadNumPerDim = 8;
    private int _workgroupCountHorz = 0;
    private int _workgroupCountVert = 0;

    // Lidar
    private int _currentDirection = 0;
    private RectTransform[,] lidarDots;
    //public RectTransform lidarDotPrefab;
    public ParticleSystem LidarVisualization = null;
    private ParticleSystem.Particle[] lidarPoints;
    public bool LidarShowRainHits = false;
    private float _rateOfRainfall = 0;

    // ROS
    public bool broadcastROSMessages = false;
    ROSConnection m_Ros;
    HeaderMsg robotHeader = new HeaderMsg(0, new TimeMsg(), baseScanFrame);
    PointFieldMsg[] pointCloudFields = new PointFieldMsg[3];
    private PointCloud2Msg pointCloud2Msg = new PointCloud2Msg();
    private byte[] rosData;

    private const string pointCloud2Topic = "points2";
    private const string baseScanFrame = "base_scan";

    private void Awake() {
        if (_cam != null) {
            _cam.projectionMatrix = PerspectiveOffCenter(-_cam.nearClipPlane, _cam.nearClipPlane, -_cam.nearClipPlane / 2f, _cam.nearClipPlane / 2f, _cam.nearClipPlane, _cam.farClipPlane);
        }

        _readingsTotalNum = (int)HorzResolution * (int)VertResolution;

        _readingsPerQuarterNum = _readingsTotalNum / kQuarterSides;
        _workgroupCountHorz = (int)HorzResolution;
        _workgroupCountVert = (int)VertResolution;

        // Inputs
        _randomSeedId = Shader.PropertyToID("_RandomSeed");
        _rawLidarPositionsBuffer = new ComputeBuffer(_readingsPerQuarterNum, sizeof(float) * 4, ComputeBufferType.Default, ComputeBufferMode.Immutable);
        _rawLidarPositionsId = Shader.PropertyToID("_RawLidarPositions");
        _dropDistribution = LidarMath.DropSizeDistribution(_rateOfRainfall);
        _dropDistributionId = Shader.PropertyToID("_DropDistribution");
        _sensorOriginId = Shader.PropertyToID("_SensorOrigin");
        _sensorDirectionId = Shader.PropertyToID("_SensorDirection");
        _lidarColorRainHitId = Shader.PropertyToID("_LidarColorRainHit");

        // Outputs
        //_rainLidarRangesBuffer = new ComputeBuffer(_readingsPerQuarterNum, sizeof(float));
        //_rainLidarRanges = new float[_readingsTotalNum];
        //_rainLidarRangesId = Shader.PropertyToID("_RainLidarRanges");
        _rainLidarPositionsBuffer = new ComputeBuffer(_readingsPerQuarterNum, sizeof(float) * 3);
        _rainLidarPositions = new float3[_readingsTotalNum];
        _rainLidarPositionsId = Shader.PropertyToID("_RainLidarPositions");
        _rainLidarParticleScalesBuffer = new ComputeBuffer(_readingsPerQuarterNum, sizeof(float));
        _rainLidarParticleScales = new float[_readingsTotalNum];
        _rainLidarParticleScalesId = Shader.PropertyToID("_RainLidarParticleScales");
        _rainLidarParticleColorsBuffer = new ComputeBuffer(_readingsPerQuarterNum, sizeof(float) * 3);
        _rainLidarParticleColors = new float3[_readingsTotalNum];
        _rainLidarParticleColorsId = Shader.PropertyToID("_RainLidarParticleColors");
        //_rainLidarMinimapPositionsBuffer = new ComputeBuffer(_readingsPerQuarterNum, sizeof(float) * 2);
        //_rainLidarMinimapPositions = new float2[_readingsTotalNum];
        //_rainLidarMinimapPositionsId = Shader.PropertyToID("_RainLidarMinimapPositions");

        // Visualization
        lidarPoints = new ParticleSystem.Particle[_readingsTotalNum];

        if (broadcastROSMessages) {
            // Get ROS connection static instance
            m_Ros = ROSConnection.GetOrCreateInstance();
            m_Ros.RegisterPublisher<PointCloud2Msg>(pointCloud2Topic);

            // Set up the Lidar and Radar ROS messages

            pointCloudFields[0] = new PointFieldMsg("x", 0, 7, 1);
            pointCloudFields[1] = new PointFieldMsg("y", 4, 7, 1);
            pointCloudFields[2] = new PointFieldMsg("z", 8, 7, 1);
            rosData = new byte[12 * (uint)HorzResolution];
            //pointCloud2Msg = new PointCloud2Msg(robotHeader, (uint) VertResolution, (uint) HorzResolution, pointCloudFields, false, 12, (uint) rosData.Length, rosData, false);
        }
    }

    private void Start() {
        Debug.Log("Perception Cam Size: " + _lidarCam.cameraSensor.pixelWidth + ", " + _lidarCam.cameraSensor.pixelHeight);
        var channel = _lidarCam.EnableChannel<PixelPositionChannel>();
        channel.outputTextureReadback += ProcessPixelPosition;
        if (_lidarActive) {
            transform.rotation = Quaternion.identity;
            _currentDirection = 0;
            _lidarCam.RequestCapture();
        }
    }

    private void OnDestroy() {
        _rawLidarPositionsBuffer.Release();
        //_rainLidarRangesBuffer.Release();
        _rainLidarPositionsBuffer.Release();
        _rainLidarParticleScalesBuffer.Release();
        _rainLidarParticleColorsBuffer.Release();
        //_rainLidarMinimapPositionsBuffer.Release();
    }

    private void ProcessPixelPosition(int frame, NativeArray<float4> data) {
        // Set the data
        // - Inputs
        _lidarShader.SetVector(_randomSeedId, new Vector4(UnityEngine.Random.value * 20f + 1, UnityEngine.Random.value * 100f + 1, UnityEngine.Random.value * 100000f + 1, 1));
        _rawLidarPositionsBuffer.SetData(data);
        _lidarShader.SetBuffer(0, _rawLidarPositionsId, _rawLidarPositionsBuffer);
        _lidarShader.SetFloat(_dropDistributionId, _dropDistribution);
        _lidarShader.SetVector(_sensorOriginId, new Vector4(transform.position.x, transform.position.y, transform.position.z, 1));
        _lidarShader.SetInt(_sensorDirectionId, _currentDirection);
        _lidarShader.SetBool(_lidarColorRainHitId, LidarShowRainHits);
        // - Outputs
        //_lidarShader.SetBuffer(0, _rainLidarRangesId, _rainLidarRangesBuffer);
        _lidarShader.SetBuffer(0, _rainLidarPositionsId, _rainLidarPositionsBuffer);
        _lidarShader.SetBuffer(0, _rainLidarParticleScalesId, _rainLidarParticleScalesBuffer);
        _lidarShader.SetBuffer(0, _rainLidarParticleColorsId, _rainLidarParticleColorsBuffer);
        //_lidarShader.SetBuffer(0, _rainLidarMinimapPositionsId, _rainLidarMinimapPositionsBuffer);

        // Run the compute shader
        _lidarShader.Dispatch(0, _workgroupCountHorz, _workgroupCountVert, 1);

        // Copy the 90 degree quarter results out into the 360 degree full circle arrays
        int arrayStartPoint = _currentDirection * _readingsPerQuarterNum;
        //_rainLidarRangesBuffer.GetData(_rainLidarRanges, arrayStartPoint, 0, _readingsPerQuarterNum);
        _rainLidarPositionsBuffer.GetData(_rainLidarPositions, arrayStartPoint, 0, _readingsPerQuarterNum);
        _rainLidarParticleScalesBuffer.GetData(_rainLidarParticleScales, arrayStartPoint, 0, _readingsPerQuarterNum);
        _rainLidarParticleColorsBuffer.GetData(_rainLidarParticleColors, arrayStartPoint, 0, _readingsPerQuarterNum);
        //_rainLidarMinimapPositionsBuffer.GetData(_rainLidarMinimapPositions, arrayStartPoint, 0, _readingsPerQuarterNum);

        // Rotate the camera
        transform.Rotate(0, 90, 0);
        _currentDirection = (_currentDirection + 1) % kQuarterSides;
        // We've captured the data in all four directions. Process the combined data!
        if (_currentDirection == 0) {
            for (int i = 0; i < _readingsTotalNum; i++) {
                lidarPoints[i].position = _rainLidarPositions[i];
                lidarPoints[i].startSize = _rainLidarParticleScales[i];
                lidarPoints[i].startColor = new Color(_rainLidarParticleColors[i].x, _rainLidarParticleColors[i].y, _rainLidarParticleColors[i].z);
            }
            LidarVisualization.Play();
            LidarVisualization.Stop();
            LidarVisualization.SetParticles(lidarPoints, lidarPoints.Length);
            // If ROS is active, send the Point Cloud 2 Message
            if (broadcastROSMessages && _lidarActive) {
                // Put all of the position data into the rosData array
                _rainLidarPositionsBuffer.GetData(rosData);
                
                // Create and send the message
                TimeStamp timestamp = new TimeStamp(Clock.time);
                robotHeader = new HeaderMsg(
                    0,
                    new TimeMsg((uint)timestamp.Seconds, timestamp.NanoSeconds),
                    baseScanFrame
                );
                pointCloud2Msg = new PointCloud2Msg(
                    robotHeader,
                    (uint)VertResolution,
                    (uint)HorzResolution,
                    pointCloudFields,
                    false,
                    12,
                    (uint)rosData.Length,
                    rosData,
                    false
                );

                m_Ros.Publish(pointCloud2Topic, pointCloud2Msg);
            }

            transform.position = _lidarTargetTransform.position;
        }
        if (_lidarActive) {
            _lidarCam.RequestCapture();
        }
    }

    // Unity's recommendation
    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far) {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }

    public bool LidarActive {
        set {
            _lidarActive = value;
            if (!value) { // Disable all the visual dots if Lidar is disabled
                for (int i = 0; i < numDegreesCircle; i++) {
                    for (int j = 0; j < numDegreesVertical; j++) {
                        lidarDots[i, j].gameObject.SetActive(false);
                        LidarVisualization.Stop();
                        //lidarRanges[i, j] = 0;
                    }
                }
            } else { // Activate the Lidar!
                transform.rotation = Quaternion.identity;
                _currentDirection = 0;
                LidarVisualization.Play();
                _lidarCam.RequestCapture();
            }
        }
    }

    public float RateOfRainfall {
        set {
            _rateOfRainfall = value;
            _dropDistribution = LidarMath.DropSizeDistribution(_rateOfRainfall);
        }
    }
}
