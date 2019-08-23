
using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

class FuncComparer<T> : IComparer<T>
{
    private readonly Comparison<T> comparison;
    public FuncComparer(Comparison<T> comparison)
    {
        this.comparison = comparison;
    }
    public int Compare(T x, T y)
    {
        return comparison(x, y);
    }
}

public class SortedVector2List<T>
{
    public SortedList<int, SortedList<int, T>> list = new SortedList<int, SortedList<int, T>>();

    public int Count { get { return list.Count; } }

    public IList<SortedList<int, T>> Values { get { return list.Values; } }

    public IList<int> Keys { get { return list.Keys; } }

    public SortedVector2List() { }

    public bool Exists(Vector2 v)
    {
        return list.ContainsKey((int)v.X) && list[(int)v.X].ContainsKey((int)v.Y);
    }

    public void Add(Vector2 v, T t)
    {
        SortedList<int, T> setPieceYList;
        if (list.TryGetValue((int)v.X, out setPieceYList))
            setPieceYList.Add((int)v.Y, t);
        else
            list.Add((int)v.X, new SortedList<int, T> { { (int)v.Y, t } });
    }

    public void Remove(int x, int y)
    {
        if (list.ContainsKey(x))
            if (list[x].ContainsKey(y))
                if (list[x].Count == 1)
                    list.Remove(x);
                else
                    list[x].Remove(y);
    }

    public void RemoveAll()
    {
        list.Clear();
    }

    public T GetItem(Vector2 v)
    {
        return list[(int)v.X][(int)v.Y];
    }

    public SortedList<int, T> GetList(int x)
    {
        return list[x];
    }

    public ICollection<T> GetAllItems()
    {
        List<T> ret = new List<T>();
        foreach (KeyValuePair<int, SortedList<int, T>> sL in list)
            ret.AddRange(sL.Value.Values);
        return ret;
    }

    public bool ForAllInRange(Vector2 position, int bounds, Func<T, bool> conditional)
    {
        return ForAllInRange(position, new Vector2(bounds), conditional);
    }

    public bool ForAllInRange(Vector2 position, Vector2 bounds, Func<T, bool> conditional)
    {
        int x = binarySearch(this, (int)(position.X - bounds.X), 1);
        int j = (x == -1) ? list.Count : x;
        for (; j < list.Count && position.X + bounds.X > list.Keys[j]; ++j)
        {
            int y = binarySearch(list.Values[j], (int)(position.Y - bounds.Y), 1);
            for (int k = (y == -1) ? list.Values[j].Count : y; k < list.Values[j].Count && position.Y + bounds.Y > list.Values[j].Keys[k]; ++k)
                if (!conditional(list.Values[j].Values[k]))
                    return false;
        }
        return true;
    }

    public static int binarySearch(SortedVector2List<T> list, int z, int goal)
    {
        return SortedVector2List<SortedList<int, T>>.binarySearch(list.list, z, goal);
    }

    public static int binarySearch(SortedList<int, T> list, int z, int goal)
    {

        if (list.Count == 0)
            return -1;
        if (list.Count == 1 && goal != 0)
            if (goal == -1 && list.Keys[0] < z || goal == 1 && list.Keys[0] > z)
                return 0;
            else
                return -1;

        int begin = 0;
        int end = list.Count;

        switch (goal)
        {
            case -1:
                while (!(list.Keys[begin + (end - begin) / 2 - 1] <= z && z < list.Keys[begin + (end - begin) / 2]))
                {
                    if (z > list.Keys[begin + (end - begin) / 2 - 1])
                        begin = begin + (end - begin) / 2 + 1;
                    else
                        end = begin + (end - begin) / 2;

                    if (end - 1 == 0)
                        return -1;
                    if (begin + (end - begin) / 2 == list.Count)
                        return list.Count - 1;
                }
                return begin + (end - begin) / 2 - 1;
            case 0:
                while (z != list.Keys[begin + (end - begin) / 2])
                {
                    if (z > list.Keys[begin + (end - begin) / 2])
                        begin = begin + (end - begin) / 2 + 1;
                    else
                        end = begin + (end - begin) / 2;

                    if (end == 0)
                        return -1;
                    if (begin == list.Count)
                        return -1;
                }
                return begin + (end - begin) / 2 - 1;
            case 1:
                while (!(list.Keys[begin + (end - begin) / 2 - 1] < z && z <= list.Keys[begin + (end - begin) / 2]))
                {
                    if (z > list.Keys[begin + (end - begin) / 2])
                        begin = begin + (end - begin) / 2 + 1;
                    else
                        end = begin + (end - begin) / 2;

                    if (begin + (end - begin) / 2 == 0)
                        return 0;
                    if (begin == list.Count)
                        return -1;
                }
                return begin + (end - begin) / 2;
        }
        return -1;
    }
}

