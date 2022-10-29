using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudScript : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(transform.localPosition.x > 666) {
            transform.localPosition = new Vector3(-666f, transform.localPosition.y, transform.localPosition.z);
        }
        else {
            
        transform.Translate (0.08f, 0f, 0f);  
        }
        
    }
}
