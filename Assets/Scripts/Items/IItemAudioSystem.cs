using DependencyInjection;
using Items.Guns;

namespace Items
{
    public interface IItemAudioSystem : IItemSystem
    {
        IAudioManager AudioManager { get; }
    }
}