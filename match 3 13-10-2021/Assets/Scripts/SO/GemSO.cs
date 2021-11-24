using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

[CreateAssetMenu()]
public class GemSO : ScriptableObject
{

    public string gemName;
    public Sprite sprite;
    public ColorType color;
    public PieceType pieceType;

    //public ScriptableObject
    public Action[] onDestruction;
}

