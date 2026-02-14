using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // our predator prefab
    [SerializeField] private Predator predatorPrefab;

    // the collider for spawning enemies
    private BoxCollider _collider;

    // Assigning our collider
    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    // this is pretty simple just spawning in our predators but with one distinction.
    // we are also initializing our values based on our enviornment values
    public void SpawnPredators(int amountToSpawn, float detectionRadius, float speed)
    {
        for (int index = 0; index < amountToSpawn; index++)
        {
            // Getting random values depending on our collider
            float randomX = Random.Range(-_collider.size.x / 2, _collider.size.x / 2);
            float randomZ = Random.Range(-_collider.size.z / 2, _collider.size.z / 2);

            Predator predator = Instantiate(
                predatorPrefab, 
                // we use _collider.center to take into account our collider position
                _collider.center + new Vector3(randomX, 0, randomZ), 
                Quaternion.identity);

            // Here we are passing through the values we got from the enviornment to
            // determine our predator speed and detection radius
            predator.Initialize(speed, detectionRadius);
        }
    }
}
