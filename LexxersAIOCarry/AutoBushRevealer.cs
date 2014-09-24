using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace UltimateCarry
{
    class AutoBushRevealer
    {
	    static readonly List<KeyValuePair<int, String>> _wards = new List<KeyValuePair<int, String>> //insertion order
        {
            new KeyValuePair<int, String>(3340, "Warding Totem Trinket"),
            new KeyValuePair<int, String>(3361, "Greater Stealth Totem Trinket"),
            new KeyValuePair<int, String>(3205, "Quill Coat"),
            new KeyValuePair<int, String>(3207, "Spirit Of The Ancient Golem"),
            new KeyValuePair<int, String>(3154, "Wriggle's Lantern"),
            new KeyValuePair<int, String>(2049, "Sight Stone"),
            new KeyValuePair<int, String>(2045, "Ruby Sightstone"),
            new KeyValuePair<int, String>(3160, "Feral Flare"),
            new KeyValuePair<int, String>(2050, "Explorer's Ward"),
            new KeyValuePair<int, String>(2044, "Stealth Ward"),
        };
        
        int _lastTimeWarded;
	    readonly Menu _menu;

        public AutoBushRevealer()
        {
            _menu = Program.Menu.AddSubMenu(new Menu("Auto Bush Revealer", "AutoBushRevealer"));
            _menu.AddItem(new MenuItem("AutoBushEnabled", "Enabled").SetValue(true));
            _menu.AddItem(new MenuItem("AutoBushKey", "Key").SetValue(new KeyBind(Program.Menu.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press))); //32 == space

            var useWardsMenu = _menu.AddSubMenu(new Menu("Use Wards: ", "AutoBushUseWards"));

            foreach(var ward in _wards)
                useWardsMenu.AddItem(new MenuItem("AutoBush" + ward.Key, ward.Value).SetValue(true));

			Game.OnGameUpdate += Game_OnGameUpdate;
        }

        InventorySlot GetWardSlot()
        {
	        return _wards.Select(x => x.Key).Where(id => _menu.Item("AutoBush" + id).GetValue<bool>() && Items.CanUseItem(id)).Select(wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId) wardId)).FirstOrDefault();
        }

        static public InventorySlot GetAnyWardSlot()
        {
            return _wards.Select(x => x.Key).Where(Items.CanUseItem).Select(wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId)).FirstOrDefault();
        }

	    Obj_AI_Base GetNearObject(String name, Vector3 pos, int maxDistance)
        {
            return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.Name == name && x.Distance(pos) <= maxDistance);
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            int time = Environment.TickCount;

            if (_menu.Item("AutoBushEnabled").GetValue<bool>() && _menu.Item("AutoBushKey").GetValue<KeyBind>().Active)
			{
                foreach (Obj_AI_Hero enemy in Program.Helper.EnemyInfo.Where(x =>
					x.Player.IsValid &&
					!x.Player.IsVisible &&
					!x.Player.IsDead &&
					x.Player.Distance(ObjectManager.Player.ServerPosition) < 1000 &&
					time - x.LastSeen < 2500).Select(x => x.Player))
				{
					var bestWardPos = GetWardPos(enemy.ServerPosition, 165, 2);

					if(bestWardPos != enemy.ServerPosition && bestWardPos != Vector3.Zero && bestWardPos.Distance(ObjectManager.Player.ServerPosition) <= 600) 
					{
                        int timedif = Environment.TickCount - _lastTimeWarded;

                        if (timedif > 1250 && !(timedif < 2500 && GetNearObject("SightWard", bestWardPos, 200) != null)) //no near wards
                        {
                            var wardSlot = GetWardSlot();

                            if (wardSlot != null && wardSlot.Id != ItemId.Unknown)
                            {
                                wardSlot.UseItem(bestWardPos);
                                _lastTimeWarded = Environment.TickCount;
                            }
                        }
					}
				}
			}
        }

        Vector3 GetWardPos(Vector3 lastPos, int radius = 165, int precision = 3) //maybe reverse autobushward code from the bots?
        {
            //old: Vector3 wardPos = enemy.Position + Vector3.Normalize(enemy.Position - ObjectManager.Player.Position) * 150;

            var count = precision;

            while (count > 0)
            {
                var vertices = radius;

                var wardLocations = new WardLocation[vertices];
                var angle = 2 * Math.PI / vertices;

                for (var i = 0; i < vertices; i++)
                {
                    var th = angle * i;
                    var pos = new Vector3((float)(lastPos.X + radius * Math.Cos(th)), (float)(lastPos.Y + radius * Math.Sin(th)), 0);
                    wardLocations[i] = new WardLocation(pos, NavMesh.IsWallOfGrass(pos));
                }

                var grassLocations = new List<GrassLocation>();

                for (var i = 0; i < wardLocations.Length; i++)
                {
	                if (!wardLocations[i].Grass) continue;
	                if (i != 0 && wardLocations[i - 1].Grass)
		                grassLocations.Last().Count++;
	                else
		                grassLocations.Add(new GrassLocation(i, 1));
                }

	            var grassLocation = grassLocations.OrderByDescending(x => x.Count).FirstOrDefault();

                if (grassLocation != null) //else: no pos found. increase/decrease radius?
                {
                    var midelement = (int)Math.Ceiling(grassLocation.Count / 2f);
                    lastPos = wardLocations[grassLocation.Index + midelement - 1].Pos;
                    radius = (int)Math.Floor(radius / 2f);
                }

                count--;
            }

            return lastPos;
        }

        class WardLocation
        {
            public readonly Vector3 Pos;
            public readonly bool Grass;

            public WardLocation(Vector3 pos, bool grass)
            {
                Pos = pos;
                Grass = grass;
            }
        }

        class GrassLocation
        {
            public readonly int Index;
            public int Count;

            public GrassLocation(int index, int count)
            {
                Index = index;
                Count = count;
            }
        }
    }
}
