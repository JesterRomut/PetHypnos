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
    public class AergiaNeuronLightProjectile : BaseAergiaNeuronProjectile
    {
        public override int MasterTypeID => ModContent.ProjectileType<HypnosLightPetProjectile>();

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.LightPet[Projectile.type] = true;
        }
    }
    public class HypnosLightPetProjectile : BaseHypnosPetProjectile
    {
        public override int AergiaID => ModContent.ProjectileType<AergiaNeuronLightProjectile>();
        public override int BuffID => ModContent.BuffType<HypnosLightPetBuff>();
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.LightPet[Projectile.type] = true;
        }

        
    }

    public class HypnosLightPetBuff : BaseHypnosPetBuff
    {

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("Neurofunk");
            Main.lightPet[Type] = true;
        }

        public override int ProjectileTypeID => ModContent.ProjectileType<HypnosLightPetProjectile>();
        

        public override string BuffDesc => "Hypnos will play its radio (with neurofunk) behind you";
        

    }

    public class HypnosLightPetItem : BaseHypnosPetItem
    {

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.DD2PetGhost);

            base.SetDefaults();

            Item.buffType = ModContent.BuffType<HypnosLightPetBuff>();
            Item.shoot = ModContent.ProjectileType<HypnosLightPetProjectile>();
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("Tape of Hypnodus"); //修普诺斯出实验室记之磁带
        }

        public override string FirstLine => "Full of neurofunks";

        public override string LastLine => "'what do you know what do you play what do you remember what do you love'";

    }
}
