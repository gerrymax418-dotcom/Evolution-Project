using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Subject"))
        {
            Destroy(other.gameObject);
        }
    }
}