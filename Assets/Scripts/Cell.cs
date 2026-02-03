using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UIElements;

public struct CellStruct
{
    public notes note;
    public int level;

    public CellStruct(notes _note, int _level)
    {
        this.note = _note;
        this.level = _level;
    }
}

public class Cell : MonoBehaviour
{
    public notes note;                      // Type of note of the audio clip this cell has in its audio source
    public int noteLevel;                   // 0, 1, 2 -> A, A1, A2
    public instruments instru;              // Type of instrument that makes the sound of the audio clip
    public bool isSeperationMark;           // Indicates whether the cell is a note or a seperation mark
    public float timeGapBetweenNotes;       // Time between two notes playing their clip

    public int trackIndex;                 // Index of the track the cell is on
    public bool canMove;                   // Indicate whether the cell has been triggered to play the audio clip

    // Initializing the cell's properties and appearance
    public void InitializeCell(notes _noteType, int _noteLevel, instruments _instru, int _trackIndex, bool _isSM)
    {
        if (_isSM)
        {
            this.isSeperationMark = true;
            
            this.note = notes.invalid;
            this.noteLevel = -1;
            this.instru = instruments.piano;
            

            GetComponent<SpriteRenderer>().sprite = LevelManager._instance.noteSprites[LevelManager._instance.noteSprites.Length - 1];
        }
        else
        {
            this.isSeperationMark = false;
            
            AudioClip[] clipSet = LevelManager._instance.GetClipSetFromInstru(_instru);

            if (clipSet == null) { Debug.LogError("Trying to assign a cell an audio clip with an invalid instrument type!"); }

            GetComponent<AudioSource>().clip = clipSet[(int)_noteType + _noteLevel * 7];
            this.note = _noteType;
            this.noteLevel = _noteLevel;
            this.instru = _instru;

            // Get sprite for the cell
            GetComponent<SpriteRenderer>().sprite = LevelManager._instance.noteSprites[(int)_noteType + _noteLevel * 7];
        }

        this.trackIndex = _trackIndex;
        canMove = true;
        GetComponent<BoxCollider2D>().enabled = true;
    }

    // Called every 0.02s, moves the cell at certain speed if the track is prepared to move
    /*private void FixedUpdate()
    {
        if (LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>().isMoving && canMove)
        {
            transform.position = new Vector2(transform.position.x - 1.25f / (timeGapBetweenNotes / 0.02f), transform.position.y); // 1.25 -> distance between two notes; 1 -> one second gap betwen two sound
        }
    }*/

    private void Update()
    {
        if (LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>().isMoving && canMove)
        {
            transform.position = new Vector2(transform.position.x - 1.25f / (timeGapBetweenNotes / Time.deltaTime), transform.position.y); // 1.25 -> distance between two notes
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "CellTrigger")
        {
            if (!isSeperationMark)
            {
                LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>().GenerateCell(0, 1);

                // Play the audio clip
                GetComponent<AudioSource>().Play();

                if (trackIndex == 0)
                {
                    LevelManager._instance.tracks.GetChild(0).GetComponent<Track>().PlayBGAnim();
                }
            }

            // Stop moving and being recorded by the track
            canMove = false;
            LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>().cellsOnTrack.RemoveAt(0);

            // Start to disappear, the cell is released to the pool at the last frame of the disappearing animation
            GetComponent<Animator>().SetTrigger("Disappear");
        }
    }

    // The function should be called when the cell completes its spawn animation. called as an animation event
    public void FinishSpawning()
    {
        LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>().isSpawning = false;
    }

    // Called as an animation event at the last frame of the destroy animation sequence, returning the cell to the pool
    public void SelfDestruct()
    {
        CellPool._instance.pool.Release(gameObject);
    }

