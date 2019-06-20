using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;
using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;

namespace BuffUtil
{
    public class BuffUtil : BaseSettingsPlugin<BuffUtilSettings>
    {
        private const string kSteelSkinBuffName = "steelskin";
        private const string kSteelSkinName = "QuicKGuard";
        private const string kSteelSkinInternalName = "steelskin";

        private const string kImmortalCallBuffName = "mortal_call";
        private const string kImmortalCallName = "ImmortalCall";
        private const string kImmortalCallInternalName = "mortal_call";

        private const string kMoltenShellBuffName = "molten_shell_shield";
        private const string kMoltenShellName = "MoltenShell";
        private const string kMoltenShellInternalName = "molten_shell_barrier";

        private const string kPhaseRunBuffName1 = "new_phase_run";
        private const string kPhaseRunBuffName2 = "new_phase_run_damage";
        private const string kPhaseRunName = "NewPhaseRun";
        private const string kPhaseRunInternalName = "new_phase_run";

        private const string kBloodRageBuffName = "blood_rage";
        private const string kBloodRageName = "BloodRage";
        private const string kBloodRageInternalName = "blood_rage";

        private const string kBladeFlurryBuffName = "charged_attack";
        private const string kInfusedChannelingBuffName = "storm_barrier_support_damage";

        private const string kScourgeArrowBuffName = "virulent_arrow_counter";

        private const string kGracePeriodBuffName = "grace_period";

        private static readonly TimeSpan kSteelSkinMinTimeBetweenCasts = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan kImmortalCallMinTimeBetweenCasts = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan kMoltenShellMinTimeBetweenCasts = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan kBloodRageMinTimeBetweenCasts = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan kPhaseRunMinTimeBetweenCasts = TimeSpan.FromSeconds(1);

        private readonly HashSet<EntityWrapper> loadedMonsters = new HashSet<EntityWrapper>();
        private readonly object loadedMonstersLock = new object();

        private List<Buff> buffs;
        private List<ActorSkill> skills;
        private DateTime? currentTime;
        private InputSimulator inputSimulator;
        private Random rand;
        private DateTime? lastBloodRageCast;
        private DateTime? lastPhaseRunCast;
        private DateTime? lastSteelSkinCast;
        private DateTime? lastImmortalCallCast;
        private DateTime? lastMoltenShellCast;
        private float HPPercent;
        private float MPPercent;
        private int? nearbyMonsterCount;
        private bool showErrors = true;

        public override void Initialise()
        {
            base.Initialise();
            PluginName = "BuffUtil";
            inputSimulator = new InputSimulator();
            rand = new Random();

            showErrors = !Settings.SilenceErrors;
            Settings.SilenceErrors.OnValueChanged += delegate { showErrors = !Settings.SilenceErrors; };
        }

        public override void OnPluginDestroyForHotReload()
        {
            if (loadedMonsters != null)
                lock (loadedMonstersLock)
                {
                    loadedMonsters.Clear();
                }

            base.OnPluginDestroyForHotReload();
        }

        public override void Render()
        {
            if (OnPreExecute())
                OnExecute();
            OnPostExecute();
        }

