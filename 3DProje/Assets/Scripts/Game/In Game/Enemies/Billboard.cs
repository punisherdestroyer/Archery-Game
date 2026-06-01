using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        // Sahnedeki ana kameranın transform referansı güvenli bir şekilde önbelleğe alınır.
        if (Camera.main != null) camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        /* Kamera referansı mevcutsa, nesnenin rotasyonu kameranın rotasyonu ile eşitlenir,
        böylece nesne kamera nereye dönerse dönsün her zaman kameraya düz bir şekilde bakar. */
        if (camTransform != null)
        {
            transform.rotation = camTransform.rotation;
        }
    }
}