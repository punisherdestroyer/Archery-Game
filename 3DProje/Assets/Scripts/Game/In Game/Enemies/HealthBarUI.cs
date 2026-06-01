using FairyGUI;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class HealthBarUI
{
    private GProgressBar _bar;
    private Transform _target;
    private Camera _mainCam;
    private float _offsetY;
    private GComponent _mainView;

    private const float BAR_WIDTH  = 150f;
    private const float BAR_HEIGHT = 20f;

    public HealthBarUI(string packageName, string componentName, Transform target, GComponent mainView, float offsetY = 2.5f)
    {
        _target   = target;
        _offsetY  = offsetY;
        _mainView = mainView;
        _mainCam  = Camera.main;

        if (_mainView == null) return;

        _bar = UIPackage.CreateObject(packageName, "barHP") as GProgressBar;
        if (_bar == null) return;

        _bar.SetSize(BAR_WIDTH, BAR_HEIGHT);
        _mainView.AddChildAt(_bar, 0);

        UpdatePosition();
    }

    public void UpdateValue(float current, float max)
    {
        if (_bar == null) return;
        _bar.max   = max;
        _bar.value = current;
    }

    public void UpdatePosition()
    {
        if (_bar == null || _target == null || _mainCam == null) return;

        Vector3 worldPos  = _target.position + new Vector3(0, _offsetY, 0);
        Vector3 screenPos = _mainCam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0)
        {
            _bar.visible = false;
            return;
        }

        _bar.visible = true;
        screenPos.y  = Screen.height - screenPos.y;

        Vector2 localPos = GRoot.inst.GlobalToLocal(screenPos);
        _bar.SetXY(localPos.x - BAR_WIDTH / 2.02f, localPos.y);
    }

    public void Destroy()
    {
        if (_bar != null)
        {
            _bar.RemoveFromParent();
            _bar.Dispose();
            _bar = null;
        }
    }
}