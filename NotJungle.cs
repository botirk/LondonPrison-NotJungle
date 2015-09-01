﻿using System.Linq;

public class NotJungle {
    public System.Collections.Generic.List<LeagueSharp.SpellSlot> LevelMe;
    public System.Func<LeagueSharp.NeutralMinionCamp> CreepSpawn, CreepSpawnEx;
    public System.Func<LeagueSharp.NeutralMinionCamp, bool, LeagueSharp.Obj_AI_Minion> Creep, CreepEx;
    public System.Action<LeagueSharp.Obj_AI_Minion, LeagueSharp.NeutralMinionCamp> Clear;
    public System.Func<LeagueSharp.NeutralMinionCamp, bool> Cleave, Channel, Slack;
    public System.Func<bool> NeedPot;
    public void Reset() {
        CreepSpawn = delegate {
            LeagueSharp.NeutralMinionCamp result = null;
            var resultScore = 10000000.0;
            foreach (var creepSpawn in LeagueSharp.ObjectManager.Get<LeagueSharp.NeutralMinionCamp>().Where(creepSpawn => creepSpawn.Position.Side() == NotLib.myHero.Team)) {
                var score = 0.0;
                var get = creepSpawn.Data().Get() * NotLib.myHero.MoveSpeed;
                var dist = NotLib.myHero.ServerPosition.Distance(creepSpawn.Position);
                if (creepSpawn.Data().dead == creepSpawn.Data().spawn || dist > get) score = dist; else score = dist + (get - dist) * 1.4;
                if (score < resultScore) { result = creepSpawn; resultScore = score; }
            }
            return result;
        };
        CreepSpawnEx = delegate { return null; };
        Creep = delegate(LeagueSharp.NeutralMinionCamp creepSpawn, bool cleave) {
            LeagueSharp.Obj_AI_Minion target = null;
            bool red = NotLib.myHero.Buff_Red();
            foreach (var creep in creepSpawn.Data().Creeps()) {
                if (target == null) target = creep;
                else if (red && (target.Buff_RedSlow() || creep.Buff_RedSlow())) {
                    if (target.Buff_RedSlow() && !creep.Buff_RedSlow()) target = creep;
                } else if (cleave && creep.MaxHealth > target.MaxHealth) target = creep;
                else if (!cleave && creep.MaxHealth < target.MaxHealth) target = creep;
                else if (creep.MaxHealth == target.MaxHealth && creep.NetworkId > target.NetworkId) target = creep;
            }
            return target;
        };
        CreepEx = delegate { return null; };
        Clear = delegate(LeagueSharp.Obj_AI_Minion creep, LeagueSharp.NeutralMinionCamp camp) { NotLib.myHero.Attack(creep); };
        Cleave = Channel = Slack = delegate { return false; };
        NeedPot = delegate { return NotLib.myHero.Health/NotLib.myHero.MaxHealth < 0.4; };
    }
    public void Logic() {
        if (NotLib.myHero.IsDead) return;
        if (LevelMe != null) NotLib.myHero.Level(LevelMe);
        if (NeedPot() && !NotLib.myHero.Buff_HealthPot()) NotLib.myHero.Cast(NotLib.myHero.Item_HealthPot());
        var creepSpawn = CreepSpawnEx() ?? CreepSpawn();
        if (creepSpawn != null) {
            System.Console.WriteLine(LeagueSharp.Game.ClockTime + " - " + creepSpawn.Data().type + " - " + creepSpawn.Data().Get());
            var creep = CreepEx(creepSpawn, false) ?? Creep(creepSpawn, false);
            if (creep != null) {
                Clear(creep, creepSpawn);
            } else if (NotLib.myHero.ServerPosition.Distance(creepSpawn.Position) > 50) NotLib.myHero.MoveTo(creepSpawn.Position);
        }
    }
    public NotJungle() {
        Reset();
        LeagueSharp.Game.OnUpdate += delegate(System.EventArgs a) {
            foreach (var creepSpawn in LeagueSharp.ObjectManager.Get<LeagueSharp.NeutralMinionCamp>()) { if (creepSpawn.IsValid) creepSpawn.Data().Refresh(); }
        };
    }
}

public class INotJungle:NotJungle {
    public bool @switch = false;
    public int switchButton = 112; //F1
    public INotJungle() {
        LeagueSharp.Game.OnWndProc += delegate(LeagueSharp.WndEventArgs a) {
            if (a.Msg == 256 && a.WParam == switchButton) @switch = !@switch;
            if (a.Msg == 516 && a.WParam == 2) @switch = false;
        };
        NotLib.SmartTick(delegate {
            if (@switch) Logic();
        });
    }  
}
