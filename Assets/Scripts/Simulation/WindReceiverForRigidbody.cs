using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindReceiverForRigidbody : MonoBehaviour
{
    public WindEmitterForRigidbody wind;
    private WindEmitterForRigidbody localWind;
    public Rigidbody rb;

    private void FixedUpdate() {
        if(rb != null) {
            if(localWind != null) {
                rb.AddForce(localWind.WindVector);
            } else {
                if(wind != null) {
                    rb.AddForce(wind.WindVector);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider col) {
        WindEmitterForRigidbody possibleWind = col.gameObject.GetComponent<WindEmitterForRigidbody>();
        if(possibleWind != null) {
            localWind = possibleWind;
        }
    }

    private void OnTriggerExit(Collider col) {
        if(localWind != null && col.gameObject.Equals(localWind.gameObject)) {
            localWind = null;
        }
    }
}
