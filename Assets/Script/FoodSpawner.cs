using System.Dynamic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject spawnObject;
    public BoxCollider collision;

    public void SpawnFood(int spawnAmount)
    {
        for (int index = 0; index < spawnAmount; index++)
        {
            float randomX = Random.Range(-collision.size.x / 2, collision.size.x / 2);
            float randomZ = Random.Range(-collision.size.z / 2, collision.size.z / 2);

            Instantiate(
                spawnObject,
                new Vector3(
                    transform.position.x + randomX,
                    0,
                    transform.position.z + randomZ),
                Quaternion.identity);
        }
    }
}
