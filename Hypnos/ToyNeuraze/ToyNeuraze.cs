using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace PetHypnos.Hypnos.ToyNeuraze
{
    public class ToyNeuraze: ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Toy Neuraze");
            base.Tooltip.SetDefault("Working in progress");
            base.SacrificeTotal = 1;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        public override void SetDefaults()
        {
            base.Item.damage = 3;
            base.Item.DamageType = DamageClass.Magic;
            base.Item.mana = 20;
            base.Item.width = 62;
            base.Item.height = 32;
            base.Item.useTime = (base.Item.useAnimation = 7);
            base.Item.useStyle = ItemUseStyleID.Shoot;
            base.Item.noMelee = true;
            base.Item.knockBack = 0f;
            base.Item.UseSound = SoundID.Item92;
            base.Item.autoReuse = true;
            
            //base.Item.shoot = ModContent.ProjectileType<SHPB>();
            base.Item.shootSpeed = 20f;
            Item.value = Item.buyPrice(0, 0, 0, 0);
            Item.rare = ModCompatibility.VioletOrPurple;
        }
    }
}
