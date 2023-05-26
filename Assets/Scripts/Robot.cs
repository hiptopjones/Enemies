using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static RootMotion.FinalIK.GrounderQuadruped;

public class Robot : MonoBehaviour
{
    [SerializeField]
    private float robotHeight = 0.8f;

    [SerializeField]
    private LegsController legsController;

    // Average the z positions of the feet and create an xy plane
    // Position the body a fixed distance along the normal of the plane to position
    // Tilt the body to match the angle of the normal
    // Adjust the body position to keep center of mass over the feet

    // Use the velocity of the robot to apply an offset to the body tilt and position
    //  - Lean forward to help realism of motion

    // Use rotation and position to show personality
    //  - aggression (leaning in)
    //  - fleeing (leaning out)

    private void Update()
    {
        // Adjust the position
        transform.position = new Vector3(transform.position.x, legsController.AverageTerrainHeight + robotHeight, transform.position.z);

        // Orient the body with the average foot height
        Vector3 forward = Vector3.Cross(transform.right, legsController.AverageFootNormal);
        transform.rotation = Quaternion.LookRotation(forward, legsController.AverageFootNormal);
    }
}
