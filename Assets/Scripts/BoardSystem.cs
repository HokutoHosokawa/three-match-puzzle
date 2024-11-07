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

        int board_size = 8;

        NativeArray<byte> board = GameSystem.BoardLayout;

                        
        int board_count = 0;
        for (int j=0; j<board.Length; j++){
                if (board[j] != (byte)5){
                    board_count++;
                }
        }
        var instances = state.EntityManager.Instantiate(spawner.Prefab, board_count, Allocator.Temp);
        //var instances = state.EntityManager.Instantiate(spawner.Prefab, 56, Allocator.Temp);
        int i = 0;
        int width = board_size;
        foreach (var entity in instances)
        {
            while(board[i] == (byte)5){
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