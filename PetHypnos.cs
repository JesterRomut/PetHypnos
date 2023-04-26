using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using PetHypnos.Hypnos;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

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

        public override void PostSetupContent()
        {
            ModCompatibility.calamityEnabled = ModLoader.HasMod("CalamityMod");
            ModCompatibility.hypnosEnabled = ModLoader.HasMod("Hypnos");
        }

    }

    public enum PetHypnosMessageType
    {
        SyncMousePos,
        SyncMouseRightClick
    }

    public static class PetHypnosExtensions
    {
        internal static void SendPacket(this Player player, ModPacket packet, bool server)
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
        internal static T Random<T>(this IEnumerable<T> li)
        {
            return li.ElementAt(Main.rand.Next(li.Count()));
        }

        internal static void Reroll(this ref int num, int min, int max)
        {
            num = Main.rand.Next(min, max);
        }

        internal static Delegate ConvertDelegate(this Delegate sourceDelegate, Type targetType)
        {
            return Delegate.CreateDelegate(
                    targetType,
                    sourceDelegate.Target,
                    sourceDelegate.Method);
        }

        internal static T ConvertDelegate<T>(this Delegate sourceDelegate) where T : Delegate
        {
            return (T)sourceDelegate.ConvertDelegate(typeof(T));
        }

        //internal static IEnumerable<Assembly> GetAssemblyByName(this AppDomain domain, string name)
        //{
        //    return domain.GetAssemblies().Where(assembly => assembly.GetName().Name == name);
        //}
    }

    public partial class PetHypnosPlayer : ModPlayer
    {
        private bool shouldSyncMouse = false;

        public bool shouldCheckRightClick = false;
        public bool rightClicked = false;
        private bool rightClickedOld = false;

        public bool shouldCheckMouseWorld = false;
        public Vector2 mouseWorld;
        private Vector2 mouseWorldOld;

        public int idleTime = 0;
        public Vector2 positionOld;
        public Vector2 mouseWorldOldForIdleCheck;

        

        public override void PostUpdateMiscEffects()
        {
            if (Player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient && shouldSyncMouse)
            {
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
                if ((Vector2.Distance(Player.position, positionOld) > 3f || Vector2.Distance(Main.MouseWorld, mouseWorldOldForIdleCheck) > 3f || (Player.noItems ? true : PlayerInput.Triggers.Current.MouseRight || PlayerInput.Triggers.Current.MouseLeft)) || Player.CCed)
                {// && (Player.noItems ? true : PlayerInput.Triggers.Current.MouseRight || PlayerInput.Triggers.Current.MouseLeft)) || Player.CCed
                    positionOld = Player.position;
                    mouseWorldOldForIdleCheck = Main.MouseWorld;
                    idleTime = 0;
                }
                else
                {
                    idleTime++;
                }
            }
        }
    }

    //public static class ModCompatibility
    //{
    //    public static Mod CalamityMod
    //    {
    //        get
    //        {
    //            if (calamityMod == null)
    //            {
    //                ModLoader.TryGetMod("CalamityMod", out calamityMod);
    //            }
    //            return calamityMod;
    //        }
    //    }
    //    private static Mod calamityMod;

    //    public static int VioletOrPurple
    //    {
    //        get
    //        {
    //            if (calamityModVioletID == null && (CalamityMod?.TryFind("Violet", out ModRarity calamityModViolet) ?? false))
    //            {
    //                calamityModVioletID = calamityModViolet.Type;
    //            }
    //            return calamityModVioletID ?? ItemRarityID.Purple;
    //        }
    //    }
    //    private static int? calamityModVioletID = null;

    //    public static Mod Hypnos
    //    {
    //        get
    //        {
    //            if (hypnos == null)
    //            {
    //                ModLoader.TryGetMod("Hypnos", out hypnos);
    //            }
    //            return hypnos;
    //        }
    //    }
    //    private static Mod hypnos;

    //    //public static Dictionary<string, Type> modCompatibilityTypes = new Dictionary<string, Type>()
    //    //{
    //    //    {"BloomRing", null},
    //    //    {"ThisShouldNotExistReallyyyyyyy", null}
    //    //};
    //    public static MethodInfo GetMethodInfo(Delegate d)
    //    {
    //        return d.Method;
    //    }
    //}

    //public class ModCompatibilityType
    //{
    //    public Type Type
    //    {
    //        get
    //        {
    //            if(type== null)
    //            {
    //                type = Assemblies.LastOrDefault()?.GetType(typeName);
                    
    //            }
    //            return type;
    //        }
    //    }
    //    private Type type = null;
    //    public string typeName;

    //    public IEnumerable<Assembly> Assemblies
    //    {
    //        get
    //        {
    //            return AppDomain.CurrentDomain.GetAssemblyByName(assemblyName);
    //        }
    //    }

    //    public string assemblyName;
    //    public bool IsNull
    //    {
    //        get
    //        {
    //            return Type == null;
    //        }
    //    }
    //    public ModCompatibilityType(string typeName, string assemblyName)
    //    {
    //        this.typeName = typeName;
    //        this.assemblyName = assemblyName;
    //    }
    //}

    //public static class ModCompatibilityTypes
    //{
    //    public static readonly string calamityAssemblyName = "CalamityMod";

    //    public static ModCompatibilityType CommonCalamitySounds = new ModCompatibilityType("CalamityMod.Sounds.CommonCalamitySounds", calamityAssemblyName);

    //    public static ModCompatibilityType Particle = new ModCompatibilityType("CalamityMod.Particles.Particle", calamityAssemblyName);
    //    public static ModCompatibilityType GeneralParticleHandler = new ModCompatibilityType("CalamityMod.Particles.GeneralParticleHandler", calamityAssemblyName);

    //    public static ModCompatibilityType BloomRing = new ModCompatibilityType("CalamityMod.Particles.BloomRing", calamityAssemblyName);
    //    public static ModCompatibilityType StrongBloom = new ModCompatibilityType("CalamityMod.Particles.StrongBloom", calamityAssemblyName);
    //    public static ModCompatibilityType PrimitiveTrail = new ModCompatibilityType("CalamityMod.PrimitiveTrail", calamityAssemblyName);
    //    public static ModCompatibilityType AresTeslaOrb = new ModCompatibilityType("CalamityMod.Projectiles.Boss.AresTeslaOrb", calamityAssemblyName);
    //}

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

        public static int HypnosBoss
        {
            get
            {
                if (hypnosBoss == -1 && (ModCompatibility.Hypnos?.TryFind("HypnosBoss", out ModNPC bosstype) ?? false))
                {
                    hypnosBoss = bosstype.Type;
                }
                
                return hypnosBoss;
            }
        }
        private static int hypnosBoss = -1;
        
        public static bool calamityEnabled = false;
        public static bool hypnosEnabled = false;
    }

    public static partial class ModCompatibilityTypes {
    }


    public class PetHypnosRecipes : ModSystem
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
            if (npc.boss)
            {
                //object comp = typeof(Pawn).GetMethod("GetComp").MakeGenericMethod(ModCompatibility.PickUpAndHaul.CompHauledToInventory).Invoke(pawn, null);

                //int hypBossType = typeof(ModContent).GetMethod("NPCType").MakeGenericMethod(hypBoss).Invoke();
                // First, we need to check the npc.type to see if the code is running for the vanilla NPCwe want to change
                if (ModCompatibility.hypnosEnabled)
                {
                    if (npc.type == ModCompatibility.HypnosBoss)
                    {
                        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HypnosPetItem>()));
                    }
                }
                else
                {
                    
                    if (npc.type == NPCID.BrainofCthulhu)
                    {
                        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HypnosPetItem>(), 42));
                    }
                }
                // We can use other if statements here to adjust the drop rules of other vanilla NPC
            }

        }

        public override void OnKill(NPC npc)
        {
            if (npc.boss && npc.type == ModCompatibility.HypnosBoss)
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

    [Label("$Mods.PetHypnos.Config.MainTitle")]
    [BackgroundColor(49, 36, 49, 216)]
    public class PetHypnosConfig : ModConfig
    {
        public static PetHypnosConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("$Mods.PetHypnos.Config.Label.DisableHypnosQuotes")]
        [BackgroundColor(192, 54, 192, 192)]
        [DefaultValue(true)]
        [Tooltip("$Mods.PetHypnos.Config.Tooltip.DisableHypnosQuotes")]
        public bool DisableHypnosQuotes { get; set; }
    }

    public class PetHypnosQuote
    {
        public PetHypnosQuote(string value)
        {
            this.value = value;
        }

        //public HashSet<string> Quotes
        //{
        //    get
        //    {
        //        if (quotes == null)
        //        {
        //            Language.GetTextValue(key);
        //        }
        //        return quotes;
        //    }
        //}
        //private HashSet<string> quotes = null;

        private string value;
        public string Value
        {
            get { return value; }
        }

        private bool cooldown = false;
        public bool Cooldown
        {
            get { return cooldown; }
        }
        public static void HypnosQuote(Projectile projectile, string quote, bool dramatic = false)
        {
            if (PetHypnosConfig.Instance.DisableHypnosQuotes == true)
            {
                return;
            }
            if (Main.myPlayer != projectile.owner)
            {
                return;
            }
            Rectangle displayZone = projectile.Hitbox;
            string[] quotes = quote.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            displayZone.Y -= (quotes.Length - 1) * 30;
            foreach (string s in quotes)
            {
                CombatText.NewText(displayZone, new Color(155, 255, 255), s, dramatic: dramatic);
                displayZone.Y += 30;
            }
        }
        public void EnableCooldown()
        {
            cooldown= true;
        }
        public void DisableCooldown()
        {
            cooldown= false;
        }
    }

    public class PetHypnosQuoteCollection: Collection<PetHypnosQuote>, IEnumerable<PetHypnosQuote>
    {
        //public PetHypnosQuoteHashSet(IEnumerable<string> quotes): base(quotes.Select(st => new PetHypnosQuote(st))) { }
        private string RandomQuote(int counter)
        {
            if (counter > 100)
            {
                throw new StackOverflowException();
            }
            IEnumerable<PetHypnosQuote> hashset = this.Where(quote => !quote.Cooldown);
            if (!hashset.Any())
            {
                DisableAllCooldown();
                return RandomQuote(counter+1);
            }
            PetHypnosQuote chosen = hashset.Random();
            chosen.EnableCooldown();
            return chosen.Value;
        }

        public string RandomQuote()
        {
            return RandomQuote(0);
        }

        public void DisableAllCooldown()
        {
            foreach(PetHypnosQuote quote in this)
            {
                quote.DisableCooldown();
            }
        }

        public void Add(string st)
        {
            this.Add(new PetHypnosQuote(st));
        }
    }

    public static class PetHypnosQuotes
    {
        public static PetHypnosQuoteCollection buffTooltip = new PetHypnosQuoteCollection() {
            "Tiny Hypnos' assault on Thanatos keep", //小修普诺斯强袭塔纳堡
            "The day you went away",
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
            "42",
            "I truly present here",
            "Your ip has been banned for INFINITE",
            "Your best cutter",
            "What is this? An amnesia spray? Try it.",
            "You wake up to more nightmares",
            "3 body problem",
            "Y2000 Mindcrash",
            "EATEOT"
        };
        public static PetHypnosQuoteCollection toystaffAttack = new PetHypnosQuoteCollection()
        {
            "Ladies✰ and gentlemen.",
            "Let's start.",
            "Close your eyes.",
            "Tonight.",
            "Mahoshojo✰ ikuzo!",
            "Suki suki daisuki✰",
            "Daisuke✰",
            "SOMEBODY'S SCREEEEAM!",
            "BURN ALL THE BABIES!!!!!",
            //"Yo.",
            "Oh haiiii!",
            //"Kill. Kill. Kill. Kill. Kill. Kill. Kill.",
            //"Oh my pretty pretty boy✰",
            "Ring-a-round the roses.",
            "Ashes✰ Ashes✰ You. Down.",
            "Divano messia.",
            "Blessed are the dead who die in the Lord!",
            "Merry Christmas.",
            "Nya Poka.",
            "Your best cutter✰",
            "Are you ready✰",
            "You wake up to more nightmares.",
        };

        public static PetHypnosQuoteCollection appear = new PetHypnosQuoteCollection() {
            "Wild✰Hypnos appeared!",
            "Oh haiiii!",
            "Glitter✰ landing!",
            "Yo, player!",
            "For ya.",
            "Player✰",
            "More cutter.",
            "Are you ready✰"
        };
        public static PetHypnosQuoteCollection idle = new PetHypnosQuoteCollection {
            "Ahead loci gibbuses ordain wrong sect...",
            "Once upon a time...",
            "PLAYER. Will you leave?",
            "Player. player. PLAYER. PLAYER. PLAYER.",
            "What do you know\nWhat do you play\nWhat do you remember\nWhat do you love",
            "I think, therefore i am.",
            "PLAYER. I truly present here.",
            "Ring-a-round the roses.\nPocket full of posies.",
            "A gray room...\nA gray life...\nA dusty hair dryer...",
            "Ashes to ashes, dust to dust.",
            "Do android brain dream of electric serpent?",
            "You wake up to more nightmares.",
        };
        public static PetHypnosQuoteCollection becomeStressed = new PetHypnosQuoteCollection()
        {
            "There must be something strange about things going wrong.",
            "Interesting✰",
            "Heaven brings forth innumerable things to nurture man.",
            "Are you ready✰",
            "Rec ✰ Hypnos Live",
            "This will be the world's most wonderful fight!",
            "This will be the world's second most wonderful fight!"
        };
        public static PetHypnosQuoteCollection becomeStressedWithItself = new PetHypnosQuoteCollection()
        {
            "Let's start.",
            "Close your eyes.",
            "Neuron✰ Blast!",
            "Y2000 Mindcrash!",
            "Just try to take me down✰",
            "Are you ready✰",
            "Ashes✰ Ashes✰ You. Down."
        };
    }
}