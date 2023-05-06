using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Class to update the UI for layer entrances
public class LayerEnemyPool : MonoBehaviour
{
    private int currentLayerPool;                   //Current number enemies remaining in this layer
    private int layerHealthPool;                    //How many enemies can be in this layer
    private int layerThreshold;                     //min # enemies needed left in layer to proceed to next

    public Button layerEntrace;
    [SerializeField] private Slider layercurrentHealthSlider;
    [SerializeField] private Slider layerThresholdSlider;
    [SerializeField] private TMP_Text layerHealthPoolText;

    public void SetLayerHealthPool(int layerHealthPool)
    {
        this.layerHealthPool = layerHealthPool;
        layercurrentHealthSlider.maxValue = layerHealthPool;
        layerThresholdSlider.maxValue = layerHealthPool;
        UpdateLayerUI();
    }

    public void SetCurrentLayerPool(int currentLayerPool)
    {
        this.currentLayerPool = currentLayerPool;
        UpdateLayerUI();
    }

    public void SetLayerThreshold(int layerThreshold)
    {
        this.layerThreshold = layerThreshold;
        UpdateLayerUI();
    }

    //updates the slider and text for the layer
    private void UpdateLayerUI()
    {
        UpdateLayerHealthPoolText();
        UpdateLayerCurrentHealthSlider();
        UpdateLayerThresholdSlider();
    }

    private void UpdateLayerHealthPoolText()
    {
        layerHealthPoolText.text = "Remaining: " + currentLayerPool + "/" + layerHealthPool;
    }

    private void UpdateLayerCurrentHealthSlider()
    {
        layercurrentHealthSlider.value = currentLayerPool;
    }

    private void UpdateLayerThresholdSlider()
    {
        layerThresholdSlider.value = layerThreshold;
    }

}
