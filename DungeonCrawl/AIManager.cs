
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;
using System.Diagnostics;

public class PathWaypoint
{
    public Vector2 position { get; private set; }
    public PathWaypoint nextWaypoint { get; private set; }
    public PathType pathType { get; private set; }

    public PathWaypoint(Vector2 position, PathWaypoint nextWaypoint, PathType pathType)
    {
        this.position = position;
        this.nextWaypoint = nextWaypoint;
        this.pathType = pathType;
    }
}

public class AIManager
{
    private class ConnectivityNodeWrapper
    {
        public ConnectivityNode node;
        public ConnectivityNodeWrapper parent;
        public PathType pathType;
        public float totalTimeCost;
        public float heurisitc;

        public ConnectivityNodeWrapper(ConnectivityNode node, float heuristic)
        {
            this.node = node;
            parent = null;
            this.heurisitc = heuristic;
            pathType = PathType.Free;
        }

        public ConnectivityNodeWrapper(ConnectivityNode node, float timeCost, float heuristic, ConnectivityNodeWrapper parent, PathType pathType)
        {
            this.node = node;
            this.parent = parent;
            totalTimeCost = parent.totalTimeCost + timeCost;
            this.heurisitc = totalTimeCost + heuristic;
            this.pathType = pathType;
        }

        public static bool operator ==(ConnectivityNodeWrapper n1, ConnectivityNodeWrapper n2)
        {
            return n1.node.position == n2.node.position;
        }

        public static bool operator !=(ConnectivityNodeWrapper n1, ConnectivityNodeWrapper n2)
        {
            return n1.node.position != n2.node.position;
        }

        public static bool operator ==(ConnectivityNodeWrapper n1, ConnectivityNode n2)
        {
            return n1.node.position == n2.position;
        }

        public static bool operator !=(ConnectivityNodeWrapper n1, ConnectivityNode n2)
        {
            return n1.node.position != n2.position;
        }

        public static bool operator ==(ConnectivityNode n1, ConnectivityNodeWrapper n2)
        {
            return n1.position == n2.node.position;
        }

        public static bool operator !=(ConnectivityNode n1, ConnectivityNodeWrapper n2)
        {
            return n1.position != n2.node.position;
        }
    }

    public class ConnectivityNode
    {
        public Vector2 position;
        public List<ConnectivityPath> connections = new List<ConnectivityPath>();

        public ConnectivityNode(Vector2 pos)
        {
            position = pos;
        }

    }

    public class ConnectivityPath
    {
        public ConnectivityNode startNode;
        public ConnectivityNode endNode;
        public float[] jumpCosts;
        public bool[] jumpableStrengths;
        public bool[] largeCharacterJumpable;
        public bool straightPath = false;
        public bool walkable = false;
        public bool largeCharacterStraightPath = false;
        public bool jumpPath = false;

        public ConnectivityPath(ConnectivityNode n1, ConnectivityNode n2, bool[] jumpableStrengths, bool[] largeCharacterJumpable, bool largeCharacterStraightPath, bool straightPath, bool walkable, bool jumpPath, float[] jumpCosts)
        {
            startNode = n1;
            endNode = n2;
            this.jumpableStrengths = jumpableStrengths;
            this.largeCharacterJumpable = largeCharacterJumpable;
            this.straightPath = straightPath;
            this.walkable = walkable;
            this.largeCharacterStraightPath = largeCharacterStraightPath;
            this.jumpPath = jumpPath;
            this.jumpCosts = jumpCosts;
        }

        public float getStraightPathCost(int xVel, float gravity)
        {
            Vector2 distance = new Vector2(Math.Abs(endNode.position.X - startNode.position.X), Math.Abs(endNode.position.Y - startNode.position.Y));
            float fallTime = (float)Math.Sqrt(distance.Y * 2 / gravity);
            float xTime = distance.X / xVel;
            return (fallTime > xTime) ? fallTime : xTime;
        }

        public int minXVel(int jumpStrength, float gravity)
        {
            // dx = t * Vx
            // t = tup + tdown
            // tup = J / g
            // tdown = sqrt(2 * d / a)
            // tdown = sqrt(2 * (|dactual| + |djump|) / g)
            // tdown = sqrt(2 * (|J^2 / g / 2| + |dy|) / g)
            // tdown = sqrt((|J^2 / g| + 2 * |dy|) / g)
            // Vxmin = dx / (tup + tdown)
            // Vxmin = dx / (J / g + sqrt((|J^2 / g| - 2 * |dy|) / g)
            Vector2 distance = new Vector2(Math.Abs(endNode.position.X - startNode.position.X), endNode.position.Y - startNode.position.Y);
            float x = (float)Math.Abs(distance.X / (jumpStrength / gravity + Math.Sqrt((jumpStrength * jumpStrength / gravity + 2 * distance.Y) / gravity)));
            return (int)Math.Abs(distance.X / (jumpStrength / gravity + Math.Sqrt((jumpStrength * jumpStrength / gravity + 2 * distance.Y) / gravity))) + 1;
        }
    }

