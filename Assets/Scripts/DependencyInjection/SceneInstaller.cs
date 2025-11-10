using Reflex.Core;
using UnityEngine;

namespace DependencyInjection
{
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            //   builder.AddSingleton(typeof(InputManager),typeof(InputManager), typeof(IInputService), typeof(IPlayerInput), typeof(IUIInput));
        }
    }
}