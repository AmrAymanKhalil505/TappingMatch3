using Unity.MLAgents;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;
public class MLAgentBoard : AbstractBoard
{   
    //this is related to my mvc implementation 
    [SerializeField] private TappingGrid match3;
    [SerializeField] private TappingGridVisual match3Visual;

    //usally present in every implemetation 
    private Agent agent;

    private void Awake()
    {
        //usally present in every implemetation 
        agent = GetComponent<Agent>();



        //this will chagne depending on your implementation 
        match3Visual.OnStateChanged += ask_AI_to_make_action;
        match3.OnGemGridPositionDestroyed += give_reward_to_AI;
        match3.OnMoveUsed += decrease_reward_AI_used_move;
        match3.OnOutOfMoves += decrease_reward_AI_lost_game;
        match3.OnWin += give_reward_to_AI_game_won;
    }

    // give reward to AI and start new level
    //this will chagne depending on your implementation 
    private void give_reward_to_AI_game_won(object sender, System.EventArgs e)
    {
        agent.AddReward(10f);
        Debug.Log(agent.GetCumulativeReward());
        agent.EndEpisode();
       
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        
    }

    // punish AI for lossing and start new level
    //this will chagne depending on your implementation 
    private void decrease_reward_AI_lost_game(object sender, System.EventArgs e)
    {
        agent.AddReward(-10f);
        //Debug.Log(agent.GetCumulativeReward());
        agent.EndEpisode();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    //take rewards from AI because it used a move
    //this will chagne depending on your implementation 
    private void decrease_reward_AI_used_move(object sender, System.EventArgs e)
    {
        LevelSO levelSO = match3.GetLevelSO();
        if (levelSO.goalType == LevelSO.GoalType.Glass)
        {
            agent.AddReward(-.5f);
        }
    }

    //give reward to AI on destroying a one gem
    //this will chagne depending on your implementation 
    private void give_reward_to_AI(object sender, System.EventArgs e)
    {
        LevelSO levelSO = match3.GetLevelSO();
        if (levelSO.goalType == LevelSO.GoalType.Score)
        {
            agent.AddReward(1f);
        }
    }


    // request action from AI
    //this will chagne depending on your implementation 
    private void ask_AI_to_make_action(object sender, System.EventArgs e)
    {
        if (match3Visual.state.currentState.Equals(StateType.WaitingForUser))
        {
            agent.RequestDecision();
        }

    }



    //usally present in every implemetation 
    //get the color of the gem in the cell/ is bomb/is flask/is rainbow/is rocket
    public override int GetCellType(int row, int col)
    {
        GemSO gemSO = match3.grid.GetGridObject(row, col).GetGemGrid().GetGem();
        return ((int)gemSO.pieceType)*10 + ((int)gemSO.color);
    }
    //usally present in every implemetation 
    //get general info about the board size
    public override BoardSize GetMaxBoardSize()
    {
        BoardSize b = new BoardSize();
        LevelSO levelSO = match3.GetLevelSO();
        //get the recangle shape of the board
        b.Columns = levelSO.width;
        b.Rows = levelSO.height;
        //number of colors and types and gems in the game
        b.NumCellTypes = 100;
        //background of the cell types 
        //if you have glass cells
        //if you have empty cells
        b.NumSpecialTypes = 1;
        return b;
    }

    //usally present in every implemetation 
    //get the background cell type
    public override int GetSpecialType(int row, int col)
    {
        return match3.grid.GetGridObject(row, col).HasGlass() ? 1 : 0;
    }


    //usally present in every implemetation 
    //tell AI what is a valid move and what isn't
    public override bool IsMoveValid(Move m)
    {
        LevelSO levelSO = match3.GetLevelSO();
        int startx = m.Column;
        int starty = m.Row;
        var moveEnd = m.OtherCell();
        int endx = moveEnd.Column;
        int endy = moveEnd.Row;
        bool test1 = (startx != levelSO.width - 1) && (startx != endx - 1 || starty != endy);
        bool test2 = (startx == levelSO.width - 1) && (startx != endx + 1 || starty != endy);
        if (test1 || test2)
        {
            return false;
        }
            
        if(match3.canMakeMove(startx, starty))
        {
            return true;
        }
             
        
        return  false;
    }

    //usally present in every implemetation 
    //what should the game do when the AI asks to do a move
    public override bool MakeMove(Move m)
    {
        if (!IsMoveValid(m)) { return false; }

        return match3Visual.makeMove(m.Column, m.Row);
    }
}
