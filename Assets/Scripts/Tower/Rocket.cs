using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Rocket : MonoBehaviour
{
    private float damage;
    public float explosionRadius = 3f;
    private Vector2 direction;
    private Vector2 targetPosition;
    private Rigidbody2D rb;
    private bool hasExploded = false;
    
    public float speed = 8f; // Slower than laser
    public GameObject explosionEffectPrefab; // Optional: assign a particle effect
    public float explosionEffectDuration = 0.25f; // seconds to keep the visual effect alive

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            // ensure collider interactions work; velocity will be used for movement
        }
    }

    public void Initialize(float damage, float explosionRadius, Vector2 targetPosition)
    {
        this.damage = damage;
        this.explosionRadius = explosionRadius;
        this.targetPosition = targetPosition;

        // compute direction towards target and set velocity
        this.direction = (targetPosition - (Vector2)transform.position).normalized;
        if (rb != null)
        {
            rb.velocity = this.direction * speed;
        }
    }

    void Update()
    {
        // Rotate rocket to face direction of travel (use current velocity if available)
        Vector2 vel = (rb != null) ? rb.velocity : direction * speed;
        if (vel.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Move toward the fixed target position and explode exactly when we arrive.
        if (!hasExploded)
        {
            float step = speed * Time.deltaTime;
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (distance <= step)
            {
                // Snap to exact target position and explode. This prevents overshooting and guarantees no misses.
                if (rb != null)
                {
                    rb.MovePosition(targetPosition);
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    transform.position = targetPosition;
                }
                Explode();
            }
            else
            {
                Vector2 move = direction * step;
                if (rb != null)
                {
                    rb.MovePosition((Vector2)transform.position + move);
                }
                else
                {
                    transform.position += (Vector3)move;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;
        // Debug: log collision target
        Debug.Log($"[Rocket] OnTriggerEnter2D with {other.name}");
        var enemyComponent = other.GetComponent<Enemy>() ?? other.GetComponentInParent<Enemy>();
        if (enemyComponent != null)
        {
            Explode();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;
        // Debug: log collision target
        Debug.Log($"[Rocket] OnCollisionEnter2D with {collision.collider.name}");
        var enemyComp = collision.collider.GetComponent<Enemy>() ?? collision.collider.GetComponentInParent<Enemy>();
        if (enemyComp != null)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        // stop movement
        if (rb != null) rb.velocity = Vector2.zero;
        // Find all enemies in explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // No damage falloff: apply full damage to all enemies in radius
                int finalDamage = Mathf.RoundToInt(damage);
                enemy.TakeDamage(finalDamage);
            }
        }

        // Spawn explosion effect if assigned (auto-destroy after a short duration)
        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            if (explosionEffectDuration > 0f)
            {
                Destroy(fx, explosionEffectDuration);
            }
        }

        // Optional: Add screen shake or other effects here
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Debug visualization of explosion radius (editor only)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}