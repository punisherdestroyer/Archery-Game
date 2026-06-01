using FairyGUI;
using UnityEngine;

public class JoystickModule : EventDispatcher
{
    float _centerLocalX;
    float _centerLocalY;
    GButton _button;
    GObject _touchArea;
    GObject _thumb;
    GObject _center;
    int touchId;
    GTweener _tweener;

    public EventListener onMove { get; private set; }
    public EventListener onEnd { get; private set; }
    public int radius { get; set; }

    public JoystickModule(GComponent mainView)
    {
        // Tetiklenecek arayüz olayları başlatılır.
        onMove = new EventListener(this, "onMove");
        onEnd = new EventListener(this, "onEnd");

        // Gerekli arayüz bileşenleri hiyerarşi içinde aranarak referansları alınır.
        _button = FindRecursive(mainView, "joystick")?.asButton;
        _touchArea = FindRecursive(mainView, "joystick_touch");
        _center = FindRecursive(mainView, "joystick_center");

        // Joystick butonunun tıklama ile otomatik durum değiştirmesi engellenir ve iç halkası önbelleğe alınır.
        if (_button != null)
        {
            _button.changeStateOnClick = false;
            _thumb = _button.GetChild("thumb");
        }

        // Joystick merkez noktasının yerel koordinat merkezleri hesaplanır.
        if (_center != null)
        {
            _centerLocalX = _center.x + _center.width / 2;
            _centerLocalY = _center.y + _center.height / 2;
        }

        touchId = -1;
        radius = 150;

        // Dokunma alanının etkileşimi açılır ve dokunma olayları ilgili fonksiyonlara bağlanır.
        if (_touchArea != null)
        {
            _touchArea.touchable = true;
            _touchArea.onTouchBegin.Add(OnTouchBegin);
            _touchArea.onTouchMove.Add(OnTouchMove);
            _touchArea.onTouchEnd.Add(OnTouchEnd);
        }
    }

    private GObject FindRecursive(GComponent parent, string name)
    {
        // Belirtilen isimdeki bileşeni hiyerarşik olarak derinlemesine arar.
        GObject child = parent.GetChild(name);
        if (child != null) return child;
        for (int i = 0; i < parent.numChildren; i++)
        {
            GObject obj = parent.GetChildAt(i);
            if (obj.name == name) return obj;
            if (obj is GComponent com)
            {
                GObject found = FindRecursive(com, name);
                if (found != null) return found;
            }
        }
        return null;
    }

    private GComponent GetTouchAreaParent()
    {
        // Dokunma alanının üst ebeveyn bileşenini döndürür.
        return _touchArea?.parent;
    }

    private void OnTouchBegin(EventContext context)
    {
        // Eğer zaten aktif bir dokunma varsa yeni dokunma girdileri göz ardı edilir.
        if (touchId != -1) return;
        InputEvent evt = (InputEvent)context.data;
        touchId = evt.touchId;
        
        // Eğer joystick merkeze dönüyorsa mevcut animasyon kesilir.
        if (_tweener != null) { _tweener.Kill(); _tweener = null; }
        _button.selected = true;
        
        // Dokunma hareketi bu bileşen üzerine kilitlenir.
        context.CaptureTouch();
    }

    private void OnTouchEnd(EventContext context)
    {
        InputEvent inputEvt = (InputEvent)context.data;
        // Dokunmanın bittiği ID ile takibe alınan ID eşleşmiyorsa işlem yapılmaz.
        if (touchId == -1 || inputEvt.touchId != touchId) return;
        touchId = -1;
        
        // Joystick butonunun başlangıçtaki merkez pozisyonu hesaplanır.
        float returnX = _centerLocalX - _button.width / 2;
        float returnY = _centerLocalY - _button.height / 2;
        
        // Joystick yumuşak bir animasyonla merkeze döndürülür ve durumları sıfırlanır.
        _tweener = _button.TweenMove(new Vector2(returnX, returnY), 0.3f).OnComplete(() =>
        {
            _tweener = null;
            _button.selected = false;
            if (_thumb != null) _thumb.rotation = 0;
        });
        
        // Hareketin bittiğine dair olay tetiklenir.
        this.onEnd.Call();
    }

    private void OnTouchMove(EventContext context)
    {
        InputEvent evt = (InputEvent)context.data;
        if (touchId == -1 || evt.touchId != touchId) return;

        GComponent touchParent = GetTouchAreaParent();
        // Global ekran koordinatları, ebeveynin yerel arayüz koordinat sistemine dönüştürülür.
        Vector2 localPt = touchParent != null
            ? touchParent.GlobalToLocal(new Vector2(evt.x, evt.y))
            : GRoot.inst.GlobalToLocal(new Vector2(evt.x, evt.y));

        // Merkeze olan uzaklık sapmaları hesaplanarak girdi motoruna gönderilir.
        float offsetX = localPt.x - _centerLocalX;
        float offsetY = localPt.y - _centerLocalY;
        ApplyInput(offsetX, offsetY);
    }

    public void ApplyKeyboardInput(float axisX, float axisY)
    {
        // Eğer hiçbir klavye tuşuna basılmıyorsa joystick konumu merkeze çekilir.
        if (axisX == 0 && axisY == 0)
        {
            if (touchId != -1) return;
            float returnX = _centerLocalX - _button.width / 2;
            float returnY = _centerLocalY - _button.height / 2;
            _button.SetXY(returnX, returnY);
            if (_thumb != null) _thumb.rotation = 0;
            this.onEnd.Call();
            return;
        }
        
        // Klavye eksen verileri joystick yarıçapı ile çarpılarak yön sapmasına dönüştürülür.
        float offsetX = axisX * radius;
        float offsetY = -axisY * radius;
        ApplyInput(offsetX, offsetY);
    }

    private void ApplyInput(float offsetX, float offsetY)
    {
        // Girdinin oluşturduğu açı radyan ve derece cinsinden hesaplanır.
        float rad = Mathf.Atan2(offsetY, offsetX);
        float degree = rad * Mathf.Rad2Deg;
        
        // Joystick iç halkası hareket yönüne doğru döndürülür.
        if (_thumb != null) _thumb.rotation = degree + 90;
        
        // Matematiksel mesafe hesabı yapılarak joystick'in maksimum yarıçap dışına taşması engellenir.
        float dist = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
        if (dist > radius)
        {
            offsetX = radius * Mathf.Cos(rad);
            offsetY = radius * Mathf.Sin(rad);
        }
        
        // Joystick görseli hesaplanan sınırlar dahilinde yeni konumuna yerleştirilir.
        _button.SetXY(_centerLocalX + offsetX - _button.width / 2, _centerLocalY + offsetY - _button.height / 2);
        
        // Hareket açısı dinleyici fonksiyonlara fırlatılır.
        this.onMove.Call(degree);
    }
}