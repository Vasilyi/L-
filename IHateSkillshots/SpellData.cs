#region

using System.Net.Configuration;
using LeagueSharp;
using LeagueSharp.SDK.Enumerations;
#endregion

namespace Skillshots
{
    public class SpellData
    {

        public bool AddHitbox;
        public string ChampionName;
        public bool CanBeRemoved = false;
        public float Delay;
        public bool DonCross = false;
        public int ExtraDuration;
        public bool FixedRange;
        public int Id = -1;
        public float MissileSpeed;
        public string MissileSpellName;
        private float _range;
        public SpellSlot Slot;
        public string SpellName;
        private float _radius;
        public int DangerValue;
        public string ToggleParticleName = "";
        public string FromObject = "";
        public SkillshotType Type;
        public bool DontAddExtraDuration;
        public bool Centered;
        public bool Invert;
        public int MultipleNumber = -1;
        public float MultipleAngle;
        public bool ForceRemove = false;
        public int ExtraRange = -1;
        public bool MissileFollowsUnit;
        public int MissileAccel = 0;
        public int MissileMaxSpeed;
        public int MissileMinSpeed;
        public bool DisableFowDetection = false;
        public bool IsDangerous = false;

        public int RingRadius;
        public SpellData()
        {

        }

        public SpellData(string ChampionName, string spellName, SpellSlot slot, SkillshotType type, float delay, float range,
            float radius, float missileSpeed, bool addHitbox, bool fixedRange, int defaultDangerValue)
        {
            SpellName = spellName;
            Slot = slot;
            Type = type;
            Delay = delay;
            Range = range;
            _radius = radius;
            MissileSpeed = missileSpeed;
            AddHitbox = addHitbox;
            FixedRange = fixedRange;
            DangerValue = defaultDangerValue;
        }

        public string MenuItemName
        {
            get { return ChampionName + " - " + SpellName; }
        }

        public float Radius
        {
            get
            {
                return _radius;
            }
            set { _radius = value; }
        }



        public float Range
        {
            get
            {
                return _range;
            }
            set
            {
                _range = value;
            }
        }


    }
}