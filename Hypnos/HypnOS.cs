using Terraria.ModLoader;
using ReLogic;
using Terraria;
using Terraria.ID;
using log4net;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Mono.Cecil;
using static Terraria.ModLoader.PlayerDrawLayer;
using SteelSeries.GameSense;
using Terraria.Graphics.Renderers;
using IL.Terraria.GameContent.UI.States;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;

namespace PetHypnos.Hypnos
{
    public enum HypnosBehavior
    {
        ChasePlayer,
        ChaseMouse,
        Stressed
    }


    public class AIUtils
    {
        public static bool bossIsAlive
        {
            get
            {
                foreach(NPC npc in Main.npc)
                {
                    if (npc.active && npc.boss)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static void DoChasePosition(Projectile projectile, Vector2 pos, float speed = 16f, float inertia = 60f, float closedSpeed = 4f, float closedInertia = 80f, bool idlePoke = true)
        {

            Vector2 vectorToTarget = pos - projectile.Center;
            float distanceToTarget = vectorToTarget.Length();

            if (distanceToTarget <= 600f)
            {
                // Slow down the minion if closer to the player
                speed = closedSpeed;
                inertia = closedInertia;
            }
            if (distanceToTarget > 20f)
            {
                // The immediate range around the player (when it passively floats about)

                // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                vectorToTarget.Normalize();
                vectorToTarget *= speed;
                projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTarget) / inertia;
            }
            else if (idlePoke && projectile.velocity == Vector2.Zero)
            {
                // If there is a case where it's not moving at all, give it a little "poke"
                projectile.velocity.X = -0.15f;
                projectile.velocity.Y = -0.05f;
            }
        }

        public static void DoReachPosition(Projectile projectile, Vector2 pos, Player master,float flyingSpeed = 12f, float flyingInertia = 60f, float minFlyDistance = 20f, float speedupThreshold = 200f)
        {
            Vector2 vectorToPlayer = master.Center - projectile.Center;
            vectorToPlayer += pos;
            if (vectorToPlayer.Length() > speedupThreshold)
            {
                flyingSpeed *= 1.25f;
                flyingInertia *= 0.75f;
            }
            if (vectorToPlayer.Length() < minFlyDistance)
            {
                flyingSpeed *= 0.5f;
                flyingInertia *= 1.2f;
            }
                vectorToPlayer.Normalize();
                vectorToPlayer *= flyingSpeed;
                if (projectile.velocity == Vector2.Zero)
                {
                    projectile.velocity = new Vector2(-0.15f);
                }
                if (flyingInertia != 0f && flyingSpeed != 0f)
                {
                    projectile.velocity = (projectile.velocity * (flyingInertia - 1f) + vectorToPlayer) / flyingInertia;
                }
        }
    }

    public class AergiaNeuronProjectile : ModProjectile
    {
        Projectile Master
        {
            get
            {
                int byUUID = Projectile.GetByUUID(Projectile.owner, Projectile.ai[0]);
                Projectile UFO = Main.projectile.ElementAtOrDefault(byUUID);
                if (byUUID >= 0 && UFO != null)
                {
                    return UFO;
                }
                return null;
            }
        }

        public bool red = false;
        //BaseHypnosPetProjectile masterModProjectile => (BaseHypnosPetProjectile)master.ModProjectile;

        public static readonly Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/AergiaNeuronGlow");
        public static readonly Asset<Texture2D> redGlowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/AergiaNeuronRedGlow");
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aergia Neuron");
            Main.projPet[Projectile.type] = true;
            //Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Projectile master = Master;
            if (master == null || !master.active)
            {
                Projectile.timeLeft = 0;
                return;
            }

            Projectile.timeLeft = 2;

            // If your minion is flying, you want to do this independently of any conditions
            float overlapVelocity = 0.04f;
            foreach (Projectile other in Main.projectile)
            {
                // Fix overlap with other minions
                if (other.whoAmI != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;

                    if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }

            float distanceToMaster = (master.Center - Projectile.Center).Length();

            if (distanceToMaster > 2000f)
            {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                Projectile.position = master.Center;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            //float distanceToTarget = vectorToTarget.Length();
            //follow master or follow mouse
            if ((Main.MouseWorld - Projectile.Center).Length() < 40f && distanceToMaster > 50f)
            {
                AIUtils.DoChasePosition(Projectile, Main.MouseWorld, 20f, 40f, 6f, 16f);
                

                if (distanceToMaster > 120f)
                {
                    red = true;
                }
                else
                {
                    red = false;
                }

                Vector3 liiight = Color.HotPink.ToVector3() * 0.7f;
                
                if (red)
                {
                    liiight = Color.HotPink.ToVector3() * 1.2f;
                }

                Lighting.AddLight(Projectile.Center, liiight);

                //ModLoader.TryGetMod("CalamityMod", out Mod calamity);
                //calamity?.Call("AddAbyssLightStrength", Main.player[Projectile.owner], 3);
            }
            else
            {
                red = false;
                double angle = Math.Atan2(Projectile.Center.Y - master.Center.Y, Projectile.Center.X - master.Center.X);
                float radius = 60f;

                Vector2 target = new Vector2((float)(master.Center.X + radius * Math.Cos(angle)), (float)(master.Center.Y + radius * Math.Sin(angle)));

                AIUtils.DoChasePosition(Projectile, target, 20f, 40f, 6f, 16f);
            }



        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D sprite = red ? redGlowTex.Value : glowTex.Value;
            float originOffsetX = (sprite.Width - Projectile.width) * 0.5f + Projectile.width * 0.5f + DrawOriginOffsetX;

            Rectangle frame = new Rectangle(0, 0, sprite.Width, sprite.Height);

            Vector2 origin = new Vector2(originOffsetX, Projectile.height / 2 - DrawOriginOffsetY);


            Main.EntitySpriteDraw(sprite, Projectile.position - Main.screenPosition + new Vector2(originOffsetX + DrawOffsetX, Projectile.height / 2 + Projectile.gfxOffY), (Rectangle?)frame, Color.White, Projectile.rotation, origin, Projectile.scale, default(SpriteEffects), 0);
        }
    }

    public abstract class BaseHypnosPetProjectile : ModProjectile
    {
        public static readonly int frameCount = 4;
        public int frameCurrent = 0;
        public int frameInterval = 0;
        public int frameSpeed = 8;
        public bool flipped = false;
        public bool initialized = false;

        public override string Texture => "PetHypnos/Hypnos/HypnosPetProjectile";

        public static readonly Asset<Texture2D> tex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/HypnosPetProjectile");
        public static readonly Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/HypnosPetProjectileGlow");

        public static readonly int aergiaID = ModContent.ProjectileType<AergiaNeuronProjectile>();

        public Player Master => Main.player[Projectile.owner];


        public HypnosBehavior behavior = HypnosBehavior.ChasePlayer;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Denpa-kei Hypnos");
            Main.projPet[Projectile.type] = true;
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
            Projectile.scale = 0.7f;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public abstract int GetBuffType();

        public void Init()
        {

            for (int i = 0; i < 9; i++)
            {
                Projectile.NewProjectile(Master.GetSource_Buff(Master.FindBuffIndex(GetBuffType())), Master.Center, Vector2.Zero, aergiaID, 0, 0f, Master.whoAmI, Projectile.whoAmI, 0f);
            }

            initialized = true;
        }



        public override void AI()
        {
            Player player = Master;
            if (player.dead || !player.active)
            {
                player.ClearBuff(GetBuffType());
            }
            if (player.HasBuff(GetBuffType()))
            {
                Projectile.timeLeft = 2;
            }
            else
            {
                Projectile.timeLeft = 0;
            }

            if (!initialized)
            {
                Init();
            }

            Vector2 idlePosition = player.Center;
            //idlePosition.Y -= 48f;
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();

            if (AIUtils.bossIsAlive)
            {
                behavior = HypnosBehavior.Stressed;
            }

            else if ((Main.MouseWorld - player.Center).Length() < 300f && distanceToIdlePosition < 400f)
            {
                behavior = HypnosBehavior.ChaseMouse;
            }
            else
            {
                behavior = HypnosBehavior.ChasePlayer;
            }

            // If your minion is flying, you want to do this independently of any conditions
            float overlapVelocity = 0.04f;
            foreach (Projectile other in Main.projectile)
            {
                // Fix overlap with other minions
                if (other.whoAmI != Projectile.whoAmI && other.active && other.type != aergiaID && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;

                    if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }

            switch (behavior)
            {
                case HypnosBehavior.ChasePlayer:

                    if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
                    {
                        // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                        // and then set netUpdate to true
                        Projectile.position = idlePosition;
                        Projectile.velocity *= 0.1f;
                        Projectile.netUpdate = true;
                    }

                    Vector2 targetPos = player.Center;
                    targetPos.Y -= 100f;

                    AIUtils.DoChasePosition(Projectile, targetPos);



                    break;
                case HypnosBehavior.ChaseMouse:
                    AIUtils.DoChasePosition(Projectile, Main.MouseWorld);
                    break;
                case HypnosBehavior.Stressed:
                    AIUtils.DoReachPosition(Projectile, new Vector2(68f * (float)(-player.direction), -20f), player, 20f, 40f, 16f);
                    break;
                default:
                    behavior = HypnosBehavior.ChasePlayer;
                    break;
            }



            Projectile.rotation = Projectile.velocity.X * 0.05f;


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

            if (Projectile.velocity.X >= 0)
            {
                flipped = true;
            }
            else
            {
                flipped = false;
            }

            //Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D sprite = tex.Value;
            Texture2D spriteGlow = glowTex.Value;

            int frameStartX = (flipped ? 1 : 0) * sprite.Width / 2;
            int frameStartY = sprite.Height / frameCount * frameCurrent;
            int frameWidth = sprite.Width / 2;
            int frameHeight = sprite.Height / frameCount;
            frameWidth -= 2;
            frameHeight -= 2;
            Rectangle frame = new Rectangle(frameStartX, frameStartY, frameWidth, frameHeight);
            Vector2 origin = new Vector2(Projectile.width / 2, Projectile.height / 2);

            Color liiight = Lighting.GetColor((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16);

            Vector2 hypnpos = Projectile.position - Main.screenPosition + new Vector2(frameWidth / 4, frameHeight / 4 - 2f);

            Main.EntitySpriteDraw(sprite, hypnpos, (Rectangle?)frame, liiight, Projectile.rotation, origin, Projectile.scale, default(SpriteEffects), 0);
            Main.EntitySpriteDraw(spriteGlow, hypnpos, (Rectangle?)frame, Color.White, Projectile.rotation, origin, Projectile.scale, default(SpriteEffects), 0);

            return false;
            //return true;
        }
    }

    public class HypnosPetProjectile : BaseHypnosPetProjectile
    {
        public override int GetBuffType()
        {
            return ModContent.BuffType<HypnosPetBuff>();
        }
    }

    public class HypnosLightPetProjectile : BaseHypnosPetProjectile
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.LightPet[Projectile.type] = true;
        }

        public override int GetBuffType()
        {
            return ModContent.BuffType<HypnosLightPetBuff>();
        }
    }

    public abstract class BaseHypnosPetBuff : ModBuff
    {
        public static readonly HashSet<string> bible = new HashSet<string>() {
            "Tiny Hypnos' assault on Thanatos keep", //小修普诺斯强袭塔纳堡
            "The day you went away",
            "Hi",
            "Hypnos brings forth innumerable things to nurture man", //修普诺斯生万物以养人
            "From the great above to the great below", //从伟大的天到伟大的地
            "When the flying birds are done with, the good bow is stored away; when the sly rabbit dies, the hunting dog is boiled.", //飞鸟尽，良弓藏；狡兔死，走狗烹
            "There must be something strange about things going wrong", //事出反常必有妖
            "Player suki suki daisuki", //普雷尔 suki suki daisuki
            "It sexually identifies as an attack ornithopter", //它的性别认知是武装扑翼机
            "I think therefore I am", //我思故我在
            "History repeats itself", //历史总是惊人的相似
            "The Next Generation", //下一代
            "Aleph-0",
            "HypnOS v5.64 Code:Vaporwave", 
            "Do android brain dream of electric serpent?", //仿生大脑会梦到电子长直吗？
            "Already dyed itself" //已经染过色了
        };

        public override void SetStaticDefaults()
        {
            Random random = new Random();
            Main.buffNoTimeDisplay[((ModBuff)this).Type] = true;
            
            Description.SetDefault(string.Concat(GetBuffDesc(), "\n", bible.ElementAt(random.Next(bible.Count))));
        }

        public abstract int GetProjectileType();
        public abstract string GetBuffDesc();

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            if (player.ownedProjectileCounts[GetProjectileType()] <= 0 && ((Entity)player).whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), ((Entity)player).Center, Vector2.Zero, GetProjectileType(), 0, 0f, ((Entity)player).whoAmI, 0f, 0f);
            }
        }
    }

