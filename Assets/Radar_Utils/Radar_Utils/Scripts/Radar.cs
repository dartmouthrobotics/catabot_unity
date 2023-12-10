using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{

    public RectTransform sweepTransform;
    private float rotationSpeed;
    public float radarDistance;
    public float radarRadius;
    private List<Collider> colliderList;
    public RectTransform radarDot;
    float sigma = 0.2f;
    float mu = 0;

    private float beamAngle = 0;
    
    // Start is called before the first frame update
    void Awake() {
        //sweepTransform = transform.Find("Sweep");
        rotationSpeed = 180f;
        radarDistance = 525f;
        radarRadius = 100f;
        colliderList = new List<Collider>();
    }

    // Update is called once per frame
    void Update() {
        float previousRotation = beamAngle - 180f;
        beamAngle = nfmod((beamAngle - (rotationSpeed * Time.deltaTime)), 360f);
        if (previousRotation < 0 && (beamAngle - 180f) >= 0) {
            print("CLEAR");
            colliderList.Clear();
        }

        print(beamAngle);

        float unityToMathAngle = 360f - nfmod(transform.rotation.eulerAngles.y, 360);
        sweepTransform.parent.localRotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.y);
        sweepTransform.eulerAngles = new Vector3(0, 0, beamAngle);

        RaycastHit raycastHit;
        Vector3 radarDirection = RadarMath.GetRadarVectorFromAngle(unityToMathAngle + beamAngle);
        print((unityToMathAngle + beamAngle) + " | " + radarDirection);
        Debug.DrawRay(transform.position, radarDirection.normalized * radarDistance, Color.red);
        if (Physics.Raycast(transform.position, radarDirection, out raycastHit, radarDistance)) {
            //print(radarDirection);
            //print(raycastHit.transform.name);
            colliderList.Add(raycastHit.collider);
            Vector3 direction = RadarMath.GetVectorFromAngle(unityToMathAngle + beamAngle);
            RectTransform dot = Instantiate<RectTransform>(radarDot, sweepTransform.parent);
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

    float nfmod(float a, float b) {
        return a - b * Mathf.Floor(a / b);
    }
}
