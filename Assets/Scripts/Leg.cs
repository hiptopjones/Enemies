using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Leg : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float footHeight = 0.2f;

    [SerializeField]
    private float raycastDistance = 5;

    [SerializeField]
    private Transform raycastSource;

    [SerializeField]
    private float stepSpeed = 0.5f;

    [SerializeField]
    private float minStepDistance = 1f;

    [SerializeField]
    private float minBalanceDistance = 0.01f;

    [SerializeField]
    private float minMovementDistance = 0.001f;

    [SerializeField]
    private AnimationCurve stepArcCurve;

    [SerializeField]
    private Leg[] alternatingLegs;

    [SerializeField]
    private float minStepDelay = 0.2f;

    [SerializeField]
    private float minBalanceDelay = 0.5f;

    private Transform foot;
    private Quaternion footRotation;

    private Transform hip;

    private float stepProgress = 1f;

    private float lastStepTime;
    private float lastMovementTime;

    private Vector3 lastStepTargetPosition;

    private int groundLayerMask;

    public bool IsStepping => stepProgress < 1f;

    public Vector3 TargetPosition => target.transform.position;

    public Vector3 Velocity { get; set; }

    private void Start()
    {
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        IKSolver solver = GetComponent<IK>().GetIKSolver();
        solver.OnPostUpdate += OnIkPostUpdate;

        hip = solver.GetPoints().First().transform;
        foot = solver.GetPoints().Last().transform;

        // Assumes that the starting rotation of the foot is what we want to maintain
        footRotation = foot.rotation;

        target.position = GetStepTargetPosition();
    }

    private void Update()
    {
        if (IsStepping)
        {
            return;
        }

        // Ensures that we always have legs planted
        if (alternatingLegs.Any(x => x.IsStepping))
        {
            return;
        }

        // Avoids a stuttering effect where one leg is always moving (e.g. when turning in a circle)
        if (Time.time - lastStepTime < minStepDelay)
        {
            return;
        }

        Vector3 stepTargetPosition = GetStepTargetPosition();

        float distance = Vector3.Distance(target.position, stepTargetPosition);
        if (distance > minStepDistance)
        {
            // TODO: Make the steps mostly symmetrical in their cadence
            //  - Currently the steps can take on all sorts of awkward rhythms

            StartCoroutine(CoStepToPosition(target.position, stepTargetPosition));
        }
        else if (distance > minBalanceDistance)
        {
            // TODO: Avoid re-balancing when actively moving
            //  - This is happening now during the time where the user is moving, but the first step has not be triggered yet.
            //  - Probably want to check for active input, rather than trying to detect motion using step targets

            if (Time.time - lastStepTime < minBalanceDelay)
            {
                return;
            }

            // No leaning when we're still.  Always choose to balance ourselves if we're out of position.
            StartCoroutine(CoStepToPosition(target.position, stepTargetPosition));
        }
    }

    private Vector3 GetStepTargetPosition()
    {
        Debug.DrawRay(raycastSource.position, Vector3.down * raycastDistance, Color.cyan);

        RaycastHit hit;
        if (!Physics.Raycast(raycastSource.position, Vector3.down, out hit, raycastDistance, groundLayerMask))
        {
            throw new Exception("Step target raycast was unsuccessful");
        }

        Debug.DrawRay(hit.point, hit.normal, Color.red);

        return hit.point + hit.normal * footHeight;
    }

    private IEnumerator CoStepToPosition(Vector3 startPosition, Vector3 endPosition)
    {
        stepProgress = 0;

        while (stepProgress < 1f)
        {
            stepProgress += Time.deltaTime * stepSpeed;

            // Keep updating the target position as we move so we don't fall behind
            endPosition = GetStepTargetPosition();

            target.position = Vector3.Lerp(startPosition, endPosition, stepProgress);
            target.position += Vector3.up * stepArcCurve.Evaluate(stepProgress);

            lastStepTime = Time.time;

            yield return null;
        }
    }

    private void OnIkPostUpdate()
    {
        // Update the foot rotation after IK solve
        foot.transform.rotation = footRotation;
    }
}
