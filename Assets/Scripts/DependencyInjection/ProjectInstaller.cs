using General.Logging;
using Reflex.Core;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace DependencyInjection
{
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            // builder.AddSingleton(typeof(InGameAudioManager), typeof(IAudioManager));
            builder.AddSingleton(new VandullLogger(), typeof(ILogger));
        }
    }
}