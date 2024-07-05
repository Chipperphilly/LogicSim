using UnityEngine;

public class NumToSegmentScript : MonoBehaviour
{
    public NodeValue[] Refresh(NodeValue[] inputs)
    {
        int sum = 0;
        sum += inputs[0] == NodeValue.True ? 1 : 0;
        sum += inputs[1] == NodeValue.True ? 2 : 0;
        sum += inputs[2] == NodeValue.True ? 4 : 0;
        sum += inputs[3] == NodeValue.True ? 8 : 0;
        print(sum);
        switch (sum)
        {
            case 0:
                return new NodeValue[] { NodeValue.True, NodeValue.False, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True };
            case 1:
                return new NodeValue[] { NodeValue.False, NodeValue.False, NodeValue.False, NodeValue.False, NodeValue.False, NodeValue.True, NodeValue.True };
            case 2:
                return new NodeValue[] { NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.False, NodeValue.False, NodeValue.True };
            case 3:
                return new NodeValue[] { NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.False, NodeValue.False, NodeValue.True, NodeValue.True };
            case 4:
                return new NodeValue[] { NodeValue.False, NodeValue.True, NodeValue.False, NodeValue.False, NodeValue.True, NodeValue.True, NodeValue.True };
            case 5:
                return new NodeValue[] { NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.False, NodeValue.True, NodeValue.True, NodeValue.False };
            case 6:
                return new NodeValue[] { NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.False };
            case 7:
                return new NodeValue[] { NodeValue.False, NodeValue.False, NodeValue.True, NodeValue.False, NodeValue.False, NodeValue.True, NodeValue.True };
            case 8:
                return new NodeValue[] { NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.True };
            case 9:
                return new NodeValue[] { NodeValue.True, NodeValue.True, NodeValue.True, NodeValue.False, NodeValue.True, NodeValue.True, NodeValue.True };
            default:
                return new NodeValue[] { NodeValue.False, NodeValue.False, NodeValue.False, NodeValue.False, NodeValue.False, NodeValue.False, NodeValue.False };
        }
    }
}
