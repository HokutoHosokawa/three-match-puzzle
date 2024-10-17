using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BurstCompile]
public partial struct PieceSystem : ISystem{
    public void OnCreate(ref SystemState state){}
    public void OnDestroy(ref SystemState state){}
    public void OnUpdate(ref SystemState state){}
}
