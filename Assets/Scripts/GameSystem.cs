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
        if (trigger.Trigger == 3) {
            var input = SystemAPI.GetSingleton<InputPos>();
            int start_index_x;
            int start_index_y;
            int end_index_x;
            int end_index_y;
            Debug.Log(input.startX + " " + input.startY + " " + input.endX + " " + input.endY);
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

            int moveVecX = end_index_x - start_index_x;
            int moveVecY = end_index_y - start_index_y;

            if (Mathf.Abs(moveVecX) == Mathf.Abs(moveVecY)) {
                //斜め移動だった場合は何もしない
                // end_index_x = start_index_x;
                // end_index_y = start_index_y;
                trigger.Trigger = 0;
                SystemAPI.SetSingleton(trigger);
                return;
            } else if (Mathf.Abs(moveVecX) > Mathf.Abs(moveVecY)) {
                //X方向の移動が大きい場合→横方向に移動させる
                if (moveVecX > 0 && start_index_x < BoardWidth - 1 && start_index_x >= 0) {
                    //右方向に移動
                    end_index_x = start_index_x + 1;
                    end_index_y = start_index_y;
                } else if (moveVecX < 0 && start_index_x > 0 && start_index_x < BoardWidth) {
                    //左方向に移動
                    end_index_x = start_index_x - 1;
                    end_index_y = start_index_y;
                }
            } else {
                //Y方向の移動が大きい場合→縦方向に移動させる
                if (moveVecY > 0 && start_index_y < BoardHeight - 1 && start_index_y >= 0) {
                    //下方向に移動
                    end_index_x = start_index_x;
                    end_index_y = start_index_y + 1;
                } else if (moveVecY < 0 && start_index_y > 0 && start_index_y < BoardHeight) {
                    //上方向に移動
                    end_index_x = start_index_x;
                    end_index_y = start_index_y - 1;
                }
            }

            //Debug.Log("start: " + start_index_x + " " + start_index_y + " end: " + end_index_x + " " + end_index_y);

            int start_index = start_index_x + start_index_y * BoardWidth;
            int end_index = end_index_x + end_index_y * BoardWidth;

            if (BoardLayout[start_index] == MaxColors || BoardLayout[end_index] == MaxColors || BoardLayout[start_index] == BoardLayout[end_index]) {
                trigger.Trigger = 0;
                SystemAPI.SetSingleton(trigger);
                return;
            }

            byte temp_color = BoardLayout[start_index];
            BoardLayout[start_index] = BoardLayout[end_index];
            BoardLayout[end_index] = temp_color;

            NativeArray<int> matchedIndices = new NativeArray<int>(BoardWidth * BoardHeight, Allocator.Temp);
            for (int i = 0; i < matchedIndices.Length; i++) {
                matchedIndices[i] = 0;
            }

            matchedIndices = CheckHorizontalMatches(matchedIndices);
            matchedIndices = CheckVerticalMatches(matchedIndices);
            int matchCount = 0;
            for (int i = 0; i < matchedIndices.Length; i++) {
                if (matchedIndices[i] == -1) {
                    BoardLayout[i] = (byte)(MaxColors + 1);
                    matchCount++;
                }
            }

            matchedIndices.Dispose();
            if(matchCount == 0) {
                //マッチしなかった場合、元に戻す
                temp_color = BoardLayout[start_index];
                BoardLayout[start_index] = BoardLayout[end_index];
                BoardLayout[end_index] = temp_color;
                trigger.Trigger = 0;
                SystemAPI.SetSingleton(trigger);
                return;
            }

            showBoard();

            //マッチした場合、連結成分の確認
            //今は、連結成分を省略して、消えた個数×100をスコアに加算

            var scores = SystemAPI.GetSingleton<GameData>();
            scores.totalScore += matchCount * 100;
            SystemAPI.SetSingleton(scores);

            //消す

            trigger.Trigger = 2;
            SystemAPI.SetSingleton(trigger);
            //Debug.Log("Trigger: 2");
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

        for (int y = 0; y < BoardHeight; y++){
            for (int x = 0; x < BoardWidth; x++){
                int index = x + y * BoardWidth;

                if (BoardLayout[index] == (byte)MaxColors){
                    continue;
                }
                byte color;

                //ランダムな色を選択、かつマッチを発生しないような色に調整
                do{
                    color = (byte)random.NextInt(MaxColors);
                }while (IsPartOfMatch(state, x, y, color));

                // var piece = new Piece {
                //     color = color,
                //     is_moved = 0,
                //     y_from = (byte)y,
                //     y_to = (byte)y
                // };

                BoardLayout[index] = color;

                //board[x, y] = new Piece {color = color, is_moved = 0, y_from = (byte)y, y_to = (byte)y};
                BoardLayout[index] = color;


                /*
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, piece);
                state.EntityManager.AddComponentData(entity, new PiecePos { position = new int2(x, y)});
                */
            }
        }
        showBoard();
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

    private NativeArray<int> CheckHorizontalMatches(NativeArray<int> matchedIndices){
        for (int y = 0; y < BoardHeight; ++y){
            int matchStart = -1;
            int currentColor = -1;
            for (int x = 0; x < BoardWidth; ++x){
                int index = x + y * BoardWidth;
                int color = BoardLayout[index];
                if (color == currentColor && color != MaxColors){
                    if (matchStart == -1){
                        matchStart = x - 1;
                    }
                } else {
                    if(matchStart != -1 && x - matchStart >= 3){
                        for (int i = matchStart; i < x; ++i){
                            matchedIndices[i + y * BoardWidth] = -1;
                        }
                    }
                    matchStart = -1;
                }
                currentColor = color;
            }

            if (matchStart != -1 && BoardWidth - matchStart >= 3){
                for (int i = matchStart; i < BoardWidth; ++i){
                    matchedIndices[i + y * BoardWidth] = -1;
                }
            }
        }
        return matchedIndices;
    }

    private NativeArray<int> CheckVerticalMatches(NativeArray<int> matchedIndices) {
        for (int x = 0; x < BoardWidth; x++) {
            int matchStart = -1;
            int currentColor = -1;
            for (int y = 0; y < BoardHeight; y++) {
                int index = x + y * BoardWidth;
                int color = BoardLayout[index];
                if (color == currentColor && color != MaxColors) {
                    if (matchStart == -1){
                        matchStart = y - 1;
                    }
                } else {
                    if (matchStart != -1 && y - matchStart >= 3) {
                        for (int i = matchStart; i < y; ++i) {
                            matchedIndices[x + i * BoardWidth] = -1;
                        }
                    }
                    matchStart = -1;
                }
                currentColor = color;
            }

            if (matchStart != -1 && BoardHeight - matchStart >= 3) {
                for (int i = matchStart; i < BoardHeight; ++i) {
                    matchedIndices[x + i * BoardWidth] = -1;
                }
            }
        }
        return matchedIndices;
    }

    


    private void showBoard(){
        var text = "[";
        for (int y = 0; y < BoardHeight; y++){
            for (int x = 0; x < BoardWidth; x++){
                int index = x + y * BoardWidth;
                text += BoardLayout[index] + ", ";
            }
            text += "\n";
        }
        text += "]";
        Debug.Log(text);
    }
}

