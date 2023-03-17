using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    [Tooltip("Last color in the array must be green")]
    [SerializeField] Color[] colors = new Color[1];

    Renderer rend;
    int currentColor = 0;

    Rigidbody rb;
    SphereCollider sphereCol;

    public float Radius => sphereCol.radius * transform.lossyScale.x;

    int collisionCount = 0;

    const float maxAirTime = 0.75f;
    float airTime = 0;

    public bool IsGreen => currentColor == colors.Length - 1;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        sphereCol = GetComponent<SphereCollider>();
    }

    private void OnCollisionEnter(Collision other) 
    {
        collisionCount++;    
    }

    void OnCollisionExit(Collision other) 
    {
        collisionCount--;
    }

    void Update() 
    {
        Color color = rend.material.color;
        color.a = Mathf.Max(0, maxAirTime - airTime);
        rend.material.color = color;

        if (collisionCount <= 0)
        {
            airTime -= Time.deltaTime;
            if (airTime > maxAirTime)
            {
                OrbManager.Instance.pool.Release(this);
            }
        }
        else
        {
            airTime = 0;
        }
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

    public void ResetProperties()
    {
        currentColor = Random.Range(0, colors.Length);
        UpdateColor();

        airTime = 0;
        collisionCount = 0;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.localScale = Vector3.one * Random.Range(0.025f, 0.1f);
    }
}