public class RedBlackTree<T> where T : IComparable
{
    private RBTNode<T> root;

    public RedBlackTree() { }

    public bool exists(T data)
    {
        RBTNode<T> curNode = root;
        while (curNode != null)
            if (data.CompareTo(curNode.data) == -1)
                curNode = curNode.left;
            else if (data.CompareTo(curNode.data) == 1)
                curNode = curNode.right;
        else if (data.CompareTo(curNode.data) == 0)
            return true;
        return false;
    }

    private void rotateLeft(RBTNode<T> x)
    {
        // Create node y to be the right node of x (x and y will be the nodes being rotated left)
        RBTNode<T> y = x.right;

        x.right = y.left;
        if (x.right != null)
            x.right.parent = x;

        y.parent = x.parent;
        if (x.parent == null)
            root = y;
        else if (x.parent.left == x)
            x.parent.left = y;
        else
            x.parent.right = y;

        x.parent = y;
        y.left = x;
    }

    private void rotateRight(RBTNode<T> x)
    {
        // Create node y to be the right node of x (x and y will be the nodes being rotated left)
        RBTNode<T> y = x.left;

        x.left = y.right;
        if (x.left != null)
            x.left.parent = x;

        y.parent = x.parent;
        if (x.parent == null)
            root = y;
        else if (x.parent.left == x)
            x.parent.left = y;
        else
            x.parent.right = y;

        x.parent = y;
        y.right = x;
    }

    private void adjustTreeColour(RBTNode<T> node)
    {
        if (node == root)
        {
            node.colour = true;
            return;
        }

        if (node.parent != root)
        {
            RBTNode<T> p = node.parent;
            RBTNode<T> u = (p.parent.left == p) ? p.parent.right : p.parent.left;

            if (!p.colour)
                if (u != null && !u.colour)
                {
                    p.colour = true;
                    u.colour = true;
                    p.parent.colour = false;
                    adjustTreeColour(p.parent);
                }
        }
    }

    private void leftLeftAdjust(RBTNode<T> node)
    {
        node.parent.colour = !node.parent.colour;
        node.parent.parent.colour = !node.parent.parent.colour;
        rotateRight(node.parent.parent);
    }

    private void rightRightAdjust(RBTNode<T> node)
    {
        node.parent.colour = !node.parent.colour;
        node.parent.parent.colour = !node.parent.parent.colour;
        rotateLeft(node.parent.parent);
    }

    public void insert(T data)
    {
        RBTNode<T> node = new RBTNode<T>(data);
        node.colour = false;
        if (root == null)
        {
            root = node;
            node.colour = true;
            return;
        }

        RBTNode<T> curNode = root;
        while (node < curNode && curNode.left != null || node > curNode && curNode.right != null)
            curNode = (node < curNode) ? curNode.left : curNode.right;
        if (node < curNode)
            curNode.left = node;
        else
            curNode.right = node;
        node.parent = curNode;


        if (node.parent != root)
        {
            RBTNode<T> p = node.parent;
            RBTNode<T> u = (p.parent.left == p) ? p.parent.right : p.parent.left;
            if (u == null || u.colour)
            {
                if (node.parent.parent.left == node.parent)
                    if (node.parent.left == node)
                        leftLeftAdjust(node);
                    else
                    {
                        rotateLeft(p);
                        leftLeftAdjust(p);
                    }
                else
                    if (node.parent.right == node)
                    rightRightAdjust(node);
                else
                {
                    rotateRight(p);
                    rightRightAdjust(p);
                }

            }
            else
                adjustTreeColour(node);
        }
    }

