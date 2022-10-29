using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class faceCam : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;
    void LateUpdate()
    {
       
        Vector3 newPos = player.position + new Vector3(0, 0.6f, 1f); 
        transform.position = newPos;
        transform.rotation = Quaternion.Euler(0, 180f, 0);       
    }

}
