using System.Windows.Forms;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace BuffUtil
{
    public class BuffUtilSettings : SettingsBase
    {
        public BuffUtilSettings()
        {
            BloodRage = new ToggleNode(false);
            BloodRageKey = new HotkeyNode(Keys.E);
            BloodRageConnectedSkill = new RangeNode<int>(1, 1, 8);
            BloodRageMaxHP = new RangeNode<int>(100, 0, 100);
            BloodRageMaxMP = new RangeNode<int>(100, 0, 100);

            SteelSkin = new ToggleNode(false);
            SteelSkinKey = new HotkeyNode(Keys.W);
            SteelSkinConnectedSkill = new RangeNode<int>(1, 1, 8);
            SteelSkinMaxHP = new RangeNode<int>(90, 0, 100);

            BladeFlurry = new ToggleNode(false);
            BladeFlurryMinCharges = new RangeNode<int>(6, 1, 6);
            BladeFlurryUseLeftClick = new ToggleNode(false);
            BladeFlurryWaitForInfused = new ToggleNode(true);

            RequireMinMonsterCount = new ToggleNode(false);
            NearbyMonsterCount = new RangeNode<int>(1, 1, 30);
            NearbyMonsterMaxDistance = new RangeNode<int>(500, 1, 2000);
            Debug = new ToggleNode(false);
            DisableInHideout = new ToggleNode(true);
        }

        #region Debug

        [Menu("Debug", "Print debug messages?", 4)]
        public ToggleNode Debug { get; set; }

        #endregion

        #region Blood Rage

        [Menu("Blood Rage", 1)] public ToggleNode BloodRage { get; set; }

        [Menu("Blood Rage Key", "Which key to press to activate Blood Rage?", 11, 1)]
        public HotkeyNode BloodRageKey { get; set; }


        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 12, 1)]
        public RangeNode<int> BloodRageConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 13, 1)]
        public RangeNode<int> BloodRageMaxHP { get; set; }

        [Menu("Max Mana", "Mana percent above which skill is not cast", 14, 1)]
        public RangeNode<int> BloodRageMaxMP { get; set; }

        #endregion

        #region Steel Skin

        [Menu("Steel Skin", 2)] public ToggleNode SteelSkin { get; set; }

        [Menu("Steel Skin Key", 21, 2)] public HotkeyNode SteelSkinKey { get; set; }

        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 22, 2)]
        public RangeNode<int> SteelSkinConnectedSkill { get; set; }

        [Menu("Max HP", "HP percent above which skill is not cast", 23, 2)]
        public RangeNode<int> SteelSkinMaxHP { get; set; }

        #endregion

        #region Blade Flurry

        [Menu("Blade Flurry", "Use mouse click to release Blade Flurry charges", 3)] public ToggleNode BladeFlurry { get; set; }

        [Menu("Min charges", "Minimal amount of BF charges to release", 31, 3)]
        public RangeNode<int> BladeFlurryMinCharges { get; set; }

        [Menu("Use left click", "Use left click instead of right click to release charges", 32, 3)] 
        public ToggleNode BladeFlurryUseLeftClick { get; set; }
        
        [Menu("Wait for Infused Channeling buff", "Wait for Infused Channeling buff before release", 33, 3)] 
        public ToggleNode BladeFlurryWaitForInfused { get; set; }

        #endregion

        #region Misc

        [Menu("Misc", 10)] public EmptyNode MiscSettings { get; set; }

        [Menu("Nearby monsters", "Require a minimum count of nearby monsters to cast buffs?", 101, 10)]
        public ToggleNode RequireMinMonsterCount { get; set; }

        [Menu("Range", "Minimum count of nearby monsters to cast", 102, 10)]
        public RangeNode<int> NearbyMonsterCount { get; set; }

        [Menu("Range", "Max distance of monsters to player to count as nearby", 103, 10)]
        public RangeNode<int> NearbyMonsterMaxDistance { get; set; }

        [Menu("Disable in hideout", "Disable the plugin in hideout?", 103, 10)]
        public ToggleNode DisableInHideout { get; set; }

        #endregion
    }
}