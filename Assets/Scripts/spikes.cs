using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnCollision : MonoBehaviour
{
    // Метод, который срабатывает при столкновении с коллайдером
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Здесь вы можете проверить тег объекта, с которым происходит столкновение, если нужно
        // Например, если вы хотите, чтобы перезагрузка происходила только при столкновении с игроком
        if (other.CompareTag("Player"))
        {
            // Перезагружаем сцену
            RestartScene();
        }
    }

    // Метод для перезагрузки текущей сцены
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
