using System;

[Serializable]
public class Round
{
    public int SubjectsAtStart { get; set; }
    public int SubjectsAtEnd { get; set; }
    public int SubjectsDiedByHeat { get; set; }
    public int SubjectsDiedByPredator { get; set; }
    public float AverageSpeed { get; set; }
    public float AverageSize {  get; set; }
}
