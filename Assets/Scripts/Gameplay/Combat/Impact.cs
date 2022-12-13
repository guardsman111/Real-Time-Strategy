using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impact : MonoBehaviour
{
    [SerializeField] private float duration;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Destroy", duration);
    }

    private void Destroy()
    {
        Destroy(this.gameObject);
    }

}
