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
    private Dictionary<int, int> m_ParentsMap;
    private Dictionary<int, List<int>> m_ChildrenMap;
    private List<int> m_RootIndices;
    private int _m_CurrentIdx = 0;
    private int m_CurrentIdx
    {
        set => _m_CurrentIdx = Math.Clamp(value, 0, m_Items.Count - 1);
        get => _m_CurrentIdx;
    }
 
    internal BreadcrumbForest(IEnumerable<T> items, IEnumerable<Relation> relations)
    {
        m_Items = items.ToList();
        m_ParentsMap = relations.Select((item, idx) => (idx, item.Parent)).ToDictionary();
        m_ChildrenMap = relations.Select((item, idx) => (idx, item.Children)).ToDictionary();
        m_Relations = relations.ToList();
        m_RootIndices = m_ParentsMap
            .Where(i => i.Value == -1)
            .Select(i => i.Key).ToList();

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
            if (parentIdx == -1)
            {
                return -1;
            } else
            {
                return toLeafNodeIdx(childrenIndices[0]);
            }
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

        if (idx == -1 || m_Relations[idx].Children.Count != 0) return false;

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
        return new ForestEnumerator<T>(m_Items, m_ParentsMap, m_ChildrenMap);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// Enumerator that only enumerates leaf nodes
    public class ForestEnumerator<T> : IEnumerator, IEnumerator<T> where T : IEquatable<T>
    {
        private List<T> _items = new();
        private Dictionary<int, int> _parentMap;
        private Dictionary<int, List<int>> _childrenMap;
        private int _currentIndex
        {
            get => __currentIndex;
            set => __currentIndex = Math.Clamp(value, 0, (_items.Count == 0) ? 0 : _items.Count - 1);
        }
        private int __currentIndex;
        private bool _beforeEnumeration = true;

        public ForestEnumerator(List<T> items, Dictionary<int, int> parentMap, Dictionary<int, List<int>> childrenMap)
        {
            _items = items;
            _parentMap = parentMap;
            _childrenMap = childrenMap;
            _currentIndex = 0;
        }

        object IEnumerator.Current => (object)Current;
        public T Current => _items[_currentIndex];

        public bool MoveNext()
        {
            if (_beforeEnumeration)
            {
                _beforeEnumeration = false;
                _currentIndex = toLeafNode(0);
                return true;
            }

            if (_items.Count <= (_currentIndex - 1)) return false;

            if (TryGetSiblingNode(_currentIndex, out int sibling))
            {
                _currentIndex = toLeafNode(sibling);
                return true;
            }

            if (TryGetUncleNode(_currentIndex, out int uncleIdx))
            {
                _currentIndex = toLeafNode(uncleIdx);
                return true;
            }

            return false;
        }

        public void Reset() => _beforeEnumeration = true;

        private bool TryGetUncleNode(int beg, out int uncleIdx)
        {
            if (_parentMap.TryGetValue(beg, out var parent))
            {
                return TryGetSiblingNode(parent, out uncleIdx);
            }
            uncleIdx = -1;
            return false;
        }

        private bool TryGetSiblingNode(int beg, out int siblingIdx)
        {
            if (_parentMap.TryGetValue(beg, out var parent)
                && _childrenMap.TryGetValue(parent, out var siblings)
                && 1 < siblings.Count)
            {
                var youngerSiblings = siblings.SkipWhile(i => i != beg).Skip(1);
                if (1 <= youngerSiblings.Count())
                {
                    siblingIdx = youngerSiblings.ElementAt(0);
                    return true;
                }
            }

            siblingIdx = -1;
            return false;
        }
        private int toLeafNode(int beg)
        {
            if (_childrenMap.TryGetValue(beg, out var children) && 1 <= children.Count)
            {
                return toLeafNode(children[0]);
            } else
            {
                return beg;
            }
        }
        private bool isLeafNode(int i) => _childrenMap.TryGetValue(i, out var children) && children.Count == 0;

        public void Dispose() {}
    }
}
