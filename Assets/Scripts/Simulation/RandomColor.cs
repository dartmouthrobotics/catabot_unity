using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RandomColor : MonoBehaviour {
    public bool randomColor;
    public Material[] colors;
    private Renderer objectRenderer;

    private void Awake() {
        objectRenderer = GetComponent<Renderer>();

        if(randomColor) {
            SetRandomColor();
        }
    }

    public void SetRandomColor() {
        objectRenderer.material = colors[Random.Range(0, colors.Length)];
    }
}
