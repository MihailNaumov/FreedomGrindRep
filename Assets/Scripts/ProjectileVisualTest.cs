using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileVisualTest : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 180f; // градусов в секунду
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        float pulse = 0.3f + Mathf.Sin(Time.time * 15f) * 0.1f;
        transform.localScale = Vector3.one * (1f + pulse);
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
