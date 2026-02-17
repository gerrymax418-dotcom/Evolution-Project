using UnityEngine;

public class SubjectHome : MonoBehaviour
{
    [SerializeField] private Subject prefab;

    // This is an array of subject data that is 2 wide.
    // I was having issues that I believe were unrelated but I ended up
    // separating the values of the subject into their own class.
    // this isn't too bad because it makes passing around data much faster.
    // If you are constantly using data, it is a good idea to make a lightweight
    // container class such as SubjectData that doesn't inherit from anything
    // and just holds data. Check it out by clickng on SubjectData and clicking f12
    private SubjectData[] _subjects = new SubjectData[2];

    // This was a debugging step. For some reason _subjects was returning null even after
    // we assigned them. This was just to make sure that the _subjects array existed
    // and that they had a null in both indexes
    private void Awake()
    {
        _subjects = new SubjectData[2]
            { null, null};
    }

    // This method is for other objects to "FILL" our home. So if a subject approaches
    // they can add themselves into the home. And when we first start our simulation.
    // We can add random subjects to our home.
    public void AddSubject(SubjectData subject)
    {
        if (Full()) return;

        int addIndex = _subjects[0] == null ? 0 : 1;
        _subjects[addIndex] = subject;
    }

    public void SpawnSubjects(float heat, float offSpringDifferential, bool withChildren = true)
    {
        // This if statement is to spawn our child of our two subjects
        if (withChildren)
        {
            // We are checking if it's full because then we know the two
            // adults can procreate and make an offspring
            // but if there's only one subject in the home, that wouldn't make
            // sense to have an offspring
            if (Full())
            {
                // We are getting the averages by first adding the first element of our
                // subjects array and second element together
                float averageSize = _subjects[0].Size + _subjects[1].Size;
                float averageSpeed = _subjects[0].Speed + _subjects[1].Speed;

                // We then divide them by 2
                // /= 2 is the same as averageSize = averageSize / 2;
                averageSize /= 2;
                averageSpeed /= 2;

                // We are getting the averages and we are adding a random value
                // the randomvalue is the averge * the differential we set
                // if our average size = 10 and our differential is .5
                // the random value will return anywhere between -5 and 5
                // we are then adding this value to our original average size
                // this makes it so our possible size is between 5 and 15
                float randomSize = averageSize + Random.Range(-offSpringDifferential * averageSize, offSpringDifferential * averageSize);
                float randomSpeed = averageSpeed + Random.Range(-offSpringDifferential * averageSpeed, offSpringDifferential * averageSpeed);

                // Here we are instantiating the subject and intializing it
                // you can take a look at initializing by clicking on it
                // and pressing f12
                Subject subject = Instantiate(prefab, transform.position, Quaternion.identity);
                subject.Initialize(randomSize, randomSpeed, heat);

                // We are passing in the subject that we Instantiated and
                // giving it to our simulation manager
                SimulationManager.Instance.AddToSimulation(subject);
            }
        }

        // This loop is to spawn the actual subjects not the children
        for (int i = 0; i < _subjects.Length; i++)
        {
            // Here we are checking if there's an actual subject in the array
            // If there is then we do want to spawn a subject
            if (_subjects[i] != null)
            {
                // instantiating the subject
                Subject subject = Instantiate(prefab, transform.position, Quaternion.identity);
                
                // Initalizing it just giving it, just passing through it's original values
                subject.Initialize(_subjects[i].Size, _subjects[i].Speed, heat, _subjects[i].WasChased);

                // Giving the subject we spawned to the simulation manager
                SimulationManager.Instance.AddToSimulation(subject);
            }

            // this is really important after we spawn the subject we want to
            // set the _subject[i] to null so the we can say that there is no
            // longer a subject in here. This is basically like freeing a slot
            // for a subject to come into our home
            _subjects[i] = null;
        }
    }

    // We're just checking the first and second element. If they are both
    // not null then we know that we as a home are full.
    public bool Full() => _subjects[0] != null && _subjects[1] != null;
}
