using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ReLogic;
using Terraria;
using Terraria.ID;
using log4net;
using System;
using Microsoft.Xna.Framework;
using PetHypnos.Hypnos;

namespace PetHypnos
{
	public class PetHypnos : Mod
	{
        
	}

    public static class ModCompatibility
    {
        public static Mod CalamityMod
        {
            get
            {
                if (calamityMod == null)
                {
                    ModLoader.TryGetMod("CalamityMod", out calamityMod);
                }
                return calamityMod;
            }
        }
        private static Mod calamityMod;

        public static IModType CalamityModBloomRing
        {
            get
            {
                CalamityMod?.TryFind("BloomRing", out calamityModBloomRing);
                return calamityModBloomRing;
            }
        }
        private static IModType calamityModBloomRing;

        public static Mod Hypnos
        {
            get
            {
                if (hypnos == null)
                {
                    ModLoader.TryGetMod("Hypnos", out hypnos);
                }
                return hypnos;
            }
        }
        private static Mod hypnos;
    }


    public class PetHypnosRecipes: ModSystem
	{
        public override void AddRecipes()
        {
            int hypnosisID = ModContent.ItemType<HypnosPetItem>();
            int hypnodusID = ModContent.ItemType<HypnosLightPetItem>();

            Recipe.Create(hypnosisID).AddIngredient(hypnodusID).Register();

            Recipe.Create(hypnodusID).AddIngredient(hypnosisID).Register();
        }
    }

    public class PetHypnosGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.boss && (ModCompatibility.Hypnos?.TryFind("HypnosBoss", out ModNPC hypBoss) ?? false))
            {
                //object comp = typeof(Pawn).GetMethod("GetComp").MakeGenericMethod(ModCompatibility.PickUpAndHaul.CompHauledToInventory).Invoke(pawn, null);
                
                //int hypBossType = typeof(ModContent).GetMethod("NPCType").MakeGenericMethod(hypBoss).Invoke();
                int hypBossType = hypBoss.Type;
                // First, we need to check the npc.type to see if the code is running for the vanilla NPCwe want to change
                if (npc.type == hypBossType)
                {
                    // This is where we add item drop rules for VampireBat, here is a simple example:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HypnosPetItem>()));
                }
                // We can use other if statements here to adjust the drop rules of other vanilla NPC
            }

        }
    }

}