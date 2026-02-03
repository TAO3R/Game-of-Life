using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnFunctions : MonoBehaviour
{
    // Set two children under Note Grouping as active, set start and quit buttons as inactive
    public void StartGame()
    {
        LevelManager._instance.notesToBeGrouped.gameObject.SetActive(true);
        LevelManager._instance.groupingSlots.gameObject.SetActive(true);
        LevelManager._instance.groupingTip.gameObject.SetActive(true);
        LevelManager._instance.backBtn.gameObject.SetActive(true);

        LevelManager._instance.startBtn.gameObject.SetActive(false);
        LevelManager._instance.quitBtn.gameObject.SetActive(false);
        LevelManager._instance.startMenu.gameObject.SetActive(false);
    }

    // Quit the game
    public void Quit()
    {
        Application.Quit();
    }

    // Pass the grouping configuration to LevelManager script
    public void ConfirmGrouping()
    {
        // Record grouping configuration and grouping info based on configuration, which is used for cell generation -> randomly arranged [0, 0, 1, 1, 2, 2, 2]
        for (int i = 0; i < 7; i++)
        {
            LevelManager._instance.noteGroupingConfig[i] = LevelManager._instance.groupingSlots.GetChild(i).GetComponent<GroupingSlot>().note;

            if (i == 0 || i == 1)
            {
                LevelManager._instance.groupingInfo[(int)LevelManager._instance.noteGroupingConfig[i]] = 0;
            }
            else if (i == 2 || i == 3)
            {
                LevelManager._instance.groupingInfo[(int)LevelManager._instance.noteGroupingConfig[i]] = 1;
            }
            else
            {
                LevelManager._instance.groupingInfo[(int)LevelManager._instance.noteGroupingConfig[i]] = 2;
            }
        }

        // Activate track-playing stage parent for configuring seed
        LevelManager._instance.trackPlaying.gameObject.SetActive(true);

        // Activate seed slots on track zero
        for (int i = 0; i < 5; i++)
        {
            LevelManager._instance.trackZeroSlots.GetChild(i).gameObject.SetActive(true);
        }

        // Activate replay button
        LevelManager._instance.replayBtn.gameObject.SetActive(true);

        // Disactivate note-grouping stage game objects and confirm-grouping button
        LevelManager._instance.notesToBeGrouped.gameObject.SetActive(false);
        LevelManager._instance.groupingSlots.gameObject.SetActive(false);
        LevelManager._instance.confirmGroupingBtn.gameObject.SetActive(false);
        LevelManager._instance.backBtn.gameObject.SetActive(false);

        // Update activated track count for track zero
        LevelManager._instance.trackActivated = 1;
    }

    // Start to play with configured seed
    public void BeginPlay()
    {
        LevelManager._instance.pointerBtn.gameObject.SetActive(true);

        // Instantiate 5 cells based on the seed selection on track zero
        LevelManager._instance.trackZeroSlots.GetComponent<Track>().isSpawning = true;
        Transform tempSlotTrans = null;

        for (int i = 0; i < 5; i++)
        {
            tempSlotTrans = LevelManager._instance.trackZeroSlots.GetChild(i);
            //  Spawn a cell on slot with type indicated by the seed, default level (0), instrument (0), and is on track indexed 0
            LevelManager._instance.trackZeroSlots.GetComponent<Track>().cellsOnTrack.Add(CellPool._instance.Spawn(tempSlotTrans.position, tempSlotTrans.GetComponent<TrackSlot>().note, 0, 0, 0, false));

            // Disactivate the slot
            LevelManager._instance.trackZeroSlots.GetChild(i).gameObject.SetActive(false);
        }

        // Since Spawn() instead of GenerateCell() is used, which will not automatically increment the cellGroupingTracker, a seperation mark needs to be manually added
        Vector2 _pos = new Vector2(tempSlotTrans.position.x + 1.25f, tempSlotTrans.position.y);
        LevelManager._instance.trackZeroSlots.GetComponent<Track>().cellsOnTrack.Add(CellPool._instance.Spawn(_pos, notes.invalid, 0, 0, 0, true));

        // Generation 5 cells one at a time first
        LevelManager._instance.trackZeroSlots.GetComponent<Track>().StartSequentialSpawn(5);

        // Disactivate self
        LevelManager._instance.beginPlayBtn.gameObject.SetActive(false);
    }

    // Back to the state where the player has just launched the game
    public void Back()
    {
        LevelManager._instance.startBtn.gameObject.SetActive(true);
        LevelManager._instance.quitBtn.gameObject.SetActive(true);
        LevelManager._instance.startMenu.gameObject.SetActive(true);

        LevelManager._instance.notesToBeGrouped.gameObject.SetActive(false);
        LevelManager._instance.groupingSlots.gameObject.SetActive(false);
        LevelManager._instance.groupingTip.gameObject.SetActive(false);
        LevelManager._instance.backBtn.gameObject.SetActive(false);
        LevelManager._instance.confirmGroupingBtn.gameObject.SetActive(false);
    }

    // Back to the state where the player has just clicked the Start Btn
    public void Replay()
    {
        LevelManager._instance.notesToBeGrouped.gameObject.SetActive(true);
        LevelManager._instance.groupingSlots.gameObject.SetActive(true);
        LevelManager._instance.groupingTip.gameObject.SetActive(true);
        LevelManager._instance.backBtn.gameObject.SetActive(true);

        LevelManager._instance.replayBtn.gameObject.SetActive(false);
        LevelManager._instance.trackPlaying.gameObject.SetActive(false);
        LevelManager._instance.pointerBtn.gameObject.SetActive(false);
        LevelManager._instance.beginPlayBtn.gameObject.SetActive(false);

        LevelManager._instance.copyGroupParent.gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = true;

        // Release all cells into cell pool
        int cellNum = CellPool._instance.transform.childCount;
        for (int i = 0; i < cellNum; i++)
        {
            if (CellPool._instance.transform.GetChild(i).gameObject.activeSelf)
            {
                CellPool._instance.pool.Release(CellPool._instance.transform.GetChild(i).gameObject);
            }
        }

        // Disactivate any on-going bg anims
        for (int i = 0; i < LevelManager._instance.BGAnimations.childCount; i++)
        {
            LevelManager._instance.BGAnimations.GetChild(i).gameObject.SetActive(false);
        }

        // Disable track one and two spriterenderer
        for (int i = 1; i < 3; i++)
        {
            LevelManager._instance.tracks.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
            LevelManager._instance.tracks.GetChild(i).GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    // The player presses for a pointer to manipulate the track
    public void Pointer()
    {
        if (Time.timeScale != 1)
        {
            Cursor.visible = true;
            Time.timeScale = 1;
            LevelManager._instance.copyGroupParent.gameObject.SetActive(false);
        }
        else
        {
            // Hide cursor
            Cursor.visible = false;

            // Slow down time
            Time.timeScale = 0.2f;

            // Activate copy group parent to follow the mouse
            LevelManager._instance.copyGroupParent.gameObject.SetActive(true);
        }
    }
}
