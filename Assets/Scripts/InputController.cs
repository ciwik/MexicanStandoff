using System;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private Touch firstTouch, currenTouch, lastTouch;

	void Update ()
	{
        if (!GraphView.Instance.CanTouch)
            return;

        if (Input.touchCount == 1)
	    {
            currenTouch = Input.GetTouch(0);

	        if (currenTouch.phase == TouchPhase.Began)
	            firstTouch = currenTouch;

            if (currenTouch.phase == TouchPhase.Ended)
            {
                lastTouch = currenTouch;
	            Edge edge = GetTouchedEdge();
                MainController.Move(edge);
                Action.Execute();
            }
	    }
	}

    private Edge GetTouchedEdge()
    {
        Vector2 direction = lastTouch.position - firstTouch.position;
        direction = direction.normalized;

        float dot = Vector3.Dot(direction, Vector3.right);
        if (dot <= 0.1f)
            direction *= GraphView.Instance.VerticalEdgeLength;
        else if (dot >= 0.9f)
            direction *= GraphView.Instance.HorizontalEdgeLength;
        else
            direction *= GraphView.Instance.DiagonalEdgeLength;

        Vector2 beginTouchPosition = firstTouch.position;
        Vertex beginTouchVertex = GetVertexFromTouchPosition(beginTouchPosition);

        Vector2 endTouchPosition = Camera.main.WorldToScreenPoint(
            GraphView.Instance.GetPosition(beginTouchVertex) 
                + new Vector3(direction.x, 0, direction.y));
        Vertex endTouchVertex = GetVertexFromTouchPosition(endTouchPosition);

        Edge edge = new Edge(beginTouchVertex, endTouchVertex);

        if (MainController.Graph.Edges.Contains(edge))
            return edge;

        throw new EdgeNotExistException();        
    }

    private Vertex GetVertexFromTouchPosition(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 touchPosition;
            touchPosition = hit.point;

            Vertex touchVertex = GraphView.Instance.GetVertex(touchPosition);
            return touchVertex;
        }
        throw new Exception("Wrong touch");        
    }
}
