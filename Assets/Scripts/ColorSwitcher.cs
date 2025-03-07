using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSwitcher : MonoBehaviour
{
    MeshRenderer meshRenderer;
    private Color[] colors = { Color.blue, Color.red, Color.green, Color.cyan, Color.magenta, Color.yellow, Color.grey };
    private int currentColorIndex = 0;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.blue;
    }

   

    public void SwitchColor()
    {
        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        meshRenderer.material.color = colors[currentColorIndex];
    }
}
