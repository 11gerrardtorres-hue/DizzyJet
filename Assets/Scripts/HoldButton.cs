using UnityEngine;
using UnityEngine.EventSystems;

// UI 위에서 누르고 있는 동안 IsHeld = true (터치/마우스 공용, 멀티터치 지원)
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsHeld { get; private set; }

    public void OnPointerDown(PointerEventData e) { IsHeld = true; }
    public void OnPointerUp(PointerEventData e)   { IsHeld = false; }

    void OnDisable() { IsHeld = false; }
}
