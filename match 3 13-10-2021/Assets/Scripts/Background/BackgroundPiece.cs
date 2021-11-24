using UnityEngine;

public class BackgroundPiece : MonoBehaviour
{
    public int score;

    private int _x;
    private int _y;

    public int X
    {
        get => _x;
        //set { if (IsMovable()) { _x = value; } }
    }

    public int Y
    {
        get => _y;
        //set { if (IsMovable()) { _y = value; } }
    }
    
    private BackgroundType _type;

    public BackgroundType Type => _type;

    private SwapGrid _grid;

    public SwapGrid GridRef => _grid;

    private SpawnComponent _SpawnComponent;

    public SpawnComponent SpawnComponent => _SpawnComponent;

    private PortalComponent _portalComponent;
    public PortalComponent PortalComponent => _portalComponent;

    public OuterComponent _outerComponent;

    public OuterComponent OuterComponent => _outerComponent;
    //private ColorPiece _colorComponent;

    //public ColorPiece ColorComponent => _colorComponent;

    //private ClearablePiece _clearableComponent;

    //public ClearablePiece ClearableComponent => _clearableComponent;

    private void Awake()
    {
        _SpawnComponent = GetComponent<SpawnComponent>();
        _portalComponent = GetComponent<PortalComponent>();
        //_colorComponent = GetComponent<ColorPiece>();
        //_clearableComponent = GetComponent<ClearablePiece>();
    }

    public void Init(int x, int y, SwapGrid grid, BackgroundType type)
    {
        _x = x;
        _y = y;
        _grid = grid;
        _type = type;
        if (!(_SpawnComponent is null))
            _SpawnComponent._gridRefernce = grid;
    }

    //private void OnMouseEnter()
    //{
    //    _grid.EnterPiece(this);
    //}

    //private void OnMouseDown()
    //{
    //    _grid.PressPiece(this);
    //}

    //private void OnMouseUp()
    //{
    //    _grid.ReleasePiece();
    //}

    //public bool IsMovable()
    //{
    //    return _movableComponent != null && !_movableComponent.isTempLocked;
    //}
    //public bool IsMovableByUser()
    //{
    //    return _movableComponent != null && !_movableComponent.isTempLocked && _movableComponent.isMoveableByUser;
    //}
    //public bool IsColored()
    //{
    //    return _colorComponent != null;
    //}

    //public bool IsClearable()
    //{
    //    return _clearableComponent != null;
    //}

}
