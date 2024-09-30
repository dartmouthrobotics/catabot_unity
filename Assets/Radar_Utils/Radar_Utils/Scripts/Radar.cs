using UnityEngine;


public class Radar : MonoBehaviour
{
    private bool _radarActive = true;

    public Transform RadarOrigin = null;

    public RectTransform sweepTransform;
    private float rotationSpeed = 180f;
    private float radarRange = 250f;
    private float radarGraphicRange = 525f;
    private float radarGraphicRadius = 100f;
    public RectTransform radarDot;
    float sigma = 0.2f;
    float mu = 0;

    float unityToMathAngle = 0;
    private float beamAngle = 0;

    private const float numDegreesCircle = 360f;
    private const float numDegreesHalfCircle = numDegreesCircle / 2f;

    // Update is called once per frame
    void Update() {
        // Get the robot's rotation in the standard mathematical coordinate system
        unityToMathAngle = (float)numDegreesCircle - nfmod(transform.rotation.eulerAngles.y, numDegreesCircle);

        // Update the orientation of the radar circle
        sweepTransform.parent.localRotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.y);

        if (_radarActive) {
            RadarRaycast();
        }
    }

    float nfmod(float a, float b) {
        return a - b * Mathf.Floor(a / b);
    }

    private void RadarRaycast() {
        // Update the angle of the spinning radar beam
        float previousRotation = beamAngle - (float)numDegreesHalfCircle;
        beamAngle = nfmod((beamAngle - (rotationSpeed * Time.deltaTime)), numDegreesCircle);

        // Update the graphic of the radar beam
        sweepTransform.eulerAngles = new Vector3(0, 0, beamAngle);

        // Detect if anything is in the radar beam
        RaycastHit raycastHit;
        Vector3 radarDirection = RadarMath.GetRadarVectorFromAngle(unityToMathAngle + beamAngle, 0);

        if (Physics.Raycast(RadarOrigin.position, radarDirection, out raycastHit, radarRange)) {
            // Create a radar dot if the radar hit something
            RectTransform dot = Instantiate<RectTransform>(radarDot, sweepTransform.parent);
            Vector3 direction = RadarMath.GetVectorFromAngle(unityToMathAngle + beamAngle).normalized;
            float noisyDistance = raycastHit.distance + RadarMath.GaussianMath(mu, sigma);
            dot.anchoredPosition = (direction) * (noisyDistance * radarGraphicRadius / radarGraphicRange);
            Destroy(dot.gameObject, 1);
        }
    }

    public bool RadarActive { set {
            _radarActive = value;
            sweepTransform.gameObject.SetActive(value);
    } }
}
