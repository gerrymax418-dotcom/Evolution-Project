using UnityEngine;

public class SubjectHome : MonoBehaviour
{
    [SerializeField] private Subject prefab;

    private SubjectData[] _subjects = new SubjectData[2];

    private void Awake()
    {
        _subjects = new SubjectData[2]
            { null, null};
    }

    public void AddSubject(SubjectData subject)
    {
        if (Full()) return;

        int addIndex = _subjects[0] == null ? 0 : 1;
        _subjects[addIndex] = subject;
    }

    public void SpawnSubjects(float offSpringDifferential, bool withChildren = true)
    {
        if (withChildren)
        {
            if (Full())
            {
                float averageSize = _subjects[0].Size + _subjects[1].Size;
                float averageSpeed = _subjects[0].Speed + _subjects[1].Speed;

                averageSize /= 2;
                averageSpeed /= 2;

                float randomSize = averageSize + Random.Range(-offSpringDifferential * averageSize, offSpringDifferential * averageSize);
                float randomSpeed = averageSpeed + Random.Range(-offSpringDifferential * averageSpeed, offSpringDifferential * averageSpeed);

                Subject subject = Instantiate(prefab, transform.position, Quaternion.identity);
                subject.Initialize(randomSize, randomSpeed);
                Debug.Log("Spawned");

            }
        }

        for (int i = 0; i < _subjects.Length; i++)
        {
            if (_subjects[i] != null)
            {
                Debug.Log("Spawned");
                Instantiate(prefab, transform.position, Quaternion.identity).Initialize(_subjects[i].Size, _subjects[i].Speed, _subjects[i].WasChased);
            }

            _subjects[i] = null;
        }
    }

    public float GetTotalSpeed()
    {
        float accumulatedSpeed = 0;

        for (int i = 0; i < _subjects.Length; i++)
        {
            if (_subjects[i] == null) continue;

            accumulatedSpeed += _subjects[i].Speed;
        }

        return accumulatedSpeed;
    }

    public float GetTotalSize()
    {
        float accumulatedSize = 0;

        for (int i = 0; i < _subjects.Length; i++)
        {
            if (_subjects[i] == null) continue;

            accumulatedSize += _subjects[i].Size;
        }

        return accumulatedSize;
    }

    public int GetSubjectCount()
    {
        int count = 0;

        foreach (SubjectData subject in _subjects) 
        {
            if (subject != null)
                count++;
        }

        return count == 2 ? 3 : count;
    }
    public int GetRealCount()
    {
        int count = 0;

        foreach (SubjectData subject in _subjects)
        {
            if (subject != null)
                count++;
        }

        return count;
    }

    public bool Full() => _subjects[0] != null && _subjects[1] != null;
}
