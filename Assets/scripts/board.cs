using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;

public class board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; } 
    public Piece activePiece { get; private set; } 
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition; 
    public Vector2Int boardSize = new Vector2Int(10, 20); 

    public TMP_Text scoreText; 
    public TMP_Text FinalScoreText; 
    public int score; 
    public new AudioSource audio;
    public TMP_Text textMeshProFast;


    private int totalLinesCleared = 0;
    private int linesCleared = 0; 
    public int currentLevel = 1;
    public bool fastState = false;

    public GameObject optionsMenu; 
    public GameObject deadScreen;
    public bool paused; 

    public int width; 
    public int height; 

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
        textMeshProFast.color = Color.gray;

    }

    private void Start()
    {
        // Spawn the initial piece
        SpawnPiece();
        // Get reference to the AudioSource component
        audio = GetComponent<AudioSource>();
        // UpdatePause();
    }

    public void Update()
    {
        // Check for input to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        this.activePiece.Initialize(this, this.spawnPosition, data);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        // Show the game over screen
        ToggleGameOver();
    }

    public void ToggleGameOver()
    {
        deadScreen.gameObject.SetActive(true);
        FinalScoreText.text = "Your score: " + score;
        Time.timeScale = 0;
        paused = !paused;
    }

    public void TogglePause()
    {
        optionsMenu.gameObject.SetActive(!optionsMenu.gameObject.activeSelf);
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        Debug.Log("Paused state: " + paused);
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void clearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        linesCleared = 0; 

        while (row < bounds.yMax)
        {
            if (isLineFull(row))
            {
                lineClear(row);
                linesCleared++;
                totalLinesCleared++;
                audio.Play();
            }
            else
            {
                row++;
            }
        }

        switch (linesCleared)
        {
            case 1:
                score += 40;
                break;
            case 2:
                score += 100;
                break;
            case 3:
                score += 300;
                break;
            case 4:
                score += 1200;
                break;
        }
        scoreText.text = "Score: " + score;

        if (totalLinesCleared >= 10 && activePiece.stepDelay >= 0.1f)
        {
            activePiece.stepDelay -= 0.1f; // Decrease stepDelay
            totalLinesCleared = 0;
        }
        else if (totalLinesCleared >= 10 && activePiece.stepDelay >= 0.05f)
        {
            activePiece.stepDelay -= 0.01f; // Decrease stepDelay
            totalLinesCleared = 0;
        }
    }

    public void toggleFastMode()
    {
        fastState = !fastState;
        if(fastState)
        {
            activePiece.stepDelay = 0.05f;
            textMeshProFast.color = Color.white;

        } else
        {
            textMeshProFast.color = Color.gray;

            // currently this resets the speed to 1 eventhough it should be faster (after clearing some lines)
            // DOTO: calculate what speed it should be
            activePiece.stepDelay = 1.0f;
        }
    }

    private bool isLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void lineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }
            row++;
        }
    }
}