using System.Collections;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public bool fadeInOnStart = true;
    public float fadeDuration = 2f;

    public Color fadeColor = Color.black;

    private Renderer screenRenderer;
    private MeshRenderer meshRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenRenderer = GetComponent<Renderer>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (fadeInOnStart)
        {
            FadeIn();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            FadeIn();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            FadeOut();
        }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeCoroutine(1f, 0f, true));
        
    }

    public void FadeOut()
    {
        meshRenderer.enabled = true;
        StartCoroutine(FadeCoroutine(0f, 1f, false));
    }   

    public void Fade(float alphaIn, float alphaOut, bool disableMeshRenderer)
    {
        StartCoroutine(FadeCoroutine(alphaIn, alphaOut, disableMeshRenderer));
    }

    public IEnumerator FadeCoroutine(float alphaIn, float alphaOut, bool disableMeshRenderer)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, elapsed / fadeDuration);

            screenRenderer.material.color = newColor;

            elapsed += Time.deltaTime;
            yield return null;
        }
        Color finalcolor = fadeColor;
            finalcolor.a = Mathf.Lerp(alphaIn, alphaOut, elapsed / fadeDuration);

            screenRenderer.material.color = finalcolor;
        if (disableMeshRenderer)
        {
            meshRenderer.enabled = false;
        }
    }
}
