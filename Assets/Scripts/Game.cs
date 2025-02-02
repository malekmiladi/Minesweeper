using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    private GameManager _gameManager;
    public TMP_Text remainingMinesText;
    private void Awake()
    {
        var mainCamera = Camera.main;
        var tileManager = GetComponentInChildren<TileManager>();
        _gameManager = new GameManager(tileManager, mainCamera, remainingMinesText);
    }

    private void Start()
    {
        _gameManager.NewGame((38, 24), 15);
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
