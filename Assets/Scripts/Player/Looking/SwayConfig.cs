using UnityEngine;

namespace Player.Looking
{
    [CreateAssetMenu(fileName = "SwayConfig", menuName = "Configs/Player/Movement/SwayConfig", order = 1)]
    public class SwayConfig : ScriptableObject
    {
    
        public Sway IdleSway => idleSway;
        
        [SerializeField] private Sway idleSway;
  
        
    }


    namespace Player.Looking
    {
    }
}