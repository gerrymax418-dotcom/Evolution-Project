using System;

[Serializable]
public class Round
{
    public int SubjectsAtStart { get; set; }
    public int SubjectsAtEnd { get; set; }
    public int HeatDeath { get; set; }
    public int Eaten { get; set; }
    public int ExposureDeath { get; set; }
    public int StarvationDeath { get; set; }
    public float AverageSpeed { get; set; }
    public float AverageSize {  get; set; }
}
