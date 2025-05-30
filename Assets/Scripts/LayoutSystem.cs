using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(GameSystem))]
[UpdateAfter(typeof(MoveSystem))]
[UpdateAfter(typeof(DeleteSystem))]
[BurstCompile]
public partial struct LayoutSystem : ISystem
{
    //このシステムが作成されたときに呼び出される。
    public void OnCreate(ref SystemState state)
        => state.RequireForUpdate<Layout>();

    //このシステムが破壊されたときに呼び出される。
    public void OnDestroy(ref SystemState state) {}

    public void OnUpdate(ref SystemState state)
    {
        var layout = SystemAPI.GetSingleton<Layout>();

        int board_size = GameSystem.BoardWidth;
        NativeArray<byte> board = GameSystem.BoardLayout;
        int[] orb_num = new int[GameSystem.MaxColors];
                        
        int board_count = 0;
        for (int j=0; j<board.Length; j++){
            if (board[j] != (byte)GameSystem.MaxColors){
                board_count++;
                switch(board[j]){
                    case 0:
                        orb_num[0]++;
                        break;
                    case 1:
                        orb_num[1]++;
                        break;
                    case 2:
                        orb_num[2]++;
                        break;
                    case 3:
                        orb_num[3]++;
                        break;
                    case 4:
                        orb_num[4]++;
                        break;
                }
            }
        }

        //Debug.Log(orb_num[0]);
        
        

        int width = board_size;
        Entity Orb  = layout.Prefab1;
        for (int color = 0; color < GameSystem.MaxColors; color++){
            switch(color){
                case 0:
                    Orb = layout.Prefab1;
                    break;
                case 1:
                    Orb = layout.Prefab2;
                    break;
                case 2:
                    Orb = layout.Prefab3;
                    break;
                case 3:
                    Orb = layout.Prefab4;
                    break;
                case 4:
                    Orb = layout.Prefab5;
                    break;
            }
            var orbs = state.EntityManager.Instantiate(Orb, orb_num[color], Allocator.Temp);
            int i = 0;
            foreach (var entity in orbs)
            {
                while(board[i] != (byte)color){
                    i++;
                }

                var xform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                xform.ValueRW = LocalTransform.FromScale(0.8f);
                if (width % 2 == 0){
                    xform.ValueRW = LocalTransform.FromPosition(1.2f * (i%width - width/2 + 0.5f), 1.2f * ((width - (i/width) - 1) - width/2 + 0.5f),0);
                }
                else
                {
                    xform.ValueRW = LocalTransform.FromPosition(1.2f * (i%width - width/2), 1.2f * ((width - (i/width) - 1) - width/2),0);
                }
                i++;
            }
        }


        
        //state.Enabled = false;
    }
}