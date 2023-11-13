using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int mineCount = 32;

    private Board board;
    private Cell[,] state;

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        state = new Cell[width, height];

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(0, 16, 0); //(width / 2f, 16, height / 2f)
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
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

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

                cell.revealed=true;
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

                if (x < 0 || x >= width || y < 0 || y >= height) {
                    continue;
                }

                if (state[x, y].type == Cell.Type.Mine) {
                    count++;
                }
            }
        }

        return count;
    }

}
