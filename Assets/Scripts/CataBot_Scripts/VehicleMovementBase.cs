using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementBase : MonoBehaviour {
    [SerializeField]
    protected bool _movementActive = false;

    public virtual void SetMovementActive(bool value) {
        _movementActive = value;
    }

    public virtual void SetRosId(int id) { }

    public virtual string DisplayedName { get { return "Unknown"; } }
}
