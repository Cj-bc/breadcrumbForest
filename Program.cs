var tree = RelationTree<string>.FromItems(item => 
                                          item("root").WithChildren(
                                              item("root.0"),
                                              item("root.1").WithChildren(
                                                  item("root.1.0"),
                                                  item("root.1.2")
                                              )));

Console.WriteLine(tree.Current == "root");
var prev = DateTime.Now;
Console.WriteLine(tree.Children == new List<string>(){"root.0", "root.1"});
Console.WriteLine($"Elapsed: {DateTime.Now - prev}");
