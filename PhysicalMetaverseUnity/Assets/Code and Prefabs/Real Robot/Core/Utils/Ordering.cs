
using System.Collections.Generic;


public interface IOrdered
{
    public int Order { get; }
}

public static class OrderHelper
{
    public static void SortByOrderDescending<T>(List<T> orderedList) where T : IOrdered =>
        orderedList.Sort((a, b) =>
            a.Order.CompareTo(b.Order));
}
