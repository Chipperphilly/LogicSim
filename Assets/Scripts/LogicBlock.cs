using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.FullSerializer;

[RequireComponent(typeof(Image))]
public class LogicBlock : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler,IPointerEnterHandler, IPointerExitHandler
{
    public string blockName;
    public int blockID;

    [SerializeField]
    public TextMeshProUGUI blockNameText;

    Vector2 initialDragMousePos;
    Vector2 initialDragImagePos;

    static int maxBlockWidth = 155;

    public Node[] inputNodes;
    public Node[] outputNodes;

    public int depth = -1;

    BlockManager blockManager;

    bool mouseOver = false;
    [SerializeField]
    bool isPremade;

    public void Refresh()
    {
        NodeValue[] inputs = new NodeValue[inputNodes.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = inputNodes[i].value;
        }

        NodeValue[] vals;

        // Run the script for the premade block
        switch (blockName.ToLower())
        {
            case "led":
                vals = GetComponent<LEDScript>().Refresh(inputs);
                break;
            case "segment display":
                vals = GetComponent<SegmentDisplayScript>().Refresh(inputs);
                break;
            case "num to segment":
                vals = GetComponent<NumToSegmentScript>().Refresh(inputs);
                break;
            case "random":
                vals = GetComponent<RandomBlockScript>().Refresh(inputs);
                break;

            default: 
                vals = blockManager.RunBlock(blockName, inputs);
                break;
        }

        
        
        for (int i = 0;i < vals.Length; i++)
        {
            outputNodes[i].SetNodeValue(vals[i]);
        }

    }

    public void PropagateDepth(int depthToSet)
    {
        int greatestDepth = 0;
        foreach (Node node in inputNodes)
        {
            if (node.depth == -1)
            {
                return;
            }
            else
            {
                greatestDepth = Mathf.Max(greatestDepth, node.depth);
            }
        }
        depth = greatestDepth;
        print(blockName + ":" + blockID + "," + "block set as " + depth);
        foreach (Node node in outputNodes)
        {
            node.depth = depth + 1;
            node.PropagateDepth();
        }
    }

    public void OnBeginDrag(PointerEventData pointer)
    {
        initialDragMousePos = pointer.position;
        initialDragImagePos = transform.position;
    }

    public void OnEndDrag(PointerEventData pointer)
    {

    }

    public void OnDrag(PointerEventData pointer)
    {
        transform.position = initialDragImagePos + pointer.position - initialDragMousePos;
    }

    public void OnPointerEnter(PointerEventData pointer)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        mouseOver = false;
    }

    public void ResizeBlock()
    {
        blockNameText.text = blockName.ToLower() == "split" ? ":" : blockName;
        float height = Mathf.Max(blockNameText.preferredHeight + 10, GetComponent<RectTransform>().sizeDelta.y);
        float width = Mathf.Min(maxBlockWidth, blockNameText.preferredWidth + 30);
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

    public void DeleteBlock()
    {
        foreach (Node node in inputNodes)
        {
            if (node.connector != null)
            {
                node.RemoveConnector();
            }
            blockManager.nodes.Remove(node);
            Destroy(node.gameObject);
        }
        foreach (Node node in outputNodes)
        {
            if (node.connector != null)
            {
                node.RemoveConnector();
            }
            blockManager.nodes.Remove(node);
            Destroy(node.gameObject);
        }
        blockManager.blocksExisting[blockName.ToLower()]--;
        blockManager.blocks.Remove(this);
        Destroy(gameObject);
    }

    void Start()
    {
        blockManager = FindAnyObjectByType<BlockManager>();
        if (!isPremade)
            ResizeBlock();
    }

    void Update()
    {
        if (mouseOver && Input.GetKeyDown(KeyCode.Backspace))
        {
            DeleteBlock();
        }
    }

    public string SaveBlock()
    {
        string json = "\"" + blockID + "\"" + ": {\"name\":\"" + blockName + "\", \"depth\": " + depth + ", \"inputs\": [";
        foreach (Node node in inputNodes)
        {
            json += node.nodeID.ToString() + ",";
        }
        json = json.Remove(json.Length - 1);
        json += "], \"outputs\": [";
        foreach (Node node in outputNodes)
        {
            json += node.nodeID.ToString() + ",";
        }
        json = json.Remove(json.Length - 1);
        json += "]}";
        return json;
    }
}
