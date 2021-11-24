using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SwapGrid : MonoBehaviour
{
    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };

    [System.Serializable]
    public struct PiecePosition
    {
        public PieceType type;
        public int x;
        public int y;
    };


    [System.Serializable]
    public struct BackgroundPrefab
    {
        public BackgroundType type;
        public GameObject prefab;
    };

    public int xDim;
    public int yDim;
    public float fillTime;

    public PiecePrefab[] piecePrefabs;
    public BackgroundPrefab[] backgroundPrefabs;

    public PiecePosition[] initialPieces;

    private Dictionary<PieceType, GameObject> _piecePrefabDict;

    private Dictionary<BackgroundType, GameObject> _backgroundPrefabDict;
    private GamePiece[,] _pieces;
    private BackgroundPiece[,] backgroundPieces;
    private bool _isFilling;

    public bool IsFilling => _isFilling;
    private bool _inverse = false;

    private GamePiece _pressedPiece;
    private GamePiece _enteredPiece;
    void Awake()
    {

        defaultLevel();

        StartCoroutine(Fill());
    }

    private GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {

        GameObject newPiece = (GameObject)Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity, this.transform);
        _pieces[x, y] = newPiece.GetComponent<GamePiece>();
        _pieces[x, y].Init(x, y, this, type);

        return _pieces[x, y];
    }


    public GamePiece SpawnNewPiece(int x, int y, PieceType type, ColorType variation)
    {
        GameObject newPiece = (GameObject)Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity, this.transform);
        _pieces[x, y] = newPiece.GetComponent<GamePiece>();
        _pieces[x, y].Init(x, y, this, type);
        _pieces[x, y].ColorComponent.SetColor(variation);
        return _pieces[x, y];
    }
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2((transform.position.x - xDim / 2.0f + x) * _backgroundPrefabDict[BackgroundType.Normal].transform.localScale.x,
                           (transform.position.y + yDim / 2.0f - y) * _backgroundPrefabDict[BackgroundType.Normal].transform.localScale.y);
    }

    private IEnumerator Fill()
    {
        bool needsRefil = true;
        _isFilling = true;

        refill: while (needsRefil)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {

                _inverse = !_inverse;
                yield return new WaitForSeconds(fillTime);
            }

            needsRefil = ClearAllValidMatches();
            
        }
        yield return new WaitForSeconds(fillTime);
        Debug.Log("{");
        for (int i = 0; i < xDim; i++)
        {
            string s = "{";

            for (int j = 0; j < yDim; j++)
            {
                s += "," + _pieces[i, j].ColorComponent.Color;
            }
            s += "}";
            Debug.Log(s);
        }


        Debug.Log("}");

        if (!isThereSwap())
        {
            needsRefil = true;
            randomEveryThing();
            yield return new WaitForSeconds(fillTime);
            goto refill;
        }

        _isFilling = false;
    }

    /// <summary>
    /// One pass through all grid cells, moving them down one grid, if possible.
    /// </summary>
    /// <returns> returns true if at least one piece is moved down</returns>
    private bool FillStep()
    {
        bool movedPiece = false;
        // y = 0 is at the top, we ignore the last row, since it can't be moved down.


        for (int y = yDim - 1; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                //Debug.Log(y + "," + loopX);
                int x = loopX;
                if (_inverse) { x = xDim - 1 - loopX; }
                GamePiece piece = _pieces[x, y];
                if (backgroundPieces[x, y].Type.Equals(BackgroundType.Outer) && piece.Type.Equals(PieceType.COUNT1))
                {
                    Destroy(piece.gameObject);
                    backgroundPieces[x, y].OuterComponent.inc(piece.ColorComponent.Color);
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                }

                if (!piece.IsMovable()) continue;




                if (backgroundPieces[x, y].Type.Equals(BackgroundType.PortalIn))
                {
                    PortalComponent connectedTo = backgroundPieces[x, y].GetComponent<PortalComponent>()._connectedToPortalComponent;
                    GamePiece pieceBelowPortal = _pieces[connectedTo._BackgroundRefernce.X, connectedTo._BackgroundRefernce.Y];

                    if (pieceBelowPortal.Type == PieceType.EMPTY)
                    {
                        teleportPiece(x, y, connectedTo._BackgroundRefernce.X, connectedTo._BackgroundRefernce.Y, piece, pieceBelowPortal);
                    }
                }

                if (y == yDim - 1) continue;

                GamePiece pieceBelow = _pieces[x, y + 1];
                BackgroundPiece backgroundBelow = backgroundPieces[x, y + 1];
                if (backgroundPieces[x, y].Type.Equals(BackgroundType.Spawner) && emptyPiece(x, y + 1))
                {
                    spawnPieceBlow(x, y, piece, pieceBelow);
                    movedPiece = true;
                }
                else if (emptyPiece(x, y + 1))
                {

                    movePieceBlow(x, y, piece, pieceBelow);
                    movedPiece = true;
                }
                else
                {
                    movedPiece = movedPiece || moveDiagonally(x, y, piece);

                }
            }
        }

        //the highest row(0) is a special case, we must fill it with new pieces if empty
        for (int x = 0; x < xDim; x++)
        {
            GamePiece pieceBelow = _pieces[x, 0];

            if (pieceBelow.Type != PieceType.EMPTY) continue;
            if (backgroundPieces[x, 0].Type.Equals(BackgroundType.Spawner) && emptyPiece(x, 0))
            {
                Destroy(pieceBelow.gameObject);
                _pieces[x, 0] = backgroundPieces[x, 0].SpawnComponent.spawnPiece();
                _pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                movedPiece = true;
            }

        }

        return movedPiece;
    }

    private void spawnPieceBlow(int x, int y, GamePiece piece, GamePiece pieceBelow)
    {
        Destroy(pieceBelow.gameObject);
        piece.MovableComponent.Move(x, y + 1, fillTime);
        _pieces[x, y + 1] = piece;
        _pieces[x, y] = backgroundPieces[x, y].SpawnComponent.spawnPiece();
    }
    private void movePieceBlow(int x, int y, GamePiece piece, GamePiece pieceBelow)
    {
        Destroy(pieceBelow.gameObject);
        piece.MovableComponent.Move(x, y + 1, fillTime);
        _pieces[x, y + 1] = piece;
        SpawnNewPiece(x, y, PieceType.EMPTY);
    }
    private bool moveDiagonally(int x, int y, GamePiece piece)
    {
        bool movedPiece = false;
        for (int diag = -1; diag <= 1; diag++)
        {
            if (diag == 0) continue;

            int diagX = x + diag;

            if (_inverse)
            {
                diagX = x - diag;
            }

            if (diagX < 0 || diagX >= xDim) continue;

            GamePiece diagonalPiece = _pieces[diagX, y + 1];

            if (diagonalPiece.Type != PieceType.EMPTY) continue;

            bool hasPieceAbove = true;

            for (int aboveY = y; aboveY >= 0; aboveY--)
            {
                GamePiece pieceAbove = _pieces[diagX, aboveY];

                if (pieceAbove.IsMovable())
                {
                    break;
                }
                else if (/*!pieceAbove.IsMovable() && */pieceAbove.Type != PieceType.EMPTY)
                {
                    hasPieceAbove = false;
                    break;
                }
            }

            if (hasPieceAbove) continue;

            Destroy(diagonalPiece.gameObject);
            piece.MovableComponent.Move(diagX, y + 1, fillTime);
            _pieces[diagX, y + 1] = piece;
            SpawnNewPiece(x, y, PieceType.EMPTY);
            movedPiece = true;
            break;
        }
        return movedPiece;
    }
    private void teleportPiece(int x, int y, int toX, int toY, GamePiece piece, GamePiece pieceTo)
    {
        Destroy(pieceTo.gameObject);
        SpawnNewPiece(x, y, PieceType.EMPTY);
        SpawnNewPiece(toX, toY, piece.Type, piece.ColorComponent.Color);
        Destroy(piece.gameObject);
    }
    private bool emptyPiece(int x, int y)
    {
        GamePiece pieceBelow = _pieces[x, y];
        BackgroundPiece backgroundBelow = backgroundPieces[x, y];
        return (pieceBelow.Type == PieceType.EMPTY) && (backgroundBelow.Type == BackgroundType.Normal || backgroundBelow.Type == BackgroundType.PortalIn || backgroundBelow.Type == BackgroundType.Spawner);
    }
    private static bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1)
            || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }
    private void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        //if (_gameOver) { return; }

        if (!piece1.IsMovable() || !piece2.IsMovable()) return;

        _pieces[piece1.X, piece1.Y] = piece2;
        _pieces[piece2.X, piece2.Y] = piece1;

        if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null
                                                         || piece1.Type == PieceType.RAINBOW || piece2.Type == PieceType.RAINBOW)
        {
            int piece1X = piece1.X;
            int piece1Y = piece1.Y;

            piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
            piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

            if (piece1.Type == PieceType.RAINBOW && piece1.IsClearable() && piece2.IsColored())
            {
                ClearColorPiece clearColor = piece1.GetComponent<ClearColorPiece>();

                if (clearColor)
                {
                    clearColor.Color = piece2.ColorComponent.Color;
                }

                ClearPiece(piece1.X, piece1.Y);
            }

            if (piece2.Type == PieceType.RAINBOW && piece2.IsClearable() && piece1.IsColored())
            {
                ClearColorPiece clearColor = piece2.GetComponent<ClearColorPiece>();

                if (clearColor)
                {
                    clearColor.Color = piece1.ColorComponent.Color;
                }

                ClearPiece(piece2.X, piece2.Y);
            }

            ClearAllValidMatches();

            // special pieces get cleared, event if they are not matched
            if (piece1.Type == PieceType.ROW_CLEAR || piece1.Type == PieceType.COLUMN_CLEAR)
            {
                ClearPiece(piece1.X, piece1.Y);
            }

            if (piece2.Type == PieceType.ROW_CLEAR || piece2.Type == PieceType.COLUMN_CLEAR)
            {
                ClearPiece(piece2.X, piece2.Y);
            }

            _pressedPiece = null;
            _enteredPiece = null;

            StartCoroutine(Fill());

            // TODO consider doing this using delegates
            //level.OnMove();
        }
        else
        {
            _pieces[piece1.X, piece1.Y] = piece1;
            _pieces[piece2.X, piece2.Y] = piece2;
        }
    }

    public void PressPiece(GamePiece piece)
    {
        _pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        _enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if (IsAdjacent(_pressedPiece, _enteredPiece))
        {
            SwapPieces(_pressedPiece, _enteredPiece);
        }
    }
    private bool ClearPiece(int x, int y)
    {
        if (!_pieces[x, y].IsClearable() || _pieces[x, y].ClearableComponent.IsBeingCleared) return false;

        _pieces[x, y].ClearableComponent.Clear();
        SpawnNewPiece(x, y, PieceType.EMPTY);

        ClearObstacles(x, y);

        return true;

    }
    private void ClearObstacles(int x, int y)
    {
        for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
        {
            if (adjacentX == x || adjacentX < 0 || adjacentX >= xDim) continue;

            if (_pieces[adjacentX, y].Type != PieceType.BUBBLE || !_pieces[adjacentX, y].IsClearable()) continue;

            _pieces[adjacentX, y].ClearableComponent.Clear();
            SpawnNewPiece(adjacentX, y, PieceType.EMPTY);
        }

        for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
        {
            if (adjacentY == y || adjacentY < 0 || adjacentY >= yDim) continue;

            if (_pieces[x, adjacentY].Type != PieceType.BUBBLE || !_pieces[x, adjacentY].IsClearable()) continue;

            _pieces[x, adjacentY].ClearableComponent.Clear();
            SpawnNewPiece(x, adjacentY, PieceType.EMPTY);
        }
    }
    private List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (!piece.IsColored()) return null;
        var color = piece.ColorComponent.Color;
        var horizontalPieces = new List<GamePiece>();
        var verticalPieces = new List<GamePiece>();
        var matchingPieces = new List<GamePiece>();

        // First check horizontal
        horizontalPieces.Add(piece);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int xOffset = 1; xOffset < xDim; xOffset++)
            {
                int x;

                if (dir == 0)
                { // Left
                    x = newX - xOffset;
                }
                else
                { // right
                    x = newX + xOffset;
                }

                // out-of-bounds
                if (x < 0 || x >= xDim) { break; }

                // piece is the same color?
                if (_pieces[x, newY].IsColored() && _pieces[x, newY].ColorComponent.Color == color)
                {
                    horizontalPieces.Add(_pieces[x, newY]);
                }
                else
                {
                    break;
                }
            }
        }

        if (horizontalPieces.Count >= 3)
        {
            for (int i = 0; i < horizontalPieces.Count; i++)
            {
                matchingPieces.Add(horizontalPieces[i]);
            }
        }

        // Traverse vertically if we found a match (for L and T shape)
        if (horizontalPieces.Count >= 3)
        {
            for (int i = 0; i < horizontalPieces.Count; i++)
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int yOffset = 1; yOffset < yDim; yOffset++)
                    {
                        int y;

                        if (dir == 0)
                        { // Up
                            y = newY - yOffset;
                        }
                        else
                        { // Down
                            y = newY + yOffset;
                        }

                        if (y < 0 || y >= yDim)
                        {
                            break;
                        }

                        if (_pieces[horizontalPieces[i].X, y].IsColored() && _pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                        {
                            verticalPieces.Add(_pieces[horizontalPieces[i].X, y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (verticalPieces.Count < 2)
                {
                    verticalPieces.Clear();
                }
                else
                {
                    for (int j = 0; j < verticalPieces.Count; j++)
                    {
                        matchingPieces.Add(verticalPieces[j]);
                    }
                    break;
                }
            }
        }

        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }


        // Didn't find anything going horizontally first,
        // so now check vertically
        horizontalPieces.Clear();
        verticalPieces.Clear();
        verticalPieces.Add(piece);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int yOffset = 1; yOffset < xDim; yOffset++)
            {
                int y;

                if (dir == 0)
                { // Up
                    y = newY - yOffset;
                }
                else
                { // Down
                    y = newY + yOffset;
                }

                // out-of-bounds
                if (y < 0 || y >= yDim) { break; }

                // piece is the same color?
                if (_pieces[newX, y].IsColored() && _pieces[newX, y].ColorComponent.Color == color)
                {
                    verticalPieces.Add(_pieces[newX, y]);
                }
                else
                {
                    break;
                }
            }
        }

        if (verticalPieces.Count >= 3)
        {
            for (int i = 0; i < verticalPieces.Count; i++)
            {
                matchingPieces.Add(verticalPieces[i]);
            }
        }

        // Traverse horizontally if we found a match (for L and T shape)
        if (verticalPieces.Count >= 3)
        {
            for (int i = 0; i < verticalPieces.Count; i++)
            {
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int xOffset = 1; xOffset < yDim; xOffset++)
                    {
                        int x;

                        if (dir == 0)
                        { // Left
                            x = newX - xOffset;
                        }
                        else
                        { // Right
                            x = newX + xOffset;
                        }

                        if (x < 0 || x >= xDim)
                        {
                            break;
                        }

                        if (_pieces[x, verticalPieces[i].Y].IsColored() && _pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                        {
                            horizontalPieces.Add(_pieces[x, verticalPieces[i].Y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (horizontalPieces.Count < 2)
                {
                    horizontalPieces.Clear();
                }
                else
                {
                    for (int j = 0; j < horizontalPieces.Count; j++)
                    {
                        matchingPieces.Add(horizontalPieces[j]);
                    }
                    break;
                }
            }
        }

        if (matchingPieces.Count >= 3)
        {
            return matchingPieces;
        }

        return null;
    }
    private bool ClearAllValidMatches()
    {
        bool needsRefill = false;

        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (!_pieces[x, y].IsClearable()) continue;

                List<GamePiece> match = GetMatch(_pieces[x, y], x, y);

                if (match == null) continue;

                PieceType specialPieceType = PieceType.COUNT;
                GamePiece randomPiece = match[UnityEngine.Random.Range(0, match.Count)];
                int specialPieceX = randomPiece.X;
                int specialPieceY = randomPiece.Y;

                // Spawning special pieces
                if (match.Count == 4)
                {
                    if (_pressedPiece == null || _enteredPiece == null)
                    {
                        specialPieceType = (PieceType)UnityEngine.Random.Range((int)PieceType.ROW_CLEAR, (int)PieceType.COLUMN_CLEAR);
                    }
                    else if (_pressedPiece.Y == _enteredPiece.Y)
                    {
                        specialPieceType = PieceType.ROW_CLEAR;
                    }
                    else
                    {
                        specialPieceType = PieceType.COLUMN_CLEAR;
                    }
                } // Spawning a rainbow piece
                else if (match.Count >= 5)
                {
                    specialPieceType = PieceType.RAINBOW;
                }

                for (int i = 0; i < match.Count; i++)
                {
                    if (!ClearPiece(match[i].X, match[i].Y)) continue;

                    needsRefill = true;

                    if (match[i] != _pressedPiece && match[i] != _enteredPiece) continue;

                    specialPieceX = match[i].X;
                    specialPieceY = match[i].Y;
                }

                // Setting their colors
                if (specialPieceType == PieceType.COUNT) continue;

                Destroy(_pieces[specialPieceX, specialPieceY]);
                GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

                if ((specialPieceType == PieceType.ROW_CLEAR || specialPieceType == PieceType.COLUMN_CLEAR)
                    && newPiece.IsColored() && match[0].IsColored())
                {
                    newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                }
                else if (specialPieceType == PieceType.RAINBOW && newPiece.IsColored())
                {
                    newPiece.ColorComponent.SetColor(ColorType.ANY);
                }
            }
        }

        return needsRefill;
    }
    public void defaultLevel()
    {
        _piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!_piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                _piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }
        _backgroundPrefabDict = new Dictionary<BackgroundType, GameObject>();
        for (int i = 0; i < backgroundPrefabs.Length; i++)
        {
            if (!_backgroundPrefabDict.ContainsKey(backgroundPrefabs[i].type))
            {
                _backgroundPrefabDict.Add(backgroundPrefabs[i].type, backgroundPrefabs[i].prefab);
            }
        }
        backgroundPieces = new BackgroundPiece[xDim, yDim];
        for (int y = 0; y < xDim; y++)
        {
            GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.Spawner], GetWorldPosition(y, 0), Quaternion.identity);
            background.transform.parent = transform;
            backgroundPieces[y, 0] = background.GetComponent<BackgroundPiece>();
            backgroundPieces[y, 0].Init(y, 0, this, BackgroundType.Spawner);
        }
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 1; y < yDim; y++)
            {
                GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.Normal], GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
                backgroundPieces[x, y] = background.GetComponent<BackgroundPiece>();
                backgroundPieces[x, y].Init(x, y, this, BackgroundType.Normal);
            }
        }
        _pieces = new GamePiece[xDim, yDim];

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (backgroundPieces[x, y].Type == BackgroundType.Block)
                {

                    SpawnNewPiece(x, y, PieceType.Block);

                }
                else
                {
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                }

            }
        }
    }

    public void level1()
    {
        _piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!_piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                _piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }
        _backgroundPrefabDict = new Dictionary<BackgroundType, GameObject>();
        for (int i = 0; i < backgroundPrefabs.Length; i++)
        {
            if (!_backgroundPrefabDict.ContainsKey(backgroundPrefabs[i].type))
            {
                _backgroundPrefabDict.Add(backgroundPrefabs[i].type, backgroundPrefabs[i].prefab);
            }
        }
        backgroundPieces = new BackgroundPiece[xDim, yDim];
        for (int y = 0; y < xDim; y++)
        {
            GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.Spawner], GetWorldPosition(y, 0), Quaternion.identity);
            background.transform.parent = transform;
            backgroundPieces[y, 0] = background.GetComponent<BackgroundPiece>();
            backgroundPieces[y, 0].Init(y, 0, this, BackgroundType.Spawner);
        }
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 1; y < 5; y++)
            {
                GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.Normal], GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
                backgroundPieces[x, y] = background.GetComponent<BackgroundPiece>();
                backgroundPieces[x, y].Init(x, y, this, BackgroundType.Normal);
            }
        }
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 5; y < 6; y++)
            {
                GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.PortalIn], GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
                backgroundPieces[x, y] = background.GetComponent<BackgroundPiece>();
                backgroundPieces[x, y].Init(x, y, this, BackgroundType.PortalIn);
                backgroundPieces[x, y].PortalComponent.init(x, x, null);
            }
        }
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 6; y < 7; y++)
            {
                GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.Block], GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
                backgroundPieces[x, y] = background.GetComponent<BackgroundPiece>();
                backgroundPieces[x, y].Init(x, y, this, BackgroundType.Block);

            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 7; y < 8; y++)
            {
                GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.PortalOut], GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
                backgroundPieces[x, y] = background.GetComponent<BackgroundPiece>();
                backgroundPieces[x, y].Init(x, y, this, BackgroundType.PortalOut);
                backgroundPieces[x, y].PortalComponent.init(x, x, backgroundPieces[x, y - 2].PortalComponent);
                backgroundPieces[x, y - 2].PortalComponent._connectedToPortalComponent = backgroundPieces[x, y].PortalComponent;
            }
        }
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 8; y < yDim; y++)
            {
                GameObject background = Instantiate(_backgroundPrefabDict[BackgroundType.Normal], GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
                backgroundPieces[x, y] = background.GetComponent<BackgroundPiece>();
                backgroundPieces[x, y].Init(x, y, this, BackgroundType.Normal);
            }
        }
        BackgroundPiece go = backgroundPieces[4, 9];
        Destroy(go.gameObject);
        GameObject gobackground = Instantiate(_backgroundPrefabDict[BackgroundType.Block], GetWorldPosition(4, 9), Quaternion.identity);
        gobackground.transform.parent = transform;
        backgroundPieces[4, 9] = gobackground.GetComponent<BackgroundPiece>();
        backgroundPieces[4, 9].Init(4, 9, this, BackgroundType.Block);

        // instantiating pieces
        _pieces = new GamePiece[xDim, yDim];

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (backgroundPieces[x, y].Type == BackgroundType.Block)
                {

                    SpawnNewPiece(x, y, PieceType.Block);

                }
                else
                {
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                }

            }
        }
    }
    public void ClearRow(int row)
    {
        for (int x = 0; x < xDim; x++)
        {
            ClearPiece(x, row);
        }
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < yDim; y++)
        {
            ClearPiece(column, y);
        }
    }

    public void ClearColor(ColorType color)
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if ((_pieces[x, y].IsColored() && _pieces[x, y].ColorComponent.Color == color)
                    || (color == ColorType.ANY))
                {
                    ClearPiece(x, y);
                }
            }
        }
    }
    private bool isThereSwap()
    {
        var toReturn = false;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                int[] xDir = new int[] { 0, 1, 0, -1 };
                int[] yDir = new int[] { 1, 0, -1, 0 };
                for (int z = 0; z < 4; z++)
                {
                    if (!isInsideTheGrid(x + xDir[z], y + yDir[z]))
                    {
                        continue;
                    }
                    var piece1 = _pieces[x, y];
                    var piece2 = _pieces[x + xDir[z], y + yDir[z]];
                    if (!piece1.IsMovable() || !piece2.IsMovable()) continue;
                    _pieces[piece1.X, piece1.Y] = piece2;
                    _pieces[piece2.X, piece2.Y] = piece1;

                    if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null
                                                                     || piece1.Type == PieceType.RAINBOW || piece2.Type == PieceType.RAINBOW)
                    {

                        toReturn = true;
                    }

                    _pieces[piece1.X, piece1.Y] = piece2;
                    _pieces[piece2.X, piece2.Y] = piece1;
                    //level.OnMove();
                    
                }
            }
        }
        Debug.Log("{");
        for (int i = 0; i < xDim; i++)
        {
            string s = "{";

            for (int j = 0; j < yDim; j++)
            {
                s += "," + _pieces[i, j].ColorComponent.Color;
            }
            s += "}";
            Debug.Log(s);
        }


        Debug.Log("}");
        return toReturn;
    }
    private void randomEveryThing()
    {

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (_pieces[x, y].Type.Equals(PieceType.NORMAL))
                {
                    
                    Destroy(_pieces[x, y].transform.gameObject);
                    _pieces[x, y] = null;
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                }
            }
        }

      
        
        //List<Tuple<int, int>> source = new List<Tuple<int, int>>();
        //List<Tuple<int, int>> toRandom = new List<Tuple<int, int>>();

        //for (int x = 0; x < xDim; x++)
        //{
        //    for (int y = 0; y < yDim; y++)
        //    {
        //        if (_pieces[x, y].Type.Equals(PieceType.NORMAL))
        //        {
        //            toRandom.Add(new Tuple<int, int>(x, y));
        //            source.Add(new Tuple<int, int>(x, y));
        //        }
        //    }
        //}

        //int last = toRandom.Count;
        //for (var i = 0; i < last; ++i)
        //{
        //    var r = UnityEngine.Random.Range(i, last);
        //    Tuple<int, int> tmp1 = toRandom[i];
        //    Tuple<int, int> tmp2 = toRandom[r];
        //    toRandom[i] = tmp2;
        //    toRandom[r] = tmp1;
        //}
        //string s = "{";
        //for (int i = 0; i < toRandom.Count; i++)
        //{

        //    s += ",[" + toRandom[i].Item1 + " ," + toRandom[i].Item2 + "]";
        //}
        //s += "}";
        //Debug.Log(s);
        //s = "{";
        //for (int i = 0; i < source.Count; i++)
        //{
        //    s += ",[" + source[i].Item1 + " ," + source[i].Item2 + "]";
        //}
        //s += "}";
        //Debug.Log(s);
        //for (int i = 0; i < toRandom.Count; i++)
        //{
        //    //try { 
        //    if ((source[i].Item1!= toRandom[i].Item1&& source[i].Item2 != toRandom[i].Item2) &&!(_pieces[source[i].Item1, source[i].Item2].transform.gameObject is null) && !(_pieces[toRandom[i].Item1, toRandom[i].Item2].transform.gameObject is null))
        //    {
        //        Destroy(_pieces[source[i].Item1, source[i].Item2].transform.gameObject);
        //        Destroy(_pieces[toRandom[i].Item1, toRandom[i].Item2].transform.gameObject);
        //        int xSour = source[i].Item1;
        //        int ySour = source[i].Item2;
        //        int xToRandom = toRandom[i].Item1;
        //        int yToRandom = toRandom[i].Item2;
        //        ref GamePiece tmp1 = ref _pieces[xSour, xSour];
        //        ref GamePiece tmp2 = ref _pieces[xToRandom, yToRandom];
        //        _pieces[xSour, xSour] = tmp2;
        //        _pieces[xToRandom, yToRandom] = tmp1;

        //        SpawnNewPiece(xSour, ySour, PieceType.NORMAL, _pieces[xSour, xSour].ColorComponent.Color);
        //        SpawnNewPiece(xToRandom, yToRandom, PieceType.NORMAL, _pieces[xToRandom, yToRandom].ColorComponent.Color);
        //        //Vector3 tmpV = _pieces[xSour, ySour].gameObject.transform.position;
        //        //_pieces[xSour, xSour].gameObject.transform.position = _pieces[xToRandom, yToRandom].gameObject.transform.position;
        //        //_pieces[xToRandom, yToRandom].gameObject.transform.position = tmpV;

        //        //var tmpP = _pieces[xSour, xSour];
        //        //_pieces[xSour, xSour] = _pieces[xToRandom, yToRandom];
        //        //_pieces[xToRandom, yToRandom] = tmpP;

        //        //swapTwoColors(xSour,ySour,xToRandom,yToRandom);
        //    }

        //    //}catch (System.Exception e)
        //    //{
        //    //    Debug.Log(i);
        //    //    Debug.Log(source.Count +" "+ toRandom.Count);
        //    //    Debug.Log(source[i].Item1 + " " + source[i].Item2 + " " + toRandom[i].Item1 + " " + toRandom[i].Item2);
        //    //}
        //    //var tmp1 = _pieces[source[i].Item1, source[i].Item2];
        //    //_pieces[source[i].Item1, source[i].Item2] = _pieces[toRandom[i].Item1, toRandom[i].Item2];
        //    //_pieces[toRandom[i].Item1, toRandom[i].Item2] = tmp1;

        //    //ColorType cSour = (ColorType)((int)_pieces[source[i].Item1, source[i].Item2].ColorComponent.Color);


        //    //ColorType cToRandom = (ColorType)((int)_pieces[toRandom[i].Item1, toRandom[i].Item2].ColorComponent.Color);


        //}
    }
    public bool isInsideTheGrid(int x, int y)
    {
        if (x < 0 || y < 0 || y >= yDim || x >= xDim)
        {
            return false;
        }
        return true;
    }
    //private void swapTwoColors(int xSour, int ySour, int xToRandom, int yToRandom)
    //{
    //    Vector3 tmpV = _pieces[xSour, ySour].gameObject.transform.position;
    //    _pieces[xSour, xSour].gameObject.transform.position = _pieces[xToRandom, yToRandom].gameObject.transform.position;
    //    _pieces[xToRandom, yToRandom].gameObject.transform.position = tmpV;


    //}
}



