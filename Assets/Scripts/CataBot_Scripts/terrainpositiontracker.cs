using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainpositiontracker : MonoBehaviour
{
    void Start() {
        Debug.Log("Approx Y @ Base of measure: " + (transform.position.y - 0.5f));
        GetRestDestination();
        GetTerrainHeight();
    }


    void GetRestDestination() {
        Physics.Raycast(transform.position, Vector3.forward, out RaycastHit rayHit, Mathf.Infinity);
        Debug.Log("Raycast height: " + rayHit.point.y);
    }


    void GetTerrainHeight() {
        Debug.Log("SampleHeight value: " + Terrain.activeTerrain.SampleHeight(transform.position));
        Terrain ter = Terrain.activeTerrain;
        Vector3 terPosition = ter.transform.position;
        Debug.Log("terrain position: " + terPosition);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
