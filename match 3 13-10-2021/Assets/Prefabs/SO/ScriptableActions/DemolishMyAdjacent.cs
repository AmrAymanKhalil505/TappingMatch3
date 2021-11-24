using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemolishMyAdjacent : ScriptableAction
{
    
    public override void execute()
    {
        return;
    }

    public override void executeOnGrid(TappingGrid g, GemGridPosition gem)
    {
        int[] dirx = new int[] { 0, 1, 0, -1 };
        int[] diry = new int[] { 1, 0, -1, 0 };
        for (int dir = 0; dir < dirx.Length; dir++)
        {
            g.tryDemolish(g.grid.GetGridObject(gem.GetX() + dirx[dir], gem.GetY() + diry[dir]));
        }
    }

    public override List<ScriptableAction> executeWithScriptActions(TappingGrid g, GemGridPosition gem)
    {
        return new List<ScriptableAction>();
    }
}
