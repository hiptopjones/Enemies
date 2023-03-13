using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LegsController : MonoBehaviour
{
    private Leg[] legs;

    public float AverageTerrainHeight { get; private set; }

    private void Start()
    {
        legs = GetComponentsInChildren<Leg>();
    }

    private void Update()
    {
        AverageTerrainHeight = 0;

        float legWeight = 1f / legs.Length;
        foreach (Leg leg in legs)
        {
            AverageTerrainHeight += leg.TargetPosition.y * legWeight;
        }
    }
}
