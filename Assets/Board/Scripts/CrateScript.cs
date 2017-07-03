using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateScript : MonoBehaviour
{

    private static int strength = 15;
    public ParticleSystem crateParticles;
    public ParticleSystem player1Particles;
    public ParticleSystem player2Particles;
    public AudioSource crateAudio;
    public Material crateMaterial;

    public void spawnAnimation(bool player1)
    {
        if (player1) player1Particles.Emit(10);
        else player2Particles.Emit(10);
    }

    public void detonate(GameObject crateObject)
    {
        //ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
        //settings.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 0, 1));
        crateParticles.Emit(10);
        //crateAudio.Stop();
        crateAudio.Play();
        GetComponent<Renderer>().enabled = false;

        //crateObject.GetComponent<Renderer>().enabled = false;
        //crateObject.SetActive(false);
        Destroy(crateObject, 1);
    }



    // Update is called once per frame
    //void Update ()
    //{
    //}


    public static int getStrength()
    {
        return strength;
    }
}
