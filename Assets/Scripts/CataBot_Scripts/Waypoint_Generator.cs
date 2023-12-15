using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint_Generator : MonoBehaviour
{
    public bool generateWaypointsFromChildren = true;
    public Transform[] waypoints;
    int present_waypoint = 0;
    private bool forwardDirection = true;

    private bool oceanActive = true;

    public Transform boat;
    public float speed = 1.00f;
    public float rotspeed = 0.1f;
    
    void Start()
    {
        oceanActive = GameObject.Find("Ocean").activeInHierarchy;
        if(generateWaypointsFromChildren) {
            GenerateWaypointsFromChildren();
        }
    }

    public void GenerateWaypointsFromChildren() {
        int waypointCount = transform.childCount;
        waypoints = new Transform[waypointCount];
        for(int i = 0; i < waypointCount; i++) {
            waypoints[i] = transform.GetChild(i);
        }

        if(waypointCount > 0) {
            boat.position = waypoints[0].position;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (oceanActive) {
            if (waypoints.Length == 0) return;

            Vector3 lookAtGoal = new Vector3(waypoints[present_waypoint].position.x, boat.position.y, waypoints[present_waypoint].position.z);
            Vector3 direction = lookAtGoal - boat.position;
            boat.rotation = Quaternion.Slerp(boat.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotspeed);

            if(direction.magnitude < 0.4f) {
                if(forwardDirection) {
                    present_waypoint++;
                    if(present_waypoint >= waypoints.Length) {
                        present_waypoint = waypoints.Length - 1;
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

            boat.Translate(0, 0, speed * Time.deltaTime);
        }
    }
}
