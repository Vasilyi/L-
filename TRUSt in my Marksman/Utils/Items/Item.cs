using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace TRUStinMarksman.Utils.Items
{
    public class Item
    {
        internal virtual int Id { get; set; }
        internal virtual string Name { get; set; }
        internal virtual float Range { get; set; }

        public bool IsActive
        {
            get { return LeagueSharp.Common.Items.CanUseItem(Id) && MenuItem.GetValue<bool>(); }
        }

        public MenuItem MenuItem { get; private set; }

        public Item CreateMenuItem(Menu parent)
        {
            MenuItem = parent.AddItem(new MenuItem(Name, "Use " + Name).SetValue(true));
            return this;
        }

        public virtual void OnUpdate()
        {

        }

        public virtual void Use()
        {
            
        }
    }
}