using System.Dynamic;
using UnityEngine;


public class Spawner : MonoBehaviour
{
    public GameObject spawnObject;
    public int foodSpawnAmount;
    public BoxCollider collision;
    public Transform location;
    void OnEnable()
    {
        //Instantiate(spawnObject, /* figure out how to find a random location for it to spawn*/);
        
        for (int index = 0; index < foodSpawnAmount; index++) //loop
        {
            float randomX = Random.Range(-collision.size.x / 2, collision.size.x / 2);
            float randomZ = Random.Range(-collision.size.z / 2, collision.size.z / 2);
            Instantiate(spawnObject, new Vector3(location.position.x + randomX,0,location.position.z + randomZ), Quaternion.identity);
            if (index == 0)
            {
                Debug.Log("index \n = 0");
            }
            if (index == 0 && index <= 1)
            {
                Debug.Log("index = 0");
            }
            if (index == 0 || index > 0)
            {
                Debug.Log("index = 0");
            }
            //runs "index = 0" in the console 4 times

        }

        







    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
