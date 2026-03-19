using NUnit.Framework;

[TestFixture]
public class BreadcrumbForestTests
{
    [Test]
    public void FromItems_CreatesSingleRootItem_ReturnsForestWithOneItem()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root")
        );

        Assert.That(forest.Current, Is.EqualTo("Root"));
        Assert.That(forest.Children, Is.Empty);
    }

    [Test]
    public void FromItems_CreatesRootWithChildren_ReturnsForestWithCorrectStructure()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Child1"),
                item("Child2")
            )
        );

        Assert.That(forest.Current, Is.EqualTo("Child1"));
        Assert.That(forest.Children, Is.Empty);
    }

    [Test]
    public void SetCurrent_WithExistingItem_ReturnsTrueAndUpdatesCurrent()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Child1"),
                item("Child2")
            )
        );

        bool result = forest.SetCurrent("Child1");

        Assert.That(result, Is.True);
        Assert.That(forest.Current, Is.EqualTo("Child1"));
    }

    [Test]
    public void SetCurrent_WithNonExistingItem_ReturnsFalseAndKeepsCurrent()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Child1")
            )
        );

        string originalCurrent = forest.Current;
        bool result = forest.SetCurrent("NonExisting");

        Assert.That(result, Is.False);
        Assert.That(forest.Current, Is.EqualTo(originalCurrent));
    }

    [Test]
    public void SetCurrent_WithNonLeafItem_ReturnsFalseAndKeepsCurrent()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Child1")
            )
        );

        string originalCurrent = forest.Current;
        bool result = forest.SetCurrent("Root");

        Assert.That(result, Is.False);
        Assert.That(forest.Current, Is.EqualTo(originalCurrent));
    }

    [Test]
    public void Children_WhenCurrentHasNoChildren_ReturnsEmptyList()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Leaf")
            )
        );

        forest.SetCurrent("Leaf");

        Assert.That(forest.Children, Is.Empty);
    }

    [Test]
    public void FromItems_WithIntegerType_WorksCorrectly()
    {
        var forest = BreadcrumbForest<int>.FromItems(item =>
            item(1).WithChildren(
                item(2),
                item(3)
            )
        );

        Assert.That(forest.Current, Is.EqualTo(2));
        Assert.That(forest.Children, Is.Empty);
    }

    [Test]
    public void FromItems_WithMultipleRoots_HandlesCorrectly()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root1").WithChildren(
                item("Child1")
            )
        );

        Assert.That(forest.Current, Is.EqualTo("Child1"));
        Assert.That(forest.Children, Is.Empty);
    }

    [Test]
    public void Current_AlwaysReturnsValidItem()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("OnlyItem")
        );

        Assert.That(forest.Current, Is.Not.Null);
        Assert.That(forest.Current, Is.EqualTo("OnlyItem"));
    }

    [Test]
    public void Next_OnSingleLeafNode_ReturnsFalse()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("OnlyLeaf")
        );

        bool result = forest.Next();

        Assert.That(result, Is.False);
        Assert.That(forest.Current, Is.EqualTo("OnlyLeaf"));
    }

    [Test]
    public void Next_OnFirstChildWithSiblings_MovesToNextSibling()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Parent").WithChildren(
                item("Child1"),
                item("Child2"),
                item("Child3")
            )
        );

        forest.SetCurrent("Child1");
        bool result = forest.Next();

        Assert.That(result, Is.True);
        Assert.That(forest.Current, Is.EqualTo("Child2"));
    }

    [Test]
    public void Next_OnLastSiblingLeaf_MovesToParentNextSibling()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Parent1").WithChildren(
                    item("Child1"),
                    item("Child2")
                ),
                item("Parent2")
            )
        );

        forest.SetCurrent("Child2");
        bool result = forest.Next();

        Assert.That(result, Is.True);
        Assert.That(forest.Current, Is.EqualTo("Parent2"));
    }

    [Test]
    public void Next_CompleteTreeTraversal_VisitsAllNodesInOrder()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("A").WithChildren(
                    item("A1"),
                    item("A2")
                ),
                item("B").WithChildren(
                    item("B1")
                ),
                item("C")
            )
        );

        List<string> visitedNodes = [forest.Current];
        
        while (forest.Next())
        {
            visitedNodes.Add(forest.Current);
        }

        Assert.That(visitedNodes, Is.EqualTo(new[] { "A1", "A2", "B1", "C" }));
    }

    [Test]
    public void Next_AtLastNodeInTree_ReturnsFalse()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Child1"),
                item("Child2")
            )
        );

        forest.SetCurrent("Child2");
        bool result = forest.Next();

        Assert.That(result, Is.False);
        Assert.That(forest.Current, Is.EqualTo("Child2"));
    }

    [Test]
    public void IEnumerable_OnNestedStructure_TraversesDepthFirst()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("1").WithChildren(
                item("1.1").WithChildren(
                    item("1.1.1"),
                    item("1.1.2")
                ),
                item("1.2")
            )
        );

        List<string> traversalOrder = forest.ToList();
        Assert.That(traversalOrder, Is.EqualTo(new[] { "1.1.1", "1.1.2", "1.2" }));
    }

    [Test]
    public void IEnumerable_WithMultipleSiblingBranches_TraversesCorrectly()
    {
        var forest = BreadcrumbForest<string>.FromItems(item =>
            item("Root").WithChildren(
                item("Branch1").WithChildren(
                    item("Leaf1")
                ),
                item("Branch2").WithChildren(
                    item("Leaf2"),
                    item("Leaf3")
                ),
                item("Branch3")
            )
        );

        List<string> traversalOrder = forest.ToList();
        Assert.That(traversalOrder, Is.EqualTo(new[] { "Leaf1", "Leaf2", "Leaf3", "Branch3" }));
    }
}
