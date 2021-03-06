using RSBot.Core.Event;

namespace RSBot.Core.Network.Handler.Agent.Action
{
    internal class ActionSkillCastResponse : IPacketHandler
    {
        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public PacketDestination Destination => PacketDestination.Client;

        /// <summary>
        /// Gets or sets the opcode.
        /// </summary>
        /// <value>
        /// The opcode.
        /// </value>
        public ushort Opcode => 0xB070;

        public void Invoke(Packet packet)
        {
            var result = packet.ReadByte();

            if (result != 0x01) return;

            packet.ReadUShort(); //Error .. always 00 30

            var action = Objects.Action.FromPacket(packet);

            if (action.PlayerIsExecutor)
            {
                Core.Game.Player.Tracker.StopMoving();

                var skill = Core.Game.Player.Skills.GetSkillInfoById(action.SkillId);
                if (skill == null) return;

                skill.UpdateTicks();

                EventManager.FireEvent("OnCastSkill", action.SkillId);

                return;
            }

            var executor = action.GetExecutor();
            if (executor == null) return;

            executor.Tracker?.StopMoving();

            if (!action.PlayerIsTarget) return;

            EventManager.FireEvent("OnEnemySkillOnPlayer");

            executor.StartAttackingTimer();
        }
    }
}