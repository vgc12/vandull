using EventBus;

namespace Audio
{
    public readonly struct AudioSettingsChangedEvent : IEvent
    {
        public float MainVolume { get; init; }
        public float MusicVolume { get; init; }
        
        public float SoundEffectsVolume { get; init; }
        
        public float DialogueVolume { get; init; }
        
        public float MusicAndSoundEffectsVolume { get; init; }
        
    }
}