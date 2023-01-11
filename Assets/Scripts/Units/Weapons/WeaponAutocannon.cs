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
            if (isLoading == false)
            {
                StartCoroutine(Loaded(60f / (float)Stats.roundsPerMinute));
                magazineFill -= 1;
                return;
            }
        }

        if (ammoReadyRack > 0)
        {
            ammoReadyRack -= 1;
            StartCoroutine(MagazineLoad(60f / magazineReloadTime));
            return;
        }

        if (ammoHalfReadyRack > 0)
        {
            ammoHalfReadyRack -= 1;
            StartCoroutine(MagazineLoad(60f / (magazineReloadTime / 2)));
            return;
        }

        if (ammoCount > 0)
        {
            if (Stats.readyRack == 0)
            {
                StartCoroutine(MagazineLoad(60f / magazineReloadTime));
                ammoCount -= 1;
                return;
            }

            StartCoroutine(MagazineLoad(60f / (magazineReloadTime / 4)));
            ammoCount -= 1;
        }

        Debug.Log("Out of ammo so cannot load");
    }

    private IEnumerator MagazineLoad(float delay)
    {
        yield return new WaitForSeconds(delay);

        magazineFill = magazineSize;
        Load();
    }
}
