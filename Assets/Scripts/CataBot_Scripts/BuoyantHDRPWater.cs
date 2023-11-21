using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class BuoyantHDRPWater : MonoBehaviour {
    public Vector3 centerOfMass = Vector3.zero; // Center Of Mass offset
    public float waterLevelOffset = 0f;

    [SerializeField]
    private DetectWaterHeight[] heights; // water height array
    public Vector3 waterUpVector;

    private void Update() {
        float avgHeight = 0;
        for (int i = 0; i < heights.Length; i++) {
            avgHeight += heights[i].waterPos.y;
        }
        avgHeight /= (float)heights.Length;

        Transform t = transform;
        Vector3 vec = t.position;
        vec.y = avgHeight + waterLevelOffset;
        t.position = vec;

        // Calculate the water up vector from the buoyancy objects
        if(heights.Length == 3) {
            Vector3 vec1 = (heights[0].waterPos - heights[1].waterPos).normalized;
            Vector3 vec2 = (heights[0].waterPos - heights[2].waterPos).normalized;
            Vector3 waterUpVector = Vector3.Cross(vec1, vec2);
            if(waterUpVector.y < 0) {
                waterUpVector = -waterUpVector;
            }
            transform.rotation = Quaternion.LookRotation(Vector3.Slerp(t.up, waterUpVector, Time.deltaTime), -transform.forward) * Quaternion.Euler(90, 0, 0);
        }
    }
}
