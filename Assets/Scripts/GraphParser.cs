using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class GraphParser
{
    private static XmlDocument xmlDocument;
    private static Dictionary<string, Vertex> id2VertexDictionary;

    public static void OpenFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(textAsset.text);
    }

    public static Graph GetGraph()
    {        
        XmlNode node;

        node = xmlDocument.DocumentElement.SelectSingleNode("/Graph/VertexCount");
        uint vertexCount = UInt32.Parse(node.InnerText);

        node = xmlDocument.DocumentElement.SelectSingleNode("/Graph/EdgeCount");
        uint edgeCount = UInt32.Parse(node.InnerText);

        node = xmlDocument.DocumentElement.SelectSingleNode("/Graph/GraphSize");
        uint sizeX = UInt32.Parse(node.Attributes[0].Value);
        uint sizeY = UInt32.Parse(node.Attributes[1].Value);

        Graph graph = Graph.Create(sizeX, sizeY);

        {
            node = xmlDocument.DocumentElement.SelectSingleNode("/Graph/Vertices");
            uint x, y;
            string id;
            id2VertexDictionary = new Dictionary<string, Vertex>();
            foreach (XmlNode subNode in node.ChildNodes)
            {
                id = subNode.Attributes[0].Value;
                x = UInt32.Parse(subNode.Attributes[1].Value);
                y = UInt32.Parse(subNode.Attributes[2].Value);

                Vertex vertex = new Vertex(x, y);
                id2VertexDictionary.Add(id, vertex);
            }
        }

        {
            node = xmlDocument.DocumentElement.SelectSingleNode("/Graph/Edges");
            string fromId, toId;
            Vertex from, to;
            foreach (XmlNode subNode in node.ChildNodes)
            {
                fromId = subNode.Attributes[0].Value;
                toId = subNode.Attributes[1].Value;

                Edge edge = null;
                if (id2VertexDictionary.TryGetValue(fromId, out from) &&
                    id2VertexDictionary.TryGetValue(toId, out to))
                {
                    edge = new Edge(from, to);
                    graph.AddEdge(edge);
                }
                else
                {
                    Debug.LogError("Error in XML: cannot find vertex by id");
                }
            }
        }

        {
            node = xmlDocument.DocumentElement.SelectSingleNode("/Graph/Units");
            string vertexId;
            bool isPlayer;
            Vertex vertex;
            foreach (XmlNode subNode in node.ChildNodes)
            {
                vertexId = subNode.Attributes[0].Value;
                id2VertexDictionary.TryGetValue(vertexId, out vertex);

                Unit unit;
                isPlayer = Boolean.Parse(subNode.Attributes[1].Value);

                if (isPlayer)
                    unit = new Player();
                else
                    unit = new Enemy();

                unit.Init(vertex);

                graph.AddUnit(unit);
            }
        }
        id2VertexDictionary = null;
        xmlDocument = null;

        return graph;
    }
}
