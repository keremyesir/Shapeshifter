using UnityEngine;

// Bu script Player objesine eklenir.
// Animator Controller'daki parametreleri (Speed, Dead) otomatik olarak gunceller.
// Animator'da su parametreler olmali:
//   - "Speed" (float): 0 = idle, > 0.01 = run
//   - "Dead" (trigger): Oyuncu oldugunde tetiklenir
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (animator == null || rb == null) return;

        // Hareket hizina gore Run/Idle gecisi
        float speed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    // PlayerHealth tarafindan olum aninda cagrilir
    public void TriggerDeath()
    {
        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }
    }
}
