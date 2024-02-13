using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneVisualiser : MonoBehaviour
{
    private void OnParticleTrigger()
    {
        Debug.Log("Trigger");
    }
    private void OnParticleCollision(GameObject other)
    {
        Debug.Log(other.name);
    }
}
