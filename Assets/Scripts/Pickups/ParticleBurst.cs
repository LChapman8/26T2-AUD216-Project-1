using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBurst : MonoBehaviour
{
    public ParticleSystem collisionParticleSystem;
    public MeshFilter mf;
    public bool once = true;

    private void OnTriggerEnter(Collider other) { 
        if (other.gameObject.CompareTag ("Player") && once) {

            var em = collisionParticleSystem.emission;
            var dur = collisionParticleSystem.main.duration;
            em.enabled = true;
            collisionParticleSystem.Play();
            once = false;
            Destroy(mf);
            Invoke(nameof(DestroyObj), dur);
        }
    }
    void DestroyObj() {
        Destroy(gameObject);
    }
}
