using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class Menu: MonoBehaviour
    {
        public void StartNewGame()
        {
            SceneManager.LoadScene(1);
        }
        
        public void ContinueGame()
        {
            SceneManager.LoadScene(1);
        }

        public void Quit()
        {
            Application.Quit();
        }
        
    }
}