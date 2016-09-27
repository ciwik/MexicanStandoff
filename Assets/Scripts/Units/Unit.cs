using System;
using System.Collections.Generic;

public class Unit
{
    public Vertex CurrentVertex;
    public bool CanSkipAction;

    private int actionCount;

    public Unit()
    {
        actionCount = 0;
        CanSkipAction = true;
    }

    public void Init(Vertex vertex)
    {
        CurrentVertex = vertex;
        vertex.SetUnit(this);
    }

    public void Move(Vertex vertex)
    {
        CurrentVertex.RemoveUnit();
        CurrentVertex = vertex;
        vertex.SetUnit(this);

        //TODO events
        GraphView.Instance.ShowMove(this, new Edge(CurrentVertex, vertex));
    }

    public void Kill()
    {
        MainController.Graph.Units.Remove(this);
        GraphView.Instance.KillUnit(this);
    }

    public static Unit operator ++(Unit unit)
    {
        unit.actionCount++;
        return unit;
    }

    public override string ToString()
    {
        return String.Format("{0}: {1}", this.GetType().Name, CurrentVertex);
    }
}
