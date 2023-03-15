using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LegsController : MonoBehaviour
{
    private Leg[] legs;

    public float AverageTerrainHeight { get; private set; }
    public Vector3 AverageFootNormal { get; private set; }

    private void Start()
    {
        legs = GetComponentsInChildren<Leg>();
    }

    private void Update()
    {
        AverageTerrainHeight = 0;
        AverageFootNormal = Vector3.zero;

        foreach (Leg leg in legs)
        {
            AverageTerrainHeight += leg.TargetPosition.y;
            AverageFootNormal += leg.FootNormal;
        }

        AverageTerrainHeight /= legs.Length;
        AverageFootNormal = AverageFootNormal.normalized;
    }
}
