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

        Invoke("MagazineLoad", magazineReloadTime);
    }

    private void MagazineLoad()
    {
        magazineFill = magazineSize;
        Load();
        ammoCount -= 1;
    }
}
