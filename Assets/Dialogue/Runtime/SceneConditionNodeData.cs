using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]

public class SceneConditionNodeData : BasicNodeData
{
    public ExposedProperty property;
    public bool necessaryState;
}
