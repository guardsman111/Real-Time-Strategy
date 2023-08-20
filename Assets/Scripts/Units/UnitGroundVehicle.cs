using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitGroundVehicle : UnitVehicle
{
    private ParticleManager particleManager;

    public override void Initialize(UnitManager newManager = null)
    {
        base.Initialize(newManager);
        if(particleManager == null)
        {
            particleManager = Instantiate(manager.ParticleManagerPrefab, transform).GetComponent<ParticleManager>();
        }
    }
}
