using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Subject : MonoBehaviour
{
    public static event EventHandler<bool> OnRemovedFromSimulation;

    public float Size {  get; private set; }
    public float Speed { get; private set; }
    public int FoodRequirement {  get; private set; }
    public bool WasChased { get; private set; }

    private NavMeshAgent _agent;
    private Food _trackedFood;
    private FoodSpawner _nearestSpawner;

    private float _heatCapacity;

    private const float HEAT_RESITANCE = 200f;

    public void Initialize(float size, float speed, bool wasChased)
    {
        _agent = GetComponent<NavMeshAgent>();

        Size = size;
        Speed = speed;
        FoodRequirement = wasChased ? 3 : 2;

        transform.localScale = Vector3.one * Size;

        FindNearestFood();
    }

    private void Update()
    {
        AddHeat();
        EatFood();
        EnterHome();

        if (_trackedFood == null)
        {
            if (FoodRequirement > 0)
            {
                FindNearestFood();
            }
            else
            {
                GoHome();
            }
        }
    }

    private void AddHeat()
    {
        //
    }

    private void EnterHome()
    {
        if (_nearestSpawner == null) return;
        if (Vector3.Distance(transform.position, _nearestSpawner.transform.position) < _agent.stoppingDistance)
        {
            Destroy(gameObject);
            OnRemovedFromSimulation?.Invoke(this, true);
        }
    }

    private void GoHome()
    {
        float closestSubject = float.MaxValue;

        foreach (FoodSpawner spawner in FindObjectsByType<FoodSpawner>(FindObjectsSortMode.None))
        {
            // if (spawner.Full) continue;
            Vector3 spawnerPosition = spawner.transform.position;

            float distance = Vector3.Distance(spawnerPosition, transform.position);

            if (distance < closestSubject)
            {
                _nearestSpawner = spawner;
            }
        }

        _agent.destination = _nearestSpawner.transform.position;
    }

    private void EatFood()
    {
        if (_trackedFood == null) return;

        float distance = Vector3.Distance(transform.position, _trackedFood.transform.position);

        if (distance < _agent.stoppingDistance)
        {
            Destroy(_trackedFood.gameObject);
            FoodRequirement--;
        }
    }

    private void FindNearestFood()
    {
        float closestSubject = float.MaxValue;

        foreach (Food food in FindObjectsByType<Food>(FindObjectsSortMode.None))
        {
            Vector3 subjectPosition = food.transform.position;

            float distance = Vector3.Distance(subjectPosition, transform.position);

            if (distance < closestSubject)
            {
                _trackedFood = food;
            }
        }

        _agent.destination = _trackedFood.transform.position;
    }

    public void Eaten()
    {
        Destroy(gameObject);
        OnRemovedFromSimulation?.Invoke(this, false);
    }

    public void Chase()
    {
        WasChased = true;
        // There should be a speed change here too
    }
}
