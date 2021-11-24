using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public abstract class ScriptableAction : ScriptableObject
{
    protected bool usedOnGrid = false;
    protected bool usedWithScriptActions = false;
    public abstract List<ScriptableAction> executeWithScriptActions(TappingGrid g, GemGridPosition gem);
    public abstract void execute();
    public abstract void executeOnGrid(TappingGrid g, GemGridPosition gem);
    
}

