using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GroupingIndicator : MonoBehaviour
{
    public notes note;              // Record the note this indicator is
    public int[] trackSlots;        // Record slots on track that the player may potentially want to place the note at
    public AudioClip noteClip;      // The audio clip this note will be playing

    private bool isDown;            // Whether the mouse is pressed on this game object
    private GameObject copy;        // Reference to the copy of this game object
    private Vector2 offset;         // Record the offset between mouse position when pressed down and the position of this game object
    private bool isCopy;            // Whether this note is a copy of the grouping indicator or the indicator itself

    // Called upon being enabled, before Start() gets called
    private void OnEnable()
    {
        // Initialize the note if its an indicator, otherwise the note (which is a copy of another) will be assigned by the indicator from which it is copied
        if (transform.parent == LevelManager._instance.groupingIndicator)
        {
            isCopy = false;
            
            note = LevelManager._instance.noteGroupingConfig[transform.GetSiblingIndex()];
            noteClip = LevelManager._instance.GetClipSetFromInstru(instruments.piano)[(int)note];
        }
        else
        {
            isCopy = true;
        }
        
        trackSlots = Enumerable.Repeat(0, 5).ToArray();     // A seed is composed of 5 notes
        isDown = false;
        copy = null;
        offset = Vector2.zero;

        if (!isCopy)  // Only the original indicator (not the copy) will try get the sprite. used to be if (GetComponent<SpriteRenderer>().sprite == null)
        {
            GetComponent<SpriteRenderer>().sprite = LevelManager._instance.notesToBeGrouped.GetChild((int)note).GetComponent<SpriteRenderer>().sprite;
        }
    }

    private void OnDisable()
    {
        GetComponent<AudioSource>().clip = null;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && isDown)
        {
            isDown = false;

            if (copyHasCollidedSlot())
            {
                GameObject targetSlot = LevelManager._instance.trackZeroSlots.GetChild(getClosestSlotCollided()).gameObject;

                targetSlot.GetComponent<TrackSlot>().note = note;

                // Appearance
                targetSlot.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;

                // If all slots on track zero is filled up, activate the play button
                if (isSeedReady())
                {
                    LevelManager._instance.beginPlayBtn.gameObject.SetActive(true);

                    // Add a seperation indicator at the end of the tail cell
                    Debug.LogError("missing the implementation of add a seperation indicator at the end of the seed");
                }
            }

            Destroy(copy);
            copy = null;
        }
        
        if (isDown)
        {
            copy.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
        }
    }

    // Instantiates a copy that follows the cursor
    private void OnMouseDown()
    {
        isDown = true;
        offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        copy = Instantiate(gameObject, transform.position, Quaternion.identity);

        if (!isCopy)
        {
            GetComponent<AudioSource>().clip = noteClip;
            GetComponent<AudioSource>().Play();
        }
    }

    // Checks if the note has collided with any slot upon mouse up, returns true if so
    private bool copyHasCollidedSlot()
    {
        foreach (int i in copy.GetComponent<GroupingIndicator>().trackSlots)
        {
            if (i == 1) { return true; }
        }

        return false;
    }

    // Returns the index of the closest slot collided upon mouse up
    private int getClosestSlotCollided()
    {
        int index = -1;
        float distance = 99999;

        for (int i = 0; i < 5; i++)
        {
            if (copy.GetComponent<GroupingIndicator>().trackSlots[i] == 1)
            {
                float temp = Vector2.Distance(copy.transform.position, LevelManager._instance.trackZeroSlots.GetChild(i).position);
                if (temp < distance)
                {
                    index = i;
                    distance = temp;
                }
            }
        }

        return index;
    }

    // Returns true if all five slots on track zero are placed with a note
    private bool isSeedReady()
    {
        for (int i = 0; i < 5; i++)
        {
            if (LevelManager._instance.trackZeroSlots.GetChild(i).GetComponent<TrackSlot>().note == notes.invalid)
            { return false; }
        }

        return true;
    }
}
