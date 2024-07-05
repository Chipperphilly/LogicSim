using UnityEngine;

public class RandomBlockScript : MonoBehaviour
{
    public NodeValue[] Refresh(NodeValue[] inputs)
    {
        return new NodeValue[] { 
            Random.Range(0,2) == 1 ? NodeValue.True : NodeValue.False
        };
    }
}
