using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterComponent : MonoBehaviour
{


    
    
    
    [SerializeField]
    public ColorType[] PieceTypes;
    public SwapGrid _gridRefernce;
    public BackgroundPiece _BackgroundRefernce;
    private Dictionary<ColorType, int> _count; 
    //private int [] _count;
    public Dictionary<ColorType, int> Count { get { return _count; }  }

    public void inc(ColorType type)
    {
        _count[type] = _count[type] + 1;
    }
    // Start is called before the first frame update
    void Awake()
    {
        _BackgroundRefernce = GetComponent<BackgroundPiece>();
        _count = new Dictionary<ColorType, int>();
        for (int i = 0; i < PieceTypes.Length; i++)
        {
            _count.Add(PieceTypes[i], 0);
        }
    }

    

    
    
}
