using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TrackSlot : MonoBehaviour
{
    public notes note;      // Record the note
    private Sprite square;

    private void OnEnable()
    {
        note = notes.invalid;
        square = GetComponent<SpriteRenderer>().sprite;
    }

    private void OnDisable()
    {
        GetComponent<SpriteRenderer>().sprite = square;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "GroupingIndicator")
        {
            collision.GetComponent<GroupingIndicator>().trackSlots[transform.GetSiblingIndex()] = 1;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "GroupingIndicator")
        {
            collision.GetComponent<GroupingIndicator>().trackSlots[transform.GetSiblingIndex()] = 0;
        }
    }
}
