using System;
using UnityEngine;

public class MainController : MonoBehaviour
{
    public static Graph Graph;

    public static void Move(Edge edge)
    {
        if (Graph.Player.CurrentVertex.Equals(edge.From))
        {
            if (edge.To.IsUnitHere)
                edge.To.KillUnit();
            Graph.Player.Move(edge.To);
            //GraphView.Instance.ShowMove(Graph.Player, edge);            
        }
        else
        {
            throw new Exception("No player on start vertex");
        }
    }
}
