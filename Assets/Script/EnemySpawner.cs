using UnityEngine;

public class EnemySpawner : MonoBehaviour

{
    public GameObject spawnObject;
    public int enemySpawnAmount;
    public BoxCollider size;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int index = 0; index < enemySpawnAmount; index++) 
        {
            float randomX = Random.Range(-size.size.x / 2, size.size.x / 2);
            float randomZ = Random.Range(-size.size.z / 2, size.size.z / 2);
            Instantiate(spawnObject, new Vector3(randomX + transform.position.x, 0, randomZ + transform.position.z), Quaternion.identity);
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
