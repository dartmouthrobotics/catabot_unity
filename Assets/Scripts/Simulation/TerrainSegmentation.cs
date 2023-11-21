using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSegmentation : MonoBehaviour
{
    Material terrainSegmentationMat;

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Terrain child = transform.GetChild(i).gameObject.GetComponent<Terrain>();
            child.materialTemplate = terrainSegmentationMat;
        }
    }
}
