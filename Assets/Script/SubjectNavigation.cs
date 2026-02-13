using UnityEngine;
using UnityEngine.AI;

// This is forcing us to have a NavMeshAgent on our Subject Navigation making it safer for us to use
[RequireComponent(typeof(NavMeshAgent))]
public class SubjectNavigation : MonoBehaviour
{
    private NavMeshAgent _agent;

    // Regions are way to group a piece of code together
    // just to make the script cleaner and easier to use
    // #region [name of the region]
    // #endregion
    #region PREVIOUS_HOMEWORK_SOLUTIONS
    private void FindClosestFood()
    {
        // This is getting all the foods in the game.
        // Since we are running this on start this will only run at the start of the game once
        // The FindObjectsByType<DataTypeWeAreLookingFor>() method returns an array
        // You can learn more about arrays in this section of this video https://youtu.be/798cyzhQYSo?si=04ZC3qcE96mI41q-&t=536
        Food[] foods = FindObjectsByType<Food>(FindObjectsSortMode.None);

        // We are creating the float for the closest distance.
        // The reason we are setting it to float.MaxValue is because we know that any other value will be guaranteed lower than float.MaxValue
        // this means even if a food is really far away when we run the code the closest distance will be replaced with the first one
        float closestDistance = float.MaxValue;

        // Here we are tracking what is the position of the closest food
        // new() is just creating a default position to store as a placeholder
        //
        // new() = new Vector3(0,0,0) = Vector3.Zero
        Vector3 closestFoodPosition = new();

        // This is our loop where we are checking
        for (int index = 0; index < foods.Length; index++)
        {
            // Since we have all of our foods inside of an array we have to provide a number for our foods array to be able to access an individual food element
            // foods you can think of a list of individual food components. So in order to access one element inside of our list we use the following syntax
            // foods[ the index of the food we are trying to check ];
            // arrays begin at 0 so during our first loop our code will look like this
            // Food currentFoodWeAreChecking = foods[ 0 ];
            // Because arrays start at 0 foods [ 0 ] will return the first element in our array's list
            Food currentFoodWeAreChecking = foods[index];

            // Here we are checking the distance from our current food position to our position
            // Unity provides us with a handy method that handles all this math for us
            // Vector3.Distance(position1, position2) gives us a float value of how far away they are from each other
            float distanceToFood = Vector3.Distance(currentFoodWeAreChecking.transform.position, transform.position);

            // if the distance is less than the previous closest distance we know that this food is closer
            // In that case we want to run our logic
            if (distanceToFood < closestDistance)
            {
                // since we know that our distance to food is smaller than the previous closest distance we want to override the closest distance to our new distance to food
                closestDistance = distanceToFood;

                // Here we are actually grabbing the position of the food we are looking at and setting that to the position we want to go to
                closestFoodPosition = currentFoodWeAreChecking.transform.position;
            }
        }

        // Here because we know that we checked all the food positions
        // we know that this closest food position is the correct position for our food
        _agent.destination = closestFoodPosition;
    }
    #endregion

    private void Start()
    {
        // We don't have to have this inside our Find Closest Food Method since we only want to do this once
        _agent = GetComponent<NavMeshAgent>();

        FindClosestFood();
    }

    // Update is called once per frame
    void Update()
    {
        FindClosestFood();
    }
}
