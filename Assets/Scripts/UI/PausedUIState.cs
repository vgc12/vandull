using System;
using UI.States;
using UnityEngine.UIElements;

public class PausedUIState : UIBaseState
{
    private readonly Action _resumeButtonClicked;
    private readonly Action _settingsButtonClicked;
    private readonly Action _quitButtonClicked;

    private readonly Button _resumeButton;
    private readonly Button _settingsButton;
    private readonly Button _quitButton;

    public PausedUIState(VisualElement rootElement, 
        Action resumeButtonClicked = null,
        Action settingsButtonClicked = null, 
        Action quitButtonClicked = null) : base(rootElement)
    {
        _resumeButtonClicked = resumeButtonClicked;
        _settingsButtonClicked = settingsButtonClicked;
        _quitButtonClicked = quitButtonClicked;

        _resumeButton = rootElement.Q<Button>("ResumeButton");
        _settingsButton = rootElement.Q<Button>("SettingsButton");
        _quitButton = rootElement.Q<Button>("QuitButton");
    }

    public override void Enter()
    {
        base.Enter();
        
        if (_resumeButton != null) _resumeButton.clicked += _resumeButtonClicked;
        if (_settingsButton != null) _settingsButton.clicked += _settingsButtonClicked;
        if (_quitButton != null) _quitButton.clicked += _quitButtonClicked;
    }

    public override void Exit()
    {
        if (_resumeButton != null) _resumeButton.clicked -= _resumeButtonClicked;
        if (_settingsButton != null) _settingsButton.clicked -= _settingsButtonClicked;
        if (_quitButton != null) _quitButton.clicked -= _quitButtonClicked;
        
        base.Exit();
    }
}