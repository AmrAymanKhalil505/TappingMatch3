using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearMyCell : ScriptableAction
{
    

    public override void execute()
    {
        return;
    }

    public override void executeOnGrid(TappingGrid g, GemGridPosition gem)
    {
        if (usedOnGrid) return;
        g.TryDestroyGemGridPosition(gem); usedOnGrid = true;
        return  ;
    }

    public override List<ScriptableAction> executeWithScriptActions(TappingGrid g, GemGridPosition gem)
    {
        if (usedWithScriptActions) return new List<ScriptableAction>();
        usedWithScriptActions = true;
        return new List<ScriptableAction>();
    }
}
