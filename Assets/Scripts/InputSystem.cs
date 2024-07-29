using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BurstCompile]
public partial struct InputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
        => state.RequireForUpdate<InputPos>();
    
    public void OnDestroy(ref SystemState state) {}

    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetMouseButtonDown(0))
        {
            var input = SystemAPI.GetSingleton<InputPos>();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            input.x = worldPosition.x;
            input.y = worldPosition.y;
            Debug.Log(input.x + ", " + input.y);
            SystemAPI.SetSingleton(input);
        }
    }
}