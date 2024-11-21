using Unity.Entities;
using UnityEngine;

public struct InputPos : IComponentData
{
    public float startX;
    public float startY;
    public float endX;
    public float endY;
}

public class InputAuthoring : MonoBehaviour
{
    public float _startX = 0;
    public float _startY = 0;
    public float _endX = 0;
    public float _endY = 0;

    class Baker : Baker<InputAuthoring>
    {
        public override void Bake(InputAuthoring src)
        {
            var data = new InputPos()
            {
                startX = src._startX,
                startY = src._startY,
                endX = src._endX,
                endY = src._endY
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}
