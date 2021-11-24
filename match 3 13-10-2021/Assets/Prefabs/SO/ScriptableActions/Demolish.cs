using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demolish : ScriptableAction
{
    public int x;
    public int y;
    public override void execute()
    {
        return;
    }

    public override void executeOnGrid(TappingGrid g, GemGridPosition gem)
    {
        g.tryDemolish(g.grid.GetGridObject(x, y));
    }

    public override List<ScriptableAction> executeWithScriptActions(TappingGrid g, GemGridPosition gem)
    {
        return new List<ScriptableAction>();
    }
}
