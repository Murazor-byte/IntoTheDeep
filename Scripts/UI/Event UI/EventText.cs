using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventText : MonoBehaviour
{
    protected TMP_Text eventText;

    private void Awake()
    {
        eventText = GetComponent<TMP_Text>();
    }

    public void UpdateEventText(string text)
    {
        eventText.text = text;
    }

}
