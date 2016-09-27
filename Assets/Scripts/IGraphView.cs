using UnityEngine;

public interface IGraphView
{
    Vector3 GetPosition(Vertex vertex);
    Vertex GetVertex(Vector3 position);

    void Draw(Graph graph);
    void DrawVertex(Vertex vertex);
    void DrawEdge(Edge edge);
    void DrawUnit(Unit unit);

    void Init();
}
