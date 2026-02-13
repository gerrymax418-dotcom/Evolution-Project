using UnityEngine;

// Because of the way that we wrote this script we can actually just use this or all our spawning and we don't have to make new scripts
// We just drag and drop new prefabs into our spawnobject field in the Unity Editor
public class Spawner : MonoBehaviour
{
    public GameObject spawnObject;
    public BoxCollider collision;

    public int foodSpawnAmount;

    // Changed this to on enable so that all our objects are spawned before the first start method is called
    // https://docs.unity3d.com/2020.1/Documentation/Manual/ExecutionOrder.html if you want to reference what is
    // called in what order
    private void OnEnable()
    {
        for (int index = 0; index < foodSpawnAmount; index++) 
        {
            float randomX = Random.Range(-collision.size.x / 2, collision.size.x / 2);
            float randomZ = Random.Range(-collision.size.z / 2, collision.size.z / 2);

            Instantiate(
                spawnObject, 
                transform.position + new Vector3(randomX, 0, randomZ), 
                Quaternion.identity);
        }
    }
}
