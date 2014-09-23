
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace TRUSDominion
{
    public class ChampionsItems
    {

        public static List<BuildsList> _Builds;
        public static List<BuildsList> Builds
        {
            get
            {
                if (_Builds == null)
                {
                    InitializeBuilds();
                }

                return _Builds;
            }
        }


        public static List<ItemsList> _ItemsToBuy;
        public static List<ItemsList> ItemsToBuy
        {
            get
            {
                if (_ItemsToBuy == null)
                {
                    InitializeItems();
                }

                return _ItemsToBuy;
            }
        }

        private static void InitializeItems()
        {
            _ItemsToBuy = new List<ItemsList>();
            Console.WriteLine("initialize items list");
#region itemlist
    _ItemsToBuy.Add(new ItemsList(3001,"Abyssal Scepter", 980,1026, 1057));
	_ItemsToBuy.Add(new ItemsList(3105,"Aegis of the Legion", 625,3097, 1028));
                _ItemsToBuy.Add(new ItemsList(1052,"Amplifying Tome", 435,0, 0));
	_ItemsToBuy.Add(new ItemsList(3007, "Archangel's Staff", 1140,3073, 1026));
	_ItemsToBuy.Add(new ItemsList(3174, "Athene's Unholy Grail", 920,3108, 3028));
	//_ItemsToBuy.Add(new ItemsList(3005] =780,1031, 3093));
    _ItemsToBuy.Add(new ItemsList(3198, "Augment: Death", 1000, 3200, 0));
    _ItemsToBuy.Add(new ItemsList(3197, "Augment: Gravity", 1000, 3200, 0));
    _ItemsToBuy.Add(new ItemsList(3196, "Augment: Power", 1000, 3200, 0));
    _ItemsToBuy.Add(new ItemsList(3093, "Avarice Blade", 400, 1051, 0));
    _ItemsToBuy.Add(new ItemsList(1038, "B. F. Sword", 1550, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3060, "Banner of Command", 890,1026, 3097));
	_ItemsToBuy.Add(new ItemsList(3102, "Banshee's Veil", 490,1057, 3010));
	_ItemsToBuy.Add(new ItemsList(3006, "Berserker's Greaves", 150,1001, 1042));
    _ItemsToBuy.Add(new ItemsList(3254, "Berserker's Greaves - Enchantment: Alacrity", 475, 3006, 0));
    _ItemsToBuy.Add(new ItemsList(3251, "Berserker's Greaves - Enchantment: Captain", 750, 3006, 0));
    _ItemsToBuy.Add(new ItemsList(3253, "Berserker's Greaves - Enchantment: Distortion", 475, 3006, 0));
    _ItemsToBuy.Add(new ItemsList(3252, "Berserker's Greaves - Enchantment: Furor", 650, 3006, 0));
    _ItemsToBuy.Add(new ItemsList(3250, "Berserker's Greaves - Enchantment: Homeguard", 475, 3006, 0));
	_ItemsToBuy.Add(new ItemsList(3144, "Bilgewater Cutlass", 250,1037, 1053));
	_ItemsToBuy.Add(new ItemsList(3188, "Blackfire Torch", 700,3098, 3136));
    _ItemsToBuy.Add(new ItemsList(3153, "Blade of the Ruined King", 975, 3144, 0));
    _ItemsToBuy.Add(new ItemsList(1026, "Blasting Wand", 860, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3168, "Bonetooth Necklace", 800, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3117, "Boots of Mobility", 650, 1001, 0));
    _ItemsToBuy.Add(new ItemsList(3274, "Boots of Mobility - Enchantment: Alacrity", 475, 3117, 0));
    _ItemsToBuy.Add(new ItemsList(3271, "Boots of Mobility - Enchantment: Captain", 750, 3117, 0));
    _ItemsToBuy.Add(new ItemsList(3273, "Boots of Mobility - Enchantment: Distortion", 475, 3117, 0));
    _ItemsToBuy.Add(new ItemsList(3272, "Boots of Mobility - Enchantment: Furor", 650, 3117, 0));
    _ItemsToBuy.Add(new ItemsList(3270, "Boots of Mobility - Enchantment: Homeguard", 475, 3117, 0));
    _ItemsToBuy.Add(new ItemsList(1001, "Boots of Speed", 350, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3009, "Boots of Swiftness", 650, 1001, 0));
    _ItemsToBuy.Add(new ItemsList(3284, "Boots of Swiftness - Enchantment: Alacrity", 475, 3009, 0));
    _ItemsToBuy.Add(new ItemsList(3281, "Boots of Swiftness - Enchantment: Captain", 750, 3009, 0));
    _ItemsToBuy.Add(new ItemsList(3283, "Boots of Swiftness - Enchantment: Distortion", 475, 3009, 0));
    _ItemsToBuy.Add(new ItemsList(3282, "Boots of Swiftness - Enchantment: Furor", 650, 3009, 0));
    _ItemsToBuy.Add(new ItemsList(3280, "Boots of Swiftness - Enchantment: Homeguard", 475, 3009, 0));
    _ItemsToBuy.Add(new ItemsList(1051, "Brawler's Gloves", 400, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3010, "Catalyst the Protector", 325,1028, 1027));
    _ItemsToBuy.Add(new ItemsList(1031, "Chain Vest", 720, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3028, "Chalice of Harmony", 300,1004, 1033));
	_ItemsToBuy.Add(new ItemsList(3172, "Cloak and Dagger", 1200,3101, 1036));
    _ItemsToBuy.Add(new ItemsList(1018, "Cloak of Agility", 730, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1029, "Cloth Armor", 300, 0, 0));
    _ItemsToBuy.Add(new ItemsList(2041, "Crystalline Flask", 225, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1042, "Dagger", 400, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3128, "Deathfire Grasp", 965,1058, 1052));
    _ItemsToBuy.Add(new ItemsList(1055, "Doran's Blade", 475, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1056, "Doran's Ring", 475, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1054, "Doran's Shield", 475, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3173, "Eleisa's Miracle", 400, 3096, 0));
    _ItemsToBuy.Add(new ItemsList(2039, "Elixir of Brilliance", 250, 0, 0));
    _ItemsToBuy.Add(new ItemsList(2037, "Elixir of Fortitude", 250, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3097, "Emblem of Valor", 170,1029, 1006));
	_ItemsToBuy.Add(new ItemsList(3184, "Entropy", 600,3044, 1038));
	_ItemsToBuy.Add(new ItemsList(3123, "Executioner's Calling", 700,3093, 1036));
    _ItemsToBuy.Add(new ItemsList(2050, "Explorer Ward", 0, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1004, "Faerie Charm", 180, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3108, "Fiendish Codex", 385,1004, 1052));
	_ItemsToBuy.Add(new ItemsList(3110, "Frozen Heart", 500,3082, 3024));
	_ItemsToBuy.Add(new ItemsList(3022, "Frozen Mallet", 835,3044, 1011));
    _ItemsToBuy.Add(new ItemsList(1011, "Giant's Belt", 1000, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3024, "Glacial Shroud", 380,1027, 1031));
	_ItemsToBuy.Add(new ItemsList(3159, "Grez's Spectral Lantern", 150,1029, 1053));
	_ItemsToBuy.Add(new ItemsList(3026, "Guardian Angel", 1480,1033, 1031));
	_ItemsToBuy.Add(new ItemsList(3124, "Guinsoo's Rageblade", 865,1026, 1037));
	_ItemsToBuy.Add(new ItemsList(3136, "Haunting Guise", 575,1028, 1052));
    _ItemsToBuy.Add(new ItemsList(2003, "Health Potion", 35, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3132, "Heart of Gold", 350, 1028, 0));
	_ItemsToBuy.Add(new ItemsList(3155, "Hexdrinker", 550,1036, 1033));
	_ItemsToBuy.Add(new ItemsList(3146, "Hextech Gunblade", 275,3144, 3145));
	_ItemsToBuy.Add(new ItemsList(3145, "Hextech Revolver", 330,1052, 1052));
	_ItemsToBuy.Add(new ItemsList(3187, "Hextech Sweeper", 200,1052, 1052));
    _ItemsToBuy.Add(new ItemsList(1039, "Hunter's Machete", 300, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3025, "Iceborn Gauntlet", 640,3057, 3024));
    _ItemsToBuy.Add(new ItemsList(2048, "Ichor of Illumination", 500, 0, 0));
    _ItemsToBuy.Add(new ItemsList(2040, "Ichor of Rage", 500, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3031, "Infinity Edge", 645,1038, 1037));
    _ItemsToBuy.Add(new ItemsList(3158, "Ionian Boots of Lucidity", 700, 1001, 0));
    _ItemsToBuy.Add(new ItemsList(3279, "Ionian Boots of Lucidity - Enchantment: Alacrity", 475, 3158, 0));
    _ItemsToBuy.Add(new ItemsList(3276, "Ionian Boots of Lucidity - Enchantment: Captain", 750, 3158, 0));
    _ItemsToBuy.Add(new ItemsList(3278, "Ionian Boots of Lucidity - Enchantment: Distortion", 475, 3158, 0));
    _ItemsToBuy.Add(new ItemsList(3277, "Ionian Boots of Lucidity - Enchantment: Furor", 650, 3158, 0));
    _ItemsToBuy.Add(new ItemsList(3275, "Ionian Boots of Lucidity - Enchantment: Homeguard", 475, 3158, 0));
    _ItemsToBuy.Add(new ItemsList(3098, "Kage's Lucky Pick", 330, 1052, 0));
    _ItemsToBuy.Add(new ItemsList(3175, "Kha'Zix Head", 800, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3067, "Kindlegem", 375, 1028, 0));
	_ItemsToBuy.Add(new ItemsList(3186, "Kitae's Bloodrazor", 700,1037, 1043));
	_ItemsToBuy.Add(new ItemsList(3035, "Last Whisper", 860,1037, 1036));
	_ItemsToBuy.Add(new ItemsList(3151, "Liandry's Torment", 980,3136, 1052));
	_ItemsToBuy.Add(new ItemsList(3100, "Lich Bane", 880,3057, 1026));
	_ItemsToBuy.Add(new ItemsList(3190, "Locket of the Iron Solari", 670,3067, 1029));
    _ItemsToBuy.Add(new ItemsList(1036, "Long Sword", 400, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3104, "Lord Van Damm's Pillager", 800,3132, 3134));
	//_ItemsToBuy.Add(new ItemsList(3106] =100,1029, 1039));
	_ItemsToBuy.Add(new ItemsList(3114, "Malady", 800,1042, 1042));
	_ItemsToBuy.Add(new ItemsList(3037, "Mana Manipulator", 40,1004, 1004));
    _ItemsToBuy.Add(new ItemsList(2004, "Mana Potion", 35, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3004, "Manamune", 1000,3070, 1036));
	_ItemsToBuy.Add(new ItemsList(3156, "Maw of Malmortius", 975,3155, 1037));
    _ItemsToBuy.Add(new ItemsList(3041, "Mejai's Soulstealer", 800, 1052, 0));
	_ItemsToBuy.Add(new ItemsList(3139, "Mercurial Scimitar", 600,3140, 1038));
	_ItemsToBuy.Add(new ItemsList(3111, "Mercury's Treads", 450,1001, 1033));
    _ItemsToBuy.Add(new ItemsList(3269, "Mercury's Treads - Enchantment: Alacrity", 475, 3111, 0));
    _ItemsToBuy.Add(new ItemsList(3266, "Mercury's Treads - Enchantment: Captain", 750, 3111, 0));
    _ItemsToBuy.Add(new ItemsList(3268, "Mercury's Treads - Enchantment: Distortion", 475, 3111, 0));
    _ItemsToBuy.Add(new ItemsList(3267, "Mercury's Treads - Enchantment: Furor", 650, 3111, 0));
    _ItemsToBuy.Add(new ItemsList(3265, "Mercury's Treads - Enchantment: Homeguard", 475, 3111, 0));
	_ItemsToBuy.Add(new ItemsList(3222, "Mikael's Crucible", 920,1027, 3028));
	_ItemsToBuy.Add(new ItemsList(2009, "Mini Pot", 0, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3170, "Moonflair Spellblade", 340, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3165, "Morello's Evil Tome", 435,3098, 3108));
	_ItemsToBuy.Add(new ItemsList(3042, "Muramana", 2100, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3115, "Nashor's Tooth", 250,3101, 3108));
    _ItemsToBuy.Add(new ItemsList(1058, "Needlessly Large Rod", 1600, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1057, "Negatron Cloak", 810, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3047, "Ninja Tabi", 350,1001, 1029));
    _ItemsToBuy.Add(new ItemsList(3264, "Ninja Tabi - Enchantment: Alacrity", 475, 3047, 0));
    _ItemsToBuy.Add(new ItemsList(3261, "Ninja Tabi - Enchantment: Captain", 750, 3047, 0));
    _ItemsToBuy.Add(new ItemsList(3263, "Ninja Tabi - Enchantment: Distortion", 475, 3047, 0));
    _ItemsToBuy.Add(new ItemsList(3262, "Ninja Tabi - Enchantment: Furor", 650, 3047, 0));
    _ItemsToBuy.Add(new ItemsList(3260, "Ninja Tabi - Enchantment: Homeguard", 475, 3047, 0));
    _ItemsToBuy.Add(new ItemsList(1033, "Null-Magic Mantle", 400, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3180, "Odyn's Veil", 600,1057, 3010));
	_ItemsToBuy.Add(new ItemsList(3056, "Ohmwrecker", 930,3010, 1031));
    _ItemsToBuy.Add(new ItemsList(2042, "Oracle's Elixir", 400, 0, 0));
    _ItemsToBuy.Add(new ItemsList(2047, "Oracle's Extract", 250, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3084, "Overlord's Bloodmail", 980,1011, 1028));
	_ItemsToBuy.Add(new ItemsList(3044, "Phage", 590,1028, 1036));
	_ItemsToBuy.Add(new ItemsList(3046, "Phantom Dancer", 495,1018, 3086));
	_ItemsToBuy.Add(new ItemsList(3096, "Philosopher's Stone", 340,1004, 1006));
    _ItemsToBuy.Add(new ItemsList(1037, "Pickaxe", 875, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1062, "Prospector's Blade", 950, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1063, "Prospector's Ring", 950, 0, 0));
    _ItemsToBuy.Add(new ItemsList(3140, "Quicksilver Sash", 850, 1057, 0));
	_ItemsToBuy.Add(new ItemsList(3089, "Rabadon's Deathcap", 740,1026, 1058));
	_ItemsToBuy.Add(new ItemsList(3143, "Randuin's Omen", 1000,1011, 3082));
	_ItemsToBuy.Add(new ItemsList(3074, "Ravenous Hydra", 400,3077, 1053));
    _ItemsToBuy.Add(new ItemsList(1043, "Recurve Bow", 950, 0, 0));
    _ItemsToBuy.Add(new ItemsList(1006, "Rejuvenation Bead", 180, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3029, "Rod of Ages", 740,3010, 1026));
	_ItemsToBuy.Add(new ItemsList(1028, "Ruby Crystal", 475, 0, 0));
	_ItemsToBuy.Add(new ItemsList(2045, "Ruby Sightstone", 125,2049, 1028));
	_ItemsToBuy.Add(new ItemsList(3085, "Runaan's Hurricane", 1000,1042, 1043));
	_ItemsToBuy.Add(new ItemsList(3107, "Runic Bulwark", 650,1033, 3105));
	_ItemsToBuy.Add(new ItemsList(3116, "Rylai's Crystal Scepter", 605,1026, 1052));
	_ItemsToBuy.Add(new ItemsList(3181, "Sanguine Blade", 800,1038, 1053));
	_ItemsToBuy.Add(new ItemsList(1027, "Sapphire Crystal", 400, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3040, "Seraph's Embrace", 2710, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3092, "Shard of True Ice", 535,3098, 3037));
	_ItemsToBuy.Add(new ItemsList(3057, "Sheen", 425,1027, 1052));
	_ItemsToBuy.Add(new ItemsList(3069, "Shurelya's Reverie", 550,3067, 3096));
	_ItemsToBuy.Add(new ItemsList(2044, "Sight Ward", 75, 0, 0));
	_ItemsToBuy.Add(new ItemsList(2049, "Sightstone", 700, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3020, "Sorcerer's Shoes", 750,1001,0));
	_ItemsToBuy.Add(new ItemsList(3259, "Sorcerer's Shoes - Enchantment: Alacrity", 475,3020, 0));
	_ItemsToBuy.Add(new ItemsList(3256, "Sorcerer's Shoes - Enchantment: Captain", 750,3020, 0));
	_ItemsToBuy.Add(new ItemsList(3258, "Sorcerer's Shoes - Enchantment: Distortion", 475,3020, 0));
	_ItemsToBuy.Add(new ItemsList(3257, "Sorcerer's Shoes - Enchantment: Furor", 650,3020, 0));
	_ItemsToBuy.Add(new ItemsList(3255, "Sorcerer's Shoes - Enchantment: Homeguard", 475,3020, 0));
	_ItemsToBuy.Add(new ItemsList(3207, "Spirit of the Ancient Golem", 600,1080, 1011));
	_ItemsToBuy.Add(new ItemsList(3209, "Spirit of the Elder Lizard", 725,1080, 1037));
	_ItemsToBuy.Add(new ItemsList(3206, "Spirit of the Spectral Wraith", 400,1080, 3145));
	_ItemsToBuy.Add(new ItemsList(1080, "Spirit Stone", 140,1039, 1004));
	_ItemsToBuy.Add(new ItemsList(3065, "Spirit Visage", 540,3067, 1057));
	_ItemsToBuy.Add(new ItemsList(3087, "Statikk Shiv", 525,3086, 3093));
	_ItemsToBuy.Add(new ItemsList(3101, "Stinger", 450,1042, 1042));
	_ItemsToBuy.Add(new ItemsList(3068, "Sunfire Cape", 780,1031, 1011));
	_ItemsToBuy.Add(new ItemsList(3131, "Sword of the Divine", 850,1043, 1042));
	_ItemsToBuy.Add(new ItemsList(3141, "Sword of the Occult", 800,1036, 0));
	_ItemsToBuy.Add(new ItemsList(3073, "Tear of the Goddess", 120,1027, 1004));
	_ItemsToBuy.Add(new ItemsList(3071, "The Black Cleaver", 1188,3134, 1028));
	_ItemsToBuy.Add(new ItemsList(3072, "The Bloodthirster", 650,1038, 1053));
	_ItemsToBuy.Add(new ItemsList(3134, "The Brutalizer", 537,1036, 1036));
	_ItemsToBuy.Add(new ItemsList(3200, "The Hex Core", 0, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3185, "The Lightbringer", 245,1043, 1036));
	_ItemsToBuy.Add(new ItemsList(3075, "Thornmail", 1180,1029, 1031));
	_ItemsToBuy.Add(new ItemsList(3077, "Tiamat", 665,1037, 1036));
	_ItemsToBuy.Add(new ItemsList(3078, "Trinity Force", 300,3086, 3057));
	_ItemsToBuy.Add(new ItemsList(3023, "Twin Shadows", 735,3098, 1033));
	_ItemsToBuy.Add(new ItemsList(1053, "Vampiric Scepter", 400,1036, 0));
	_ItemsToBuy.Add(new ItemsList(2043, "Vision Ward", 125, 0, 0));
	_ItemsToBuy.Add(new ItemsList(3135, "Void Staff", 1000,1026, 1052));
	_ItemsToBuy.Add(new ItemsList(3082, "Warden's Mail", 500,1029, 1029));
	_ItemsToBuy.Add(new ItemsList(3083, "Warmog's Armor", 995,1011, 1028));
	_ItemsToBuy.Add(new ItemsList(3122, "Wicked Hatchet", 710,1018, 1036));
	_ItemsToBuy.Add(new ItemsList(3152, "Will of the Ancients", 440,3108, 3145));
	_ItemsToBuy.Add(new ItemsList(3091, "Wit's End", 850,1043, 1033));
	_ItemsToBuy.Add(new ItemsList(3090, "Wooglet's Witchcap", 1920,1031, 1026));
	//_ItemsToBuy.Add(new ItemsList(3154] =100,3106, 1053));
	_ItemsToBuy.Add(new ItemsList(3142, "Youmuu's Ghostblade", 563,3093, 3134));
	_ItemsToBuy.Add(new ItemsList(3086, "Zeal", 375,1051, 1042));
	_ItemsToBuy.Add(new ItemsList(3050, "Zeke's Herald", 800,3067, 1053));
	_ItemsToBuy.Add(new ItemsList(3157, "Zhonya's Hourglass", 780,1031, 1058));
#endregion
            
        }


        private static void InitializeBuilds()
        {
            _Builds = new List<BuildsList>();
            Console.WriteLine("initialize builds");
          #region Ahri
if (ObjectManager.Player.BaseSkinName == "Ahri")
          {
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Morello's Evil Tome"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion
#region Akali
if (ObjectManager.Player.BaseSkinName == "Akali")
          {
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Lich Bane"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion   
#region Alistar
	if (ObjectManager.Player.BaseSkinName == "Alistar")
          {
_Builds.Add(new BuildsList("Ninja Tabi"));
_Builds.Add(new BuildsList("Guinsoo's Rageblade"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
}
#endregion
#region Amumu
	if (ObjectManager.Player.BaseSkinName == "Amumu")
          {
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Boots of Swiftness"));
_Builds.Add(new BuildsList("Sunfire Cape"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion
#region Anivia
	if (ObjectManager.Player.BaseSkinName == "Anivia")
          {
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Will of the Ancients"));
}
#endregion
#region Annie
	if (ObjectManager.Player.BaseSkinName == "Annie")
          {
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion
#region Ashe
	if (ObjectManager.Player.BaseSkinName == "Ashe")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Last Whisper"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion
#region Blitzcrank
	if (ObjectManager.Player.BaseSkinName == "Blitzcrank")
          {
_Builds.Add(new BuildsList("Sapphire Crystal"));
_Builds.Add(new BuildsList("Tear of the Goddess"));
_Builds.Add(new BuildsList("Sapphire Crystal"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Manamune"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion
#region Brand
	if (ObjectManager.Player.BaseSkinName == "Brand")
          {
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Will of the Ancients"));
}
#endregion
#region Caitlyn
if (ObjectManager.Player.BaseSkinName == "Caitlyn")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Last Whisper"));
}
#endregion
#region Cassiopeia
if (ObjectManager.Player.BaseSkinName == "Caitlyn")
          {
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Will of the Ancients"));
}
#endregion
#region Chogath
if (ObjectManager.Player.BaseSkinName == "Chogath")
          {
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
}
#endregion
#region Corki
if (ObjectManager.Player.BaseSkinName == "Corki")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Last Whisper"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion
#region DrMundo
if (ObjectManager.Player.BaseSkinName == "DrMundo")
          {
_Builds.Add(new BuildsList("Doran's Shield"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Avarice Blade"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Zeke's Herald"));
_Builds.Add(new BuildsList("Youmuu's Ghostblade"));
}
#endregion
	#region Darius
if (ObjectManager.Player.BaseSkinName == "Darius")
          {
_Builds.Add(new BuildsList("Prospector's Blade"));
_Builds.Add(new BuildsList("Boots of Speed"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Maw of Malmortius"));
_Builds.Add(new BuildsList("Runic Bulwark"));
        }
#endregion
    #region Evelynn
if (ObjectManager.Player.BaseSkinName == "Evelynn")
          {
_Builds.Add(new BuildsList("Boots of Mobility"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Pickaxe"));
_Builds.Add(new BuildsList("Guinsoo's Rageblade"));
_Builds.Add(new BuildsList("Guardian Angel"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion
    #region Ezreal
if (ObjectManager.Player.BaseSkinName == "Ezreal")
          {
_Builds.Add(new BuildsList("Sapphire Crystal"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Madred's Bloodrazor"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion
    #region FiddleSticks
if (ObjectManager.Player.BaseSkinName == "FiddleSticks")
          {
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Will of the Ancients"));
}
#endregion
    #region Fiora
if (ObjectManager.Player.BaseSkinName == "Fiora")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Frozen Mallet"));
}
#endregion
    #region Fizz
if (ObjectManager.Player.BaseSkinName == "Fizz")
          {
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Lich Bane"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Deathfire Grasp"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion
   #region Galio
if (ObjectManager.Player.BaseSkinName == "Galio")
          {
_Builds.Add(new BuildsList("Null-Magic Mantle"));
_Builds.Add(new BuildsList("Chalice of Harmony"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Aegis of the Legion"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Thornmail"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
     #endregion
    #region Gangplank
if (ObjectManager.Player.BaseSkinName == "Gangplank")
          {
_Builds.Add(new BuildsList("Brawler's Gloves"));
_Builds.Add(new BuildsList("Avarice Blade"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Cloak of Agility"));
_Builds.Add(new BuildsList("Pickaxe"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Last Whisper"));
}
#endregion
    #region Garen
if (ObjectManager.Player.BaseSkinName == "Garen")
          {
_Builds.Add(new BuildsList("Prospector's Blade"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Maw of Malmortius"));
_Builds.Add(new BuildsList("Runic Bulwark"));
}
#endregion
    #region Gragas
if (ObjectManager.Player.BaseSkinName == "Gragas")
          {
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Lich Bane"));
}
#endregion
    #region Graves
if (ObjectManager.Player.BaseSkinName == "Graves")
          {
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("The Black Cleaver"));
}
#endregion
    #region Heimerdinger
if (ObjectManager.Player.BaseSkinName == "Heimerdinger")
          {
_Builds.Add(new BuildsList("Meki Pendant"));
_Builds.Add(new BuildsList("Tear of the Goddess"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Archangel's Staff"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Will of the Ancients"));
}
#endregion
    #region Irelia
if (ObjectManager.Player.BaseSkinName == "Irelia")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Mercury's Treads's"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Warden's Mail"));
_Builds.Add(new BuildsList("Randuin's Omen"));
}
#endregion
    #region JarvanIV
if (ObjectManager.Player.BaseSkinName == "JarvanIV")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Null-Magic Mantle"));
_Builds.Add(new BuildsList("Aegis of the Legion"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion
    #region Jax
if (ObjectManager.Player.BaseSkinName == "Jax")
          {
_Builds.Add(new BuildsList("Ninja Tabi"));
_Builds.Add(new BuildsList("Guinsoo's Rageblade"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
}
#endregion    
    #region Karma
if (ObjectManager.Player.BaseSkinName == "Karma")
          {
_Builds.Add(new BuildsList("Sapphire Crystal"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
}
#endregion
    #region Karthus
if (ObjectManager.Player.BaseSkinName == "Karthus")
          {
_Builds.Add(new BuildsList("Sapphire Crystal"));
_Builds.Add(new BuildsList("Tear of the Goddess"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Archangel's Staff"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion
    #region Kassadin
if (ObjectManager.Player.BaseSkinName == "Kassadin")
          {
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
}
#endregion     
    #region Katarina
if (ObjectManager.Player.BaseSkinName == "Katarina")
          {
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion
      
    #region Kayle
if (ObjectManager.Player.BaseSkinName == "Kayle")
          {
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Guinsoo's Rageblade"));
_Builds.Add(new BuildsList("Nashor's Tooth"));
_Builds.Add(new BuildsList("Madred's Bloodrazor"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
}
#endregion

    #region Kennen
if (ObjectManager.Player.BaseSkinName == "Kennen")
          {
_Builds.Add(new BuildsList("Doran's Shield"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
}
#endregion

    #region KogMaw
if (ObjectManager.Player.BaseSkinName == "KogMaw")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Executioner's Calling"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion
      
    #region LeBlanc
if (ObjectManager.Player.BaseSkinName == "LeBlanc")
          {
_Builds.Add(new BuildsList("Amplifying Tome"));
_Builds.Add(new BuildsList("Mejai's Soulstealer"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Archangel's Staff"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion

    #region LeeSin
if (ObjectManager.Player.BaseSkinName == "LeeSin")
          {
_Builds.Add(new BuildsList("Cloth Armor"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion

    #region Leona
if (ObjectManager.Player.BaseSkinName == "Leona")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Shurelya's Reverie"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Warmog's Armor"));
}
#endregion

    #region Lux
if (ObjectManager.Player.BaseSkinName == "Lux")
          {
_Builds.Add(new BuildsList("Kage's Lucky Pick"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Lich Bane"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Morello's Evil Tome"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion
    
    #region Malphite
if (ObjectManager.Player.BaseSkinName == "Malphite")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion

    #region Malzahar
if (ObjectManager.Player.BaseSkinName == "Malzahar")
          {
_Builds.Add(new BuildsList("Tear of the Goddess"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Archangel's Staff"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion
      
    #region Maokai
if (ObjectManager.Player.BaseSkinName == "Maokai")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Boots of Swiftness"));
_Builds.Add(new BuildsList("Eleisa's Miracle"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Locket of the Iron Solari"));
}
#endregion
    #region MasterYi
if (ObjectManager.Player.BaseSkinName == "MasterYi")
          {
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion


    #region Mordekaiser
if (ObjectManager.Player.BaseSkinName == "Mordekaiser")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Lich Bane"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion

    #region Morgana
if (ObjectManager.Player.BaseSkinName == "Morgana")
          {
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion

    #region Nasus
if (ObjectManager.Player.BaseSkinName == "Nasus")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Sunfire Cape"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Frozen Heart"));
}
#endregion
    #region Nautilus
if (ObjectManager.Player.BaseSkinName == "Nautilus")
          {
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Thornmail"));
}
#endregion

    #region Nidalee
if (ObjectManager.Player.BaseSkinName == "Nidalee")
          {
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Youmuu's Ghostblade"));
_Builds.Add(new BuildsList("Last Whisper"));
}
#endregion
    #region Nocturne
if (ObjectManager.Player.BaseSkinName == "Nocturne")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Youmuu's Ghostblade"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion

    #region Nunu
if (ObjectManager.Player.BaseSkinName == "Nunu")
          {
_Builds.Add(new BuildsList( "Blasting Wand"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Amplifying Tome"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Frozen Heart"));
}
#endregion
       
    #region Olaf
if (ObjectManager.Player.BaseSkinName == "Olaf")
          {
_Builds.Add(new BuildsList("Ruby Crystal"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion
       
    #region Orianna
if (ObjectManager.Player.BaseSkinName == "Orianna")
          {
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Boots of Mobility"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Nashor's Tooth"));
_Builds.Add(new BuildsList("Lich Bane"));
}
#endregion
       
    #region Pantheon
if (ObjectManager.Player.BaseSkinName == "Pantheon")
          {
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Youmuu's Ghostblade"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion

    #region Poppy
if (ObjectManager.Player.BaseSkinName == "Poppy")
          {
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion

    #region Rammus
if (ObjectManager.Player.BaseSkinName == "Rammus")
          {
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Guardian Angel"));
_Builds.Add(new BuildsList("Sunfire Cape"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Thornmail"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion
    
    #region Renekton
if (ObjectManager.Player.BaseSkinName == "Renekton")
          {
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Last Whisper"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion
      
    #region Riven
if (ObjectManager.Player.BaseSkinName == "Riven")
          {
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Last Whisper"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion

    #region Rumble
if (ObjectManager.Player.BaseSkinName == "Rumble")
          {
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Sunfire Cape"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
}
#endregion

    #region Ryze
if (ObjectManager.Player.BaseSkinName == "Ryze")
          {
_Builds.Add(new BuildsList("Tear of the Goddess"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Glacial Shroud"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Archangel's Staff"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Spirit Visage"));
_Builds.Add(new BuildsList("Frozen Heart"));
}
#endregion
       
    #region Sejuani
if (ObjectManager.Player.BaseSkinName == "Sejuani")
          {
_Builds.Add(new BuildsList("Cloth Armor"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Warden's Mail"));
_Builds.Add(new BuildsList("Shurelya's Reverie"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Frozen Mallet"));
}
#endregion
    #region Shaco
if (ObjectManager.Player.BaseSkinName == "Shaco")
          {
_Builds.Add(new BuildsList("Boots of Mobility"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Guardian Angel"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion

    #region Shen
if (ObjectManager.Player.BaseSkinName == "Shen")
          {
_Builds.Add(new BuildsList("Ruby Crystal"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Sunfire Cape"));
_Builds.Add(new BuildsList("Spirit Visage"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
}
#endregion

    #region Shyvana
if (ObjectManager.Player.BaseSkinName == "Shyvana")
          {
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Recurve Bow"));
_Builds.Add(new BuildsList("Wit's End"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Sunfire Cape"));
_Builds.Add(new BuildsList("Madred's Bloodrazor"));
}
#endregion
     
    #region Singed
if (ObjectManager.Player.BaseSkinName == "Singed")
          {
_Builds.Add(new BuildsList("Sapphire Crystal"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Guardian Angel"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
}
#endregion

    #region Sion
if (ObjectManager.Player.BaseSkinName == "Sion")
          {
_Builds.Add(new BuildsList("Prospector's Blade"));
_Builds.Add(new BuildsList("Boots of Speed"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Last Whisper"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("The Black Cleaver"));
}
#endregion

    #region Sivir
if (ObjectManager.Player.BaseSkinName == "Sivir")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Pickaxe"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Thornmail"));
_Builds.Add(new BuildsList("Last Whisper"));
}
#endregion

    #region Skarner
if (ObjectManager.Player.BaseSkinName == "Skarner")
          {
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Wit's End"));
_Builds.Add(new BuildsList("Guardian Angel"));
_Builds.Add(new BuildsList("Hextech Gunblade"));
}
#endregion

    #region Sona
if (ObjectManager.Player.BaseSkinName == "Sona")
          {
_Builds.Add(new BuildsList("Faerie Charm"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Aegis of the Legion"));
_Builds.Add(new BuildsList("Shurelya's Reverie"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Randuin's Omen"));
}
#endregion

    #region Soraka
if (ObjectManager.Player.BaseSkinName == "Soraka")
          {
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
}
#endregion
    #region Swain
if (ObjectManager.Player.BaseSkinName == "Swain")
          {
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Amplifying Tome"));
_Builds.Add(new BuildsList("Amplifying Tome"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
}
#endregion
       
    #region Talon
if (ObjectManager.Player.BaseSkinName == "Talon")
          {
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Long Sword"));
_Builds.Add(new BuildsList("Long Sword"));
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Youmuu's Ghostblade"));
_Builds.Add(new BuildsList("Pickaxe"));
_Builds.Add(new BuildsList("Cloak of Agility"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Frozen Mallet"));
}
#endregion

    #region Taric
if (ObjectManager.Player.BaseSkinName == "Taric")
          {
_Builds.Add(new BuildsList("Faerie Charm"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Aegis of the Legion"));
_Builds.Add(new BuildsList("Shurelya's Reverie"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Frozen Heart"));
}
#endregion
    #region Teemo
if (ObjectManager.Player.BaseSkinName == "Teemo")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Malady"));
_Builds.Add(new BuildsList("Recurve Bow"));
_Builds.Add(new BuildsList("Wit's End"));
_Builds.Add(new BuildsList("Recurve Bow"));
_Builds.Add(new BuildsList("Madred's Bloodrazor"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Guardian's Angel"));
}
#endregion
     
    #region Tristana
if (ObjectManager.Player.BaseSkinName == "Tristana")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Last Whisper"));
_Builds.Add(new BuildsList("Quicksilver Sash"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion

    #region Trundle
if (ObjectManager.Player.BaseSkinName == "Trundle")
          {
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion
     
    #region Tryndamere
if (ObjectManager.Player.BaseSkinName == "Tryndamere")
          {
_Builds.Add(new BuildsList("Cloth Armor"));
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion
       
    #region TwistedFate
if (ObjectManager.Player.BaseSkinName == "TwistedFate")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion

    #region Twitch
if (ObjectManager.Player.BaseSkinName == "Twitch")
          {
_Builds.Add(new BuildsList("Boots of Mobility"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion

    #region Udyr
if (ObjectManager.Player.BaseSkinName == "Udyr")
          {
_Builds.Add(new BuildsList("Boots of Mobility"));
_Builds.Add(new BuildsList("Wit's End"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Madred's Bloodrazor"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion

    #region Urgot
if (ObjectManager.Player.BaseSkinName == "Urgot")
          {
_Builds.Add(new BuildsList("Meki Pendant"));
_Builds.Add(new BuildsList("Tear of the Goddess"));
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Manamune"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Last Whisper"));
_Builds.Add(new BuildsList("Frozen Heart"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion
  
    #region Varus
if (ObjectManager.Player.BaseSkinName == "Varus")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("Pickaxe"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Banshee's Veil"));
_Builds.Add(new BuildsList("Last Whisper"));
}
#endregion

    #region Vayne
if (ObjectManager.Player.BaseSkinName == "Vayne")
          {
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("Infinity Edge"));
_Builds.Add(new BuildsList("Guardian Angel"));
}
#endregion

    #region Veigar
if (ObjectManager.Player.BaseSkinName == "Veigar")
          {
_Builds.Add(new BuildsList("Kage's Lucky Pick"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Deathfire Grasp"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion

    #region Viktor
if (ObjectManager.Player.BaseSkinName == "Viktor")
          {
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Augment: Gravity"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion
       
    #region Vladimir
if (ObjectManager.Player.BaseSkinName == "Vladimir")
          {
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Lich Bane"));
}
#endregion
    #region Volibear
if (ObjectManager.Player.BaseSkinName == "Volibear")
          {
_Builds.Add(new BuildsList("Regrowth Pendant"));
_Builds.Add(new BuildsList("Philosopher's Stone"));
_Builds.Add(new BuildsList("Doran's Shield"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Kindlegem"));
_Builds.Add(new BuildsList("Spirit Visage"));
_Builds.Add(new BuildsList("Catalyst the Protector"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Banshee's Veil"));
}
#endregion

    #region Warwick
if (ObjectManager.Player.BaseSkinName == "Warwick")
          {
_Builds.Add(new BuildsList("Kindlegem"));
_Builds.Add(new BuildsList("Chalice of Harmony"));
_Builds.Add(new BuildsList("Spirit Visage"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Wit's End"));
_Builds.Add(new BuildsList("Sunfire Cape"));
_Builds.Add(new BuildsList("Madred's Bloodrazor"));
}
#endregion
     
    #region MonkeyKing
if (ObjectManager.Player.BaseSkinName == "MonkeyKing")
          {
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Phage"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Youmuu's Ghostblade"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion

    #region Vi
if (ObjectManager.Player.BaseSkinName == "Vi")
          {
_Builds.Add(new BuildsList("Prospector's Blade"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Randuin's Omen"));
_Builds.Add(new BuildsList("Maw of Malmortius"));
_Builds.Add(new BuildsList("Runic Bulwark"));
}
#endregion
	#region Xerath
if (ObjectManager.Player.BaseSkinName == "Xerath")
          {
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Giant's Belt"));
_Builds.Add(new BuildsList("Rylai's Crystal Scepter"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Chain Vest"));
_Builds.Add(new BuildsList("Negatron Cloak"));
_Builds.Add(new BuildsList("Abyssal Scepter"));
_Builds.Add(new BuildsList("Blasting Wand"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion

    #region XinZhao
if (ObjectManager.Player.BaseSkinName == "XinZhao")
          {
_Builds.Add(new BuildsList("Prospector's Blade"));
_Builds.Add(new BuildsList("Berserker's Greaves"));
_Builds.Add(new BuildsList("The Brutalizer"));
_Builds.Add(new BuildsList("Phantom Dancer"));
_Builds.Add(new BuildsList("The Black Cleaver"));
_Builds.Add(new BuildsList("Sheen"));
_Builds.Add(new BuildsList("Trinity Force"));
_Builds.Add(new BuildsList("Sanguine Blade"));
_Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion

    #region Yorick
if (ObjectManager.Player.BaseSkinName == "Yorick")
          {
_Builds.Add(new BuildsList("Meki Pendant"));
_Builds.Add(new BuildsList("Tear of the Goddess"));
_Builds.Add(new BuildsList("Manamune"));
_Builds.Add(new BuildsList("Mercury's Treads"));
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Frozen Mallet"));
_Builds.Add(new BuildsList("Sanguine Blade"));
}
#endregion

    #region Ziggs
if (ObjectManager.Player.BaseSkinName == "Ziggs")
          {
_Builds.Add(new BuildsList("Sorcerer's Shoes"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Hextech Revolver"));
_Builds.Add(new BuildsList("Will of the Ancients"));
_Builds.Add(new BuildsList("Void Staff"));
_Builds.Add(new BuildsList("Lich Bane"));
}
#endregion

    #region Zilean
if (ObjectManager.Player.BaseSkinName == "Zilean")
          {
_Builds.Add(new BuildsList("Ionian Boots of Lucidity"));
_Builds.Add(new BuildsList("Rod of Ages"));
_Builds.Add(new BuildsList("Morello's Evil Tome"));
_Builds.Add(new BuildsList("Wooglet's Witchcap"));
_Builds.Add(new BuildsList("Void Staff"));
}
#endregion
       
#region unknownchamp
if (_Builds == null)
{
    _Builds.Add(new BuildsList("Mercury's Treads"));
    _Builds.Add(new BuildsList("Phage"));
    _Builds.Add(new BuildsList("Sheen"));
    _Builds.Add(new BuildsList("Trinity Force"));
    _Builds.Add(new BuildsList("The Brutalizer"));
    _Builds.Add(new BuildsList("Sanguine Blade"));
    _Builds.Add(new BuildsList("Youmuu's Ghostblade"));
    _Builds.Add(new BuildsList("Infinity Edge"));
}
#endregion

        }
    }
}
