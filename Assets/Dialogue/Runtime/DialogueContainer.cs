using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
[CreateAssetMenu]
public class DialogueContainer : ScriptableObject
{
    private List<DialogueNodeData> dialogueNodeData = new();
    private List<NodeLinkData> nodeLinks = new();
    private List<ExposedProperty> exposedProperties = new();

    public List<DialogueNodeData> DialogueNodeData { get => dialogueNodeData; set => dialogueNodeData = value; }
    public List<NodeLinkData> NodeLinks { get => nodeLinks; set => nodeLinks = value; }
    public List<ExposedProperty> ExposedProperties { get => exposedProperties; set => exposedProperties = value; }
}
