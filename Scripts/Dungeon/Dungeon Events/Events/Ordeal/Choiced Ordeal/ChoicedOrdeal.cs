using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * this class takes four button as parameters to its constructor
 * as well as a list<Ordeal> for passing the functions assinged to those buttons
 */
public class ChoicedOrdeal : Ordeal
{
    private List<Ordeal> ordeals;

    public ChoicedOrdeal(List<Ordeal> ordeals, GameObject player, ScenesManager sceneManager) : base(player, sceneManager)
    {
        this.ordeals = ordeals;
    }

    public override void SetUpEvent()
    {
        SetChocedUIActive();
        UpdateEventText();
        UpdateEventButton();
    }

    protected override void UpdateEventText()
    {
        string eventText = "The number of choices to take is " + ordeals.Count;
        UIManager.Instance.textEvent.UpdateEventText(eventText);
    }

    public override void UpdateButtonText(GameObject eventButtonObject)
    {
        Debug.Log("Shouldn't be in here when updating button text");
    }

    //given the number of choices, assigns button listeners
    protected override void UpdateEventButton()
    {
        for(int i = 0; i < ordeals.Count; i++)
        {
            switch (i)
            {
                case 0:
                    UIManager.Instance.AddListener(UIManager.Instance.eventButton1, ordeals[i].UpdateEventButtonListener, true);
                    ordeals[i].UpdateButtonText(UIManager.Instance.eventButton1Object);
                    break;
                case 1:
                    UIManager.Instance.AddListener(UIManager.Instance.eventButton2, ordeals[i].UpdateEventButtonListener, true);
                    ordeals[i].UpdateButtonText(UIManager.Instance.eventButton2Object);
                    break;
                case 2:
                    UIManager.Instance.AddListener(UIManager.Instance.eventButton3, ordeals[i].UpdateEventButtonListener, true);
                    ordeals[i].UpdateButtonText(UIManager.Instance.eventButton3Object);
                    break;
                case 3:
                    UIManager.Instance.AddListener(UIManager.Instance.eventButton4, ordeals[i].UpdateEventButtonListener, true);
                    ordeals[i].UpdateButtonText(UIManager.Instance.eventButton4Object);
                    break;
                default:
                    break;
            }
        }
    }

    public override void UpdateEventButtonListener()
    {
        Debug.Log("Choiced Event button Listener was added to a button. This should not happen!");
    }

    //chooses which UI to activate depending on how many choices are to make
    private void SetChocedUIActive()
    {
        UIManager.Instance.SetDungeonInteractivePlayerUI(false);
        switch (ordeals.Count)
        {
            case 1:
                SetUIActive();
                break;
            case 2:
                SetUI2Active();
                break;
            case 3:
                SetUI3Active();
                break;
            case 4:
                SetUI4Active();
                break;
            default:
                Debug.Log("Invalid number of buttons");
                break;
        }
    }

    //Sets the UI for 2 buttons active, rearanging positions of buttons
    private void SetUI2Active()
    {
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
    }

    //Sets the UI for 3 buttons active, rearanging positions of buttons
    private void SetUI3Active()
    {
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);
        UIManager.Instance.eventButton3Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
        UIManager.Instance.eventButton3Object.GetComponent<RectTransform>().localPosition = new Vector3(0f, -20f, 0f);
    }

    //Sets the UI for 4 buttons active, rearanging positions of buttons
    private void SetUI4Active()
    {
        UIManager.Instance.eventButton1Object.SetActive(true);
        UIManager.Instance.eventButton2Object.SetActive(true);
        UIManager.Instance.eventButton3Object.SetActive(true);
        UIManager.Instance.eventButton4Object.SetActive(true);

        UIManager.Instance.eventButton1Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -20f, 0f);
        UIManager.Instance.eventButton2Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -20f, 0f);
        UIManager.Instance.eventButton3Object.GetComponent<RectTransform>().localPosition = new Vector3(-20f, -30f, 0f);
        UIManager.Instance.eventButton4Object.GetComponent<RectTransform>().localPosition = new Vector3(20f, -30f, 0f);
    }

}
