using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public static class ArmourPen
{
    public static bool CheckPenetration(int armourThickness, float penetration, float angle)
    {
        float hitAngle = angle - 90;

        if(hitAngle < 0)
        {
            hitAngle = -hitAngle;
        }

        float thicknessIncrease = (1 - hitAngle / 90);

        float relativeThickness = armourThickness + (armourThickness * (thicknessIncrease * 1.5f));

        //Debug.Log($"{armourThickness} armour thickness, {relativeThickness} relative Thickness from angle {angle} and hitAngle {hitAngle} and multiplier {thicknessIncrease} against {penetration} penetration");
        if (relativeThickness < penetration)
        {
            return true;
        }

        return false;
    }


    public static bool RegisterHit(UnitStats target, AmmoPiece ammo, RaycastHit hitInfo)
    {
        float hitAngle = Vector3.Angle(hitInfo.normal, ammo.transform.forward);
        //float hitAngle = Vector3.Dot(ammo.transform.forward, hitInfo.normal);

        switch (hitInfo.collider.name)
        {
            case "Front":
                if (CheckPenetration(target.frontArmour, ammo.Penetration, hitAngle) == true)
                {
                    Debug.Log("Took penetrating hit on front armour.");
                    return true;
                }
                Debug.Log("Took hit on front armour.");
                return false;
            case "Side":
                if (CheckPenetration(target.sideArmour, ammo.Penetration, hitAngle) == true)
                {
                    Debug.Log("Took penetrating hit on side armour.");
                    return true;
                }
                Debug.Log("Took hit on side armour.");
                return false;
            case "Back":
                if (CheckPenetration(target.rearArmour, ammo.Penetration, hitAngle) == true)
                {
                    Debug.Log("Took penetrating hit on rear armour.");
                    return true;
                }
                Debug.Log("Took hit on rear armour.");
                return false;
            case "Turret":
                if (CheckPenetration(target.turretArmour, ammo.Penetration, hitAngle) == true)
                {
                    Debug.Log("Took penetrating hit on turret armour.");
                    return true;
                }
                Debug.Log("Took hit on turret armour.");
                return false;
            case "Top":
                if (CheckPenetration(target.topArmour, ammo.Penetration, hitAngle) == true)
                {
                    Debug.Log("Took penetrating hit on top armour.");
                    return true;
                }
                Debug.Log("Took hit on top armour.");
                return false;
            default:
                return false;

        }
    }
}
