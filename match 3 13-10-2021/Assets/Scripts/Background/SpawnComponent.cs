using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnComponent : MonoBehaviour
{
    public SpawnType spawnType;
    [System.Serializable]
    public struct PiecePrefab2
    {
        public PieceType type;
        public ColorType variationType;
        public float persentage;
        
    };
    [SerializeField]
    public PiecePrefab2 [] piecePrefabs;
    public SwapGrid _gridRefernce;
    public BackgroundPiece _BackgroundRefernce;
    //public int _x;
    //public int _y;

    //public int X
    //{
    //    get => _x;
    //    set { _x = value;  }
    //}

    //public int Y
    //{
    //    get => _y;
    //    set {  _y = value;  }
    //}
    // Start is called before the first frame update
    void Awake()
    {
        _BackgroundRefernce = GetComponent<BackgroundPiece>();

    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
    public GamePiece spawnPiece()
    {
       
        int X = _BackgroundRefernce.X;
        int Y = _BackgroundRefernce.Y;
        if (spawnType == SpawnType.defaultSpawn)
        {
            float maxPersentage = 0;
            for (int i = 0; i < piecePrefabs.Length; i++)
            {
                maxPersentage += piecePrefabs[i].persentage;
            }
            int randomIndex = piecePrefabs.Length - 1;
            float randomeValue = ((float)Random.Range(0, maxPersentage));
            maxPersentage = 0;
            for (int i = 0; i < piecePrefabs.Length - 1; i++)
            {
                if (randomeValue >= maxPersentage && randomeValue < maxPersentage+ piecePrefabs[i].persentage)
                {
                    randomIndex = i; break;
                }
                maxPersentage += piecePrefabs[i].persentage;
            }
            return _gridRefernce.SpawnNewPiece(X, Y, piecePrefabs[randomIndex].type, piecePrefabs[randomIndex].variationType);
        }
        else
        {
            float maxPersentage = 0;
            for (int i=0;i< piecePrefabs.Length; i++)
            {
                maxPersentage += piecePrefabs[i].persentage;
            }
            int randomIndex = piecePrefabs.Length-1;
            float randomeValue= ((float)Random.Range(0, maxPersentage));
            maxPersentage = 0;
            for (int i = 0; i < piecePrefabs.Length-1; i++)
            {
                if (randomeValue >= maxPersentage && randomeValue < maxPersentage + piecePrefabs[i].persentage)
                {
                    randomIndex = i; break;
                }
                maxPersentage += piecePrefabs[i].persentage;
            }
            return _gridRefernce.SpawnNewPiece(X, Y, piecePrefabs[randomIndex].type, piecePrefabs[randomIndex].variationType);
        }
        
    }
}
