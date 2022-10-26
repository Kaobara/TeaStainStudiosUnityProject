using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [SerializeField] public Rigidbody rigidBody {get; private set; }
    [SerializeField] public Vector3 localPos {get; private set; }
    [SerializeField] private SpringJoint spring;

    // Start is called before the first frame update
    private void Start() 
    {
        rigidBody = GetComponent<Rigidbody>();

        // connect spring to dummy rigidbody when not held
        spring = GetComponent<SpringJoint>();
        spring.connectedBody = transform.Find("SpringOff").GetComponent<Rigidbody>();

        localPos = new Vector3(0f, 1.5f, 2f);
    }

    public void Attach(GameObject newParent, Vector3 localPos)
    {
        // disable gravity for object while held
        rigidBody.useGravity = false;

        // store desired local pos relative to player (rigidbody)
        this.localPos = localPos;

        transform.position = newParent.transform.TransformPoint(localPos);

        // connect spring joint to player rigidbody and set spring constants
        spring.connectedBody = newParent.GetComponent<Rigidbody>();
        spring.anchor = Vector3.zero;
        spring.connectedAnchor = localPos; 
        spring.spring = 100f;
        spring.damper = 5f;
    }

    public void Eject(Vector3 playerOrientation, float force)
    {
        Detach();

        // add impulse force to throw object
        rigidBody.AddForce(Vector3.RotateTowards(playerOrientation, Vector3.up, Mathf.PI/4, 10000) * force, ForceMode.Impulse);
    }

    public void Detach()
    {
        rigidBody.useGravity = true;

        // set spring constants to 0 and connect spring to dummy child rigidbody
        spring.connectedBody = transform.Find("SpringOff").GetComponent<Rigidbody>();
        spring.spring = 0f;
        spring.damper = 0f;
    }
}
