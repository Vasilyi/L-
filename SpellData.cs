#region

using System.Net.Configuration;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace Skillshots
{
    public class SpellData
    {
        public bool AddHitbox;
        public string BaseSkinName;
        public bool CanBeRemoved = false;
        public int Delay;
        public bool DonCross = false;
        public int ExtraDuration;
        public bool FixedRange;
        public int Id = -1;
        public int MissileSpeed;
        public string MissileSpellName;
        private int _range;
        public SpellSlot Slot;
        public string SpellName;
        private int _radius;
        public int DangerValue;
        public string ToggleParticleName = "";
        public string FromObject = "";
        public Prediction.SkillshotType Type;
        public bool DontAddExtraDuration;
        public bool Centered;
        public bool Invert;
        public int MultipleNumber = -1;
        public float MultipleAngle;
        public int ExtraRange = -1;
        public bool MissileFollowsUnit;

        public bool DisableFowDetection = false;
        public bool IsDangerous = false;

        public int RingRadius;
        public SpellData()
        {

        }

        public SpellData(string baseSkinName, string spellName, SpellSlot slot, Prediction.SkillshotType type, int delay, int range,
            int radius, int missileSpeed, bool addHitbox, bool fixedRange, int defaultDangerValue)
        {
            BaseSkinName = baseSkinName;
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
            get { return BaseSkinName + " - " + SpellName; }
        }

        public int Radius
        {
            get
            {
                return _radius;
            }
            set { _radius = value; }
        }



        public int Range
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