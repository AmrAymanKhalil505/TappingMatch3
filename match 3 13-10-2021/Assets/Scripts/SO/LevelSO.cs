using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public bool isSpawnCrossLevel;
    [SerializeField]
    public GemSO Rocket;
    public bool isSpawnBombLevel;
    [SerializeField]
    public GemSO Bomb;
    public bool isSpawnRainbowLevel;
    public enum GoalType
    {
        Glass,
        Score,
    }

    public List<GemSO> gemList;
    public int width;
    public int height;
    public List<LevelGridPosition> levelGridPositionList;
    public GoalType goalType;
    public int moveAmount;
    public int targetScore;


    [System.Serializable]
    public class LevelGridPosition
    {

        public GemSO gemSO;
        public int x;
        public int y;
        public bool hasGlass;

    }

}
