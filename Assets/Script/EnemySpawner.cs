using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Predator spawnObject;

    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public void SpawnPredators(int amountToSpawn)
    {
        for (int index = 0; index < amountToSpawn; index++)
        {
            float randomX = Random.Range(-_collider.size.x / 2, _collider.size.x / 2);
            float randomZ = Random.Range(-_collider.size.z / 2, _collider.size.z / 2);

            Instantiate(spawnObject, new Vector3(randomX + transform.position.x, 0, randomZ + transform.position.z), Quaternion.identity);
        }
    }
}
