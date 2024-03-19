using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMultiMovementManager : MonoBehaviour {
    [SerializeField]
    private VehicleMovementBase[] _movementTypes = null;

    public string[] MovementNames() {
        string[] names = new string[_movementTypes.Length];
        for(int i = 0; i < names.Length; i++) {
            names[i] = _movementTypes[i].DisplayedName;
        }

        return names;
    }

    public void SetRosId(int id) {
        for(int i = 0; i < _movementTypes.Length; i++) {
            _movementTypes[i].SetRosId(id);
        }
    }

    public void SetMovementActive(int choice) {
        for(int i = 0; i < _movementTypes.Length; i++) {
            // There should only be movement if a valid choice has been made
            // If an invalid choice has been provided, set all of them inactive
            _movementTypes[i].SetMovementActive(i == choice);
        }
    }
}
