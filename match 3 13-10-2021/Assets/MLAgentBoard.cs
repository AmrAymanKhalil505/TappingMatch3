using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Unity.MLAgents;
using Unity.MLAgents.Integrations.Match3;
public class MLAgentBoard : AbstractBoard
{
    [SerializeField] private TappingGrid match3;
    [SerializeField] private TappingGridVisual match3Visual;
    private Agent agent;

    private void Awake()
    {
        agent = GetComponent<Agent>();
        LevelSO levelSO = match3.GetLevelSO();
        //Rows = levelSO.height;
        //Colums = levelSO.width;
        match3Visual.OnStateChanged += match3_action;
        match3.OnGemGridPositionDestroyed += rewardGem;
        match3.OnGlassDestroyed += glassReward;
        match3.OnMoveUsed += onMoveUsed;
        match3.OnOutOfMoves += endEpisodLoss;
        match3.OnWin += endEpisodWin;
    }

    private void endEpisod(object sender, System.EventArgs e)
    {
        agent.EndEpisode();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void endEpisodWin(object sender, System.EventArgs e)
    {
        agent.AddReward(10f);
        Debug.Log(agent.GetCumulativeReward());
        agent.EndEpisode();
       
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        
    }
    private void endEpisodLoss(object sender, System.EventArgs e)
    {
        agent.AddReward(-10f);
        Debug.Log(agent.GetCumulativeReward());
        agent.EndEpisode();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    private void onMoveUsed(object sender, System.EventArgs e)
    {
        LevelSO levelSO = match3.GetLevelSO();
        if (levelSO.goalType == LevelSO.GoalType.Glass)
        {
            agent.AddReward(-.5f);
        }
    }

    private void glassReward(object sender, System.EventArgs e)
    {
        LevelSO levelSO = match3.GetLevelSO();
        if (levelSO.goalType == LevelSO.GoalType.Glass)
        {
            agent.AddReward(1f);
        }
    }

    private void rewardGem(object sender, System.EventArgs e)
    {
        LevelSO levelSO = match3.GetLevelSO();
        if (levelSO.goalType == LevelSO.GoalType.Score)
        {
            agent.AddReward(1f);
        }
    }

    private void match3_action(object sender, System.EventArgs e)
    {
        //agent.RequestDecision();
        if (match3Visual.state.currentState.Equals(StateType.WaitingForUser))
        {
            //yield return WaitForSeconds(.5f);
            agent.RequestDecision();
        }

    }
    



    public override int GetCellType(int row, int col)
    {
        GemSO gemSO = match3.grid.GetGridObject(row, col).GetGemGrid().GetGem();
        return ((int)gemSO.pieceType)*10 + ((int)gemSO.color);
    }

    public override BoardSize GetMaxBoardSize()
    {
        BoardSize b = new BoardSize();
        LevelSO levelSO = match3.GetLevelSO();
        b.Columns = levelSO.width;
        b.Rows = levelSO.height;
        b.NumCellTypes = 100;
        b.NumSpecialTypes = 0;
        return b;
    }

    public override int GetSpecialType(int row, int col)
    {
        return match3.grid.GetGridObject(row, col).HasGlass() ? 1 : 0;
    }

    public override bool IsMoveValid(Move m)
    {
        int startx = m.Column;
        int starty = m.Row;
        var moveEnd = m.OtherCell();
        int endx = moveEnd.Column;
        int endy = moveEnd.Row;
       
       if(match3.canMakeMove(startx, starty))
       return true; 
        
        return  false;
    }

    public override bool MakeMove(Move m)
    {
        if (!IsMoveValid(m)) { return false; }


        return match3Visual.makeMove(m.Column, m.Row);
    }
}
