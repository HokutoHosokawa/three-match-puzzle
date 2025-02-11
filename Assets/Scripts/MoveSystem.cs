using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[BurstCompile]
public partial struct MoveSystem : ISystem
{
    public void OnCreate(ref SystemState state) {}
    public void OnDestroy(ref SystemState state) {}
    public void OnUpdate(ref SystemState state) {
        var trigger = SystemAPI.GetSingleton<InputTriggerComponent>();
        if (trigger.Trigger != 2){
            return;
        }
        NativeArray<byte> Board = GameSystem.BoardLayout;
        if(trigger.Trigger == 2) {
            // Boardの-1の部分に1回ずつ移動する
            NativeArray<int> pathList = new NativeArray<int>(GameSystem.BoardWidth * GameSystem.BoardHeight, Allocator.Temp);
            for (int i = 0; i < GameSystem.BoardWidth * GameSystem.BoardHeight; i++) {
                if(Board[i] == (byte)(GameSystem.MaxColors + 1)) {
                    //空白は-1
                    pathList[i] = -1;
                } else if(Board[i] == (byte)GameSystem.MaxColors) {
                    //壁は-2
                    pathList[i] = -2;
                } else {
                    pathList[i] = -3;
                }
            }

            //パスの一番上の座標を保存しておくやつ
            NativeList<int> PathHead = new NativeList<int>(Allocator.Temp);
            NativeList<int> PathTail =  new NativeList<int>(Allocator.Temp);
            int k = 0;
            for(int y = GameSystem.BoardHeight - 1; y >= 0; --y) {
                for(int x = 0; x < GameSystem.BoardWidth; ++x){
                    int i = x + y * GameSystem.BoardWidth;
                    if (pathList[i] == -1 && (y > 0 && pathList[i - GameSystem.BoardWidth] != -2)){
                        //上が穴の場合
                        NativeList<int> path = new NativeList<int>(Allocator.Temp);
                        path.Add(i);
                        path = makePiecePath(x, y, path, Board);
                        int index = k;
                        for(int j = 0; j < PathHead.Length; ++j){
                            if(pathList[path[path.Length - 1]] == pathList[PathHead[j]]){
                                index = j;
                                break;
                            }
                        }
                        for(int j = 0; j < path.Length; j++){
                            pathList[path[j]] = index;
                        }
                        PathTail.Add(path[0]);
                        if(index == k){
                            PathHead.Add(path[path.Length - 1]);
                            ++k;
                        }
                        path.Dispose();
                    } else if (pathList[i] == -1 && (y == 0)){
                        //いちばん上の段が空白の場合
                        //パスを作る
                        pathList[i] = k;
                        PathHead.Add(i);
                        PathTail.Add(i);
                        ++k;
                    } else if (pathList[i] == -1 && (x > 0 && pathList[i-1] >= 0)){
                        //左にパスがある場合
                        pathList[i] = pathList[i-1];
                    } else if (pathList[i] == -1 && (x < GameSystem.BoardWidth - 1 && pathList[i+1] >= 0)){
                        //右にパスがある場合
                        pathList[i] = pathList[i+1];
                    } else if (pathList[i] == -1){
                        //まだパスができていない場合
                        NativeList<int> path = new NativeList<int>(Allocator.Temp);
                        path.Add(i);
                        path = makePiecePath(x, y, path, Board);
                        int index = k;
                        for(int j = 0; j < PathHead.Length; ++j){
                            if(pathList[path[path.Length - 1]] == pathList[PathHead[j]]){
                                index = j;
                                break;
                            }
                        }
                        for(int j = 0; j < path.Length; j++){
                            pathList[path[j]] = index;
                        }
                        PathTail.Add(path[0]);
                        if(index == k){
                            PathHead.Add(path[path.Length - 1]);
                            ++k;
                        }
                        path.Dispose();
                    }
                }
            }
            var text = "[";
            for(int i = 0; i < PathTail.Length; ++i){
                text += PathTail[i] +", ";
            }
            text += "]";
            Debug.Log("pathTail:\n" + text);

            for (int i = 0; i < PathHead.Length; ++i){
                //それぞれのパスに対する処理を行う
                //最初に盤面を全探索し、それぞれのピースごとに進行方向を決定
                NativeArray<int> PieceDirection = new NativeArray<int>(GameSystem.BoardWidth * GameSystem.BoardHeight, Allocator.Temp);
                for (int j = 0; j < GameSystem.BoardWidth * GameSystem.BoardHeight; ++j){
                    PieceDirection[j] = -1;
                }
                for (int j = 0; j < PathTail.Length; ++j){
                    if(pathList[PathTail[j]] == pathList[PathHead[i]]){
                        //PathTail[j]がiの末尾ならば
                        int index = PathTail[j];
                        while(true){
                            //方向を決定する
                            int x = index % GameSystem.BoardWidth;
                            int y = index / GameSystem.BoardWidth;
                            if(y == 0){
                                break;
                            }
                            if(y > 0 && pathList[index - GameSystem.BoardWidth] == pathList[PathHead[i]]){
                                //上にパスがあるならば
                                PieceDirection[index - GameSystem.BoardWidth] = 0;
                                index -= GameSystem.BoardWidth;
                            }else if (x < GameSystem.BoardWidth - 1 && pathList[index + 1] == pathList[PathHead[i]]){
                                //上にパスがなく、右にパスがある場合
                                PieceDirection[index + 1] = 1;
                                ++index;
                            }else if(x > 0 && pathList[index - 1] == pathList[PathHead[i]]){
                                //上と右にパスがなく、左にある場合
                                PieceDirection[index - 1] = 2;
                                --index;
                            }
                        }
                    }
                }
                
                //最後にPathHeadからのパスを確認する
                int index2 = PathHead[i];
                while(true){
                    //最下層に着くまで繰り返す
                    int x = index2 % GameSystem.BoardWidth;
                    int y = index2 / GameSystem.BoardHeight;
                    if(PieceDirection[index2] == -1){
                        break;
                    }
                    if(y < GameSystem.BoardHeight - 1 && pathList[index2 + GameSystem.BoardWidth] == pathList[PathHead[i]]){
                        PieceDirection[index2] = 0;
                        index2 += GameSystem.BoardWidth;
                    }else if(x > 0 && pathList[index2 - 1] == pathList[PathHead[i]]){
                        PieceDirection[index2] = 1;
                        --index2;
                    }else if(x < GameSystem.BoardWidth - 1 && pathList[index2 + 1] == pathList[PathHead[i]]){
                        PieceDirection[index2] = 2;
                        ++index2;
                    }
                }

                //PathTailを元にパス上のピースを移動させる
                for(int j = 0; j < PathTail.Length; ++j){
                    int index = PathTail[j];
                    int x = index % GameSystem.BoardWidth;
                    int y = index / GameSystem.BoardWidth;
                    while(true){
                        if(y == 0){
                            //一番上まで来た場合
                            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
                            Board[index] = (byte)random.NextInt(GameSystem.MaxColors);
                            break;
                        }
                        if(y > 0 && PieceDirection[index - GameSystem.BoardWidth] == 0){
                            //１つ上の進行方向が下(自分の方)なら
                           Board[index] = Board[index - GameSystem.BoardWidth];
                           index -= GameSystem.BoardWidth;
                           --y;
                        }else if(x > 0 && PieceDirection[index - 1] == 2){
                            //左のピースが右に進むならば
                            Board[index] = Board[index - 1];
                            --index;
                            --x;
                        }else if(x < GameSystem.BoardWidth - 1 && PieceDirection[index + 1] == 1){
                            Board[index] = Board[index + 1];
                            ++index;
                            ++x;
                        }else {
                            //どこからもこのマスに移動することがない場合
                            break;
                        }
                    }
                }
                PieceDirection.Dispose();
            }
            //ここに原因がある
            // bool isKFinished = false;
            // for(int l = 0; l < k; l++){
            //     for(int y = GameSystem.BoardHeight - 1; y > 0 && !isKFinished; --y) {
            //         for(int x = 0; x < GameSystem.BoardWidth && !isKFinished; ++x) {
            //             int i = x + y * GameSystem.BoardWidth;
            //             if(pathList[i] == l) {
            //                 //パスができている場合
            //                 //パスの先にあるピースを移動する
            //                 //左したがいちばん最初のパスの先なので、ここをベースに考える
            //                 int n = x;
            //                 int m = y;
            //                 while(m != 0) {
            //                     if (m > 0 && pathList[n + (m - 1) * GameSystem.BoardWidth] == l) {
            //                         //上にパスがある場合
            //                         Board[n + m * GameSystem.BoardWidth] = Board[n + (m - 1) * GameSystem.BoardWidth];
            //                         --m;
            //                     } else if (n < GameSystem.BoardWidth - 1 && pathList[n + 1 + m * GameSystem.BoardWidth] == l) {
            //                         //右にパスがある場合
            //                         Board[n + m * GameSystem.BoardWidth] = Board[n + 1 + m * GameSystem.BoardWidth];
            //                         ++n;
            //                     } else if (n > 0 && pathList[n - 1 + m * GameSystem.BoardWidth] == l) {
            //                         //右にパスがある場合
            //                         Board[n + m * GameSystem.BoardWidth] = Board[n - 1 + m * GameSystem.BoardWidth];
            //                         --n;
            //                     }
            //                 }
            //                 var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
            //                 Board[n + m * GameSystem.BoardWidth] = (byte)random.NextInt(GameSystem.MaxColors);
            //                 isKFinished = true;
            //             }
            //         }
            //     }
            // }

            pathList.Dispose();
            PathHead.Dispose();
            PathTail.Dispose();
            showBoard(Board, GameSystem.BoardWidth, GameSystem.BoardHeight);
            GameSystem.BoardLayout = Board;

            //描画処理
            //Boardの空白を確認
            int count = 0;
            for(int y = 0; y < GameSystem.BoardHeight; ++y){
                for(int x = 0; x < GameSystem.BoardWidth; ++x){
                    if(Board[x + y * GameSystem.BoardWidth] == (byte)(GameSystem.MaxColors + 1)){
                        count++;
                    }
                }
            }
            if(count == 0){
                //空白がない場合
                //移動終了
                //GameSystem.BoardLayout = Board;
                //ボードが詰みでないか確認
                bool isStun = false;
                while(isStuned(Board)){
                    //詰みの場合
                    //ランダムにピースを入れ替える
                    for(int y = 0; y < GameSystem.BoardHeight; ++y){
                        for(int x = 0; x < GameSystem.BoardWidth; ++x){
                            if(x < GameSystem.BoardWidth - 1 && Board[x + y * GameSystem.BoardWidth] == Board[x + 1 + y * GameSystem.BoardWidth]){
                                byte color = Board[x + y * GameSystem.BoardWidth];
                                var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
                                var randomIndex = random.NextInt(GameSystem.BoardWidth * GameSystem.BoardHeight);
                                while(Board[randomIndex] == GameSystem.MaxColors){
                                    randomIndex = random.NextInt(GameSystem.BoardWidth * GameSystem.BoardHeight);
                                }
                                Board[x + y * GameSystem.BoardWidth] = Board[randomIndex];
                                Board[randomIndex] = color;
                            }
                        }
                    }
                    isStun = true;
                }
                if(isStun) {
                    GameSystem.BoardLayout = Board;
                    return;
                }
                trigger.Trigger = 1;
                SystemAPI.SetSingleton(trigger);
            }
        }
    }

