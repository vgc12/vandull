using Npcs.Shared;
using StateMachine;

namespace Npcs.States
{
    public class NpcState : BaseState
    {
        protected readonly Npc Npc;

        protected NpcState(Npc npc)
        {
            Npc = npc;
        }
    }
}