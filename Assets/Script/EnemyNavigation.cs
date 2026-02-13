using UnityEngine;
using UnityEngine.AI;

// This is telling Unity that we HAVE to have a NavMeshAgent component on the same game object that this component lives on.
// Making it safer for us during our development
//
// --**IT WILL YELL AT US IF WE ARE MISSING THE COMPONENT OR AUTOMATICALLY ADD IT FOR US**--
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigation : MonoBehaviour
{
    private NavMeshAgent _agent;

    // Awake is often used to assign all the variables for this class. This is called initilization
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // We're not using start because it's already running on update anyway
    private void Update()
    {
        FindClosestSubject();
    }

    // we put it inside of a method so we don't have to copy and paste in multiple locations
    private void FindClosestSubject()
    {
        // squared brackets to indicate we want to get an array not just a single SubjectNavigation
        SubjectNavigation[] subjects;

        // We are getting the SubjectNavigation array using the FindObjectsByType method
        subjects = FindObjectsByType<SubjectNavigation>(FindObjectsSortMode.None);

        // Make this value as big as possible because we're trying to get the smallest value
        float closestDistance = float.MaxValue;

        // This is just us creating the variable to store the location of the closest subject
        Vector3 closestPosition = new();

        for (int i = 0; i < subjects.Length; i++)
        {
            // Doesn't matter if we use our position or the subject position first, it's still the same distance
            float distance = Vector3.Distance(transform.position, subjects[i].transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPosition = subjects[i].transform.position;
            }
        }

        // If you just have one line of code after an if statement you don't need to use the brackets.
        // I don't recommend this but it is possible and you will see a lot of people doing this
        //
        // We are checking if the _agent variable is assigned and if it is not then we assign it.
        // This is called a null safety check just in case for whatever reason we haven't assigned it
        // this will assign it.
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();

        // This is just saying that if our closest position is equal to zerothen we probably didn't find anyone
        // and we can just not set the position. It is possible that a subject can be on Vector3.zero but then
        // the next frame they will probably move off. So this is pretty safe to do
        if (closestPosition == Vector3.zero) return;

        _agent.destination = closestPosition;
    }
}
