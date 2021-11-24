using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCell : ScriptableAction
{
    public int x;
    public int y;
    public override void execute()
    {
        return;
    }

    public override void executeOnGrid(TappingGrid g, GemGridPosition gem)
    {
        g.TryDestroyGemGridPosition(g.grid.GetGridObject(x, y));
    }

    public override List<ScriptableAction> executeWithScriptActions(TappingGrid g, GemGridPosition gem)
    {
        return null;
    }
}
