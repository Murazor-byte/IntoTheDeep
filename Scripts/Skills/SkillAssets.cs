using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAssets : MonoBehaviour
{
    public static SkillAssets Instance { get; private set; }

    public void CreateInstance()
    {
        Instance = this;
    }

    public Texture emptySlot;
    public Texture movement;
    public Texture strike;
    public Texture charge;
    public Texture shoot;
    public Texture dodge;
    public Texture cleave;
}
