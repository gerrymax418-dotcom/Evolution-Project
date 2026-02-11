using UnityEngine;

[CreateAssetMenu(fileName = "New Enviornment", menuName = "Environment")]
public class EnvironmentSO : ScriptableObject
{
    [Header("Predator Settings")]
    public int EnemyCount;
    public float DetectionRadius;
    public float EnemySpeed;

    [Header("Environment Settings")]
    public int FoodCountPerRound;
    [Range(0f, 1f)]
    public float Heat;

    [Header("Subject Settings")]
    public int StartingSubjectCount;
}
