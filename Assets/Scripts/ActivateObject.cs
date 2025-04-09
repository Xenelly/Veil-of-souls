using UnityEngine;

public class ActivateOnStart : MonoBehaviour
{
    public GameObject[] objectsToActivate;

    void Start()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}
