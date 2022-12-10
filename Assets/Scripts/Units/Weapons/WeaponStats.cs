using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponStats
{
    public int range;
    public int rotationSpeed;
    public int ammo;
    public int ammoSpeed;
    public int roundsPerMinute;
    public int aimTime;
    public int maxGunDown;
    public int maxGunUp;
    public float instability; 
    public bool isTurreted; // Remove and replace with a turretedGun class of weapon
    public bool isFixed; //Reserved for later use - Remove and replace with a static class of weapon
    public bool removable;
}
