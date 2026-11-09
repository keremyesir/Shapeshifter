using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetPlayer; // Takip edilecek karakter

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Kameranýn Z ekseninde geride durmasý için

    [Header("Map Boundaries (Duvarlar)")]
    [SerializeField] private Transform leftBorder;    // Sol duvar
    [SerializeField] private Transform rightBorder;   // Sað duvar
    [SerializeField] private Transform topBorder;     // Üst duvar
    [SerializeField] private Transform bottomBorder;  // Alt duvar

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (targetPlayer != null && cam != null)
        {
            // Kameranýn gitmek istediði varsayýlan hedef pozisyon
            Vector3 targetPosition = targetPlayer.position + offset;

            // Kameranýn görüþ alanýnýn yarým yüksekliðini ve yarým geniþliðini alýyoruz
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float clampedX = targetPosition.x;
            float clampedY = targetPosition.y;

            // X ekseni sýnýrlandýrmasý (Sol ve Sað Duvarlar)
            if (leftBorder != null && rightBorder != null)
            {
                float minX = leftBorder.position.x + camWidth;
                float maxX = rightBorder.position.x - camWidth;

                // Harita boyutu kameranýn görüþ alanýndan küçükse kamerayý ortala, deðilse sýnýrla
                if (minX > maxX)
                    clampedX = (leftBorder.position.x + rightBorder.position.x) / 2f;
                else
                    clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
            }

            // Y ekseni sýnýrlandýrmasý (Alt ve Üst Duvarlar)
            if (bottomBorder != null && topBorder != null)
            {
                float minY = bottomBorder.position.y + camHeight;
                float maxY = topBorder.position.y - camHeight;

                if (minY > maxY)
                    clampedY = (bottomBorder.position.y + topBorder.position.y) / 2f;
                else
                    clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);
            }

            // Sýnýrlandýrýlmýþ yeni pozisyon
            Vector3 finalPosition = new Vector3(clampedX, clampedY, targetPosition.z);

            // Kamerayý yumuþak bir þekilde sýnýrlandýrýlmýþ pozisyona kaydýrýyoruz
            transform.position = Vector3.Lerp(transform.position, finalPosition, followSpeed * Time.deltaTime);
        }
    }
}