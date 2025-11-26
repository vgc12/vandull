using General.Logging;
using Player.Input;
using Reflex.Core;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace DependencyInjection
{
    public sealed class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            // builder.AddSingleton(typeof(InGameAudioManager), typeof(IAudioManager));
            builder.AddSingleton(new VandullLogger(), typeof(ILogger));

            builder.AddSingleton(typeof(InputManager), typeof(InputManager), typeof(IInputService),
                typeof(IPlayerInput), typeof(IUIInput));
        }
    }
}