using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Character : MonoBehaviour
{
    public int layerNumber;                                     //what layer number the character is currently in (if any)
    public List<Effect> statusEffects = new List<Effect>();
    public Effect currentEffectTileOn;
    /*IMPORTANT!!! what type of effect tile the Character is on, null by 'default',
    * when moving out of an effect tile it's set to null,
    * when moving into an effectTile that effect is added to their 'statusEffects'
    * if some ability/spell affects what tile the character is standing on, 
    * THIS VARIBALE MUST BE CHANGED TO EITHER NULL OR A NEW STATUS EFFECT*/

    public List<Skill> skillSet = new List<Skill>();
    public Skill currentSkill;

    public Vector3 previousLocation;                            //checking if the character has moved since previous turn
    private float rotationSpeed = 1000f;                        //how fast characters rotate during movement

    protected int minHealth;
    protected int maxHealth;
    protected int minSpeed;
    protected int maxSpeed;

    public int health;                                          //make private set later
    public int healthCap;
    public float deathRoll = 0.1f;                              //prob for player to die when below 0 hp
    private const float DEATHROLLINCREASE = 0.1f;               //prob increase to deathRoll for every succeeded deathRoll

    public int fear;
    public int horror;

    public int armor;
    public int speed;
    public int gold;
    public int inventoryCarryWeight;                            //How much the character is currently holding in their inventory
    public int carryCapacity;                                   //How much the character can hold
    public int inventoryCarryLoad;                              //How many inventory slots the character can hold
    public int weaponRange;                                     //based on what weapon the character has equiped
    public Armor helmArmor;
    public Armor bodyArmor;
    public Armor greavesArmor;
    public Armor bootArmor;
    public Weapon weapon;
    public int damage = 0;                                      //make private set later
    public int damageModifier;                                  //make private set later

    public int enemiesKilled;
    public bool characterTurn;                                  //turn in comabt

    public int combatRating;                                    //rating based on difficulty of killing an enemy
                                                                //rating of 10 means a single hero should be equivalent in strength to the enemy
    public float chanceToHit = 1f;                              //chance to hit a target in combat (reduced through skill and other effects)

    public Inventory inventory;

   protected Character(int minHealth, int maxHealth, int minSpeed, int maxSpeed, int combatRating)
   {
        this.combatRating = combatRating;
        this.minHealth = minHealth;
        this.maxHealth = maxHealth;
        this.minSpeed = minSpeed;
        this.maxSpeed = maxSpeed;
   }

    public int GetMinHealth() { return minHealth; }

    public int GetMaxHealth() { return maxHealth; }

    public int GetMinSpeed() { return minSpeed; }

    public int GetMaxSpeed() { return maxSpeed; }

    //creates one instance of an inventory for this character
    public void CreateInventory(int inventoryCarryLoad, int carryCapacity)
    {
        inventory = new Inventory(inventoryCarryLoad, carryCapacity);
    }

    public virtual IEnumerator Attack(Character character, CombatManager combatManager)
    {
        double damageReduction;
        int incomingDamage = damage;

        switch (character.armor)
        {
            case int armor when (armor <= 0):                     //takes flat damage w/ no armor
                Debug.Log("Player has a 0% damage reduction");
                damageReduction = 0;
                break;
            case int armor when (armor >= 50):
                Debug.Log("Player has a 50% damage reduction");
                damageReduction = damage * 0.5;
                break;
            default:
                Debug.Log("Player has a " + character.armor + "% damage reduction");
                damageReduction = damage * ((double)character.armor / 100);
                break;
        }
        Debug.Log("Damage Reduction Value = " + damageReduction);
        Debug.Log("Damage Reduction after calculatoin = " + (int)System.Math.Round(damageReduction));
        incomingDamage -= (int)System.Math.Round(damageReduction);
        Debug.Log("Damage being dealt after calculation = " + incomingDamage);

        character.TakeDamage(incomingDamage, this, combatManager);

        yield return new WaitForFixedUpdate();
    }

    //applies all of the effects currently on this character
    public void ApplyStatusEffects()
    {
        if (statusEffects.Count == 0) return;

        for(int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i] is Injury) continue;       //ignore injuries as they're "permanent"
            statusEffects[i].ApplyEffect();
        }
    }

    //sets the characters previous location when they have stopped moving
    public void SetCharacterLocation()
    {
        previousLocation = new Vector3(transform.position.x, 1, transform.position.z);
    }

    //checks if the character hasn't moved and is on a combatTile to reapply it's effect
    public void ReapplyEffects()
    {
        if (currentEffectTileOn == null ||previousLocation == null || statusEffects.Count == 0 ) return;


        if(previousLocation.x != transform.position.x && previousLocation.z != transform.position.z)
        {
            return;
        }

        Debug.Log("Reapplying effects");
        //else the character hasn't moved and effect is reapplied
        for(int i = 0; i < statusEffects.Count; i++)
        {
            if(statusEffects[i].GetType() == currentEffectTileOn.GetType())
            {
                statusEffects[i].ReapplyEffect();
            }
        }
    }

    //sets the correct rotation of the character based on combat movement
    public void SetRotation(Vector3 locationToMove)
    {
        Vector3 targetDirection = locationToMove - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection, Vector3.up);
    }

    //calculates the distance between two characters
    protected int DistanceToTarget(Character character)
    {
        int xDistance = (int)Mathf.Abs(character.transform.position.x - transform.position.x);
        int zDistance = (int)Mathf.Abs(character.transform.position.z - transform.position.z);

        int xPath = (int)Mathf.Pow(xDistance, 2);
        int zPath = (int)Mathf.Pow(zDistance, 2);

        int pathDistance = (int)Mathf.Sqrt(xPath + zPath);

        return pathDistance;
    }

    public virtual void EquipNewArmor(Armor armor) { }

    public virtual void EquipNewWeapon(Weapon weapon) { }

    public virtual void Heal(int healthGained) { }

    public void SetHealth(int health) { this.health = health; }

    //damage taken in combat
    public void TakeDamage(int damageTaken, Character attacker, CombatManager combatManager)
    {
        if(Random.value <= chanceToHit)
        {
            Debug.Log("Character has taken damage: " + damageTaken);
            health -= damageTaken;
        }
        else
        {
            Debug.Log("Character has missed the enemy");
        }

        if (health <= 0)
        {
            if(this is Player)
            {
                RollDeathSave(true);
            }
            else
            {
                combatManager.RemoveCharacterFromInitiative(gameObject);
                attacker.enemiesKilled++;
            }
        }
    }

    //damage taken outside of combat
    public void TakeDamage(int damageTaken)
    {
        Debug.Log("Character has taken: " + damageTaken + " damage");
        health -= damageTaken;

        if (this is Player && health <= 0)
        {
            RollDeathSave(false);
        }
    }

    //I should change this, as equipping weapons and re/assinging damage in combatManager conflict
        //(so I dont have to subtract damage modifier in combatManager)
    public void SetDamage(int damageSet)
    {
        damage = damageSet + damageModifier;
    }

    public void ChangeDamage(int damageChange)
    {
        Debug.Log("Changing Damage: " + damageChange);
        damage -= damageModifier;
        damage += damageChange;
        if (damage <= 0) damage = 1;
        damage += damageModifier;
    }

    public void ChangeDamageModifier(int damageModifier)
    {
        damage -= this.damageModifier;
        this.damageModifier += damageModifier;
        if (this.damageModifier <= 0) this.damageModifier = 0;
        damage += this.damageModifier;
    }

    public void SetDamageModifier(int damageModifier)
    {
        damage -= this.damageModifier;
        this.damageModifier = damageModifier;
        damage += this.damageModifier;
    }

    public void ChangeChanceToHit(float toHitChange)
    {
        chanceToHit += toHitChange;
        if (chanceToHit < 0) chanceToHit = 0f;
        if (chanceToHit > 1f) chanceToHit = 1f;
    }

    private void RollDeathSave(bool inCombat)
    {
        Debug.Log("Rolling for Death Save...");
        Debug.Log("Probability of Death: " + deathRoll);

        DungeonManager dungeonManager = GameObject.Find("DungeonManager").GetComponent<DungeonManager>();
        dungeonManager.playerDeath = true;

        if (Random.value < deathRoll)
        {
            Debug.Log("Player has died");
            GameManager gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
            gameManager.PlayerDeathEndGame(inCombat);
        }
        else
        {
            Injury.SustainInjury(this);
            Debug.Log("Increasing deathRoll");
            deathRoll += DEATHROLLINCREASE;
            LoseLoot();
            RecoverToTown(inCombat, dungeonManager);
            health = 1;
        }
    }

    //after sustaining an injury the character will lose some items base on what they have
    public void LoseLoot()
    {
        int numberItems = 0;        //number unique items in characters inventory
        int totalNumItems = 0;      //total quantity of items in characters inventory

        int goldSlot = 0;
        const float MAXLOSTGOLD = 0.35f;
        const float MINLOSTGOLD = 0.05f;

        //set above local fields based on what's currently in the characters inventory
        for(int i = 0; i < inventoryCarryLoad; i++)
        {
            if(!(inventory.inventory[i] is EmptySlot))
            {
                if(inventory.inventory[i] is Gold)
                {
                    goldSlot = i;
                }
                else
                {
                    totalNumItems += inventory.inventory[i].quantity;
                    numberItems++;
                }
            }
        }

        int goldPartition = 40;                                         //# gold that character has will increase by "goldIncr" amount
        float goldIncr = 0.1f;
        float probToLoseGold = (gold / goldPartition) * goldIncr;       //for every "goldParition" gold character has prob to lose gold is increased by 1%

        //Character loses gold
        if(gold > 0 && Random.value <= probToLoseGold)
        {
            int numLostGold = (int)Random.Range(gold * MINLOSTGOLD, gold * MAXLOSTGOLD);
            if (numLostGold < gold)
            {
                Debug.Log("Player has lost " + numLostGold + "Gold");
                gold -= numLostGold;

                for (int i = 0; i < numLostGold; i++)
                {
                    inventory.RemoveItem(inventory.inventory[goldSlot]);
                }
            }
        }
        else
        {
            Debug.Log("Player is not losing Gold");
        }

        float itemIncr = 0.05f;                                         //for every item prob to lose items is increased by 0.5%
        float probToLoseItems = totalNumItems * itemIncr;
        //character loses items
        if(numberItems > 0 && Random.value <= probToLoseItems)
        {
            int numItemsLost = Random.Range(1, totalNumItems / 3);
            Debug.Log("Player lost " + numItemsLost + " Items");

            //contains unique inventory items indexes and their quantities
            Dictionary<int, int> uniqueInventoryItems = new Dictionary<int, int>();
            for(int i = 0; i < inventoryCarryLoad; i++)
            {
                if (inventory.inventory[i] is Gold || inventory.inventory[i] is EmptySlot) continue;
                uniqueInventoryItems.Add(i, inventory.inventory[i].quantity);
            }

            for(int i = 0; i < numItemsLost; i++)
            {
                int itemSelected = Random.Range(0, uniqueInventoryItems.Count - 1);     //select a random inventory item to lose
                Item itemToLose = inventory.inventory[uniqueInventoryItems.ElementAt(itemSelected).Key];
                Debug.Log("Player lost a " + itemToLose.GetType());

                inventory.RemoveItem(itemToLose);

                int key = uniqueInventoryItems.ElementAt(itemSelected).Key;
                uniqueInventoryItems[key]--;
                if (uniqueInventoryItems[key] <= 0) uniqueInventoryItems.Remove(key);
            }
        }
        else
        {
            Debug.Log("Player is not losing items");
        }

    }

    //exit from the defeated combat room and dungeon, and return to town
    protected void RecoverToTown(bool inCombat, DungeonManager dungeonManager)
    {
        if (inCombat)
        {
            //leave combat Room
            CombatManager combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
            combatManager.WipeRoom();
            health = 1;                 //stabalize player
            combatManager.EndCombat();
        }

        //leave dungeon
        dungeonManager.ExitDungeon(Dungeon.Run.Fail, false);
    }
}
