using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Predator predatorPrefab;

    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public void SpawnPredators(int amountToSpawn, float detectionRadius, float speed)
    {
        for (int index = 0; index < amountToSpawn; index++)
        {
            float randomX = Random.Range(-_collider.size.x / 2, _collider.size.x / 2);
            float randomZ = Random.Range(-_collider.size.z / 2, _collider.size.z / 2);

            Predator predator = Instantiate(
                predatorPrefab, 
                new Vector3(randomX + transform.position.x, 0, randomZ + transform.position.z), 
                Quaternion.identity);

            predator.Initialize(speed, detectionRadius);
        }
    }
}
