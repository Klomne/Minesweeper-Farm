using JetBrains.Annotations;
using System;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int mineCount = 32;

    private Board board;
    private Cell[,] state;
    private bool gameover;

    //Butttons, Dropdowns, Inputs, Sliders
    public Button restartButton;
    public Slider cellAmountSlider;
    public TextMeshProUGUI cellAmount;
    public TextMeshProUGUI mineAmount;

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();

        //Butttons, Dropdowns, Inputs, Sliders
        restartButton.onClick.AddListener(delegate { NewGame(); });
        
        cellAmount.text = $"{width}";
        cellAmountSlider.onValueChanged.AddListener(delegate { BoardSize(); });

        cellAmountSlider.minValue = 2;
        cellAmountSlider.maxValue = 128;

        mineAmount.text = $"{mineCount}";
    }

    private void NewGame()
    {
        state = new Cell[width, height];
        gameover = false;

        Tilemap Tm = board.gameObject.GetComponent<Tilemap>();
        Tm.ClearAllTiles();

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(0, (width/2) + (height/2), -width / 2); //(width / 2f, 16, height / 2f)
        board.Draw(state);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                int offset = (width / 2);
                cell.position = new Vector3Int(x + -offset, y + -offset, 0);
                cell.type = Cell.Type.Empty; 
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines() 
    {
        for (int i = 0; i < mineCount; i++) 
        {
            int x = UnityEngine.Random.Range(0, width);
            int y = UnityEngine.Random.Range(0, height);

            while (state[x, y].type == Cell.Type.Mine) 
            {
                x++;

                if (x >= width) 
                {
                    x = 0;
                    y++;

                    if (y >= height) 
                    {
                        y = 0;
                    }
                }
            }

            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers() 
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine) {
                    continue; 
                }

                cell.number = CountMines(x, y);

                if (cell.number > 0) {
                    cell.type = Cell.Type.Number;
                }

                state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY) 
    {
        int count = 0;
        
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++) 
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++) 
            {
                if (adjacentX == 0 && adjacentY == 0) 
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (GetCell(x, y).type == Cell.Type.Mine) {
                    count++;
                }
            }
        }

        return count;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            NewGame();
        }

        else if (!gameover) 
        {
            if (Input.GetMouseButtonDown(1)) {
                Flag();
            } else if (Input.GetMouseButtonUp(0)) {  
                Reveal(); 
            }
        }
    }

    private void Flag()
    {
        RaycastHit hitInfo = new RaycastHit();
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out  hitInfo, Mathf.Infinity);
        Vector3 worldPosition = hitInfo.point;

        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        cellPosition.x = cellPosition.x + (width / 2);
        cellPosition.y = cellPosition.y + (height / 2);
        //Debug.Log(cellPosition); //If I need to see in console where I'm clicking on the grid
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed){
            return;
        }

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }

    private void Reveal()
    {
        RaycastHit hitInfo = new RaycastHit();
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);
        Vector3 worldPosition = hitInfo.point;

        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        cellPosition.x = cellPosition.x + (width / 2);
        cellPosition.y = cellPosition.y + (height / 2);
        //Debug.Log(cellPosition); //If I need to see in console where I'm clicking on the grid
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged) {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            
            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }

        board.Draw(state);
    }

    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x + (width / 2), cell.position.y + (height / 2)] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x + (width / 2), cell.position.y + (height / 2) + 1)); //Flood "North" based from click
            Flood(GetCell(cell.position.x + (width / 2) - 1, cell.position.y + (height / 2))); //Flood "West" based from click
            Flood(GetCell(cell.position.x + (width / 2) + 1, cell.position.y + (height / 2))); //Flood "East" based from click
            Flood(GetCell(cell.position.x + (width / 2), cell.position.y + (height / 2) - 1)); //Flood "South" based from click

            Flood(GetCell(cell.position.x + (width / 2) - 1, cell.position.y + (height / 2) + 1)); //Flood "North-West" based from click
            Flood(GetCell(cell.position.x + (width / 2) + 1, cell.position.y + (height / 2) + 1)); //Flood "North-East" based from click
            Flood(GetCell(cell.position.x + (width / 2) + 1, cell.position.y + (height / 2) - 1)); //Flood "South-East" based from click
            Flood(GetCell(cell.position.x + (width / 2) - 1, cell.position.y + (height / 2) - 1)); //Flood "South-West" based from click
        }
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game Over Mate!");
        gameover = true;

        cell.revealed = true;
        cell.exploded = true;
        state[cell.position.x + (width / 2), cell.position.y + (height / 2)] = cell;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
                //if (cell.type == cell.Type.FalseFlagged)
                // {
                //     
                //     state[x, y] = cell;
                // }
            }
        }
    }

    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type != Cell.Type.Mine && !cell.revealed) {
                    return;
                }
            }
        }

        Debug.Log("You Won!");
        gameover = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y)){
            return state[x, y];
        } else {
            return new Cell();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //Buttons, Dropdowns, Inputs, Sliders
    private void BoardSize()
    {
        height = (int)cellAmountSlider.value;
        width = (int)cellAmountSlider.value;

        cellAmount.text = $"{width}";
    }

}
