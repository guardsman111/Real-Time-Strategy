using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitGroundVehicle : UnitVehicle
{
    [ SerializeField] private Transform trackleftPos, trackRightPos, exhaustPos, travelPos;
    private ParticleEffector particleEffects;
    private Vector3 previousPos;

    public override void Initialize(UnitManager newManager = null)
    {
        base.Initialize(newManager);

        if(particleEffects == null)
        {
            particleEffects = Instantiate(manager.ParticleManagerPrefab.currentParticleSystems.gameObject, transform).GetComponent<ParticleEffector>();
        }

        previousPos = transform.position;

        StartCoroutine(SetupParticles());
    }

    private IEnumerator SetupParticles()
    {
        yield return new WaitForEndOfFrame();

        particleEffects.Initialize(this, trackleftPos, trackRightPos, exhaustPos, travelPos);
        particleEffects.ToggleExhaust(true);

        if(targetLocation != transform.position)
        {
            particleEffects.ToggleTracks(true, GetSpeed());
        }
    }

    public float GetSpeed()
    {
        float distanceMoved = Vector3.Distance(transform.position, previousPos);
        previousPos = transform.position;
        return distanceMoved;
    }
}
