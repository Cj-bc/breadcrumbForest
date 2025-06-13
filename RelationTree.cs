using System.Collections;
using System.Text;

public class RelationTree<T> where T : IEquatable<T>
{
    internal struct Relation
    {
        public int Parent;
        public List<int> Children;
    }

    private List<T> m_Items;
    private List<Relation> m_Relations;
    private List<int> m_RootIndices;
    private int _m_CurrentIdx = 0;
    private int m_CurrentIdx
    {
        set => _m_CurrentIdx = Math.Clamp(value, 0, m_Items.Count - 1);
        get => _m_CurrentIdx;
    }

    internal RelationTree(List<T> items, List<Relation> relations)
    {
        m_Items = items;
        m_Relations = relations;
        m_RootIndices = relations
            .Select((item, idx) => (item, idx))
            .Where(i => i.item.Parent == -1)
            .Select(i => i.idx).ToList();
    }

    public T Current
    {
        get => m_Items[m_CurrentIdx];
    }

    public List<T> Children => ChildrenOf(m_CurrentIdx);

    private List<T> ChildrenOf(int idx) => m_Relations[idx].Children.Select(i => m_Items[i]).ToList();

    // Returns true if current is updated, false otherwise.
    public bool SetCurrent(T item)
    {
        if (!m_Items.Contains(item)) return false;
        int idx = m_Items.FindIndex((candidate) => candidate.Equals(item));

        if (idx == -1) return false;

        m_CurrentIdx = idx;
        return true;
    }

    public static RelationTree<T> FromItems(Func<Func<T, Item<T>>, Item<T>> reg)
    {
        List<T> items = [];
        List<RelationTree<T>.Relation> relations = [];
        reg((i) => new Item<T>(i)).Build(-1, 0, items, relations);
        return new RelationTree<T>(items, relations);
    }

    public string ToDisplayString()
    {
        var builder = new StringBuilder();
        foreach (int rootItemIdx in m_RootIndices)
        {
            T rootItem = m_Items[rootItemIdx];
            builder.AppendLine($"{rootItem}");
            foreach (T item in ChildrenOf(rootItemIdx)) {
                builder.AppendLine($"`- {item}");
            }
        }
        return "";
    }
}
