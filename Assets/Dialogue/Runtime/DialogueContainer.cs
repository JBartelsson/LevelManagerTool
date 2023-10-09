using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<DialogueNodeData> dialogueNodeData = new();
    public List<NodeLinkData> nodeLinks = new();
}
