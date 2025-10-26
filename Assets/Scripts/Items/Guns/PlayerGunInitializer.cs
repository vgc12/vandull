using DependencyInjection;
using EventBus;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "Player Gun Initializer", menuName = "Guns/Initialization/Player Gun Initializer",
        order = 1)]
    public sealed class PlayerGunInitializer : GunInitializer
    {
        private ILogger _logger;

        public override GunSystems CreateGunSystems(Gun gun)
        {
            RuntimeResolver.Instance.TryResolve(out _logger);
            var builder = new Builder(gun);
            return builder.ForPlayer(_logger).AddItemEquippedHandler(() =>
                EventBus<PlayerEquippedNewItemEvent>.Raise(new PlayerEquippedNewItemEvent(gun))).Build();
        }
    }

    public class PlayerEquippedNewItemEvent : IEvent
    {
        public readonly Item Item;

        public PlayerEquippedNewItemEvent(Item item)
        {
            Item = item;
        }
    }
}