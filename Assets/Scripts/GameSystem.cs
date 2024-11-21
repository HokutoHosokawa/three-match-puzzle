using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UpdateAfter(typeof(InputSystem))]
[BurstCompile]
public partial struct GameSystem : ISystem
{
    public static int BoardWidth;
    public static int BoardHeight;
    public static int MaxColors;

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
        var trigger = SystemAPI.GetSingleton<InputTriggerComponent>();
        if (trigger.Trigger) {
            var input = SystemAPI.GetSingleton<InputPos>();
            Debug.Log("This text is from GameSystem");
            int start_index_x;
            int start_index_y;
            int end_index_x;
            int end_index_y;
            if(BoardWidth % 2 == 0) {
                if (input.startX < 0) {
                    start_index_x = (int)(input.startX/1.2f) + (BoardWidth/2) - 1;
                } else {
                    start_index_x = (int)(input.startX/1.2f) + (BoardWidth/2);
                }
                if (input.startY < 0) {
                    start_index_y = (int)(input.startY/1.2f) + (BoardHeight/2) - 1;
                } else {
                    start_index_y = (int)(input.startY/1.2f) + (BoardHeight/2);
                }
                if (input.endX < 0) {
                    end_index_x = (int)(input.endX/1.2f) + (BoardWidth/2) - 1;
                } else {
                    end_index_x = (int)(input.endX/1.2f) + (BoardWidth/2);
                }
                if (input.endY < 0) {
                    end_index_y = (int)(input.endY/1.2f) + (BoardHeight/2) - 1;
                } else {
                    end_index_y = (int)(input.endY/1.2f) + (BoardHeight/2);
                }
            } else {
                if (input.startX + 0.6 < 0) {
                    start_index_x = (int)((input.startX+0.6)/1.2f) + (BoardWidth/2) - 1;
                } else {
                    start_index_x = (int)((input.startX+0.6)/1.2f) + (BoardWidth/2);
                }
                if (input.startY + 0.6 < 0) {
                    start_index_y = (int)((input.startY+0.6)/1.2f) + (BoardHeight/2) - 1;
                } else {
                    start_index_y = (int)((input.startY+0.6)/1.2f) + (BoardHeight/2);
                }
                if (input.endX + 0.6 < 0) {
                    end_index_x = (int)((input.endX+0.6)/1.2f) + (BoardWidth/2) - 1;
                } else {
                    end_index_x = (int)((input.endX+0.6)/1.2f) + (BoardWidth/2);
                }
                if (input.endY + 0.6 < 0) {
                    end_index_y = (int)((input.endY+0.6)/1.2f) + (BoardHeight/2) - 1;
                } else {
                    end_index_y = (int)((input.endY+0.6)/1.2f) + (BoardHeight/2);
                }
            }
            start_index_y = BoardHeight - start_index_y - 1;
            end_index_y = BoardHeight - end_index_y - 1;
            //適当なスコアの加算
            var scores = SystemAPI.GetSingleton<GameData>();
            scores.totalScore += 1;
            SystemAPI.SetSingleton(scores);

            Debug.Log("(" + input.startX + ", " + input.startY + "), (" + input.endX + "," + input.endY + ") → (" + start_index_x + ", " + start_index_y + "), (" + end_index_x + ", " + end_index_y + ")");

            // if(start_index == end_index) {
            //     return;
            // }

            //盤面の更新
            //スコアの更新
            //盤面の描画
            //スコアの描画
            trigger.Trigger = false;
            SystemAPI.SetSingleton(trigger);
        }
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

        // int[] tempLayout = new int[] {
        //     0, 1, 1, 1, 0, 1, 1,
        //     1, 1, 1, 1, 1, 1, 1,
        //     1, 1, 1, 0, 1, 1, 1,
        //     1, 1, 1, 0, 1, 1, 1,
        //     1, 1, 1, 1, 1, 1, 1,
        //     1, 1, 1, 1, 1, 1, 1,
        //     1, 1, 1, 1, 1, 1, 1,
        // };

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

    private int GetPieceCoordinate(float x, float y){
        int new_x;
        int new_y;
        if(BoardWidth % 2 == 0) {
            new_x = (int)(x/1.2f + BoardWidth/2);
            new_y = (int)(y/1.2f + BoardHeight/2);
        }else{
            new_x = (int)((x+0.6)/1.2f + BoardWidth/2);
            new_y = (int)((y+0.6)/1.2f + BoardHeight/2);
        }
        return new_x + new_y * BoardWidth;
    }

    private (int, int, int, int) Change2Coordinate(int startX, int startY, int endX, int endY){
        if ((startX == endX-1 && startY == endY) || (startX == endX && startY == endY-1) || (startX == endX+1 && startY == endY) || (startX == endX && startY == endY+1)){
            return (startX, startY, endX, endY);
        }
        
        return (-1,-1,-1,-1);
        
    }
}

