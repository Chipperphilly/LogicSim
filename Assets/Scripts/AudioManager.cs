using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource connectNodes;
    [SerializeField] AudioSource createObject;
    [SerializeField] AudioSource toggleNode;
    // Start is called before the first frame update
    public void ConnectNodes()
    {
        connectNodes.Play();
    }

    public void CreateObject()
    {
        createObject.Play();
    }

    public void ToggleNode()
    {
        toggleNode.Play();
    }
}
