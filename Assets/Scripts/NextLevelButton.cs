using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelButton : MonoBehaviour
{
    public void Next()
    {
        int max = SceneManager.sceneCountInBuildSettings;
        int current = SceneManager.GetActiveScene().buildIndex;
        int target = current + 1;

        if(target >= max)
        {
            SceneManager.LoadScene(0);
        }

        else
        {
            SceneManager.LoadScene(target);
        }
    }
}
