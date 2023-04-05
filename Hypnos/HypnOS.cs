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
using static System.Formats.Asn1.AsnWriter;
using System.IO;
using static Humanizer.In;
using Terraria.Audio;

namespace PetHypnos.Hypnos
{
    public enum HypnosBehavior
    {
        ChasePlayer,
        ChaseMouse,
        Stressed
    }

    public static class AIUtils
    {
        public static bool bossIsAlive
        {
            get
            {
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && npc.boss)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static void DoChasePosition(Projectile projectile, Vector2 pos, float speed = 16f, float inertia = 60f, float closedSpeed = 4f, float closedInertia = 80f, float speedupThreshold = 800f)
        {

            Vector2 vectorToTarget = pos - projectile.Center;
            float distanceToTarget = vectorToTarget.Length();

            if (distanceToTarget > speedupThreshold)
            {
                speed *= 1.25f;
                inertia *= 0.75f;
            }

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
            else if (projectile.velocity == Vector2.Zero)
            {
                // If there is a case where it's not moving at all, give it a little "poke"
                projectile.velocity.X = -0.15f;
                projectile.velocity.Y = -0.05f;
            }
        }

        public static void DoReachPosition(Projectile projectile, Vector2 pos, Entity master, float flyingSpeed = 12f, float flyingInertia = 60f, float minFlyDistance = 20f, float speedupThreshold = 300f)
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
            //if (projectile.velocity == Vector2.Zero)
            //{
            //    projectile.velocity = new Vector2(-0.15f);
            //}

            if (flyingInertia != 0f && flyingSpeed != 0f)
            {
                projectile.velocity = (projectile.velocity * (flyingInertia - 1f) + vectorToPlayer) / flyingInertia;
            }

            if ((double)projectile.velocity.X > -0.1 && (double)projectile.velocity.X < 0.1)
            {
                projectile.velocity.X = 0f;
            }
            if ((double)projectile.velocity.Y > -0.1 && (double)projectile.velocity.Y < 0.1)
            {
                projectile.velocity.Y = 0f;
            }
        }

