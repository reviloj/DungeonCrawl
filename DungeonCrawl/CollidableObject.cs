
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;

abstract public class CollidableObj
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public Direction direction;
    public CollisionTypes collisionType;
    protected List<Vector2> collisionArea = new List<Vector2>();
    public List<Vector2> CollisionArea { get
        {
            return new List<Vector2>(collisionArea);
        } }

    public CollidableObj(Vector2 pos, CollisionTypes colType)
    {
        Position = pos;
        direction = Direction.Right;
        collisionType = colType;
    }

    public CollidableObj(Vector2 pos, Vector2 vel, CollisionTypes colType)
    {
        Position = pos;
        Velocity = vel;
        direction = Direction.Right;
        collisionType = colType;
    }

    public CollidableObj(CollidableObj obj)
    {
        Position = obj.Position;
        Velocity = obj.Velocity;
        direction = obj.direction;
        collisionType = obj.collisionType;
        foreach(Vector2 vec in obj.collisionArea)
            collisionArea.Add(vec);
    }

    virtual public bool setPosition(Vector2 pos)
    {
        Position = pos;
        return true;
    }
    virtual public bool addPosition(Vector2 pos)
    {
        Position += pos;
        return true;
    }

    virtual public bool setVelocity(Vector2 vel)
    {
        Velocity = vel;
        return true;
    }
    virtual public bool addVelocity(Vector2 vel)
    {
        Velocity += vel;
        return true;
    }

    public int getTop()
    {
        float ret = CollisionArea[0].Y;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].Y < ret)
                ret = CollisionArea[i].Y;
        return (int)(ret + Position.Y);
    }
    public int getBot()
    {
        float ret = CollisionArea[0].Y;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].Y > ret)
                ret = CollisionArea[i].Y;
        return (int)(ret + Position.Y) - 1;
    }
    public int getLeft()
    {
        float ret = CollisionArea[0].X;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].X < ret)
                ret = CollisionArea[i].X;
        return (int)(ret + Position.X);
    }
    public int getRight()
    {
        float ret = CollisionArea[0].X;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].X > ret)
                ret = CollisionArea[i].X;
        return (int)(ret + Position.X);
    }
    public int getTop(Vector2 pos)
    {
        float ret = CollisionArea[0].Y;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].Y < ret)
                ret = CollisionArea[i].Y;
        return (int)(ret + pos.Y);
    }
    public int getBot(Vector2 pos)
    {
        float ret = CollisionArea[0].Y;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].Y > ret)
                ret = CollisionArea[i].Y;
        return (int)(ret + pos.Y) - 1;
    }
    public int getLeft(Vector2 pos)
    {
        float ret = CollisionArea[0].X;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].X < ret)
                ret = CollisionArea[i].X;
        return (int)(ret + pos.X);
    }
    public int getRight(Vector2 pos)
    {
        float ret = CollisionArea[0].X;
        for (int i = 1; i < CollisionArea.Count; ++i)
            if (CollisionArea[i].X > ret)
                ret = CollisionArea[i].X;
        return (int)(ret + pos.X);
    }

    private List<Vector2> getCollisionAxis(CollidableObj obj, List<Vector2> foundAxis = null)
    {
        List<Vector2> axis = new List<Vector2>();
        for (int i = 0; i < obj.collisionArea.Count; ++i)
        {
            Vector2 p1 = obj.collisionArea[i] * new Vector2((int)obj.direction, 1) + obj.Position;
            Vector2 p2 = obj.collisionArea[(i == obj.collisionArea.Count - 1) ? 0 : i + 1] * new Vector2((int)obj.direction, 1) + obj.Position;
            float xDif = p1.X - p2.X;
            float yDif = p1.Y - p2.Y;
            double magnitude = Math.Sqrt(Math.Pow(xDif, 2) + Math.Pow(yDif, 2));
            Vector2 curAxis = new Vector2(-yDif / (float)magnitude, xDif / (float)magnitude);
            if (foundAxis != null && foundAxis.IndexOf(curAxis) == -1)
                axis.Add(curAxis);
        }
        return axis;
    }

    private double[] getObjectMinMax(Vector2 axis, CollidableObj obj, float timeDisplacement = 0, bool useVelocity = false)
    {
        double[] ret = new double[2];
        for (int i = 0; i < obj.collisionArea.Count; ++i)
        {
            Vector2 p = obj.collisionArea[i] * new Vector2((int)obj.direction, 1) + obj.Position + (useVelocity ? obj.Velocity * timeDisplacement : new Vector2(0, 0));
            if (i == 0)
            {
                ret[0] = axis.X * p.X + axis.Y * p.Y;
                ret[1] = ret[0];
            }
            else
            {
                double curPointAxisMagnitude = axis.X * p.X + axis.Y * p.Y;
                if (curPointAxisMagnitude < ret[0])
                    ret[0] = curPointAxisMagnitude;
                else if (curPointAxisMagnitude > ret[1])
                    ret[1] = curPointAxisMagnitude;
            }
        }
        return ret;
    }

    public List<Vector2>[] hasCollision(CollidableObj obj, float timeDisplacement)
    {
        if (collisionArea.Count == 0 || obj.collisionArea.Count == 0)
            return null;
        List<Vector2> axis = new List<Vector2>();
        axis.AddRange(getCollisionAxis(this));
        axis.AddRange(getCollisionAxis(obj, axis));

        foreach (Vector2 curAxis in axis)
        {
            double[] obj1MinMax = getObjectMinMax(curAxis, this, timeDisplacement, true);
            double[] obj2MinMax = getObjectMinMax(curAxis, obj, timeDisplacement, true);
            double[] preMoveObj1MinMax = getObjectMinMax(curAxis, this);
            double[] preMoveObj2MinMax = getObjectMinMax(curAxis, obj);

            if (preMoveObj1MinMax[0] > preMoveObj2MinMax[1] && obj1MinMax[1] < obj2MinMax[0] // If obj1 min was greater then obj2 max and now obj1 max is less then obj2 min
                || preMoveObj1MinMax[1] < preMoveObj2MinMax[0] && obj1MinMax[0] > obj2MinMax[1] // If obj1 max was less then obj2 min and now obj1 min is greater then obj2 max
                //|| preMoveObj1MinMax[0] > preMoveObj2MinMax[0] && preMoveObj1MinMax[0] < preMoveObj2MinMax[1] // Obj1 min is colliding with obj2
                //|| preMoveObj1MinMax[1] > preMoveObj2MinMax[0] && preMoveObj1MinMax[1] < preMoveObj2MinMax[1] // Obj1 max is colliding with obj2
                )
                continue;
            if (obj1MinMax[0] > obj2MinMax[1] || obj1MinMax[1] < obj2MinMax[0])
                return null;
        }
        List<Vector2>[] collisionAxis = new List<Vector2>[2];
        collisionAxis[0] = new List<Vector2>();
        // Distance to obj
        collisionAxis[1] = new List<Vector2>();
        foreach (Vector2 curAxis in axis)
        {
            double[] preMoveObj1MinMax = getObjectMinMax(curAxis, this);
            double[] preMoveObj2MinMax = getObjectMinMax(curAxis, obj);
            if (preMoveObj1MinMax[0] >= preMoveObj2MinMax[1]/* || obj1MinMax[1] < obj2MinMax[0]*/)
            {
                collisionAxis[0].Add(curAxis * new Vector2(-1, -1));
                float distance = (float)((preMoveObj1MinMax[0] >= preMoveObj2MinMax[1]) ? preMoveObj1MinMax[0] - preMoveObj2MinMax[1] : preMoveObj2MinMax[0] - preMoveObj1MinMax[1]);
                collisionAxis[1].Add(new Vector2(distance * curAxis.X, distance * curAxis.Y));
            }
        }
        
        if (collisionAxis[0].Count == 2) 
        {
            if (obj.Velocity * collisionAxis[0][0] + collisionAxis[0][0] * this.Velocity != new Vector2()
                && obj.Velocity * collisionAxis[0][1] + collisionAxis[0][1] * this.Velocity != new Vector2())
            {
                Vector2 axis0 = obj.Velocity * collisionAxis[0][0] + collisionAxis[0][0] * this.Velocity;
                Vector2 axis1 = obj.Velocity * collisionAxis[0][1] + collisionAxis[0][1] * this.Velocity;
                if (axis0.X != 0 && axis0.X > axis1.Y 
                    || axis0.Y != 0 && axis0.Y > axis1.X)
                {
                    collisionAxis[0].RemoveAt(0);
                    collisionAxis[1].RemoveAt(0);
                }
                else if (axis0 == axis1)
                {
                    if(axis0.X == 0)
                    {
                        collisionAxis[0].RemoveAt(1);
                        collisionAxis[1].RemoveAt(1);
                    }
                    else
                    {
                        collisionAxis[0].RemoveAt(0);
                        collisionAxis[1].RemoveAt(0);
                    }
                }
                else
                {
                    collisionAxis[0].RemoveAt(1);
                    collisionAxis[1].RemoveAt(1);
                }
                return collisionAxis;
            }
            
            if (collisionAxis[1][0].X == 0 && collisionAxis[1][0].Y == 0 && obj.Velocity * collisionAxis[0][0] == new Vector2() && this.Velocity * collisionAxis[0][0] == new Vector2()
                    || collisionAxis[1][1].X == 0 && collisionAxis[1][1].Y == 0 && obj.Velocity * collisionAxis[0][1] == new Vector2() && this.Velocity * collisionAxis[0][1] == new Vector2())
                for (int i = collisionAxis[0].Count - 1; i >= 0; --i)
                    if (obj.Velocity * collisionAxis[0][i] != new Vector2() || this.Velocity * collisionAxis[0][i] != new Vector2())
                    {
                        collisionAxis[0].RemoveAt(i);
                        collisionAxis[1].RemoveAt(i);
                    }
            return (collisionAxis[0].Count > 0) ? collisionAxis : null;
        }
        return collisionAxis;
    }

    public void handleCollision(CollidableObj obj, float timeDisplacement)
    {
        List<Vector2>[] collisionAxis = hasCollision(obj, timeDisplacement);
        if (collisionAxis != null)
            if (collisionType > obj.collisionType)
                collide(obj, collisionAxis);
            else if (collisionType < obj.collisionType)
                obj.collide(this, collisionAxis);
            else
            {
                collide(obj, collisionAxis);
                obj.collide(this, collisionAxis);
            }
    }

    abstract public void collide(CollidableObj obj, List<Vector2>[] axis);
}
