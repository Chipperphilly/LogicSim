using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;

public class Node : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public NodeValue value = NodeValue.Undefined;

    public Connector connector;

    public int nodeID;

    LineRenderer lineRenderer;

    NodeManager nodeManager;
    AudioManager audioManager;
    BlockManager blockManager;

    public bool connected = false;
    public bool isMasterNode = false;

    bool mouseOver = false;

    public NodeType nodeType;

    Vector2 initialDragMousePos;
    Vector2 initialDragImagePos;

    public LogicBlock parentBlock;

    public int depth = -1;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        nodeManager = FindAnyObjectByType<NodeManager>();
        audioManager = FindObjectOfType<AudioManager>();
        blockManager = FindObjectOfType<BlockManager>();
    }

    private void Update()
    {
        if (connected)
        {
            Vector3 startPos = Camera.main.ScreenToWorldPoint(connector.inputNode.gameObject.transform.position);
            startPos.Set(startPos.x, startPos.y, 0);
            Vector3 endPos = Camera.main.ScreenToWorldPoint(connector.outputNode.gameObject.transform.position);
            endPos.Set(endPos.x, endPos.y, 0);
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            if (value == NodeValue.True)
            {
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
            }
            else
            {
                lineRenderer.startColor = Color.black;
                lineRenderer.endColor = Color.black;
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
        if (isMasterNode && mouseOver && Input.GetKeyDown(KeyCode.Backspace))
        {
            DestroyNode();
        }
    }

    public void RemoveConnector()
    {
        if (connector == null)
            return;
        nodeManager.connectors.Remove(connector);
        connector.RemoveConnector();
    }

    public void PropagateDepth()
    {
        if (nodeType == NodeType.Input)
        {
            if (parentBlock != null)
            {
                parentBlock.PropagateDepth(depth);
            }
        }
        else
        {
            if (connector != null)
            {
                connector.depth = depth;
                connector.PropagateDepth();
            }
        }
    }

    public void ToggleNode()
    {
        if (nodeType == NodeType.Input)
        {
            return;
        }

        audioManager.ToggleNode();
        value = value == NodeValue.True ? NodeValue.False : NodeValue.True;
        
        GetComponent<Image>().color = value == NodeValue.True ? Color.black : Color.white;


        if (connector != null)
            connector.PassValue(value);
    }

    public void SetNodeValue(NodeValue nodeValue)
    {
        value = nodeValue;

        if (nodeType == NodeType.Input && parentBlock != null)
        {
            parentBlock.Refresh();
        }

        if (connector != null && nodeType == NodeType.Output && parentBlock != null)
        {
            connector.PassValue(nodeValue);
        }

        if (isMasterNode)
        {
            GetComponent<Image>().color = value == NodeValue.True ? Color.black : Color.white;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        nodeManager.BeginConnection(this);

        if (isMasterNode)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                initialDragMousePos = eventData.position;
                initialDragImagePos = transform.position;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        nodeManager.CreateConnection();
        lineRenderer.positionCount = 2;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMasterNode)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                transform.position = initialDragImagePos + eventData.position - initialDragMousePos;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isMasterNode)
            GetComponent<Image>().color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 0.2f);
        nodeManager.hoveredNode = this;
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isMasterNode)
            GetComponent<Image>().color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 0);
        nodeManager.hoveredNode = null;
        mouseOver = false;
    }

    public string SaveNode()
    {
        string json;
        json = "\"" + nodeID + "\"" + ": {\"depth\": " + depth + ", \"nodeType\": " + (int)nodeType + ", \"connectorID\":" + (connector != null ? connector.connectorID.ToString() : "-1") + ",\"parentBlockID\":" + (parentBlock != null ? parentBlock.blockID.ToString() : "-1") + ", \"masterNode\": " + (isMasterNode ? "true" : "false") + "}";
        return json;
    }

    public void DestroyNode()
    {
        if (isMasterNode)
        {
            blockManager.blocksExisting[nodeType == NodeType.Input ? "input" : "output"]--;
        }
        RemoveConnector();
        Destroy(gameObject);
    }
}

public enum NodeType
{
    Input = 0,
    Output = 1
}

public enum NodeValue
{
    True,
    False,
    Undefined
}
