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

namespace PetHypnos.Hypnos.ToyAergianTechnistaff
{
    public partial class PetHypnosPlayer : ModPlayer
    {
        public float spinOffset = 0;
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
            Tooltip.SetDefault("Summons a pair of tiny Aergia Neurons\nYou can only have up to 12 tiny Aergia Neurons\nReduce enemy defense\nWorking in progress, please wait for a while(1)");

            DisplayName.AddTranslation(7, "玩具埃吉亚神经元杖");
            Tooltip.AddTranslation(7, "召唤一对小埃吉亚神经元\n最多只能拥有十二个神经元\n降低敌人的防御\nWorking in progress, please wait for a while(1)");
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 2;
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

            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<ToyAergianTechnistaffBuff>();
            // No buffTime because otherwise the item tooltip would say something like "1 minute duration"
            Item.shoot = ModContent.ProjectileType<ToyAergiaNeuronProjectile>();
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[base.Item.shoot] < 12;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2 && (float)player.maxMinions - player.slotsMinions >= 1f)
            {
                int owned = player.ownedProjectileCounts[type];
                for (int i = 0; i < 2; i++)
                {
                    int p = Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
                    if (Main.projectile.IndexInRange(p))
                    {
                        Main.projectile[p].ai[0] = owned + i;
                        Main.projectile[p].originalDamage = Item.damage;
                    }
                }
                player.AddBuff(ModContent.BuffType<ToyAergianTechnistaffBuff>(), 666);
            }
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
            UpdateStaffOffset(player.GetModPlayer<PetHypnosPlayer>());
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ToyAergiaNeuronProjectile>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }

        public void UpdateStaffOffset(PetHypnosPlayer modPlayer)
        {
            if (modPlayer.spinOffset <= 1)
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

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;
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
        }

        private int AergiaIndex => (int)Projectile.ai[0];

        public int shotCooldown = 0;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();
            
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<ToyAergianTechnistaffBuff>());
            }
            if (player.HasBuff(ModContent.BuffType<ToyAergianTechnistaffBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            float offset = modPlayer.spinOffset * 2 * (float)Math.PI ;
            //这玩意转一圈是一个派
            Vector2 dest = ((float)Math.PI * 2f * AergiaIndex / (float)player.ownedProjectileCounts[Type] + offset).ToRotationVector2() * 250f + player.Center;
            
            //if (!Projectile.WithinRange(dest, 30f))
            //{
            //    Projectile.velocity = (Projectile.velocity * 10f + (dest - Projectile.Center) * 16f) / 21f;
            //}
            if (!Projectile.WithinRange(player.Center, 1800f))
            {

                Projectile.Center = player.Center;
                Projectile.velocity = -Vector2.UnitY * 4f;
                Projectile.netUpdate = true;
            }

            if (shotCooldown == 0)
            {
                NPC mayTarget = FindTarget(player);
                if (mayTarget != null)
                {
                    TryAttackNPC(mayTarget);
                    shotCooldown = 20;
                }
            }

            if (shotCooldown > 0)
            {
                shotCooldown--;
            }
            

            //AIUtils.DoReachPosition(Projectile, dest - Projectile.Center, player, 26f, 40f, 16f);
            AIUtils.DoReachPosition(Projectile, dest - Projectile.Center, player, 40f, 10f, 16f);
            
        }

        private NPC FindTarget(Player player)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                return Main.npc[player.MinionAttackTargetNPC];
            }
            else
            {
                NPC target = null;
                float targetDist = 400f;
                bool hasBuff = false;
                foreach (NPC npc in Main.npc)
                {
                    if (npc.CanBeChasedBy(this, false))
                    {
                        float distance = Vector2.Distance(npc.Center, player.Center);
                        bool npcHasBuff = npc.HasBuff(BuffID.Ichor) && npc.HasBuff(BuffID.BetsysCurse);
                        if ((distance < targetDist && hasBuff == npcHasBuff) || target == null)
                        {
                            targetDist = distance;
                            target = npc;
                            hasBuff = npcHasBuff;
                        }
                    }
                }
                return target;
            }
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
                
                Main.projectile[bolt].originalDamage = Projectile.originalDamage;
                Main.projectile[bolt].netUpdate= true;
                Projectile.netUpdate = true;
            }
        }
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
            Main.projFrames[base.Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.alpha = 254;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 480;
            Projectile.scale = 0.5f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
        }

        public override void AI()
        {
            NPC potentialTarget = Main.npc.ElementAtOrDefault(TargetIndex);
            if (potentialTarget != null && potentialTarget.active)
            {
                float angularOffsetToTarget = MathHelper.WrapAngle(Projectile.AngleTo(potentialTarget.Center) - Projectile.velocity.ToRotation()) * 0.1f;
                Projectile.velocity = Projectile.velocity.RotatedBy(angularOffsetToTarget);
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
            Projectile.alpha -= 30;
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

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Ichor, 180);
            target.AddBuff(BuffID.BetsysCurse, 180);
            Projectile.Kill();
        }
    }
}
