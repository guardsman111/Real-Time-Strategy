using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particleSystems;
    private int intValue;

    private void Start()
    {
        if (particleSystems.Count < intValue)
        {
            particleSystems[intValue].Play();
        }
        else
        {
            Debug.LogError("ParticleManager: intValue is out of range of particleSystems list");
        }
    }

    public void SetIntValue(int newValue)
    {
        intValue = newValue;
    }
}
