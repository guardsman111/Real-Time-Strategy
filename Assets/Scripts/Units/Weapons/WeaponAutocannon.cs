using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAutocannon : UnitWeapon
{
    [SerializeField] private int magazineSize;
    [SerializeField] private int magazineFill;
    [SerializeField] private float magazineReloadTime;

    protected override void Start()
    {
        base.Start();
        magazineFill = magazineSize;
    }

    protected override void Load()
    {
        if (magazineFill > 0)
        {
            isLoading = true;
            Invoke("Loaded", 60f / (float)Stats.roundsPerMinute);
            magazineFill -= 1;
            return;
        }

        if (ammoReadyRack > 0)
        {
            ammoReadyRack -= 1;
            Invoke("MagazineLoad", magazineReloadTime);
            return;
        }

        if (ammoHalfReadyRack > 0)
        {
            ammoHalfReadyRack -= 1;
            Invoke("MagazineLoad", magazineReloadTime / 2);
            return;
        }

        if (ammoCount > 0)
        {
            if (Stats.readyRack == 0)
            {
                Invoke("MagazineLoad", magazineReloadTime);
                ammoCount -= 1;
                return;
            }

            Invoke("MagazineLoad", magazineReloadTime / 4);
            ammoCount -= 1;
        }

        Debug.Log("Out of ammo so cannot load");
    }

    private void MagazineLoad()
    {
        magazineFill = magazineSize;
        Load();
    }
}
