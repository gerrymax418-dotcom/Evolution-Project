using System.Collections.Generic;
using UnityEngine;
using System;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;

    [Header("Starting Values")]
    [SerializeField] private EnvironmentSO environment;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private SubjectHome homePrefab;

    private List<Subject> _spawnedSubjects = new List<Subject>();
    private List<Subject> _survivedSubjects = new();
    private List<Round> _rounds = new();

    private SubjectHome[] _homes;

    private Round _currentRound = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartSimulation();
    }

    private void StartSimulation()
    {
        SpawnAndCacheHomes();
        AddSubjectsToHomes();

        enemySpawner.SpawnPredators(
            environment.EnemyCount,
            environment.DetectionRadius,
            environment.EnemySpeed);

        foodSpawner.SpawnFood(environment.FoodCountPerRound);

        SpawnSubjects(false);
    }

    private void SpawnSubjects(bool withChildren)
    {
        foreach (SubjectHome home in _homes)
        {
            home.SpawnSubjects(environment.offspringDifferential, withChildren);
        }
    }

    private void AddSubjectsToHomes()
    {
        int currentHome = 0;

        for (int i = 0; i < environment.StartingSubjectCount; i++)
        {
            while (currentHome < _homes.Length && _homes[currentHome].Full())
            {
                currentHome++;
            }

            float randomSize = environment.BaseSize +
                UnityEngine.Random.Range(-environment.SizeDifferential, environment.SizeDifferential);
            float randomSpeed = environment.BaseSpeed +
                UnityEngine.Random.Range(-environment.SpeedDifferential, environment.SpeedDifferential);

            SubjectData newSubject = new();

            newSubject.Size = randomSize;
            newSubject.Speed = randomSpeed;

            _homes[currentHome].AddSubject(newSubject);
        }
    }

    private void SpawnAndCacheHomes()
    {
        List<Vector3> homePositions = SpawnerRingPositions.GenerateRing(environment.StartingSubjectCount);

        _homes = new SubjectHome[homePositions.Count];

        for (int i = 0; i < homePositions.Count; i++)
        {
            SubjectHome spawnedHome = Instantiate(homePrefab, homePositions[i], Quaternion.LookRotation(-homePositions[i], Vector3.up));
            _homes[i] = spawnedHome;
        }
    }

    public void AddToSimulation(Subject subject)
    {
        _spawnedSubjects.Add(subject);
        _currentRound.SubjectsAtStart = _spawnedSubjects.Count;
    }

    public void RemoveFromSimulation(Subject subject, bool survived)
    {
        if (_spawnedSubjects.Contains(subject)) 
        {
            _spawnedSubjects.Remove(subject);

            if (survived)
            {
                _survivedSubjects.Add(subject);
            }
        }

        if (_spawnedSubjects.Count <= 0)
        {
            GatherData();
            StartNewRound();
        }
    }

    private void StartNewRound()
    {
        _currentRound = new();
        _survivedSubjects.Clear();
        _spawnedSubjects.Clear();

        foreach (Food food in FindObjectsByType<Food>(FindObjectsSortMode.None))
        {
            Destroy(food.gameObject);
        }

        foreach (Predator predator in FindObjectsByType<Predator>(FindObjectsSortMode.None))
        {
            predator.ResetEnemy();
        }

        foodSpawner.SpawnFood(environment.FoodCountPerRound);
        SpawnSubjects(true);
    }

    private void GatherData()
    {
        int survivedCount = _survivedSubjects.Count;
        float accumulatedSize = 0;
        float accumulatedSpeed = 0;

        foreach (Subject survivedSubject in _survivedSubjects)
        {
            accumulatedSize += survivedSubject.Size;
            accumulatedSpeed += survivedSubject.Speed;
        }

        _currentRound.SubjectsAtEnd = survivedCount;
        _currentRound.AverageSize = accumulatedSize / survivedCount;
        _currentRound.AverageSpeed = accumulatedSpeed / survivedCount;

        _rounds.Add(_currentRound);
    }
}

public static class SpawnerRingPositions
{
    public static List<Vector3> GenerateRing(
        int subjectCount,
        float radius = 200,
        Vector3 center = default,
        int subjectsPerSpawner = 2,
        float startAngleDegrees = 0)
    {
        if (subjectsPerSpawner <= 0) throw new ArgumentOutOfRangeException();
        if (subjectCount <= 0) return new();

        int spawnerCount = Mathf.CeilToInt((float)subjectCount / subjectsPerSpawner);
        List<Vector3> positions = new List<Vector3>(spawnerCount);

        float startRadian = startAngleDegrees * Mathf.Deg2Rad;
        float step = (Mathf.PI * 2f) / spawnerCount;

        for (int i = 0; i < spawnerCount; ++i)
        {
            float a = startRadian + step * i;
            float x = Mathf.Cos(a) * radius;
            float z = Mathf.Sin(a) * radius;
            positions.Add(center + new Vector3(x, 0f, z));
        }

        return positions;
    }
}