using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitStats
{
    // Dynamic Variables - Change during the game
    public string leaderName;
    public float currentHealth;

    //Static Variables - Don't change during the game
    public string unitName;
    public string unitDescription;
    public int opticsRange;
    public int driveSpeed;
    public int driveAcceleration;
    public int traverseSpeed;
    public int frontArmour;
    public int sideArmour;
    public int rearArmour;
    public int topArmour;
    public int turretArmour;
    public int toughness;
    public float health;
}
