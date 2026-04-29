using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSwitchScene : MonoBehaviour
{
    public string sceneName;

    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
