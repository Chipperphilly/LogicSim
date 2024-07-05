using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public bool isCreatingConnector;

    public Node hoveredNode;

    public Node startNode;
    public Node endNode;
    int currentConnectorID = 0;
    AudioManager audioManager;

    public List<Connector> connectors = new List<Connector>();


    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();   
    }

    public void BeginConnection(Node node)
    {
        startNode = node;
        isCreatingConnector = true;
    }

    public void CreateConnection()
    {
        endNode = hoveredNode;
        isCreatingConnector = false;

        if (endNode == null)
            return;

        if (startNode == endNode)
        {
            return;
        }

        if (startNode.nodeType == endNode.nodeType)
        {
            return;
        }

        Connector connector = startNode.nodeType == NodeType.Input ? new Connector(startNode, endNode, currentConnectorID) : new Connector(endNode, startNode, currentConnectorID);
        currentConnectorID++;
        if (startNode.connector != null)
            startNode.connector.RemoveConnector();
        if (endNode.connector != null)
            endNode.connector.RemoveConnector();

        endNode.connector = connector;
        startNode.connector = connector;

        startNode.connected = true;
        connectors.Add(connector);
        audioManager.ConnectNodes();
    }

    public void DestroyAllConnectors()
    {
        foreach (Connector connector in connectors)
        {
            connector.RemoveConnector();
        }
        connectors.Clear();
    }
}
