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
            float distance = Vector3.Distance(foods[index].transform.position, transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFoodPosition = foods[index].transform.position;
            }

        }

        _agent = GetComponent<NavMeshAgent>();
        _agent.destination = closestFoodPosition;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
