using UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public interface IBuilder<out T>
    {
        T Build();
    }
    
    public class MissionOverUIState : UIBaseState
    {
        private Data _stateData;
        public MissionOverUIState(VisualElement rootElement, Data stateData) : base(rootElement)
        {
            _stateData = stateData;
    
        }

        private void ApplyData()
        {
            var statusLabel = RootPageElement.Q<Label>("MissionStatusLabel");
            if (statusLabel != null)
            {
                statusLabel.text = _stateData.StatusLabelText;
                statusLabel.style.color = _stateData.StatusLabelColor;
            }
            var descriptionLabel = RootPageElement.Q<Label>("MissionDescriptionLabel");
            if (descriptionLabel != null)
            {
                descriptionLabel.text = _stateData.DescriptionLabelText;
            }
        }
        public override void Enter()
        {
            base.Enter();
            ApplyData();
        }
  
        
        public class Data
        {
            public string StatusLabelText { get; private set; }
            public string DescriptionLabelText { get; private set; }
            public Color StatusLabelColor { get; private set; }
            
            public Color DescriptionLabelColor { get; private set; }


            public class Builder : IBuilder<Data>
            {
                private Data _data = new Data();
           
           
                
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
                
                
                public Data Build()
                {
                    var data = _data;
                    return data;
                }
            }
        }
    }
}