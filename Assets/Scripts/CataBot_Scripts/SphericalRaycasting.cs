using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalRaycasting : MonoBehaviour
{
    public float radius;
    public float maxDistance;
    public LayerMask layerMask;
    Collider[] hits; //SphereCastAll outputs an Array

    // Update is called once per frame
    void Update() {
       
            Cast();
       


    }
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, radius);
    }
    void Cast() {
        hits = Physics.OverlapSphere(transform.position, radius);
        string hitList = hits.Length + ": ";
        for(int i = 0; i < hits.Length; i++) {
            hitList += hits[i].gameObject.name + ", ";
        }
        //Debug.Log(hitList);
    }

    void hitPostion(float x, float y, float z) {
        this.transform.position = new Vector3(x, y, z);
    }
    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Terriane") {
            Vector3 linePos = col.transform.position;
            float linePosX = col.transform.position.x;
            float linePosY = col.transform.position.y;
            float linePosZ = col.transform.position.z;
            hitPostion(linePosX, linePosY, linePosZ);
        }
    }
}
