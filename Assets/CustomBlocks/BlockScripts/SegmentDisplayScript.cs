using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SegmentDisplayScript : MonoBehaviour
{
    [SerializeField]
    List<GameObject> segments;

    public NodeValue[] Refresh(NodeValue[] inputs)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].GetComponent<Image>().color = inputs[i] == NodeValue.True ? Color.red : Color.black;
        }
        return new NodeValue[0];
    }
}
