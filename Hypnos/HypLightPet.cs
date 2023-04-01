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
            Description.SetDefault("Hypnos will play its radio (with neurofunk) behind you");

            DisplayName.AddTranslation(7, "神经放克");
            Description.AddTranslation(7, "修普诺斯将会在你身后播放它的电台（和神经放克）");
        }

        public override int ProjectileTypeID => ModContent.ProjectileType<HypnosLightPetProjectile>();
        

        

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
            Tooltip.SetDefault("Full of neurofunks\nSummons the great✰ Hypnos' projection to charm you\nThe great✰ Hypnos comes with its Aergia Neurons\nWill somewhat be attracted by mouse\n'what do you know what do you play what do you remember what do you love'");
            
            DisplayName.AddTranslation(7, "修普诺斯出实验室记之磁带");
            Tooltip.AddTranslation(7, "塞满了神经放克\n召唤伟大的✰修普诺斯的投影来诱惑你\n伟大的✰修普诺斯将会和它的埃吉亚神经元一起过来\n略容易被鼠标吸引\n'你知道什么你扮演什么你记得什么你爱着什么'");
        }

    }
}
