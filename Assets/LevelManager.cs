using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    GameManager gameManager;
    BlockManager blockManager;
    NodeManager nodeManager;

    [SerializeField]
    GameObject testObject;
    [SerializeField]
    GameObject testMarker;

    [SerializeField]
    GameObject testObjectHolder;

    public List<GameObject> testObjects = new List<GameObject>();

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        blockManager = FindObjectOfType<BlockManager>();
        nodeManager = FindObjectOfType<NodeManager>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            ClearLevel();
        }
    }

    public void ClearLevel()
    {
        nodeManager.DestroyAllConnectors();
        blockManager.DestroyAllNodes();
        blockManager.DestroyAllBlocks();
        gameManager.DestroyAllMasterNodes();
    }

    public GameObject CreateTestObject(int testIndex)
    {
        GameObject testObj = Instantiate(testObject, testObjectHolder.transform);
        testObj.GetComponent<RectTransform>().localPosition = new Vector3(0, -25 + (testObjects.Count != 0 ? -testObjects.Count * testObjects[0].GetComponent<TestBehaviour>().height : 0));
        TestBehaviour testBehaviour = testObj.GetComponent<TestBehaviour>();

        LevelTests test = gameManager.levelSetup.inputTests[testIndex];

        for (int i = 0; i < test.inputs.Length; i++)
        {
            testBehaviour.inputs.Add(test.inputs[i]);
        }
        for (int i = 0; i < test.outputs.Length; i++)
        {
            testBehaviour.outputs.Add(test.outputs[i]);
        }

        testObjects.Add(testObj);
        testBehaviour.ConfigureTestObject();

        return testObj;
    }

    public void TestLogic()
    {
        bool[] testResults = new bool[gameManager.levelSetup.inputTests.Length];

        int index = 0;
        foreach (LevelTests test in gameManager.levelSetup.inputTests)
        {
            print("Test " + index.ToString());
            testResults[index] = true;
            for (int i = 0; i < test.inputs.Length; i++)
            {
                gameManager.inputNodes[i].SetNodeValue(test.inputs[i] ? NodeValue.True : NodeValue.False);
                gameManager.inputNodes[i].connector.PassValue(gameManager.inputNodes[i].value);
            }

            for (int i = 0; i < test.outputs.Length; i++)
            {
                print("Node " + i + ": Expected " + test.outputs[i] + ", Observed " + (gameManager.outputNodes[i].value == NodeValue.True ? true : false));
                if (gameManager.outputNodes[i].value != (test.outputs[i] ? NodeValue.True : NodeValue.False))
                {
                    testResults[index] = false;
                    break;
                }
            }
            index++;
        }
        foreach (bool testResult in testResults)
        {
            print(testResult);
        }
        //return testResults;
    }
}
