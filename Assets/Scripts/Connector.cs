using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector
{
    public Node inputNode;
    public Node outputNode;
    public int connectorID;
    public int depth = -1;

    public Connector(Node inputNode, Node outputNode, int connectorID)
    {
        this.inputNode = inputNode;
        this.outputNode = outputNode;
        this.connectorID = connectorID;
    }

    public void RemoveConnector()
    {
        inputNode.connector = null;
        inputNode.connected = false;
        outputNode.connector = null;
        outputNode.connected = false;
    }

    public void PropagateDepth()
    {
        inputNode.depth = depth;

        inputNode.PropagateDepth();
    }

    public void PassValue(NodeValue value)
    {
        inputNode.SetNodeValue(value);
    }

    public string SaveConnector()
    {
        string json = "\"" + connectorID + "\"" + ": {\"depth\": " + depth + ", \"start\": " + outputNode.nodeID + ",\"end\": " + inputNode.nodeID + "}";
        return json;
    }
}
