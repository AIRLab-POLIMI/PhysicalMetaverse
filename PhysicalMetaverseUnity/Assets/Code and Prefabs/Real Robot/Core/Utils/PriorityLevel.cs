
using System.Collections.Generic;

[System.Serializable]

public enum PriorityLevel
{
    High,
    Medium,
    Low
}

public interface IPrioritized
{
    public PriorityLevel Priority { get; }
}

public static class PriorityLevelHelper
{
    public static void SortByPriorityLevelDescending<T>(List<T> prioritizedList) where T : IPrioritized =>
        prioritizedList.Sort((a, b) =>
            a.Priority.CompareTo(b.Priority));
}
