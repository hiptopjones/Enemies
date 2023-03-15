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
    private float minStepDistance = 0.01f;

    [SerializeField]
    private AnimationCurve stepArcCurve;

    [SerializeField]
    private Leg[] alternatingLegs;

    [SerializeField]
    private float minStepDelay = 0.1f;

    private Transform foot;
    private Quaternion footRotation;

    private float stepProgress = 1f;
    private float lastStepTime;

    private int groundLayerMask;

    public bool IsStepping => stepProgress < 1f;

    public Vector3 TargetPosition => target.transform.position;
    public Vector3 FootNormal { get; private set; }

    public Vector3 Velocity { get; set; }

    private void Start()
    {
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        IKSolver solver = GetComponent<IK>().GetIKSolver();
        solver.OnPostUpdate += OnIkPostUpdate;

        foot = solver.GetPoints().Last().transform;

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

        // Helps prevent a stuttering effect where one leg is always moving and the others are starved out
        // In other words, this delay helps to offset the fixed order of Update() calls and gives all legs a chance to move
        // Acheiving satisfying results may require tuning the delay time, along with step time, and body speed
        if (Time.time - lastStepTime < minStepDelay)
        {
            return;
        }

        float distance = Vector3.Distance(target.position, GetStepTargetPosition());
        if (distance > minStepDistance)
        {
            StartCoroutine(CoExecuteStep(target.position));
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

    private IEnumerator CoExecuteStep(Vector3 startPosition)
    {
        stepProgress = 0;

        while (stepProgress < 1f)
        {
            stepProgress += Time.deltaTime * stepSpeed;

            // Keep updating the target position as we move so we don't fall behind
            Vector3 endPosition = GetStepTargetPosition();

            target.position = Vector3.Lerp(startPosition, endPosition, stepProgress);
            target.position += Vector3.up * stepArcCurve.Evaluate(stepProgress);

            lastStepTime = Time.time;

            yield return null;
        }
    }

    private void OnIkPostUpdate()
    {
        UpdateFootOrientation();
    }

    private void UpdateFootOrientation()
    {
        RaycastHit hit;
        if (!Physics.Raycast(foot.position, Vector3.down, out hit, raycastDistance, groundLayerMask))
        {
            throw new Exception("Foot raycast was unsuccessful");
        }

        FootNormal = hit.normal;

        // Keeps the foot level with the ground, but still oriented correctly with the leg
        Vector3 forward = Vector3.Cross(transform.up, hit.normal);
        foot.transform.rotation = Quaternion.LookRotation(forward, hit.normal) * Quaternion.Euler(-90, 0, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(foot.transform.position, Vector3.one * 0.02f);
    }
}
