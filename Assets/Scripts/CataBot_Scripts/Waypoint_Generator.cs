using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint_Generator : MonoBehaviour
{
    public Transform[] waypoints;
    int present_waypoint = 0;

    public Transform boat;
    public bool oceanActive = true;
    public float speed = 1.00f;
    public float rotspeed = 0.1f;
    
    void Start()
    {
        int waypointCount = transform.childCount;
        waypoints = new Transform[waypointCount];
        for(int i = 0; i < waypointCount; i++) {
            waypoints[i] = transform.GetChild(i);
        }
        oceanActive = GameObject.Find("Ocean").activeInHierarchy;
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
                present_waypoint++;
                
                if(present_waypoint >= waypoints.Length) {
                    present_waypoint = 0;
                }
            }

            boat.Translate(0, 0, speed * Time.deltaTime);
        }
    }
}
