using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrifiedTownsfolkEvent : DecisionOrdeal
{
    private int fearLost;
    private int rationsNeeded;
    private bool hasRation;

    public TerrifiedTownsfolkEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        rationsNeeded = Random.Range(1, 3);

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Give Rations");
        UpdateButtonText(UIManager.Instance.eventButton2Object, "Move On");
        UpdateEventButton();

        for (int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if (playerScript.inventory.inventory[i] is Ration) hasRation = true;
        }

        if(!hasRation) UIManager.Instance.eventButton1.interactable = false;
    }

    private void ContinueEvent()
    {
        UpdateGiveRationsEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object, "Lose " + fearLost + " fear");
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, LeaveEvent, true);

        UIManager.Instance.eventButton2Object.SetActive(false);
        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -30f, 0f);
    }

    protected override void UpdateEventText()
    {
        string eventText = "A strange noise draws your attention ahead unsure from exactly where. With further inspection you notice a man, " +
            "holding a ragged look with fear in his eyes. Another lost stranger looking for a way out. He begs you for food in finding his exit.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateGiveRationsEventText()
    {
        string eventText = "At the presence of food he nearly leaps at your grasp, filling his mouth with its portions and stuffing what remains in his pouches." +
            " With a heartfelt thanks and some advice towards his exit, he vanishes back into the darkness form where you came.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    private void UpdateButtonText(GameObject eventButtonObject, string buttonText)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = buttonText;
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
        UIManager.Instance.AddListener(UIManager.Instance.eventButton2, MoveOn, true);
    }

    public override void UpdateEventButtonListener()
    {
        for(int i = 0; i < playerScript.inventory.inventorySlots.Count; i++)
        {
            if(playerScript.inventory.inventory[i] is Ration)
            {
                Debug.Log("Again player has rations at index " + i + " and needs " + rationsNeeded + " rations, but has amount" + playerScript.inventory.inventory[i].quantity);
                //has enough to give all rations needed
                if(playerScript.inventory.inventory[i].quantity >= rationsNeeded)
                {
                    Debug.Log("Giving all rations");
                    for(int j = 0; j < rationsNeeded; j++)
                    {
                        playerScript.inventory.inventory[i].UseItem();
                    }
                    fearLost = Random.Range(5, 10);
                }
                //has enough to give some rations needed
                else
                {
                    for (int j = 0; j < playerScript.inventory.inventory[i].quantity; j++)
                    {
                        Debug.Log("Giving some rations");
                        playerScript.inventory.inventory[i].UseItem();
                    }
                    fearLost = Random.Range(2, 6);
                }
                break;
            }
        }
        ContinueEvent();
    }

    private void MoveOn()
    {
        UIManager.Instance.eventButton1.interactable = true;
        EndEvent();
    }

    private void LeaveEvent()
    {
        playerScript.AddFear(-fearLost);
        EndEvent();
    }
}
