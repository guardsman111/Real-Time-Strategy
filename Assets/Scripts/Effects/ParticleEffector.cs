using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffector : MonoBehaviour
{
    private UnitGroundVehicle unit;
    private ParticleSystem.EmissionModule trackLeftEmission, trackRightEmission, travelEmission;
    private Coroutine updateSpeedsCoroutine;

    [SerializeField] private ParticleSystem trackLeft;
    [SerializeField] private ParticleSystem trackRight;
    [SerializeField] private ParticleSystem travel;
    [SerializeField] private ParticleSystem exhaust;

    public bool moving { get; private set; } = false;

    public void Initialize(UnitGroundVehicle newUnit, Transform trackLeftPos, Transform trackRightPos, Transform exhaustPos, Transform travelPos)
    {
        unit = newUnit;

        trackLeftEmission = trackLeft.emission;
        trackRightEmission = trackRight.emission;
        travelEmission = travel.emission;

        trackLeft.transform.position = trackLeftPos.position;
        trackRight.transform.position = trackRightPos.position;
        exhaust.transform.position = exhaustPos.position;
        travel.transform.position = travelPos.position;
    }

    // Toggle tracks particle system play or stop with bool parameter
    public void ToggleTracks(bool toggle, float speed)
    {
        if (toggle == true)
        {
            trackLeft.Play();
            trackRight.Play();
            travel.Play();

            if(updateSpeedsCoroutine == null)
            {
                updateSpeedsCoroutine = StartCoroutine(UpdateSpeedsCoroutine());
            }

            return;
        }

        if(updateSpeedsCoroutine != null)
        {
            StopCoroutine(updateSpeedsCoroutine);
        }

        trackLeft.Stop();  
        trackRight.Stop();
        travel.Stop();
    }
    
    // toggle exhaust particle system play or stop with bool parameter
    public void ToggleExhaust(bool toggle)
    {
        if (toggle == true)
        {
            exhaust.Play();
            return;
        }

        exhaust.Stop();
    }

    private IEnumerator UpdateSpeedsCoroutine()
    {
        while (true)
        {
            float speed = unit.GetSpeed();

            if(speed >= 25)
            {
                speed = 25;
            }
            else if(speed <= 0)
            {
                ToggleTracks(false, 0);
            }

            trackLeftEmission.rateOverTime = speed;
            trackRightEmission.rateOverTime = speed;
            travelEmission.rateOverTime = speed * 2;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
