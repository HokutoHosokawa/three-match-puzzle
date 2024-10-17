using UnityEngine;
using Unity.Entities;

public struct Board : IComponentData
{
    public Entity Prefab;
    public float SpawnPositionX;
    public float SpawnPositionY;
}

public class BoardAuthoring : MonoBehaviour
{
    public GameObject _prefab;

    class BoardBaker : Baker<BoardAuthoring>
    {
        public override void Bake(BoardAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Board{
                Prefab = GetEntity(authoring._prefab, TransformUsageFlags.Dynamic),
                SpawnPositionX = authoring.transform.position.x,
                SpawnPositionY = authoring.transform.position.y
            });
        }
    }

}