        private void OnExecute()
        {
            try
            {
                HandleBladeFlurry();
                HandleScourgeArrow();
                HandleBloodRage();
                HandleSteelSkin();
                HandleImmortalCall();
                HandleMoltenShell();
                HandlePhaseRun();
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(OnExecute)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleBladeFlurry()
        {
            try
            {
                if (!Settings.BladeFlurry)
                    return;

                var stacksBuff = GetBuff(kBladeFlurryBuffName);
                if (stacksBuff == null)
                    return;

                var charges = stacksBuff.Charges;
                if (charges < Settings.BladeFlurryMinCharges.Value)
                    return;

                if (Settings.BladeFlurryWaitForInfused)
                {
                    var hasInfusedBuff = HasBuff(kInfusedChannelingBuffName);
                    if (!hasInfusedBuff.HasValue || !hasInfusedBuff.Value)
                        return;
                }

                if (Settings.Debug)
                    LogMessage($"Releasing Blade Flurry at {charges} charges.", 1);

                if (Settings.BladeFlurryUseLeftClick)
                {
                    inputSimulator.Mouse.LeftButtonUp();
                    inputSimulator.Mouse.LeftButtonDown();
                }
                else
                {
                    inputSimulator.Mouse.RightButtonUp();
                    inputSimulator.Mouse.RightButtonDown();
                }
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleBloodRage)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleScourgeArrow()
        {
            try
            {
                if (!Settings.ScourgeArrow)
                    return;

                var stacksBuff = GetBuff(kScourgeArrowBuffName);
                if (stacksBuff == null)
                    return;

                var charges = stacksBuff.Charges;
                if (charges < Settings.ScourgeArrowMinCharges.Value)
                    return;

                if (Settings.ScourgeArrowWaitForInfused)
                {
                    var hasInfusedBuff = HasBuff(kInfusedChannelingBuffName);
                    if (!hasInfusedBuff.HasValue || !hasInfusedBuff.Value)
                        return;
                }

                if (Settings.Debug)
                    LogMessage($"Releasing Scourge Arrow at {charges} charges.", 1);

                if (Settings.ScourgeArrowUseLeftClick)
                {
                    inputSimulator.Mouse.LeftButtonUp();
                    inputSimulator.Mouse.LeftButtonDown();
                }
                else
                {
                    inputSimulator.Mouse.RightButtonUp();
                    inputSimulator.Mouse.RightButtonDown();
                }
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleScourgeArrow)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleBloodRage()
        {
            try
            {
                if (!Settings.BloodRage)
                    return;

                if (lastBloodRageCast.HasValue && currentTime - lastBloodRageCast.Value <
                    kBloodRageMinTimeBetweenCasts)
                    return;

                if (HPPercent > Settings.BloodRageMaxHP.Value || MPPercent > Settings.BloodRageMaxMP)
                    return;

                var hasBuff = HasBuff(kBloodRageBuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(kBloodRageName, kBloodRageInternalName,
                    Settings.BloodRageConnectedSkill.Value);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Blood Rage - not found in usable skills.", 1);
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Blood Rage", 1);
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode) Settings.BloodRageKey.Value);
                lastBloodRageCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.2));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleBloodRage)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleSteelSkin()
        {
            try
            {
                if (!Settings.SteelSkin)
                    return;

                if (lastSteelSkinCast.HasValue && currentTime - lastSteelSkinCast.Value <
                    kSteelSkinMinTimeBetweenCasts)
                    return;

                if (HPPercent > Settings.SteelSkinMaxHP.Value)
                    return;

                var hasBuff = HasBuff(kSteelSkinBuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(kSteelSkinName, kSteelSkinInternalName,
                    Settings.SteelSkinConnectedSkill.Value);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Steel Skin - not found in usable skills.", 1);
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Steel Skin", 1);
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode) Settings.SteelSkinKey.Value);
                lastSteelSkinCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.2));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleSteelSkin)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleImmortalCall()
        {
            try
            {
                if (!Settings.ImmortalCall)
                    return;

                if (lastImmortalCallCast.HasValue && currentTime - lastImmortalCallCast.Value <
                    kImmortalCallMinTimeBetweenCasts)
                    return;

                if (HPPercent > Settings.ImmortalCallMaxHP.Value)
                    return;

                var hasBuff = HasBuff(kImmortalCallBuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(kImmortalCallName, kImmortalCallInternalName,
                    Settings.ImmortalCallConnectedSkill.Value);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Immortal Call - not found in usable skills.", 1);
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Immortal Call", 1);
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode) Settings.ImmortalCallKey.Value);
                lastImmortalCallCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.2));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleImmortalCall)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleMoltenShell()
        {
            try
            {
                if (!Settings.MoltenShell)
                    return;

                if (lastMoltenShellCast.HasValue && currentTime - lastMoltenShellCast.Value <
                    kMoltenShellMinTimeBetweenCasts)
                    return;

                if (HPPercent > Settings.MoltenShellMaxHP.Value)
                    return;

                var hasBuff = HasBuff(kMoltenShellBuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(kMoltenShellName, kMoltenShellInternalName,
                    Settings.MoltenShellConnectedSkill.Value);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Molten Shell - not found in usable skills.", 1);
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Molten Shell", 1);
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode) Settings.MoltenShellKey.Value);
                lastMoltenShellCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.2));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleMoltenShell)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandlePhaseRun()
        {
            try
            {
                if (!Settings.PhaseRun)
                    return;

                if (lastPhaseRunCast.HasValue && currentTime - lastPhaseRunCast.Value <
                    kPhaseRunMinTimeBetweenCasts)
                    return;

                if (HPPercent > Settings.PhaseRunMaxHP.Value)
                    return;

                var hasBuff = HasBuff(kPhaseRunBuffName1);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(kPhaseRunName, kPhaseRunInternalName,
                    Settings.PhaseRunConnectedSkill.Value);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Phase Run - not found in usable skills.", 1);
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Phase Run", 1);
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode) Settings.PhaseRunKey.Value);
                lastPhaseRunCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.2));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleSteelSkin)}: {ex.StackTrace}", 3f);
            }
        }

        private bool OnPreExecute()
        {
            try
            {
                if (!Settings.Enable)
                    return false;
                var inTown = GameController.Area.CurrentArea.IsTown;
                if (inTown)
                    return false;
                if (Settings.DisableInHideout && GameController.Area.CurrentArea.IsHideout)
                    return false;
                var player = GameController.Game.IngameState.Data.LocalPlayer;
                var playerLife = player.GetComponent<Life>();
                var isDead = playerLife.CurHP <= 0;
                if (isDead)
                    return false;

                buffs = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>().Buffs;
                if (buffs == null)
                    return false;

                var gracePeriod = HasBuff(kGracePeriodBuffName);
                if (!gracePeriod.HasValue || gracePeriod.Value)
                    return false;

                skills = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Actor>().ActorSkills;
                if (skills == null || skills.Count == 0)
                    return false;

                currentTime = DateTime.UtcNow;

                HPPercent = 100f * playerLife.HPPercentage;
                MPPercent = 100f * playerLife.MPPercentage;

                return true;
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(OnPreExecute)}: {ex.StackTrace}", 3f);
                return false;
            }
        }

        private void OnPostExecute()
        {
            try
            {
                buffs = null;
                skills = null;
                currentTime = null;
                nearbyMonsterCount = null;
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(OnPostExecute)}: {ex.StackTrace}", 3f);
            }
        }

        private bool? HasBuff(string buffName)
        {
            if (buffs == null)
            {
                if (showErrors)
                    LogError("Requested buff check, but buff list is empty.", 1);
                return null;
            }

            return buffs.Any(b => string.Compare(b.Name, buffName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private Buff GetBuff(string buffName)
        {
            if (buffs == null)
            {
                if (showErrors)
                    LogError("Requested buff retrieval, but buff list is empty.", 1);
                return null;
            }

            return buffs.FirstOrDefault(b => string.Compare(b.Name, buffName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private ActorSkill GetUsableSkill(string skillName, string skillInternalName, int skillSlotIndex)
        {
            if (skills == null)
            {
                if (showErrors)
                    LogError("Requested usable skill, but skill list is empty.", 1);
                return null;
            }

            return skills.FirstOrDefault(s =>
                (s.Name == skillName || s.InternalName == skillInternalName) && s.CanBeUsed && s.AllowedToCast &&
                s.SkillSlotIndex == skillSlotIndex - 1);
        }

        private bool NearbyMonsterCheck()
        {
            if (!Settings.RequireMinMonsterCount.Value)
                return true;

            if (nearbyMonsterCount.HasValue)
                return nearbyMonsterCount.Value >= Settings.NearbyMonsterCount;

            var playerPosition = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Render>().Pos;

            List<EntityWrapper> localLoadedMonsters;
            lock (loadedMonstersLock)
            {
                localLoadedMonsters = new List<EntityWrapper>(loadedMonsters);
            }

            var maxDistance = Settings.NearbyMonsterMaxDistance.Value;
            var maxDistanceSquared = maxDistance * maxDistance;
            var monsterCount = 0;
            foreach (var monster in localLoadedMonsters)
                if (IsValidNearbyMonster(monster, playerPosition, maxDistanceSquared))
                    monsterCount++;

            nearbyMonsterCount = monsterCount;


            var result = nearbyMonsterCount.Value >= Settings.NearbyMonsterCount;
            if (Settings.Debug.Value && !result)
                LogMessage("NearbyMonstersCheck failed.", 1);
            return result;
        }

        private bool IsValidNearbyMonster(EntityWrapper monster, Vector3 playerPosition, int maxDistanceSquared)
        {
            try
            {
                if (!monster.IsDamageableMonster())
                    return false;

                var monsterPosition = monster.Pos;

                var xDiff = playerPosition.X - monsterPosition.X;
                var yDiff = playerPosition.Y - monsterPosition.Y;
                var monsterDistanceSquare = xDiff * xDiff + yDiff * yDiff;

                return monsterDistanceSquare <= maxDistanceSquared;
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(IsValidNearbyMonster)}: {ex.StackTrace}", 3f);
                return false;
            }
        }

        public override void EntityAdded(EntityWrapper entityWrapper)
        {
            if (!entityWrapper.IsMonster()) return;

            lock (loadedMonstersLock)
            {
                loadedMonsters.Add(entityWrapper);
            }
        }

        public override void EntityRemoved(EntityWrapper entityWrapper)
        {
            lock (loadedMonstersLock)
            {
                loadedMonsters.Remove(entityWrapper);
            }
        }
    }
}