    private bool isStuned(NativeArray<byte> Board){
        return isHorizontalStuned(Board) && isVerticalStuned(Board);
    }

    private bool isHorizontalStuned(NativeArray<byte> Board){
        
        for(int y = 0; y < GameSystem.BoardHeight; ++y){
            for(int x = 0; x < GameSystem.BoardWidth - 1; ++x){
                if(Board[x + y * GameSystem.BoardWidth] == Board[x + 1 + y * GameSystem.BoardWidth]){
                    byte color = Board[x + y * GameSystem.BoardWidth];
                    if((x < GameSystem.BoardWidth - 2 && Board[(x + 2) + y * GameSystem.BoardWidth] != GameSystem.MaxColors && ((y < GameSystem.BoardHeight - 1 && Board[x + 2 + (y + 1) * GameSystem.BoardWidth] == color) || (y > 0 && Board[x + 2 + (y - 1) * GameSystem.BoardWidth] == color))) || (x < GameSystem.BoardWidth - 3 && Board[x + 3 + y * GameSystem.BoardWidth] == color) || (x > 0 && Board[(x - 1) + y * GameSystem.BoardWidth] != GameSystem.MaxColors && ((y > 0 && Board[x - 1 + (y - 1) * GameSystem.BoardWidth] == color) || (y < GameSystem.BoardHeight - 1 && Board[x - 1 + (y + 1) * GameSystem.BoardWidth] == color))) || (x > 1 && Board[x - 2 + y * GameSystem.BoardWidth] == color)){
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private bool isVerticalStuned(NativeArray<byte> Board){
        for(int x = 0; x < GameSystem.BoardWidth; ++x){
            for(int y = 0; y < GameSystem.BoardHeight - 1; ++y){
                if(Board[x + y * GameSystem.BoardWidth] == Board[x + (y + 1) * GameSystem.BoardWidth]){
                    byte color = Board[x + y * GameSystem.BoardWidth];
                    if((y < GameSystem.BoardHeight - 2 && Board[x + (y + 2) * GameSystem.BoardWidth] != GameSystem.MaxColors && ((x < GameSystem.BoardWidth - 1 && Board[x + 1 + (y + 2) * GameSystem.BoardWidth] == color) || (x > 0 && Board[x - 1 + (y + 2) * GameSystem.BoardWidth] == color))) || (y < GameSystem.BoardHeight - 3 && Board[x + (y + 3) * GameSystem.BoardWidth] == color) || (y > 0 && Board[x + (y - 1) * GameSystem.BoardWidth] != GameSystem.MaxColors && ((x > 0 && Board[x - 1 + (y - 1) * GameSystem.BoardWidth] == color) || (x < GameSystem.BoardWidth - 1 && Board[x + 1 + (y - 1) * GameSystem.BoardWidth] == color))) || (y > 1 && Board[x + (y - 2) * GameSystem.BoardWidth] == color)){
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private NativeList<int> makePiecePath(int x, int y, NativeList<int> path, NativeArray<byte> Board){
        // ここにパスを作る処理を書く
        //x, yは空白(Board[x + (y + 1) * GameSystem.BoardWidth] == (byte)(GameSystem.MaxColors + 1))の場所
        path.Add(x + y * GameSystem.BoardWidth);
        if(y == 0){
            //一番上の場合
            return path;
        }
        if(y > 0 && Board[x + (y - 1) * GameSystem.BoardWidth] != (byte)GameSystem.MaxColors){
            //上が壁でない場合(空白でも可)
            return makePiecePath(x, y - 1, path, Board);
        }

        //上が壁の場合
        //右のピース(or空白)が一番上の段につながっている場合
        //一番上までのパスが今回求めるパスと一致
        //幅優先探索でいちばん上までつながっているかどうかを確認
        NativeList<int> queue = new NativeList<int>(Allocator.Temp);
        NativeArray<int> visited = new NativeArray<int>(GameSystem.BoardHeight * GameSystem.BoardWidth,Allocator.Temp);
        for(int i = 0; i < GameSystem.BoardHeight * GameSystem.BoardWidth; i++){
            visited[i] = -1;
        }
        if(x < GameSystem.BoardWidth - 1 && Board[x + 1 + y * GameSystem.BoardWidth] != (byte)GameSystem.MaxColors){
            visited[x + y * GameSystem.BoardWidth] = 0;
            visited[(x + 1) + y * GameSystem.BoardWidth] = 1;
            queue.Add(x + 1 + y * GameSystem.BoardWidth);
            int head = 0;
            while(head < queue.Length){
                int current = queue[head];
                ++head;
                int currentX = current % GameSystem.BoardWidth;
                int currentY = current / GameSystem.BoardWidth;
                if(currentY == 0){
                    //一番上の段につながっている場合
                    //パスを作る
                    NativeList<int> path2 = new NativeList<int>(Allocator.Temp);
                    path2.Add(current);
                    while(visited[current] != 0){
                        if(visited[current - 1] == visited[current] - 1){
                            path2.Add(current - 1);
                            current--;
                        } else if (visited[current + 1] == visited[current] - 1){
                            path2.Add(current + 1);
                            current++;
                        } else if (visited[current + GameSystem.BoardWidth] == visited[current] - 1){
                            path2.Add(current + GameSystem.BoardWidth);
                            current += GameSystem.BoardWidth;
                        }
                    }
                    for(int i = 0; i < path2.Length; i++){
                        path.Add(path2[path2.Length - i - 1]);
                    }
                    path2.Dispose();
                    queue.Dispose();
                    visited.Dispose();
                    return path;
                }
                if(currentY > 0 && Board[current - GameSystem.BoardWidth] != (byte)GameSystem.MaxColors && visited[current - GameSystem.BoardWidth] == -1){
                    visited[current - GameSystem.BoardWidth] = visited[current] + 1;
                    queue.Add(current - GameSystem.BoardWidth);
                }
                if(currentX < GameSystem.BoardWidth - 1 && Board[current + 1] != (byte)GameSystem.MaxColors && visited[current + 1] == -1){
                    visited[current + 1] = visited[current] + 1;
                    queue.Add(current + 1);
                }
                if(currentX > 0 && Board[current - 1] != (byte)GameSystem.MaxColors && visited[current - 1] == -1){
                    visited[current - 1] = visited[current] + 1;
                    queue.Add(current - 1);
                }
            }
        }
        for(int i = 0; i < GameSystem.BoardHeight * GameSystem.BoardWidth; i++){
            visited[i] = -1;
        }
        if (x > 0 && Board[x - 1 + y * GameSystem.BoardWidth] != (byte)GameSystem.MaxColors){
            visited[x + y * GameSystem.BoardWidth] = 0;
            visited[(x - 1) + y * GameSystem.BoardWidth] = 1;
            queue.Add(x - 1 + y * GameSystem.BoardWidth);
            int head = 0;
            while(head < queue.Length){
                int current = queue[head];
                ++head;
                int currentX = current % GameSystem.BoardWidth;
                int currentY = current / GameSystem.BoardWidth;
                if(currentY == 0){
                    //一番上の段につながっている場合
                    //パスを作る
                    NativeList<int> path2 = new NativeList<int>(Allocator.Temp);
                    path2.Add(current);
                    while(visited[current] != 0){
                        if(visited[current - 1] == visited[current] - 1){
                            path2.Add(current - 1);
                            current--;
                        } else if (visited[current + 1] == visited[current] - 1){
                            path2.Add(current + 1);
                            current++;
                        } else if (visited[current + GameSystem.BoardWidth] == visited[current] - 1){
                            path2.Add(current + GameSystem.BoardWidth);
                            current += GameSystem.BoardWidth;
                        }
                    }
                    for(int i = 0; i < path2.Length; i++){
                        path.Add(path2[path2.Length - i - 1]);
                    }
                    path2.Dispose();
                    queue.Dispose();
                    visited.Dispose();
                    return path;
                }
                if(currentY > 0 && Board[current - GameSystem.BoardWidth] != (byte)GameSystem.MaxColors && visited[current - GameSystem.BoardWidth] == -1){
                    visited[current - GameSystem.BoardWidth] = visited[current] + 1;
                    queue.Add(current - GameSystem.BoardWidth);
                }
                if(currentX < GameSystem.BoardWidth - 1 && Board[current + 1] != (byte)GameSystem.MaxColors && visited[current + 1] == -1){
                    visited[current + 1] = visited[current] + 1;
                    queue.Add(current + 1);
                }
                if(currentX > 0 && Board[current - 1] != (byte)GameSystem.MaxColors && visited[current - 1] == -1){
                    visited[current - 1] = visited[current] + 1;
                    queue.Add(current - 1);
                }
            }
        }
        //ここは、左右どちらのピースもいちばん上につながっていない場合
        //しょうがないからpathを返す
        queue.Dispose();
        visited.Dispose();
        return path;
    }

    private void showBoard(NativeArray<byte> BoardLayout, int BoardWidth, int BoardHeight){
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
