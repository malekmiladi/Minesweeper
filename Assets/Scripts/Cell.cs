using UnityEngine;

public struct Cell
{
    public enum Type
    {
        Invalid,
        Mine,
        Empty,
        Number
    }
    
    public Type type;
    public Vector3Int position;
    public int number;
    public bool flagged;
    public bool exploded;
    public bool revealed;

}
