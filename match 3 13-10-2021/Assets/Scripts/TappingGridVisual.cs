using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using System;

public class TappingGridVisual : MonoBehaviour
{
    public event EventHandler OnStateChanged;
    //public event EventHandler OnStateChangedHuman;


    [SerializeField] private Transform pfGemGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform pfBackgroundGridVisual;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TappingGrid match3;

    private Grid<GemGridPosition> grid;
    private Dictionary<GemGrid, GemGridVisual> gemGridDictionary;
    private Dictionary<GemGridPosition, GlassGridVisual> glassGridDictionary;

    private bool isSetup;
    private State _state;
    public State state { get { return _state; } }

    private float busyTimer;
    private Action onBusyTimerElapsedAction;

    private int startDragX;
    private int startDragY;
    private Vector3 startDragMouseWorldPosition;

    private void Awake()
    {
        _state = new State();
        SetState(StateType.Busy);
        // TODO: work with the states
        isSetup = false;

        match3.OnLevelSet += Match3_OnLevelSet;
    }

    private void Match3_OnLevelSet(object sender, TappingGrid.OnLevelSetEventArgs e)
    {
        FunctionTimer.Create(() =>
        {
            Setup(sender as TappingGrid, e.grid);
        }, .1f);
    }

    public void Setup(TappingGrid match3, Grid<GemGridPosition> grid)
    {
        this.match3 = match3;
        this.grid = grid;

        float cameraYOffset = 1f;
        cameraTransform.position = new Vector3(grid.GetWidth() * .5f, grid.GetHeight() * .5f + cameraYOffset, cameraTransform.position.z);

        match3.OnGemGridPositionDestroyed += Match3_OnGemGridPositionDestroyed;
        match3.OnNewGemGridSpawned += Match3_OnNewGemGridSpawned;
        match3.OnGemSpecialGridChanged += Match3_OnSpeicalGemGridChanged;

        // Initialize Visual
        gemGridDictionary = new Dictionary<GemGrid, GemGridVisual>();
        glassGridDictionary = new Dictionary<GemGridPosition, GlassGridVisual>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
                GemGrid gemGrid = gemGridPosition.GetGemGrid();

                Vector3 position = grid.GetWorldPosition(x, y);
                position = new Vector3(position.x, 12);

                // Visual Transform
                Transform gemGridVisualTransform = Instantiate(pfGemGridVisual, position, Quaternion.identity);
                gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sprite = gemGrid.GetGem().sprite;

                GemGridVisual gemGridVisual = new GemGridVisual(gemGridVisualTransform, gemGrid);

                gemGridDictionary[gemGrid] = gemGridVisual;

                // Glass Visual Transform
                Transform glassGridVisualTransform = Instantiate(pfGlassGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity);

                GlassGridVisual glassGridVisual = new GlassGridVisual(glassGridVisualTransform, gemGridPosition);

                glassGridDictionary[gemGridPosition] = glassGridVisual;

                // Background Grid Visual
                Instantiate(pfBackgroundGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity);
            }
        }

        SetBusyState(.5f, () => SetState(StateType.TryFindMatches));

        isSetup = true;
    }

    private void Match3_OnSpeicalGemGridChanged(object sender, TappingGrid.OnNewGemGridSpawnedEventArgs e)
    {

        Vector3 position = e.gemGridPosition.GetWorldPosition();

        position = new Vector3(position.x, 12);

        Transform gemGridVisualTransform = Instantiate(pfGemGridVisual, position, Quaternion.identity);
        gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sprite = e.gemGrid.GetGem().sprite;

        GemGridVisual gemGridVisual = new GemGridVisual(gemGridVisualTransform, e.gemGrid);

        gemGridDictionary[e.gemGrid] = gemGridVisual;
    }

    private void Match3_OnNewGemGridSpawned(object sender, TappingGrid.OnNewGemGridSpawnedEventArgs e)
    {
        Vector3 position = e.gemGridPosition.GetWorldPosition();
        position = new Vector3(position.x, 12);

        Transform gemGridVisualTransform = Instantiate(pfGemGridVisual, position, Quaternion.identity);
        gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sprite = e.gemGrid.GetGem().sprite;

        GemGridVisual gemGridVisual = new GemGridVisual(gemGridVisualTransform, e.gemGrid);

        gemGridDictionary[e.gemGrid] = gemGridVisual;
    }

    private void Match3_OnGemGridPositionDestroyed(object sender, System.EventArgs e)
    {
        GemGridPosition gemGridPosition = sender as GemGridPosition;
        if (gemGridPosition != null && gemGridPosition.GetGemGrid() != null)
        {
            gemGridDictionary.Remove(gemGridPosition.GetGemGrid());
        }
    }

    private void Update()
    {
        if (!isSetup) return;

        UpdateVisual();

        switch (_state.GetState())
        {
            case StateType.Busy:
                busyTimer -= Time.deltaTime;
                if (busyTimer <= 0f)
                {
                    onBusyTimerElapsedAction();
                    //TrySetStateWaitingForUser();
                }
                break;
            case StateType.WaitingForUser:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
                    grid.GetXY(mouseWorldPosition, out startDragX, out startDragY);

                    makeMove(startDragX, startDragY);
                   
                    
                }

                break;
            case StateType.TryFindMatches:
                //TODO: handle this
                //if (match3.TryFindMatchesAndDestroyThem())
                //{
                SetBusyState(.3f, () =>
                {
                    match3.FallGemsIntoEmptyPositions();

                    SetBusyState(.3f, () =>
                    {
                        match3.SpawnNewMissingGridPositions();

                        SetBusyState(.5f, () => { SetState(StateType.TryFindMatches); TrySetStateWaitingForUser(); });
                    });

                });
                //TrySetStateWaitingForUser();
                //else
                //{
                //    TrySetStateWaitingForUser();
                //}
                break;
            case StateType.GameOver:
                break;
        }
    }
    public bool makeMove(int X,int Y)
    {
        if (match3.makeMove(X, Y)) {
            SetBusyState(.5f, () => SetState(StateType.TryFindMatches));
            return true;
        }
        return false;

       
    }
    public List<GemGridPosition> GetTheSetOfSameColor(int x, int y)
    {

        List<GemGridPosition> TheSetOfSameColor = new List<GemGridPosition>();
        if (!match3.IsValidPosition(x, y)) return TheSetOfSameColor;
        Queue<GemGridPosition> queue = new Queue<GemGridPosition>();
        TheSetOfSameColor.Add(grid.GetGridObject(x, y));
        queue.Enqueue(grid.GetGridObject(x, y));
        int[] dirx = new int[] { 0, 1, 0, -1 };
        int[] diry = new int[] { 1, 0, -1, 0 };
        while (queue.Count > 0)
        {
            GemGridPosition q = queue.Dequeue();
            for (int dir = 0; dir < dirx.Length; dir++)
            {
                if (match3.IsValidPosition(dirx[dir] + q.GetX(), diry[dir] + q.GetY()))
                {
                    if (GetGem(dirx[dir] + q.GetX(), diry[dir] + q.GetY()).color.Equals(GetGem(q.GetX(), q.GetY()).color))
                    {
                        TheSetOfSameColor.Add(grid.GetGridObject(dirx[dir] + q.GetX(), diry[dir] + q.GetY()));
                        queue.Enqueue(grid.GetGridObject(dirx[dir] + q.GetX(), diry[dir] + q.GetY()));
                    }
                }
            }
        }
        return TheSetOfSameColor;
    }
    private void UpdateVisual()
    {
        foreach (GemGrid gemGrid in gemGridDictionary.Keys)
        {
            gemGridDictionary[gemGrid].Update();
        }
    }

    

    private void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction)
    {
        SetState(StateType.Busy);
        this.busyTimer = busyTimer;
        this.onBusyTimerElapsedAction = onBusyTimerElapsedAction;
    }

    private void TrySetStateWaitingForUser()
    {
        if (match3.TryIsGameOver())
        {
            // Game Over!
            Debug.Log("Game Over!");
            SetState(StateType.GameOver);
        }
        else
        {
            // Keep Playing
            SetState(StateType.WaitingForUser);
        }
    }

    private void SetState(State state)
    {
        this._state = state;

        //if (state.Equals(State.WaitingForUser))
        //{
        //    StartCoroutine("wait5s");
        //}
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    //public IEnumerable wait5s()
    //{
    //    yield return new WaitForSeconds(1f);
    //    OnStateChangedHuman?.Invoke(this, EventArgs.Empty);
    //}
    public State GetState()
    {
        return _state;
    }

    public class GemGridVisual
    {

        private Transform transform;
        private GemGrid gemGrid;

        public GemGridVisual(Transform transform, GemGrid gemGrid)
        {
            this.transform = transform;
            this.gemGrid = gemGrid;

            gemGrid.OnDestroyed += GemGrid_OnDestroyed;
        }

        private void GemGrid_OnDestroyed(object sender, System.EventArgs e)
        {
            transform.GetComponent<Animation>().Play();
            Destroy(transform.gameObject, 1f);
        }

        public void Update()
        {
            Vector3 targetPosition = gemGrid.GetWorldPosition();
            Vector3 moveDir = (targetPosition - transform.position);
            float moveSpeed = 10f;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

    }

    public class GlassGridVisual
    {

        private Transform transform;
        private GemGridPosition gemGridPosition;

        public GlassGridVisual(Transform transform, GemGridPosition gemGridPosition)
        {
            this.transform = transform;
            this.gemGridPosition = gemGridPosition;

            transform.gameObject.SetActive(gemGridPosition.HasGlass());

            gemGridPosition.OnGlassDestroyed += GemGridPosition_OnGlassDestroyed;
        }

        private void GemGridPosition_OnGlassDestroyed(object sender, EventArgs e)
        {
            transform.gameObject.SetActive(gemGridPosition.HasGlass());
        }
    }
    private void SetState(StateType state)
    {
        this._state.SetState(state);

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    public GemSO GetGem(int x, int y)
    {
        if(grid.GetGridObject(x, y)is null) { return null; }
        if(grid.GetGridObject(x, y).GetGemGrid() is null) { return null; }

        return grid.GetGridObject(x, y).GetGemGrid().GetGem();
    }

}