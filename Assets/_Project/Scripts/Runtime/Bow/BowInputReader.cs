using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ArrowBuster
{
    /// <summary>
    /// Polls <c>Pointer.current</c> (mouse, touch, pen) once per frame (D-007) and emits Began/Moved/Ended.
    /// A press that begins over UI is flagged so the bow ignores it until release.
    /// </summary>
    public sealed class BowInputReader
    {
        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>(4);
        private PointerEventData _eventData;
        private bool _pressed;
        private bool _startedOverUi;
        private Vector2 _lastPosition;

        public PointerSample Read()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null) return default;

            bool isPressed = pointer.press.isPressed;
            Vector2 position = pointer.position.ReadValue();

            if (isPressed && !_pressed)
            {
                _pressed = true;
                _startedOverUi = IsOverUi(position);
                _lastPosition = position;
                return new PointerSample(PointerPhase.Began, position, _startedOverUi);
            }
            if (isPressed)
            {
                _lastPosition = position;
                return new PointerSample(PointerPhase.Moved, position, _startedOverUi);
            }
            if (_pressed)
            {
                _pressed = false;
                return new PointerSample(PointerPhase.Ended, _lastPosition, _startedOverUi);
            }
            return default;
        }

        public void Reset() => _pressed = false;

        private bool IsOverUi(Vector2 position)
        {
            EventSystem system = EventSystem.current;
            if (system == null) return false;
            if (_eventData == null || _eventData.currentInputModule == null) _eventData = new PointerEventData(system);
            _eventData.position = position;
            _uiHits.Clear();
            system.RaycastAll(_eventData, _uiHits);
            return _uiHits.Count > 0;
        }
    }
}
