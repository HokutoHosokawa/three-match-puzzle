using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct BoardSystem : ISystem
{
    //このシステムが作成されたときに呼び出される。
    public void OnCreate(ref SystemState state)
        => state.RequireForUpdate<Board>();

    //このシステムが破壊されたときに呼び出される。
    public void OnDestroy(ref SystemState state) {}

    public void OnUpdate(ref SystemState state)
    {
        var spawner = SystemAPI.GetSingleton<Board>();

        int board_size = 5;

        int[,] board = 
        {{1,1,1,1,1},
        {1,1,1,0,0},
        {1,1,1,1,1},
        {0,0,1,1,1},
        {1,1,1,1,1}};
                        
        int board_count = 0;
        for (int j=0; j<board_size; j++){
            for (int k=0; k<board_size; k++){
                if (board[j,k]==1){
                    board_count++;
                }
            }
        }
        var instances = state.EntityManager.Instantiate(spawner.Prefab, board_count, Allocator.Temp);
        int i = 0;
        int width = board_size;
        foreach (var entity in instances)
        {
            while(board[(i % board_size), (i / board_size)] == 0){
                i++;
            }
            var xform = SystemAPI.GetComponentRW<LocalTransform>(entity);
            xform.ValueRW = LocalTransform.FromScale(0.8f);
            xform.ValueRW = LocalTransform.FromPosition(1.2f * (i/width - width/2), 1.2f * ((i%width) - width/2),0);
            i++;
        }
        Debug.Log(i);

        state.Enabled = false;
    }
}