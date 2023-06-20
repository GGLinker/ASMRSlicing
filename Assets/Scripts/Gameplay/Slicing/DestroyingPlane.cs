using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyingPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(collision.gameObject);
    }
}
