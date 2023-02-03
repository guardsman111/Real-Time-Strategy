using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostUnit : MonoBehaviour
{
    [SerializeField] private GameObject unit;
    public GameObject UnitPrefab { get => unit; }
}
