using UnityEngine;
using UnityEngine.AI;
using UnityEngine.LowLevelPhysics2D;

public class Subject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            Destroy(other.gameObject);
        }
    }
    public float detectionRadius = 5f;
    public float normalSpeed = 3.5f;
    public float escapeSpeed = 6f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        
    }

    void Update()
    {
        DetectEnemies();
    }

    void DetectEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        bool enemyNearby = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                enemyNearby = true;
                break;

            }
        }
        if (enemyNearby)
            agent.speed = escapeSpeed;
        else
                agent.speed = normalSpeed;
        agent.speed = enemyNearby ? escapeSpeed : normalSpeed;
        // the same as the if/else statement above, "?" meaning if it is then this, : else
        }
        
    }
