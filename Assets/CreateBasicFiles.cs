using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CreateBasicFiles : MonoBehaviour
{
    string[] filesToCreate = new string[] { "nand", "split", "led", "num to segment", "random", "segment display" };
    void Start()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/blocks"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/blocks");
        }
        foreach (string fileName in filesToCreate)
        {
            if (!File.Exists(Application.persistentDataPath + "/blocks/" + fileName + ".json"))
            {
                File.Create(Application.persistentDataPath + "/blocks/" + fileName + ".json");
            }
        }
    }
}
