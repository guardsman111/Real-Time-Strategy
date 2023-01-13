using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMissileIlluminator : UnitWeapon
{
    public new void Update()
    {
        base.Update();

        if(firedAmmo != null)
        {
            CheckStillTargetting();
        }
    }

    private void CheckStillTargetting()
    {
        if(targetUnit == null)
        {
            firedAmmo.SetWillHit(false);
        }

        if (targetUnit != firedAmmo.GetTarget())
        {
            firedAmmo.SetTarget(targetUnit);
        }
    }

    public override void Die()
    {
        if (firedAmmo != null)
        {
            firedAmmo.SetWillHit(false);
        }

        base.Die();
    }
}
