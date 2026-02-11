using UnityEngine;
using UnityEngine.AI;

public class SubjectNavigation : MonoBehaviour
{
    private NavMeshAgent _agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Food[] foods = FindObjectsByType<Food>(FindObjectsSortMode.None);
        float closestDistance = float.MaxValue;
        Vector3 closestFoodPosition = new();
        for (int index = 0; index < foods.Length; index++)
        {
            if (Vector3.Distance(foods[i].transform.position, transform.position))
            {
                closestDistance = //vector3.distance
            }
        }

        _agent = GetComponent<NavMeshAgent>();
        _agent.destination

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
