using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RogueCore
{
    /// <summary>
    /// Provides methods for tracing geometric shapes and paths through a grid
    /// </summary>
    public class Tracer
    {
        /// <summary>
        /// Delegate for processing each point during a trace operation
        /// Returns negative value to stop tracing, 0 to continue normally, positive for other behaviors
        /// </summary>
        /// <param name="point">The current point being traced</param>
        /// <param name="context">User-provided context object</param>
        /// <returns>Negative to stop, 0 to continue, positive for other behavior</returns>
        public delegate int TraceDelegate(Point point, object context);

        /// <summary>
        /// Traces a line between two points using Bresenham's algorithm
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="end">Ending point</param>
        /// <param name="callback">Optional callback to process each point</param>
        /// <param name="context">Optional context object passed to callback</param>
        /// <returns>List of points that form the line</returns>
        public static List<Point> TraceLine(Point start, Point end, TraceDelegate callback = null, object context = null)
        {
            var visited = new List<Point>();
            var x0 = start.X;
            var y0 = start.Y;
            var x1 = end.X;
            var y1 = end.Y;

            // Special case: if start and end are the same point
            if (x0 == x1 && y0 == y1)
            {
                var point = new Point(x0, y0);
                visited.Add(point);

                if (callback != null)
                {
                    callback(point, context);
                }

                return visited;
            }

            // Bresenham's algorithm implementation
            var dx = Math.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = Math.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var error = (dx > dy ? dx : -dy) / 2;
            int error2;

            while (true)
            {
                var point = new Point(x0, y0);

                // Check if we've already visited this point to avoid duplicates
                if (!visited.Any(p => p.X == point.X && p.Y == point.Y))
                {
                    if (callback != null)
                    {
                        var result = callback(point, context);
                        if (result < 0)
                            break;
                    }

                    visited.Add(point);
                }

                if (x0 == x1 && y0 == y1)
                    break;

                error2 = error;
                if (error2 > -dx) { error -= dy; x0 += sx; }
                if (error2 < dy) { error += dx; y0 += sy; }
            }

            return visited;
        }

        /// <summary>
        /// Traces a circular shape by calculating points along the circumference
        /// </summary>
        /// <param name="center">Center of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="callback">Optional callback to process each point</param>
        /// <param name="context">Optional context object passed to callback</param>
        /// <returns>List of points that form the circle</returns>
        public static List<Point> TraceCircle(Point center, int radius, TraceDelegate callback = null, object context = null)
        {
            var visited = new List<Point>();

            if (radius <= 0)
                return visited;

            // Calculate number of points based on radius to ensure smooth circle
            var numPoints = radius * 12;
            var angleStep = 2.0 * Math.PI / numPoints;

            for (var t = 0; t < numPoints; t++)
            {
                var angle = t * angleStep;
                var x = (int)Math.Round(radius * Math.Cos(angle));
                var y = (int)Math.Round(radius * Math.Sin(angle));

                var point = new Point(center.X + x, center.Y + y);

                // Check if we've already visited this point to avoid duplicates
                if (!visited.Any(p => p.X == point.X && p.Y == point.Y))
                {
                    if (callback != null)
                    {
                        var result = callback(point, context);
                        if (result < 0)
                            break;
                    }

                    visited.Add(point);
                }
            }

            return visited;
        }

        /// <summary>
        /// Internal state class for Field of View calculations
        /// </summary>
        private class FovState
        {
            public List<Point> Visited { get; set; }
            public TraceDelegate OriginalCallback { get; set; }
            public object OriginalContext { get; set; }
        }

        /// <summary>
        /// Callback method used during FOV calculations to track visited points and call the original callback
        /// </summary>
        private static int FovCallback(Point point, object stateObj)
        {
            var state = (FovState)stateObj;

            // Check if we've already visited this point to avoid duplicates
            if (state.Visited.Any(p => p.X == point.X && p.Y == point.Y))
            {
                return 0;
            }

            // Call the original callback if provided
            if (state.OriginalCallback != null)
            {
                var result = state.OriginalCallback(point, state.OriginalContext);
                if (result < 0)
                    return result;
            }

            state.Visited.Add(point);
            return 0;
        }

        /// <summary>
        /// Traces a field of view from a central point using radial ray casting
        /// </summary>
        /// <param name="center">Center point for the FOV calculation</param>
        /// <param name="radius">Radius of the field of view</param>
        /// <param name="callback">Optional callback to process each point</param>
        /// <param name="context">Optional context object passed to callback</param>
        /// <returns>List of points within the field of view</returns>
        public static List<Point> TraceFov(Point center, int radius, TraceDelegate callback = null, object context = null)
        {
            var visited = new List<Point>();
            var state = new FovState
            {
                Visited = visited,
                OriginalCallback = callback,
                OriginalContext = context
            };

            // Process the center point first
            if (callback != null)
            {
                var result = callback(center, context);
                if (result < 0)
                    return visited;
            }

            visited.Add(center);

            // Cast rays in all directions to cover the entire field of view
            var numRays = radius * 24;
            var angleStep = 2.0 * Math.PI / numRays;

            for (var t = 0; t < numRays; t++)
            {
                var angle = t * angleStep;
                var x = (int)Math.Round(radius * Math.Cos(angle));
                var y = (int)Math.Round(radius * Math.Sin(angle));

                var endPoint = new Point(center.X + x, center.Y + y);

                // Trace a line from center to the edge point, collecting all intermediate points
                TraceLine(center, endPoint, FovCallback, state);
            }

            return visited;
        }

        /// <summary>
        /// Traces a path between two points using pathfinding algorithm
        /// </summary>
        /// <param name="map">The dungeon map to find path on</param>
        /// <param name="from">Starting point</param>
        /// <param name="to">Destination point</param>
        /// <param name="callback">Optional callback to process each point in the path</param>
        /// <param name="context">Optional context object passed to callback</param>
        /// <returns>List of points forming the path, or empty list if no path found</returns>
        public static List<Point> TracePath(Dungeon map, Point from, Point to, TraceDelegate callback = null, object context = null)
        {
            var path = FindPath(map, from, to);

            if (path == null)
                return new List<Point>();

            // Process each point in the path with the callback if provided
            foreach (var point in path)
            {
                if (callback != null)
                {
                    var result = callback(point, context);
                    if (result < 0)
                        break;
                }
            }

            return path;
        }

        #region Pathfinding Implementation (A* Algorithm)

        /// <summary>
        /// Node class used internally by the A* pathfinding algorithm
        /// </summary>
        private class PathNode
        {
            public Point Position { get; set; }
            public int PathLengthFromStart { get; set; }
            public PathNode CameFrom { get; set; }
            public int HeuristicEstimatePathLength { get; set; }
            
            public int EstimateFullPathLength => PathLengthFromStart + HeuristicEstimatePathLength;
        }

        /// <summary>
        /// Finds a path between two points using the A* algorithm
        /// </summary>
        /// <param name="field">The dungeon map</param>
        /// <param name="start">Starting point</param>
        /// <param name="goal">Destination point</param>
        /// <returns>List of points forming the path, or null if no path found</returns>
        private static List<Point> FindPath(Dungeon field, Point start, Point goal)
        {
            var closedSet = new List<PathNode>();
            var openSet = new List<PathNode>();

            var startNode = new PathNode
            {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Get the node with the lowest estimated full path length
                var currentNode = openSet.OrderBy(node => node.EstimateFullPathLength).First();

                // If we reached the goal, reconstruct the path
                if (currentNode.Position == goal)
                    return GetPathForNode(currentNode);

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Explore neighbors
                foreach (var neighborNode in GetNeighbours(currentNode, goal, field))
                {
                    // Skip if already in closed set
                    if (closedSet.Any(node => node.Position == neighborNode.Position))
                        continue;

                    var existingOpenNode = openSet.FirstOrDefault(node => node.Position == neighborNode.Position);

                    if (existingOpenNode == null)
                    {
                        // New node, add to open set
                        openSet.Add(neighborNode);
                    }
                    else if (existingOpenNode.PathLengthFromStart > neighborNode.PathLengthFromStart)
                    {
                        // Better path found, update the existing node
                        existingOpenNode.CameFrom = currentNode;
                        existingOpenNode.PathLengthFromStart = neighborNode.PathLengthFromStart;
                    }
                }
            }

            // No path found
            return null;
        }

        /// <summary>
        /// Calculates the distance between adjacent nodes (typically 1 for grid movement)
        /// </summary>
        private static int GetDistanceBetweenNeighbours()
        {
            return 1;
        }

        /// <summary>
        /// Calculates the heuristic estimate (Manhattan distance) between two points
        /// </summary>
        private static int GetHeuristicPathLength(Point from, Point to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }

        /// <summary>
        /// Gets valid neighboring nodes for the A* algorithm
        /// </summary>
        private static List<PathNode> GetNeighbours(PathNode pathNode, Point goal, Dungeon field)
        {
            var result = new List<PathNode>();
            var currentPos = pathNode.Position;

            // Define four possible directions (up, down, left, right)
            var neighborPositions = new[]
            {
                new Point(currentPos.X + 1, currentPos.Y),     // Right
                new Point(currentPos.X - 1, currentPos.Y),     // Left
                new Point(currentPos.X, currentPos.Y + 1),     // Down
                new Point(currentPos.X, currentPos.Y - 1)      // Up
            };

            foreach (var position in neighborPositions)
            {
                // Check boundaries
                if (position.X < 0 || position.X >= field.Width || 
                    position.Y < 0 || position.Y >= field.Height)
                    continue;

                // Check if the cell is passable (not solid)
                if (field.GetCell(position.X, position.Y).Solid)
                    continue;

                var neighborNode = new PathNode
                {
                    Position = position,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart + GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(position, goal)
                };

                result.Add(neighborNode);
            }

            return result;
        }

        /// <summary>
        /// Reconstructs the path from the goal node back to the start
        /// </summary>
        private static List<Point> GetPathForNode(PathNode goalNode)
        {
            var result = new List<Point>();
            var currentNode = goalNode;

            while (currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }

            result.Reverse();
            return result;
        }

        #endregion
    }
}