    public SortedList<int, SortedList<int, ConnectivityNode>> connectivityGraph = new SortedList<int, SortedList<int, ConnectivityNode>>();
    private const int MAX_NODE_SEARCH_RANGE = 700;
    private const int MIN_SEARCH_INTERVAL = 30;
    private const int idleTime = 75;
    private List<Enemy> enemies;

    public AIManager(SortedVector2List<Platform> platforms, float gravity, List<Enemy> enemies)
    {
        this.enemies = enemies;

        SortedVector2List<bool> platformAdded = new SortedVector2List<bool>();

        foreach(Enemy e in enemies)
        {
            platforms.ForAllInRange(e.ai.boundingBox.Center.ToVector2(), e.ai.boundingBox.Width / 2 + Level.MAX_PLATFORM_BOUND, 
                delegate(Platform p) {
                    if (!platformAdded.Exists(p.Position))
                    {
                        platformAdded.Add(p.Position, true);
                        List<Vector2> positions = p.getConnectivityNodeCoordinates();
                        for (int i = 0; i < positions.Count; ++i)
                        {
                            ConnectivityNode node = new ConnectivityNode(positions[i] - new Vector2((i == 0) ? 1 : (i == positions.Count - 1) ? -1 : 0, 1));
                            if (e.ai.inBoundingBox(node.position))
                            {
                                SortedList<int, ConnectivityNode> nodeYList;
                                if (connectivityGraph.TryGetValue((int)node.position.X, out nodeYList))
                                    nodeYList.Add((int)node.position.Y, node);
                                else
                                    connectivityGraph.Add((int)node.position.X, new SortedList<int, ConnectivityNode> { { (int)node.position.Y, node } });
                            }
                        }

                    }
                    return true;
                });
        }

        /*foreach (SortedList<int, Platform> list in platforms.Values)
            foreach (Platform p in list.Values)
            {
                List<Vector2> positions = p.getConnectivityNodeCoordinates();
                for (int i = 0; i < positions.Count; ++i)
                {
                    ConnectivityNode node = new ConnectivityNode(positions[i] - new Vector2((i == 0) ? 1 : (i == positions.Count - 1) ? -1 : 0, 1));
                    SortedList<int, ConnectivityNode> nodeYList;
                    if (connectivityGraph.TryGetValue((int)node.position.X, out nodeYList))
                        nodeYList.Add((int)node.position.Y, node);
                    else
                        connectivityGraph.Add((int)node.position.X, new SortedList<int, ConnectivityNode> { { (int)node.position.Y, node } });
                }
            }*/

        for(int i = 0; i < connectivityGraph.Count; ++i)
            for(int ii = 0; ii < connectivityGraph.Values[i].Count; ++ii)
                for(int j = i; j < connectivityGraph.Count; ++j)
                    for(int jj = (j == i) ? ii + 1 : 0; jj < connectivityGraph.Values[j].Count; ++jj)
                    {
                        if (distance(connectivityGraph.Values[i].Values[ii], connectivityGraph.Values[j].Values[jj]) > MAX_NODE_SEARCH_RANGE)
                            continue;

                        ConnectivityPath connectivityPath = generateConnectivityPath(connectivityGraph.Values[i].Values[ii], connectivityGraph.Values[j].Values[jj], platforms, gravity);
                        if (connectivityPath != null)
                            connectivityGraph.Values[i].Values[ii].connections.Add(connectivityPath);
                        connectivityPath = generateConnectivityPath(connectivityGraph.Values[j].Values[jj], connectivityGraph.Values[i].Values[ii], platforms, gravity);
                        if (connectivityPath != null)
                            connectivityGraph.Values[j].Values[jj].connections.Add(connectivityPath);
                    }
    }

