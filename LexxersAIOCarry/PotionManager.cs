using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
	internal class PotionManager
	{
		// Based on AutoPotions from Nikita Bernthaler
		private enum PotionType
		{
			Health,
			Mana
		};

		private class Potion
		{
			public string Name
			{
				get;
				set;
			}
			public int MinCharges
			{
				get;
				set;
			}
			public ItemId ItemId
			{
				get;
				set;
			}
			public int Priority
			{
				get;
				set;
			}
			public List<PotionType> TypeList
			{
				get;
				set;
			}
		}

		private List<Potion> _potions;

		public PotionManager()
		{
			_potions = new List<Potion>();
			_potions.Add(
				new Potion
				{
					Name = "ItemCrystalFlask",
					MinCharges = 1,
					ItemId = (ItemId)2041,
					Priority = 1,
					TypeList = new List<PotionType> { PotionType.Health, PotionType.Mana }
				});
			_potions.Add(
				new Potion
				{
					Name = "RegenerationPotion",
					MinCharges = 0,
					ItemId = (ItemId)2003,
					Priority = 2,
					TypeList = new List<PotionType> { PotionType.Health }
				});
			_potions.Add(
				new Potion
				{
					Name = "ItemMiniRegenPotion",
					MinCharges = 0,
					ItemId = (ItemId)2010,
					Priority = 4,
					TypeList = new List<PotionType> { PotionType.Health, PotionType.Mana }
				});
			_potions.Add(
				new Potion
				{
					Name = "FlaskOfCrystalWater",
					MinCharges = 0,
					ItemId = (ItemId)2004,
					Priority = 3,
					TypeList = new List<PotionType> { PotionType.Mana }
				});
			Start();
		}

		private void Start()
		{
			_potions = _potions.OrderBy(x => x.Priority).ToList();
			Program.Menu.AddSubMenu(new Menu("PotionManager", "PotionManager"));

			Program.Menu.SubMenu("PotionManager").AddSubMenu(new Menu("Health", "Health"));
			Program.Menu.SubMenu("PotionManager").SubMenu("Health").AddItem(new MenuItem("HealthPotion", "Use Health Potion").SetValue(true));
			Program.Menu.SubMenu("PotionManager").SubMenu("Health").AddItem(new MenuItem("HealthPercent", "HP Trigger Percent").SetValue(new Slider(30)));

			Program.Menu.SubMenu("PotionManager").AddSubMenu(new Menu("Mana", "Mana"));
			Program.Menu.SubMenu("PotionManager").SubMenu("Mana").AddItem(new MenuItem("ManaPotion", "Use Mana Potion").SetValue(true));
			Program.Menu.SubMenu("PotionManager").SubMenu("Mana").AddItem(new MenuItem("ManaPercent", "MP Trigger Percent").SetValue(new Slider(30)));

			Game.OnGameUpdate += OnGameUpdate;
		}

		private void OnGameUpdate(EventArgs args)
		{
			try
			{
				if(Program.Menu.Item("HealthPotion").GetValue<bool>())
				{
					if(GetPlayerHealthPercentage() <= Program.Menu.Item("HealthPercent").GetValue<Slider>().Value)
					{
						InventorySlot healthSlot = GetPotionSlot(PotionType.Health);
						if(!IsBuffActive(PotionType.Health))
							healthSlot.UseItem();
					}
				}
				if(Program.Menu.Item("ManaPotion").GetValue<bool>())
				{
					if(GetPlayerManaPercentage() <= Program.Menu.Item("ManaPercent").GetValue<Slider>().Value)
					{
						InventorySlot manaSlot = GetPotionSlot(PotionType.Mana);
						if(!IsBuffActive(PotionType.Mana))
							manaSlot.UseItem();
					}
				}
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch(Exception)
			{

			}
		}

		private InventorySlot GetPotionSlot(PotionType type)
		{
			return (from potion in _potions
					where potion.TypeList.Contains(type)
					from item in ObjectManager.Player.InventoryItems
					where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
					select item).FirstOrDefault();
		}

		private bool IsBuffActive(PotionType type)
		{
			return (from potion in _potions
					where potion.TypeList.Contains(type)
					from buff in ObjectManager.Player.Buffs
					where buff.Name == potion.Name && buff.IsActive
					select potion).Any();
		}

		internal float GetPlayerHealthPercentage()
		{
			return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth;
		}

		internal float GetPlayerManaPercentage()
		{
			return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;
		}
	}
}
