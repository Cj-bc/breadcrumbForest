using System.Collections;
using System.Collections.Generic;
using System.Text;

public class BreadcrumbForest<T> : IEnumerable<T> where T : IEquatable<T>
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

    internal BreadcrumbForest(List<T> items, List<Relation> relations)
    {
        m_Items = items;
        m_Relations = relations;
        m_RootIndices = relations
            .Select((item, idx) => (item, idx))
            .Where(i => i.item.Parent == -1)
            .Select(i => i.idx).ToList();

        m_CurrentIdx = toLeafNodeIdx(m_RootIndices[0]);
    }

    public T Current
    {
        get => m_Items[m_CurrentIdx];
    }

    public List<T> Children => ChildrenOf(m_CurrentIdx);

    // Returns parent item if available, null if Current item is one of the root.
    public T? Parent
    {
        get
        {
            int parentIdx = m_Relations[m_CurrentIdx].Parent;
            return parentIdx == -1 ? default : m_Items[parentIdx]; // TODO: Use Null
        }
    }


    // returns fale when end of contents
    public bool Next()
    {
        int _nextIdx = nextIdx(m_CurrentIdx, -1);
        if (_nextIdx != -1)
        {
            m_CurrentIdx = _nextIdx;
            return true;
        }
        return false;
    }
    private int nextIdx(int currentIdx, int previousIdx)
    {
        List<int> childrenIndices = m_Relations[currentIdx].Children;
        int parentIdx = m_Relations[currentIdx].Parent;

        if (childrenIndices.Count == 0)
        {
            List<int> siblings = parentIdx == -1
                ? m_RootIndices
                : m_Relations[parentIdx].Children;
            int siblingIndex = siblings.FindIndex(i => i.Equals(currentIdx));

            if (siblingIndex < siblings.Count - 1)
            {
                return siblings[siblingIndex + 1];
            }

            // Last sibling

            if (parentIdx == -1) return -1; // Last sibling of root items = no items are left

            return nextIdx(parentIdx, currentIdx);
        }

        if (previousIdx == -1)
        {
            return toLeafNodeIdx(childrenIndices[0]);
        }

        int previousChildIdx = childrenIndices.FindIndex(i => i.Equals(previousIdx));
        if (previousChildIdx < childrenIndices.Count - 1)
        {
            return toLeafNodeIdx(childrenIndices[previousChildIdx + 1]);
        }
        if (parentIdx != -1)
        {
            return nextIdx(parentIdx, currentIdx);
        }
        return -1;
    }

    // Returns true if current is updated, false otherwise.
    public bool SetCurrent(T item)
    {
        if (!m_Items.Contains(item)) return false;
        int idx = m_Items.FindIndex((candidate) => candidate.Equals(item));

        if (idx == -1) return false;

        m_CurrentIdx = idx;
        return true;
    }

    public static BreadcrumbForest<T> FromItems(Func<Func<T, Item<T>>, Item<T>> reg)
    {
        List<T> items = [];
        List<BreadcrumbForest<T>.Relation> relations = [];
        reg((i) => new Item<T>(i)).Build(-1, 0, items, relations);
        return new BreadcrumbForest<T>(items, relations);
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

    private List<T> ChildrenOf(int idx) => m_Relations[idx].Children.Select(i => m_Items[i]).ToList();

    private int toLeafNodeIdx(int startIdx)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(startIdx, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startIdx, m_Relations.Count);
        var children = m_Relations[startIdx].Children;

        return children.Count == 0
            ? startIdx
            : toLeafNodeIdx(children[0]);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return m_Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
