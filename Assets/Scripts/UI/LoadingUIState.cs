using EventBus;
using Levels;
using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class LoadingUIState : UIBaseState
    {
        private readonly EventBinding<LevelLoadProgressEvent> _levelLoadProgressEventBinding;
        private readonly ProgressBar _progressBar;

        public LoadingUIState(VisualElement rootElement, UIStateMachine uiStateMachine) : base(rootElement,
            uiStateMachine, UIStateType.Loading)
        {
            _progressBar = rootElement.Q<ProgressBar>("loading-bar");
            _levelLoadProgressEventBinding = new EventBinding<LevelLoadProgressEvent>(OnLevelLoadProgress);
            EventBus<LevelLoadProgressEvent>.Register(_levelLoadProgressEventBinding);
        }

        private void OnLevelLoadProgress(LevelLoadProgressEvent obj)
        {
            _progressBar.value = obj.Progress;
        }

        ~LoadingUIState()
        {
            EventBus<LevelLoadProgressEvent>.Deregister(_levelLoadProgressEventBinding);
        }
    }
}