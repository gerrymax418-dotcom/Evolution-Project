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

    private List<Round> _rounds = new();
    private SubjectHome[] _homes;

    private int _currentNumberOfSubjects = 0;
    private Round _currentRound;

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
            environment.StartingSubjectCount,
            environment.DetectionRadius,
            environment.EnemySpeed);

        foodSpawner.SpawnFood(environment.FoodCountPerRound);

        SpawnSubjects(false);

        Round newRound = new Round();
        _currentRound = newRound;
        _currentRound.SubjectsAtStart = _currentNumberOfSubjects;
    }

    private void SpawnSubjects(bool withChildren)
    {
        foreach (SubjectHome home in _homes)
        {
            home.SpawnSubjects(environment.offspringDifferential, withChildren);
        }
    }

    private (int count, float size, float speed) GetSubjectData()
    {
        int subjectCount = 0;
        float accumulatedSize = 0;
        float accumulatedSpeed = 0;
        int realCount = 0;

        foreach (SubjectHome home in _homes)
        {
            subjectCount += home.GetSubjectCount();
            accumulatedSize += home.GetTotalSize();
            accumulatedSpeed += home.GetTotalSpeed();
            realCount += home.GetRealCount();
        }
        return (subjectCount, accumulatedSize / realCount, accumulatedSpeed / realCount);
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

            // if (currentHome >= _homes.Length) break;

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
        _currentNumberOfSubjects = environment.StartingSubjectCount;

        List<Vector3> homePositions = SpawnerRingPositions.GenerateRing(_currentNumberOfSubjects);

        _homes = new SubjectHome[homePositions.Count];

        for (int i = 0; i < homePositions.Count; i++)
        {
            SubjectHome spawnedHome = Instantiate(homePrefab, homePositions[i], Quaternion.LookRotation(-homePositions[i], Vector3.up));
            _homes[i] = spawnedHome;
        }
    }

    public void RemoveFromSimulation()
    {
        _currentNumberOfSubjects--;

        if (_currentNumberOfSubjects <= 0)
        {
            Subject[] subjects = FindObjectsByType<Subject>(FindObjectsSortMode.None);

            Debug.Log(subjects.Length);

            if (subjects.Length != 0)
            {
                subjects[0].GoHome();
                return;
            }

            Debug.Log(subjects.Length);

            // Collecting Data
            var data = GetSubjectData();

            _currentRound.SubjectsAtEnd = data.count;
            _currentRound.AverageSize = data.size;
            _currentRound.AverageSpeed = data.speed;

            _rounds.Add(_currentRound);

            //Starting a new Round
            _currentRound = new();

            _currentRound.SubjectsAtStart = data.count;
            _currentNumberOfSubjects = data.count;

            foreach(Food food in FindObjectsByType<Food>(FindObjectsSortMode.None))
            {
                Destroy(food.gameObject);
            }

            foreach(Predator predator in FindObjectsByType<Predator>(FindObjectsSortMode.None))
            {
                predator.ResetEnemy();
            }

            foodSpawner.SpawnFood(environment.FoodCountPerRound);
            SpawnSubjects(true);
        }
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