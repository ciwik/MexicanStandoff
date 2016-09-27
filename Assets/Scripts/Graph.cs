using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Graph
{
    private Graph()
    {
        MainController.Graph = this;
    }

    private List<Vertex> vertices;
    private List<Edge> edges;
    private List<Unit> units;
    private List<Action> actions;

    private uint width, height;

    public uint VertexCount
    {
        get { return (uint)vertices.Count; }
    }

    public uint EdgeCount
    {
        get { return (uint)edges.Count; }
    }

    internal List<Vertex> Vertices
    {
        get { return vertices; }
    }

    internal List<Edge> Edges
    {
        get { return edges; }
    }

    internal List<Unit> Units
    {
        get { return units; }
    }

    public uint Width
    {
        get { return width; }
    }

    public uint Height
    {
        get { return height; }
    }

    public Vector2 Size
    {
        get { return new Vector2(width, height); }
    }

    public Unit Player;

    internal static Graph Create(uint x, uint y)
    {
        Graph graph = new Graph();        
        graph.width = x;
        graph.height = y;

        graph.vertices = new List<Vertex>();
        graph.edges = new List<Edge>();
        graph.units = new List<Unit>();
        graph.actions = new List<Action>();

        return graph;
    }

    public void AddAction(Action action)
    {
        actions.Add(action);
    }

    public Dictionary<Unit, List<Distance>> GetDistances()
    {
        Dictionary<Unit, List<Distance>> distanceDictionary =
            new Dictionary<Unit, List<Distance>>();

        foreach (Unit unit in units)
        {
            List<Distance> distances = GetDistancesForUnit(unit);
            if (distances != null)
                distanceDictionary.Add(unit, distances);
        }

        return distanceDictionary;
    }

    public List<Distance> GetDistancesForUnit(Unit unit)
    {
        Vertex startVertex = unit.CurrentVertex;
        List<Distance> distances = new List<Distance>();
        List<Vertex> targets = new List<Vertex>();
        foreach (Unit targetUnit in units)
        {
            if (!targetUnit.Equals(unit))
                targets.Add(targetUnit.CurrentVertex);
        }

        foreach (Vertex targetVertex in targets)
        {
            List<MarkedVertex> markedVertices = MarkedVertex.CreateMarkedVertexList();
            int mark = 0;
            foreach (Vertex vertex in vertices)
            {
                markedVertices.Add(new MarkedVertex(vertex));
            }

            MarkedVertex startMarkedVertex = MarkedVertex.Find(startVertex);
            startMarkedVertex.SetMark(mark);
            MarkedVertex targetMarkedVertex = MarkedVertex.Find(targetVertex);
            MarkedVertex currentMarkedVertex = startMarkedVertex;

            while (!targetMarkedVertex.IsMarked)
            {
                foreach (MarkedVertex markedVertex in markedVertices)
                {
                    if (markedVertex.Mark == mark)
                    {
                        currentMarkedVertex = markedVertex;
                        foreach (MarkedVertex markedVertexNeighbor in currentMarkedVertex.Neighbors)
                        {
                            if (!markedVertexNeighbor.IsMarked)
                                markedVertexNeighbor.SetMark(mark + 1);
                        }
                    }
                }
                mark++;
            }
            
            currentMarkedVertex = targetMarkedVertex;
            List<Vertex> pathList = new List<Vertex>();

            while (!currentMarkedVertex.Equals(startMarkedVertex))
            {
                foreach (MarkedVertex markedVertexNeighbor in currentMarkedVertex.Neighbors)
                {
                    if (markedVertexNeighbor.Mark + 1 == currentMarkedVertex.Mark)
                    {
                        pathList.Add(currentMarkedVertex.Vertex);
                        currentMarkedVertex = markedVertexNeighbor;
                        break;
                    }
                }
            }
            pathList.Add(currentMarkedVertex.Vertex);
            pathList.Reverse();

            Distance distance = new Distance(pathList.Count, pathList);
            distances.Add(distance);
        }

        return distances;
    }

    public override bool Equals(object obj)
    {
        Graph graph = obj as Graph;
        if (graph.EdgeCount != EdgeCount)
            return false;

        foreach (Edge edge in graph.edges)
        {
            if (!edges.Contains(edge))
                return false;
        }
        return true;
    }

    internal bool AddEdge(Edge edge)
    {
        if (edges.Contains(edge))
            return false;

        edge.From.AddAssociateEdge(edge);
        edge.To.AddAssociateEdge(edge);

        if (!vertices.Contains(edge.From))
            vertices.Add(edge.From);
        if (!vertices.Contains(edge.To))
            vertices.Add(edge.To);

        edges.Add(edge);

        return true;
    }

    internal bool AddUnit(Unit unit)
    {
        if (units.Contains(unit))
            return false;
        units.Add(unit);

        if (unit is Player)
            Player = unit;

        return true;
    }
}

