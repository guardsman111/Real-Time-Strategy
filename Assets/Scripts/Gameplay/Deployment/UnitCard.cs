using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCard : MonoBehaviour
{
    [SerializeField] private GameObject ghost;
    [SerializeField] private GameObject unit;

    [SerializeField] private BattleModeView view;

    public void CardPressed()
    {
        view.SpawnGhost(ghost);
    }
}
