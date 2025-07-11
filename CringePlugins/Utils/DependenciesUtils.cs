namespace CringePlugins.Utils;

internal static class DependenciesUtils
{
    public static List<Dictionary<T, HashSet<T>>> SplitIntoSubGraphs<T>(Dictionary<T, HashSet<T>> map) where T : notnull
    {
        var undirected = new Dictionary<T, HashSet<T>>();

        foreach (var entry in map)
        {
            if (!undirected.ContainsKey(entry.Key))
                undirected[entry.Key] = [];

            foreach (var dep in entry.Value)
            {
                if (!undirected.ContainsKey(dep))
                    undirected[dep] = [];

                undirected[entry.Key].Add(dep);
                undirected[dep].Add(entry.Key);
            }
        }

        foreach (var key in map.Keys)
        {
            if (!undirected.ContainsKey(key))
                undirected[key] = [];
        }

        var visited = new HashSet<T>();
        var components = new List<HashSet<T>>();

        var queue = new Queue<T>();
        foreach (var node in undirected.Keys)
        {
            if (visited.Contains(node)) continue;
            
            var component = new HashSet<T>();
            queue.Enqueue(node);

            while (queue.TryDequeue(out var current))
            {
                if (visited.Add(current))
                {
                    component.Add(current);
                    foreach (var neighbor in undirected[current])
                    {
                        if (!visited.Contains(neighbor))
                            queue.Enqueue(neighbor);
                    }
                }
            }

            components.Add(component);
        }

        return components.Select(b => b.ToDictionary(k => k, v => map[v])).ToList();
    }
}