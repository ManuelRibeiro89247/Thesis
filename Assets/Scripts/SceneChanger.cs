using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{

    public int sceneNumber;

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneNumber);
    }
     public void Return()
     {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
     } 

    public void ChangeSceneISDA()
    {
        SceneManager.LoadScene(sceneNumber);
    }
    public void ChangeSceneICDA()
    {
        SceneManager.LoadScene(sceneNumber);
    }

}
