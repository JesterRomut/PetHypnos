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
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using static Humanizer.In;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.CodeAnalysis.Rename;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using static Terraria.Utilities.NPCUtils;
using PetHypnos.Hypnos;

namespace PetHypnos
{
    public static partial class ModCompatibilityTypes
    {

    }

    public partial class PetHypnosPlayer
    {
        public float spinOffset = 0;
        public int currentGhostHypnosIndex = -1;
        public int desiredNeurons = 0;
        public Item technistaff = null;
        public object ring = null;

        internal Vector2 RCenter => Player.Center;

        public override void PostUpdate()
        {
            if (ModCompatibility.calamityEnabled)
            {
                if (desiredNeurons > 0 && currentGhostHypnosIndex == -1)
                {
                    if (ring == null)
                    {
                        CalamityWeakRef.InitRing(0.7f, ref ring, RCenter);
                    }
                    CalamityWeakRef.UpdateRing(Player.velocity, ref ring, RCenter);
                }
                else
                {
                    CalamityWeakRef.KillRing(ref ring);
                    ring = null;
                }
            }
            
        }
    }
}

namespace PetHypnos.Hypnos.ToyAergianTechnistaff
{
    public static class ToyUtils
    {
        public static NPC FindTargetCareBuff(Projectile projectile, Player player)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                return Main.npc[player.MinionAttackTargetNPC];
            }
            else
            {
                NPC target = null;
                float targetDist = 800f;
                bool targetBuffed = true;
                foreach (NPC npc in Main.npc)
                {
                    if (npc.CanBeChasedBy(projectile, false))
                    {
                        float distance = Vector2.Distance(npc.Center, projectile.Center);
                        bool npcHasBuff = npc.HasBuff(BuffID.Ichor) && npc.HasBuff(BuffID.BetsysCurse);
                        if (npcHasBuff)
                        {
                            targetBuffed = false;
                        }
                        if ((distance < targetDist && npcHasBuff == targetBuffed) || target == null)
                        {
                            targetDist = distance;
                            target = npc;
                        }
                    }
                }
                return target;
            }
        }
        public static NPC FindTarget(Projectile projectile, Player player, float sight)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                return Main.npc[player.MinionAttackTargetNPC];
            }
            else
            {
                NPC target = null;
                float targetDist = sight;
                foreach (NPC npc in Main.npc)
                {
                    if (npc.CanBeChasedBy(projectile, false))
                    {
                        float distance = Vector2.Distance(npc.Center, player.Center);
                        if ((distance < targetDist) || target == null)
                        {
                            targetDist = distance;
                            target = npc;
                        }
                    }
                }
                return target;
            }
        }

        public static Projectile GetByAergiaIndex(int index, Player player)
        {
            //foreach (Projectile proj in Main.projectile)
            //{
            //    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ToyAergiaNeuronProjectile>() && ((ToyAergiaNeuronProjectile)proj.ModProjectile).AergiaIndex == index)
            //    {
            //        return proj;
            //    }
            //}
            return Main.projectile.FirstOrDefault(proj => proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ToyAergiaNeuronProjectile>() && ((ToyAergiaNeuronProjectile)proj.ModProjectile).AergiaIndex == index) ?? null;
        }

        public static IEnumerable<Projectile> FindAllNeurons(Player player)
        {
            return Main.projectile.Where(proj => proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ToyAergiaNeuronProjectile>());
        }

        public static List<Projectile> FindAllNeuronsWithNullSlots(Player player, PetHypnosPlayer modPlayer)
        {
            List<Projectile> li = new List<Projectile>();
            foreach (int i in Enumerable.Range(0, modPlayer.desiredNeurons))
            {
                li.Add(GetByAergiaIndex(i, player));
            }
            return li;
        }

        public static int PrevAergiaIndex(int index, Player player)
        {
            if (index - 1 <= 0)
            {
                return player.ownedProjectileCounts[ModContent.ProjectileType<ToyAergiaNeuronProjectile>()];
            }
            return index - 1;
        }

        public static int NextAergiaIndex(int index, Player player)
        {
            if (index + 1 >= player.ownedProjectileCounts[ModContent.ProjectileType<ToyAergiaNeuronProjectile>()])
            {
                return 0;
            }
            return index + 1;
        }

        public static void HitNPCEffect(Projectile proj, NPC target)
        {
            target.AddBuff(BuffID.Ichor, 180);
            target.AddBuff(BuffID.BetsysCurse, 180);

            if (Main.rand.NextBool(3))
            {
                Player player = Main.player[proj.owner];
                player.statMana += 6;
                player.ManaEffect(6);
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric);
                d.velocity *= 0.5f;
            }
        }

        public static int CalcDamage(NPC target, int minDamage)
        {
            return (int)Math.Max(Math.Min(int.MaxValue / 2, (float)target.lifeMax / (target.boss ? 18000 : 6)), minDamage);
        }

        public static Projectile TryGetHypnos(PetHypnosPlayer modPlayer)
        {
            Projectile hypnos = Main.projectile.ElementAtOrDefault(modPlayer.currentGhostHypnosIndex);
            if (hypnos != null && hypnos != default && hypnos.active)
            {
                return hypnos;
            }
            else
            {
                ThisHappensWhenHypnosNotFound(modPlayer);
                return null;
            }
        }

        public static void ThisHappensWhenHypnosNotFound(PetHypnosPlayer modPlayer)
        {
            modPlayer.currentGhostHypnosIndex = -1;
        }

        public static Projectile TryGetHypnos(Player player)
        {
            return TryGetHypnos(player.GetModPlayer<PetHypnosPlayer>());
        }


    }

    public class ToyAergianTechnistaff : ModItem
    {
        public static readonly Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/ToyAergianTechnistaff/ToyAergianTechnistaffGlow");
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D sprite = glowTex.Value;
            spriteBatch.Draw
            (
                sprite,
                new Vector2
                (
                    Item.position.X - Main.screenPosition.X + Item.width * 0.5f,
                    Item.position.Y - Main.screenPosition.Y + Item.height - sprite.Height * 0.5f// + 2f
                ),
                new Rectangle(0, 0, sprite.Width, sprite.Height),
                Color.White,
                rotation,
                glowTex.Size() * 0.5f,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Toy Aergian Technistaff");
            Tooltip.SetDefault("Summons a pair of tiny Aergia Neurons\nShoot exo lasers which reduce enemy defense\nRight clicking releases ghost hypnos dealing massive contact damage\n'Y2000 Mindcrash incoming!'");

            DisplayName.AddTranslation(7, "玩具埃吉亚神经元杖");
            Tooltip.AddTranslation(7, "召唤一对小埃吉亚神经元\n射击能降低敌人防御的星流激光\n右键释放幽灵修普诺斯来造成大量碰撞伤害\n'Y2000思维大崩溃即将来临！'");
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = false;
        }

        public override void SetDefaults()
        {
            Item.damage = 6;
            Item.knockBack = 0f;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.buyPrice(0, 0, 0, 0);
            Item.rare = ModCompatibility.VioletOrPurple;
            Item.UseSound = SoundID.Item44;

            // These below are needed for a minion weapon
            Item.channel = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<ToyAergianTechnistaffBuff>();
            // No buffTime because otherwise the item tooltip would say something like "1 minute duration"
            Item.shoot = ModContent.ProjectileType<ToyGhostHypnosProjectile>();
        }

        public override void HoldItem(Player player)
        {
            Item.channel = player.altFunctionUse == 2;
            PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();
            modPlayer.shouldCheckRightClick = true;
            modPlayer.shouldCheckMouseWorld = true;
            //CombatText.NewText(player.Hitbox, Color.White, $"{modPlayer.desiredNeuronPair}");
        }


        //public override bool CanUseItem(Player player)
        //{
        //    return player.ownedProjectileCounts[aergiaNeuronType] < 12;
        //}

        static int aergiaNeuronType = ModContent.ProjectileType<ToyAergiaNeuronProjectile>();
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            if (player.altFunctionUse != 2)
            {
                if ((float)player.maxMinions - player.slotsMinions >= 1f && player.ownedProjectileCounts[aergiaNeuronType] < 12)
                {


                    PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();


                    modPlayer.desiredNeurons += 2;
                    modPlayer.technistaff = Item;

                    FillNeurons(player, modPlayer, Item);
                    player.AddBuff(ModContent.BuffType<ToyAergianTechnistaffBuff>(), 666);
                }
            }
            else
            {
                if (player.ownedProjectileCounts[aergiaNeuronType] > 0)
                {
                    AIUtils.KillAll(player, type);
                    PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();
                    Projectile p = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
                    modPlayer.currentGhostHypnosIndex = p.whoAmI;
                    p.originalDamage = Item.damage;
                }


                //int p = Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<ToyGhostHypnosProjectile>(), damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override bool AltFunctionUse(Player player)
        {

            return true;
        }

        public static void SummonNeuron(Player player, Item item, int index, PetHypnosPlayer modPlayer)
        {
            int p = Projectile.NewProjectile(player.GetSource_ItemUse_WithPotentialAmmo(item, 0), modPlayer.mouseWorld, Vector2.Zero, aergiaNeuronType, player.GetWeaponDamage(item), player.GetWeaponKnockback(item), player.whoAmI);
            if (Main.projectile.IndexInRange(p))
            {
                Main.projectile[p].ai[0] = index;
                Main.projectile[p].originalDamage = item.damage;
            }
        }

        public static void FillNeurons(Player player, PetHypnosPlayer modPlayer, Item item)
        {
            List<Projectile> li = ToyUtils.FindAllNeuronsWithNullSlots(player, modPlayer);
            foreach (int i in Enumerable.Range(0, li.Count))
            {
                if (li[i] == null)
                {
                    SummonNeuron(player, item, i, modPlayer);
                }
            }
        }

        public override bool AllowPrefix(int pre)
        {
            return false;
        }
    }

    public class ToyAergianTechnistaffBuff : ModBuff
    {
        public override string Texture => "PetHypnos/Hypnos/AergiaNeuronProjectile";
        public override void SetStaticDefaults()
        {

            Main.buffNoTimeDisplay[((ModBuff)this).Type] = true;

            DisplayName.SetDefault("Tiny Aergia Neurons");
            Description.SetDefault("In wonderlands");

            DisplayName.AddTranslation(7, "小埃吉亚神经元");
            Description.AddTranslation(7, "梦游仙境");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();
            UpdateStaffOffset(modPlayer);
            if (modPlayer.desiredNeurons > 0)
            {
                player.buffTime[buffIndex] = 18000;
                if (player.ownedProjectileCounts[Type] < modPlayer.desiredNeurons)
                {
                    ToyAergianTechnistaff.FillNeurons(player, modPlayer, modPlayer.technistaff);
                }
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }

        public void UpdateStaffOffset(PetHypnosPlayer modPlayer)
        {
            if (modPlayer.spinOffset < 1)
            {
                modPlayer.spinOffset += 0.01f;
            }
            else
            {
                modPlayer.spinOffset = 0;
            }
        }

    }

    public class ToyAergiaNeuronProjectile : BaseAergiaNeuronProjectile
    {
        //public bool sticked = false;

        

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage()
        {
            Projectile hypnos = ToyUtils.TryGetHypnos(Main.player[Projectile.owner]);
            if (hypnos != default)
            {
                return ((ToyGhostHypnosProjectile)hypnos.ModProjectile).FullyCharged;
            }
            return false;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tiny Aergia Neuron");
            DisplayName.AddTranslation(7, "小埃吉亚神经元");
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;

        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.minion = true;
            Projectile.minionSlots = 0.5f;
            Projectile.scale = 0.8f;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
        }

        public int AergiaIndex => (int)Projectile.ai[0];

        public int shotCooldown = 0;

        //public bool shouldReturn = false;

        public bool shouldDrawLightning = false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();

            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<ToyAergianTechnistaffBuff>());
            }
            if (!player.HasBuff(ModContent.BuffType<ToyAergianTechnistaffBuff>()))
            {
                Projectile.Kill();
                modPlayer.desiredNeurons = 0;
                return;
            }
            Projectile.timeLeft = 2;

            //if (!Projectile.WithinRange(dest, 30f))
            //{
            //    Projectile.velocity = (Projectile.velocity * 10f + (dest - Projectile.Center) * 16f) / 21f;
            //}
            //if (shouldReturn)
            //{
            //    TeleportToPlayer(player);
            //}
            if (shotCooldown > 0)
            {
                shotCooldown--;
            }

            if (modPlayer.currentGhostHypnosIndex != -1)
            {
                DoRightClickBehavior(player, modPlayer);
            }
            else
            {

                DoCommonBehavior(player, modPlayer);
            }


        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (shouldDrawLightning)
            {
                Player player = Main.player[Projectile.owner];
                Projectile hypnos = ToyUtils.TryGetHypnos(player);//Main.projectile.ElementAtOrDefault(player.GetModPlayer<PetHypnosPlayer>().currentGhostHypnosIndex);
                if (hypnos != default)
                {
                    //ToyGhostHypnosProjectile hypnosMod = (ToyGhostHypnosProjectile)hypnos.ModProjectile;

                    //Projectile prev = ToyUtils.GetByAergiaIndex(ToyUtils.PrevAergiaIndex(AergiaIndex, player), player);
                    //if (prev != null)
                    //{
                    //    DrawLightning(prev.Center);
                    //}

                    Projectile next = ToyUtils.GetByAergiaIndex(ToyUtils.NextAergiaIndex(AergiaIndex, player), player);
                    if (next != null)
                    {
                        DrawLightning(next.Center);
                    }

                }

            }

            return true;
        }

        

        private void DoRightClickBehavior(Player player, PetHypnosPlayer modPlayer)
        {
            Projectile hypnos = ToyUtils.TryGetHypnos(modPlayer);//Main.projectile.ElementAtOrDefault(modPlayer.currentGhostHypnosIndex);
            ToyGhostHypnosProjectile hypnosMod = (ToyGhostHypnosProjectile)hypnos.ModProjectile;

            float offset = modPlayer.spinOffset * 2 * (float)Math.PI;
            //这玩意转一圈是一个派
            Vector2 dest = ((float)Math.PI * 2f * AergiaIndex / (float)player.ownedProjectileCounts[Type] - hypnos.scale * hypnosMod.time * 0.03f - offset).ToRotationVector2() * 100f + hypnos.Center;

            red = true;

            if (hypnosMod.FullyCharged)
            {
                if (shouldDrawLightning == false)
                {
                    AddElectricDusts();

                }
                shouldDrawLightning = true;
            }
            else
            {
                shouldDrawLightning = false;
            }
            if (Projectile.Center.Distance(dest) > player.Center.Distance(dest) * 2)
            {
                TeleportToPlayer(player);
            }

            Vector3 liiight = Color.HotPink.ToVector3() * 1.2f;

            Lighting.AddLight(Projectile.Center, liiight);

            //AIUtils.DoReachPosition(Projectile, dest - Projectile.Center, player, 26f, 40f, 16f);
            AIUtils.DoMoveTo3(Projectile, dest, 0.03f);
            //AIUtils.DoReachPosition(Projectile, dest - Projectile.Center, hypnos, hypnos.scale * 100f + 12f, 4f, 16f);
        }

        private void TeleportToPlayer(Player player)
        {
            Projectile.Center = player.Center;
            Projectile.velocity = -Vector2.UnitY * 4f;
            Projectile.netUpdate = true;
        }

        private void DoCommonBehavior(Player player, PetHypnosPlayer modPlayer)
        {
            
            red = false;
            shouldDrawLightning = false;

            if (!Projectile.WithinRange(player.Center, 1800f))
            {
                TeleportToPlayer(player);

            }

            if (shotCooldown == 0)
            {
                NPC mayTarget = ToyUtils.FindTargetCareBuff(Projectile, player);
                if (mayTarget != null)
                {
                    TryAttackNPC(mayTarget);

                }
            }

            float offset = modPlayer.spinOffset * 2 * (float)Math.PI;
            //这玩意转一圈是一个派
            Vector2 dest = ((float)Math.PI * 2f * AergiaIndex / (float)player.ownedProjectileCounts[Type] + offset).ToRotationVector2() * 100f + player.Center;

            if (Projectile.Center.Distance(dest) > player.Center.Distance(dest) * 2)
            {
                TeleportToPlayer(player);
            }
            //float rottimer = 0;
            //int owned = 12;
            //double test = (360 / owned);

            //double rad6 = (double)(test * AergiaIndex + rottimer) * (Math.PI / 180.0);
            //double dist4 = 200.0;

            //float hyposx4 = player.Center.X - (float)(Math.Cos(rad6) * dist4) - (float)(Projectile.width / 2);
            //float hyposy4 = player.Center.Y - (float)(Math.Sin(rad6) * dist4) - (float)(Projectile.height / 2);

            //Projectile.Center = dest;
            AIUtils.DoMoveTo3(Projectile, dest);

            //AIUtils.DoMoveTo(Projectile, Projectile.Center, dest);
            //AIUtils.DoReachPosition(Projectile, dest - Projectile.Center, player, 26f, 40f, 16f);
            //AIUtils.DoReachPosition(Projectile, dest - Projectile.Center, player, 40f, 10f, 16f);
        }



        private void TryAttackNPC(NPC target)
        {

            if (Main.myPlayer == Projectile.owner && target.active)
            {

                Vector2 shootVel = target.Center - Projectile.Center;
                if (shootVel == Vector2.Zero)
                {
                    shootVel = new Vector2(0f, 1f);
                }
                shootVel.Normalize();
                shootVel *= 18f;
                int bolt = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVel, ModContent.ProjectileType<ToyAergiaNeuronBlueExoPulse>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI);

                shotCooldown = 20;
                Main.projectile[bolt].originalDamage = Projectile.originalDamage;
                Main.projectile[bolt].netUpdate = true;
                Projectile.netUpdate = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            crit = true;
            damage = ToyUtils.CalcDamage(target, damage * 10);
        }

        //public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        //{
        //    ToyUtils.HitNPCEffect(Projectile, target);
        //}
    }

    public class ToyGhostHypnosProjectile : ModProjectile
    {
        public static readonly int frameCount = 4;
        public int frameCurrent = 0;
        public int frameInterval = 0;
        public int frameSpeed = 8;
        public bool flipped = false;
        public override string Texture => "PetHypnos/Hypnos/HypnosPetProjectile";

        public bool FullyCharged => Projectile.scale >= 0.7f;
        public bool released = false;
        public int time = 0;

        public bool returningToPlayer = false;
        public static readonly int startTimeout = 60;
        public object ring = null;

        public static readonly Asset<Texture2D> tex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/HypnosPetProjectile");
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D sprite = tex.Value;

            int frameStartX = (flipped ? 1 : 0) * sprite.Width / 2;
            int frameStartY = sprite.Height / frameCount * frameCurrent;
            int frameWidth = sprite.Width / 2;
            int frameHeight = sprite.Height / frameCount;
            frameWidth -= 2;
            frameHeight -= 2;
            Rectangle frame = new Rectangle(frameStartX, frameStartY, frameWidth, frameHeight);
            Vector2 origin = new Vector2(Projectile.width / 2, Projectile.height / 2);

            Vector2 hypnpos = Projectile.position - Main.screenPosition - new Vector2(frameWidth / 4, frameHeight / 4 - 2f);

            Main.EntitySpriteDraw(sprite, hypnpos, (Rectangle?)frame, new Color(0.7f, 1, 1, 0.2f), Projectile.rotation, origin, Projectile.scale, default(SpriteEffects), 0);

            return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ghost Hypnos");
            DisplayName.AddTranslation(7, "幽灵修普诺斯");
            //Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            // Needed so the minion doesn't despawn on collision with enemies or tiles
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            //Projectile.scale = 0.7f;
            Projectile.minion = true;
            Projectile.timeLeft = 300;
            base.Projectile.minionSlots = 0f;
            Projectile.scale = 0.01f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.X * 0.05f;

            if (ModCompatibility.calamityEnabled)
            {
                if (ring == null)
                {
                    CalamityWeakRef.InitRing(0.7f, ref ring, Projectile.Center);
                }
                //CalamityWeakRef.ChangeRingColor(ref ring, Color.Purple, Projectile.scale * 2);
                CalamityWeakRef.UpdateRing(Projectile, ref ring, Projectile.Center);
            }
            

            frameInterval++;
            if (frameInterval >= frameSpeed)
            {
                frameInterval = 0;
                frameCurrent++;
                if (frameCurrent >= frameCount)
                {
                    frameCurrent = 0;
                }
            }

            if (Projectile.velocity.X > 0)
            {
                flipped = true;
            }
            else if (Projectile.velocity.X < 0)
            {
                flipped = false;
            }

            if (!FullyCharged)
            {
                Projectile.scale += 0.003f;
            }

            if (!Projectile.active)
            {
                Projectile.Kill();
            }


            Player player = Main.player[Projectile.owner];
            PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();

            if (!player.HasBuff(ModContent.BuffType<ToyAergianTechnistaffBuff>()))
            {
                Projectile.Kill();
            }


            if (!released)
            {
                Projectile.timeLeft = startTimeout;
                Vector2 hoverDestination = player.Top + new Vector2((float)player.direction * base.Projectile.scale * 30f, 0);
                hoverDestination += (modPlayer.mouseWorld - hoverDestination) * 0.09f;
                //player.position - player.Size + new Vector2( 20f * player.direction, 30f * Projectile.scale
                AIUtils.DoMoveTo3(Projectile, hoverDestination);
                if (!modPlayer.rightClicked)
                {

                    if (FullyCharged)
                    {
                        PetHypnosQuote.HypnosQuote(Projectile, PetHypnosQuotes.toystaffAttack.RandomQuote());
                        if (Main.myPlayer == base.Projectile.owner)
                        {
                            base.Projectile.velocity = (modPlayer.mouseWorld - Projectile.Center).SafeNormalize(default) * 33f;
                            base.Projectile.damage = (int)((float)base.Projectile.damage * 4.35f);
                            released = true;
                            base.Projectile.netUpdate = true;
                        }
                    }
                    else
                    {

                        Projectile.Kill();
                    }
                }



            }
            else
            {
                if (returningToPlayer)
                {
                    Projectile.timeLeft = startTimeout;
                    AIUtils.DoChasePosition(Projectile, player.Center, 60, 10, 33, 10);
                    if (Projectile.WithinRange(player.Center, 50f))
                    {
                        returningToPlayer = false;
                    }
                }
                else
                {
                    if (!Projectile.WithinRange(player.Center, 4000f))
                    {
                        //Projectile.Center = player.Center;
                        //Projectile.netUpdate = true;
                        returningToPlayer = true;
                    }
                    NPC mayTarget = ToyUtils.FindTarget(Projectile, player, 1200f);
                    if (mayTarget != null)
                    {
                        Projectile.timeLeft = startTimeout;
                        TryAttackNPC(mayTarget);

                    }
                    else
                    {
                        Projectile.velocity = Projectile.velocity.ToRotation().ToRotationVector2() * 33f;
                    }
                }
            }



            AdjustPlayerValues();
            time++;
        }

        private void TryAttackNPC(NPC target)
        {
            AIUtils.DoChasePosition(Projectile, target.Center, 40, 10, 33, 10);
            //float flySpeed = 40f / (float)base.Projectile.MaxUpdates;
            //base.Projectile.velocity = Vector2.Lerp(base.Projectile.velocity, (target.Center - Projectile.Center).SafeNormalize(default) * flySpeed, 0.02f);
            //float angularOffsetToTarget = MathHelper.WrapAngle(Projectile.AngleTo(target.Center) - Projectile.velocity.ToRotation()) * 0.1f;
            //Projectile.velocity = Projectile.velocity.RotatedBy(angularOffsetToTarget);
        }

        public override void Kill(int timeLeft)
        {
            ToyUtils.ThisHappensWhenHypnosNotFound(Main.player[Projectile.owner].GetModPlayer<PetHypnosPlayer>());
            //Main.player[Projectile.owner].GetModPlayer<PetHypnosPlayer>().currentGhostHypnosIndex = -1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(base.Projectile.scale);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.Projectile.scale = reader.ReadSingle();
        }

        public void AdjustPlayerValues()
        {

            if (!released)
            {
                Player player = Main.player[Projectile.owner];
                base.Projectile.spriteDirection = (base.Projectile.direction = player.direction);
                player.heldProj = base.Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
            }

        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            crit = true;
            damage = ToyUtils.CalcDamage(target, damage * 10);
        }

        //public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        //{
        //    ToyUtils.HitNPCEffect(Projectile, target);
        //}
    }

    public class ToyAergiaNeuronBlueExoPulse : ModProjectile
    {
        public override string Texture => "PetHypnos/Hypnos/ToyAergianTechnistaff/BlueExoPulseLaser";
        public int TargetIndex => (int)Projectile.ai[0];
        public Vector2 targetPos;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
            DisplayName.SetDefault("Blue Exo Pulse Laser");
            DisplayName.AddTranslation(7, "蓝色星流脉冲激光");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 200;
            Projectile.scale = 0.5f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
            //Projectile.ArmorPenetration = 666;
            Projectile.DamageType = DamageClass.Summon;
            base.Projectile.minion = true;
            base.Projectile.minionSlots = 0f;
        }

        public override void AI()
        {
            NPC potentialTarget = Main.npc.ElementAtOrDefault(TargetIndex);
            if (potentialTarget != null && potentialTarget.CanBeChasedBy(this))
            {
                //AIUtils.DoChasePosition(Projectile, potentialTarget.Center, 20, 10, 20, 10);
                //float angularOffsetToTarget = MathHelper.WrapAngle(Projectile.AngleTo(potentialTarget.Center) - Projectile.velocity.ToRotation()) * 0.1f;
                //Projectile.velocity = Projectile.velocity.RotatedBy(angularOffsetToTarget);
                AIUtils.DoChasePosition(Projectile, potentialTarget.Center, 20, 10, 20, 10);
                Projectile.alpha -= 30;
            }
            else
            {
                Projectile.alpha += 10;
            }


            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }

            Lighting.AddLight(Projectile.Center, 0f, 0f, 0.6f);
            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(0.0 - (double)Projectile.velocity.Y, 0.0 - (double)Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }
            if (Projectile.timeLeft <= 60)
            {
                Projectile.alpha += 10;
            }
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
        }

        //public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        //{
        //    crit = true;
        //}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            ToyUtils.HitNPCEffect(Projectile, target);

            Projectile.Kill();
        }
    }

    //public class DenpaDebuff: ModBuff
    //{
    //    public override string Texture => "PetHypnos/Hypnos/HypnosPetBuff"; 
    //    public override void SetStaticDefaults()
    //    {
    //        DisplayName.SetDefault("Electromagnetic radiation");
    //        Description.SetDefault("Remained lingering around the beams for three days");

    //        DisplayName.AddTranslation(7, "电波辐射");
    //        Description.AddTranslation(7, "绕梁三日 余音不绝 三花聚顶 羽化登仙");
    //        Main.debuff[Type] = true;
    //        Main.pvpBuff[Type] = true;
    //        Main.buffNoSave[Type] = true;
    //        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    //        BuffID.Sets.LongerExpertDebuff[Type] = true;
    //    }

    //    private void UniversalUpdate(Entity entity, ref int buffIndex)
    //    {
    //        if (Main.rand.NextBool(3))
    //        {
    //            Dust d = Dust.NewDustDirect(entity.position, entity.width, entity.height, DustID.Electric);
    //            d.velocity *= 0.5f;
    //        }
    //    }

    //    public override void Update(NPC npc, ref int buffIndex)
    //    {
    //        UniversalUpdate(npc, ref buffIndex);
    //        npc.defense -= 100;
    //    }

    //    public override void Update(Player player, ref int buffIndex)
    //    {
    //        UniversalUpdate(player, ref buffIndex);
    //        player.statDefense -= 100;
    //    }
    //}
}
