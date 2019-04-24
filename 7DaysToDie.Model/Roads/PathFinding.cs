using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using _7DaysToDie.Contracts;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Roads
{
    static class PathFinding
    {
        public static List<Vector2<int>> TwelveRadius16Points = new List<Vector2<int>>()
        {
            { new Vector2<int>(0,16) },
            { new Vector2<int>(8,14) },
            { new Vector2<int>(14,8) },
            { new Vector2<int>(16,0) },
            { new Vector2<int>(14,-8) },
            { new Vector2<int>(8,-14) },
            { new Vector2<int>(0,-16) },
            { new Vector2<int>(-8,-14) },
            { new Vector2<int>(-14,-8) },
            { new Vector2<int>(-16,0) },
            { new Vector2<int>(-14,8) },
            { new Vector2<int>(-8,14) },
            { new Vector2<int>(0,16) }
        };

        public static List<Vector2<int>> TwelveRadius4Points = new List<Vector2<int>>()
        {
            {new Vector2<int>(-2, 3)},
            {new Vector2<int>(0, 4)},
            {new Vector2<int>(2, 3)},
            {new Vector2<int>(3, 2)},
            {new Vector2<int>(4, 0)},
            {new Vector2<int>(3, -2)},
            {new Vector2<int>(2, -3)},
            {new Vector2<int>(0, -4)},
            {new Vector2<int>(-2, -3)},
            {new Vector2<int>(-3, -2)},
            {new Vector2<int>(-4, 0)},
            {new Vector2<int>(-3, 2)}
        };

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

        public static Path<RoadCell> AStarPathFindingUsingPriorityQueue<RoadCell>(
            RoadCell start,
            RoadCell destination,
            Func<RoadCell, IEnumerable<RoadCell>> getNeighbours,
            Func<RoadCell, RoadCell, double> distance,
            Func<RoadCell, RoadCell, double> estimate)
        {
            var closed = new HashSet<RoadCell>();
            var queue = new PriorityQueue<double, Path<RoadCell>>();
            queue.Enqueue(0, new Path<RoadCell>(start));
            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();
                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;
                closed.Add(path.LastStep);
                foreach (RoadCell n in getNeighbours(path.LastStep))
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
