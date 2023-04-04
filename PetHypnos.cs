using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ReLogic;
using Terraria;
using Terraria.ID;
using log4net;
using System;
using Microsoft.Xna.Framework;
using PetHypnos.Hypnos;
using Terraria.GameInput;
using static Humanizer.In;
using System.IO;

namespace PetHypnos
{
	public class PetHypnos : Mod
	{
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PetHypnosMessageType msgType = (PetHypnosMessageType)reader.ReadByte();
            switch (msgType)
            {
                case PetHypnosMessageType.SyncMousePos:
                    Main.player[reader.ReadInt32()].GetModPlayer<PetHypnosPlayer>().HandleMousePos(reader);
                    break;
                case PetHypnosMessageType.SyncMouseRightClick:
                    Main.player[reader.ReadInt32()].GetModPlayer<PetHypnosPlayer>().HandleMouseRightClick(reader);
                    break;
            }
            }
    }

    public enum PetHypnosMessageType
    {
        SyncMousePos,
        SyncMouseRightClick
    }

    public static class PetHypnosExtensions
    {
        public static void SendPacket(this Player player, ModPacket packet, bool server)
        {
            if (!server)
            {
                packet.Send();
            }
            else
            {
                packet.Send(-1, player.whoAmI);
            }
        }
    }

    public class PetHypnosPlayer : ModPlayer
    {
        private bool shouldSyncMouse = false;

        public bool shouldCheckRightClick = false;
        public bool rightClicked = false;
        private bool rightClickedOld = false;

        public bool shouldCheckMouseWorld = false;
        public Vector2 mouseWorld;
        private Vector2 mouseWorldOld;

        public float spinOffset = 0;
        public int currentGhostHypnosIndex = -1;

        public override void PostUpdateMiscEffects()
        {
            if (Player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient && shouldSyncMouse) {
                shouldSyncMouse = false;
                SyncMousePos(false);
                SyncMouseRightClick(false);
            }
        }

        

        public void SyncMousePos(bool isServer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)PetHypnosMessageType.SyncMousePos);
            packet.Write(Player.whoAmI);
            packet.WriteVector2(mouseWorld);
            Player.SendPacket(packet, isServer);
        }

        internal void HandleMousePos(BinaryReader reader)
        {
            mouseWorld = reader.ReadVector2();
            if (Main.netMode == NetmodeID.Server)
            {
                SyncMousePos(true);
            }
        }

        public void SyncMouseRightClick(bool isServer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)PetHypnosMessageType.SyncMouseRightClick);
            packet.Write(Player.whoAmI);
            packet.WriteVector2(mouseWorld);
            Player.SendPacket(packet, isServer);
        }

        public void HandleMouseRightClick(BinaryReader reader)
        {
            rightClicked = reader.ReadBoolean();
            if (Main.netMode == NetmodeID.Server)
            {
                SyncMouseRightClick(true);
            }
        }

        public override void PreUpdate()
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                rightClicked = PlayerInput.Triggers.Current.MouseRight;
                mouseWorld = Main.MouseWorld;
                if (shouldCheckRightClick && rightClicked != rightClickedOld)
                {
                    rightClickedOld = rightClicked;
                    shouldSyncMouse = true;
                    shouldCheckRightClick = false;
                }
                if (shouldCheckMouseWorld && Vector2.Distance(mouseWorld, mouseWorldOld) > 5f)
                {
                    mouseWorldOld = mouseWorld;
                    shouldSyncMouse = true;
                    shouldCheckMouseWorld = false;
                }
            }
        }
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

        public static int VioletOrPurple
        {
            get
            {
                if (calamityModVioletID == null && (CalamityMod?.TryFind("Violet", out ModRarity calamityModViolet) ?? false))
                {
                    calamityModVioletID = calamityModViolet.Type;
                }
                return calamityModVioletID ?? ItemRarityID.Purple;
            }
        }
        private static int? calamityModVioletID = null;

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
                // First, we need to check the npc.type to see if the code is running for the vanilla NPCwe want to change
                if (npc.type == hypBoss.Type)
                {
                        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HypnosPetItem>()));
                    
                }
                // We can use other if statements here to adjust the drop rules of other vanilla NPC
            }

        }

        public override void OnKill(NPC npc)
        {
            if (npc.boss && (ModCompatibility.Hypnos?.TryFind("HypnosBoss", out ModNPC hypBoss) ?? false))
            {
                if (npc.type == hypBoss.Type)
                {
                    int hypPetType = ModContent.ProjectileType<HypnosPetProjectile>();
                    int hypLightPetType = ModContent.ProjectileType<HypnosLightPetProjectile>();
                    foreach (Projectile projectile in Main.projectile)
                    {
                        if (projectile.active && (projectile.type == hypPetType || projectile.type == hypLightPetType))
                        {
                            ((BaseHypnosPetProjectile)projectile.ModProjectile).SpecialStarKill();
                        }
                    }
                }
            }
        }
    }

}