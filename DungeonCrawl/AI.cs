
using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

public class AI
{
    private float agroTimer = 0;
    private int tiredIntervals = 1;
    public Rectangle boundingBox { get; private set; }
    private Enemy body;
    public PathWaypoint path { get; private set; }
    public CollidableObj targetObj { get; private set; }
    private bool jumped = false;
    public float timeSincePathUpdate { get; private set; } = 0;
    public PathWaypoint currentWaypoint { get; private set; }
    public AIState state = AIState.Idling;
    public bool newPathFlag
    {
        get
        {
            return currentWaypoint != null || path == null;
        }
    }

    public AI(Enemy body, Rectangle boundingBox)
    {
        this.body = body;
        this.boundingBox = boundingBox;
    }

    public AI(AI ai, Enemy body)
    {
        this.body = body;
        boundingBox = ai.boundingBox;
    }

    public void setPath(PathWaypoint newPath, AIState state, CollidableObj obj = null)
    {
        timeSincePathUpdate = 0;
        path = newPath;
        targetObj = obj;
        jumped = false;
        if (this.state != AIState.Chasing && state == AIState.Chasing)
        {
            agroTimer = 0;
            tiredIntervals = 1;
        }
        this.state = state;
    }

    public void clearPath()
    {
        timeSincePathUpdate = 9999;
        path = null;
        targetObj = null;
        jumped = false;
    }

    public bool inBoundingBox(Vector2 vec)
    {
        return vec.X > boundingBox.Left && vec.X < boundingBox.Right && vec.Y < boundingBox.Y && vec.Y > boundingBox.Y - boundingBox.Height;
    }

    public int boundingBoxBottom()
    {
        return boundingBox.Y;
    }

    public void update(float timeDisplacement, SortedVector2List<Platform> platforms)
    {
        if (targetObj != null)
            agroTimer += timeDisplacement;
        timeSincePathUpdate += timeDisplacement;
        if (path != null || targetObj != null)
            currentWaypoint = null;

        if (state == AIState.Correctional && path == null && timeSincePathUpdate > 60)
            state = AIState.Idling;
        int x = new Random().Next(100000000);
        if (tiredIntervals * 30 < agroTimer)
        {
            ++tiredIntervals;
            if (x < Math.Pow(Math.Round(agroTimer / body.maxAgroTime * 100), 4) && state == AIState.Chasing)
                setPath(null, AIState.Correctional);
        }

        if (targetObj != null && body.hasCollision(targetObj, timeDisplacement) != null)
            clearPath();
        if (path != null)
            if (Math.Abs(body.Position.X - path.position.X) < body.calculateStat(StatTypes.Movement)
                && (Math.Abs(body.getBot() - path.position.Y) < body.calculateStat(StatTypes.Jump)
                && (body.states[(int)TypesOfStates.Movement] != ObjectStates.Airborne || path.pathType == PathType.Free)
                || path.nextWaypoint != null && path.nextWaypoint.pathType != PathType.Jump && path.nextWaypoint.position.Y >= path.nextWaypoint.position.Y
                && !AIManager.platformsCollissionRay(platforms, body.Position, path.nextWaypoint.position) && !AIManager.platformsCollissionRay(platforms, new Vector2(path.nextWaypoint.position.X, body.Position.Y), path.nextWaypoint.position)))
            {
                if (state != AIState.Chasing)
                    state = AIState.Idling;
                currentWaypoint = path;
                if (path.nextWaypoint == null && state != AIState.Idling)
                    clearPath();
                else
                {
                    path = path.nextWaypoint;
                    jumped = false;
                }
            }
    }

    public void moveBody()
    {
        if (body.states[(int)TypesOfStates.MovementLock] != ObjectStates.ControlLocked)
        {
            body.setVelocity(new Vector2(0, body.Velocity.Y));

            if (path != null)
            {
                if (Math.Abs(body.Position.X - path.position.X) < body.calculateStat(StatTypes.Movement) && body.states[(int)TypesOfStates.Movement] != ObjectStates.Airborne && body.getBot() < path.position.Y)
                {
                    clearPath();
                    return;
                }
                if (path.pathType == PathType.Free && path.position.Y < body.getBot() || path.pathType == PathType.Jump)
                {
                    if (jumped && body.canJump())
                    {
                        clearPath();
                        return;
                    }
                    else
                    {
                        body.jump();
                        jumped = true;
                    }
                }
                if (Math.Abs(body.Position.X - path.position.X) >= body.calculateStat(StatTypes.Movement))
                    if (path.position.X < body.Position.X)
                        body.moveLeft();
                    else if (path.position.X > body.Position.X)
                        body.moveRight();
            }
            else if (targetObj != null)
            {

                if (targetObj.getBot() < body.getBot())
                    body.jump();
                if (Math.Abs(body.Position.X - targetObj.Position.X) >= body.calculateStat(StatTypes.Movement))
                    if (targetObj.Position.X < body.Position.X)
                        body.moveLeft();
                    else if (targetObj.Position.X > body.Position.X)
                        body.moveRight();
            }
        }
    }
}


