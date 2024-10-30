using Unity.Entities;
using UnityEngine;

public struct GameData : IComponentData
{
    public int totalScore;
}

public class GameAuthoring : MonoBehaviour
{
    public int _totalScore = 0;

    class Baker : Baker<GameAuthoring>
    {
        public override void Bake(GameAuthoring src)
        {
            var data = new GameData()
            {
                totalScore = src._totalScore
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}
