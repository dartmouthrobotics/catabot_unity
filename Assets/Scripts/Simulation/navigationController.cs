using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class navigationController : MonoBehaviour
{
    enum State {
        Move,
        Rotate,
        Stay
    };

    public Transform[] wayPoints;
    int index = 0;
    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;
    float angleEps = 0.1f;
    float distanceEps = 0.01f;
    int increment = 1;
 
    State state = State.Rotate;

    void Start()
    {
        for (int i = 0; i < wayPoints.Length; i++)
        {
            Vector3 newPos = wayPoints[i].transform.position;
            newPos.y = transform.position.y;
            wayPoints[i].transform.position = newPos;
        }
    }

    void Update()
    {
        if (state == State.Move)
        {
            float distance = Vector3.Distance(transform.position, wayPoints[index].position);
            if (distance > distanceEps)
            {
                transform.position = Vector3.MoveTowards(transform.position, wayPoints[index].transform.position, moveSpeed * Time.deltaTime);
            }
            else
            {
                //index += increment;
                //if (index > wayPoints.Length - 1 || index < 0)
                //{
                //    increment *= -1;
                //    index += 2 * increment;
                //}
                index = (index + 1) % wayPoints.Length;

                //if (index == 1)
                //{
                //    state = State.Stay;
                //    Debug.Log("stay");
                //}
                //else
                //{
                    state = State.Rotate;
                //}
            }
        }
        else
        {
            Vector3 targetDirection = wayPoints[index].position - transform.position;
            if (Vector3.Angle(transform.forward, targetDirection)<= angleEps)
            {
                //if (index != 1)
                //{
                    state = State.Move;
                //}
          
            }
            else
            {
                Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
                transform.rotation = newRotation;
            }

        }

        // Move and rotate together
        //float distance = Vector3.Distance(transform.position, wayPoints[index].position);
        //if (distance > distanceEps)
        //{
        //    transform.position = Vector3.MoveTowards(transform.position, wayPoints[index].transform.position, moveSpeed * Time.deltaTime);
        //}
        //else
        //{
        //    index = (index + 1) % wayPoints.Length;
        //    state = State.Rotate;
        //}
        //Vector3 targetDirection = wayPoints[index].position - transform.position;
        //if (Vector3.Angle(transform.forward, targetDirection) >= angleEps)
        //{
        //    Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        //    Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        //    transform.rotation = newRotation;
        //}

    }
}
