using UnityEngine;
using System.Collections;

public class Destroy : MonoBehaviour
{
    public string destroyTag = "Yellow";

    public ParticleSystem destroyEffect;
    public ParticleSystem destroyEffectOther;

    public AudioClip destroySound;
    public AudioClip destroySoundOther;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(destroyTag))
        {
            StartCoroutine(DestroyOtherObject(other));
        }
        else if (!other.CompareTag("Untagged"))
        {
            StartCoroutine(DestroyObject());
        }
    }

    private IEnumerator DestroyObject()
    {
        yield return new WaitForEndOfFrame();

        ParticleSystem ps = Instantiate(destroyEffect, transform.position, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration);

        PlaySound(destroySound, transform.position);
        Destroy(gameObject);
    }

    private IEnumerator DestroyOtherObject(Collider other)
{
    Vector3 otherPosition = other.transform.position;
    Debug.Log("DestroyOtherObject called, position: " + otherPosition);
    Debug.Log("destroyEffectOther assigned: " + (destroyEffectOther != null ? destroyEffectOther.name : "NULL"));
    Debug.Log("destroySoundOther assigned: " + (destroySoundOther != null ? destroySoundOther.name : "NULL"));

    yield return new WaitForEndOfFrame();

    Debug.Log("After yield, instantiating particle at: " + otherPosition);
    ParticleSystem ps = Instantiate(destroyEffectOther, otherPosition, Quaternion.identity);
    Debug.Log("Particle instantiated: " + (ps != null ? ps.name : "NULL"));
    ps.Play();
    Debug.Log("Particle duration: " + ps.main.duration);
    Destroy(ps.gameObject, ps.main.duration);

    PlaySound(destroySoundOther, otherPosition);
    Destroy(other.gameObject);
    Destroy(gameObject);
}

    private static void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject temp = new GameObject("TempAudio");
        temp.transform.position = position;
        AudioSource source = temp.AddComponent<AudioSource>();
        source.spatialBlend = 0f;
        source.PlayOneShot(clip);
        Destroy(temp, clip.length);
    }
}