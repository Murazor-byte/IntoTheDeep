using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GainArmorEvent : PositiveOrdeal
{
    private Armor armorToGive;

    public GainArmorEvent(GameObject player, ScenesManager sceneManager) : base (player, sceneManager) { }

    public override void SetUpEvent()
    {
        List<Armor> armorSelection = new List<Armor>() { new ChainMail(), new ClothShirt(), new ClothBoots(), new MailBoots(), new ClothGreaves(), new MailGreaves(), new ClothHelm(), new MailHelm()};
        ProbabilityGenerator armorSelector = new ProbabilityGenerator(new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f });
        int armorSelected = armorSelector.GenerateNumber();

        armorToGive = armorSelection[armorSelected];

        SetUIActive();
        UpdateEventText();
        UpdateButtonText(UIManager.Instance.eventButton1Object);
        UpdateEventButton();
    }

    protected override void UpdateEventText()
    {
        string eventText = "An item lays glimmering in the shadow before you confident its previous owner wont have need of it any time soon.";
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        eventButtonObject.GetComponentInChildren<Text>().text = "Pick up a " + armorToGive.GetType();
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void UpdateEventButtonListener()
    {
        Player playerScript = player.GetComponent<Player>();
        playerScript.inventory.AddItem(armorToGive,playerScript);
        EndEvent();
    }

}
