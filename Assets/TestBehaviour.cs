using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestBehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject testMarker;

    public List<bool> inputs = new List<bool>();
    public List<bool> outputs = new List<bool>();

    public int height;

    private readonly int TESTMARKERHEIGHT = 20;
    private readonly int TESTMARKERSPACING = 10;

    private void Start()
    {
        ConfigureTestObject();
    }

    public void ConfigureTestObject()
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            GameObject marker = Instantiate(testMarker, transform, true);
            marker.GetComponent<RectTransform>().localPosition = new Vector3(-176, -55 - i * (TESTMARKERHEIGHT + TESTMARKERSPACING));
            marker.GetComponent<Image>().color = inputs[i] ? Color.green : Color.red;
        }
        for (int i = 0; i < outputs.Count; i++)
        {
            GameObject marker = Instantiate(testMarker, transform, true);
            marker.GetComponent<RectTransform>().localPosition = new Vector3(0, -55 - i * (TESTMARKERHEIGHT + TESTMARKERSPACING));
            marker.GetComponent<Image>().color = outputs[i] ? Color.green : Color.red;
        }
        height = 90 + Mathf.Max(inputs.Count, outputs.Count) * (TESTMARKERHEIGHT + TESTMARKERSPACING);
        print(height);
    }
}
