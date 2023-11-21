using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lidar : MonoBehaviour
{
    public GameObject particle;
    GameObject[,] particles;
    public int row = 64;
    public int col = 180;
    float minVerticalAngle = -30f;
    float maxVerticalAngle = 20f;
    float minHorizontalAngle = -180f;
    float maxHorizontalAngle = 180f;
    public Color NoObstructionColor = new Color(0.66f, 1f, 0.52f);
    public Color ObstructionColor = new Color(0.66f, 1f, 0.52f);
    List<Color> colors;
    public Transform folder;
    public Transform origin;
    Color[] segmentationColors = new Color[] { new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(0f, 0f, 1f) };
    int fixedFrame = 0;


    // Start is called before the first frame update
    void Start()
    {
        particles = new GameObject[row, col];
        colors = new List<Color>();
        float colorInc = 0.5f / ((float)row);
        for (int i = 0; i < row; i++)
        {

            colors.Add(Color.HSVToRGB(colorInc * i, 1, 1));
            for (int j = 0; j < col; j++)
            {
                particles[i, j] = Instantiate(particle, particle.transform.position, particle.transform.rotation);
                particles[i, j].GetComponent<Renderer>().material.color = colors[i];
                particles[i, j].transform.parent = folder;
            }
        }

    }

    void FixedUpdate()
    {
        if (fixedFrame % 30 == 0)
        {
            Vector3 fwd = Vector3.forward;
            Vector3 randomPos = new Vector3(-100f, 0f, 0f);
            for (int i = 0; i < row; i++)
            {
                float incRow = (float)(maxVerticalAngle - minVerticalAngle) / row;
                Vector3 v = Quaternion.AngleAxis(i * incRow + minVerticalAngle, Vector3.right) * fwd;
                for (int j = 0; j < col; j++)
                {
                    float incCol = (float)(maxHorizontalAngle - minHorizontalAngle) / col;
                    Vector3 dir = Quaternion.AngleAxis(j * incCol + minHorizontalAngle, Vector3.up) * v;
                    RaycastHit hit;
             
                    // particles[i, j].transform.position = randomPos;
                    if (Physics.Raycast(origin.position, dir, out hit, 1000))
                    {
                        if (j == 0)
                        {
                            //Debug.DrawLine(origin.position, hit.point, ObstructionColor, Time.fixedDeltaTime, true);
                        }
                        particles[i, j].transform.position = hit.point;
                        particles[i, j].transform.rotation = Quaternion.LookRotation(dir);
                        //if (hit.collider.gameObject.CompareTag("Cube"))
                        //{
                        //    particles[i, j].GetComponent<Renderer>().material.color = segmentationColors[0];
                        //}
                        //else if (hit.collider.gameObject.CompareTag("Sphere"))
                        //{
                        //    particles[i, j].GetComponent<Renderer>().material.color = segmentationColors[1];
                        //}
                        //else
                        //{
                        //    particles[i, j].GetComponent<Renderer>().material.color = segmentationColors[2];
                        //}

                    }
                    else
                    {
                        particles[i, j].transform.position = randomPos;
                        //if (j == 0)
                        //{
                        //    Debug.DrawRay(origin.position, dir * 20f, NoObstructionColor);
                        //}
                    }
                }
            }
        }
        fixedFrame++;


    }




}
