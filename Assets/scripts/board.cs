using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;

public class board : MonoBehaviour
{
    // Public variables accessible from Unity editor
    public Tilemap tilemap { get; private set; } 
    public Piece activePiece { get; private set; } 
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition; 
    public Vector2Int boardSize = new Vector2Int(10, 20); 

    public TMP_Text scoreText; 
    public TMP_Text FinalScoreText; 
    public int score; 
    public new AudioSource audio; 

    // Private variables
    private int totalLinesCleared = 0;
    private int linesCleared = 0; 
    public int currentLevel = 1; 

    // References to UI elements
    public GameObject optionsMenu; 
    public GameObject deadScreen;
    public bool paused; 

    // Properties to represent the width and height of the board
    public int width; 
    public int height; 

    // Property to calculate the bounds of the game board
    public RectInt Bounds
    {
        get
        {
            // Calculate the position of the bottom-left corner of the board
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            // Create a rectangle representing the bounds of the board
            return new RectInt(position, this.boardSize);
        }
    }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Get references to the Tilemap and Piece components
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        // Initialize all tetromino shapes
        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Spawn the initial piece
        SpawnPiece();
        // Get reference to the AudioSource component
        audio = GetComponent<AudioSource>();
        // UpdatePause();
    }

    // Update is called once per frame
    public void Update()
    {
        // Check for input to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // Method to spawn a new piece
    public void SpawnPiece()
    {
        // Select a random tetromino shape
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        // Initialize the active piece with the selected shape and spawn position
        this.activePiece.Initialize(this, this.spawnPosition, data);

        // Check if the initial position of the piece is valid
        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            // Set the piece on the tilemap
            Set(this.activePiece);
        }
        else
        {
            // End the game if the initial position is invalid
            GameOver();
        }
    }

    // Method to handle game over
    public void GameOver()
    {
        // Show the game over screen
        ToggleGameOver();
    }

    // Method to toggle the game over screen
    public void ToggleGameOver()
    {
        deadScreen.gameObject.SetActive(true);
        FinalScoreText.text = "Your score: " + score;
        Time.timeScale = 0;
        paused = !paused;
    }

    // Method to toggle pause
    public void TogglePause()
    {
        optionsMenu.gameObject.SetActive(!optionsMenu.gameObject.activeSelf);
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        Debug.Log("Paused state: " + paused);
    }

    // Method to set tiles on the tilemap
    public void Set(Piece piece)
    {
        // Iterate through each cell of the piece
        for (int i = 0; i < piece.cells.Length; i++)
        {
            // Calculate the position of the cell on the tilemap
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            // Set the tile at the calculated position
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    // Method to clear tiles from the tilemap
    public void Clear(Piece piece)
    {
        // Iterate through each cell of the piece
        for (int i = 0; i < piece.cells.Length; i++)
        {
            // Calculate the position of the cell on the tilemap
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            // Clear the tile at the calculated position
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    // Method to check if a position is valid for the piece
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        // Get the bounds of the game board
        RectInt bounds = this.Bounds;

        // Iterate through each cell of the piece
        for (int i = 0; i < piece.cells.Length; i++)
        {
            // Calculate the position of the cell on the tilemap
            Vector3Int tilePosition = piece.cells[i] + position;

            // Check if the cell is within the bounds of the board
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // Check if the cell is already occupied
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    // Method to clear completed lines
    public void clearLines()
    {
        // Get the bounds of the game board
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        linesCleared = 0; // Reset linesCleared count at the beginning

        // Iterate through each row of the board
        while (row < bounds.yMax)
        {
            // Check if the current row is full
            if (isLineFull(row))
            {
                // Clear the full line
                lineClear(row);
                // Increment the count of lines cleared
                linesCleared++;
                totalLinesCleared++;
                // Play audio effect
                audio.Play();
            }
            else
            {
                row++;
            }
        }

        // Calculate score based on the number of lines cleared
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
        // Update the score text
        scoreText.text = "Score: " + score;

        // Increase difficulty level if necessary
        if (totalLinesCleared >= 10 && activePiece.stepDelay >= 0.1f)
        {
            activePiece.stepDelay -= 0.1f; // Decrease stepDelay
            totalLinesCleared = 0;
        }
    }

    // Method to check if a row is full
    private bool isLineFull(int row)
    {
        // Get the bounds of the game board
        RectInt bounds = this.Bounds;

        // Iterate through each column of the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            // Calculate the position of the cell
            Vector3Int position = new Vector3Int(col, row, 0);

            // If any cell in the row is empty, return false
            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        // If all cells in the row are occupied, return true
        return true;
    }

    // Method to clear a completed line
    private void lineClear(int row)
    {
        // Get the bounds of the game board
        RectInt bounds = this.Bounds;

        // Iterate through each column of the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            // Calculate the position of the cell
            Vector3Int position = new Vector3Int(col, row, 0);
            // Clear the tile at the calculated position
            this.tilemap.SetTile(position, null);
        }

        // Shift the tiles above the cleared line down
        while (row < bounds.yMax)
        {
            // Iterate through each column of the row
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                // Calculate the position of the cell above the current one
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                // Get the tile from the cell above
                TileBase above = this.tilemap.GetTile(position);

                // Update the position to the current row
                position = new Vector3Int(col, row, 0);
                // Move the tile down to the current row
                this.tilemap.SetTile(position, above);
            }
            // Move to the next row
            row++;
        }
    }
}