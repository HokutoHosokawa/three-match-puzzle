using Unity.Entities;
using UnityEngine;

public struct InputPos : IComponentData
{
    public float x;
    public float y;
}

public class InputAuthoring : MonoBehaviour
{
    public float _x = 0;
    public float _y = 0;

    class Baker : Baker<InputAuthoring>
    {
        public override void Bake(InputAuthoring src)
        {
            var data = new InputPos()
            {
                x = src._x,
                y = src._y
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}
