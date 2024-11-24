using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BurstCompile]
public partial struct DeleteSystem : ISystem
{
    public void OnCreate(ref SystemState state){}
    public void OnDestroy(ref SystemState state){}
    public void OnUpdate(ref SystemState state){
        var trigger = SystemAPI.GetSingleton<InputTriggerComponent>();
        if (trigger.Trigger != 1){
            return;
        }
        //削除する場所があるかどうかの確認
        NativeArray<byte> board = GameSystem.BoardLayout;
        NativeArray<int> matchedIndices = new NativeArray<int>(GameSystem.BoardWidth * GameSystem.BoardHeight, Allocator.Temp);
        for (int i = 0; i < matchedIndices.Length; ++i){
            matchedIndices[i] = 0;
        }
        int width = GameSystem.BoardWidth;
        int height = GameSystem.BoardHeight;
        int Colors = GameSystem.MaxColors;
        matchedIndices = CheckHorizontalMatches2(board, matchedIndices, width, height, Colors);
        matchedIndices = CheckVerticalMatches2(board, matchedIndices, width, height, Colors);
        int matchCount = 0;
        for (int y = 0; y < height; ++y){
            for (int x = 0; x < width; ++x){
                int index = x + y * width;
                if (matchedIndices[index] == -1){
                    board[index] = (byte)(Colors + 1);
                    matchCount++;
                }
            }
        }
        matchedIndices.Dispose();
        if (matchCount == 0) {
            //削除する場所がない場合
            trigger.Trigger = 0;
            SystemAPI.SetSingleton(trigger);
            return;
        }

        var scores = SystemAPI.GetSingleton<GameData>();
        scores.totalScore += matchCount * 100;
        SystemAPI.SetSingleton(scores);

        trigger.Trigger = 2;
        SystemAPI.SetSingleton(trigger);
        GameSystem.BoardLayout = board;
        return;
    }

    private NativeArray<int> CheckHorizontalMatches2(NativeArray<byte> BoardLayout, NativeArray<int> matchedIndices, int BoardWidth, int BoardHeight, int MaxColors){
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

    private NativeArray<int> CheckVerticalMatches2(NativeArray<byte> BoardLayout, NativeArray<int> matchedIndices, int BoardWidth, int BoardHeight, int MaxColors){
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
}
