using UnityEngine;

public class LEDScript : MonoBehaviour
{
    [SerializeField]
    GameObject LEDLight;

    public NodeValue[] Refresh(NodeValue[] inputs)
    {
        LEDLight.SetActive(inputs[0] == NodeValue.True);
        return new NodeValue[0];
    }
}
