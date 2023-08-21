using System;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

[Serializable]
public class ParticleTypePair
{
    public ParticleType type;
    public ParticleEffector particleEffector;
}

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private List<ParticleTypePair> particleSystems = new();
    private ParticleType terrainType;

    public ParticleEffector currentParticleSystems { get; private set; }

    public void Initialize(ParticleType newType)
    {
        SetTypeValue(newType);
    }

    private void SetTypeValue(ParticleType newType)
    {
        terrainType = newType;
        
        foreach(ParticleTypePair pair in particleSystems)
        {
            if(pair.type == terrainType)
            {
                currentParticleSystems = pair.particleEffector;
                return;
            }
        } 

        Debug.LogError("ParticleManager: terrainType does not exist in dictionary");
    }
}