    public class HypnosPetBuff : BaseHypnosPetBuff
    {

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("City Pop");
            Main.vanityPet[((ModBuff)this).Type] = true;
        }

        public override int GetProjectileType()
        {
            return ModContent.ProjectileType<HypnosPetProjectile>();
        }

        public override string GetBuffDesc()
        {
            return "Hypnos will play its radio (with h-pop and city pop) behind you";
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

        public override int GetProjectileType()
        {
            return ModContent.ProjectileType<HypnosLightPetProjectile>();
        }

        public override string GetBuffDesc()
        {
            return "Hypnos will play its radio (with neurofunk) behind you";
        }

    }

    public abstract class BaseHypnosPetItem : ModItem
    {
        public static readonly Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/HypnosPetItemGlow", AssetRequestMode.ImmediateLoad);
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

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (((Entity)player).whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(((ModItem)this).Item.buffType, 3600, true, false);
            }
        }

        public override void SetDefaults()
        {
            
            Item.UseSound = SoundID.NPCHit4;
            //Item.damage = 30;
            //Item.knockBack = 3f;
            //Item.mana = 10;
            Item.width = 16;
            Item.height = 16;
            //Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.value = Item.buyPrice(0, 30, 0, 0);
            Item.rare = ItemRarityID.Purple;
            Item.noMelee = true;
            
        }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            Tooltip.SetDefault(string.Concat(GetFirstLine(), "\nSummons the great✰ Hypnos' projection to charm you\nThe great✰ Hypnos comes with its Aergia Neurons\nWill somewhat be attracted by mouse\n", GetLastLine()));
        }

        public abstract string GetFirstLine();
        public abstract string GetLastLine();

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

        public override string GetFirstLine()
        {
            return "Full of hypnagogic pops"; //塞满了H-pop
        }

        public override string GetLastLine()
        {
            return "'Let the bass kick.'";
        }

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

        public override string GetFirstLine()
        {
            return "Full of neurofunks"; //全是神经放克
        }

        public override string GetLastLine()
        {
            return "'what do you know what do you play what do you remember what do you love'"; //你知道什么你扮演什么你记得什么你爱着什么
        }

    }
}