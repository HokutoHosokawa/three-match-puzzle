using Unity.Entities;
using UnityEngine;

public struct InputTriggerComponent : IComponentData
{
    public bool Trigger;
}

public class InputTriggerAuthoring : MonoBehaviour
{
    public bool _trigger = false;

    class Baker : Baker<InputTriggerAuthoring>
    {
        public override void Bake(InputTriggerAuthoring src)
        {
            var data = new InputTriggerComponent()
            {
                Trigger = src._trigger
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}
