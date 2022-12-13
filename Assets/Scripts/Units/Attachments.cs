using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attachments : MonoBehaviour
{
    [SerializeField] private int appearanceChance;

    // Start is called before the first frame update
    void Start()
    {
        if(Random.Range(0, 100) >= appearanceChance)
        {
            gameObject.SetActive(false);
        }
    }

}
