public class Item<T> where T : IEquatable<T>
{
    private T m_Item;
    private List<Item<T>> Children;

    internal Item(T item)
    {
        m_Item = item;
        Children = new();
    }

    public Item<T> WithChildren(params Item<T>[] items)
    {
        Children.AddRange(items);
        return this;
    }

    public int DecendantsCount() => Children.Select(c => c.DecendantsCount()).Sum();

    // Returns index of the last item of this children.
    internal int Build(int parentIdx, int nextIdx, List<T> registeredItems, List<BreadcrumbForest<T>.Relation> registeredRelations)
    {
        registeredItems.Add(m_Item);
        if (Children.Count == 0)
        {
            registeredRelations.Add(new BreadcrumbForest<T>.Relation{Parent = parentIdx, Children = []});
            return nextIdx;
        }

        // As we don't know children's indices, put empty children to reserver item in the list.
        // TODO: indexの配布の仕方を工夫すれば一度でいける気もする。が、空間の効率も悪くなるから微妙か
        registeredRelations.Add(new BreadcrumbForest<T>.Relation{Parent = parentIdx, Children = []});
        int currentRelationIdx = registeredRelations.Count - 1;

        int lastIdx = nextIdx;
        foreach (var child in Children)
        {
            registeredRelations.ElementAt(currentRelationIdx).Children.Add(lastIdx + 1);
            lastIdx = child.Build(nextIdx, lastIdx + 1, registeredItems, registeredRelations);
        }
        return lastIdx;
    }
}
