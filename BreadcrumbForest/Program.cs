var tree = BreadcrumbForest<string>.FromItems(item => 
                                          item("root").WithChildren(
                                              item("root.0"),
                                              item("root.1").WithChildren(
                                                  item("root.1.0"),
                                                  item("root.1.1"),
                                                  item("root.1.2")
                                              ),
                                              item("root.2").WithChildren(
                                                  item("root.2.0"),
                                                  item("root.2.1").WithChildren(
                                                      item("root.2.1.0"),
                                                      item("root.2.1.1")
                                                  ),
                                                  item("root.2.2")
                                              )));

Console.WriteLine($"tree.Current == root: {tree.Current == "root"}");
Console.WriteLine($"tree.Current: {tree.Current}");
var prev = DateTime.Now;
Console.WriteLine($"children == root.0, root.1: {tree.Children == new List<string>(){"root.0", "root.1"}}");
Console.WriteLine($"children: {tree.Children}");
Console.WriteLine($"Elapsed: {DateTime.Now - prev}");

// Test IEnumerable<T> implementation
Console.WriteLine("Testing IEnumerable<T>:");
foreach (var item in tree)
{
    Console.WriteLine($"Item: {item}");
}

if (tree.TryGetSubTree("root.1", out var subtree))
{
    Console.WriteLine("Subtree of root.2");
    foreach (var item in subtree)
    {
        Console.WriteLine($"Item: {item}");
    }
}

// Test LINQ methods (which depend on IEnumerable<T>)
var allItems = tree.ToList();
Console.WriteLine($"Total items: {allItems.Count}");