    public void updateAIPaths(SortedVector2List<Platform> platforms, Hero hero, float gravity)
    {
        foreach (Enemy enemy in enemies)
        {
            // If enemy has no path or has a path that is not mandatory
            if (enemy.ai.state != AIState.Correctional)
                // If hero is within agro range and within bounding box, otherwise idle
                if (distance(hero.Position, enemy.Position) < enemy.maxAgroRange && enemy.ai.inBoundingBox(hero.Position))
                {
                    // If hero is within chase range and not blocked by a platform, otherwise if enemyy needs a new path
                    if (distance(hero.Position, enemy.Position) < 150 && !platformsCollissionRay(platforms, enemy.Position, hero.Position))
                    {
                        enemy.ai.clearPath();
                        enemy.ai.setPath(null, AIState.Chasing, hero);
                    }
                    else if (enemy.ai.timeSincePathUpdate > MIN_SEARCH_INTERVAL + distance(hero.Position, enemy.Position) / 20 && enemy.ai.newPathFlag)
                    {
                        bool noPath = false;
                        // If the target location is within an accessible area in the bounding box
                        if (canStayInBounds(platforms, enemy, gravity, new Vector2(hero.Position.X, hero.Position.Y + (enemy.Position.Y - enemy.getBot())), new Vector2(hero.Position.X, hero.Position.Y + (enemy.Position.Y - enemy.getBot()))))
                        {
                            PathWaypoint path = getPath(platforms, enemy, hero, (int)enemy.calculateStat(StatTypes.Jump), (int)enemy.calculateStat(StatTypes.Movement), gravity, enemy.ai.currentWaypoint);
                            if (path != null)
                                enemy.ai.setPath(path, AIState.Chasing, hero);
                            else
                                noPath = true;
                        }
                        else
                            noPath = true;
                        if (noPath && (enemy.ai.newPathFlag && enemy.ai.timeSincePathUpdate > idleTime || enemy.ai.state != AIState.Idling))
                            enemy.ai.setPath(generateIdlePath(platforms, enemy, enemy.ai, (int)enemy.calculateStat(StatTypes.Jump), (int)enemy.calculateStat(StatTypes.Movement), gravity, enemy.ai.currentWaypoint), AIState.Idling, null);
                    }
                }
                else if ((enemy.ai.newPathFlag && enemy.ai.timeSincePathUpdate > idleTime) || enemy.ai.state != AIState.Idling)
                    enemy.ai.setPath(
                        generateIdlePath(platforms, enemy, enemy.ai, (int)enemy.calculateStat(StatTypes.Jump), (int)enemy.calculateStat(StatTypes.Movement), gravity, enemy.ai.currentWaypoint), 
                        (enemy.ai.state == AIState.Tired) ? AIState.Tired : AIState.Idling);
            enemy.ai.moveBody();
            // If the enemy's next movement will still allow it to stay in the bounding box
            if (enemy.ai.targetObj != null && enemy.ai.path == null)
            {
                if (!canStayInBounds(platforms, enemy, gravity, new Vector2(enemy.Position.X, enemy.getBot()), new Vector2(enemy.Position.X, enemy.getBot()) + enemy.Velocity))
                {
                    enemy.ai.setPath(new PathWaypoint(getClosestNodeKeyForPosition(platforms, enemy, enemy.Position, new Vector2(enemy.Position.X, enemy.getBot()), gravity, (int)enemy.calculateStat(StatTypes.Movement))[0], null, PathType.Free), 
                        AIState.Correctional);
                    enemy.ai.moveBody();
                }
            }
        }
    }

    private bool canStayInBounds(SortedVector2List<Platform> platforms, Enemy enemy, float gravity, Vector2 curPostion, Vector2 nextPosition)
    {
        if (platformsCollissionRay(platforms, curPostion, new Vector2(curPostion.X, enemy.ai.boundingBoxBottom())))
            return true;
        List<Vector2> nodeKeys = getClosestNodeKeyForPosition(platforms, enemy, nextPosition, nextPosition, gravity, (int)enemy.calculateStat(StatTypes.Movement));
        if (nodeKeys != null)
            foreach (Vector2 vec in nodeKeys)
                if (enemy.ai.inBoundingBox(vec))
                    return true;
        return false;
    }

    private PathWaypoint getPath(SortedVector2List<Platform> platforms, CollidableObj start, CollidableObj end, int jumpStrength, int xVelocity, float gravity, PathWaypoint curWapoint = null)
    {

        Dictionary<Vector2, ConnectivityNodeWrapper> exploredSet = new Dictionary<Vector2, ConnectivityNodeWrapper>();
        Dictionary<Vector2, ConnectivityNodeWrapper> openSet = new Dictionary<Vector2, ConnectivityNodeWrapper>();

        MinHeap<ConnectivityNodeWrapper> queue = new MinHeap<ConnectivityNodeWrapper>(delegate (ConnectivityNodeWrapper n1, ConnectivityNodeWrapper n2)
        {
            if (n1.heurisitc < n2.heurisitc)
                return -1;
            else if (n1.heurisitc == n2.heurisitc)
                return 0;
            else
                return 1;
        });

        List<Vector2> res;
        ConnectivityNodeWrapper curNode;

        if (curWapoint != null && !(connectivityGraph.ContainsKey((int)curWapoint.position.X) && connectivityGraph[(int)curWapoint.position.X].ContainsKey((int)curWapoint.position.Y)))
            curWapoint = null;
        if (curWapoint != null)
            curNode = new ConnectivityNodeWrapper(connectivityGraph[(int)curWapoint.position.X][(int)curWapoint.position.Y], calculateHeuristic(curWapoint.position, end.Position, xVelocity, jumpStrength));
        else
        {
            res = getClosestNodeKeyForPosition(platforms, start, start.Position, end.Position, gravity, xVelocity);
            if (res == null)
                return null;
            foreach (Vector2 vec in res)
            {
                ConnectivityNodeWrapper cnw = new ConnectivityNodeWrapper(connectivityGraph[(int)vec.X][(int)vec.Y], calculateHeuristic(vec, end.Position, xVelocity, jumpStrength));
                queue.add(cnw);
                openSet.Add(cnw.node.position, cnw);
            }
            curNode = queue.peek();
            queue.pop();
            openSet.Remove(curNode.node.position);
        }


        res = getClosestNodeKeyForPosition(platforms, end, end.Position, end.Position, gravity, xVelocity, jumpStrength);
        if (res == null)
            return null;
        ConnectivityNode endNode = connectivityGraph[(int)res[0].X][(int)res[0].Y];

        int it = 0;

        while (curNode != endNode)
        {
            foreach(ConnectivityPath path in curNode.node.connections)
            {
                if (!exploredSet.ContainsKey(path.endNode.position))
                {
                    bool jumpPath = path.jumpPath && path.jumpableStrengths[jumpStrength] && path.minXVel(jumpStrength, gravity) <= xVelocity;
                    bool straightPath = path.straightPath && (path.minXVel(0, gravity) <= xVelocity || path.walkable);
                    if (straightPath)
                    {
                        float h = calculateHeuristic(path.endNode.position, endNode.position, xVelocity, jumpStrength);
                        if (!openSet.ContainsKey(path.endNode.position))
                        {
                            ConnectivityNodeWrapper cnw = new ConnectivityNodeWrapper(path.endNode, path.getStraightPathCost(xVelocity, gravity), h, curNode, PathType.Straight);
                            queue.add(cnw);
                            openSet.Add(cnw.node.position, cnw);
                        } else if(openSet[path.endNode.position].heurisitc > h)
                        {
                            ConnectivityNodeWrapper cnw = new ConnectivityNodeWrapper(path.endNode, path.getStraightPathCost(xVelocity, gravity), h, curNode, PathType.Straight);
                            queue.add(cnw);
                            openSet[path.endNode.position] = cnw;
                        }
                    }
                    else if (jumpPath)
                        queue.add(new ConnectivityNodeWrapper(path.endNode, path.jumpCosts[jumpStrength], calculateHeuristic(path.endNode.position, endNode.position, xVelocity, jumpStrength), curNode, PathType.Jump));
                }
            }
            exploredSet.Add(curNode.node.position, curNode);

            ++it;
            if (it == 20)
                return null;

            do
            {
                if (queue.empty())
                    return null;
                curNode = queue.peek();
                queue.pop();
            } while (exploredSet.ContainsKey(curNode.node.position));
        }

        PathWaypoint pathWaypoint = new PathWaypoint(new Vector2(end.Position.X, end.getBot()), null, PathType.Free);
        while (curNode != (object)null) 
        {
            pathWaypoint = new PathWaypoint(curNode.node.position, pathWaypoint, curNode.pathType);
            curNode = curNode.parent;
        }
        if (curWapoint != null)
            pathWaypoint = pathWaypoint.nextWaypoint;
        return pathWaypoint;
    }

