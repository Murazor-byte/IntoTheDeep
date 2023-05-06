using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPlayerStats : MonoBehaviour
{
    private TMP_Text text;
    private Player player;
    private bool inCombat;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Update()
    {
        if (inCombat) UpdateCombatPlayerUI();
        else UpdatePlayerUI();
    }

    //sets new characters UI to display and their individual stats
        //Also update player combat UI if in combat
    public void SetNewPlayer(in Player newPlayer, bool inCombat)
    {
        player = newPlayer;
        this.inCombat = inCombat;
    }

    private void UpdatePlayerUI()
    {
        UpdateInventoryText();
        UpdateOverallStats();
        UpdateWeaponText();
        UpdateArmorText();
    }

    private void UpdateInventoryText()
    {
        UIManager.Instance.carryLoad.text = "Carry Load\n\n" + player.inventoryCarryLoad;
        UIManager.Instance.carryWeight.text = "Carry Weight\n\n" + player.inventoryCarryWeight;
    }

    private void UpdateOverallStats()
    {
        UIManager.Instance.health.text = "Health\n\n" + player.health + "/" + player.healthCap;
        UIManager.Instance.armorValue.text = "Armor\n\n" + player.armor;
        UIManager.Instance.fearGauge.text = "Fear\t\t\t" + player.fear + "/100";
        UIManager.Instance.horrorGauge.text = "Horror\t\t" + player.horror + "/100";
        UIManager.Instance.fearGaugeValue.value = (float)player.fear / 100;
        UIManager.Instance.horrorGaugeValue.value = (float)player.horror / 100;
    }

    private void UpdateWeaponText()
    {
        UIManager.Instance.weaponName.text = "" + player.weapon.GetType();
        UIManager.Instance.weaponDamage.text = "Damage\n\n" + player.damage.ToString();
        UIManager.Instance.weaponRange.text = "Range\n\n" + player.weaponRange.ToString();
    }

    private void UpdateArmorText()
    {
        UIManager.Instance.helmArmorName.text = "" + player.helmArmor.GetType();
        UIManager.Instance.bodyArmorName.text = "" + player.bodyArmor.GetType();
        UIManager.Instance.greavesArmorName.text = "" + player.greavesArmor.GetType();
        UIManager.Instance.bootsArmorName.text = "" + player.bootArmor.GetType();
    }

    private void UpdateCombatPlayerUI()
    {
        UIManager.Instance.combatHealth.text = "" + player.health;
        UIManager.Instance.combatArmor.text = "" + player.armor;
        UIManager.Instance.combatFearGauge.text = "Fear\t\t\t" + player.fear + "/100";
        UIManager.Instance.combatHorroGauge.text = "Horror\t\t\t" + player.horror + "/100";
        UIManager.Instance.combatFearGaugeValue.value = (float)player.fear / 100;
        UIManager.Instance.combatHorrorGaugeValue.value = (float)player.horror / 100;

        if (UIManager.Instance.selectingSkills && UIManager.Instance.selectedSkill <= player.skillSet.Count - 1)
        {
            UIManager.Instance.selectedSkillName.text = "" + player.skillSet[UIManager.Instance.selectedSkill].GetType().ToString();
        }
        else if(UIManager.Instance.selectingSkills && UIManager.Instance.selectedSkill > player.skillSet.Count)
        {
            UIManager.Instance.selectedSkillName.text = "" + player.skillSet[player.skillSet.Count - 1].GetType().ToString();
        }
        else UIManager.Instance.selectedSkillName.text = "" + player.currentSkill.GetType().ToString();
    }
}
