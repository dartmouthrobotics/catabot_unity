using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindEmitterForRigidbody : MonoBehaviour
{
    public float windStrength = 0;

    public Vector3 WindVector {
        get { return transform.forward * windStrength; }
    }
}
