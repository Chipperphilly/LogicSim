using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSetup", menuName = "ScriptableObjects/LevelSetup", order = 1)]
public class LevelSetup : ScriptableObject
{
    public List<BlockNumberPair> blockCounts;
    public List<BlockNumberPair> nodeCounts;

    public LevelTests[] inputTests;
}

[Serializable]
public class BlockNumberPair
{
    public string blockName;
    public int blockCount;

    public BlockNumberPair(string name, int count)
    {
        blockName = name;
        blockCount = count;
    }
}
