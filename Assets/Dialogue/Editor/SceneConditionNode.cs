using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class SceneConditionNode : BasicNode
{
    public ExposedProperty property;
    public bool necessaryState;
}
