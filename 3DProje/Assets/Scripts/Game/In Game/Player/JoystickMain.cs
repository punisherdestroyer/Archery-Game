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
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = this.GetComponent<UIPanel>().ui;
        _text = _mainView.GetChild("n9").asTextField;

        _joystick = new JoystickModule(_mainView);
        _joystick.onMove.Add(__joystickMove);
        _joystick.onEnd.Add(__joystickEnd);
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            _wasKeyboard = true;
            _joystick.ApplyKeyboardInput(h, v);
        }
        else if (_wasKeyboard)
        {
            _wasKeyboard = false;
            _joystick.ApplyKeyboardInput(0, 0);
        }

        if (_isMoving && player != null)
        {
            player.transform.Translate(_moveDir * moveSpeed * Time.deltaTime, Space.World);
            if (_moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDir);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    void __joystickMove(EventContext context)
    {
        float degree = (float)context.data;
        _text.text = "" + degree;
        float rad = degree * Mathf.Deg2Rad;
        _moveDir = new Vector3(Mathf.Cos(rad), 0, -Mathf.Sin(rad));
        _isMoving = true;
    }

    void __joystickEnd()
    {
        _text.text = "";
        _isMoving = false;
        _moveDir = Vector3.zero;
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
            Application.Quit();
    }
}