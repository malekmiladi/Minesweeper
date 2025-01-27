using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    private Tilemap Tilemap { get; set; }
    public Tile tileEmpty;
    public Tile tileMine;
    public Tile tileMineExploded;
    public Tile tileHidden;
    public Tile tileFlag;
    public Tile tile1;
    public Tile tile2;
    public Tile tile3;
    public Tile tile4;
    public Tile tile5;
    public Tile tile6;
    public Tile tile7;
    public Tile tile8;

    private void Awake()
    {
        Tilemap = GetComponent<Tilemap>();
    }

    public void DrawField(Cell[,] field)
    {
        for (var x = 0; x < field.GetLength(0); x++)
        {
            for (var y = 0; y < field.GetLength(1); y++)
            {
                var cell = field[x, y];
                Tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    private Tile GetTile(Cell cell)
    {
        if (cell.flagged)
        {
            return tileFlag;
        }

        if (cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        
        return tileHidden;
        
    }
    
    private Tile GetRevealedTile(Cell cell)
    {
        return cell.type switch
        {
            Cell.Type.Empty => tileEmpty,
            Cell.Type.Mine => cell.exploded ? tileMineExploded : tileMine,
            Cell.Type.Number => GetNumberTile(cell),
            _ => null
        };
    }

    private Tile GetNumberTile(Cell cell)
    {
        return cell.number switch
        {
            1 => tile1,
            2 => tile2,
            3 => tile3,
            4 => tile4,
            5 => tile5,
            6 => tile6,
            7 => tile7,
            8 => tile8,
            _ => null
        };
    }

    public Vector3Int ConvertToCell(Vector3 coordinates)
    {
        return Tilemap.WorldToCell(coordinates);
    }

}
