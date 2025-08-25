using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform startPoint;
    private Transform targetPoint;
    private float duration = 0.1f;
    private float timer;
    private float damage;
    private Enemy targetEnemy;

    // Initialize with tower damage
    public void Initialize(Transform start, Transform target, Transform parent, float towerDamage)
    {
        startPoint = start;
        targetPoint = target;
        damage = towerDamage;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        if (parent != null)
            transform.SetParent(parent);

        timer = duration;

        // Keep reference to Enemy
        if (targetPoint != null)
        {
            targetEnemy = targetPoint.GetComponent<Enemy>();
        }
    }

    void Update()
    {
        if (startPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 endPos = targetPoint != null ? targetPoint.position : targetEnemy != null ? targetEnemy.transform.position : startPoint.position;
        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPos);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (targetEnemy != null)
            {
                // Just deal normal damage
                targetEnemy.TakeDamage((int)damage);

                if (targetEnemy.isPendingDeath)
                {
                    Destroy(targetEnemy.gameObject);
                }
            }
            Destroy(gameObject); // destroy laser
        }

    }

}
