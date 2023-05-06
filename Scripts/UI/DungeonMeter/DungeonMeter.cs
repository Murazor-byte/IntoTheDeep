using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMeter : MonoBehaviour
{
    public Slider dungeonMeter;
    public int maxDungeonMeterValue;
    public int currentDungeonMeterValue;

    public void CreateDungeonMeter(int maxDungeonMeterValue)
    {
        int initialMeterValue = (int)(0.85f * maxDungeonMeterValue);

        this.maxDungeonMeterValue = maxDungeonMeterValue;
        currentDungeonMeterValue = initialMeterValue;

        dungeonMeter.maxValue = maxDungeonMeterValue;
        dungeonMeter.value = currentDungeonMeterValue;
    }

    //updating DungeonMeter based on run, with what layer # the player was on
    public void UpdateDungeonMeter(Dungeon.Run run, Player player, Dungeon dungeon)
    {
        switch (run)
        {
            case Dungeon.Run.Successful:
                if (currentDungeonMeterValue - player.enemiesKilled < 0) currentDungeonMeterValue = 0;
                else currentDungeonMeterValue -= player.enemiesKilled;
                break;
            case Dungeon.Run.BossKilled:    //move dungeon meter toward Dungeon based on the cleared layers threshold
                int dungeonMeterShift = dungeon.GetLayer(player.layerNumber - 1).layerThreshold + player.enemiesKilled;
                if (currentDungeonMeterValue - dungeonMeterShift < 0) currentDungeonMeterValue = 0;
                else currentDungeonMeterValue -= dungeonMeterShift;
                break;
            case Dungeon.Run.Fail:          //move dungeon meter toward Hero (for fixed amount now)
                if (currentDungeonMeterValue + 25 > maxDungeonMeterValue) currentDungeonMeterValue = maxDungeonMeterValue;
                else currentDungeonMeterValue += 25;
                break;
            case Dungeon.Run.NoRun:
                break;
        }

        dungeonMeter.value = currentDungeonMeterValue;
        CheckEndCondition();
    }



    //if meter is fully on either side
    private void CheckEndCondition()
    {
        if(currentDungeonMeterValue == 0)
        {
            Debug.Log("The Dungeon has been cleared and the Player wins!");
            GameObject.Find("Game Manager").GetComponent<GameManager>().DungeonMeterEndGame(true);
        }
        if(currentDungeonMeterValue == maxDungeonMeterValue)
        {
            Debug.Log("Dungeon has been overrun. Player lost!");
            GameObject.Find("Game Manager").GetComponent<GameManager>().DungeonMeterEndGame(false);
        }
    }
}
