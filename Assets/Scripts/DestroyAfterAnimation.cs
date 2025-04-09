using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("DestroyAfterAnimation: Animator not found on " + gameObject.name);
            Destroy(gameObject); // Уничтожаем сразу, если нет аниматора
        }
    }

    void Update()
    {
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !anim.IsInTransition(0))
        {
            Destroy(gameObject);
        }
    }
}
