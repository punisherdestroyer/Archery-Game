using UnityEngine;
using FairyGUI;

public class JoystickMain : MonoBehaviour
{
    GComponent _mainView;
    GTextField _text;
    JoystickModule _joystick;

    public GameObject player;
    public float moveSpeed = 5f;
    private Vector3 _moveDir;
    private bool _isMoving;
    private bool _wasKeyboard;

    void Start()
    {
        // Oyunun hedef kare hızını sabitler.
        Application.targetFrameRate = 60;
        
        // FairyGUI sahnesindeki klavye tuş basımlarını dinleyecek fonksiyonu bağlar.
        Stage.inst.onKeyDown.Add(OnKeyDown);

        // UI Panel bileşeni üzerinden ana görünümü ve ilgili metin alanını önbelleğe alır.
        _mainView = this.GetComponent<UIPanel>().ui;
        _text = _mainView.GetChild("n9").asTextField;

        // Joystick modülünü başlatır ve hareket/bitiş olaylarına ilgili fonksiyonları bağlar.
        _joystick = new JoystickModule(_mainView);
        _joystick.onMove.Add(__joystickMove);
        _joystick.onEnd.Add(__joystickEnd);
    }

    void Update()
    {
        // Klavyeden gelen yatay ve dikey eksen girdilerini ham (raw) olarak okur.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Klavye girdisi mevcutsa veriyi işlenmek üzere joystick modülüne aktarır.
        if (h != 0 || v != 0)
        {
            _wasKeyboard = true;
            _joystick.ApplyKeyboardInput(h, v);
        }
        // Klavye girdisi bittiği an joystick üzerindeki klavye etkisini sıfırlar.
        else if (_wasKeyboard)
        {
            _wasKeyboard = false;
            _joystick.ApplyKeyboardInput(0, 0);
        }

        // Joystick veya klavye tetiklemesiyle hareket durumu aktifse oyuncuyu dünya koordinatlarında yürütür.
        if (_isMoving && player != null)
        {
            player.transform.Translate(_moveDir * moveSpeed * Time.deltaTime, Space.World);
            
            // Oyuncu hareket ediyorsa yüzünü yumuşak bir rotasyonla hareket yönüne doğru çevirir.
            if (_moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDir);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    void __joystickMove(EventContext context)
    {
        // Joystick'in döndüğü anlık açı değerini alır ve ekrandaki metin alanına yansıtır.
        float degree = (float)context.data;
        _text.text = degree.ToString();
        
        // Derece cinsinden gelen açıyı radyana çevirerek trigonometrik hareket yönü vektörünü hesaplar.
        float rad = degree * Mathf.Deg2Rad;
        _moveDir = new Vector3(Mathf.Cos(rad), 0, -Mathf.Sin(rad));
        _isMoving = true;
    }

    void __joystickEnd()
    {
        // Ekrana dokunma bırakıldığında veya klavye girdisi kesildiğinde verileri ve arayüzü sıfırlar.
        _text.text = string.Empty;
        _isMoving = false;
        _moveDir = Vector3.zero;
    }

    void OnKeyDown(EventContext context)
    {
        // Eğer basılan klavye tuşu Escape ise uygulamadan çıkış yapar.
        if (context.inputEvent.keyCode == KeyCode.Escape)
            Debug.Log("Başarıyla çıkış yapıldı.");
            Application.Quit();
    }
}