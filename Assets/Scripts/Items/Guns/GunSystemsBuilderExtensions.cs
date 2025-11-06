using General.Logging;
using Items.Guns.Trail;

namespace Items.Guns
{
    public static class GunSystemsBuilderExtensions
    {
        public static IGunSystemsBuilder ForPlayer(this IGunSystemsBuilder builder, ILogger logger)
        {
            return builder
                .AddShotFiredHandler(e => logger.Log($"Player fired at {e.ShootPoint}"))
                .AddAmmoOutHandler(() => logger.Log("Player out of ammo!"))
                .WithOwnerStatus(OwnerStatus.Player);
        }


        public static IGunSystemsBuilder WithCustomTrails(this IGunSystemsBuilder builder,
            TrailSettings customTrailSettings)
        {
            return builder.WithTrailSystem(() => new TrailSystem(customTrailSettings));
        }
    }

    public enum OwnerStatus
    {
        Player,
        Enemy,
        Neutral
    }
}