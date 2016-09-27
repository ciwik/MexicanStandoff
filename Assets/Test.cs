using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
	void Start ()
	{
	    string fileName = "level";
        GraphParser.OpenFile(fileName);
	    Graph graph = GraphParser.GetGraph();

        GraphView.Instance.Draw(graph);

	    //Dictionary<Unit, List<Distance>> pathDictionary = graph.GetDistances();
	}	
}
