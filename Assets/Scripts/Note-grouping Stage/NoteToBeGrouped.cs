using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class NoteToBeGrouped : MonoBehaviour
{
    public int[] slots;             // Record slots that the player may potentially want to place the note at
    public AudioClip noteClip;      // The audio clip this note will be playing

    private Vector2 mouseOffset;    // Difference between the mouse position on screen and the object position
    private Vector2 initPos;        // Position when the game starts
    [SerializeField]
    private bool isOnSlot;          // Whether the note is put on a slot
    [SerializeField]
    private int slotIndex;          // Number of slot the note is being placed at
    private Vector2 slotOffset;     // Offset between the self position and the placed slot's position

    void OnEnable()
    {
        slots = Enumerable.Repeat(0, 7).ToArray();

        mouseOffset = Vector2.zero;
        if (initPos == Vector2.zero)
        {
            initPos = (Vector2)transform.position;
        }
        isOnSlot = false;
        slotIndex = 7;
        slotOffset = new Vector2(0, 1f);
    }

    private void OnDisable()
    {
        transform.position = initPos;
        GetComponent<AudioSource>().clip = null;
    }

    private void OnMouseDown()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseOffset = mouseWorldPos - (Vector2)transform.position;

        GetComponent<AudioSource>().clip = noteClip;
        GetComponent<AudioSource>().Play();
    }

    private void OnMouseDrag()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mouseWorldPos - mouseOffset;
    }

    private void OnMouseUp()
    {
        // Check for any collided slots
        if (hasCollidedSlot())
        {
            int targetIndex = getClosestSlotCollided();

            // Checks for switch (the note is moved to a non-empty slot) and migrate (the note is moved to an empty slot)
            if (isOnSlot && slotIndex != targetIndex)
            {
                Transform targetSlot = LevelManager._instance.groupingSlots.GetChild(targetIndex);
                Transform currentSlot = LevelManager._instance.groupingSlots.GetChild(slotIndex);

                if (targetSlot.GetComponent<GroupingSlot>().note != notes.invalid)
                {   // Switch - Update note recorded in the current slot, note's position, and slot number recorded in note
                    currentSlot.GetComponent<GroupingSlot>().note = targetSlot.GetComponent<GroupingSlot>().note;
                    LevelManager._instance.notesToBeGrouped.GetChild((int)targetSlot.GetComponent<GroupingSlot>().note).position = (Vector2)currentSlot.position + slotOffset;
                    LevelManager._instance.notesToBeGrouped.GetChild((int)targetSlot.GetComponent<GroupingSlot>().note).GetComponent<NoteToBeGrouped>().slotIndex = slotIndex;
                }
                else
                {   // Migrate
                    currentSlot.GetComponent<GroupingSlot>().note = notes.invalid;
                }
            }
            
            // Record status
            if (!isOnSlot) 
            {
                if (LevelManager._instance.groupingSlots.GetChild(targetIndex).GetComponent<GroupingSlot>().note == notes.invalid)
                {
                    isOnSlot = true;

                    // All notes are placed on slots, display confirm grouping btn
                    if (slotsAreFilledUp())
                    {
                        LevelManager._instance.groupingTip.gameObject.SetActive(false);
                        LevelManager._instance.confirmGroupingBtn.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Trying to occupy a slot that already has a note
                    transform.position = initPos;
                    return;
                }
            }
            slotIndex = targetIndex;
            
            // Set position
            transform.position = (Vector2)LevelManager._instance.groupingSlots.GetChild(slotIndex).position + slotOffset;

            // Record the note on the slot
            LevelManager._instance.groupingSlots.GetChild(slotIndex).GetComponent<GroupingSlot>().note = (notes)transform.GetSiblingIndex();
        }
        else
        {
            if (isOnSlot)
            {
                // Back to from which slot the note is being dragged
                transform.position = (Vector2)LevelManager._instance.groupingSlots.GetChild(slotIndex).position + slotOffset;
            }
            else
            {
                // Back to the initial pos
                transform.position = initPos;
            }
        }
    }

    // Checks if the note is collided with any slot upon mouse up, returns True if so
    private bool hasCollidedSlot()
    {
        foreach (int i in slots)
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

        for (int i = 0; i < 7; i++)
        {
            if (slots[i] == 1)
            {
                float temp = Vector2.Distance(transform.position, LevelManager._instance.groupingSlots.GetChild(i).position);
                if (temp < distance)
                {
                    index = i;
                    distance = temp;
                }
            }
        }

        return index;
    }

    // Checks if all slots are filled with notes by checking whether all notes to be placed are on slots, returns True if so
    private bool slotsAreFilledUp()
    {
        for (int i = 0; i < LevelManager._instance.notesToBeGrouped.childCount; i++)
        {
            if (!LevelManager._instance.notesToBeGrouped.GetChild(i).GetComponent<NoteToBeGrouped>().isOnSlot)
            {
                return false;
            }
        }

        return true;
    }
}
