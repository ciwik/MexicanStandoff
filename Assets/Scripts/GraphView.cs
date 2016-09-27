using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphView : MonoBehaviour, IGraphView
{
    public GameObject Plane;
    public GameObject UnitPrefab;

    private static GraphView instance;

    public static GraphView Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        instance = this;
        Init();
    }    

    private const int edgeDefaultThin = 16;
    private const int planeSize = 10;
    private const float R = 1.25f;

    public float HorizontalEdgeLength = -1, 
        VerticalEdgeLength = -1, 
        DiagonalEdgeLength = -1;

    private Texture2D texture;
    private Material material;
    private Color color;

    private Dictionary<Vertex, Vector2> vertex2PositionOnTex;

    private Graph graph;

    private int vertexWidth, vertexHeight, vertexRadius, vertexRadius2;

    public Dictionary<Vertex, Vector3> Vertex2Position;
    public Dictionary<Vector3, Vertex> Position2Vertex;

    public Dictionary<Unit, GameObject> Unit2GO;
    public Dictionary<GameObject, Unit> GO2Unit;

    public Vector3 GetPosition(Vertex vertex)
    {
        Vector3 res;
        if (Vertex2Position.TryGetValue(vertex, out res))
            return res;

        throw new VertexNotExistException(vertex);
    }

    public Vertex GetVertex(Vector3 position)
    {
        Vertex res;
        if (Position2Vertex.TryGetValue(position, out res))
            return res;

        foreach (Vector3 vertexPosition in Position2Vertex.Keys)
        {
            if (Vector3.Distance(position, vertexPosition) <= R)
            {
                Position2Vertex.TryGetValue(vertexPosition, out res);
                return res;
            }                
        }

        throw new VertexNotExistException(position);
    }

    public void Draw(Graph graph)
    {
        this.graph = graph;

        vertexWidth = (int)(texture.width/graph.Width);
        vertexHeight = (int)(texture.height / graph.Height);
        
        vertexRadius = Mathf.Min(vertexWidth, vertexHeight) / 8;
        vertexRadius2 = vertexRadius * vertexRadius;

        HorizontalEdgeLength = planeSize/(float)graph.Width;
        VerticalEdgeLength = planeSize/(float)graph.Height;
        DiagonalEdgeLength = Mathf.Sqrt(HorizontalEdgeLength*HorizontalEdgeLength +
                                        VerticalEdgeLength*VerticalEdgeLength);

        vertex2PositionOnTex = new Dictionary<Vertex, Vector2>();

        Vertex2Position = new Dictionary<Vertex, Vector3>();
        Position2Vertex = new Dictionary<Vector3, Vertex>();

        Unit2GO = new Dictionary<Unit, GameObject>();
        GO2Unit = new Dictionary<GameObject, Unit>();

        foreach (Vertex vertex in graph.Vertices)
        {
            DrawVertex(vertex);
        }

        foreach (Edge edge in graph.Edges)
        {
            DrawEdge(edge);
        }

        foreach (Unit unit in graph.Units)
        {
            DrawUnit(unit);
        }

        vertex2PositionOnTex = null;
    }

    public void DrawVertex(Vertex vertex)
    {
        int posX = (int)(vertex.x * vertexWidth);
        int posY = (int)(vertex.y * vertexHeight);

        Color[] colors = texture.GetPixels(posX, posY, vertexWidth, vertexHeight);
        int centerX = posX + vertexWidth / 2;
        int centerY = posY + vertexHeight / 2;

        int localCenterX = centerX - posX;
        int localCenterY = centerY - posY;

        for (int x = 0; x < vertexWidth; x++)
        {
            for (int y = 0; y < vertexHeight; y++)
            {
                if ((x - localCenterX)*(x - localCenterX) +
                    (y - localCenterY)*(y - localCenterY) <= vertexRadius2)
                {
                    colors[(y - 1)*vertexWidth + x] = color;
                }
            }
        }

        vertex2PositionOnTex.Add(vertex, new Vector2(centerX, centerY));

        Vector3 positionInWorld = new Vector3((float)centerX / (float)texture.width, 
            0,
            (float)(texture.height - centerY) / (float)texture.width) * planeSize;
        positionInWorld -= new Vector3(1, 0, 1) * planeSize/2f;
        Vertex2Position.Add(vertex, positionInWorld);
        Position2Vertex.Add(positionInWorld, vertex);

        texture.SetPixels(posX, posY, vertexWidth, vertexHeight, colors);
        SetTexture();
    }

    public void DrawEdge(Edge edge)
    {
        Vector2 from, to;
        vertex2PositionOnTex.TryGetValue(edge.From, out from);
        vertex2PositionOnTex.TryGetValue(edge.To, out to);

        int posX = (int)Mathf.Min(from.x, to.x);
        int posY = (int)Mathf.Min(from.y, to.y);

        Vector2 dPos = to - from;
        int edgeWidth = (int)Mathf.Abs(dPos.x);
        int edgeHeight = (int)Mathf.Abs(dPos.y);

        Color[] colors = null;
        bool paintOver = false;
        Rect rect = new Rect();
        if (edgeWidth == 0)
        {
            rect = new Rect(posX - edgeDefaultThin / 2,
                posY,
                edgeDefaultThin,
                edgeHeight);            
            paintOver = true;
        }

        if (edgeHeight == 0)
        {
            rect = new Rect(posX,
                posY - edgeDefaultThin/2,
                edgeWidth,
                edgeDefaultThin);
            paintOver = true;
        }

        if (paintOver)
        {
            colors = texture.GetPixels((int) rect.x, (int) rect.y,
                (int) rect.width, (int) rect.height);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
        }
        else
        {
            rect = new Rect(posX, posY, edgeWidth, edgeHeight);
            colors = texture.GetPixels((int)rect.x, (int)rect.y,
                (int)rect.width, (int)rect.height);

            int x, y;
            float xf, yf;
            int leftX, rightX;
            bool up;

            dPos = dPos.normalized;
            if (dPos.x > 0)
            {
                x = posX;
                xf = posX;
                up = true;
            }
            else
            {
                x = posX + edgeWidth;
                xf = posX + edgeWidth;
                up = false;
            }
            y = posY;
            yf = posY;

            while ((up && x < posX + edgeWidth) ||
                (!up && x > posX))
            {
                leftX = x - edgeDefaultThin / 2;
                if (leftX < posX)
                    leftX = posX;
                rightX = x + edgeDefaultThin / 2;
                if (rightX > posX + edgeWidth)
                    rightX = posX + edgeWidth;
                for (int dx = leftX; dx < rightX; dx++)
                {
                    colors[(y - posY) * edgeWidth + dx - posX] = color;
                }

                xf += dPos.x;
                yf += dPos.y;
                x = (int)xf;
                y = (int) yf;
            }
        }

        texture.SetPixels((int)rect.x, (int)rect.y,
                (int)rect.width, (int)rect.height, colors);
        SetTexture();
    }

    public void DrawUnit(Unit unit)
    {
        Vector3 position = GetPosition(unit.CurrentVertex);

        GameObject unitGO = 
            (GameObject)Instantiate(UnitPrefab, position, Quaternion.identity);

        //TODO
        if (unit is Player)
            unitGO.GetComponent<Renderer>().material.color = Color.red;

        Unit2GO.Add(unit, unitGO);
        GO2Unit.Add(unitGO, unit);
    }

    private void SetTexture()
    {
        texture.Apply(false);
        material.SetTexture("_MainTex", texture);
    }

    public void Init()
    {
        material = Plane.GetComponent<Renderer>().material;
        texture = Instantiate(material.GetTexture("_MainTex") as Texture2D);
        material.SetTextureScale("_MainTex", new Vector2(-1, 1));
        color = Color.black;
    }

    public bool CanTouch = true;
        
    private IEnumerator ShowMoveAsync(GameObject unit, Vector3 from, Vector3 to)
    {
        CanTouch = false;
        //TODO
        float k = 0, dk = 0.05f;
        while (k <= 1f)
        {
            unit.transform.position = Vector3.Lerp(from, to, k);
            k += dk;
            yield return new WaitForEndOfFrame();            
        }
        unit.transform.position = to;
        CanTouch = true;
    }

    public void ShowMove(Unit unit, Edge edge)
    {
        GameObject unitGO;
        Unit2GO.TryGetValue(unit, out unitGO);
        Vector3 from = GetPosition(edge.From);
        Vector3 to = GetPosition(edge.To);

        StartCoroutine(ShowMoveAsync(unitGO, from, to));
    }

    public void KillUnit(Unit unit)
    {
        GameObject unitGO;
        Unit2GO.TryGetValue(unit, out unitGO);
        Unit2GO.Remove(unit);
        GO2Unit.Remove(unitGO);

        Destroy(unitGO);
    }
}
