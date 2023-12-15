using Cinemachine.Utility;
using UnityEngine;

[RequireComponent(typeof(Waypoint_Generator))]
public class AutogenerateWaypoints : MonoBehaviour {
    private Waypoint_Generator waypointGenerator;
    public Transform buoyPrefab;

    public bool generateWaypoints = true;
    public int numWaypoints = 5;
    public Vector2 minRange = Vector2.zero;
    public Vector2 maxRange = Vector2.one * 1000f;
    [Tooltip("How deep must the water be for the boat to safely travel. Enter a positive number.")]
    public float safeDepth = 2f;
    [Tooltip("How far must the obstacle be away from the boat for the boat to safely travel. Enter a positive number.")]
    public float safeWidth = 1f;
    private float terrainMaxHeight = 1000f;
    private float depthCheck = 1002f;

    private void Awake() {
        waypointGenerator = GetComponent<Waypoint_Generator>();
        waypointGenerator.generateWaypointsFromChildren = generateWaypoints;

        depthCheck = terrainMaxHeight + safeDepth;

        if(generateWaypoints) {
            GenerateWaypoints();
        }
    }

    private void GenerateWaypoints() {
        Transform previousWaypoint = null;
        for(int i = 0; i < numWaypoints; i++) {
            Transform potentialWaypoint = CreateNextWaypoint(previousWaypoint, i);
            // Did not find a good point. Try again.
            if(potentialWaypoint == null) {
                i--;
            } else { // Found a good waypoint. Move on to the next one.
                previousWaypoint = potentialWaypoint;
            }
        }
    }

    private Transform CreateNextWaypoint(Transform previousWaypoint, int waypointNum) {
        // Find a point where there is nothing above the water
        bool hitSomething = true;
        Vector3 nextLocation = Vector3.zero;
        while(hitSomething) {
            nextLocation = new Vector3(Random.Range(minRange.x, maxRange.x), 0, Random.Range(minRange.y, maxRange.y));
            hitSomething = Physics.Raycast(new Vector3(nextLocation.x, terrainMaxHeight, nextLocation.z), Vector3.down, depthCheck);
        }

        // If there was a previous waypoint, make sure that this new point is located in a straight line with nothing in the way
        if(previousWaypoint != null) {
            RaycastHit hit;
            Vector3 lineToNextPoint = nextLocation - previousWaypoint.position;
            
            // Check for obstacles in the way at the water surface level
            if(Physics.Raycast(previousWaypoint.position, lineToNextPoint.normalized, out hit, lineToNextPoint.magnitude)) {
                if(hit.distance < safeWidth) {
                    return null;
                } else {
                    nextLocation = hit.point - (lineToNextPoint.normalized * safeWidth);
                    lineToNextPoint = nextLocation - previousWaypoint.position;
                }
            }

            // Check for obstacles at the safe water depth level
            if(Physics.Raycast(previousWaypoint.position - (Vector3.down * safeDepth), lineToNextPoint.normalized, out hit, lineToNextPoint.magnitude)) {
                if(hit.distance < safeWidth) {
                    return null;
                } else {
                    nextLocation = hit.point - (lineToNextPoint.normalized * safeWidth);
                    lineToNextPoint = nextLocation - previousWaypoint.position;
                }
            }

            Instantiate(buoyPrefab, nextLocation + lineToNextPoint.normalized * (safeWidth - 1f), Quaternion.identity);
        }

        // Create the new waypoint and set the values properly
        Transform waypoint = new GameObject("waypoint" + waypointNum).transform;
        waypoint.position = nextLocation;
        waypoint.parent = transform;
        return waypoint;
    }
}