    private void doubleBlackRecolour(RBTNode<T> node)
    {
        RBTNode<T> s = (node.parent.left == node) ? node.parent.right : node.parent.left;
        if (s.colour && s.left.colour && s.right.colour)
        {
            s.colour = false;
            if (s.parent.colour)
                doubleBlackRecolour(s.parent);
        }
    }

    private void handleDoubleBlack(RBTNode<T> node)
    {
        RBTNode<T> s = (node.parent.left == node) ? node.parent.right : node.parent.left;
        if (!s.colour)
        {
            if (node.parent.left == s)
                rotateRight(node.parent);
            else
                rotateLeft(node.parent);
            doubleBlackRecolour(node);
        }//else if(s.colour)
    }

    public void delete(T data)
    {
        RBTNode<T> nodeToDelete = root;
        while (nodeToDelete != null && nodeToDelete.data.CompareTo(data) != 0)
            nodeToDelete = (nodeToDelete.data.CompareTo(data) == -1) ? nodeToDelete.left : nodeToDelete.right;
        if (nodeToDelete == null)
            return;

        RBTNode<T> successor;
        if (nodeToDelete.left == null && nodeToDelete.right == null)
            successor = null;
        else if (nodeToDelete.left != null && nodeToDelete.right != null)
        {
            successor = nodeToDelete.right;
            while (successor.left != null)
                successor = successor.left;
            nodeToDelete.data = successor.data;
            nodeToDelete = successor;
            if (nodeToDelete.right == null)
                successor = null;
            else
                successor = nodeToDelete.right;
        }
        else
            successor = (nodeToDelete.left == null) ? nodeToDelete.right : nodeToDelete.left;

        if (!successor.colour || !nodeToDelete.colour)
            successor.colour = true;

        if (successor != null)
        {
            successor.left = root.left;
            successor.right = root.right;
        }
        nodeToDelete = successor;

        if (nodeToDelete != root)
            handleDoubleBlack(nodeToDelete);

    }
}

public class RBTNode<T> where T : IComparable
{
    public T data;
    public RBTNode<T> parent;
    public RBTNode<T> left;
    public RBTNode<T> right;
    // true for black false for red
    public bool colour;

    public RBTNode(T data)
    {
        this.data = data;
    }

    public static bool operator <(RBTNode<T> n1, RBTNode<T> n2)
    {
        return n1.data.CompareTo(n2.data) == -1;
    }

    public static bool operator >(RBTNode<T> n1, RBTNode<T> n2)
    {
        return n1.data.CompareTo(n2.data) == 1;
    }

}

public class MinHeap<T>
{
    private List<T> heap = new List<T>();
    private Func<T, T, int> comparator;

    public MinHeap(Func<T, T, int> comparator)
    {
        this.comparator = comparator;
    }

    public T peek()
    {
        return heap[1];
    }

    private void bubbleUpSort(int child)
    {
        if (child == 1)
            return;
        if(comparator(heap[child], heap[child / 2]) == -1)
        {
            T temp = heap[child / 2];
            heap[child / 2] = heap[child];
            heap[child] = temp;
            bubbleUpSort(child / 2);
        }
        return;
    }

    private void bubbleDownSort(int parent)
    {
        if (parent * 2 > heap.Count - 1)
            return;
        int smallestChild;
        if (parent * 2 + 1 > heap.Count - 1)
            smallestChild = parent * 2;
        else
            smallestChild = (comparator(heap[parent * 2], heap[parent * 2 + 1]) == -1) ? parent * 2 : parent * 2 + 1;
        if (comparator(heap[parent], heap[smallestChild]) == 1)
        {
            T temp = heap[smallestChild];
            heap[smallestChild] = heap[parent];
            heap[parent] = temp;
            bubbleDownSort(smallestChild);
        }
        return;
    }

    public void add(T data)
    {
        if (heap.Count == 0)
            heap.Add(data);
        heap.Add(data);
        bubbleUpSort(heap.Count - 1);
    }

    public void pop()
    {
        heap[1] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        bubbleDownSort(1);
    }

    public bool empty()
    {
        return heap.Count <= 1;
    }

}
