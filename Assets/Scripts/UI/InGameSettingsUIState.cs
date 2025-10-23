using System;
using UnityEngine.UIElements;

namespace UI
{
    public class InGameSettingsUIState : SettingsUIState
    {
        public InGameSettingsUIState(VisualElement rootElement, Action onClose) : base(rootElement, onClose)
        {
        }
    }
}