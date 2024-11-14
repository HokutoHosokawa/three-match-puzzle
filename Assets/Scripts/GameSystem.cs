using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BurstCompile]
public partial struct GameSystem : ISystem
{
    private int BoardWidth;
    private int BoardHeight;
    private int MaxColors;

    public static NativeArray<byte> BoardLayout;

    public void OnCreate(ref SystemState state) {
        //盤面の初期化
        InitializeGameSettings(ref state);
        InitializeBoard(ref state);
        state.RequireForUpdate<GameData>();
    }
    public void OnDestroy(ref SystemState state) {
        if (BoardLayout.IsCreated) {
            BoardLayout.Dispose();
        }
    }
    public void OnUpdate(ref SystemState state) {
        //盤面とスコアの更新

    }

    private void InitializeGameSettings(ref SystemState state) {
        //盤面の初期化
        MaxColors = 5;
        BoardWidth = 8;
        BoardHeight = 8;

        BoardLayout = new NativeArray<byte>(BoardWidth * BoardHeight, Allocator.Persistent);
        int[] tempLayout = new int[] {
            0, 1, 1, 1, 1, 1, 1, 0,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 0, 0, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 0, 0, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            0, 1, 1, 1, 1, 1, 1, 0,
        };

        for (int i = 0; i < BoardLayout.Length; i++) {
            if (tempLayout[i] == 0) {
                BoardLayout[i] = (byte)MaxColors;
            }
            else {
                BoardLayout[i] = (byte)tempLayout[i];
            }
        }
    }

    private void InitializeBoard(ref SystemState state) {
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        //var board = new Piece[BoardWidth, BoardHeight];

        for (int x = 0; x < BoardWidth; x++){
            for (int y = 0; y < BoardHeight; y++){
                int index = x + y * BoardWidth;

                if (BoardLayout[index] == 0){
                    continue;
                }
                byte color;

                //ランダムな色を選択、かつマッチを発生しないような色に調整
                do{
                    color = (byte)random.NextInt(MaxColors);
                }while (IsPartOfMatch(state, x, y, color));
                Debug.Log("color: " + color);

                var piece = new Piece {
                    color = color,
                    is_moved = 0,
                    y_from = (byte)y,
                    y_to = (byte)y
                };

                //board[x, y] = new Piece {color = color, is_moved = 0, y_from = (byte)y, y_to = (byte)y};

                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, piece);
                state.EntityManager.AddComponentData(entity, new PiecePos { position = new int2(x, y)});
            }
        }
    }

    private bool IsPartOfMatch(SystemState state, int x, int y, byte color){
        if (x >= 2){
            int left1 = (x - 1) + y * BoardWidth;
            int left2 = (x - 2) + y * BoardWidth;
            if (BoardLayout[left1] == color && BoardLayout[left2] == color){
                return true;
            }
        }

        if (y >= 2){
            int below1 = x + (y - 1) * BoardWidth;
            int below2 = x + (y - 2) * BoardWidth;
            if (BoardLayout[below1] == color && BoardLayout[below2] == color){
                return true;
            }
        }

        return false;
    }

}

