using System; 
using System.Collections.Generic;
using System.IO;

namespace Travelling;
public class CityGraph
{
    private Dictionary<string, List<(string, int)>> adjacencyList;
    private List<(string from, string to, int distance)> edges;

    private CityGraph()
    {
        adjacencyList = new Dictionary<string, List<(string, int)>>();
        edges = new List<(string, string, int)>();
    }
    public static CityGraph LoadFromFile(string filePath)
    {
        CityGraph cityGraph = new CityGraph();

        string[] filelines = File.ReadAllLines(filePath);
        foreach (string? line in filelines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            string[] Line = line.Split(',');
            string[] City = Line[0].Split('-');
            string cityA = City[0];
            string cityB = City[1];
            if (!int.TryParse(Line[1], out int distance))
                continue;

            if (!cityGraph.adjacencyList.ContainsKey(cityA))
                cityGraph.adjacencyList[cityA] = new List<(string, int)>();
            if (!cityGraph.adjacencyList.ContainsKey(cityB))
                cityGraph.adjacencyList[cityB] = new List<(string, int)>();

            cityGraph.adjacencyList[cityA].Add((cityB, distance));
            cityGraph.adjacencyList[cityB].Add((cityA, distance));

            cityGraph.edges.Add((cityA, cityB, distance));
            cityGraph.edges.Add((cityB, cityA, distance));

        }
        return cityGraph;
    }

    public int GetPathDistance(List<string> path)
    {
        int sum = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (!adjacencyList.TryGetValue(path[i], out var neighbors))
                continue;

            foreach (var (neighbor, distance) in adjacencyList[path[i]])
            {
                if (neighbor == path[i + 1]) { 
                sum = sum + distance;
                break;
                } 
            }
        }
        return sum;
    }

    public List<string> FindShortestPath(string from, string to)
    {
        var distances = new Dictionary<string, int>();
        var previous = new Dictionary<string, string?>();
        var visited = new HashSet<string>();
        var queue = new PriorityQueue<string, int>();

        foreach (var city in adjacencyList.Keys)
        {
            distances[city] = int.MaxValue;
            previous[city] = null;
        }

        if (!adjacencyList.ContainsKey(from) || !adjacencyList.ContainsKey(to))
            return new List<string>();

        distances[from] = 0;
        queue.Enqueue(from, 0);

        while (queue.Count > 0)
        {
            queue.TryDequeue(out string? current, out int currentDist);

            if (current == null)
                continue;

            if (visited.Contains(current))
                continue;
            visited.Add(current);

            if (current == to)
                break;

            foreach (var (neighbor, distance) in adjacencyList[current])
            {
                int newDist = currentDist + distance;
                if (newDist < distances[neighbor])
                {
                    distances[neighbor] = newDist;
                    previous[neighbor] = current;
                    queue.Enqueue(neighbor, newDist);
                }
            }
        }

        var path = new List<string>();
        string? step = to;

        while (step != null)
        {
            path.Add(step);
            step = previous[step];
        }

        path.Reverse();

        if (path[0] != from)
            return new List<string>();
        return path;
    }


     public override string ToString()
    {
        var lines = new List<string>();
        foreach (var e in edges)
            lines.Add($"{e.from}-{e.to},{e.distance}");
        return string.Join(Environment.NewLine, lines);
    }
}
