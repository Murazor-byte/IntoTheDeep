using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GainWeaponEvent : PositiveOrdeal
{
    private Weapon weaponToGive;
    [SerializeField] private static Button dungeonRetreat;
    [SerializeField] private static Button combatRetreat;

    public GainWeaponEvent(GameObject player, ScenesManager sceneManager) : base(player, sceneManager) { } 

    public override void SetUpEvent()
    {
        List<Weapon> weaponSelection = new List<Weapon>() { new Sword(), new BattleAxe(), new Mace(), new Halberd(), new Maul() };
        ProbabilityGenerator weaponSelector = new ProbabilityGenerator(new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f });
        int weaponSelected = weaponSelector.GenerateNumber();

        weaponToGive = weaponSelection[weaponSelected];

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
        eventButtonObject.GetComponentInChildren<Text>().text = "Pick up a " + weaponToGive.GetType();
    }

    protected override void UpdateEventButton()
    {
        UIManager.Instance.AddListener(UIManager.Instance.eventButton1, UpdateEventButtonListener, true);
    }

    public override void UpdateEventButtonListener()
    {
        Player playerScript = player.GetComponent<Player>();
        playerScript.inventory.AddItem(weaponToGive, playerScript);
        EndEvent();
    }
}
