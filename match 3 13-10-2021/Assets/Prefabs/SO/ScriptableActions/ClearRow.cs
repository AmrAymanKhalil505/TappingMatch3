using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearRow : ScriptableAction
{
    public int x;
    public int y;

    public override void execute()
    {
        return;
    }

    public override void executeOnGrid(TappingGrid g, GemGridPosition gem)
    {
        return;
    }

    public override List<ScriptableAction> executeWithScriptActions(TappingGrid g, GemGridPosition gem)
    {

        return null;
    }
}
