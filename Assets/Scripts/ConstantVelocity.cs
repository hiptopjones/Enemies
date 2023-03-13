using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantVelocity : MonoBehaviour
{
    [SerializeField]
    private float speed;

    private void Update()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;
    }
}
