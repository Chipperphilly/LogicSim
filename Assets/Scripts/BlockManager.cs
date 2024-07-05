using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField]
    GameObject logicBlockObject;
    [SerializeField]
    GameObject nodeObject;
    [SerializeField]
    GameObject masterNodeObject;

    [SerializeField]
    GameObject blockHolder;

    static int nodeBuffer = 10;
    static int nodeSpacing = 10;
    static int nodeSize = 10;

    int currentNodeID = 0;
    int currentBlockID = 0;

    AudioManager audioManager;

    public List<LogicBlock> blocks = new List<LogicBlock>();
    public List<Node> nodes = new List<Node>();

    Dictionary<string, GameObject> premadeBlocks;
    [SerializeField]
    List<string> premadeBlockNames;
    [SerializeField]
    List<GameObject> premadeBlockObjects;

    GameManager gameManager;

    public Dictionary<string, int> blocksExisting;

    
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        premadeBlocks = new Dictionary<string, GameObject>();
        audioManager = FindObjectOfType<AudioManager>();
        for (int i = 0; i < premadeBlockNames.Count; i++)
        {
            premadeBlocks.Add(premadeBlockNames[i], premadeBlockObjects[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public NodeValue[] RunBlock(string blockName, NodeValue[] inputs)
    {
        // Testing for pre-coded blocks
        switch (blockName.ToLower())
        {
            case "nand":
                return new NodeValue[] { (inputs[1] == NodeValue.True && inputs[0] == NodeValue.True) ? NodeValue.False : NodeValue.True };
            case "split":
                return new NodeValue[] { inputs[0], inputs[0] };
            case "led":
                // Run function
                return new NodeValue[0];
        }


        string data = File.ReadAllText(Application.persistentDataPath + "/blocks/" + blockName + ".json");
        fsData dataJson = fsJsonParser.Parse(data);

        Dictionary<string, NodeValue> nodeValues = new Dictionary<string, NodeValue>();
        Dictionary<int, List<string>> nodeDepths = new Dictionary<int, List<string>>();
        Dictionary<int, List<string>> blockDepths = new Dictionary<int, List<string>>();

        // Preparing Nodes
        foreach (KeyValuePair<string, fsData> node in dataJson.AsDictionary["nodes"].AsDictionary)
        {
            nodeValues.Add(node.Key, NodeValue.Undefined);
            int depth = (int)node.Value.AsDictionary["depth"].AsInt64;
            if (!nodeDepths.ContainsKey(depth))
            {
                nodeDepths.Add(depth, new List<string>());
            }
            nodeDepths[depth].Add(node.Key);
        }

        int inputIndex = 0;
        foreach (KeyValuePair<string, fsData> node in dataJson.AsDictionary["inputs"].AsDictionary)
        {
            nodeValues.Add(node.Key, inputs[inputIndex]);
            int depth = (int)node.Value.AsDictionary["depth"].AsInt64;
            if (!nodeDepths.ContainsKey(depth))
            {
                nodeDepths.Add(depth, new List<string>());
            }
            nodeDepths[depth].Add(node.Key);
        }
        // Placing blocks in dictionaries of depth
        foreach (KeyValuePair<string, fsData> block in dataJson.AsDictionary["blocks"].AsDictionary)
        {
            int depth = (int)block.Value.AsDictionary["depth"].AsInt64;
            if (!blockDepths.ContainsKey(depth))
            {
                blockDepths.Add(depth, new List<string>());
            }
            blockDepths[depth].Add(block.Key);
        }

        // Doing block function
        int currentDepth = 0;
        while (currentDepth < nodeDepths.Count)
        {
            // On first iteration we use the master nodes
            if (currentDepth == 0)
            {
                int nodeIdx = 0;
                foreach (KeyValuePair<string, fsData> node in dataJson.AsDictionary["inputs"].AsDictionary)
                {
                    string connector = node.Value.AsDictionary["connectorID"].ToString();
                    string connectedNode = dataJson.AsDictionary["connectors"].AsDictionary[connector].AsDictionary["end"].AsInt64.ToString();
                    nodeValues[connectedNode] = inputs[nodeIdx];
                    nodeIdx++;
                }
            }
            else
            {
                // Pass on the outputs of all nodes that have just been updated
                foreach (string nodeID in nodeDepths[currentDepth])
                {
                    fsData node = dataJson.AsDictionary["nodes"].AsDictionary[nodeID];
                    // Only pass on values if the node is an output node meaning it has just been updated
                    if ((int)node.AsDictionary["nodeType"].AsInt64 == (int)NodeType.Output)
                    {
                        string connector = node.AsDictionary["connectorID"].ToString();
                        string connectedNode = dataJson.AsDictionary["connectors"].AsDictionary[connector].AsDictionary["end"].AsInt64.ToString();
                        nodeValues[connectedNode] = nodeValues[nodeID];
                    }
                }
            }

            // Always run blocks last
            if (blockDepths.ContainsKey(currentDepth))
            {
                foreach (string blockID in blockDepths[currentDepth])
                {
                    fsData block = dataJson.AsDictionary["blocks"].AsDictionary[blockID];

                    string nameOfBlock = block.AsDictionary["name"].AsString;
                    NodeValue[] valuesOfInputs = new NodeValue[block.AsDictionary["inputs"].AsList.Count];
                    for (int i = 0; i < valuesOfInputs.Length; i++)
                    {
                        valuesOfInputs[i] = nodeValues[block.AsDictionary["inputs"].AsList[i].AsInt64.ToString()];
                    }

                    NodeValue[] vals = RunBlock(nameOfBlock, valuesOfInputs);
                    for (int i = 0; i < vals.Length; i++)
                    {
                        nodeValues[block.AsDictionary["outputs"].AsList[i].ToString()] = vals[i];
                    }
                }
            }
            currentDepth++;
        }

        NodeValue[] outputs = new NodeValue[dataJson.AsDictionary["outputs"].AsDictionary.Count];

        for (int i = 0;i < outputs.Length; i++) 
        {
            outputs[i] = nodeValues[dataJson.AsDictionary["outputs"].AsDictionary.Keys.ToArray<string>()[i]];
        }
        return outputs;
    }

    public GameObject CreateMasterNode(NodeType nodeType)
    {
        GameObject node = Instantiate(masterNodeObject);
        node.GetComponent<Node>().nodeType = nodeType;
        node.transform.SetParent(blockHolder.transform, false);
        node.GetComponent<RectTransform>().anchoredPosition = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50));
        node.GetComponent<Node>().nodeID = currentNodeID;
        currentNodeID++;
        if (nodeType == NodeType.Output)
        {
            gameManager.inputNodes.Add(node.GetComponent<Node>());
        }
        else
        {
            gameManager.outputNodes.Add(node.GetComponent<Node>());
        }
        return node;
    }

    public GameObject CreatePremadeBlock(string blockName)
    {
        GameObject block = Instantiate(premadeBlocks[blockName]);
        block.transform.SetParent(blockHolder.transform, false);
        block.GetComponent<RectTransform>().anchoredPosition = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50));
        LogicBlock logicBlock = block.GetComponent<LogicBlock>();
        logicBlock.blockName = blockName.ToUpper();
        logicBlock.blockID = currentBlockID;
        currentBlockID++;
        blocks.Add(logicBlock);
        int numInputs;
        int numOutputs;

        switch (blockName.ToLower())
        {
            case "led":
                numInputs = 1;
                numOutputs = 0;
                break;
            case "segment display":
                numInputs = 7;
                numOutputs = 0;
                break;
            case "num to segment":
                numInputs = 4;
                numOutputs = 7;
                break;
            case "random":
                numInputs = 1;
                numOutputs = 1;
                break;
            default:
                numInputs = 0;
                numOutputs = 0;
                break;
        }
        logicBlock.inputNodes = new Node[numInputs];
        logicBlock.outputNodes = new Node[numOutputs];

        // Minimum heights to fit all nodes
        int inputMinHeight = 2 * nodeBuffer + (numInputs - 1) * (2 * nodeSpacing + nodeSize);
        int outputMinHeight = 2 * nodeBuffer + (numOutputs - 1) * (2 * nodeSpacing + nodeSize);

        int minBlockHeight = Mathf.Max(inputMinHeight, outputMinHeight);

        // If all the nodes already fit spread them out
        if (minBlockHeight >= logicBlock.blockNameText.preferredHeight + 10)
        {
            block.GetComponent<RectTransform>().sizeDelta = new Vector2(block.GetComponent<RectTransform>().sizeDelta.x, minBlockHeight);
        }

        float spaceBetweenNodes = (block.GetComponent<RectTransform>().sizeDelta.y - nodeBuffer * 2) / (numInputs);
        float offsetY = logicBlock.GetComponent<RectTransform>().sizeDelta.y / 2;
        float offsetX = logicBlock.GetComponent<RectTransform>().sizeDelta.x / 2;
        for (int i = 0; i < numInputs; i++)
        {
            GameObject node = Instantiate(nodeObject);
            node.transform.SetParent(logicBlock.transform, false);
            node.GetComponent<RectTransform>().anchoredPosition = new Vector2(-offsetX, nodeBuffer + spaceBetweenNodes / 2 + spaceBetweenNodes * i - offsetY);
            node.GetComponent<Node>().nodeType = NodeType.Input;
            node.GetComponent<Node>().parentBlock = logicBlock;
            node.GetComponent<Node>().nodeID = currentNodeID;
            currentNodeID++;
            logicBlock.inputNodes[i] = node.GetComponent<Node>();
            nodes.Add(node.GetComponent<Node>());

        }
        spaceBetweenNodes = (block.GetComponent<RectTransform>().sizeDelta.y - nodeSize * 2) / (numOutputs);
        for (int i = 0; i < numOutputs; i++)
        {
            GameObject node = Instantiate(nodeObject);
            node.transform.SetParent(logicBlock.transform, false);
            node.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetX, nodeBuffer + spaceBetweenNodes / 2 + spaceBetweenNodes * i - offsetY);
            node.GetComponent<Node>().nodeType = NodeType.Output;
            node.GetComponent<Node>().parentBlock = logicBlock;
            node.GetComponent<Node>().nodeID = currentNodeID;
            currentNodeID++;
            logicBlock.outputNodes[i] = node.GetComponent<Node>();
            nodes.Add(node.GetComponent<Node>());
        }
        return block;
    }

    public GameObject CreateBlock(string blockName)
    {
        audioManager.CreateObject();

        
        if (!blocksExisting.ContainsKey(blockName.ToLower()))
        {
            blocksExisting.Add(blockName.ToLower(), 0);
        }
        blocksExisting[blockName.ToLower()] += 1;

        // Check if the block is a custom block
        if (premadeBlocks.ContainsKey(blockName))
        {
            return CreatePremadeBlock(blockName);
        }

        int numInputs;
        int numOutputs;

        switch (blockName.ToLower())
        {
            case "nand":
                numInputs = 2;
                numOutputs = 1;
                break;
            case "split":
                numInputs = 1;
                numOutputs = 2;
                break;
            case "input":
                return CreateMasterNode(NodeType.Output);
            case "output":
                return CreateMasterNode(NodeType.Input);
            default:
                string data = File.ReadAllText(Application.persistentDataPath + "/blocks/" + blockName + ".json");
                fsData dataJson = fsJsonParser.Parse(data);
                numInputs = dataJson.AsDictionary["inputs"].AsDictionary.Count;
                numOutputs = dataJson.AsDictionary["outputs"].AsDictionary.Count;
                break;
        }

        GameObject block = Instantiate(logicBlockObject);
        block.transform.SetParent(blockHolder.transform, false);
        block.GetComponent<RectTransform>().anchoredPosition = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50));
        LogicBlock logicBlock = block.GetComponent<LogicBlock>();
        logicBlock.blockName = blockName.ToUpper();
        logicBlock.blockID = currentBlockID;
        currentBlockID++;
        blocks.Add(logicBlock);        

        logicBlock.ResizeBlock();

        logicBlock.inputNodes = new Node[numInputs];
        logicBlock.outputNodes = new Node[numOutputs];

        // Minimum heights to fit all nodes
        int inputMinHeight = 2 * nodeBuffer + (numInputs - 1) * (2 * nodeSpacing + nodeSize);
        int outputMinHeight = 2 * nodeBuffer + (numOutputs - 1) * (2 * nodeSpacing + nodeSize);

        int minBlockHeight = Mathf.Max(inputMinHeight, outputMinHeight);

        // If all the nodes already fit spread them out
        if (minBlockHeight >= logicBlock.blockNameText.preferredHeight + 10)
        {
            block.GetComponent<RectTransform>().sizeDelta = new Vector2(block.GetComponent<RectTransform>().sizeDelta.x, minBlockHeight);
        }

        float spaceBetweenNodes = (block.GetComponent<RectTransform>().sizeDelta.y - nodeBuffer * 2) / (numInputs);
        float offsetY = logicBlock.GetComponent<RectTransform>().sizeDelta.y / 2;
        float offsetX = logicBlock.GetComponent<RectTransform>().sizeDelta.x / 2;
        for (int i = 0; i < numInputs; i++)
        {
            GameObject node = Instantiate(nodeObject);
            node.transform.SetParent(logicBlock.transform, false);
            node.GetComponent<RectTransform>().anchoredPosition = new Vector2(-offsetX, nodeBuffer + spaceBetweenNodes / 2 + spaceBetweenNodes * i - offsetY);
            node.GetComponent<Node>().nodeType = NodeType.Input;
            node.GetComponent<Node>().parentBlock = logicBlock;
            node.GetComponent<Node>().nodeID = currentNodeID;
            currentNodeID++;
            logicBlock.inputNodes[i] = node.GetComponent<Node>();
            nodes.Add(node.GetComponent<Node>());

        }
        spaceBetweenNodes = (block.GetComponent<RectTransform>().sizeDelta.y - nodeSize * 2) / (numOutputs);
        for (int i = 0; i < numOutputs; i++)
        {
            GameObject node = Instantiate(nodeObject);
            node.transform.SetParent(logicBlock.transform, false);
            node.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetX, nodeBuffer + spaceBetweenNodes / 2 + spaceBetweenNodes * i - offsetY);
            node.GetComponent<Node>().nodeType = NodeType.Output;
            node.GetComponent<Node>().parentBlock = logicBlock;
            node.GetComponent<Node>().nodeID = currentNodeID;
            currentNodeID++;
            logicBlock.outputNodes[i] = node.GetComponent<Node>();
            nodes.Add(node.GetComponent<Node>());
        }
        return block;
    }

    public void DestroyAllNodes()
    {
        foreach (Node node in nodes)
        {
            node.DestroyNode();
        }
        nodes.Clear();
    }

    public void DestroyAllBlocks()
    {
        int length = blocks.Count;
        for (int i = 0; i < length; i++)
        {
            blocks[length - i - 1].DeleteBlock();
        }
        blocks.Clear();
    }
}
