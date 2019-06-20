using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;
using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace BuffUtil
{
    public class BuffUtil : BaseSettingsPlugin<BuffUtilSettings>
    {
        private const string kSteelSkinBuffName = "steelskin";
        private const string kSteelSkinName = "QuicKGuard";
        private const string kSteelSkinInternalName = "steelskin";
        
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

        private static readonly TimeSpan kSteelSkinMinTimeBetweenCasts = TimeSpan.FromSeconds(4.5);
        private static readonly TimeSpan kBloodRageMinTimeBetweenCasts = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan kPhaseRunMinTimeBetweenCasts = TimeSpan.FromSeconds(4);
        private static readonly TimeSpan kExtraMinTime = TimeSpan.FromSeconds(0.15);
        private readonly HashSet<EntityWrapper> loadedMonsters = new HashSet<EntityWrapper>();
        private readonly object loadedMonstersLock = new object();

        private List<Buff> buffs;
        private DateTime? currentTime;

        private InputSimulator inputSimulator;
        private DateTime? lastBloodRageCast;
        private DateTime? lastSteelSkinCast;
        private DateTime? lastPhaseRunCast;
        private int? nearbyMonsterCount;
        private List<ActorSkill> skills;
        private float HPPercent;
        private float MPPercent;

        public override void Initialise()
        {
            base.Initialise();
            PluginName = "BuffUtil";
            inputSimulator = new InputSimulator();
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
                HandlePhaseRun();
            }
            catch (Exception ex)
            {
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
                    kBloodRageMinTimeBetweenCasts + kExtraMinTime)
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
                lastBloodRageCast = currentTime;
            }
            catch (Exception ex)
            {
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
                    kSteelSkinMinTimeBetweenCasts + kExtraMinTime)
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
                lastSteelSkinCast = currentTime;
            }
            catch (Exception ex)
            {
                LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleSteelSkin)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandlePhaseRun()
        {
            try
            {
                if (!Settings.PhaseRun)
                    return;

                if (lastPhaseRunCast.HasValue && currentTime - lastPhaseRunCast.Value <
                    kPhaseRunMinTimeBetweenCasts + kExtraMinTime)
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
                lastPhaseRunCast = currentTime;
            }
            catch (Exception ex)
            {
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
                LogError($"Exception in {nameof(BuffUtil)}.{nameof(OnPostExecute)}: {ex.StackTrace}", 3f);
            }
        }

        private bool? HasBuff(string buffName)
        {
            if (buffs == null)
            {
                LogError("Requested buff check, but buff list is empty.", 1);
                return null;
            }

            return buffs.Any(b => string.Compare(b.Name, buffName, StringComparison.OrdinalIgnoreCase) == 0);
        }
        
        private Buff GetBuff(string buffName)
        {
            if (buffs == null)
            {
                LogError("Requested buff retrieval, but buff list is empty.", 1);
                return null;
            }

            return buffs.FirstOrDefault(b => string.Compare(b.Name, buffName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private ActorSkill GetUsableSkill(string skillName, string skillInternalName, int skillSlotIndex)
        {
            if (skills == null)
            {
                LogError("Requested usable skill, but skill list is empty.", 1);
                return null;
            }

            return skills.FirstOrDefault(s =>
                (s.Name == skillName || s.InternalName == skillInternalName) && s.CanBeUsed &&
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
            var maxDistanceSquare = maxDistance * maxDistance;
            var monsterCount = 0;
            foreach (var monster in localLoadedMonsters)
            {
                if (!monster.HasComponent<Monster>() || !monster.IsValid || !monster.IsAlive || !monster.IsHostile ||
                    monster.Invincible || monster.CannotBeDamaged)
                    continue;

                var monsterPosition = monster.Pos;

                var xDiff = playerPosition.X - monsterPosition.X;
                var yDiff = playerPosition.Y - monsterPosition.Y;
                var monsterDistanceSquare = xDiff * xDiff + yDiff * yDiff;

                if (monsterDistanceSquare <= maxDistanceSquare) monsterCount++;
            }

            nearbyMonsterCount = monsterCount;


            var result = nearbyMonsterCount.Value >= Settings.NearbyMonsterCount;
            if (Settings.Debug.Value && !result)
                LogMessage("NearbyMonstersCheck failed.", 1);
            return result;
        }

        public override void EntityAdded(EntityWrapper entityWrapper)
        {
            if (entityWrapper.HasComponent<Monster>())
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