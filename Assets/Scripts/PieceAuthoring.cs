using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Collections.Generic;

public struct PiecePos : IComponentData {
    public int2 position;
}

public struct Piece : IComponentData {
    public byte color;
    public byte addon;
    //is_movedは0で動かない、1で下方向に飲み動いた、2で横方向を含めて動いたことを表すことにする。
    public byte is_moved;
    public byte y_from;
    public byte y_to;
}

public class PieceAuthoring : MonoBehaviour
{
    public byte _color = 0;
    public byte _addon = 0;
    public byte _is_moved = 0;
    public byte _y_from = 0;
    public byte _y_to = 0;
    public Vector2Int position;

    class Baker : Baker<PieceAuthoring>{
        public override void Bake(PieceAuthoring src){
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var data = new Piece(){
                color = src._color,
                addon = src._addon,
                is_moved = src._is_moved,
                y_from = src._y_from,
                y_to = src._y_to
            };
            AddComponent(entity, data);

            AddComponent(entity, new PiecePos{position = new int2(src.position.x, src.position.y)});
        }
    }
}
