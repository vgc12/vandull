using General;
using Items.Guns.Trail;

namespace Items.Guns.Items.Guns.Builder
{
    public static class GunSystemsBuilderExtensions
    {
   
        public static IGunSystemsBuilder ForPlayer(this IGunSystemsBuilder builder)
        {
            return builder
                .AddShotFiredHandler(e => VandullLogger.Log($"Player fired at {e.ShootPoint}"))
                .AddAmmoOutHandler(() => VandullLogger.Log("Player out of ammo!"));
        }
        
        
        public static IGunSystemsBuilder WithCustomTrails(this IGunSystemsBuilder builder, 
            TrailConfig customTrailConfig)
        {
            return builder.WithTrailSystem(() => new TrailSystem(customTrailConfig));
        }
    }
}