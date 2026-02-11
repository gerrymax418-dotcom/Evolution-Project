using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Predator : MonoBehaviour
{
    [SerializeField] private float patrolTime;
    [SerializeField] private float patrolDistance;
    [SerializeField] private float checkTime;

    private NavMeshAgent _agent;
    private Subject _trackedSubject;
    private List<Subject> _attemptedToEat;

    private float _viewDistance;
    private float _patrolTimer;
    private float _checkTimer;

    private bool _eaten;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (ChasingTarget())
        {
            _agent.destination = _trackedSubject.transform.position;

            EatTarget();

            return;
        }

        HandleLookingForTarget();
        HandlePatrol();
    }

    private void EatTarget()
    {
        float distance = Vector3.Distance(_trackedSubject.transform.position, transform.position);

        if (distance < _agent.stoppingDistance)
        {
            // Figure out how a target can escape
            // if target escapes add them to attempted list so we can ignore them in the future
            // else
            {
                _eaten = true;
                _trackedSubject.Eaten();
            }
        }
    }

    private void HandlePatrol()
    {
        if (ChasingTarget()) return;

        if (_agent.stoppingDistance <= _agent.remainingDistance)
        {
            _agent.isStopped = true;

            _patrolTimer += Time.deltaTime;

            if (_patrolTimer > patrolTime)
            {
                _patrolTimer = 0;
                Vector3 randomDeltaPosition = new Vector3(Random.Range(-patrolDistance, patrolDistance), 0f, Random.Range(-patrolDistance, patrolDistance));

                _agent.destination = transform.position + randomDeltaPosition;
                _agent.isStopped = false;
            }
        }
    }

    private void HandleLookingForTarget()
    {
        _checkTimer += Time.deltaTime;

        if (_checkTimer > checkTime)
        {
            _checkTimer = 0;

            if (!_eaten)
            {
                FindClosestSubjectInViewDistance();
            }
        }
    }

    private void FindClosestSubjectInViewDistance()
    {
        float closestSubject = float.MaxValue;

        foreach (Subject subject in FindObjectsByType<Subject>(FindObjectsSortMode.None))
        {
            if (_attemptedToEat.Contains(subject)) continue;

            Vector3 subjectPosition = subject.transform.position;

            float distance = Vector3.Distance(subjectPosition, transform.position);

            if (distance > _viewDistance) continue;

            if (distance < closestSubject)
            {
                _trackedSubject = subject;
            }
        }
    }

    public void Initialize(float speed, float viewDistance)
    {
        _viewDistance = viewDistance;
    }

    public void Reset()
    {
        _eaten = false;
        _trackedSubject = null;
        _attemptedToEat = new();
    }

    private bool ChasingTarget() => _trackedSubject != null && !_eaten;
}
