using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : VehicleMovementBase
{
    public bool generateWaypoints = true;
    int present_waypoint = 0;
    private bool forwardDirection = true;

    public float speed = 1.00f;
    public float rotspeed = 0.1f;

    public Transform buoyPrefab;

    public int numWaypoints = 5;
    public Vector2 minRange = Vector2.zero;
    public Vector2 maxRange = Vector2.one * 1000f;
    [Tooltip("How deep must the water be for the boat to safely travel. Enter a positive number.")]
    public float safeDepth = 2f;
    [Tooltip("How far must the obstacle be away from the boat for the boat to safely travel. Enter a positive number.")]
    public float safeWidth = 1f;
    private float terrainMaxHeight = 1000f;
    private float depthCheck = 1002f;

    public List<GameObject> buoys = new List<GameObject>();
    private Transform waypointsParent;
    public List<Transform> waypoints = new List<Transform>();

    void Start()
    {
        depthCheck = terrainMaxHeight + safeDepth;

        if (generateWaypoints) {
            GenerateWaypoints();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (_movementActive) {
            if (waypoints.Count == 0) return;

            Vector3 lookAtGoal = new Vector3(waypoints[present_waypoint].position.x, transform.position.y, waypoints[present_waypoint].position.z);
            Vector3 direction = lookAtGoal - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotspeed);

            if(direction.magnitude < 0.4f) {
                if(forwardDirection) {
                    present_waypoint++;
                    if(present_waypoint >= waypoints.Count) {
                        present_waypoint = waypoints.Count - 1;
                        forwardDirection = false;
                    }
                } else {
                    present_waypoint--;
                    if(present_waypoint <= 0) {
                        present_waypoint = 0;
                        forwardDirection = true;
                    }
                }
            }

            transform.Translate(0, 0, speed * Time.deltaTime);
        }
    }

    public void GenerateWaypoints() {
        if(waypointsParent == null) {
            waypointsParent = new GameObject("WaypointsParent").transform;
        }

        for (int i = 0; i < buoys.Count; i++) {
            buoys[i].SetActive(false);
        }

        for (int i = waypoints.Count - 1; i >= 0; i--) {
            Destroy(waypoints[i].gameObject);
        }
        waypoints.Clear();

        Transform previousWaypoint = null;
        for (int i = 0; i < numWaypoints; i++) {
            Transform potentialWaypoint = CreateNextWaypoint(previousWaypoint, i);
            // Did not find a good point. Try again.
            if (potentialWaypoint == null) {
                i--;
            } else { // Found a good waypoint. Move on to the next one.
                previousWaypoint = potentialWaypoint;
                waypoints.Add(potentialWaypoint);
            }
        }

        if (waypoints.Count > 0) {
            transform.position = waypoints[0].position;
        }
    }

    private Transform CreateNextWaypoint(Transform previousWaypoint, int waypointNum) {
        // Find a point where there is nothing above the water
        bool hitSomething = true;
        Vector3 nextLocation = Vector3.zero;
        while (hitSomething) {
            nextLocation = new Vector3(Random.Range(minRange.x, maxRange.x), 0, Random.Range(minRange.y, maxRange.y));
            hitSomething = Physics.Raycast(new Vector3(nextLocation.x, terrainMaxHeight, nextLocation.z), Vector3.down, depthCheck);
        }

        // If there was a previous waypoint, make sure that this new point is located in a straight line with nothing in the way
        if (previousWaypoint != null) {
            RaycastHit hit;
            Vector3 lineToNextPoint = nextLocation - previousWaypoint.position;

            // Check for obstacles in the way at the water surface level
            if (Physics.Raycast(previousWaypoint.position, lineToNextPoint.normalized, out hit, lineToNextPoint.magnitude)) {
                if (hit.distance < safeWidth) {
                    return null;
                } else {
                    nextLocation = hit.point - (lineToNextPoint.normalized * safeWidth);
                    lineToNextPoint = nextLocation - previousWaypoint.position;
                }
            }

            // Check for obstacles at the safe water depth level
            if (Physics.Raycast(previousWaypoint.position - (Vector3.down * safeDepth), lineToNextPoint.normalized, out hit, lineToNextPoint.magnitude)) {
                if (hit.distance < safeWidth) {
                    return null;
                } else {
                    nextLocation = hit.point - (lineToNextPoint.normalized * safeWidth);
                    lineToNextPoint = nextLocation - previousWaypoint.position;
                }
            }

            if (waypointNum < buoys.Count) {
                buoys[waypointNum].SetActive(true);
                buoys[waypointNum].transform.position = nextLocation + lineToNextPoint.normalized * (safeWidth - 1f);
            } else {
                buoys.Add(Instantiate(buoyPrefab, nextLocation + lineToNextPoint.normalized * (safeWidth - 1f), Quaternion.identity).gameObject);
            }
        }

        // Create the new waypoint and set the values properly
        Transform waypoint = new GameObject("waypoint" + waypointNum).transform;
        waypoint.position = nextLocation;
        waypoint.parent = waypointsParent;
        return waypoint;
    }

    public override void SetMovementActive(bool value) {
        if (value) {
            GenerateWaypoints();
        }
        _movementActive = value;

        for (int i = 0; i < buoys.Count; i++) {
            buoys[i].SetActive(gameObject.activeInHierarchy);
        }
    }

    public override string DisplayedName { get { return "Waypoints"; } }
}
