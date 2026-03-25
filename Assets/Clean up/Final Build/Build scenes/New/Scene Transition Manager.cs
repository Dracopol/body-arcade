using System.Collections;
using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;

    public void GotoScene(int sceneIndex)
    {
        StartCoroutine(Transition(sceneIndex));
    }

    IEnumerator Transition(int sceneIndex)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}
