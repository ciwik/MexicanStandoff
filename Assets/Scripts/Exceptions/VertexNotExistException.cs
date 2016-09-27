using System;
using UnityEngine;

public class VertexNotExistException : Exception
{
    private Vertex vertex;
    private Vector3 position;

    public override string Message
    {
        get
        {
            if (vertex == null)
                return String.Format("No vertex for position {0}", position);
            else
                return String.Format("No position for vertex {0}", vertex);
        }
    }

    public VertexNotExistException(Vector3 position)
    {
        this.position = position;
    }

    public VertexNotExistException(Vertex vertex)
    {
        this.vertex = vertex;
    }
}
