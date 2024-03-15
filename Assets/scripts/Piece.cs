using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;

    private float currentStepDelay;

    // UI button logic
    public bool goingDown = false;
    private bool goingLeft = false;
    private bool goingRight = false;
    public bool uiButRotateL = false;
    public bool uiButRotateR = false;
    public bool uihardDrop = false;

    private float minSwipeDistance = 0.5f;

    public board board {  get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }
    public Transform coolObject;
    private Vector2 touchStartPos;
    private bool isTouchActive = false;

    public void Initialize(board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        if(this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for(int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    public void Update()
    {
        HandleTouchInput();
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Q) || uiButRotateL == true)
        {
            uiButRotateL = false;
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E) || uiButRotateR == true)
        {
            uiButRotateR = false;
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.A) || goingLeft == true)
        {
            goingLeft = false;
            Move(Vector2Int.left);
        }

        if (Input.GetKeyDown(KeyCode.D) || goingRight == true)
        {
            goingRight = false;
            Move(Vector2Int.right);
        }

        if (Input.GetKeyDown(KeyCode.S) || goingDown == true)
        {
            goingDown = false;
            Move(Vector2Int.down);
        }

        if(Input.GetKeyDown(KeyCode.Space) || uihardDrop == true)
        {
            uihardDrop = false;
            HardDrop();
        }


        if (Time.time >= this.stepTime)
        {
            Step();
        }
        this.board.Set(this);
    }

    private void HandleTouchInput()
    {
        if (!goingLeft && !goingRight && !goingDown && !uiButRotateL && !uiButRotateR && !uihardDrop) // Check if no button input is active
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    isTouchActive = true;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (isTouchActive)
                    {
                        Vector2 touchEndPos = touch.position;
                        Vector2 touchDelta = touchEndPos - touchStartPos;

                        if (touchDelta.magnitude > minSwipeDistance)
                        {
                            // Determine swipe direction
                            float angle = Mathf.Atan2(touchDelta.y, touchDelta.x) * Mathf.Rad2Deg;
                            if (angle < 0) angle += 360;

                            if (angle < 45 || angle > 315) // Right Swipe
                            {
                                goingRight = true;
                            }
                            else if (angle > 135 && angle < 225) // Left Swipe
                            {
                                goingLeft = true;
                            }
                            else if (angle > 45 && angle < 135) // Up Swipe
                            {
                                // Nothing for now
                            }
                            else if (angle > 225 && angle < 315) // Down Swipe
                            {
                                goingDown = true;
                            }
                        }
                        else
                        {
                            // Tap detected
                            // Determine tap side (left or right)
                            if (touch.position.x < Screen.width / 2)
                            {
                                uiButRotateL = true; // Left side tap for counter-clockwise rotation
                            }
                            else
                            {
                                uiButRotateR = true; // Right side tap for clockwise rotation
                            }
                        }

                        isTouchActive = false;
                    }
                }
            }
        }
    }

    private void Step()
    {
        this.stepTime = Time.time + board.activePiece.stepDelay;

        Move(Vector2Int.down);

        if (this.lockTime > this.lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while(Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        this.board.Set(this);
        this.board.clearLines();
        this.board.SpawnPiece();
    }

    public bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);
        //Debug.Log(valid);

        if (valid)
        {
            this.position = newPosition;
            this.lockTime = 0f;
        }

        return valid;
    }

    public void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        applyRotationMatrix(direction);

        if(!testWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            applyRotationMatrix(-direction);
        }
        
    }
    private void applyRotationMatrix(int direction)
    {

        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }

        Debug.Log(" test");
    }

    private bool testWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for(int i = 0; i < this.data.wallkicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallkicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if(rotationDirection < 0)
        {
            wallKickIndex--;
        }
        return Wrap(wallKickIndex, 0, this.data.wallkicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min )
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

    public void OnRightButtonClick()
    {
        goingRight = true;
    }

    public void OnLeftButtonClick()
    {
        goingLeft = true;
    }

    public void OnDownButtonClick()
    {
        goingDown = true;
    }

    public void OnRotateCounterClockwiseButtonClick()
    {
        uiButRotateL = true;
    }

    public void OnRotateClockwiseButtonClick()
    {
        uiButRotateR = true;
    }
   
    public void OnHardDropButtonClick()
    {
        uihardDrop = true;
    }
}