    private PathWaypoint generateIdlePath(SortedVector2List<Platform> platforms, CollidableObj obj, AI ai, int jumpStrength, int xVelocity, float gravity, PathWaypoint curWapoint = null)
    {
        ConnectivityNode curNode = null;
        if (curWapoint != null && !(connectivityGraph.ContainsKey((int)curWapoint.position.X) && connectivityGraph[(int)curWapoint.position.X].ContainsKey((int)curWapoint.position.Y)))
            curWapoint = null;
        if (curWapoint != null)
            curNode = connectivityGraph[(int)curWapoint.position.X][(int)curWapoint.position.Y];

        if (curNode == null)
        {
            List<Vector2> res = getClosestNodeKeyForPosition(platforms, obj, obj.Position, obj.Position, gravity, xVelocity, jumpStrength);
            if (res == null)
                return null;
            foreach (Vector2 v in res)
                if (ai.inBoundingBox(v))
                    return new PathWaypoint(v, null, PathType.Free);
        }

        List<PathWaypoint> waypoints = new List<PathWaypoint> { new PathWaypoint(new Vector2(obj.Position.X, obj.getBot()), null, PathType.Free) };

        if (curNode != null)
            foreach (ConnectivityPath path in curNode.connections)
            {
                if (ai.inBoundingBox(path.endNode.position))
                {
                    bool jumpPath = path.jumpPath && path.jumpableStrengths[jumpStrength] && path.minXVel(jumpStrength, 0.15f) <= xVelocity;
                    bool straightPath = path.straightPath && path.minXVel(0, 0.15f) <= xVelocity;
                    if (straightPath)
                        waypoints.Add(new PathWaypoint(path.endNode.position, null, PathType.Straight));
                    if (jumpPath)
                        waypoints.Add(new PathWaypoint(path.endNode.position, null, PathType.Jump));
                }
            }

        return waypoints[new Random().Next(waypoints.Count)];
    }

