using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Image mainBar;
    [SerializeField] private Image delayedBar;
    [SerializeField] private float smoothSpeed = 5f;

    private float _targetFill = 1f;

    public void UpdateHealth(float current, float max)
    {
        _targetFill = Mathf.Clamp01(current / max);
        if (mainBar != null) mainBar.fillAmount = _targetFill;
    }

    void Update()
    {
        if (delayedBar != null && Mathf.Abs(delayedBar.fillAmount - _targetFill) > 0.001f)
        {
            delayedBar.fillAmount = Mathf.Lerp(delayedBar.fillAmount, _targetFill, Time.deltaTime * smoothSpeed);
        }
    }
}