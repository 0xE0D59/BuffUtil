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

            SteelSkin = new ToggleNode(false);
            SteelSkinKey = new HotkeyNode(Keys.W);
            SteelSkinConnectedSkill = new RangeNode<int>(1, 1, 8);

            RequireMinMonsterCount = new ToggleNode(false);
            NearbyMonsterCount = new RangeNode<int>(1, 1, 30);
            NearbyMonsterMaxDistance = new RangeNode<int>(500, 1, 2000);
            Debug = new ToggleNode(false);
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

        #endregion

        #region Steel Skin

        [Menu("Steel Skin", 2)] public ToggleNode SteelSkin { get; set; }

        [Menu("Steel Skin Key", 21, 2)] public HotkeyNode SteelSkinKey { get; set; }

        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 22, 2)]
        public RangeNode<int> SteelSkinConnectedSkill { get; set; }

        #endregion

        #region Misc

        [Menu("Misc", 3)] public EmptyNode MiscSettings { get; set; }

        [Menu("Nearby monsters", "Require a minimum count of nearby monsters to cast buffs?", 30, 3)]
        public ToggleNode RequireMinMonsterCount { get; set; }

        [Menu("Range", "Minimum count of nearby monsters to cast", 31, 3)]
        public RangeNode<int> NearbyMonsterCount { get; set; }

        [Menu("Range", "Max distance of monsters to player to count as nearby", 32, 3)]
        public RangeNode<int> NearbyMonsterMaxDistance { get; set; }

        #endregion
    }
}