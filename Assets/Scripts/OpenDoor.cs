using UnityEngine;

public class DeleteOnObjectsDestroyed : MonoBehaviour
{
    [SerializeField] private GameObject object1;  // Первый объект
    [SerializeField] private GameObject object2;  // Второй объект
    [SerializeField] private GameObject objectToDelete;  // Объект, который будет удален

    void Update()
    {
        // Если оба объекта уничтожены, то удаляем третий объект
        if (object1 == null && object2 == null)
        {
            Destroy(objectToDelete);
            // Останавливаем выполнение дальнейших проверок
            enabled = false;
        }
    }
}
