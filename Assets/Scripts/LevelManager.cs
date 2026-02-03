using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum notes { a, b, c, d, e, f, g, invalid }
public enum instruments { piano } 

public class LevelManager : MonoBehaviour
{
    public static LevelManager _instance;   // Singleton
    
    public notes[] noteGroupingConfig;      // Record the grouping configuration
    public int[] groupingInfo;              // Notes that are in the same group will have the same value given themselves as the key to the array

    [Header("Note-grouping")]
    public Transform startMenu;             // The parent of start menu decoratory game objects
    public Transform notesToBeGrouped;      // The parent of all notes to be grouped
    public Transform groupingSlots;         // The parent of all slots that receives a note
    public Transform groupingTip;           // The note grouping tip UI Element

    [Header("Track-playing")]
    public Transform trackPlaying;          // The parent of track-playing stage game objects
    public Transform groupingIndicator;     // The parent of game objects that displays grouping and can be used to configure seed
    public Transform trackZeroSlots;        // The parent of track slots on track zero, which is track zero itself

    [Header("Note group-copying")]
    public bool isHoldingCellGroup;         // Whether the mouse is holding a group of 5 cells to copy to other track
    public int trackActivated;              // Number of tracks activated in the scene, max = 3
    public Transform copyGroupParent;       // Game object that is used to as a parent to group 5 notes to be copied
    public int isOnTrack;                   // The index of the track the mouse is on that has no cell moving, -1 means the mouse is not on a track
    public Transform cellTrigger;           // The position of the cell trigger

    [Header("Buttons")]
    public Transform startBtn;              // The parent of start button
    public Transform quitBtn;               // The parent of quit game button
    public Transform backBtn;               // Go back to main when grouping notes
    public Transform confirmGroupingBtn;    // The parent of confirm grouping button
    public Transform beginPlayBtn;          // The parent of begin play button
    public Transform replayBtn;             // The parent of replay button
    public Transform pointerBtn;            // The parent of pointer button

    [Space(10)]

    public Transform tracks;                // The parent of three tracks
    public Transform disk;                  // The disk game object in the scene

    [Space(10)]

    public Sprite[] noteSprites;            // Sprites of all 3*7 notes
    [SerializeField]
    private AudioClip[] pianoClips;         // Audio clips of piano

    public Transform BGAnimations;          // The parent of background animations
    
    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        // Initialize noteGrouping array
        noteGroupingConfig = Enumerable.Repeat(notes.invalid, 7).ToArray();     // noteGrouping -> [7, 7, 7, 7, 7, 7, 7]
        groupingInfo = Enumerable.Repeat(-1, 7).ToArray();                      // groupingInfo -> [-1, -1, -1, -1, -1, -1, -1]

        // Initialize variables
        isHoldingCellGroup = false;
        trackActivated = 0;                                                     // Tracks aren't enabled by the time the game starts, OnEnable won't be called ahead of this
    }

    private void Update()
    {
        if (Time.timeScale != 1)     // Update copy group's position based on the mouse position
        {
            copyGroupParent.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        if (Input.GetMouseButtonUp(0) && isHoldingCellGroup)        // Release the copy group
        {
            if (isOnTrack != -1)
            {
                // Activate the track and place cells on it in allignment
                tracks.GetChild(isOnTrack).GetComponent<SpriteRenderer>().enabled = true;
                tracks.GetChild(isOnTrack).GetComponent<Track>().isMoving = true;
                trackActivated++;
                tracks.GetChild(isOnTrack).GetComponent<BoxCollider2D>().enabled = false;   // Stop blocking the raycast to cells that potentially can be copied

                // Get position for the first cell on track zero and in copy group
                Vector2 leadingCellPos = tracks.GetChild(0).GetComponent<Track>().cellsOnTrack[0].transform.position;
                Vector2 leadingChildPos = copyGroupParent.GetChild(0).transform.position;

                // Set X
                if (leadingChildPos.x < leadingCellPos.x)
                {
                    leadingChildPos = leadingCellPos;
                }
                else
                {
                    // Get the closest cell pos on track zero
                    int index = 0;
                    for (; index < tracks.GetChild(0).GetComponent<Track>().cellsOnTrack.Count - 1; index++)
                    {
                        if (tracks.GetChild(0).GetComponent<Track>().cellsOnTrack[index].transform.position.x <= leadingChildPos.x
                            && 
                            tracks.GetChild(0).GetComponent<Track>().cellsOnTrack[index + 1].transform.position.x > leadingChildPos.x)
                        {
                            break;
                        }
                    }

                    leadingChildPos = tracks.GetChild(0).GetComponent<Track>().cellsOnTrack[index].transform.position;
                }

                // Set Y
                leadingChildPos = new Vector2(leadingChildPos.x, tracks.GetChild(isOnTrack).position.y);

                Transform tempCell = null;
                for (int i = 0; i < 6; i++)
                {
                    tempCell = copyGroupParent.GetChild(0);

                    // Add to track
                    tracks.GetChild(isOnTrack).GetComponent<Track>().cellsOnTrack.Add(copyGroupParent.GetChild(0).gameObject);

                    // Enable movement and collider
                    tempCell.GetComponent<Cell>().canMove = true;
                    tempCell.GetComponent<BoxCollider2D>().enabled = true;

                    // Adjust position and parent child relationship
                    tempCell.position = new Vector2(leadingChildPos.x + i * 1.25f, leadingChildPos.y);
                    tempCell.SetParent(CellPool._instance.gameObject.transform);

                    // Adjust track information
                    tempCell.GetComponent<Cell>().trackIndex = isOnTrack;
                }
            }

            isOnTrack = -1;
            isHoldingCellGroup = false;

            // Release copy group and disactivate the copy group parent
            for (int i = 0; i < copyGroupParent.childCount; i++)
            {
                CellPool._instance.pool.Release(copyGroupParent.GetChild(i).gameObject);
            }

            copyGroupParent.gameObject.SetActive(false);
            Cursor.visible = true;
            Time.timeScale = 1f;
        }
    }

    public AudioClip[] GetClipSetFromInstru(instruments _instru)
    {
        if (_instru == instruments.piano) { return this.pianoClips; }

        return null;
    }
}
