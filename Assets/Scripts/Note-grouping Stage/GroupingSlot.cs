using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GroupingSlot : MonoBehaviour
{
    public notes note;   // Record the note being placed on the slot

    void OnEnable()
    {
        note = notes.invalid;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "NoteToBeGrouped")
        {
            collision.GetComponent<NoteToBeGrouped>().slots[transform.GetSiblingIndex()] = 1;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "NoteToBeGrouped")
        {
            collision.GetComponent<NoteToBeGrouped>().slots[transform.GetSiblingIndex()] = 0;
        }
    }
}
