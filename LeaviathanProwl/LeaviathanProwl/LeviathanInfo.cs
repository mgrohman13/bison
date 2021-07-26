using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace LeaviathanProwl
{
    public class LeviathanInfo
    {
        public readonly Creature creature;
        public double updateTime = 0;
        public bool setLeash = false;

        public LeviathanInfo(Creature creature)
        {
            if (!(creature is ReaperLeviathan || creature is GhostLeviathan))
                Logger.LogInfo(true, "unknown creature {0}", true, creature);

            this.creature = creature;
            ResetUpdate();
        }

        public void Update()
        {
            if (!setLeash)
            {
                StayAtLeashPosition leash = creature.GetComponent<StayAtLeashPosition>();
                leash.leashDistance = NewLeashDistance();
                setLeash = true;
            }

            if (DayNightCycle.main.timePassed > this.updateTime)
            {
                Vector3 leashPosition = creature.leashPosition;
                creature.leashPosition = creature.transform.position;

                Logger.LogInfo("moving creature from {0}", leashPosition);
                Logger.LogInfo(creature);

                ResetUpdate();
            }
        }

        private static float NewLeashDistance()
        {
            //TODO: test ghost leash
            float leash = Config.rand.GaussianCapped(200f * Config.difficulty.leashRoam, .25f, 60f * Config.difficulty.leashRoam);
            Logger.LogInfo("new leash {0}", leash.ToString());
            return leash;
        }

        private void ResetUpdate()
        {
            this.updateTime = DayNightCycle.main.timePassed + Config.rand.Gaussian(60 / Config.difficulty.leashRoam, .3);
        }

        // from ManageCreatureSpawns mod
        public bool KillCreature()
        {
            if (creature is ReaperLeviathan || creature is GhostLeviathan)
                Logger.LogInfo(creature, "Kill");
            else
                Logger.LogInfo("Kill {0}", creature);

            if (IsActive(creature))
            {
                creature.tag = "Untagged";
                creature.leashPosition = UnityEngine.Vector3.zero;
                CreatureDeath cDeath = creature.gameObject.GetComponent<CreatureDeath>();
                if (cDeath != null)
                {
                    cDeath.eatable = null;
                    cDeath.respawn = false;
                    cDeath.removeCorpseAfterSeconds = 1.0f;
                }
                if (creature.liveMixin != null && creature.liveMixin.IsAlive())
                {
                    if (creature.liveMixin.data != null)
                    {
                        creature.liveMixin.data.deathEffect = null;
                        creature.liveMixin.data.passDamageDataOnDeath = false;
                        creature.liveMixin.data.broadcastKillOnDeath = true;
                        creature.liveMixin.data.destroyOnDeath = true;
                        creature.liveMixin.data.explodeOnDestroy = false;
                    }
                    creature.liveMixin.Kill();
                }
                else
                {
                    creature.BroadcastMessage("OnKill");
                }
                return true;
            }
            return false;
        }

        public static bool IsActive(Creature creature)
        {
            return (creature != null && creature.enabled && creature.gameObject != null && creature.isActiveAndEnabled && creature.gameObject.activeInHierarchy);
        }
    }
}
