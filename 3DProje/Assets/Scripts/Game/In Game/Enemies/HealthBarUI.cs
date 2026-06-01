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
        _mainCam  = Camera.main; // Ana kameranın referansı kurucu fonksiyon içinde önbelleğe alınır.

        if (_mainView == null) return;

        // Belirtilen paketten ilgili can barı bileşeni havuzdan veya paketten oluşturulur.
        _bar = UIPackage.CreateObject(packageName, "barHP") as GProgressBar;
        if (_bar == null) return;

        // Can barının boyutları ayarlanır ve ana arayüz görünümünün en alt katmanına eklenir.
        _bar.SetSize(BAR_WIDTH, BAR_HEIGHT);
        _mainView.AddChildAt(_bar, 0);

        UpdatePosition();
    }

    public void UpdateValue(float current, float max)
    {
        if (_bar == null) return;
        
        // Can barının maksimum ve güncel doluluk değerleri güncellenir.
        _bar.max   = max;
        _bar.value = current;
    }

    public void UpdatePosition()
    {
        if (_bar == null || _target == null || _mainCam == null) return;

        // Hedef nesnenin dünya koordinatlarındaki pozisyonuna dikey bir sapma (offset) eklenir.
        Vector3 worldPos  = _target.position + new Vector3(0, _offsetY, 0);
        
        // Dünya koordinatı, kameranın bakış açısına göre ekran koordinatına dönüştürülür.
        Vector3 screenPos = _mainCam.WorldToScreenPoint(worldPos);

        // Eğer hedef nesne kameranın arkasındaysa can barı gizlenir ve işlem sonlandırılır.
        if (screenPos.z < 0)
        {
            _bar.visible = false;
            return;
        }

        _bar.visible = true;
        
        // Unity'nin sol alt köşeyi (0,0) kabul eden yapısı, FairyGUI'ın sol üst köşeyi (0,0) kabul eden yapısına dönüştürülür.
        screenPos.y  = Screen.height - screenPos.y;

        // Ekran koordinatları FairyGUI kök dizininin yerel arayüz koordinatlarına çevrilir.
        Vector2 localPos = GRoot.inst.GlobalToLocal(screenPos);
        
        // Can barı, hedef nesnenin tam ortasına denk gelecek şekilde hizalanarak konumlandırılır.
        _bar.SetXY(localPos.x - (BAR_WIDTH / 2.02f), localPos.y);
    }

    public void Destroy()
    {
        // Can barı arayüzden sökülür ve belleğin temizlenmesi için tamamen imha edilir.
        if (_bar != null)
        {
            _bar.RemoveFromParent();
            _bar.Dispose();
            _bar = null;
        }
    }
}