    // For copying a group of 5 notes to other tracks
    private void OnMouseDown()
    {
        Debug.Log("Flag 0");
        if (LevelManager._instance.trackActivated == 3 || !LevelManager._instance.copyGroupParent.gameObject.activeSelf) { return; }
        Debug.Log("Flag 1");
        if (!isSeperationMark && LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>().isMoving)
        {
            Debug.Log("Flag 2");
            // Track script of the track this cell is on
            Track track = LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>();

            int trailingGroupCellNum, index, length;
            if (track.transform.GetSiblingIndex() == 0)
            {
                // Number of cells that are grouped at the tail of the track, should be between 0 - 4, both end inclusive
                trailingGroupCellNum = track.cellGroupingTracker;

                // Get the index of this cell on the track
                index = track.cellsOnTrack.IndexOf(gameObject);

                // Get the total number of elements on track
                length = track.cellsOnTrack.Count;

                if (trailingGroupCellNum == 0)
                {
                    if ((index >= 0 && index <= 5) || (index >= 6 && index <= 11))
                    {
                        GenerateCopyGroup(index, trailingGroupCellNum, length);
                    }
                }
                else if (index >= (6 - trailingGroupCellNum) && index <= (10 - trailingGroupCellNum))
                {
                    GenerateCopyGroup(index, trailingGroupCellNum, length);
                }
            }
            else
            {
                trailingGroupCellNum = track.cellGroupingTracker;
                index = track.cellsOnTrack.IndexOf(gameObject);
                length = track.cellsOnTrack.Count;

                if (trailingGroupCellNum == 0)
                {
                    GenerateCopyGroup(index, 0, length);
                }
            }
            
        }
    }

    // Generate a group of cells that follows the mouse, which represents the group of notes to be copied to other tracks
    private void GenerateCopyGroup(int _indexOnTrack, int _trailingGroupCellNum, int _trackLength)
    {
        LevelManager._instance.isHoldingCellGroup = true;

        Transform notesParent = LevelManager._instance.copyGroupParent;
        notesParent.gameObject.SetActive(true);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        notesParent.transform.position = mousePos;

        // Get the index of the cell being clicked within the group to be copied
        int index;
        if (_trailingGroupCellNum == 0)
        {
            if (_trackLength == 13 || _trackLength == 7)
            {
                index = (_indexOnTrack + 5) % 6;                        // [1, 5] / [7, 11] -> [0, 4]
            }
            else
            {
                index = (_indexOnTrack + 6) % 6;                        // [0, 4] / [6, 10] -> [0, 4]
            }
        }
        else
        {
            index = _indexOnTrack + _trailingGroupCellNum - 6;          // 1 - [5, 9] / 2 - [4, 8] / 3 - [3, 7] / 4 - [2, 6] -> [0, 4]
        }

        // Spawn 5 cells with position offsets based on the index of the clicked cell in the group
        Vector2 cellPos = Vector2.zero;
        GameObject tempCell = null;
        Track track = LevelManager._instance.tracks.GetChild(trackIndex).GetComponent<Track>();
        Cell cell = null;
        for (int i = 0; i < 5; i++)
        {
            cellPos = new Vector2(mousePos.x + (i - index) * 1.25f, mousePos.y);
            cell = track.cellsOnTrack[_indexOnTrack - index + i].GetComponent<Cell>();
            tempCell = CellPool._instance.Spawn(cellPos, cell.note, cell.noteLevel, cell.instru, trackIndex, false);
            tempCell.transform.SetParent(notesParent);
            tempCell.GetComponent<Cell>().canMove = false;
            tempCell.GetComponent<BoxCollider2D>().enabled = false;
        }

        // Generate a seperation mark at the end
        cellPos = new Vector2(notesParent.GetChild(notesParent.childCount - 1).position.x + 1.25f, notesParent.GetChild(notesParent.childCount - 1).position.y);
        tempCell = CellPool._instance.Spawn(cellPos, notes.invalid, -1, instruments.piano, trackIndex, true);
        tempCell.transform.SetParent(notesParent);
        tempCell.GetComponent<Cell>().canMove = false;
        tempCell.GetComponent<BoxCollider2D>().enabled = false;
    }
}
