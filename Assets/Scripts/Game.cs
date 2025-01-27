using UnityEngine;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    private GameManager _gameManager;
    private void Awake()
    {
        var mainCamera = Camera.main;
        var tileManager = GetComponentInChildren<TileManager>();
        _gameManager = new GameManager(tileManager, mainCamera, (16, 24));
    }

    private void Start()
    {
        _gameManager.NewGame((16, 24));
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.MiddleMouse))
        {
            _gameManager.HandleMouseClick(MouseButton.MiddleMouse, Input.mousePosition);
        }
        else if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            _gameManager.HandleMouseClick(MouseButton.RightMouse, Input.mousePosition);
        }
        else if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            _gameManager.HandleMouseClick(MouseButton.LeftMouse, Input.mousePosition);
        }
    }
    
}