public class Vertex
{
    public uint x, y;

    private Unit unit;
    private List<Vertex> neighbors;
    private List<Edge> associateEdges;

    public List<Vertex> Neighbors
    {
        get { return neighbors; }
    }

    public List<Edge> AssociateEdges
    {
        get { return associateEdges; }
    }

    public bool IsUnitHere
    {
        get { return unit != null; }
    }

    public Unit Unit
    {
        get { return unit; }
    }

    internal Vertex(uint x, uint y)
    {
        this.x = x;
        this.y = y;

        neighbors = new List<Vertex>();
        associateEdges = new List<Edge>();
    }

    public void SetUnit(Unit unit)
    {
        this.unit = unit;
    }

    public void RemoveUnit()
    {        
        unit = null;
    }

    public void KillUnit()
    {
        unit.Kill();
        RemoveUnit();
    }

    public void AddAssociateEdge(Edge edge)
    {
        if (!associateEdges.Contains(edge))
            associateEdges.Add(edge);

        if (edge.From.Equals(this) &&
            !neighbors.Contains(edge.To))
        {
            neighbors.Add(edge.To);
            return;
        }

        if (edge.To.Equals(this) &&
            !neighbors.Contains(edge.From))
            neighbors.Add(edge.From);
    }

    public override bool Equals(object obj)
    {
        Vertex vertex = obj as Vertex;
        return vertex.x == x && 
            vertex.y == y;
    }

    public override string ToString()
    {
        return String.Format("({0}; {1})", x, y);
    }
}

public class Edge
{
    public Vertex From, To;

    internal Edge(Vertex from, Vertex to)
    {
        From = from;
        To = to;
    }

    public override bool Equals(object obj)
    {
        Edge edge = obj as Edge;
        return (edge.From.Equals(From) &&
                edge.To.Equals(To)) ||
               (edge.From.Equals(To) &&
                edge.To.Equals(From));
    }

    public override string ToString()
    {
        return String.Format("{0} - {1}", From, To);
    }
}

public class Distance : IComparable
{
    public Vertex From
    {
        get { return vertices.First(); }
    }

    public Vertex To
    {
        get { return vertices.Last(); }
    }

    public readonly int Length;

    private readonly List<Vertex> vertices;

    public Distance(int distance, List<Vertex> vertices)
    {
        Length = distance;
        this.vertices = vertices;
    }

    public int CompareTo(object obj)
    {
        Distance distance = obj as Distance;
        return Length - distance.Length;
    }

    public Vertex this[int index]
    {
        get { return vertices[index]; }
    }
}

public class MarkedVertex
{
    public Vertex Vertex;    
    public int Mark;

   private static List<MarkedVertex> MarkedVertexList;

    public MarkedVertex(Vertex vertex)
    {
        this.Vertex = vertex;
        Mark = -1;
    }

    public bool IsMarked
    {
        get { return Mark != -1; }
    }

    public List<MarkedVertex> Neighbors
    {
        get
        {
            List<MarkedVertex> markedVertices = new List<MarkedVertex>();
            foreach (Vertex neighbor in Vertex.Neighbors)
            {
                markedVertices.Add(MarkedVertex.Find(neighbor));
            }
            return markedVertices;
        }
    }

    public void SetMark (int weightValue)
    {
        Mark = weightValue;
    }

    public static MarkedVertex Find(Vertex vertex)
    {
        foreach (MarkedVertex markedVertex in MarkedVertexList)
        {
            if (markedVertex.Vertex.Equals(vertex))
                return markedVertex;
        }
        throw new Exception();
    }

    public static List<MarkedVertex> CreateMarkedVertexList()
    {
        List<MarkedVertex> vertices = new List<MarkedVertex>();
        MarkedVertexList = vertices;
        return MarkedVertexList;
    }

    public override bool Equals(object obj)
    {
        MarkedVertex markedVertex = obj as MarkedVertex;
        return markedVertex.Vertex.Equals(Vertex);
    }
}