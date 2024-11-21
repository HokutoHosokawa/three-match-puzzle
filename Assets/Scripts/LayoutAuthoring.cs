using UnityEngine;
using Unity.Entities;

public struct Layout : IComponentData
{
    public Entity Prefab1;
    public Entity Prefab2;
    public Entity Prefab3;
    public Entity Prefab4;
    public Entity Prefab5;
    public float SpawnPositionX;
    public float SpawnPositionY;
}

public class LayoutAuthoring : MonoBehaviour
{
    public GameObject _prefab1;
    public GameObject _prefab2;
    public GameObject _prefab3;
    public GameObject _prefab4;
    public GameObject _prefab5;

    class LayoutBaker : Baker<LayoutAuthoring>
    {
        public override void Bake(LayoutAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Layout{
                Prefab1 = GetEntity(authoring._prefab1, TransformUsageFlags.Dynamic),
                Prefab2 = GetEntity(authoring._prefab2, TransformUsageFlags.Dynamic),
                Prefab3 = GetEntity(authoring._prefab3, TransformUsageFlags.Dynamic),
                Prefab4 = GetEntity(authoring._prefab4, TransformUsageFlags.Dynamic),
                Prefab5 = GetEntity(authoring._prefab5, TransformUsageFlags.Dynamic),
                SpawnPositionX = authoring.transform.position.x,
                SpawnPositionY = authoring.transform.position.y
            });
        }
    }

}