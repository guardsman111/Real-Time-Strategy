using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCollider : MonoBehaviour
{
    [SerializeField] private UnitObject unit;
    public UnitObject Unit { get => unit; }

    private void Start()
    {
        if (unit.IsPlayer)
        {
            gameObject.layer = 6;

            return;
        }

        gameObject.layer = 7;
    }
}
