using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USVController : MonoBehaviour
{
    private float m_Speed = 7f;

    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.up * m_Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= transform.up * m_Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            //transform.Rotate(Vector3.up, -40 * Time.deltaTime);
            rb.AddTorque(0, 0, -5 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            transform.Rotate(Vector3.up, 40 * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * m_Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * m_Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * m_Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * m_Speed * Time.deltaTime;
        }
    }
}
