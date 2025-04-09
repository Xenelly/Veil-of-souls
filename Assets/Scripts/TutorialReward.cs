using UnityEngine;

public class ExtraJumpOnCollision : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;  // Ссылка на скрипт игрока

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, чтобы playerController был правильно назначен
        if (playerController != null)
        {
            Debug.Log("Collision detected with " + collision.gameObject.name);

            // При столкновении с объектом увеличиваем maxAirJumps до 1
            playerController.maxAirJumps = 1;

            // Печатаем новое значение для проверки
            Debug.Log("airJumpCounter set to: " + playerController.maxAirJumps);
        }
        else
        {
            Debug.LogError("PlayerController is not assigned in the inspector!");
        }
    }
}
