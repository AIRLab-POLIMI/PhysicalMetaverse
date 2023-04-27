public abstract class SortedListSO<T> : ListSO<T> where T : IOrdered
{
    public override void Add(T value)
    {
        base.Add(value);

        OrderHelper.SortByOrderDescending(list);
    }
}