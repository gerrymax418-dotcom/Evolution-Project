using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Subject : MonoBehaviour
{
    public float Size {  get; private set; }
    public float Speed { get; private set; }
    public int FoodRequirement {  get; private set; }
    public bool WasChased { get; private set; }

    [SerializeField] private Transform model;

    private NavMeshAgent _agent;
    private Food _trackedFood;
    private SubjectHome _nearestSpawner;

    private float _heatCapacity;

    private const float HEAT_RESITANCE = 200f;

    public void Initialize(float size, float speed, bool wasChased = false)
    {
        Size = size;
        Speed = speed;
        FoodRequirement = wasChased ? 3 : 2;

        _agent = GetComponent<NavMeshAgent>();

        model.localScale = Vector3.one * Size;

        float visualRadius = Size / 2f;

        _agent.radius = visualRadius;
        _agent.stoppingDistance = visualRadius + .1f;
        _agent.speed = Speed;

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

        if (_nearestSpawner.Full())
        {
            GoHome();
            return;
        }

        if (Vector3.Distance(transform.position, _nearestSpawner.transform.position) < _agent.stoppingDistance)
        {
            

            SubjectData data = new();

            data.Speed = Speed;
            data.WasChased = WasChased;
            data.Size = Size;

            _nearestSpawner.AddSubject(data);
            SimulationManager.Instance.RemoveFromSimulation();
            Destroy(gameObject);
        }
    }

    public void GoHome()
    {
        float closestSubject = float.MaxValue;
        _nearestSpawner = null;

        foreach (SubjectHome spawner in FindObjectsByType<SubjectHome>(FindObjectsSortMode.None))
        {
            if (spawner.Full()) continue;

            Vector3 spawnerPosition = spawner.transform.position;

            float distance = Vector3.Distance(spawnerPosition, transform.position);

            if (distance < closestSubject)
            {
                closestSubject = distance;
                _nearestSpawner = spawner;
            }
        }

        if (_nearestSpawner == null)
        {
            SimulationManager.Instance.RemoveFromSimulation();
            Destroy(gameObject);
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            _agent.destination = _nearestSpawner.transform.position;
        }
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
                closestSubject = distance;
                _trackedFood = food;
            }
        }

        _agent.destination = _trackedFood.transform.position;
    }

    public void Eaten()
    {
        Destroy(gameObject);
        SimulationManager.Instance.RemoveFromSimulation();
    }

    public void Chase()
    {
        WasChased = true;
        // There should be a speed change here too
    }
}
