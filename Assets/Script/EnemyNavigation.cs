using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindClosestSubject();

    }

    private void FindClosestSubject()
    {
        Subject[] subjects = FindObjectsByType<Subject>(FindObjectsSortMode.None);
        float closestDistance = float.MaxValue;
        Vector3 closestSubjectPosition = new();
        for (int index = 0; index < subjects.Length; index++)
        {
            float distance = Vector3.Distance(subjects[index].transform.position, transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSubjectPosition = subjects[index].transform.position;
            }

        }

        _agent.destination = closestSubjectPosition;
    }

    // Update is called once per frame
    void Update()
    {
        FindClosestSubject();
    }
}
