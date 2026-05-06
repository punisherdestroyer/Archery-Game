using FairyGUI;
using UnityEngine;

public class Joystick : MonoBehaviour
{
    private GComponent _mainView;
    private GComponent _joystick;
    private GObject _thumb;
    private Vector2 _startPos;
    public Vector2 InputDirection { get; private set; }

    public void Init(GComponent mainView)
    {
        _mainView = mainView;
        _joystick = _mainView.GetChild("Joystick")?.asCom;
        if (_joystick != null)
        {
            _thumb = _joystick.GetChild("Joystick Handle") ?? _joystick.GetChild("thumb");
            if (_thumb != null)
            {
                _startPos = _thumb.xy;
                _joystick.onTouchBegin.Add(OnTouchBegin);
                _joystick.onTouchMove.Add(OnTouchMove);
                _joystick.onTouchEnd.Add(OnTouchEnd);
            }
        }
    }

    private void OnTouchBegin(EventContext context)
    {
        context.CaptureTouch();
        UpdateInput(context);
    }

    private void OnTouchMove(EventContext context)
    {
        UpdateInput(context);                                                                                                                
    }

    private void OnTouchEnd(EventContext context)
    {
        if (_thumb != null) _thumb.xy = _startPos;
        InputDirection = Vector2.zero;
    }

    private void UpdateInput(EventContext context)
    {
        if (_joystick == null || _thumb == null) return;
        
        Vector2 pt = GRoot.inst.GlobalToLocal(context.inputEvent.position);
        Vector2 localPt = _joystick.GlobalToLocal(pt);
        Vector2 dist = localPt - new Vector2(_joystick.width / 2, _joystick.height / 2);
        
        if (dist.sqrMagnitude > 0)
        {
            InputDirection = dist.normalized;
        }
        else
        {
            InputDirection = Vector2.zero;
        }
        
        _thumb.xy = new Vector2(_joystick.width / 2, _joystick.height / 2) + (InputDirection * Mathf.Min(dist.magnitude, _joystick.width / 2));
    }
}