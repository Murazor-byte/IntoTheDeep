using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Character
{
    private static List<Armor> armorSelection = new List<Armor>() {new MailHelm(), new ChainMail(), new MailGreaves(), new MailBoots() };
    private static List<Weapon> weaponSelection = new List<Weapon>() {new Sword(), new Unarmed(), new LongBow(), new ShortBow() };
    //private static List<Weapon> weaponSelection = new List<Weapon>() { new MightOfZeus()};
    private static float[] weaponProb = new float[] { 0.25f, 0.25f, 0.25f, 0.25f };
    //private static float[] weaponProb = new float[] { 1f };

    public UIPlayerStats playerUI;                         //for updating the player UI stats

    public Player() : base(1, 5, 5, 5, 10) { } // 40 & 60, 3 & 7, 10

    void Awake()
    {
        SetHealth(UnityEngine.Random.Range(GetMinHealth(), GetMaxHealth()));
        healthCap = health;
        speed = UnityEngine.Random.Range(GetMinSpeed(), GetMaxSpeed());

        EquipArmor();
        EquipWeapon();


        //damageModifier += 1;
        ChangeDamageModifier(1);
        carryCapacity = 10000000;
        inventoryCarryLoad = 16;
        inventory = new Inventory(inventoryCarryLoad, carryCapacity);
    }

    /*public override IEnumerator Attack(Character enemyToAttack, CombatManager combatManager)
    {
        Debug.Log("Enemy to attack name: " + enemyToAttack.name + " position " + enemyToAttack.transform.position);
        double damageReduction;
        int incomingDamage = damage;

        switch (enemyToAttack.armor)
        {
            case int armor when (armor <= 0):                     //takes flat damage w/ no armor
                Debug.Log("Enemy has a 0% damage reduction");
                damageReduction = 0;
                break;
            case int armor when (armor >= 50):
                Debug.Log("Enemy has a 50% damage reduction");
                damageReduction = damage * 0.5;
                break;
            default:
                Debug.Log("Enemy has a " + enemyToAttack.armor + "% damage reduction");
                damageReduction = damage * ((double)enemyToAttack.armor / 100);
                break;
        }
        incomingDamage -= (int)Math.Round(damageReduction);

        enemyToAttack.TakeDamage(incomingDamage, true);

        if(enemyToAttack.health <= 0)
        {
            Debug.Log("Player killed an Enemy");
            combatManager.RemoveCharacterFromInitiative(enemyToAttack.gameObject);
            enemiesKilled++;
        }
        yield return null;
    }*/


    public void EquipArmor()
    {
        for(int i = 0; i < armorSelection.Count; i++)
        {

            switch (i)
            {
                case 0:
                    helmArmor = armorSelection[i];
                    UIManager.Instance.UpdateHelmImage(armorSelection[i]);
                    break;
                case 1:
                    bodyArmor = armorSelection[i];
                    UIManager.Instance.UpdateBodyImage(armorSelection[i]);
                    break;
                case 2:
                    greavesArmor = armorSelection[i];
                    UIManager.Instance.UpdateGreavesImage(armorSelection[i]);
                    break;
                case 3:
                    bootArmor = armorSelection[i];
                    UIManager.Instance.UpdateBootsImage(armorSelection[i]);
                    break;
            }
            armor += armorSelection[i].GetArmorValue();
        }
    }

    public void EquipWeapon()
    {
        ProbabilityGenerator selectedWeaponPiece = new ProbabilityGenerator(weaponProb);
        int selectedWeapon = selectedWeaponPiece.GenerateNumber();

        weapon = weaponSelection[selectedWeapon];

        UIManager.Instance.UpdateWeaponImage(weapon);
        SetDamage(weaponSelection[selectedWeapon].GetWeaponDamage());
        weaponRange = weaponSelection[selectedWeapon].GetWeaponRange();
    }

    //Equips the character with a new weapon that they 'picked up'
    public override void EquipNewWeapon(Weapon newWeapon)
    {
        weapon = newWeapon;

        UIManager.Instance.UpdateWeaponImage(newWeapon);
        SetDamage(newWeapon.GetWeaponDamage());
        weaponRange = newWeapon.GetWeaponRange();
    }

    //Equips the character with a new armor piece that they 'picked up'
    public override void EquipNewArmor(Armor newArmor)
    {
        switch (newArmor)
        {
            case IHelm h:
                armor -= helmArmor.GetArmorValue();
                UIManager.Instance.UpdateHelmImage(newArmor);
                //speed -= helmArmor.speedModifier;
                helmArmor = newArmor;
                break;
            case IBodyArmor b:
                armor -= bodyArmor.GetArmorValue();
                UIManager.Instance.UpdateBodyImage(newArmor);
                bodyArmor = newArmor;
                break;
            case IGreaves g:
                armor -= greavesArmor.GetArmorValue();
                UIManager.Instance.UpdateGreavesImage(newArmor);
                greavesArmor = newArmor;
                break;
            case IBoots b:
                armor -= bootArmor.GetArmorValue();
                UIManager.Instance.UpdateBootsImage(newArmor);
                bootArmor = newArmor;
                break;
        }
        armor += newArmor.GetArmorValue();
    }

    //Add inital starter items
    public void GiveStartingItems()
    {
        for (int i = 0; i < 2500; i++)
        {
            inventory.AddItem(new Gold(), this);
        }
        inventory.AddItem(new Ration(), this);

        for (int i = 0; i < 10; i++)
        {
            inventory.AddItem(new Potion_Healing(), this);
        }
        inventory.AddItem(new Camp(), this);
        inventory.AddItem(new Rope(), this);
        inventory.AddItem(new Shovel(), this);
    }

    public void GiveStartingSkills()
    {
        skillSet.Add(new Skill_CombatMove());
        skillSet.Add(new Skill_Charge());
        skillSet.Add(new Skill_Strike());
        skillSet.Add(new Skill_Shoot());
        skillSet.Add(new Skill_Dodge());
        skillSet.Add(new Skill_Cleave());
        currentSkill = skillSet[0];
    }

    public override void Heal(int healthGained)
    {
        if(health + healthGained > healthCap)
        {
            SetHealth(healthCap);
            Debug.Log("Player healed to Max Health");
        }
        else
        {
            Heal(healthGained);
        }
    }

    public void AddFear(int fearGained)
    {
        fear += fearGained;
        if (fear <= 0) fear = 0;
        else if (fear >= 100) fear = 100;
    }

    public void AddHorror(int horrorAdded)
    {
        horror += horrorAdded;
        if (horror <= 0) horror = 0;
        else if (horror >= 100) horror = 100; 
    }

    //sets the combat player and dungeon player to reference same playerUI
    public void SetUpPlayerUI(ref UIPlayerStats playerUI)
    {
        playerUI = this.playerUI;
    }
}
