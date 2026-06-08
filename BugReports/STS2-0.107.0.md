# [0.107.0][Combat Architecture] Critical Omission of Player-side Pets in `CombatManager` Turn Life Cycle and `OstyCmd.Summon()` NullReferenceException

## 1. User Observations (In-Game Symptoms)

During Mod development and testing regarding player-side summoned Pets (specifically Osty), we observed several critical mechanical inconsistencies during turn transitions:

- Normal Turn End: Pet debuffs/buffs do not resolve properly. For instance, `TemporaryStrengthPower` does not decay at turn end, and `DoomPower` completely ignores pets.

- Extra Turns: During extra turns, all buffs/debuffs on Osty are completely frozen and never updated, whereas the Player character's powers resolve perfectly.

- The Core Issue: The game currently ONLY updates/resolves Powers for Pets at the Start of a Normal Turn.

## 2. Root Cause Analysis (Decompiled Code Proof)

Through source code analysis of version 0.107.0, we identified that class `CombatManager` structurally narrows its execution scope to `Player` objects during Extra Turns and Turn End Phases, entirely omitting other player-side creatures (Pets).

## Code Proof 1: Extra Turn Start Omission

In `CombatManager.StartTurn()`, when an extra turn is triggered (`isExtraPlayerTurn == true`), the game explicitly ignores all non-player creatures on the current side:

```cs
if (state != null && state.CurrentSide == CombatSide.Player && isExtraPlayerTurn)
{
    // ❌ FATAL: Only selects Player.Creature. Pets are completely left out of the extra turn start!
    creaturesStartingTurn = _playersTakingExtraTurn.Select((Player p) => p.Creature).ToList();
    playersStartingTurn = _playersTakingExtraTurn.ToList();
}
else
{
    // Normal turns correctly include pets via CreaturesOnCurrentSide
    creaturesStartingTurn = _state?.CreaturesOnCurrentSide.ToList() ?? new List<Creature>();
    ...
}
```

## Code Proof 2: Turn End Hooks Omission (Phase 1 & Phase 2)

In both `EndPlayerTurnPhaseOneInternal()` and `EndPlayerTurnPhaseTwoInternal()`, the local list `playersEndingTurn` is strictly populated by `Player` models, which structurally blinds the subsequent hooks:

```cs
// ❌ FATAL: Hardcoded to _state.Players or extra turn players, completely omitting Pets
playersEndingTurn = ((_playersTakingExtraTurn.Count > 0) ? _playersTakingExtraTurn.ToList() : (_state?.Players.ToList() ?? new List<Player>()));

...

if (_state != null)
{
    // ❌ Consequently, Hook.BeforeTurnEnd and Hook.AfterTurnEnd only receive the main Player creature.
    // Pets debuffs (e.g. DoomPower) and temporary buffs (e.g. powers that inherit TemporaryStrengthPower) never get resolved here.
    await Hook.BeforeTurnEnd(_state, _state.CurrentSide, playersEndingTurn.Select((Player p) => p.Creature));
}
```

## 3. The Re-summoning Crash Log (`OstyCmd.Summon`)

When a patch is applied to force Osty into the Turn End hook (i.e., `Hook.BeforeTurnEnd`) to allow `DoomPower` to execute and kill it normally, the Osty object seems to become corrupted.

On the next turn, when Necrobinder's starting relic Bound Phylactery attempts to auto-summon Osty at the start of the turn, the game immediately throws a fatal `NullReferenceException` and softlocks the combat:

