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
    public class AergiaNeuronProjectile : BaseAergiaNeuronProjectile
    {
        public override int MasterTypeID => ModContent.ProjectileType<HypnosPetProjectile>();
    }

    public class HypnosPetProjectile : BaseHypnosPetProjectile
    {
        public override int AergiaID => ModContent.ProjectileType<AergiaNeuronProjectile>();
        public override int BuffID => ModContent.BuffType<HypnosPetBuff>();
        
    }

    public class HypnosPetBuff : BaseHypnosPetBuff
    {

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("City Pop");
            Main.vanityPet[((ModBuff)this).Type] = true;
        }

        public override int ProjectileTypeID => ModContent.ProjectileType<HypnosPetProjectile>();

        public override string BuffDesc => "Hypnos will play its radio (with h-pop and city pop) behind you";

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


        }

        public override string FirstLine => "Full of hypnagogic pops";

        public override string LastLine => "'Let the bass kick.'";

    }
}
