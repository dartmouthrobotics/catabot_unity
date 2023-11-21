using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    Mesh m_Mesh;
    MeshCollider m_Collider;
    Vector3[] m_Vertices;
    int[] m_Triangles;

    public int m_XPoints = 20;
    public int m_ZPoints = 20;
    public float m_UnitLength = 5f;
    public float m_patternDensity = 6f;
    public float height = 20f;

    // Start is called before the first frame update
    void Start()
    {

        m_Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_Mesh;
        m_Collider = GetComponent<MeshCollider>();

        //StartCoroutine(CreateShape());
        CreateShape();
        UpdateMesh();
    }

    private void Update()
    {
        //UpdateMesh();
    }

    void CreateShape()
    {
        Transform origin = GetComponent<Transform>();
        m_Vertices = new Vector3[(m_XPoints + 1) * (m_ZPoints + 1)];
        float xRandomOffset = Random.Range(0f, m_XPoints);
        float zRandomOffset = Random.Range(0f, m_ZPoints);

        for (int i = 0, z = 0; z <= m_ZPoints; z++)
        {
            for (int x = 0; x <= m_XPoints; x++)
            {
                //float y = Mathf.PerlinNoise((x +  xRandomOffset) * patternDensity , (z + zRandomOffset) * patternDensity) * 20f;
                float xcoord = xRandomOffset + (float)x / m_XPoints * m_patternDensity;
                float zcoord = zRandomOffset + (float)z / m_ZPoints * m_patternDensity;
                float y = Mathf.PerlinNoise(xcoord, zcoord) * height;
                m_Vertices[i] = new Vector3((x - ((float)m_XPoints / 2)) * m_UnitLength, y, (z - ((float)m_ZPoints / 2)) * m_UnitLength);
                i++;
            }
        }

        m_Triangles = new int[m_XPoints * m_ZPoints * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < m_ZPoints; z++)
        {
            for (int x = 0; x < m_XPoints; x++)
            {
                m_Triangles[tris + 0] = vert + 0;
                m_Triangles[tris + 1] = vert + m_XPoints + 1;
                m_Triangles[tris + 2] = vert + 1;
                m_Triangles[tris + 3] = vert + 1;
                m_Triangles[tris + 4] = vert + m_XPoints + 1;
                m_Triangles[tris + 5] = vert + m_XPoints + 2;

                vert++;
                tris += 6;

                //yield return new WaitForSeconds(0.002f);

            }
            vert++;
        }

    }

    // Update is called once per frame
    void UpdateMesh()
    {
        m_Mesh.Clear();
        m_Mesh.vertices = m_Vertices;
        m_Mesh.triangles = m_Triangles;

        m_Mesh.RecalculateNormals();
        m_Collider.sharedMesh = m_Mesh;
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //        return;

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], .1f);
    //    }
    //}

    public Vector2 GetSize()
    {
        return new Vector2((float)m_XPoints, (float)m_ZPoints);
    }

    public float GetSideLength()
    {
        return m_UnitLength;
    }
}
