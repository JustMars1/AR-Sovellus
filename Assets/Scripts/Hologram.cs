using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Hologram : MonoBehaviour
{
    [SerializeField] Color[] colors = new Color[1];

    Renderer rend;
    int currentColor = 0;

    Rigidbody rb;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();

        transform.localScale *= Random.Range(0.5f, 2.0f);

        currentColor = Random.Range(0, colors.Length);
        UpdateColor();
    }

    public void ChangeColor()
    {
        currentColor++;
        currentColor %= colors.Length;

        UpdateColor();
    }

    void UpdateColor()
    {
        rend.material.color = colors[currentColor];
    }
}
