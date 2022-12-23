using System.Windows.Forms;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Attributes;

namespace BuffUtil
{
    public class BuffUtilSettings : ISettings
    {
        public BuffUtilSettings()
        {
            Enable = new ToggleNode(false);
            BloodRage = new ToggleNode(false);
            BloodRageKey = new HotkeyNode(Keys.E);
            BloodRageMaxHP = new RangeNode<int>(100, 0, 100);
            BloodRageMaxMP = new RangeNode<int>(100, 0, 100);

            SteelSkin = new ToggleNode(false);
            SteelSkinKey = new HotkeyNode(Keys.W);
            SteelSkinMaxHP = new RangeNode<int>(90, 0, 100);

            ImmortalCall = new ToggleNode(false);
            ImmortalCallKey = new HotkeyNode(Keys.T);
            ImmortalCallMaxHP = new RangeNode<int>(50, 0, 100);

            MoltenShell = new ToggleNode(false);
            MoltenShellKey = new HotkeyNode(Keys.Q);
            MoltenShellMaxHP = new RangeNode<int>(50, 0, 100);

            PhaseRun = new ToggleNode(false);
            PhaseRunKey = new HotkeyNode(Keys.R);
            PhaseRunMaxHP = new RangeNode<int>(90, 0, 100);
            PhaseRunMinMoveTime = new RangeNode<int>(0, 0, 5000);
            PhaseRunMinBVStacks = new RangeNode<int>(0, 0, 10);

            WitheringStep = new ToggleNode(false);
            WitheringStepKey = new HotkeyNode(Keys.R);
            WitheringStepMaxHP = new RangeNode<int>(90, 0, 100);
            WitheringStepMinMoveTime = new RangeNode<int>(0, 0, 5000);

            Berserk = new ToggleNode(false);
            BerserkKey = new HotkeyNode(Keys.R);
            BerserkMinRage = new RangeNode<int>(5, 5, 50);
            BerserkNearbyEnemiesCount = new RangeNode<int>(1, 0, 50);
            BerserkUseOnUniqueBoss = new ToggleNode(true);

            Warcry = new ToggleNode(false);
            WarcryKey = new HotkeyNode(Keys.R);
            WarCryMinRage = new RangeNode<int>(0, 0, 50);
            WarCryMaxRage = new RangeNode<int>(24, 0, 100);
            WarcryNearbyEnemiesCount = new RangeNode<int>(1, 0, 50);
            WarcryUseOnUniqueBoss = new ToggleNode(true);

            BladeFlurry = new ToggleNode(false);
            BladeFlurryMinCharges = new RangeNode<int>(6, 1, 6);
            BladeFlurryUseLeftClick = new ToggleNode(false);
            BladeFlurryWaitForInfused = new ToggleNode(true);

            ScourgeArrow = new ToggleNode(false);
            ScourgeArrowMinCharges = new RangeNode<int>(5, 1, 6);
            ScourgeArrowUseLeftClick = new ToggleNode(false);
            ScourgeArrowWaitForInfused = new ToggleNode(true);

            RequireMinMonsterCount = new ToggleNode(false);
            NearbyMonsterCount = new RangeNode<int>(1, 1, 30);
            NearbyMonsterMaxDistance = new RangeNode<int>(500, 1, 2000);
            DisableInHideout = new ToggleNode(true);
            Debug = new ToggleNode(false);
            SilenceErrors = new ToggleNode(false);
        }

        #region Blood Rage

        public ToggleNode Enable { get; set; }

        [Menu("Blood Rage", 1)] public ToggleNode BloodRage { get; set; }

        [Menu("Blood Rage Key", "Which key to press to activate Blood Rage?", 11, 1)]
        public HotkeyNode BloodRageKey { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 12, 1)]
        public RangeNode<int> BloodRageMaxHP { get; set; }

        [Menu("Max Mana", "Mana percent above which skill is not cast", 13, 1)]
        public RangeNode<int> BloodRageMaxMP { get; set; }

        #endregion

        #region Steel Skin

        [Menu("Steel Skin", 2)] public ToggleNode SteelSkin { get; set; }

