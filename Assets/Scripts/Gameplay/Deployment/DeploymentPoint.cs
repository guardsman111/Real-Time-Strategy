using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentPoint : MonoBehaviour
{
    private bool active = false;

    public LayerMask mask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(active)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10000, mask))
            {
                this.transform.position = hit.point;
            }
        }
    }

    public void ToggleActive(bool state)
    {
        active = state;
    }
}
