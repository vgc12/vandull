using Levels;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.States
{
    public abstract class MissionOverUIState : UIBaseState
    {
        private readonly Button _button;
        protected readonly Data StateData;

        protected MissionOverUIState(VisualElement rootElement, UIStateMachine stateMachine, Data stateData,
            UIStateType stateType) : base(rootElement, stateMachine, stateType)
        {
            StateData = stateData;
            _button = rootElement.Q<Button>("restart-button");
            _button.clicked += LevelManager.Instance.ReloadLevel;
        }

        public override void Enter()
        {
            base.Enter();
            ApplyData(RootPageElement, StateData);
        }

        public void ApplyData(VisualElement rootPageElement, Data data)
        {
            var statusLabel = rootPageElement.Q<Label>("mission-status-label");
            if (statusLabel != null)
            {
                statusLabel.text = data.StatusLabelText;
                statusLabel.style.color = data.StatusLabelColor;
            }

            var descriptionLabel = rootPageElement.Q<Label>("mission-description-label");
            if (descriptionLabel != null) descriptionLabel.text = data.DescriptionLabelText;
        }


        ~MissionOverUIState()
        {
            _button.clicked -= LevelManager.Instance.ReloadLevel;
        }

        public sealed class Data
        {
            private Data()
            {
            }

            public string StatusLabelText { get; private set; }
            public string DescriptionLabelText { get; private set; }
            public Color StatusLabelColor { get; private set; }

            public Color DescriptionLabelColor { get; private set; }

            public sealed class Builder : IBuilder<Data>
            {
                private readonly Data _data = new();


                public Data Build()
                {
                    var data = _data;
                    return data;
                }


                public Builder WithStatusLabelText(string text)
                {
                    _data.StatusLabelText = text;
                    return this;
                }

                public Builder WithDescriptionLabelText(string text)
                {
                    _data.DescriptionLabelText = text;
                    return this;
                }

                public Builder WithStatusLabelColor(Color color)
                {
                    _data.StatusLabelColor = color;
                    return this;
                }

                public Builder WithDescriptionLabelColor(Color color)
                {
                    _data.DescriptionLabelColor = color;
                    return this;
                }
            }
        }
    }
}