        public static void KillAll(Player player, int type)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == type)
                {
                    proj.Kill();
                }
            }
        }

        //public static Vector2 MoveTo(Vector2 currentPosition, Vector2 targetPosition, float maxAmountToMove)
        //{
        //    Vector2 vectorTo = targetPosition - currentPosition;
        //    if (vectorTo.Length() < maxAmountToMove)
        //    {
        //        return targetPosition;
        //    }
        //    return currentPosition + vectorTo.SafeNormalize(Vector2.Zero) * maxAmountToMove;
        //}

        //public static void DoMoveTo(Projectile projectile, Vector2 current, Vector2 dest, float maxAmountToMove = 20f)
        //{
        //    projectile.velocity = Vector2.Lerp(projectile.velocity, AIUtils.MoveTo(current, dest, maxAmountToMove), 0.04f);
        //}

        //public static void DoMoveTo2(Projectile projectile, Vector2 returnPos)
        //{
        //    Vector2 playerVec = returnPos - projectile.Center;
        //    float playerHomeSpeed = 40f;
        //        playerVec.Normalize();
        //        playerVec *= playerHomeSpeed;
        //        projectile.velocity = (projectile.velocity * 10f + playerVec) / 11f;
        //}

        public static void DoMoveTo3(Projectile projectile, Vector2 dest, float lerpValue = 0.05f)
        {
            Vector2 vecToDest = dest - projectile.Center;
            //vecToDest *= vecToDest.Length() / 12;
            projectile.velocity = Vector2.Lerp(projectile.velocity, vecToDest, lerpValue);
        }
    }

    //public abstract class NeedMouseProjectile: ModProjectile
    //{
    //    public Vector2? correctMousePos = null;
    //    public Vector2? CorrectMousePos
    //    {
    //        get
    //        {
    //            if (Main.myPlayer == Projectile.owner)
    //            {
    //                correctMousePos = Main.MouseWorld;
    //            }
    //            return correctMousePos;
    //        }
    //    }

    //    public override void SendExtraAI(BinaryWriter writer)
    //    {
    //        writer.WriteVector2(correctMousePos ?? default);
    //    }

    //    public override void ReceiveExtraAI(BinaryReader reader)
    //    {
    //        correctMousePos = reader.ReadVector2();
    //    }
    //}

    public abstract class BaseAergiaNeuronProjectile : ModProjectile
    {
        public override string Texture => "PetHypnos/Hypnos/AergiaNeuronProjectile";
        public static readonly Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/AergiaNeuronGlow");

        public bool red = false;
        public static readonly Asset<Texture2D> redGlowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/AergiaNeuronRedGlow");

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

        public override void PostDraw(Color lightColor)
        {
            Texture2D sprite = red ? redGlowTex.Value : glowTex.Value;
            float originOffsetX = (sprite.Width - Projectile.width) * 0.5f + Projectile.width * 0.5f + DrawOriginOffsetX;

            Rectangle frame = new Rectangle(0, 0, sprite.Width, sprite.Height);

            Vector2 origin = new Vector2(originOffsetX, Projectile.height / 2 - DrawOriginOffsetY);


            Main.EntitySpriteDraw(sprite, Projectile.position - Main.screenPosition + new Vector2(originOffsetX + DrawOffsetX, Projectile.height / 2 + Projectile.gfxOffY), (Rectangle?)frame, Color.White, Projectile.rotation, origin, Projectile.scale, default(SpriteEffects), 0);
        }
    }

    public abstract class BaseAergiaNeuronPetProjectile : BaseAergiaNeuronProjectile
    {
        //Projectile Master
        //{
        //    get
        //    {
        //        int byUUID = Projectile.GetByUUID(Projectile.owner, Projectile.ai[0]);
        //        Projectile UFO = Main.projectile.ElementAtOrDefault(byUUID);
        //        if (byUUID >= 0 && UFO != null && UFO.type == MasterTypeID)
        //        {
        //            return UFO;
        //        }
        //        return null;
        //    }
        //}

        Projectile master;



        public bool initialized = false;

        public abstract int MasterTypeID { get; }


        //BaseHypnosPetProjectile masterModProjectile => (BaseHypnosPetProjectile)master.ModProjectile;


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Denpa-kei Aergia Neuron");
            DisplayName.AddTranslation(7, "电波系-埃吉亚神经元");
            Main.projPet[Projectile.type] = true;
            //Main.projFrames[Projectile.type] = 4;
        }



        public override void AI()
        {
            if (!initialized)
            {
                master = Main.projectile.ElementAtOrDefault((int)Projectile.ai[0]);
                initialized = true;
            }
            //Projectile master = Master;

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
            Vector2 mouseWorld = Main.player[Projectile.owner].GetModPlayer<PetHypnosPlayer>().mouseWorld;

            if (Main.myPlayer == Projectile.owner && (mouseWorld - Projectile.Center).Length() < 40f && distanceToMaster > 50f)
            {
                AIUtils.DoChasePosition(Projectile, mouseWorld, 20f, 40f, 6f, 16f);


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

            Projectile.netUpdate = true;

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
        public int portalTick = 120;

        public override string Texture => "PetHypnos/Hypnos/HypnosPetProjectile";

        public static readonly Asset<Texture2D> tex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/HypnosPetProjectile");
        public static readonly Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("PetHypnos/Hypnos/HypnosPetProjectileGlow");

        public static readonly int aergiaCount = 9;

        public abstract int AergiaID { get; }
        public abstract int BuffID { get; }

        public Player Master => Main.player[Projectile.owner];


        public HypnosBehavior behavior = HypnosBehavior.ChasePlayer;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Denpa-kei Hypnos");
            DisplayName.AddTranslation(7, "电波系-修普诺斯");
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



        public void Init()
        {
            if (Main.myPlayer == Projectile.owner)
            {


                for (int i = 0; i < aergiaCount; i++)
                {
                    Projectile.NewProjectile(Master.GetSource_Buff(Master.FindBuffIndex(BuffID)), Projectile.Center, Vector2.Zero, AergiaID, 0, 0f, Master.whoAmI, Projectile.whoAmI, 0f);
                }
            }

            for (int k = 0; k < 48; k++)
            {
                Vector2 vector2 = Vector2.UnitX * (0f - (float)Projectile.width) / 2f;
                vector2 += -Vector2.UnitY.RotatedBy((float)k * (float)Math.PI / 6f) * new Vector2(8f, 16f);
                int num6 = Dust.NewDust(RCenter, 0, 0, DustID.FireworkFountain_Blue, 0f, 0f, 160);
                Main.dust[num6].scale = 1.1f;
                Main.dust[num6].noGravity = true;
                Main.dust[num6].position = RCenter + vector2;
                Main.dust[num6].velocity = Projectile.velocity * 0.1f;
                Main.dust[num6].velocity = Vector2.Normalize(RCenter - Projectile.velocity * 3f - Main.dust[num6].position) * 1.25f;
            }
            SoundEngine.PlaySound(in calFlareSound, Projectile.Center);

            initialized = true;
        }

        public static readonly SoundStyle? calFlareSound = ModCompatibility.CalamityMod != null ? new SoundStyle("CalamityMod/Sounds/Item/FlareSound") : null;

        public override void AI()
        {
            Player player = Master;PetHypnosPlayer modPlayer = player.GetModPlayer<PetHypnosPlayer>();
            if (player.dead || !player.active)
            {
                player.ClearBuff(BuffID);
            }
            if (player.HasBuff(BuffID))
            {
                Projectile.timeLeft = 2;
            }
            else
            {
                Projectile.timeLeft = 0;
            }

            if (portalTick > 0)
            {
                if (Main.rand.NextBool(2))
                {
                    MakePortal();
                }
                portalTick--;
            }
            else
            {



                if (!initialized)
                {
                    Init();
                }

                Vector2 idlePosition = player.Center;
                Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
                float distanceToIdlePosition = vectorToIdlePosition.Length();

                if (AIUtils.bossIsAlive)
                {
                    behavior = HypnosBehavior.Stressed;
                }

                else if (Main.myPlayer == Projectile.owner && ((modPlayer.mouseWorld - player.Center).Length() < 300f && distanceToIdlePosition < 400f))
                {
                    behavior = HypnosBehavior.ChaseMouse;
                    Projectile.netUpdate = true;
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
                    if (other.whoAmI != Projectile.whoAmI && other.active && other.type != AergiaID && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
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
                            TeleportTo(new Vector2(player.Center.X, player.Center.Y - 200f));
                        }

                        Vector2 targetPos = player.Center;
                        targetPos.Y -= 100f;

                        AIUtils.DoChasePosition(Projectile, targetPos);



                        break;
                    case HypnosBehavior.ChaseMouse:
                        if (Main.myPlayer == Projectile.owner)
                        {

                            AIUtils.DoChasePosition(Projectile, modPlayer.mouseWorld);
                            Projectile.netUpdate = true;
                        }
                        break;
                    case HypnosBehavior.Stressed:
                        if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
                        {
                            TeleportTo(new Vector2(player.Center.X, player.Center.Y - 200f));
                        }
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

                if (Projectile.velocity.X > 0)
                {
                    flipped = true;
                }
                else if (Projectile.velocity.X < 0)
                {
                    flipped = false;
                }
            }
            

            

            //object ring = Activator.CreateInstance(ringType, Projectile.Center, Vector2.Zero, Color.Purple * 1.2f, Projectile.scale * 1.5f, 40);
            //ring = new BloomRing(base.NPC.Center, Vector2.get_Zero(), Color.get_Purple() * 1.2f, base.NPC.scale * 1.5f, 40);
            //if (ring != null)
            //{
            //    GeneralParticleHandler.SpawnParticle(ring);
            //}


            //Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
        }

        public void TeleportTo(Vector2 pos)
        {
            Projectile.position = pos;
            Projectile.velocity = Vector2.Zero;
            portalTick = 120;
            initialized = false;
            AIUtils.KillAll(Main.player[Projectile.owner], AergiaID);
            Projectile.netUpdate = true;
        }

        public void MakePortal()
        {

            int num5 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 200, default(Color), 1.5f);
            Main.dust[num5].noGravity = true;
            Dust obj = Main.dust[num5];
            obj.velocity *= 0.75f;
            Main.dust[num5].fadeIn = 1.3f;
            Vector2 vector = new Vector2((float)Main.rand.Next(-400, 401), (float)Main.rand.Next(-400, 401));

            vector.Normalize();
            vector *= (float)Main.rand.Next(100, 200) * 0.04f;
            Main.dust[num5].velocity = vector;
            vector.Normalize();
            vector *= 34f;
            Main.dust[num5].position = RCenter - vector;

        }

        public Vector2 RCenter => Projectile.Center + new Vector2(Projectile.width, Projectile.height);

        public override bool PreDraw(ref Color lightColor)
        {
            if (portalTick <= 0)
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

                //Color liiight = Lighting.GetColor((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16);

                Vector2 hypnpos = Projectile.position - Main.screenPosition + new Vector2(frameWidth / 4, frameHeight / 4 - 2f);

                //Main.EntitySpriteDraw(spriteRing, hypnpos, (Rectangle?)null, Color.Purple, Projectile.rotation, origin, Projectile.scale * 1.5f, default(SpriteEffects), 0);

                Main.EntitySpriteDraw(sprite, hypnpos, (Rectangle?)frame, lightColor, Projectile.rotation, origin, Projectile.scale, default(SpriteEffects), 0);
                Main.EntitySpriteDraw(spriteGlow, hypnpos, (Rectangle?)frame, Color.White, Projectile.rotation, origin, Projectile.scale, default(SpriteEffects), 0);
            }
            


            return false;
            //return true;
        }

        public virtual void SpecialStarKill()
        {
            Master.ClearBuff(BuffID);
            if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
            {
                Item.NewItem(Projectile.GetSource_Death(), Projectile.getRect(), ModContent.ItemType<ToyAergianTechnistaff.ToyAergianTechnistaff>());
            }

            if (Main.netMode != NetmodeID.Server)
            {
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("PetHypnos1").Type, Projectile.scale);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Mod.Find<ModGore>("PetHypnos2").Type, Projectile.scale);
            }
            Projectile.Kill();
        }

    }





    public abstract class BaseHypnosPetBuff : ModBuff
    {
        public static readonly HashSet<string> bible = new HashSet<string>() {
            "Tiny Hypnos' assault on Thanatos keep", //小修普诺斯强袭塔纳堡
            "The day you went away",
            "Oh haiiii!",
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
            "Already dyed itself", //已经染过色了
            "Then the fifth angel sounded his trumpet", //第五位天使吹号
            "Libet's delay",
            "42"
        };

        public string Curren
        {
            get
            {
                if (curren == null)
                {
                    curren = bible.ElementAt(new Random().Next(bible.Count));
                }
                return curren;
            }
        }

        private string curren;

        public override void SetStaticDefaults()
        {

            Main.buffNoTimeDisplay[((ModBuff)this).Type] = true;


        }

        public abstract int ProjectileTypeID { get; }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            player.GetModPlayer<PetHypnosPlayer>().shouldCheckMouseWorld = true;
            if (player.ownedProjectileCounts[ProjectileTypeID] <= 0 && ((Entity)player).whoAmI == Main.myPlayer)
            {
                //player.DelBuff(buffIndex);
                //buffIndex--;
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), new Vector2(player.Center.X, player.Center.Y - 200f), Vector2.Zero, ProjectileTypeID, 0, 0f, ((Entity)player).whoAmI, 0f, 0f);
            }
        }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {

            tip = string.Concat(tip, "\n", Curren);
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
            Item.value = Item.buyPrice(1, 0, 0, 0);
            Item.rare = ModCompatibility.VioletOrPurple;
            Item.noMelee = true;

        }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;

        }

    }




}