```
[ERROR] System.NullReferenceException: Object reference not set to an instance of an object.
   at MegaCrit.Sts2.Core.Commands.OstyCmd.Summon(PlayerChoiceContext choiceContext, Player summoner, Decimal amount, AbstractModel source)
   at MegaCrit.Sts2.Core.Models.Relics.BoundPhylactery.SummonPet()
   at MegaCrit.Sts2.Core.Models.Relics.BoundPhylactery.AfterEnergyResetLate(Player player)
   at MegaCrit.Sts2.Core.Hooks.Hook.AfterEnergyReset(ICombatState combatState, Player player)
   at STS2RitsuLib.Lifecycle.Patches.LifecyclePatchTaskBridge.After(Task originalTask, Action continuation) in /home/runner/work/STS2-RitsuLib/STS2-RitsuLib/Lifecycle/Patches/LifecyclePatchTaskBridge.cs:line 12
   at MegaCrit.Sts2.Core.Combat.CombatManager.SetupPlayerTurn(Player player, HookPlayerChoiceContext playerChoiceContext)
   at MegaCrit.Sts2.Core.Helpers.TaskHelper.WhenAny(Task[] tasks)
   at MegaCrit.Sts2.Core.GameActions.Multiplayer.HookPlayerChoiceContext.WaitForPauseOrCompletionWithoutAssigningTask(Task task)
   at MegaCrit.Sts2.Core.Combat.CombatManager.StartTurn(Func`1 actionDuringEnemyTurn)
   at MegaCrit.Sts2.Core.Combat.CombatManager.EndEnemyTurn()
   at MegaCrit.Sts2.Core.Combat.CombatManager.ExecuteEnemyTurn(Func`1 actionDuringEnemyTurn)
   at MegaCrit.Sts2.Core.Combat.CombatManager.StartTurn(Func`1 actionDuringEnemyTurn)
   at MegaCrit.Sts2.Core.Combat.CombatManager.SwitchFromPlayerToEnemySide(Func`1 actionDuringEnemyTurn)
   at MegaCrit.Sts2.Core.Combat.CombatManager.AfterAllPlayersReadyToBeginEnemyTurn(Func`1 actionDuringEnemyTurn)
   at MegaCrit.Sts2.Core.Helpers.TaskHelper.LogTaskExceptions(Task task)
   ...
```

### Note on the NullReferenceException Root Cause:

Please note that the exact root cause of the `NullReferenceException` inside `OstyCmd.Summon()` has not yet been identified from our side. To prevent any misleading speculation, we have refrained from making assumptions about the internal state handling of `OstyCmd`.

We only know that this crash is 100% reproducible when Osty is killed by `DoomPower` during the (patched) Turn End phase (what we do is simply include Osty in the `IEnumerable<Creature>` parameter of `Hook.BeforeTurnEnd()`) and then subsequently re-summoned on the next turn (e.g., via Bound Phylactery). We kindly request the development team to investigate why `OstyCmd` loses its object references under this specific lifecycle transition and provide a proper fix to prevent this combat softlock.

## 4. Suggested Fix

### Fix 1: Include Pets in Extra Turn Start

In `CombatManager.StartTurn()`, update the extra turn branch to include the player's pets into creaturesStartingTurn:

```cs
// CombatManager.StartTurn()
if (state != null && state.CurrentSide == CombatSide.Player && isExtraPlayerTurn)
{
    creaturesStartingTurn = _playersTakingExtraTurn.Select(p => p.Creature).ToList();
    creaturesStartingTurn.AddRange(_playersTakingExtraTurn.SelectMany(p => p.PlayerCombatState?.Pets ?? Enumerable.Empty<Creature>()));
    playersStartingTurn = _playersTakingExtraTurn.ToList();
}
```

### Fix 2: Include Pets in Turn End Hooks

In both `EndPlayerTurnPhaseOneInternal()` and `EndPlayerTurnPhaseTwoInternal()`, when invoking `Hook.BeforeTurnEnd` and `Hook.AfterTurnEnd`, ensure that `p.PlayerCombatState.Pets` are selected and passed along with `p.Creature` into the creature parameter collection, allowing pet powers to be resolved during the turn end phases:

```cs
// CombatManager.EndPlayerTurnPhaseOneInternal()
if (_state != null)
{
    await Hook.BeforeTurnEnd(
        _state,
        _state.CurrentSide,
        playersEndingTurn.Select(p => p.Creature)
            .Concat(playersEndingTurn.SelectMany(p => p.PlayerCombatState?.Pets ?? Enumerable.Empty<Creature>()))
    );
}
```

```cs
// CombatManager.EndPlayerTurnPhaseTwoInternal()
if (_state != null)
{
    await Hook.AfterTurnEnd(
        _state,
        _state.CurrentSide,
        playersEndingTurn.Select((Player p) => p.Creature)
            .Concat(playersEndingTurn.SelectMany(p => p.PlayerCombatState?.Pets ?? Enumerable.Empty<Creature>()))
    );
    CombatCt.ThrowIfCancellationRequested();
}
```
