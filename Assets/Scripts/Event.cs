using UnityEngine;

public class ActivateObjectOnCollision : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;  // Объект, который будет активирован

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, если столкновение с объектом с тегом "Player"
        if (other.CompareTag("Player"))
        {
            ActivateObject();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Если используете не trigger, можете использовать этот метод
        if (collision.gameObject.CompareTag("Player"))
        {
            ActivateObject();
        }
    }

    private void ActivateObject()
    {
        if (objectToActivate != null)
        {
            // Включаем объект
            objectToActivate.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Object to activate is not assigned in the inspector.");
        }
    }
}
