using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetPlayer; // Takip edilecek karakter

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Kameranýn Z ekseninde geride durmasý için

    private void LateUpdate()
    {
        if (targetPlayer != null)
        {
            // Kameranýn gitmesi gereken hedef pozisyon (Karakterin X ve Y'si)
            Vector3 targetPosition = targetPlayer.position + offset;

            // Kamera pozisyonunu yumuþak bir þekilde hedefe doðru kaydýrýyoruz
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}