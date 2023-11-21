using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindReceiverForRigidbody : MonoBehaviour
{
    public WindEmitterForRigidbody wind;
    public Rigidbody rb;

    private void FixedUpdate() {
        if(wind != null && rb != null) {
            if (wind) {
                rb.AddForce(wind.WindVector);
            }
        }
    }
}
