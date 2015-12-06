﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using DamageMeter.Skills;
using DamageMeter.Skills.Skill;

namespace DamageMeter.Dealt
{
    public class EntitiesDealt
    {
        private readonly PlayerInfo _playerInfo;
        public ConcurrentDictionary<Entity, SkillsStats> EntitiesStats = new ConcurrentDictionary<Entity, SkillsStats>();

        public EntitiesDealt(PlayerInfo playerInfo)
        {
            _playerInfo = playerInfo;
        }

        public double DamageFraction
        {
            get
            {
                if (DamageTracker.Instance.TotalDamage == 0)
                {
                    return 0;
                }
                return Math.Round(((double) Damage*100/DamageTracker.Instance.TotalDamage), 1);
            }
        }

        public long FirstHit
        {
            get
            {
                lock (EntitiesStats)
                {
                    if (NetworkController.Instance.Encounter != null &&
                        EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                        return EntitiesStats[NetworkController.Instance.Encounter].FirstHit;
                    if (NetworkController.Instance.Encounter != null) return 0;
                    long firsthit = 0;
                    foreach (var entityStats in EntitiesStats)
                    {
                        if (firsthit == 0)
                        {
                            firsthit = entityStats.Value.FirstHit;
                        }
                        else if (entityStats.Value.FirstHit < firsthit && entityStats.Value.FirstHit != 0)
                        {
                            firsthit = entityStats.Value.FirstHit;
                        }
                    }
                    return firsthit;
                }
            }
        }

        public long LastHit
        {
            get
            {
                lock (EntitiesStats)
                {
                    if (NetworkController.Instance.Encounter != null &&
                        EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                        return EntitiesStats[NetworkController.Instance.Encounter].LastHit;
                    if (NetworkController.Instance.Encounter != null) return 0;
                    long lasthit = 0;
                    foreach (var entityStats in EntitiesStats)
                    {
                        if (lasthit == 0)
                        {
                            lasthit = entityStats.Value.LastHit;
                        }
                        else if (entityStats.Value.LastHit > lasthit)
                        {
                            lasthit = entityStats.Value.LastHit;
                        }
                    }
                    return lasthit;
                }
            }
        }

        public long Interval => LastHit - FirstHit;

        public long Dps
        {
            get
            {
                if (Interval == 0)
                {
                    return 0;
                }
                return Damage/Interval;
            }
        }


        public long Damage
        {
            get
            {
                lock (EntitiesStats)
                {
                    if (NetworkController.Instance.Encounter == null)
                    {
                        return EntitiesStats.Sum(entityStats => entityStats.Value.Damage);
                    }
                    if (EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                    {
                        return EntitiesStats[NetworkController.Instance.Encounter].Damage;
                    }
                    return 0;
                }
            }
        }

        public long Mana
        {
            get
            {
                lock (EntitiesStats)
                {
                    return EntitiesStats.Sum(entityStats => entityStats.Value.Mana);
                }
            }
        }

        public long Heal
        {
            get
            {
                lock (EntitiesStats)
                {
                    return EntitiesStats.Sum(entityStats => entityStats.Value.Heal);
                }
            }
        }

        public ConcurrentDictionary<Skill, SkillStats> AllSkills
        {
            get
            {
                var skills = new ConcurrentDictionary<Skill, SkillStats>();

                lock (EntitiesStats)
                {
                    foreach (var skill in EntitiesStats.SelectMany(entities => entities.Value.Skills))
                    {
                        if (skills.ContainsKey(skill.Key))
                        {
                            skills[skill.Key] += skill.Value;
                        }
                        else
                        {
                            skills[skill.Key] = skill.Value;
                        }
                    }
                }
                return skills;
            }
        }

        public double CritRate
        {
            get
            {
                var hits = Hits;
                return hits == 0 ? 0 : Math.Round((double) Crits*100/hits, 1);
            }
        }

        public int Crits
        {
            get
            {
                lock (EntitiesStats)
                {
                    if (NetworkController.Instance.Encounter == null || _playerInfo.IsHealer())
                    {
                        return EntitiesStats.Sum(skills => skills.Value.Crits);
                    }
                    if (EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                    {
                        return EntitiesStats[NetworkController.Instance.Encounter].Crits;
                    }
                    return 0;
                }
            }
        }

        public int Hits
        {
            get
            {
                lock (EntitiesStats)
                {
                    if (NetworkController.Instance.Encounter == null || _playerInfo.IsHealer())
                    {
                        return EntitiesStats.Sum(skills => skills.Value.Hits);
                    }
                    if (EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                    {
                        return EntitiesStats[NetworkController.Instance.Encounter].Hits;
                    }
                    return 0;
                }
            }
        }
    }
}