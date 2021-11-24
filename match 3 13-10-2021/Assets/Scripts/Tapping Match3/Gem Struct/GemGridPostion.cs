using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GemGridPosition
{

    public event EventHandler OnGlassDestroyed;

    private GemGrid gemGrid;

    private Grid<GemGridPosition> grid;
    private int x;
    private int y;
    private bool hasGlass;

    public GemGridPosition(Grid<GemGridPosition> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetGemGrid(GemGrid gemGrid)
    {
        this.gemGrid = gemGrid;
        grid.TriggerGridObjectChanged(x, y);

    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public Vector3 GetWorldPosition()
    {
        return grid.GetWorldPosition(x, y);
    }

    public GemGrid GetGemGrid()
    {
        return gemGrid;
    }

    public void ClearGemGrid()
    {
        gemGrid = null;
    }

    public void DestroyGem()
    {
        gemGrid?.Destroy();
        grid.TriggerGridObjectChanged(x, y);
    }

    public bool HasGemGrid()
    {
        return gemGrid != null;
    }

    public bool IsEmpty()
    {
        return gemGrid == null;
    }

    public bool HasGlass()
    {
        return hasGlass;
    }

    public void SetHasGlass(bool hasGlass)
    {
        this.hasGlass = hasGlass;
    }

    public void DestroyGlass()
    {
        SetHasGlass(false);
        OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString()
    {
        return gemGrid?.ToString();
    }
}
public class GemGrid
{

    public event EventHandler OnDestroyed;

    private GemSO gem;
    private int x;
    private int y;
    private bool isDestroyed;

    public GemGrid(GemSO gem, int x, int y)
    {
        this.gem = gem;
        this.x = x;
        this.y = y;

        isDestroyed = false;
    }

    public GemSO GetGem()
    {
        return gem;
    }

    public Vector3 GetWorldPosition()
    {
        return new Vector3(x, y);
    }

    public void SetGemXY(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public void SetGem(GemSO gem)
    {
        this.gem = gem;
    }

    public void Destroy()
    {
        isDestroyed = true;
        OnDestroyed?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString()
    {
        return isDestroyed.ToString();
    }

}