    private List<Vector2> getClosestNodeKeyForPosition(SortedVector2List<Platform> platforms, CollidableObj obj, Vector2 pos, Vector2 goal, float gravity, int xVelocity, int? jumpStrength = null)
    {
        if (connectivityGraph.Count == 0)
            return null;

        int maxSearchRange = MAX_NODE_SEARCH_RANGE;
        if (jumpStrength != null)
            maxSearchRange = (int)(Math.Pow((int)jumpStrength, 2) / gravity) / 2;

        Vector2 ret = new Vector2(0, 1);
        int begin = 0;
        int end = connectivityGraph.Count;
        while (!(pos.X <= connectivityGraph.Keys[begin + (end - begin) / 2] && pos.X >= connectivityGraph.Keys[begin + (end - begin) / 2 - 1]
            || pos.X >= connectivityGraph.Keys[begin + (end - begin) / 2] && pos.X <= connectivityGraph.Keys[begin + (end - begin) / 2 + 1]))
            if (pos.X > connectivityGraph.Keys[begin + (end - begin) / 2])
                begin = begin + (end - begin) / 2 + 1;
            else
                end = begin + (end - begin) / 2;

        int i;
        if (pos.X < connectivityGraph.Keys[begin + (end - begin) / 2])
            if (Math.Abs(pos.X - connectivityGraph.Keys[begin + (end - begin) / 2]) < Math.Abs(pos.X - connectivityGraph.Keys[begin + (end - begin) / 2 - 1]))
                i = begin + (end - begin) / 2;
            else
                i = begin + (end - begin) / 2 - 1;
        else if (Math.Abs(pos.X - connectivityGraph.Keys[begin + (end - begin) / 2]) < Math.Abs(pos.X - connectivityGraph.Keys[begin + (end - begin) / 2 + 1]))
            i = begin + (end - begin) / 2;
        else
            i = begin + (end - begin) / 2 + 1;
        ret.X = connectivityGraph.Keys[i];
        int prevXIndex = i - 1;
        int postXIndex = i + 1;

        int lowXBound = (int)(pos.X - Math.Sqrt(maxSearchRange * 2 / gravity) * xVelocity);
        int highXBound = (int)(pos.X + Math.Sqrt(maxSearchRange * 2 / gravity) * xVelocity);
        MinHeap<Tuple<float, Vector2>> bestNodes = new MinHeap<Tuple<float, Vector2>>(delegate (Tuple<float, Vector2> n1, Tuple<float, Vector2> n2)
        {
            if (n1.Item1 < n2.Item1)
                return -1;
            else if (n1.Item1 == n2.Item1)
                return 0;
            else
                return 1;
        });
        while (true)
        {
            begin = 0;
            end = connectivityGraph[(int)ret.X].Count;
            if ((end == 1 || end == 2) && obj.getBot(pos) <= connectivityGraph[(int)ret.X].Keys[0])
                ret.Y = connectivityGraph[(int)ret.X].Keys[0];
            else if (end == 2 && obj.getBot(pos) <= connectivityGraph[(int)ret.X].Keys[1])
                ret.Y = connectivityGraph[(int)ret.X].Keys[1];
            else
            {
                while (begin + (end - begin) / 2 != 0
                    && begin + (end - begin) / 2 + 1 != connectivityGraph[(int)ret.X].Count
                    && !(obj.getBot(pos) <= connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2] && obj.getBot(pos) >= connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2 - 1]
                    || obj.getBot(pos) >= connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2] && obj.getBot(pos) <= connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2 + 1]))
                    if (obj.getBot(pos) > connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2])
                        begin = begin + (end - begin) / 2 + 1;
                    else
                        end = begin + (end - begin) / 2;

                if (begin + (end - begin) / 2 + 1 != connectivityGraph[(int)ret.X].Count)
                    if (obj.getBot(pos) < connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2])
                        ret.Y = connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2];
                    else
                        ret.Y = connectivityGraph[(int)ret.X].Keys[begin + (end - begin) / 2 + 1];
            }

            if (ret.Y != 1
                && straightable(platforms, new Vector2(pos.X, obj.getBot(pos)), ret)
                && straightable(platforms, new Vector2(obj.getLeft(pos), pos.Y), new Vector2(obj.getLeft(pos), ret.Y + (pos.Y - obj.getBot(pos))))
                && straightable(platforms, new Vector2(obj.getRight(pos), pos.Y), new Vector2(obj.getRight(pos), ret.Y + (pos.Y - obj.getBot(pos))))
                && straightable(platforms, new Vector2(pos.X, obj.getTop(pos)), new Vector2(ret.X, ret.Y + (obj.getTop(pos) - obj.getBot(pos))))
                && (obj.getBot(pos) == ret.Y && (straightable(platforms, new Vector2(obj.getLeft(pos), obj.getBot(pos)), ret, true) || straightable(platforms, new Vector2(obj.getRight(pos), obj.getBot(pos)), ret, true))
                || Math.Sqrt(Math.Abs(obj.getBot(pos) - ret.Y) * 2 / gravity) * xVelocity > Math.Abs(pos.X - ret.X) && Math.Abs(obj.getBot(pos) - ret.Y) < maxSearchRange))
                bestNodes.add(new Tuple<float, Vector2>((float)Math.Sqrt(Math.Abs(pos.X - ret.Y) * 2 / gravity) + distance(ret, goal), ret));
            ret.Y = 1;

            if (prevXIndex == -1 && (postXIndex == connectivityGraph.Keys.Count || connectivityGraph.Keys[postXIndex] > highXBound)
                || prevXIndex != -1 && connectivityGraph.Keys[prevXIndex] < lowXBound && (postXIndex == connectivityGraph.Keys.Count || connectivityGraph.Keys[postXIndex] > highXBound))
                if (!bestNodes.empty())
                {
                    List<Vector2> retList = new List<Vector2>();
                    while (!bestNodes.empty())
                    {
                        retList.Add(bestNodes.peek().Item2);
                        bestNodes.pop();
                    }
                    return retList;
                }
                else
                    return null;

            if (prevXIndex == -1 || connectivityGraph.Keys[prevXIndex] < lowXBound)
                ret.X = connectivityGraph.Keys[postXIndex++];
            else if (postXIndex == connectivityGraph.Keys.Count || connectivityGraph.Keys[postXIndex] > highXBound)
                ret.X = connectivityGraph.Keys[prevXIndex--];
            else if (Math.Abs(pos.X - connectivityGraph.Keys[prevXIndex]) < Math.Abs(pos.X - connectivityGraph.Keys[postXIndex]))
                ret.X = connectivityGraph.Keys[prevXIndex--];
            else
                ret.X = connectivityGraph.Keys[postXIndex++];
        }
    }

    private ConnectivityPath generateConnectivityPath(ConnectivityNode startNode, ConnectivityNode endNode, SortedVector2List<Platform> platforms, float gravity)
    {
        bool straightPath = straightable(platforms, startNode.position, endNode.position, true)
            && straightable(platforms, startNode.position - new Vector2(0, Game.MEDIUM_ENEMY_HEIGHT / 3), endNode.position - new Vector2(0, Game.MEDIUM_ENEMY_HEIGHT / 3), false)
            && (endNode.position.X > startNode.position.X + Game.MEDIUM_ENEMY_WIDTH / 2
            || straightable(platforms, startNode.position - new Vector2(Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 3 * 2), endNode.position - new Vector2(-Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 3 * 2), false))
            && (endNode.position.X < startNode.position.X - Game.MEDIUM_ENEMY_WIDTH / 2
            || straightable(platforms, startNode.position - new Vector2(-Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 3 * 2), endNode.position - new Vector2(Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 3 * 2), false))
            && straightable(platforms, startNode.position - new Vector2(0, Game.MEDIUM_ENEMY_HEIGHT), endNode.position - new Vector2(0, Game.MEDIUM_ENEMY_HEIGHT), false);

        bool largeCharacterStraightPath = straightable(platforms, startNode.position, endNode.position, true)
            && straightable(platforms, startNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT) / 4, endNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT) / 3, false)
            && straightable(platforms, startNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT) / 4 * 2, endNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT) / 4 * 2, false)
            && (endNode.position.X > startNode.position.X + Game.MEDIUM_ENEMY_WIDTH / 2
            || straightable(platforms, startNode.position - new Vector2(Game.LARGE_ENEMY_WIDTH / 2, Game.LARGE_ENEMY_HEIGHT / 4 * 3), endNode.position - new Vector2(-Game.LARGE_ENEMY_WIDTH / 2, Game.LARGE_ENEMY_HEIGHT / 4 * 3), false))
            && (endNode.position.X < startNode.position.X - Game.MEDIUM_ENEMY_WIDTH / 2
            || straightable(platforms, startNode.position - new Vector2(-Game.LARGE_ENEMY_WIDTH / 2, Game.LARGE_ENEMY_HEIGHT / 4 * 3), endNode.position - new Vector2(Game.LARGE_ENEMY_WIDTH / 2, Game.LARGE_ENEMY_HEIGHT / 4 * 3), false))
            && straightable(platforms, startNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT), endNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT), false);

        bool walkable = straightPath && straightable(platforms, new Vector2(startNode.position.X, endNode.position.Y), endNode.position, true);

        // dy = t * Vy
        // dy = t * J / 2
        // dy = J / g * J / 2
        // dy = J^2 / g / 2
        // Jmin = sqrt(dy * 2 * g)
        float[] jumpCosts = new float[Game.ALL_ENEMY_MAX_JUMP_STRENGTH + 1];
        bool[] jumpableStrengths = new bool[Game.ALL_ENEMY_MAX_JUMP_STRENGTH + 1];
        bool[] largeCharacterJumpable = new bool[Game.ALL_ENEMY_MAX_JUMP_STRENGTH + 1];
        int jumpMin = (endNode.position.Y > startNode.position.Y) ? 0 : (int)Math.Sqrt(Math.Abs(endNode.position.Y - startNode.position.Y) * 2 * gravity) + 1;
        if (Game.ALL_ENEMY_MIN_JUMP_STRENGTH > jumpMin)
            jumpMin = Game.ALL_ENEMY_MIN_JUMP_STRENGTH;
        for (; jumpMin <= Game.ALL_ENEMY_MAX_JUMP_STRENGTH; jumpMin += 1)
            if ((jumpCosts[jumpMin] = jumpable(platforms, gravity, startNode.position, endNode.position, jumpMin)) != -1
                && jumpable(platforms, gravity, startNode.position - new Vector2(-Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 2), endNode.position - new Vector2(-Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 2), jumpMin) != -1
                && jumpable(platforms, gravity, startNode.position - new Vector2(Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 2), endNode.position - new Vector2(Game.MEDIUM_ENEMY_WIDTH / 2, Game.MEDIUM_ENEMY_HEIGHT / 2), jumpMin) != -1
                && jumpable(platforms, gravity, startNode.position - new Vector2(0, Game.MEDIUM_ENEMY_HEIGHT), endNode.position - new Vector2(0, Game.MEDIUM_ENEMY_HEIGHT), jumpMin) != -1)
            {
                jumpableStrengths[jumpMin] = true;
                if (jumpable(platforms, gravity, startNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT), endNode.position - new Vector2(0, Game.LARGE_ENEMY_HEIGHT), jumpMin) != -1)
                    largeCharacterJumpable[jumpMin] = true;
            }

        bool jumpPath = false;
        for (int i = 0; i < jumpableStrengths.Length && !jumpPath; ++i)
            jumpPath = true;

        if (!jumpPath && !straightPath)
            return null;

        return new ConnectivityPath(startNode, endNode, jumpableStrengths, largeCharacterJumpable, largeCharacterStraightPath, straightPath, walkable, jumpPath, jumpCosts);
    }

    public static bool platformsCollissionRay(SortedVector2List<Platform> platforms, Vector2 startNode, Vector2 endNode)
    {
        if(!platforms.ForAllInRange((startNode - endNode) / 2 + endNode, new Vector2(Math.Abs(startNode.X - endNode.X), Math.Abs(startNode.X - endNode.X)), 
            delegate (Platform p)
        {
            for (int i = 0; i < p.CollisionArea.Count; ++i)
            {
                int ii = (i == p.CollisionArea.Count - 1) ? 0 : i + 1;
                if (collissionRay(p.Position + p.CollisionArea[i], p.Position + p.CollisionArea[ii], startNode, endNode))
                    return false;
            }
            return true;
        }))
            return true;
        return false;

        // TO DELETE
        /*foreach (Platform p in platforms)
            for (int i = 0; i < p.CollisionArea.Count; ++i)
            {
                int ii = (i == p.CollisionArea.Count - 1) ? 0 : i + 1;
                if (collissionRay(p.Position + p.CollisionArea[i], p.Position + p.CollisionArea[ii], startNode, endNode))
                    return true;
            }
        return false;*/
    }

    public static bool collissionRay(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
    {
        // https:// stackoverflow.com/questions/4977491/determining-if-two-line-segments-intersect/4977569#4977569

        Vector2 u0 = start1;
        Vector2 v0 = end1 - u0;

        Vector2 u1 = start2;
        Vector2 v1 = end2 - u1;

        float determinant = v1.X * v0.Y - v0.X * v1.Y;
        float s = (1 / determinant) * ((u0.X - u1.X) * v0.Y - (u0.Y - u1.Y) * v0.X);
        float t = (1 / determinant) * -(-(u0.X - u1.X) * v1.Y + (u0.Y - u1.Y) * v1.X);

        if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            return true;
        return false;
    }

    public static bool straightable(SortedVector2List<Platform> platforms, Vector2 startNode, Vector2 endNode, bool walkable = false)
    {
        bool platformToWalkOn = false;
        if (startNode.Y <= endNode.Y)
        {
            if (!platforms.ForAllInRange((startNode - endNode) / 2 + endNode, new Vector2(Math.Abs(startNode.X - endNode.X), Math.Abs(startNode.X - endNode.X)),
                delegate (Platform p)
                {
                    for (int i = 0; i < p.CollisionArea.Count; ++i)
                    {
                        int ii = (i == p.CollisionArea.Count - 1) ? 0 : i + 1;
                        if (collissionRay(p.Position + p.CollisionArea[i], p.Position + p.CollisionArea[ii], startNode, endNode))
                            return false;
                    }
                    return true;
                }))
                return false;

            if (walkable && !platformToWalkOn)
                platformToWalkOn = !platforms.ForAllInRange((startNode - endNode) / 2 + endNode, new Vector2(Math.Abs(startNode.X - endNode.X), Math.Abs(startNode.X - endNode.X)),
                    delegate (Platform p)
                    {
                        float smallX;
                        float bigX;
                        if (startNode.X < endNode.X)
                        {
                            smallX = startNode.X + 1;
                            bigX = endNode.X - 1;
                        }
                        else
                        {
                            bigX = startNode.X - 1;
                            smallX = endNode.X + 1;
                        }
                        return !(p.getTop() == startNode.Y + 1 && p.getLeft() <= smallX && p.getRight() >= bigX);
                    });

                /*if (walkable && !platformToWalkOn)
                {
                    float smallX;
                    float bigX;
                    if (startNode.X < endNode.X)
                    {
                        smallX = startNode.X + 1;
                        bigX = endNode.X - 1;
                    }
                    else
                    {
                        bigX = startNode.X - 1;
                        smallX = endNode.X + 1;
                    }
                    platformToWalkOn = p.getTop() == startNode.Y + 1 && p.getLeft() <= smallX && p.getRight() >= bigX;
                }*/
        }
        else
            return false;
        if (walkable && startNode.Y == endNode.Y && !platformToWalkOn)
            return false;

        return true;
    }

    private float jumpable(SortedVector2List<Platform> platforms, float gravity, Vector2 startNode, Vector2 endNode, int jumpStrength)
    {
        int J = -(int)(jumpStrength * jumpStrength / gravity / 2.0);
        int D = (int)(endNode.X - startNode.X);

        float dropDistance = endNode.Y - startNode.Y;
        Vector2[] jumpParabolaPoints = new Vector2[5];
        jumpParabolaPoints[0] = new Vector2(0, 0);
        jumpParabolaPoints[4] = endNode - startNode;

        float totalTimeAirborne = jumpStrength / gravity + (float)Math.Sqrt((jumpStrength * jumpStrength / gravity + 2 * (endNode.Y - startNode.Y)) / gravity);
        float Vx = D / totalTimeAirborne;
        jumpParabolaPoints[2] = new Vector2(jumpStrength / gravity * Vx, J);

        // y = ax^2 + bx + c
        // Using three points (jumpParabolaPoints 0, 2, 4)
        // y0 = ax0^2 + bx0 + c
        //  (0) = a(0) + b(0) + c
        //  0 = c
        // y2 = ax2^2 + bx2 + c
        // y4 = ax4^2 + bx4 + c
        // a = (x4 * y2 - x2 * y4) / (x2^2 * x4 - x2 * x4^2)
        // b = (y - ax^2) / x
        float a = (float)((jumpParabolaPoints[4].X * jumpParabolaPoints[2].Y - jumpParabolaPoints[2].X * jumpParabolaPoints[4].Y)
            / (Math.Pow(jumpParabolaPoints[2].X, 2) * jumpParabolaPoints[4].X - jumpParabolaPoints[2].X * Math.Pow(jumpParabolaPoints[4].X, 2)));
        float b = (float)(jumpParabolaPoints[4].Y - a * Math.Pow(jumpParabolaPoints[4].X, 2)) / jumpParabolaPoints[4].X;

        jumpParabolaPoints[0] += startNode;
        jumpParabolaPoints[2] += startNode;
        jumpParabolaPoints[4] += startNode;

        if (startNode.X < endNode.X)
        {
            // y = ax^2 + bx + c
            // 0 = ax^2 + bx + (0) - y
            // (-b +- sqrt(b^2 - 4a(-y)) / (2 * a)
            jumpParabolaPoints[1] = startNode + new Vector2((float)((-b - Math.Sqrt(Math.Pow(b, 2) - 4 * a * -(J / 2))) / (2 * a)), J / 2f);
            jumpParabolaPoints[3] = startNode + new Vector2((float)((-b + Math.Sqrt(Math.Pow(b, 2) - 4 * a * -((J + dropDistance) / 2f))) / (2 * a)), (J + dropDistance) / 2f);
        }
        else
        {
            jumpParabolaPoints[1] = startNode + new Vector2((float)((-b + Math.Sqrt(Math.Pow(b, 2) - 4 * a * -(J / 2))) / (2 * a)), J / 2f);
            jumpParabolaPoints[3] = startNode + new Vector2((float)((-b - Math.Sqrt(Math.Pow(b, 2) - 4 * a * -((J + dropDistance)) / 2f)) / (2 * a)), (J + dropDistance) / 2f);
        }

        for (int j = 0; j < 4; ++j)
            if (!platforms.ForAllInRange((jumpParabolaPoints[j] - jumpParabolaPoints[j + 1]) / 2 + jumpParabolaPoints[j], 
                new Vector2(Level.MAX_PLATFORM_BOUND) + (jumpParabolaPoints[j] - jumpParabolaPoints[j + 1]) / 2, 
                delegate (Platform p)
                {
                     for (int i = 0; i < p.CollisionArea.Count; ++i)
                     {
                         int ii = (i == p.CollisionArea.Count - 1) ? 0 : i + 1;

                         if (collissionRay(p.Position + p.CollisionArea[i], p.Position + p.CollisionArea[ii], jumpParabolaPoints[j], jumpParabolaPoints[j + 1]))
                             return false;
                     };
                     return true;
                }))
                return -1;

        // TO DELETE
        /*foreach (Platform p in platforms)
            for (int i = 0; i < p.CollisionArea.Count; ++i)
            {
                int ii = (i == p.CollisionArea.Count - 1) ? 0 : i + 1;

                for (int j = 0; j < 4; ++j)
                {
                    if (collissionRay(p.Position + p.CollisionArea[i], p.Position + p.CollisionArea[ii], jumpParabolaPoints[j], jumpParabolaPoints[j + 1]))
                        return -1;
                }
            }*/
        return totalTimeAirborne;
    }

    private float calculateHeuristic(Vector2 p1, Vector2 p2, int xVel, int jumpStrength)
    {
        //http:// theory.stanford.edu/~amitp/GameProgramming/Heuristics.html#heuristics-for-grid-maps
        Vector2 distance = new Vector2(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
        return 1 / (float)((xVel > jumpStrength / 2) ? xVel : jumpStrength / 2) * (float)Math.Sqrt(distance.X * distance.X + distance.Y * distance.Y);
    }

    private float distance(ConnectivityNode p1, ConnectivityNode p2)
    {
        return (float)Math.Sqrt(Math.Pow(p1.position.X - p2.position.X, 2) + Math.Pow(p1.position.Y - p2.position.Y, 2));
    }


    private float distance(Vector2 p1, Vector2 p2)
    {
        return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

}
 