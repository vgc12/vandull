using General.Logging;
using Items.Guns.Trail;

namespace Items.Guns
{
    public static class GunSystemsBuilderExtensions
    {
        public static IGunSystemsBuilder ForPlayer(this IGunSystemsBuilder builder, ILogger logger) =>
            builder
                .AddShotFiredHandler(e => logger.Log($"Player fired at {e.RaycastPoint}"))
                .AddAmmoOutHandler(() => logger.Log("Player out of ammo!"))
                .WithOwnerStatus(OwnerStatus.Player);


        public static IGunSystemsBuilder WithCustomTrails(this IGunSystemsBuilder builder,
            TrailSettings customTrailSettings) =>
            builder.WithTrailSystem(() => new TrailSystem(customTrailSettings));
    }

    public enum OwnerStatus
    {
        Player,
        Enemy,
        Neutral
    }
}