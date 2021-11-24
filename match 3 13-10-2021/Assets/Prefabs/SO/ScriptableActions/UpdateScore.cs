using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScore : ScriptableAction
{
    public int score;
    public override void execute()
    {
        return;
    }

    public override void executeOnGrid(TappingGrid g, GemGridPosition gem=null)
    {
        g.UpdateScore(score);
    }

    public override List<ScriptableAction> executeWithScriptActions(TappingGrid g, GemGridPosition gem=null)
    {
        return new List<ScriptableAction>();
    }
}
