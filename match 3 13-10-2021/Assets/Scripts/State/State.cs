using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum StateType
{   None=0,
    Created=8,
    SettingLevel = 1,
    FillingVertically = 2,
    FillingDiagonally = 3,
    WaitingForUser = 4,
    DestroyPieces = 5,
    Busy = 6,
    GameOver = 7,
    TryFindMatches=9
}
public class OnStateChangedArgs : EventArgs
{
    public StateType currentState;
    public StateType previousState;
}
public class State : MonoBehaviour
{
    
    public event EventHandler OnStateChanged;
    public StateType currentState;
    public StateType previousState;
    public void SetState(StateType state)
    {
        if(currentState.Equals(StateType.None))
        {
            currentState = StateType.Created;
        }
        previousState = currentState;
        currentState = state;
        OnStateChanged?.Invoke(this, new OnStateChangedArgs { currentState = currentState, previousState = previousState });
    }
    public StateType GetState()
    {
        return currentState;
    }
    public StateType GetPreState()
    {
        return previousState;
    }

}