        [Menu("Steel Skin Key", 21, 2)] public HotkeyNode SteelSkinKey { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 22, 2)]
        public RangeNode<int> SteelSkinMaxHP { get; set; }

        #endregion

        #region Immortal Call

        [Menu("Immortal Call", 3)] public ToggleNode ImmortalCall { get; set; }

        [Menu("Immortal Call Key", 31, 3)] public HotkeyNode ImmortalCallKey { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 32, 3)]
        public RangeNode<int> ImmortalCallMaxHP { get; set; }

        #endregion

        #region Molten Shell

        [Menu("Molten Shell", 4)] public ToggleNode MoltenShell { get; set; }

        [Menu("Molten Shell Key", 41, 4)] public HotkeyNode MoltenShellKey { get; set; }


        [Menu("Max HP", "HP percent above which skill is not cast", 42, 4)]
        public RangeNode<int> MoltenShellMaxHP { get; set; }

        #endregion

        #region Phase Run

        [Menu("Phase Run", 5)] public ToggleNode PhaseRun { get; set; }

        [Menu("Phase Run Key", 51, 5)] public HotkeyNode PhaseRunKey { get; set; }


        [Menu("Max HP", "HP percent above which skill is not cast", 52, 5)]
        public RangeNode<int> PhaseRunMaxHP { get; set; }

        [Menu("Move time", "Time in ms spent moving after which skill can be cast", 53, 5)]
        public RangeNode<int> PhaseRunMinMoveTime { get; set; }

        [Menu("BV Stacks", "Blade Vortex stacks required to cast Phase Run", 54, 5)]
        public RangeNode<int> PhaseRunMinBVStacks { get; set; }

        #endregion

        #region Withering Step

        [Menu("Withering Step", 6)] public ToggleNode WitheringStep { get; set; }

        [Menu("Withering Step Key", 61, 6)] public HotkeyNode WitheringStepKey { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 62, 6)]
        public RangeNode<int> WitheringStepMaxHP { get; set; }

        [Menu("Move time", "Time in ms spent moving after which skill can be cast", 63, 6)]
        public RangeNode<int> WitheringStepMinMoveTime { get; set; }

        #endregion

        #region Berserk

        [Menu("Berserk", 7)] public ToggleNode Berserk { get; set; }

        [Menu("Berserk Key", 71, 7)] public HotkeyNode BerserkKey { get; set; }

        [Menu("Min Rage", "Minimum amount of Rage charges to use", 72, 7)]
        public RangeNode<int> BerserkMinRage { get; set; }

        [Menu("Nearby monsters", "Nearby enemy monsters count to use", 73, 7)]
        public RangeNode<int> BerserkNearbyEnemiesCount { get; set; }

        [Menu("Unique bosses", "Use Berserk on Unique Bosses", 74, 7)]
        public ToggleNode BerserkUseOnUniqueBoss { get; set; }

        #endregion

        #region Warcry

        [Menu("Warcry", 8)] public ToggleNode Warcry { get; set; }

        [Menu("Warcry Key", 81, 8)] public HotkeyNode WarcryKey { get; set; }


        [Menu("MinRage", "Use if at least X amount of Rage charges", 82, 8)]
        public RangeNode<int> WarCryMinRage { get; set; }

        [Menu("Max Rage", "Use if at most X amount of Rage charges", 83, 8)]
        public RangeNode<int> WarCryMaxRage { get; set; }

        [Menu("Nearby monsters", "Nearby enemy monsters count to use", 84, 8)]
        public RangeNode<int> WarcryNearbyEnemiesCount { get; set; }

        [Menu("Unique bosses", "Use Berserk on Unique Bosses", 85, 8)]
        public ToggleNode WarcryUseOnUniqueBoss { get; set; }

        #endregion

        #region Blade Flurry

        [Menu("Blade Flurry", "Use mouse click to release Blade Flurry charges", 9)]
        public ToggleNode BladeFlurry { get; set; }

        [Menu("Min charges", "Minimal amount of BF charges to release", 91, 9)]
        public RangeNode<int> BladeFlurryMinCharges { get; set; }

        [Menu("Use left click", "Use left click instead of right click to release charges", 92, 9)]
        public ToggleNode BladeFlurryUseLeftClick { get; set; }

        [Menu("Wait for Infused Channeling buff", "Wait for Infused Channeling buff before release", 93, 9)]
        public ToggleNode BladeFlurryWaitForInfused { get; set; }

        #endregion

        #region Scourge Arrow

        [Menu("Scourge Arrow", "Use mouse click to release Scourge Arrow charges", 10)]
        public ToggleNode ScourgeArrow { get; set; }

        [Menu("Min charges", "Minimal amount of BF charges to release", 101, 10)]
        public RangeNode<int> ScourgeArrowMinCharges { get; set; }

        [Menu("Use left click", "Use left click instead of right click to release charges", 102, 10)]
        public ToggleNode ScourgeArrowUseLeftClick { get; set; }

        [Menu("Wait for Infused Channeling buff", "Wait for Infused Channeling buff before release", 103, 10)]
        public ToggleNode ScourgeArrowWaitForInfused { get; set; }

        #endregion

        #region Misc

        [Menu("Misc", 12)] public EmptyNode MiscSettings { get; set; }

        [Menu("Nearby monsters", "Require a minimum count of nearby monsters to cast buffs?", 121, 12)]
        public ToggleNode RequireMinMonsterCount { get; set; }

        [Menu("Range", "Minimum count of nearby monsters to cast", 122, 12)]
        public RangeNode<int> NearbyMonsterCount { get; set; }

        [Menu("Range", "Max distance of monsters to player to count as nearby", 123, 12)]
        public RangeNode<int> NearbyMonsterMaxDistance { get; set; }

        [Menu("Disable in hideout", "Disable the plugin in hideout?", 124, 12)]
        public ToggleNode DisableInHideout { get; set; }

        [Menu("Debug", "Print debug messages?", 125, 12)]
        public ToggleNode Debug { get; set; }

        [Menu("Silence errors", "Hide error messages?", 126, 12)]
        public ToggleNode SilenceErrors { get; set; }

        #endregion
    }
}