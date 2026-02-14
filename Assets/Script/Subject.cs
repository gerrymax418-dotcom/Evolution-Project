using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Subject : MonoBehaviour
{
    // These are called properties. A special thing about these
    // is that they are public meaning that they are accessible to other
    // classes in our code, but they will NOT appear in the editor
    // In this case this is what we want so I set these as properties
    // the { get; private set; } means that any other code can get the 
    // value for each property but private set; means that only 
    // This subject class can change it
    public float Size {  get; private set; }
    public float Speed { get; private set; }
    public int FoodRequirement {  get; private set; }
    public bool WasChased { get; private set; }

    // Instead of holding a GameObject we can hold a model.
    // In game development it is always a good idea to separate out your
    // game logic from your visuals. This is that separation
    [SerializeField] private Transform model;

    // Cached values for using throughout the script
    private NavMeshAgent _agent;
    private Food _trackedFood;
    private SubjectHome _nearestSpawner;

    // The heat stuff is yet to be implemented. Need some sort of logic behind this to
    // work these are just here to actually use later
    private float _heatCapacity;

    // const means that this value NEVER changes. When we create the const
    // variable we have to assign the value and UPPER_CASING is the standard
    // convention for writing const values. Sometimes you will see people
    // using _UNDERSCORE_BEGINNING with this convention as well. I don't... you can.
    private const float HEAT_RESITANCE = 200f;

    // Runs every frame
    private void Update()
    {
        AddHeat();
        EatFood();
        EnterHome();

        // if trackedFood is null that means
        // that we are not looking for a food to eat
        // OR it was eaten by someone else already
        if (_trackedFood == null)
        {
            // If our foodrequirement is still above 0 then
            // we know that someone else ate our food. So if that's the
            // case we can just simply search for the next nearest food
            if (FoodRequirement > 0)
            {
                FindNearestFood();
            }

            // If not then that means that we have finished eating and we can just go home
            else
            {
                GoHome();
            }
        }
    }

    // This is what we're doing when we first initalize our subject
    public void Initialize(float size, float speed, bool wasChased = false)
    {
        // Assigning the values
        Size = size;
        Speed = speed;

        // this is using the ternary operator I mentioned.
        // FoodRequirement is equal to if wasChased then 3 else 2
        FoodRequirement = wasChased ? 3 : 2;

        // Assigning _agent
        _agent = GetComponent<NavMeshAgent>();

        // Scaling the visual representation of the model
        // The reason we are using Vector3.one is because
        // we want to Size to convert to a Vector3. The easiest
        // way to do that is to multiply it by a Vector3.one
        // Our scale on our transform component shows that our scale
        // is divided into 3 values. Our x y and z
        model.localScale = Vector3.one * Size;

        // this is just dividing the size to a radius since we will use
        // these values to change our _agent.radius
        float visualRadius = Size / 2f;

        // see
        _agent.radius = visualRadius;

        // We are using the stopping distance to determine if our subject has eaten
        // a food pellet. the + .1f is a tiny buffer we are giving the agent so it
        // doesn't have to be right ontop of the food. We also add the visual radius
        // because visual radius is the size of our actual capsule.
        _agent.stoppingDistance = visualRadius + .1f;
        _agent.speed = Speed;

        FindNearestFood();
    }

    private void AddHeat()
    {
        // Nothing exists here we have to add the logic for how size and heat
        // interact with our subjects
    }

    // This is checking if we are close enough to the home to actually enter the home
    private void EnterHome()
    {
        // If _nearestSpawner is null that means that we are not going home
        // and we can ignore the rest of the code. So we return to say
        // ignore the rest of the code don't run it
        if (_nearestSpawner == null) return;

        // If we do have a nearestSpawner we know we are going home. But
        // if the nearest spawner is Full that means we won't be able to fit inside
        // so we should look for a different home.
        if (_nearestSpawner.Full())
        {
            GoHome();
            return;
        }

        // Here we are checking if we are actually close enough to enter the home
        if (Vector3.Distance(transform.position, _nearestSpawner.transform.position) < _agent.stoppingDistance)
        {
            // Here we are creating that container class that just holds data
            // we are creating a new lightweight version of the subject called
            // subject data
            SubjectData data = new();

            // Here we are assigning the data based on this subject's values
            data.Speed = Speed;
            data.WasChased = WasChased;
            data.Size = Size;

            // Then we add the data we created to the spawner or home
            _nearestSpawner.AddSubject(data);

            // we are removing ourselves from the simulation, that's what this means
            // it means ourselves and true in this case means that we survived.
            // you can check out the method by clicking f12
            SimulationManager.Instance.RemoveFromSimulation(this, true);

            // Once we have removed ourselves we can safely destroy the gameObject
            // It's important to note that Destroy doesn't destory the game object
            // immediately. It stores the game object and destroys it later.
            // So you can actually call destroy anywhere in this if statement and it
            // won't break anything... HOWEVER this be the cause of some issues.
            // in fact there is a bug in this version of the game because of this 
            // not destroying immediately behavior that is causing our code to try
            // to assign an agent's destination after we've been destroyed.
            Destroy(gameObject);
        }
    }

    // Finding the closest home that can support us
    public void GoHome()
    {
        float closestSubject = float.MaxValue;

        // Here we are setting the nearest spawner to null because, we will eventually
        // have it assigned after the loop we want to ensure it's null right now
        // so that when we check at the end if it's null we know that there are no
        // more available homes that can fit us.
        _nearestSpawner = null;

        foreach (SubjectHome spawner in FindObjectsByType<SubjectHome>(FindObjectsSortMode.None))
        {
            // Checking if the spawner is full, if it is then we continue to the next
            // spawner in our FindObjects array
            if (spawner.Full()) continue;

            // Getting the spawner position
            Vector3 spawnerPosition = spawner.transform.position;

            // checking this distance from us to the spawner
            float distance = Vector3.Distance(spawnerPosition, transform.position);

            // if the distance is closer than we assign our _nearest spawner and
            // check if the next spawner on the next iteration if that is closer
            if (distance < closestSubject)
            {
                closestSubject = distance;
                _nearestSpawner = spawner;
            }
        }

        // If this is null that means that all the spawners are full and we cannot
        // rest anywhere for the night. That means that we die
        if (_nearestSpawner == null)
        {
            // This is removing ourselves from the simulation
            // false in this case means that we didn't survive
            SimulationManager.Instance.RemoveFromSimulation(this, false);

            // Destroying ourselves here
            Destroy(gameObject);

            // this return is saying ignore all the code after this line. Since
            // we are destroying oursevles we don't want to run the rest of the code
            return;
        }

        // this if statement was to try to solve the Destroy bug I mentioned earlier. But
        // this didn't work. Actually we can try if _agent is null maybe that will work
        // I'll add that in and you'll know if it works or not
        if (gameObject.activeInHierarchy &&

            // Lets see if this works, your present self will know bofore my past self.
            _agent != null)
        {

            // Setting our destination to the nearest spawner's position;
            _agent.destination = _nearestSpawner.transform.position;
        }
    }

    // Eating food if we can
    private void EatFood()
    {
        // if we don't have a tracked food we are not trying to eat anything and can 
        // ignore the rest of the code
        if (_trackedFood == null) return;

        // caching our distance for reaadability
        float distance = Vector3.Distance(transform.position, _trackedFood.transform.position);

        // if we are close enough we can check this by using our _agent.stoppingDistance
        // value that we set when we initialized this subject
        if (distance < _agent.stoppingDistance)
        {
            Destroy(_trackedFood.gameObject);
            FoodRequirement--;
        }
    }

    // Looking for the closets food
    private void FindNearestFood()
    {
        // setting this to the maximum possible value
        float closestSubject = float.MaxValue;

        // searching for the closest food
        foreach (Food food in FindObjectsByType<Food>(FindObjectsSortMode.None))
        {
            Vector3 subjectPosition = food.transform.position;

            float distance = Vector3.Distance(subjectPosition, transform.position);

            if (distance < closestSubject)
            {
                closestSubject = distance;
                _trackedFood = food;
            }
        }
        
        // if we did'nt find a food then there is no more food available and we die
        // from starvation.
        if (_trackedFood == null)
        {
            // We are removing ourselves from the simulation and setting survived to
            // false since we died from starvation.
            SimulationManager.Instance.RemoveFromSimulation(this, false);
            Destroy(gameObject);

            // We don't want to set our destination because we are dead so we return
            return;
        }

        // setting our destination because there is still food available
        _agent.destination = _trackedFood.transform.position;
    }

    // This method is called by the enemy class to tell us that we died 
    // because we were eaten.
    public void Eaten()
    {
        // We are destroyed because we got eaten
        Destroy(gameObject);

        // We didn't survive so we are setting that value to false
        SimulationManager.Instance.RemoveFromSimulation(this, false);
    }

    // Letting us know that we were chased. So next round if we survive, we have to
    // search for more food
    public void Chase()
    {
        WasChased = true;
        // There should be a speed change here too
    }
}
