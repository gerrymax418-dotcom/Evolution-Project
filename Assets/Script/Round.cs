using System;

[Serializable]
public struct Round
{
    public int SubjectsAtStart { get; set; }
    public int SubjectsAtEnd { get; set; }
    public float AverageSpeed { get; set; }
    public float AverageSize {  get; set; }
}
