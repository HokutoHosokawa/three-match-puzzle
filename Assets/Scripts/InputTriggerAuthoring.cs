using Unity.Entities;
using UnityEngine;

public struct InputTriggerComponent : IComponentData
{
    public byte Trigger; //0: トリガーなし, 1: 削除, 2: 移動, 3: 最初のInput
}

public class InputTriggerAuthoring : MonoBehaviour
{
    public byte _trigger = 0;

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
