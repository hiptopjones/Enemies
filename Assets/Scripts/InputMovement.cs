using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 10.0f;

    [SerializeField]
    private float rotationSpeed = 100.0f;

    private void Update()
    {
        // From: https://docs.unity3d.com/ScriptReference/Input.GetAxis.html

        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        // Make it move 10 meters per second instead of 10 meters per frame...
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        // Move translation along the object's z-axis
        transform.Translate(0, 0, translation);

        // Rotate around our y-axis
        transform.Rotate(0, rotation, 0);
    }
}
