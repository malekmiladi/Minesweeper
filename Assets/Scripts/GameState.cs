public enum StateType
{
    Playing,
    Win,
    Loss
}
public struct GameState
{
    public StateType State;
    public int Width { get; set; }
    public int Height { get; set; }
    public Cell[,] Grid;
    public int MineCount;
    public int FlagCount;
}

