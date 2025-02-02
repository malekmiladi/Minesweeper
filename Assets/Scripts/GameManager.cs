using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager
{
    private readonly Vector2Int[] _adjacentOffsets = {
        new(-1, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, -1),
        new(0, 1),
        new(1, -1),
        new(1, 0),
        new(1, 1)
    };
    
    private GameState _state;
    private readonly TileManager _tileManager;
    private readonly Camera _mainCamera;
    private TMP_Text _mineText;

    public GameManager(TileManager tileManager, Camera mainCamera, TMP_Text remainingMinesText)
    {
        _tileManager = tileManager;
        _mainCamera = mainCamera;
        _mineText = remainingMinesText;
    }
    
    private void InitializeState((int, int) dimensions, int difficulty)
    {
        _state = new GameState
        {
            State = StateType.Playing,
            Width = dimensions.Item1,
            Height = dimensions.Item2,
            Grid = new Cell[dimensions.Item1, dimensions.Item2],
            MineCount = (int)(dimensions.Item1 * dimensions.Item2 * (difficulty / 100F)),
            FlagCount = 0
        };
    }

    public void NewGame((int, int) dimensions, int difficulty)
    {
        InitializeState(dimensions, difficulty);
        InitializeField();
        PlaceMines();
        SetNumbers();
        UpdateRemainingMinesText();
        ResizeCamera();
    
        _tileManager.DrawField(_state.Grid);
    }
    
    private void ResizeCamera()
    {
        _mainCamera.transform.position = new Vector3(_state.Width / 2F, _state.Height / 2F, -10F);
        _mainCamera.orthographicSize = (_state.Height + 2) / 2F;
    }
    
    private void InitializeField()
    {
        for (var x = 0; x < _state.Width; x++)
        {
            for (var y = 0; y < _state.Height; y++)
            {
                var cell = new Cell
                {
                    position = new Vector3Int(x, y),
                    type = Cell.Type.Empty
                };
                UpdateGridCell(cell);
            }
        }
    }
    
    private void PlaceMines()
    {
        for (var _ = 0; _ < _state.MineCount; _++)
        {
            PlaceMine(GetRandomCellPosition());
        }
    }

    private Vector2Int GetRandomCellPosition()
    {
        int x;
        int y;
        do
        {
            x = Random.Range(0, _state.Width);
            y = Random.Range(0, _state.Height);
        } while (_state.Grid[x, y].type == Cell.Type.Mine);
        return new Vector2Int(x, y);
    }

    private void PlaceMine(Vector2Int position)
    {
        _state.Grid[position.x, position.y].type = Cell.Type.Mine;
    }
    
    private void SetNumbers()
    {
        for (var x = 0; x < _state.Width; x++)
        {
            for (var y = 0; y < _state.Height; y++)
            {
                var cell = _state.Grid[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }
                
                cell.number = CountSurroundingMines(x, y);
                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }
                UpdateGridCell(cell);
            }
        }
    }
    
    private int CountSurroundingMines(int cellX, int cellY) =>
        _adjacentOffsets
            .Select(adjacentOffset => new { x = cellX + adjacentOffset.x, y = cellY + adjacentOffset.y })
            .Where(coordinate => IsValidCellCoordinate(coordinate.x, coordinate.y))
            .Select(coordinate => _state.Grid[coordinate.x, coordinate.y])
            .Count(cell => cell.type == Cell.Type.Mine);

    private bool IsValidCellCoordinate(int x, int y)
    {
        return x >= 0 && x < _state.Width && y >= 0 && y < _state.Height;
    }

    public void HandleMouseClick(MouseButton button, Vector3 mousePosition)
    {
        if (_state.State is not StateType.Playing) return;
        
        var cellPosition = GetCellPosition(mousePosition);
        var cell = GetCell(cellPosition.x, cellPosition.y);

        switch (button)
        {
            case MouseButton.LeftMouse:
            {
                RevealTile(cell);
                break;
            }
            case MouseButton.RightMouse:
            {
                SetFlag(cell);
                break;
            }
            case MouseButton.MiddleMouse:
            {
                RevealNeighbors(cell);
                break;
            }
            default:
            {
                return;
            }
        }

        CheckWinCondition();
        _tileManager.DrawField(_state.Grid);
        Debug.Log(_state.State.ToString());
    }

    private void RevealNeighbors(Cell cell)
    {
        if (cell is { revealed: false, type: not Cell.Type.Number }) return;
        if (!CheckFlagCount(cell)) return;
        foreach (var adjacentOffset in _adjacentOffsets)
        {
            var neighbor = GetCell(cell.position.x + adjacentOffset.x, cell.position.y + adjacentOffset.y);
            RevealTile(neighbor);
        }
    }

    private bool CheckFlagCount(Cell cell)
    {
        var flagCount = _adjacentOffsets
            .Select(adjacentOffset => GetCell(cell.position.x + adjacentOffset.x, cell.position.y + adjacentOffset.y))
            .Where(neighbor => neighbor.type is not Cell.Type.Invalid)
            .Count(neighbor => neighbor.flagged);
        return flagCount == cell.number;
    }

    private void CheckWinCondition()
    {
        var revealedTiles = 0;
        var flaggedMines = 0;
        for (var x = 0; x < _state.Width; x++)
        {
            for (var y = 0; y < _state.Height; y++)
            {
                var cell = _state.Grid[x, y];
                if (cell is { revealed: true, type: not Cell.Type.Mine })
                {
                    revealedTiles++;
                }

                if (cell is { flagged: true, type: Cell.Type.Mine })
                {
                    flaggedMines++;
                }
            }
        }

        if (revealedTiles + flaggedMines == _state.Width * _state.Height)
        {
            _state.State = StateType.Win;
        }
    }

    private void RevealTile(Cell cell)
    {
        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                ExplodeMine(cell);
                return;
            case Cell.Type.Empty:
            {
                FloodEmpty(cell);
                return;
            }
            default:
            {
                cell.revealed = true;
                UpdateGridCell(cell);
                return;
            }
        }
    }

    private void ExplodeMine(Cell cell)
    {
        _state.State = StateType.Loss;
        cell.exploded = true;
        cell.revealed = true;
        UpdateGridCell(cell);
        RevealMines();
    }

    private void RevealMines()
    {
        for (var x = 0; x < _state.Width; x++)
        {
            for (var y = 0; y < _state.Height; y++)
            {
                if (_state.Grid[x, y].type == Cell.Type.Mine)
                {
                    _state.Grid[x, y].revealed = true;
                }
            }
        }
    }
    
    private void FloodEmpty(Cell cell)
    {
        if (cell.revealed || cell.type is Cell.Type.Invalid)
        {
            return;
        }

        if (cell.type is Cell.Type.Mine)
        {
            ExplodeMine(cell);
            return;
        }
        
        cell.revealed = true;
        UpdateGridCell(cell);

        if (cell.type != Cell.Type.Empty) return;

        foreach (var adjacentOffset in _adjacentOffsets)
        {
            FloodEmpty(GetCell(cell.position.x + adjacentOffset.x, cell.position.y + adjacentOffset.y));
        }
    }

    private void SetFlag(Cell cell)
    {
        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }
        cell.flagged = !cell.flagged;
        if (cell.flagged)
        {
            _state.FlagCount++;
        }
        else
        {
            _state.FlagCount--;
        }
        UpdateRemainingMinesText();
        UpdateGridCell(cell);
    }

    private void UpdateGridCell(Cell cell)
    {
        _state.Grid[cell.position.x, cell.position.y] = cell;
    }
    
    private Vector3Int GetCellPosition(Vector3 mousePosition)
    {
        var worldPoint = _mainCamera.ScreenToWorldPoint(mousePosition);
        return _tileManager.ConvertToCell(worldPoint);
    }

    private Cell GetCell(int x, int y)
    {
        return IsValidCell(x, y) ? _state.Grid[x, y] : new Cell();
    }
    
    private bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < _state.Width && y >= 0 && y < _state.Height;
    }

    public void UpdateRemainingMinesText()
    {
        _mineText.text = GetMineCount().ToString();
    }
    private int GetMineCount()
    {
        return _state.MineCount - _state.FlagCount;
    }
    
}
