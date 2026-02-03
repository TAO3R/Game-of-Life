using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Track : MonoBehaviour
{
    public List<GameObject> cellsOnTrack;       // Record cells on this track
    public bool isMoving;                       // Indicates whether the cells on this track should be moving
    public int beatsBtwNotes;                   // Number of beats between two adjacent notes, one beat is 0.5s
    public bool isSpawning;                     // Whether there is a cell whose spawning animation is playing on this track
    public int cellGroupingTracker;             // Used to indicate when a seperation mark needs to be added

    public int noteHits;                        // Used to play bg anim at a given speed

    private void OnEnable()
    {
        cellsOnTrack = new List<GameObject>();     // cellsOnTrack.Count -> 0
        isMoving = false;

        cellGroupingTracker = 0;
        noteHits = 0;
    }

    // Generate a cell based on two cells indexed by two parameters
    public void GenerateCell(int _parentIndexOne, int _parentIndexTwo)
    {
        Cell cellParentOne = this.cellsOnTrack[_parentIndexOne].GetComponent<Cell>();       // Get one parent
        Cell cellParentTwo = this.cellsOnTrack[_parentIndexTwo].GetComponent<Cell>();       // Get another

        if (cellParentTwo.isSeperationMark)                                                 // Checks if the second parent is a seperation mark, if so, get a new parent
        {
            cellParentTwo = this.cellsOnTrack[_parentIndexTwo + 1].GetComponent<Cell>();
        }

        Vector2 tailCellPos = this.cellsOnTrack[cellsOnTrack.Count - 1].transform.position;     // Position of tail cell

        Vector2 _pos;
        if (cellGroupingTracker == 0)
        {
            _pos = new Vector2(tailCellPos.x + 1.25f, tailCellPos.y);   // The gap between two adjacent cells is set to 1.25f since the tail cell is a seperation mark
        }
        else
        {
            _pos = new Vector2(tailCellPos.x + 1.25f, tailCellPos.y);   // The gap between two adjacent cells is set to 1.25f
        }    

        CellStruct cellInfo = GetChildCellInfo(cellParentOne.note, cellParentTwo.note, cellParentOne.noteLevel, cellParentTwo.noteLevel);
        instruments _instru = GetInstru();

        if (cellInfo.note != notes.invalid && cellInfo.level != -1)
        {
            this.isSpawning = true;
            cellsOnTrack.Add(CellPool._instance.Spawn(_pos, cellInfo.note, cellInfo.level, _instru, transform.GetSiblingIndex(), false));
            
            // Debug.Log("Generated a cell with note: " + cellInfo.note + " and level: " + cellInfo.level + " based on cell: " + _parentIndexOne + " and cell: " + _parentIndexTwo);
        }
        else
        {
            Debug.LogError("Invalid spawn information!");
        }

        // Update grouping Indicator
        cellGroupingTracker++;

        // Whether to add a seperation mark, no need to set isSpawning to true since this happens simultaneously with the spawning of the last cell in a group of 5
        if (cellGroupingTracker == 5)
        {
            cellGroupingTracker = 0;
            tailCellPos = this.cellsOnTrack[cellsOnTrack.Count - 1].transform.position;     // Position of tail cell
            _pos = new Vector2(tailCellPos.x + 1.25f, tailCellPos.y);   // The gap between two adjacent cells is set to 1.25f
            cellsOnTrack.Add(CellPool._instance.Spawn(_pos, notes.invalid, -1, instruments.piano, transform.GetSiblingIndex(), true));
        }
    }

    // Gets child cell note and level based on parents
    private CellStruct GetChildCellInfo(notes _noteOne, notes _noteTwo, int _levelOne, int _levelTwo)
    {
        // Creates and initializes a cell struct
        CellStruct cellInfo = new CellStruct();
        cellInfo.note = notes.invalid;
        cellInfo.level = -1;

        // Debug.Log("Receiving parents with notes: " + _noteOne + " and " + _noteTwo);
        int _groupOne = LevelManager._instance.groupingInfo[(int)_noteOne];
        int _groupTwo = LevelManager._instance.groupingInfo[(int)_noteTwo];
        notes[] groupingConfig = LevelManager._instance.noteGroupingConfig;

        if (_groupOne == _groupTwo)
        {
            if (_noteOne == _noteTwo)
            {
                cellInfo.note = _noteOne;

                if (_levelOne != _levelTwo)
                {
                    // Same group, same note, different level
                    cellInfo.level = GetRemainingOneOutOfThree(_levelOne, _levelTwo);
                }
                else
                {
                    // Same group, same note, same level, pick one from the remaining two level randomly
                    do
                    {
                        cellInfo.level = PickRandom(0, 1, 2);
                    } while (cellInfo.level == _levelOne);
                }
            }
            else
            {
                // Same group, different note
                notes tempNote = notes.invalid;
                int tempLevel = -1;

                if (_groupOne == 0)
                {
                    do
                    {
                        tempNote = PickRandom(groupingConfig[0], groupingConfig[1]);
                        tempLevel = PickRandom(0, 1, 2);
                    } while ((tempNote == _noteOne && tempLevel == _levelOne) || (tempNote == _noteTwo && tempLevel == _levelTwo));
                }
                else if (_groupOne == 1)
                {
                    do
                    {
                        tempNote = PickRandom(groupingConfig[2], groupingConfig[3]);
                        tempLevel = PickRandom(0, 1, 2);
                    } while ((tempNote == _noteOne && tempLevel == _levelOne) || (tempNote == _noteTwo && tempLevel == _levelTwo));
                }
                else
                {
                    do
                    {
                        tempNote = PickRandom(groupingConfig[4], groupingConfig[5], groupingConfig[6]);
                        tempLevel = PickRandom(0, 1, 2);
                    } while ((tempNote == _noteOne && tempLevel == _levelOne) || (tempNote == _noteTwo && tempLevel == _levelTwo));
                }

                cellInfo.note = tempNote;
                cellInfo.level = tempLevel;
            }
        }
        else
        {
            // Sets note based on grouping
            int noteGroup = GetRemainingOneOutOfThree(_groupOne, _groupTwo);
            if (noteGroup == 0)
            {
                cellInfo.note = PickRandom(groupingConfig[0], groupingConfig[1]);
            }
            else if (noteGroup == 1)
            {
                cellInfo.note = PickRandom(groupingConfig[2], groupingConfig[3]);
            }
            else
            {
                cellInfo.note = PickRandom(groupingConfig[4], groupingConfig[5], groupingConfig[6]);
            }
            
            // Sets level
            if (_levelOne == 0 && _levelTwo == 0)
            {
                // Different group, both levels are zero
                cellInfo.level = 0;
            }
            else
            {
                // Different group, either level is not zero
                cellInfo.level = PickRandom(0, 1, 2);
            }
        }

        return cellInfo;
    }

    private instruments GetInstru()
    {
        Debug.LogWarning("Get instrument not fully implemented");
        return instruments.piano;
    }

    // Returns 1 if get 0 and 2
    private int GetRemainingOneOutOfThree(int _indexOne, int _indexTwo)
    {
        if (_indexOne == _indexTwo) { Debug.LogError("Cannot get the remaining one while two inputs are the same!"); return -1; }

        int[] arr = Enumerable.Repeat(0, 3).ToArray();     // [0, 0, 0]
        arr[_indexOne] = 1; arr[_indexTwo] = 1;

        for (int i = 0; i < 3; i++)
        {
            if (arr[i] == 0)
            {
                return i;
            }
        }

        return -1;
    }

    // Randomly returns an argument out of many of the same type
    private T PickRandom<T>(params T[] args)
    {
        System.Random random = new System.Random();
        
        if (args == null || args.Length == 0)
        {
            throw new ArgumentException("No arguments provided.");
        }

        int randomIndex = random.Next(args.Length);
        return args[randomIndex];
    }

    // Packed function to start sequential generation of cells
    public void StartSequentialSpawn(int _num)
    {
        StartCoroutine(SpawnCellsSequentially(_num));
    }

    // Sequentially generates cells with a given number
    private IEnumerator SpawnCellsSequentially(int _num)
    {
        for (int i = 0; i < _num; i++)
        {
            while (isSpawning)
            {
                yield return null;
            }

            GenerateCell(i, i + 1);
        }

        while (isSpawning)
        {
            yield return null;
        }

        this.isMoving = true;

        LevelManager._instance.disk.GetComponent<Animator>().SetTrigger("Start");
    }

    private void OnMouseEnter()
    {
        if (!GetComponent<SpriteRenderer>().enabled && LevelManager._instance.isHoldingCellGroup)
        {
            LevelManager._instance.isOnTrack = transform.GetSiblingIndex();

            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void OnMouseExit()
    {
        if (GetComponent<SpriteRenderer>().enabled && transform.GetSiblingIndex() != 0 && cellsOnTrack.Count == 0)
        {
            LevelManager._instance.isOnTrack = -1;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    // Randomly play a background animation
    public void PlayBGAnim()
    {
        noteHits++;

        if (noteHits == 2)
        {
            noteHits = 0;

            System.Random random = new System.Random();
            int index;
            do
            {
                index = random.Next(LevelManager._instance.BGAnimations.childCount);
            } while (LevelManager._instance.BGAnimations.GetChild(index).gameObject.activeSelf);

            LevelManager._instance.BGAnimations.GetChild(index).gameObject.SetActive(true);
        }
    }
}
