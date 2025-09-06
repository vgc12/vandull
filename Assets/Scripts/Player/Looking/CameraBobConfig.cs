using Player.Looking.Player.Looking;
using UnityEngine;

namespace Player.Looking
{
    [CreateAssetMenu(fileName = "CameraBobConfig", menuName = "Configs/Player/Movement/CameraBobConfig", order = 1)]
    public class CameraBobConfig : ScriptableObject
    {
    
        public CameraBobSetting walkConfig;
        public CameraBobSetting sprintConfig;

    }
}