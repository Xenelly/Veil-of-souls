using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;

    [SerializeField] protected float damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;

    protected enum EnemyStates
    {
        FlyingIdle,
        FLyingChase,
        FLyingDeath,
    }

    public virtual void Start()
    {

    }
    
    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
        sr = GetComponent<SpriteRenderer>();
    }

    public virtual void Update()
    {
        if (health < 0)
        {
            Destroy(gameObject);
        }
        if (isRecoiling)
        {
            if(recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(hitForce * recoilFactor * -hitDirection);
            isRecoiling = true;
        }
    }

    protected void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
            PlayerController.Instance.HitTimeStop(0, 5, 0.5f);
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }

    protected virtual void UpdateEnemyStates()
    {
        // Заглушка, переопределяется в подклассах
    }

}
