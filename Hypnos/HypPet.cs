using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace PetHypnos.Hypnos
{
    public class AergiaNeuronProjectile : BaseAergiaNeuronPetProjectile
    {
        public override int MasterTypeID => ModContent.ProjectileType<HypnosPetProjectile>();
    }

    public class HypnosPetProjectile : BaseHypnosPetProjectile
    {
        public override int AergiaID => ModContent.ProjectileType<AergiaNeuronProjectile>();
        public override int BuffID => ModContent.BuffType<HypnosPetBuff>();

        public override void SpecialStarKill()
        {
            Master.TogglePet();
            base.SpecialStarKill();
        }
    }

    public class HypnosPetBuff : BaseHypnosPetBuff
    {

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("City Pop");
            Main.vanityPet[((ModBuff)this).Type] = true;
            Description.SetDefault("Hypnos will play its radio (with h-pop and city pop) behind you");

            DisplayName.AddTranslation(7, "City Pop");
            Description.AddTranslation(7, "修普诺斯将会在你身后播放它的电台（和City pop）");
        }

        public override int ProjectileTypeID => ModContent.ProjectileType<HypnosPetProjectile>();


    }

    public class HypnosPetItem : BaseHypnosPetItem
    {

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish);

            base.SetDefaults();

            Item.buffType = ModContent.BuffType<HypnosPetBuff>();
            Item.shoot = ModContent.ProjectileType<HypnosPetProjectile>();
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("Tape of Hypnosis"); //修普诺斯创世纪之磁带

            Tooltip.SetDefault("Full of hypnagogic pops and city pops\nSummons the great✰ Hypnos' projection to charm you\nThe great✰ Hypnos comes with its Aergia Neurons\nWill somewhat be attracted by mouse\n'Let the bass kick.'");

            DisplayName.AddTranslation(7, "修普诺斯创世纪之磁带");
            Tooltip.AddTranslation(7, "塞满了H-pop和City pop\n召唤伟大的✰修普诺斯的投影来诱惑你\n伟大的✰修普诺斯将会和它的埃吉亚神经元一起过来\n略容易被鼠标吸引\n'Let the bass kick.'");
        }

    }
}
