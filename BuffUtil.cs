using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;

namespace BuffUtil
{
    public class BuffUtil : BaseSettingsPlugin<BuffUtilSettings>
    {
        private readonly HashSet<Entity> loadedMonsters = new HashSet<Entity>();
        private readonly object loadedMonstersLock = new object();

        private List<Buff> buffs;
        private List<ActorSkill> skills;
        private DateTime? currentTime;
        private InputSimulator inputSimulator;
        private Random rand;
        private DateTime? lastBloodRageCast;
        private DateTime? lastPhaseRunCast;
        private DateTime? lastWitheringStepCast;
        private DateTime? lastSteelSkinCast;
        private DateTime? lastImmortalCallCast;
        private DateTime? lastMoltenShellCast;
        private DateTime? lastWarcryCast;
        private DateTime? lastBerserkCast;
        private float HPPercent;
        private float MPPercent;
        private int? nearbyMonsterCount;
        private int? rageBuffCount;
        private bool? uniqueBossNearby;
        private bool showErrors = true;
        private Stopwatch movementStopwatch { get; set; } = new Stopwatch();

        public override bool Initialise()
        {
            inputSimulator = new InputSimulator();
            rand = new Random();

            showErrors = !Settings.SilenceErrors;
            Settings.SilenceErrors.OnValueChanged += delegate { showErrors = !Settings.SilenceErrors; };
            return base.Initialise();
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
            // Should move to Tick?
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
                HandleWitheringStep();
                HandleWarcry();
                HandleBerserk();
            }
            catch (Exception ex)
            {
                if (showErrors)
                {
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(OnExecute)}: {ex.StackTrace}", 3f);
                }
            }
        }

        private void HandleBladeFlurry()
        {
            try
            {
                if (!Settings.BladeFlurry)
                    return;

                var stacksBuff = GetBuff(C.BladeFlurry.BuffName);
                if (stacksBuff == null)
                    return;

                var charges = stacksBuff.BuffCharges;
                if (charges < Settings.BladeFlurryMinCharges.Value)
                    return;

                if (Settings.BladeFlurryWaitForInfused)
                {
                    var hasInfusedBuff = HasBuff(C.InfusedChanneling.BuffName);
                    if (!hasInfusedBuff.HasValue || !hasInfusedBuff.Value)
                        return;
                }

                if (Settings.Debug)
                    LogMessage($"Releasing Blade Flurry at {charges} charges.");

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

                var stacksBuff = GetBuff(C.ScourgeArrow.BuffName);
                if (stacksBuff == null)
                    return;

                var charges = stacksBuff.BuffCharges;
                if (charges < Settings.ScourgeArrowMinCharges.Value)
                    return;

                if (Settings.ScourgeArrowWaitForInfused)
                {
                    var hasInfusedBuff = HasBuff(C.InfusedChanneling.BuffName);
                    if (!hasInfusedBuff.HasValue || !hasInfusedBuff.Value)
                        return;
                }

                if (Settings.Debug)
                    LogMessage($"Releasing Scourge Arrow at {charges} charges.");

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
                    C.BloodRage.TimeBetweenCasts)
                    return;

                if (HPPercent > Settings.BloodRageMaxHP.Value || MPPercent > Settings.BloodRageMaxMP)
                    return;

                var hasBuff = HasBuff(C.BloodRage.BuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(C.BloodRage.Name, C.BloodRage.InternalName);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Blood Rage - not found in usable skills.");
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Blood Rage");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.BloodRageKey.Value);
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
                    C.SteelSkin.TimeBetweenCasts)
                    return;

                if (HPPercent > Settings.SteelSkinMaxHP.Value)
                    return;

                var hasBuff = HasBuff(C.SteelSkin.BuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(C.SteelSkin.Name, C.SteelSkin.InternalName
                );
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Steel Skin - not found in usable skills.");
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Steel Skin");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.SteelSkinKey.Value);
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
                    C.ImmortalCall.TimeBetweenCasts)
                    return;

                if (HPPercent > Settings.ImmortalCallMaxHP.Value)
                    return;

                var hasBuff = HasBuff(C.ImmortalCall.BuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(C.ImmortalCall.Name, C.ImmortalCall.InternalName);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Immortal Call - not found in usable skills.");
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Immortal Call");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.ImmortalCallKey.Value);
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
                    C.MoltenShell.TimeBetweenCasts)
                    return;

                if (HPPercent > Settings.MoltenShellMaxHP.Value)
                    return;

                var hasBuff = HasBuff(C.MoltenShell.BuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(C.MoltenShell.Name, C.MoltenShell.InternalName);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Molten Shell - not found in usable skills.");
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Molten Shell");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.MoltenShellKey.Value);
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
                    C.PhaseRun.TimeBetweenCasts)
                    return;

                if (HPPercent > Settings.PhaseRunMaxHP.Value)
                    return;

                if (movementStopwatch.ElapsedMilliseconds < Settings.PhaseRunMinMoveTime)
                    return;

                var hasBuff = HasBuff(C.PhaseRun.BuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var requiredBVStacks = Settings.PhaseRunMinBVStacks.Value;
                if (requiredBVStacks > 0)
                {
                    var bvBuff = GetBuff(C.BladeVortex.BuffName);
                    if (bvBuff == null || bvBuff.BuffCharges < requiredBVStacks)
                        return;
                }

                var skill = GetUsableSkill(C.PhaseRun.Name, C.PhaseRun.InternalName);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Phase Run - not found in usable skills.");
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Phase Run");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.PhaseRunKey.Value);
                lastPhaseRunCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.2));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandlePhaseRun)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleWitheringStep()
        {
            try
            {
                if (!Settings.WitheringStep)
                    return;

                if (lastWitheringStepCast.HasValue && currentTime - lastWitheringStepCast.Value <
                    C.WitheringStep.TimeBetweenCasts)
                    return;

                if (HPPercent > Settings.WitheringStepMaxHP.Value)
                    return;

                if (movementStopwatch.ElapsedMilliseconds < Settings.WitheringStepMinMoveTime)
                    return;

                var hasBuff = HasBuff(C.WitheringStep.BuffName);
                if (!hasBuff.HasValue || hasBuff.Value)
                    return;

                var skill = GetUsableSkill(C.WitheringStep.Name, C.WitheringStep.InternalName);
                if (skill == null)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Withering Step - not found in usable skills.");
                    return;
                }

                if (!NearbyMonsterCheck())
                    return;

                if (Settings.Debug)
                    LogMessage("Casting Withering Step");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.WitheringStepKey.Value);
                lastWitheringStepCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.2));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleWitheringStep)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleWarcry()
        {
            try
            {
                if (!Settings.Warcry)
                    return;

                if (lastWarcryCast.HasValue && currentTime - lastWarcryCast.Value <
                    TimeSpan.FromMilliseconds(500))
                    return;

                var minRage = Settings.WarCryMinRage.Value;
                var maxRage = Settings.WarCryMaxRage.Value;
                var rage = GetRageBuffCharges();
                if (rage < minRage || rage > maxRage)
                    return;

                var requiredEnemies = Settings.WarcryNearbyEnemiesCount;
                var useOnBosses = Settings.WarcryUseOnUniqueBoss.Value;
                if (!(requiredEnemies > 0 && requiredEnemies <= GetNearbyMonsterCount() ||
                      useOnBosses && IsUniqueBossInRange()))
                    return;

                var skill = GetWarcryActorSkill();
                if (skill == null || !skill.CanBeUsed)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Warcry skill - not found in usable skills.");
                    return;
                }

                if (Settings.Debug)
                    LogMessage("Casting Warcry");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.WarcryKey.Value);
                lastWarcryCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.1));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleWarcry)}: {ex.StackTrace}", 3f);
            }
        }

        private void HandleBerserk()
        {
            try
            {
                if (!Settings.Berserk)
                    return;

                if (lastBerserkCast.HasValue && currentTime - lastBerserkCast.Value <
                    TimeSpan.FromMilliseconds(500))
                    return;

                var minRage = Settings.BerserkMinRage.Value;
                var rage = GetRageBuffCharges();
                if (rage < minRage)
                    return;

                var requiredEnemies = Settings.BerserkNearbyEnemiesCount.Value;
                var useOnBosses = Settings.BerserkUseOnUniqueBoss.Value;
                if (!(requiredEnemies > 0 && requiredEnemies <= GetNearbyMonsterCount() ||
                      useOnBosses && IsUniqueBossInRange()))
                    return;

                var skill = GetUsableSkill(C.Berserk.Name, C.Berserk.InternalName);
                if (skill == null || !skill.CanBeUsed)
                {
                    if (Settings.Debug)
                        LogMessage("Can not cast Berserk skill - not found in usable skills.");
                    return;
                }

                if (Settings.Debug)
                    LogMessage("Casting Berserk");
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)Settings.BerserkKey.Value);
                lastBerserkCast = currentTime + TimeSpan.FromSeconds(rand.NextDouble(0, 0.1));
            }
            catch (Exception ex)
            {
                if (showErrors)
                    LogError($"Exception in {nameof(BuffUtil)}.{nameof(HandleWarcry)}: {ex.StackTrace}", 3f);
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
                if (player == null)
                    return false;
                var playerLife = player.GetComponent<Life>();
                if (playerLife == null)
                    return false;
                var isDead = playerLife.CurHP <= 0;
                if (isDead)
                    return false;

                buffs = player.GetComponent<Buffs>()?.BuffsList;
                if (buffs == null)
                    return false;

                var gracePeriod = HasBuff(C.GracePeriod.BuffName);
                if (!gracePeriod.HasValue || gracePeriod.Value)
                    return false;

                skills = player.GetComponent<Actor>().ActorSkills;
                if (skills == null || skills.Count == 0)
                    return false;

                currentTime = DateTime.UtcNow;

                HPPercent = 100f * playerLife.HPPercentage;
                MPPercent = 100f * playerLife.MPPercentage;

                var playerActor = player.GetComponent<Actor>();
                if (player.Address != 0 && playerActor.isMoving)
                {
                    if (!movementStopwatch.IsRunning)
                        movementStopwatch.Start();
                }
                else
                {
                    movementStopwatch.Reset();
                }


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
                rageBuffCount = null;
                uniqueBossNearby = null;
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
                    LogError("Requested buff check, but buff list is empty.");
                return null;
            }

            return buffs.Any(b => string.Compare(b.Name, buffName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private Buff GetBuff(string buffName)
        {
            if (buffs == null)
            {
                if (showErrors)
                    LogError("Requested buff retrieval, but buff list is empty.");
                return null;
            }

            return buffs.FirstOrDefault(b => string.Compare(b.Name, buffName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private ActorSkill GetUsableSkill(string skillName, string skillInternalName)
        {
            if (skills == null)
            {
                if (showErrors)
                    LogError("Requested usable skill, but skill list is empty.");
                return null;
            }

            return skills.FirstOrDefault(s =>
                (string.Compare(s.Name, skillName, StringComparison.OrdinalIgnoreCase) == 0 ||
                 string.Compare(s.InternalName, skillInternalName, StringComparison.OrdinalIgnoreCase) == 0));
        }

        private ActorSkill GetUsableSkill(IEnumerable<string> skillNames)
        {
            if (skills == null)
            {
                if (showErrors)
                    LogError("Requested usable skill, but skill list is empty.");
                return null;
            }

            return skills.FirstOrDefault(s =>
                (skillNames.Any(sn =>
                    string.Compare(s.Name, sn, StringComparison.OrdinalIgnoreCase) == 0)));
        }

        private bool NearbyMonsterCheck()
        {
            if (!Settings.RequireMinMonsterCount.Value)
                return true;
            var result = GetNearbyMonsterCount() >= Settings.NearbyMonsterCount;
            if (Settings.Debug.Value && !result)
                LogMessage("NearbyMonstersCheck failed.");
            return result;
        }

        private int GetNearbyMonsterCount()
        {
            if (nearbyMonsterCount.HasValue)
                return nearbyMonsterCount.Value;

            var playerPosition = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Render>().Pos;

            List<Entity> localLoadedMonsters;
            lock (loadedMonstersLock)
            {
                localLoadedMonsters = new List<Entity>(loadedMonsters);
            }

            var maxDistance = Settings.NearbyMonsterMaxDistance.Value;
            var maxDistanceSquared = maxDistance * maxDistance;
            var monsterCount = 0;
            foreach (var monster in localLoadedMonsters)
                if (IsValidNearbyMonster(monster, playerPosition, maxDistanceSquared))
                    monsterCount++;

            nearbyMonsterCount = monsterCount;
            return monsterCount;
        }

        private bool IsUniqueBossInRange()
        {
            if (uniqueBossNearby.HasValue)
                return uniqueBossNearby.Value;

            var playerPosition = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Render>().Pos;

            List<Entity> localLoadedMonsters;
            lock (loadedMonstersLock)
            {
                localLoadedMonsters = new List<Entity>(loadedMonsters);
            }

            var maxDistance = 750;
            var maxDistanceSquared = maxDistance * maxDistance;
            foreach (var monster in localLoadedMonsters)
                if (monster.Rarity == MonsterRarity.Unique &&
                    IsValidNearbyMonster(monster, playerPosition, maxDistanceSquared))
                {
                    var life = monster.GetComponent<Life>();
                    if (life.CurHP < life.MaxHP || life.CurES < life.MaxES)
                    {
                        uniqueBossNearby = true;
                        return true;
                    }
                }

            uniqueBossNearby = false;
            return false;
        }

        private bool IsValidNearbyMonster(Entity monster, Vector3 playerPosition, int maxDistanceSquared)
        {
            try
            {
                if (!monster.IsTargetable || !monster.IsAlive || !monster.IsHostile || monster.IsHidden ||
                    !monster.IsValid)
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

        private bool IsMonster(Entity entity) => entity != null && entity.HasComponent<Monster>();

        private int GetRageBuffCharges()
        {
            if (rageBuffCount.HasValue)
                return rageBuffCount.Value;
            if (buffs == null || buffs.Count == 0)
            {
                rageBuffCount = 0;
                return rageBuffCount.Value;
            }

            var rageBuff = GetBuff(C.Rage.BuffName);
            rageBuffCount = rageBuff?.BuffCharges ?? 0;
            return rageBuffCount.Value;
        }


        private ActorSkill GetWarcryActorSkill()
        {
            string[] warcryNames =
            {
                C.WarcryAncestralCry.Name, C.WarcryBattlemagesCry.Name, C.WarcryEnduringCry.Name,
                C.WarcryInfernalCry.Name, C.WarcryIntimidatingCry.Name, C.WarcryRallyingCry.Name
            };

            return GetUsableSkill(warcryNames);
        }

        public override void EntityAdded(Entity entity)
        {
            if (!IsMonster(entity))
                return;

            lock (loadedMonstersLock)
            {
                loadedMonsters.Add(entity);
            }
        }

        public override void EntityRemoved(Entity entity)
        {
            lock (loadedMonstersLock)
            {
                loadedMonsters.Remove(entity);
            }
        }
    }
}