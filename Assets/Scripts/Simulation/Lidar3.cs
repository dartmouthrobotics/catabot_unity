using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Instancer))]
public class Lidar3 : MonoBehaviour
{
    float minVerticalAngle = -30f;
    float maxVerticalAngle = 20f;
    float minHorizontalAngle = -180f;
    float maxHorizontalAngle = 180f;
    Color RayColor = new Color(0.66f, 1f, 0.52f);
    int fixedFrame = 0;

    public Transform origin;
    public bool showRay = false;
    public int row = 64;
    public int col = 180;
    [HideInInspector]
    public Instancer instancer;
    public bool drawLidar = true;



    // Start is called before the first frame update
    void Start()
    {
        instancer = GetComponent<Instancer>();
        instancer.enabled = true;
    }


    void FixedUpdate()
    {
        if (fixedFrame % 3 == 0)
        {
            // Create new matrix for batching
            int AddedMatricies = 0;
            float scale = instancer.scale;
            List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();

            Vector3 fwd = Vector3.forward;
            for (int i = 0; i < row; i++)
            {
                float incRow = (float)(maxVerticalAngle - minVerticalAngle) / row;
                Vector3 v = Quaternion.AngleAxis(i * incRow + minVerticalAngle, Vector3.right) * fwd;
                for (int j = 0; j < col; j++)
                {
                    float incCol = (float)(maxHorizontalAngle - minHorizontalAngle) / col;
                    Vector3 dir = Quaternion.AngleAxis(j * incCol + minHorizontalAngle, Vector3.up) * v;

                    // Draw ray for angle visualization
                    if (j == 0 && showRay)
                    {
                        Debug.DrawRay(origin.position, dir * 20f, RayColor, 1f);
                    }

                    // Raycast and build matrix for batching
                    RaycastHit hit;
                    if (drawLidar && Physics.Raycast(origin.position, dir, out hit, 100))
                    {
                        if (AddedMatricies >= 1023 || batches.Count == 0)
                        {
                            batches.Add(new List<Matrix4x4>());
                            AddedMatricies = 0;
                        }
                        Matrix4x4 mat = Matrix4x4.TRS(hit.point, Quaternion.LookRotation(dir), Vector3.one * scale);
                        batches[batches.Count - 1].Add(mat);
                        AddedMatricies++;
                    }
                }
            }
            instancer.batches = batches;
        }
        fixedFrame++;



    }

}
