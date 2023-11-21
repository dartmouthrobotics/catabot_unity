using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instancer : MonoBehaviour
{
    public Mesh mesh;
    public float scale = 0.1f;
    public Material[] materials;
    [HideInInspector]
    public List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();


    private void Update()
    {
        RenderBatches();
    }


    private void RenderBatches()
    {
        foreach (var batch in batches)
        {
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                Graphics.DrawMeshInstanced(mesh, i, materials[i], batch);
            }
        }
    }
}

