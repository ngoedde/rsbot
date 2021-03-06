using RSBot.Core.Network;
using RSBot.Core.Objects.Skill;
using System.Collections.Generic;
using System.Linq;

namespace RSBot.Core.Objects
{
    public class State
    {
        /// <summary>
        /// Gets or sets the state of the life.
        /// </summary>
        /// <value>
        /// The state of the life.
        /// </value>
        public LifeState LifeState { get; set; }

        /// <summary>
        /// Gets or sets the motion.
        /// </summary>
        /// <value>
        /// The motion.
        /// </value>
        public MotionState MotionState { get; set; }

        /// <summary>
        /// Gets or sets the state of the body.
        /// </summary>
        /// <value>
        /// The state of the body.
        /// </value>
        public BodyState BodyState { get; set; }

        /// <summary>
        /// Gets or sets the walk speed.
        /// </summary>
        /// <value>
        /// The walk speed.
        /// </value>
        public float WalkSpeed { get; set; }

        /// <summary>
        /// Gets or sets the run speed.
        /// </summary>
        /// <value>
        /// The run speed.
        /// </value>
        public float RunSpeed { get; set; }

        /// <summary>
        /// Gets or sets the berzerk speed.
        /// </summary>
        /// <value>
        /// The berzerk speed.
        /// </value>
        public float BerzerkSpeed { get; set; }

        /// <summary>
        /// Gets or sets the active buffs.
        /// </summary>
        /// <value>
        /// The active buffs.
        /// </value>
        public List<BuffInfo> ActiveBuffs { get; set; }

        /// <summary>
        /// Gets or sets the state of the PVP.
        /// </summary>
        /// <value>
        /// The state of the PVP.
        /// </value>
        public PvpState PvpState { get; set; }

        /// <summary>
        /// Gets or sets the state of the battle.
        /// </summary>
        /// <value>
        /// The state of the battle.
        /// </value>
        public BattleState BattleState { get; set; }

        /// <summary>
        /// Gets or sets the state of the scroll.
        /// </summary>
        /// <value>
        /// The state of the scroll.
        /// </value>
        public ScrollState ScrollState { get; set; }

        /// <summary>
        /// Creates a new state object by the given packet
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns></returns>
        public static State FromPacket(Packet packet)
        {
            var result = new State
            {
                ActiveBuffs = new List<BuffInfo>(),
                LifeState = (LifeState)packet.ReadByte()
            };

            packet.ReadByte(); //UNK1

            result.MotionState = (MotionState)packet.ReadByte();
            result.BodyState = (BodyState)packet.ReadByte();
            result.WalkSpeed = packet.ReadFloat();
            result.RunSpeed = packet.ReadFloat();
            result.BerzerkSpeed = packet.ReadFloat();

            var buffCount = packet.ReadByte();
            for (var i = 0; i < buffCount; i++)
            {
                var buff = new BuffInfo
                {
                    Id = packet.ReadUInt(),
                    Duration = packet.ReadUInt()
                };

                if (buff.Record == null)
                    continue;

                if (buff.Record.Params.Contains(1701213281))
                    packet.ReadBool(); //IsCreator

                result.ActiveBuffs.Add(buff);
            }

            return result;
        }

        /// <summary>
        /// Gets the active buff by skill identifier.
        /// </summary>
        /// <returns></returns>
        public bool HasActiveBuff(SkillInfo skill, out BuffInfo buff)
        {
            /*
             * TODO:
             * This is not a definitive solution.
             * This will check if there is the same type of buff until you find a better way.
             * The purpose here is to check the ReqLearn_Skill1 or groupId chain linked to each other. 
             * This way, we could check if any buf of the same type was working.
             * basicGroup = SKILL_CH_FIRE_GONGUP_A => SKILL_CH_FIRE_GONGUP
             * basicGroup = SKILL_CH_FIRE_GONGUP_B => SKILL_CH_FIRE_GONGUP
             * basicGroup = SKILL_CH_FIRE_GONGUP_C => SKILL_CH_FIRE_GONGUP
             * basicGroup = SKILL_CH_FIRE_GONGUP_D => SKILL_CH_FIRE_GONGUP
             */
            var groupBase = skill.Record.Basic_Group.Remove(skill.Record.Basic_Group.Length - 2, 2);
            buff = ActiveBuffs.Find(p => p.Record.Basic_Group.Remove(p.Record.Basic_Group.Length - 2, 2) == groupBase);

            return buff != null;
        }

        /// <summary>
        /// Gets the active buff by skill identifier.
        /// </summary>
        /// <returns></returns>
        public BuffInfo GetActiveBuffBySkillId(uint skillId)
        {
            return ActiveBuffs.FirstOrDefault(b => b.Id == skillId);
        }

        /// <summary>
        /// Gets the active buff by skill identifier.
        /// </summary>
        /// <returns></returns>
        public BuffInfo GetActiveBuff(uint token)
        {
            return ActiveBuffs.FirstOrDefault(b => b.Token == token);
        }
    }
}