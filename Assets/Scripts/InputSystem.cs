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
        bool isInputDetected = false;

        if (Input.GetMouseButtonDown(0))
        {
            var input = SystemAPI.GetSingleton<InputPos>();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            input.startX = worldPosition.x;
            input.startY = worldPosition.y;
            SystemAPI.SetSingleton(input);
        }
        if (Input.GetMouseButtonUp(0))
        {
            var input = SystemAPI.GetSingleton<InputPos>();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            input.endX = worldPosition.x;
            input.endY = worldPosition.y;
            SystemAPI.SetSingleton(input);
            var trigger = SystemAPI.GetSingleton<InputTriggerComponent>();
            trigger.Trigger = true;
            SystemAPI.SetSingleton(trigger);
        }

    }
}