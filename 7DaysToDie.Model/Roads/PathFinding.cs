using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model;

namespace _7DaysToDie.Roads
{
    static class PathFinding
    {
        public static Path<Node> FindPathForIHasNeighbours<Node>(
            Node start,
            Node destination,
            Func<Node, Node, double> distance,
            Func<Node, double> estimate)
            where Node : IHasNeighbours<Node>
        {
            var closed = new HashSet<Node>();
            var queue = new PriorityQueue<double, Path<Node>>();
            queue.Enqueue(0, new Path<Node>(start));
            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();
                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;
                closed.Add(path.LastStep);
                foreach (Node n in path.LastStep.Neighbours)
                {
                    double d = distance(path.LastStep, n);
                    var newPath = path.AddStep(n, d);
                    queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                }
            }
            return null;
        }

        public static Path<Node> FindPath<Node>(
            Node start,
            Node destination,
            Func<Node,IEnumerable<Node>> getNeighbours,
            Func<Node, Node, double> distance,
            Func<Node, Node, double> estimate)
        {
            var closed = new HashSet<Node>();
            var queue = new PriorityQueue<double, Path<Node>>();
            queue.Enqueue(0, new Path<Node>(start));
            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();
                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;
                closed.Add(path.LastStep);
                foreach (Node n in getNeighbours(path.LastStep))
                {
                    double d = distance(path.LastStep, n);
                    var newPath = path.AddStep(n, d);
                    queue.Enqueue(newPath.TotalCost + estimate(n,destination), newPath);
                }
            }
            return null;
        }
    }
}
