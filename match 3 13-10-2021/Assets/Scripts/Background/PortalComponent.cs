using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalComponent : MonoBehaviour
{
    public int id;
    public int connectedToId;
    public PortalComponent _connectedToPortalComponent;
    public SwapGrid _gridRefernce;
    public BackgroundPiece _BackgroundRefernce;
    
    // Start is called before the first frame update
    void Awake()
    {
        _BackgroundRefernce = GetComponent<BackgroundPiece>();
    }
    public void init(int id , int connectedToId, PortalComponent _connectedToPortalComponent)
    {
        this.id = id;
        this.connectedToId = connectedToId;
        this._connectedToPortalComponent = _connectedToPortalComponent;
        
    }
}
