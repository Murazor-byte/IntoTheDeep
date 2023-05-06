using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Color highlightedBorderColor = new Color32(152, 12, 12, 255);
    public List<Button> playerInventoryButtons;

    private GameObject dungeonRetreatObject;
    private GameObject combatRetreatObject;
    private Button dungeonRetreat;
    private Button combatRetreat;

    private GameObject playerUIBackground;
    private GameObject switchToSkills;          //button to switch to combat Skills ui
    public TMP_Text carryLoad;
    public TMP_Text carryWeight;

    public TMP_Text fearGauge;
    public TMP_Text horrorGauge;
    public TMP_Text health;
    public TMP_Text armorValue;
    public Slider fearGaugeValue;
    public Slider horrorGaugeValue;

    public TMP_Text weaponName;
    public RawImage weaponImage;
    public TMP_Text weaponDamage;
    public TMP_Text weaponRange;

    public TMP_Text helmArmorName;
    public TMP_Text bodyArmorName;
    public TMP_Text greavesArmorName;
    public TMP_Text bootsArmorName;

    public RawImage helmArmor;
    public RawImage bodyArmor;
    public RawImage greavesArmor;
    public RawImage bootsArmor;

    public TMP_Text combatHealth;
    public TMP_Text combatArmor;
    public TMP_Text combatFearGauge;
    public TMP_Text combatHorroGauge;
    public Slider combatFearGaugeValue;
    public Slider combatHorrorGaugeValue;
    public TMP_Text selectedSkillName;

    public GameObject eventUIHolder { get; private set; }
    public GameObject eventButton1Object { get; private set; }
    public GameObject eventButton2Object { get; private set; }
    public GameObject eventButton3Object { get; private set; }
    public GameObject eventButton4Object { get; private set; }
    public EventText textEvent;

    public Button eventButton1 { get; private set; }
    public Button eventButton2 { get; private set; }
    public Button eventButton3 { get; private set; }
    public Button eventButton4 { get; private set; }

    public GameObject dungeonShopObject;
    public DungeonShopInventory dungeonShop;
    public Button exitDungeonShop;

    private GameObject combatUI;
    private GameObject turnOrderContainer;
    private List<GameObject> turnOrderIcons;                    //keeps same order as InitiativeOrder in combatManager
    private GameObject endTurnGameObject;
    private Button endTurnButton;
    private GameObject skillsBackground;
    private GameObject skillsContainer;
    private List<RawImage> skillSlotBorders = new List<RawImage>();
    private List<Button> skillSlots = new List<Button>();
    public List<RawImage> skillSlotImages = new List<RawImage>();
    public int selectedSkill;                       //skills slots index on the currently selected skill
    public bool selectingSkills;                    //if the player is currently cycling through their skills

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        StacheUIObject();
    }

    private void StacheUIObject()
    {
        dungeonRetreatObject = GameObject.Find("Dungeon Retreat");
        combatRetreatObject = GameObject.Find("Combat Retreat");
        dungeonRetreat = dungeonRetreatObject.GetComponent<Button>();
        combatRetreat = combatRetreatObject.GetComponent<Button>();

        DisableRetreats();

        playerUIBackground = GameObject.Find("PlayerUI Background");
        switchToSkills = GameObject.Find("Switch To Skills Button");
        ActivateSwitchToSkillsButton(false);
        carryLoad = GameObject.Find("Carry Load").GetComponent<TMP_Text>();
        carryWeight = GameObject.Find("Carry Weight").GetComponent<TMP_Text>();

        fearGauge = GameObject.Find("Fear Gauge Value").GetComponent<TMP_Text>();
        fearGaugeValue = GameObject.Find("Fear Gauge Slider").GetComponent<Slider>();
        horrorGauge = GameObject.Find("Horror Gauge Value").GetComponent<TMP_Text>();
        horrorGaugeValue = GameObject.Find("Horror Gauge Slider").GetComponent<Slider>();
        health = GameObject.Find("Health").GetComponent<TMP_Text>();
        armorValue = GameObject.Find("Armor Value").GetComponent<TMP_Text>();

        weaponName = GameObject.Find("Weapon Name").GetComponent<TMP_Text>();
        weaponImage = GameObject.Find("Weapon Image").GetComponent<RawImage>();
        weaponDamage = GameObject.Find("Weapon Damage").GetComponent<TMP_Text>();
        weaponRange = GameObject.Find("Weapon Range").GetComponent<TMP_Text>();

        helmArmorName = GameObject.Find("Helm Armor Name").GetComponent<TMP_Text>();
        bodyArmorName = GameObject.Find("Body Armor Name").GetComponent<TMP_Text>();
        greavesArmorName = GameObject.Find("Greaves Armor Name").GetComponent<TMP_Text>();
        bootsArmorName = GameObject.Find("Boots Armor Name").GetComponent<TMP_Text>();

        helmArmor = GameObject.Find("Helm Armor Image").GetComponent<RawImage>();
        bodyArmor = GameObject.Find("Body Armor Image").GetComponent<RawImage>();
        greavesArmor = GameObject.Find("Greaves Armor Image").GetComponent<RawImage>();
        bootsArmor = GameObject.Find("Boots Armor Image").GetComponent<RawImage>();

        eventUIHolder = GameObject.Find("Event Background Holder");
        textEvent = GameObject.Find("Event Text").GetComponent<EventText>();
        eventButton1Object = GameObject.Find("Button1");
        eventButton2Object = GameObject.Find("Button2");
        eventButton3Object = GameObject.Find("Button3");
        eventButton4Object = GameObject.Find("Button4");

        dungeonShopObject = GameObject.Find("Dungeon Shop");
        dungeonShop = dungeonShopObject.transform.Find("Dungeon Shop Inventory").GetComponent<DungeonShopInventory>();

        //sets up the dungeon shop exit button listener
        dungeonShopObject.transform.Find("Exit Dungeon Shop").GetComponent<Button>().onClick.AddListener(SetDungeonShopExitButton);
        dungeonShopObject.SetActive(false);

        eventButton1 = eventButton1Object.GetComponent<Button>();
        eventButton2 = eventButton2Object.GetComponent<Button>();
        eventButton3 = eventButton3Object.GetComponent<Button>();
        eventButton4 = eventButton4Object.GetComponent<Button>();

        eventUIHolder.SetActive(false);       //deactivate UI when game starts
        eventButton1Object.SetActive(false);
        eventButton2Object.SetActive(false);
        eventButton3Object.SetActive(false);
        eventButton4Object.SetActive(false);

        combatUI = GameObject.Find("Combat UI");
        turnOrderContainer = GameObject.Find("Turn Order Container");
        endTurnGameObject = GameObject.Find("End Turn Button");

        endTurnButton = endTurnGameObject.GetComponent<Button>();

        skillsBackground = GameObject.Find("Player Skills UI Background");
        skillsContainer = GameObject.Find("Skills Container");
        foreach (Transform skillSlot in skillsContainer.transform)
        {
            skillSlotBorders.Add(skillSlot.GetComponentInChildren<RawImage>());
            skillSlots.Add(skillSlot.GetComponentInChildren<Button>());
            skillSlotImages.Add(skillSlot.transform.Find("Image").GetComponent<RawImage>());
        }

        combatHealth = GameObject.Find("Combat Health Value UI").GetComponent<TMP_Text>();
        combatArmor = GameObject.Find("Combat Armor Value UI").GetComponent<TMP_Text>();
        combatFearGauge = GameObject.Find("Fear Gauge Value").GetComponent<TMP_Text>();
        combatFearGaugeValue = GameObject.Find("Fear Gauge Slider").GetComponent<Slider>();
        combatHorroGauge = GameObject.Find("Horror Gauge Value").GetComponent<TMP_Text>();
        combatHorrorGaugeValue = GameObject.Find("Horror Gauge Slider").GetComponent<Slider>();
        selectedSkillName = GameObject.Find("Selected Skill Name").GetComponent<TMP_Text>();

        skillsBackground.SetActive(false);
        combatUI.SetActive(false);
    }

    //called from GameManager as to not race in Awake after player is spawned and LoadGame
    //param "active" if the buttons should be active or not when referenced
    public void ReferenceInventoryUI(Player player, bool active)
    {
        playerInventoryButtons = new List<Button>();

        foreach (GameObject itemSlotUI in player.inventory.inventorySlots)
        {
            playerInventoryButtons.Add(itemSlotUI.gameObject.GetComponentInChildren<Button>());
        }

        if (!active) SetDungeonInteractivePlayerUI(false);
    }

    //Updating Weapon and Armor UI images

    public void UpdateWeaponImage(Item item)
    {
        weaponImage.texture = item.GetItemImage();
    }

    public void UpdateHelmImage(Item item)
    {
        helmArmor.texture = item.GetItemImage();
    }

    public void UpdateBodyImage(Item item)
    {
        bodyArmor.texture = item.GetItemImage();
    }

    public void UpdateGreavesImage(Item item)
    {
        greavesArmor.texture = item.GetItemImage();
    }

    public void UpdateBootsImage(Item item)
    {
        bootsArmor.texture = item.GetItemImage();
    }

    //Retreat Button Methods

    //disables all interactive player ui buttons in the dungeon such has inventory and dungeon retreat
    //when pausing or during events
    public void SetDungeonInteractivePlayerUI(bool active)
    {
        SetDungeonRetreatActive(active);
        SetInventoryButtons(active);
    }

    public void SetCombatInteractivePlayerUI(bool active)
    {
        SetCombatRetreatActive(active);
        SetInventoryButtons(active);
        endTurnButton.interactable = active;
    }

    public bool DungeonObjectActive() { return dungeonRetreatObject.activeSelf; }

    public bool CombatObjectActive() { return combatRetreatObject.activeSelf; }

    public void DisableRetreats()
    {
        DeactivateDungeonRetreat();
        DeactivateCombatRetreat();
    }

    public void DeactivateDungeonRetreat()
    {
        SetDungeonRetreatActive(false);
        dungeonRetreatObject.SetActive(false);
    }

    public void ActivateDungeonRetreat()
    {
        SetDungeonRetreatActive(true);
        dungeonRetreatObject.SetActive(true);
    }

    public void SetDungeonRetreatActive(bool active)
    {
        dungeonRetreat.interactable = active;
    }

    public void DeactivateCombatRetreat()
    {
        SetCombatRetreatActive(false);
        combatRetreatObject.SetActive(false);
    }

    public void ActivateCombatRetreat()
    {
        SetCombatRetreatActive(true);
        combatRetreatObject.SetActive(true);
    }

    public void SetCombatRetreatActive(bool active)
    {
        combatRetreat.interactable = active;
    }

    //makes all inventory items clickable or not based on parameter
    public void SetInventoryButtons(bool activate)
    {
        for(int i = 0; i < playerInventoryButtons.Count; i++)
        {
            playerInventoryButtons[i].interactable = activate;
        }
    }

    //Event UI

    //adds a listener to an event button
    public void AddListener(Button eventButton, UnityAction listener, bool removeListeners)
    {
        if(removeListeners) eventButton.onClick.RemoveAllListeners();

        eventButton.onClick.AddListener(listener);
    }

    public void DeactivateAllButton()
    {
        eventButton1Object.SetActive(false);
        eventButton2Object.SetActive(false);
        eventButton3Object.SetActive(false);
        eventButton4Object.SetActive(false);
    }


    //Dungeon Shop

    //sets up the shop for every new dungeon shop event
    public void SetUpDungeonShop(Player player)
    {
        dungeonShopObject.SetActive(true);
        dungeonShop.SetUpShop(player);
        dungeonShopObject.SetActive(false);
    }

    //sets the dungeon shop ui active or inactive
    public void SetDungeonShopActive(bool active)
    {
        if (active) eventUIHolder.SetActive(false);
        else eventUIHolder.SetActive(true);

        dungeonShopObject.SetActive(active);
    }

    private void SetDungeonShopExitButton()
    {
        SetDungeonShopActive(false);
    }

    //Combat UI

    public void ActivateCombatUI(bool active)
    {
        if (active)
        {
            combatUI.SetActive(true);
            SetEndTurnButton();
        }
        else
        {
            combatUI.SetActive(false);
        }
    }

    private void SetEndTurnButton()
    {
        endTurnButton.onClick.RemoveAllListeners();
        CombatManager combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
        endTurnButton.onClick.AddListener(combatManager.EndPlayerTurn);
    }

    //set the initial turn order ui based on all characters in combat on initiative
    public void SetTurnOrderUI(List<GameObject> initiativeOrder)
    {
        turnOrderIcons = new List<GameObject>();
        for(int i = 0; i < initiativeOrder.Count; i++)
        {
            GameObject turnOrderIcon = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Combat/Turn Order Icon")) as GameObject;
            turnOrderIcon.transform.parent = turnOrderContainer.transform;
            Texture icon = initiativeOrder[i].GetComponent<RawImage>().texture;
            turnOrderIcon.GetComponentInChildren<RawImage>().texture = icon;
            turnOrderIcons.Add(turnOrderIcon);
        }
    }

    //after each turn, move the current characters turn to the end of the iniative order
    public void UpdateTurnOrderUI(GameObject currentCharacter)
    {
        //remove the character first in initiative order to the last
        if (turnOrderContainer.transform.childCount > 0)
        {
            Destroy(turnOrderContainer.transform.GetChild(0).gameObject);
            turnOrderIcons.RemoveAt(0);
        }
        else
        {
            Debug.Log("Turn order container has No children to remove");
            return;
        }

        GameObject turnOrderIcon = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Combat/Turn Order Icon")) as GameObject;
        Texture icon = currentCharacter.GetComponent<RawImage>().texture;
        turnOrderIcon.GetComponentInChildren<RawImage>().texture = icon;
        turnOrderIcon.transform.parent = turnOrderContainer.transform;
        turnOrderIcons.Add(turnOrderIcon);
    }

    //adds or removes a characters from the turn order icons index
    //where the current turn is used to find the correct icon index (since they continuously are replaced)
    public void PlaceCharacterInInitiative(GameObject characterToAdd, bool addToInitative, int index, int currentTurn)
    {
        if (addToInitative)
        {
            GameObject turnOrderIcon = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Combat/Turn Order Icon")) as GameObject;
            Texture icon = characterToAdd.GetComponent<RawImage>().texture;
            turnOrderIcon.GetComponentInChildren<RawImage>().texture = icon;
            turnOrderIcon.transform.parent = turnOrderContainer.transform;

            turnOrderIcon.transform.SetSiblingIndex(index);
            turnOrderIcons.Insert(index, turnOrderIcon);
        }
        else
        {
            //quick adjustments based on the non-moving initiative order in CM. Character going before the player in CM are positioned after
            //in here, so adding is needed based on the current Turn. 
            //Oppositely, it's the difference between the Total Count and (character and player) indecies if they go first in CM
            if (index > currentTurn) index -= currentTurn;
            else if (index < currentTurn) index = (turnOrderIcons.Count - currentTurn + index);
            Debug.Log("Index to place icon: " + index);
            Destroy(turnOrderIcons[index]);
            turnOrderIcons.RemoveAt(index);
        }
    }

    //removes all turn order UI icons
    public void ClearUITurnOrder()
    {
        foreach (Transform icon in turnOrderContainer.transform)
        {
            Destroy(icon.gameObject);
        }
    }

    //updates the selected border color based on what direction the player is going in the menu, and sets the previous back to normal
    public void UpdateSelectedSkill(bool right)
    {
        skillSlotBorders[selectedSkill].color = Color.white;
        if (!right)     //cycling left
        {
            if (selectedSkill == 0) selectedSkill = skillSlotBorders.Count - 1;
            else selectedSkill--;
        }
        else            //cycling right
        {
            if (selectedSkill == skillSlotBorders.Count - 1) selectedSkill = 0;
            else selectedSkill++;
        }
        skillSlotBorders[selectedSkill].color = highlightedBorderColor;
    }

    //sets the initial selected border to highlighted color, else to white and reset the selected skill to 0
    public void SetSelectedSkillBorder(bool setSelected)
    {
        if (setSelected) skillSlotBorders[selectedSkill].color = highlightedBorderColor;
        else
        {
            skillSlotBorders[selectedSkill].color = Color.white;
            selectedSkill = 0;
        }
    }

    //actiavtes/deactivates player inventory or combat skills ui when cycling through inventory and skills in combat
    public void ActivateCombatPlayerUI(bool skillsUI)
    {
        if (skillsUI)
        {
            playerUIBackground.SetActive(false);
            ActivateSwitchToSkillsButton(false);
            skillsBackground.SetActive(true);
        }
        else
        {
            playerUIBackground.SetActive(true);
            ActivateSwitchToSkillsButton(true);
            skillsBackground.SetActive(false);
        }
    }

    public void ActivateSwitchToSkillsButton(bool active)
    {
        switchToSkills.SetActive(active);
    }

    //sets the players image skill slot images corresponding to the players skill set
    public void SetPlayerSkillSlotImages(Player player)
    {
        for(int i = 0; i < player.skillSet.Count; i++)
        {
            skillSlotImages[i].texture = player.skillSet[i].GetSkillAsset();
        }
    }
}
