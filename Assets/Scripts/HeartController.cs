using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    private GameObject[] heartContainers;
    private Image[] heartFills;
    private GameObject[] heartExtras; // Добавляем массив для Extra

    public Transform heartsParent;
    public GameObject heartContainerPrefab;

    void Start()
    {
        int maxHealth = PlayerController.Instance.maxHealth;
        heartContainers = new GameObject[maxHealth];
        heartFills = new Image[maxHealth];
        heartExtras = new GameObject[maxHealth]; // Инициализируем массив

        PlayerController.Instance.onHealthChangedCallback += UpdateHeartsHUD;
        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }

    void InstantiateHeartContainers()
    {
        for (int i = 0; i < PlayerController.Instance.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab, heartsParent);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();

            // Ищем Extra внутри контейнера
            Transform extraTransform = temp.transform.Find("extra");
            if (extraTransform != null)
            {
                heartExtras[i] = extraTransform.gameObject;
            }
        }
    }

    void UpdateHeartsHUD()
    {
        int health = PlayerController.Instance.Health;

        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < health)
            {
                heartFills[i].fillAmount = 1; // Полное сердце
                if (heartExtras[i] != null) heartExtras[i].SetActive(true); // Extra включен
            }
            else
            {
                heartFills[i].fillAmount = 0; // Пустое сердце
                if (heartExtras[i] != null) heartExtras[i].SetActive(false); // Отключаем Extra
            }
        }
    }
}
