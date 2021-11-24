using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TappingGrid : MonoBehaviour
{
    public event EventHandler<OnNewGemGridSpawnedEventArgs> OnNewGemGridSpawned;
    public event EventHandler<OnLevelSetEventArgs> OnLevelSet;
    public event EventHandler OnWaitingInput;
    public event EventHandler OnStartFillingVertically;
    public event EventHandler OnStartFillingDiagonally;
    public event EventHandler OnGemGridPositionDestroyed;
    public event EventHandler OnWin;
    public event EventHandler OnGlassDestroyed;
    public event EventHandler OnMoveUsed;
    public event EventHandler OnOutOfMoves;
    public event EventHandler OnScoreChanged;
    public event EventHandler<OnNewGemGridSpawnedEventArgs> OnGemSpecialGridChanged;
    // Start is called before the first frame update

    //public class OnNewGemGridSpawnedEventArgs : EventArgs
    //{
    //    public GemGrid gemGrid;
    //    public GemGridPosition gemGridPosition;
    //}
    State state;
    //public class OnLevelSetEventArgs : EventArgs
    //{
    //    public LevelSO levelSO;
    //    public Grid<GemGridPosition> grid;
    //}
    public bool isSpawnCrossLevel;
    public bool isSpawnBombLevel;
    public bool isSpawnRainbowLevel;

    [SerializeField]
    public GemSO Rocket;
    [SerializeField]
    public GemSO Bomb;

    public List<GemSO> allGemsSO;

    private Dictionary<PieceType, GameObject> _piecePrefabDict;

    private Dictionary<BackgroundType, GameObject> _backgroundPrefabDict;
    private GamePiece[,] _pieces;
    private BackgroundPiece[,] backgroundPieces;


    private GamePiece _pressedPiece;

    private void Awake()
    {

    }


    public class OnNewGemGridSpawnedEventArgs : EventArgs
    {
        public GemGrid gemGrid;
        public GemGridPosition gemGridPosition;
    }
    public class OnLevelSetEventArgs : EventArgs
    {
        public LevelSO levelSO;
        public Grid<GemGridPosition> grid;
    }
    [SerializeField] private LevelSO levelSO;
    [SerializeField] private bool autoLoadLevel;
    [SerializeField] private bool match4Explosions; // Explode neighbour nodes on 4 match

    private int gridWidth;
    private int gridHeight;
    public Grid<GemGridPosition> grid;
    private int score;
    private int moveCount;
    public void UpdateScore(int s)
    {
        score += s;
    }
    public void SetLevelSO(LevelSO levelSO)
    {
        this.levelSO = levelSO;

        gridWidth = levelSO.width;
        gridHeight = levelSO.height;
        grid = new Grid<GemGridPosition>(gridWidth, gridHeight, 1f, Vector3.zero, (Grid<GemGridPosition> g, int x, int y) => new GemGridPosition(g, x, y));

        isSpawnCrossLevel = levelSO.isSpawnCrossLevel;
        isSpawnBombLevel = levelSO.isSpawnBombLevel;
        isSpawnRainbowLevel = levelSO.isSpawnRainbowLevel;
        // Initialize Grid
        allGemsSO = new List<GemSO>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {

                // Get Saved LevelGridPosition
                LevelSO.LevelGridPosition levelGridPosition = null;

                foreach (LevelSO.LevelGridPosition tmpLevelGridPosition in levelSO.levelGridPositionList)
                {
                    if (tmpLevelGridPosition.x == x && tmpLevelGridPosition.y == y)
                    {
                        levelGridPosition = tmpLevelGridPosition;
                        break;
                    }
                }

                if (levelGridPosition == null)
                {
                    // Couldn't find LevelGridPosition with this x, y!
                    Debug.LogError("Error! Null!");
                }

                GemSO gem = levelGridPosition.gemSO;
                GemGrid gemGrid = new GemGrid(gem, x, y);
                grid.GetGridObject(x, y).SetGemGrid(gemGrid);
                grid.GetGridObject(x, y).SetHasGlass(levelGridPosition.hasGlass);
            }
        }

        score = 0;
        moveCount = levelSO.moveAmount;
        allGemsSO.AddRange(GetLevelSO().gemList);
        //if (isSpawnCrossLevel) { allGemsSO.Add(GetLevelSO().Rocket); }
        //if (isSpawnBombLevel) { allGemsSO.Add(GetLevelSO().Bomb); }
        OnLevelSet?.Invoke(this, new OnLevelSetEventArgs { levelSO = levelSO, grid = grid });
    }
    //public void Grid_SpeicalGemChanged(object sender, Grid<GemGridPosition>.OnGridObjectChangedEventArgs e)
    //{
    //    GemSO GSO = GetGem(e.x, e.y);
    //    if (GSO is null) return;
    //    if (!isSpecial(GSO)) return;

    //}
    public LevelSO GetLevelSO()
    {
        return levelSO;
    }
    public int GetScore()
    {
        return score;
    }

    public bool HasMoveAvailable()
    {
        return moveCount > 0;
    }

    public int GetMoveCount()
    {
        return moveCount;
    }

    public int GetUsedMoveCount()
    {
        return levelSO.moveAmount - moveCount;
    }

    public void UseMove()
    {
        moveCount--;
        OnMoveUsed?.Invoke(this, EventArgs.Empty);
    }

    public int GetGlassAmount()
    {
        int glassAmount = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
                if (gemGridPosition.HasGlass())
                {
                    glassAmount++;
                }
            }
        }
        return glassAmount;
    }
    public void FallGemsIntoEmptyPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

                if (!gemGridPosition.IsEmpty())
                {
                    // Grid Position has Gem
                    for (int i = y - 1; i >= 0; i--)
                    {
                        GemGridPosition nextGemGridPosition = grid.GetGridObject(x, i);
                        if (nextGemGridPosition.IsEmpty())
                        {
                            gemGridPosition.GetGemGrid().SetGemXY(x, i);
                            nextGemGridPosition.SetGemGrid(gemGridPosition.GetGemGrid());
                            gemGridPosition.ClearGemGrid();

                            gemGridPosition = nextGemGridPosition;
                        }
                        else
                        {
                            // Next Grid Position is not empty, stop looking
                            break;
                        }
                    }
                }
            }
        }
    }
    private GemSO GetGemSO(int x, int y)
    {
        if (!IsValidPosition(x, y)) return null;

        GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

        if (gemGridPosition.GetGemGrid() == null) return null;

        return gemGridPosition.GetGemGrid().GetGem();
    }

    public bool IsValidPosition(int x, int y)
    {
        if (x < 0 || y < 0 ||
            x >= gridWidth || y >= gridHeight)
        {
            // Invalid position
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool TryIsGameOver()
    {
        if (!HasMoveAvailable())
        {
            // No more moves, game over!
            OnOutOfMoves?.Invoke(this, EventArgs.Empty);
            return true;
        }

        switch (levelSO.goalType)
        {
            default:
            case LevelSO.GoalType.Score:
                if (score >= levelSO.targetScore)
                {
                    // Reached Target Score!
                    OnWin?.Invoke(this, EventArgs.Empty);
                    return true;
                }
                break;
            case LevelSO.GoalType.Glass:
                if (GetGlassAmount() <= 0)
                {
                    // All glass destroyed!
                    OnWin?.Invoke(this, EventArgs.Empty);
                    return true;
                }
                break;
        }

        // Not game over
        return false;
    }
    public void tryDemolish(GemGridPosition gem)
    {
        throw new  NotImplementedException();
    }
    public List<GemGridPosition> GetTheSetOfSpecial(int x, int y)
    {
        List<GemGridPosition> TheSetOfSameColor = new List<GemGridPosition>();
        if (!IsValidPosition(x, y)) return TheSetOfSameColor;
        Queue<GemGridPosition> queue = new Queue<GemGridPosition>();
        bool[,] Visited = new bool[gridWidth, gridHeight];

        TheSetOfSameColor.Add(grid.GetGridObject(x, y));
        queue.Enqueue(grid.GetGridObject(x, y));
        Visited[x, y] = true;

        int[] dirx = new int[] { 0, 1, 0, -1 };
        int[] diry = new int[] { 1, 0, -1, 0 };
        while (queue.Count > 0)
        {
            GemGridPosition q = queue.Dequeue();
            for (int dir = 0; dir < dirx.Length; dir++)
            {
                if (IsValidPosition(dirx[dir] + q.GetX(), diry[dir] + q.GetY()) && !Visited[dirx[dir] + q.GetX(), diry[dir] + q.GetY()])
                {
                    if (isSpecial(GetGem(dirx[dir] + q.GetX(), diry[dir] + q.GetY())))
                    {
                        TheSetOfSameColor.Add(grid.GetGridObject(dirx[dir] + q.GetX(), diry[dir] + q.GetY()));
                        queue.Enqueue(grid.GetGridObject(dirx[dir] + q.GetX(), diry[dir] + q.GetY()));
                        Visited[dirx[dir] + q.GetX(), diry[dir] + q.GetY()] = true;
                    }
                }
            }
        }
        return TheSetOfSameColor;
    }

    public bool isSpecial(GemSO gemSO)
    {
        if (gemSO.pieceType.Equals(PieceType.BOMB)) { return true; }
        if (gemSO.pieceType.Equals(PieceType.RAINBOW)) { return true; }
        if (gemSO.pieceType.Equals(PieceType.ROW_CLEAR)) { return true; }
        if (gemSO.pieceType.Equals(PieceType.COLUMN_CLEAR)) { return true; }
        if (gemSO.pieceType.Equals(PieceType.CROSS)) { return true; }
        return false;
    }

    public void destroySetOfSameColor(List<GemGridPosition> TheSetOfSameColor)
    {

        int len = TheSetOfSameColor.Count;
        bool spawnCrossClear = false;
        bool spawnBomb = false;
        bool spwanRainbow = false;
        if (len >= 5 && len <= 7 && isSpawnCrossLevel)
        {
            spawnCrossClear = true;
        }
        else if ((len == 8 || len == 9) && isSpawnBombLevel)
        {
            spawnBomb = true;
        }
        else if (len >= 10 && isSpawnRainbowLevel)
        {
            spwanRainbow = true;
        }
        int GetX = TheSetOfSameColor[0].GetX();
        int GetY = TheSetOfSameColor[0].GetY();
        foreach (GemGridPosition gem in TheSetOfSameColor)
        {
            TryDestroyGemGridPosition(gem, true);
        }
        if (spawnCrossClear)
        {
            GemGrid gemGrid = new GemGrid(Rocket, GetX, GetY);

            grid.GetGridObject(GetX, GetY).SetGemGrid(gemGrid);
            OnGemSpecialGridChanged?.Invoke(gemGrid, new OnNewGemGridSpawnedEventArgs
            {
                gemGrid = gemGrid,
                gemGridPosition = grid.GetGridObject(GetX, GetY),
            });
            //grid.GetGridObject(GetX, GetY).GetGemGrid().SetGem(Rocket);
        }
        if (spawnBomb)
        {
            GemGrid gemGrid = new GemGrid(Bomb, GetX, GetY);

            grid.GetGridObject(GetX, GetY).SetGemGrid(gemGrid);

            OnGemSpecialGridChanged?.Invoke(gemGrid, new OnNewGemGridSpawnedEventArgs
            {
                gemGrid = gemGrid,
                gemGridPosition = grid.GetGridObject(GetX, GetY),
            });
        }
        if (spwanRainbow)
        {

        }

    }
    public List<GemGridPosition> GetTheSetOfSameColor(int x, int y)
    {

        List<GemGridPosition> TheSetOfSameColor = new List<GemGridPosition>();
        if (!IsValidPosition(x, y) || !isThereGem(x, y)) return TheSetOfSameColor;
        Queue<GemGridPosition> queue = new Queue<GemGridPosition>();
        bool[,] Visited = new bool[gridWidth, gridHeight];

        TheSetOfSameColor.Add(grid.GetGridObject(x, y));
        queue.Enqueue(grid.GetGridObject(x, y));
        Visited[x, y] = true;

        int[] dirx = new int[] { 0, 1, 0, -1 };
        int[] diry = new int[] { 1, 0, -1, 0 };


        while (queue.Count > 0)
        {
            GemGridPosition q = queue.Dequeue();
            for (int dir = 0; dir < dirx.Length; dir++)
            {
                if (IsValidPosition(dirx[dir] + q.GetX(), diry[dir] + q.GetY()) && isThereGem(dirx[dir] + q.GetX(), diry[dir] + q.GetY()) && !Visited[dirx[dir] + q.GetX(), diry[dir] + q.GetY()])
                {
                    if (GetGem(dirx[dir] + q.GetX(), diry[dir] + q.GetY()).color.Equals(GetGem(q.GetX(), q.GetY()).color))
                    {
                        TheSetOfSameColor.Add(grid.GetGridObject(dirx[dir] + q.GetX(), diry[dir] + q.GetY()));
                        queue.Enqueue(grid.GetGridObject(dirx[dir] + q.GetX(), diry[dir] + q.GetY()));
                        Visited[dirx[dir] + q.GetX(), diry[dir] + q.GetY()] = true;
                    }
                }
            }
        }
        return TheSetOfSameColor;
    }
    public bool isThereGem(int x, int y)
    {
        if (grid.GetGridObject(x, y) is null)
            return false;
        if (grid.GetGridObject(x, y).GetGemGrid() is null)
            return false;
        if (grid.GetGridObject(x, y).GetGemGrid().GetGem() is null)
            return false;

        return true;
    }
    public GemSO GetGem(int x, int y)
    {
        return GetGemSO(x, y);
    }

    public void destroySpeicalGemSet(List<GemGridPosition> TheSetOfSpeical)
    {
        //List<GemGridPosition> TheSetOfSameColor = GetTheSetOfSameColor(x, y);
        int rowClearCnt = 0;
        int colClearCnt = 0;
        int bombCnt = 0;
        int rainbowCnt = 0;
        int normalCnt = 0;
        if (TheSetOfSpeical.Count == 0) return;
        foreach (GemGridPosition g in TheSetOfSpeical)
        {
            switch (GetGem(g.GetX(), g.GetY()).pieceType)
            {
                case PieceType.EMPTY:
                    break;
                case PieceType.NORMAL:
                    normalCnt += 1;
                    break;
                case PieceType.BUBBLE:
                    break;
                case PieceType.ROW_CLEAR:
                    rowClearCnt += 1;
                    break;
                case PieceType.COLUMN_CLEAR:
                    colClearCnt += 1;
                    break;
                case PieceType.RAINBOW:
                    rainbowCnt += 1;
                    break;
                case PieceType.BOMB:
                    bombCnt += 1;
                    break;
                case PieceType.COUNT1:
                    break;
                case PieceType.Block:
                    break;

            }
        }


        if (rainbowCnt > 0)
        {
            if (rainbowCnt > 1)
            {
                destroyAllGrid(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
            }
            else if (bombCnt > 0)
            {
                turnColorTo(TheSetOfSpeical[0], PieceType.BOMB);
            }
            else if (rowClearCnt > 0 && colClearCnt > 0)
            {
                turnColorTo(TheSetOfSpeical[0], PieceType.CROSS);
            }
            else if (rowClearCnt > 0)
            {
                turnColorTo(TheSetOfSpeical[0], PieceType.ROW_CLEAR);
            }
            else if (colClearCnt > 0)
            {
                turnColorTo(TheSetOfSpeical[0], PieceType.COLUMN_CLEAR);
            }
            else
            {
                turnColorTo(TheSetOfSpeical[0], PieceType.EMPTY);
            }
        }
        else if (bombCnt > 0)
        {
            if (bombCnt > 1)
            {
                doubleExplode(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
            }
            else if (rowClearCnt > 0 && colClearCnt > 0)
            {
                destroyRow(TheSetOfSpeical[0].GetX() - 1, TheSetOfSpeical[0].GetY());
                destroyRow(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
                destroyRow(TheSetOfSpeical[0].GetX() + 1, TheSetOfSpeical[0].GetY());
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY() - 1);
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY() + 1);
            }
            else if (rowClearCnt > 0)
            {
                destroyRow(TheSetOfSpeical[0].GetX() - 1, TheSetOfSpeical[0].GetY());
                destroyRow(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
                destroyRow(TheSetOfSpeical[0].GetX() + 1, TheSetOfSpeical[0].GetY());
            }
            else if (colClearCnt > 0)
            {
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY() - 1);
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY() + 1);
            }
            else
            {
                explode(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
            }
        }
        else if (rowClearCnt > 0)
        {
            if (rowClearCnt > 1)
            {
                destroyRow(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
            }
            else if (colClearCnt > 0)
            {
                destroyRow(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
            }
            else
            {
                destroyRow(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());

            }
        }
        else if (colClearCnt > 0)
        {
            if (colClearCnt > 1)
            {
                destroyRow(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
            }
            else
            {
                destroyCol(TheSetOfSpeical[0].GetX(), TheSetOfSpeical[0].GetY());
            }
        }




    }

    private void explode(int GetX, int GetY)
    {
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (IsValidPosition(GetX + x, GetY + y) && !(GetGem(GetX + x, GetY + y) is null))
                {
                    if (isSpecial(GetGem(GetX + x, GetY + y)))
                    {
                        Action destroy = ChainDestroy(grid.GetGridObject(GetX + x, GetY + y));
                        destroy();
                    }
                    else
                    {
                        TryDestroyGemGridPosition(grid.GetGridObject(GetX + x, GetY + y), false);
                    }
                }
            }
        }
    }

    private void destroyCol(int GetX, int GetY)
    {
        for (int y = 0; y < gridHeight; y++)
        {

            if (IsValidPosition(GetX, y) && !(GetGem(GetX, y) is null))
            {
                if (isSpecial(GetGem(GetX, y)))
                {
                    Action destroy = ChainDestroy(grid.GetGridObject(GetX, y));
                    destroy();
                }
                else
                {
                    TryDestroyGemGridPosition(grid.GetGridObject(GetX, y), false);
                }
            }

        }
    }

    private void destroyRow(int GetX, int GetY)
    {
        for (int x = 0; x < gridWidth; x++)
        {

            if (IsValidPosition(x, GetY) && !(GetGem(x, GetY) is null))
            {
                if (isSpecial(GetGem(x, GetY)))
                {
                    Action destroy = ChainDestroy(grid.GetGridObject(x, GetY));
                    destroy();
                }
                else
                {
                    TryDestroyGemGridPosition(grid.GetGridObject(x, GetY), false);
                }

            }

        }
    }

    private void destroyAllGrid(int GetX, int GetY)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    TryDestroyGemGridPosition(grid.GetGridObject(x, y), false);
                }
            }
        }
    }

    private void turnColorTo(GemGridPosition theSetOfSpeical, PieceType pieceType)
    {
        switch (pieceType)
        {



            case PieceType.ROW_CLEAR:
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (IsValidPosition(x, y) && GetGem(theSetOfSpeical.GetX(), theSetOfSpeical.GetY()).color.Equals(GetGem(x, y).color))
                        {
                            destroyRow(x, y);
                        }
                    }
                }
                //rowClearCnt += 1;
                break;
            case PieceType.COLUMN_CLEAR:
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (IsValidPosition(x, y) && GetGem(theSetOfSpeical.GetX(), theSetOfSpeical.GetY()).color.Equals(GetGem(x, y).color))
                        {
                            destroyCol(x, y);
                        }
                    }
                }
                break;

            case PieceType.BOMB:
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (IsValidPosition(x, y) && GetGem(theSetOfSpeical.GetX(), theSetOfSpeical.GetY()).color.Equals(GetGem(x, y).color))
                        {
                            explode(x, y);
                        }
                    }
                }
                break;
            case PieceType.CROSS:
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (IsValidPosition(x, y) && GetGem(theSetOfSpeical.GetX(), theSetOfSpeical.GetY()).color.Equals(GetGem(x, y).color))
                        {
                            float r = UnityEngine.Random.Range(0, 1);
                            if (r > 0.5)
                            {
                                destroyRow(x, y);
                            }
                            else
                            {
                                destroyCol(x, y);
                            }
                        }
                    }
                }
                break;
            default:
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (IsValidPosition(x, y) && GetGem(theSetOfSpeical.GetX(), theSetOfSpeical.GetY()).color.Equals(GetGem(x, y).color))
                        {
                            TryDestroyGemGridPosition(grid.GetGridObject(x, y), false);
                        }
                    }
                }
                break;



        }
    }

    private void doubleExplode(int GetX, int GetY)
    {
        for (int x = -3; x < 4; x++)
        {
            for (int y = -3; y < 4; y++)
            {
                if (IsValidPosition(GetX + x, GetY + y) && !(GetGem(GetX + x, GetY + y) is null))
                {
                    if (isSpecial(GetGem(GetX + x, GetY + y)))
                    {
                        Action destroy = ChainDestroy(grid.GetGridObject(GetX + x, GetY + y));
                        destroy();
                    }
                    else
                    {
                        TryDestroyGemGridPosition(grid.GetGridObject(GetX + x, GetY + y), false);
                    }
                }
            }
        }
    }


    public void TryDestroyGemGridPosition(GemGridPosition gemGridPosition, bool isDemolish = false)
    {

        if (gemGridPosition.HasGemGrid())
        {

            score += 100;



            //}
            /*else {*/
            gemGridPosition.DestroyGem();
            //}


            OnGemGridPositionDestroyed?.Invoke(gemGridPosition, EventArgs.Empty);
            gemGridPosition.ClearGemGrid();
        }

        if (gemGridPosition.HasGlass())
        {
            score += 100;

            gemGridPosition.DestroyGlass();
            OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
        }
    }


    // Update is called once per frame
    void Update()
    {

    }



    private void Start()
    {
        state = new State();
        state.SetState(StateType.SettingLevel);
        //defaultLevel();
        if (autoLoadLevel)
        {
            SetLevelSO(levelSO);
        }
    }
    public bool makeMove(int X, int Y)
    {
        List<GemGridPosition> setColor = new List<GemGridPosition>();
        List<GemGridPosition> setSpecial = new List<GemGridPosition>();
        if (grid.GetGridObject(X, Y).GetGemGrid().GetGem().pieceType.Equals(PieceType.NORMAL))
        {
            setColor = GetTheSetOfSameColor(X, Y);
        }
        else
        {
            setSpecial = GetTheSetOfSpecial(X, Y);
        }


        if (setColor.Count < 2 && setSpecial.Count < 1)
        {
            return false;
        }
        if (setColor.Count > 1)
        {
            destroySetOfSameColor(setColor);
            UseMove();
            return true;
        }
        else if (setSpecial.Count > 0)
        {
            destroySpeicalGemSet(setSpecial);
            UseMove();
            return true;
        }
        return false;
    }
    public bool canMakeMove(int X, int Y)
    {
        List<GemGridPosition> setColor = new List<GemGridPosition>();
        List<GemGridPosition> setSpecial = new List<GemGridPosition>();
        if (!IsValidPosition(X, Y)) { return false; }
        if (grid.GetGridObject(X, Y).GetGemGrid().GetGem().pieceType.Equals(PieceType.NORMAL))
        {
            setColor = GetTheSetOfSameColor(X, Y);
        }
        else
        {
            setSpecial = GetTheSetOfSpecial(X, Y);
        }


        if (setColor.Count < 2 && setSpecial.Count < 1)
        {
            return false;
        }
        if (setColor.Count > 1)
        {
            return true;
        }
        else if (setSpecial.Count > 0)
        {
            return true;
        }
        return false;
    }
    public Action ChainDestroy(GemGridPosition gemGridPosition)
    {

        if (GetGemSO(gemGridPosition.GetX(), gemGridPosition.GetY()).pieceType.Equals(PieceType.COLUMN_CLEAR))
        {
            TryDestroyGemGridPosition(gemGridPosition, false);
            return () => destroyCol(gemGridPosition.GetX(), gemGridPosition.GetY());
        }
        else if (GetGemSO(gemGridPosition.GetX(), gemGridPosition.GetY()).pieceType.Equals(PieceType.ROW_CLEAR))
        {
            TryDestroyGemGridPosition(gemGridPosition, false);
            return () => destroyRow(gemGridPosition.GetX(), gemGridPosition.GetY());
        }
        else if (GetGemSO(gemGridPosition.GetX(), gemGridPosition.GetY()).pieceType.Equals(PieceType.BOMB))
        {
            TryDestroyGemGridPosition(gemGridPosition, false);
            return () => explode(gemGridPosition.GetX(), gemGridPosition.GetY());

        }
        else if (GetGemSO(gemGridPosition.GetX(), gemGridPosition.GetY()).pieceType.Equals(PieceType.RAINBOW))
        {
            TryDestroyGemGridPosition(gemGridPosition, false);
            return () => explode(gemGridPosition.GetX(), gemGridPosition.GetY());
        }
        return () => TryDestroyGemGridPosition(gemGridPosition, false);
        //destroySpeicalGemSet()
    }

    public void SpawnNewMissingGridPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

                if (gemGridPosition.IsEmpty())
                {
                    GemSO gem = levelSO.gemList[UnityEngine.Random.Range(0, levelSO.gemList.Count)];
                    GemGrid gemGrid = new GemGrid(gem, x, y);

                    gemGridPosition.SetGemGrid(gemGrid);

                    OnNewGemGridSpawned?.Invoke(gemGrid, new OnNewGemGridSpawnedEventArgs
                    {
                        gemGrid = gemGrid,
                        gemGridPosition = gemGridPosition,
                    });
                }
            }
        }
    }
}