using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;
    public List<Subject> spawnedSubjects = new();
    public List<Subject> survivedSubjects = new();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;   
        }

        Instance = this;

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
