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
    internal int Build(int parentIdx, int nextIdx, List<T> registeredItems, List<RelationTree<T>.Relation> registeredRelations)
    {
        registeredItems.Add(m_Item);
        if (Children.Count == 0)
        {
            registeredRelations.Add(new RelationTree<T>.Relation{Parent = parentIdx, Children = []});
            return nextIdx;
        }

        // Insert partial relation record to reserve slot
        // TODO: indexの配布の仕方を工夫すれば一度でいける気もする。が、空間の効率も悪くなるから微妙か
        registeredRelations.Add(new RelationTree<T>.Relation{Parent = parentIdx, Children = new() { parentIdx + 2 }});

        int lastIdx = parentIdx + 1;
        foreach (var child in Children)
        {
            lastIdx = child.Build(parentIdx + 1, lastIdx + 1, registeredItems, registeredRelations);
        }
        return lastIdx;
    }
}
