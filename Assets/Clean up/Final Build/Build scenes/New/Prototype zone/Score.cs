using UnityEngine;

public class Score : MonoBehaviour
{
    public string fruitTag = "Left";
    public ParticleSystem collectEffect;
    public AudioClip collectSound;  
    public AudioClip collectSound2;      

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(fruitTag))
        {
            ParticleSystem ps = Instantiate(collectEffect, other.transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration);
            Destroy(other.gameObject);
            audioSource.PlayOneShot(collectSound);    
            audioSource.PlayOneShot(collectSound2);
        }
    }
}