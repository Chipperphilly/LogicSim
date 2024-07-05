using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    bool isTutorial;

    public List<GameObject> gameObjects;
    BlockManager blockManager;
    NodeManager nodeManager;
    LevelManager levelManager;

    public GameObject blockHolder;
    [SerializeField]
    GameObject testObjectHolder;

    public List<Node> inputNodes;
    public List<Node> outputNodes;

    public GameObject blockNameInput;
    public TextMeshProUGUI blockNameInputText;

    [SerializeField]
    private GameObject blockButtonPrefab;
    [SerializeField]
    private GameObject blockButtonTab;


    public LevelSetup levelSetup;

    int lastOpenedTab = -1; //0 means blocks, 1 means nodes

    void Start()
    {
        blockManager = FindAnyObjectByType<BlockManager>();
        nodeManager = FindAnyObjectByType<NodeManager>();
        levelManager = FindAnyObjectByType<LevelManager>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();
        blockManager.blocksExisting = new Dictionary<string, int>();
        if (levelSetup != null)
        {
            foreach (BlockNumberPair blockNumberPair in levelSetup.blockCounts)
            {
                blockManager.blocksExisting.Add(blockNumberPair.blockName, 0);
            }
            foreach (BlockNumberPair blockNumberPair in levelSetup.nodeCounts)
            {
                blockManager.blocksExisting.Add(blockNumberPair.blockName, 0);
            }
        }
    }

    public void SetDepthValues()
    {
        foreach (Node node in inputNodes)
        {
            node.depth = 0;
            node.PropagateDepth();
        }
    }

    public void SaveBlock()
    {
        if (isTutorial)
            return;
        if (!blockNameInput.activeInHierarchy)
            blockNameInput.SetActive(true);
        else
        {
            if (blockNameInputText.text.Length > 1)
            {
                SaveBlockAs(blockNameInputText.text.Remove(blockNameInputText.text.Length - 1));
                blockNameInput.SetActive(false);
                blockNameInputText.text = "";
            }
        }
    }

    public FileInfo[] LoadBlockTab()
    {
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/blocks");
        FileInfo[] files = info.GetFiles();
        return files;
    }

    public void OpenBlockTab()
    {
        if (blockButtonTab.activeInHierarchy && lastOpenedTab == 0)
        {
            blockButtonTab.SetActive(false);
            return;
        }


        if (lastOpenedTab == 1)
        {
            blockButtonTab.GetComponent<BlockTabScroll>().Reposition();
        }
        lastOpenedTab = 0;

        for (int i = 0; i < blockButtonTab.transform.childCount; i++)
        {
            Destroy(blockButtonTab.transform.GetChild(i).gameObject);
        }

        FileInfo[] files = LoadBlockTab();

        int fileIdx = 0;
        foreach (FileInfo file in files)
        {
            string blockName = file.Name.Remove(file.Name.Length - 5);

            // Test if the block is included in level setup
            (bool isIncluded, BlockNumberPair blockNumberPair) = IsBlockInLevel(blockName);

            if (!isIncluded)
            {
                continue;
            }

            // Create the button to make the object

            GameObject blockButton = Instantiate(blockButtonPrefab);
            blockButton.transform.SetParent(blockButtonTab.transform, false);
            blockButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20 - fileIdx * 35, 0);
            blockButton.GetComponentInChildren<TextMeshProUGUI>().text = blockName.ToUpper();
            blockButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { 
                blockManager.CreateBlock(blockName);
                if (blockNumberPair.blockCount != -1 && blockNumberPair.blockCount - blockManager.blocksExisting[blockName.ToLower()] <= 0)
                    blockButton.GetComponent<Button>().interactable = false; }));

            if (isTutorial)
            {
                blockButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { FindAnyObjectByType<TutorialManager>().NextTutorialSlide(10); }));
            }

            if (blockNumberPair.blockCount != -1 && blockNumberPair.blockCount - blockManager.blocksExisting[blockName.ToLower()] <= 0)
            {
                blockButton.GetComponent<Button>().interactable = false;
            }
            fileIdx++;
        }
        blockButtonTab.SetActive(true);
    }

    public void OpenNodeTab()
    {
        if (blockButtonTab.activeInHierarchy && lastOpenedTab == 1)
        {
            blockButtonTab.SetActive(false);
            return;
        }

        if (lastOpenedTab == 0)
        {
            blockButtonTab.GetComponent<BlockTabScroll>().Reposition();
        }
        lastOpenedTab = 1;

        for (int i = 0; i < blockButtonTab.transform.childCount; i++)
        {
            Destroy(blockButtonTab.transform.GetChild(i).gameObject);
        }

        int fileIdx = 0;
        foreach (BlockNumberPair pair in levelSetup == null ? new() { new BlockNumberPair("output", -1), new BlockNumberPair("input", -1) } : levelSetup.nodeCounts)
        {
            GameObject blockButton = Instantiate(blockButtonPrefab);
            blockButton.transform.SetParent(blockButtonTab.transform, false);
            blockButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -20 - fileIdx * 35, 0);
            blockButton.GetComponentInChildren<TextMeshProUGUI>().text = pair.blockName.ToUpper();
            blockButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { 
                blockManager.CreateBlock(pair.blockName);
                if (pair.blockCount != -1 && pair.blockCount - blockManager.blocksExisting[pair.blockName.ToLower()] <= 0)
                    blockButton.GetComponent<Button>().interactable = false; }));

            if (isTutorial)
            {
                blockButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { FindAnyObjectByType<TutorialManager>().NextTutorialSlide(7); }));
                blockButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { FindAnyObjectByType<TutorialManager>().NextTutorialSlide(5); }));
                blockButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { FindAnyObjectByType<TutorialManager>().NextTutorialSlide(4); }));
            }

            if (pair.blockCount != -1 && pair.blockCount - blockManager.blocksExisting[pair.blockName] <= 0)
            {
                blockButton.GetComponent<Button>().interactable = false;
            }

            fileIdx++;
        }
        blockButtonTab.SetActive(true);
    }

    public void OpenTestTab()
    {
        testObjectHolder.SetActive(!testObjectHolder.activeInHierarchy);

        if (!testObjectHolder.activeInHierarchy)
        {
            foreach (GameObject testObject in levelManager.testObjects)
            {
                Destroy(testObject);
            }
            levelManager.testObjects.Clear();
            return;

        }

        for (int i = 0; i < levelSetup.inputTests.Length; i++)
        {
            levelManager.CreateTestObject(i);
        }
    }

    public string SaveBlockAs(string fileName)
    {
        if (isTutorial) // Nothing saves in the tutorial
            return null;

        SetDepthValues(); // Depth values need to be properly set for block to work when saved

        string json = "{";
        string inputsJson = "\"inputs\": {";

        foreach (Node node in inputNodes)
        {
            inputsJson += node.SaveNode();
            inputsJson += ",";
        }
        inputsJson += "},";

        string outputsJson = "\"outputs\": {";
        foreach (Node node in outputNodes)
        {
            outputsJson += node.SaveNode();
            outputsJson += ",";
        }
        outputsJson += "},";

        string blocksJson = "\"blocks\": {";
        foreach (LogicBlock block in blockManager.blocks)
        {
            blocksJson += block.SaveBlock();
            blocksJson += ",";
        }
        blocksJson += "},";

        string nodesJson = "\"nodes\": {";
        foreach (Node node in blockManager.nodes)
        {
            nodesJson += node.SaveNode();
            nodesJson += ",";
        }
        foreach (Node node in outputNodes)
        {
            nodesJson += node.SaveNode();
            nodesJson += ",";
        }
        nodesJson += "},";

        string connectorsJson = "\"connectors\": {";
        foreach (Connector connector in nodeManager.connectors)
        {
            connectorsJson += connector.SaveConnector();
            connectorsJson += ",";
        }
        connectorsJson += "}}";

        json += inputsJson + outputsJson + blocksJson + nodesJson + connectorsJson;
        string path = Application.persistentDataPath + "/blocks/" + fileName + ".json";
        File.WriteAllText(path, json);
        return json;
    }

    public (bool, BlockNumberPair) IsBlockInLevel(string blockName)
    {
        if (levelSetup == null)
        {
            return (true, new BlockNumberPair(blockName, -1));
        }
        foreach (BlockNumberPair pair in levelSetup.blockCounts)
        {
            if (pair.blockName == blockName)
            {
                return (true, pair);
            }
        }
        return (false, new BlockNumberPair(blockName, -1));
    } 

    public void DestroyAllMasterNodes()
    {
        foreach (Node node in inputNodes)
        {
            node.DestroyNode();
        }
        inputNodes.Clear();

        foreach (Node node in outputNodes)
        {
            node.DestroyNode();
        }
        outputNodes.Clear();
    }
    
}
