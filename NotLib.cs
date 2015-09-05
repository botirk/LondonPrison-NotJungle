using System.Linq;
// extensions
namespace NotLib.Ext {
    // static methods (several will be transformed into classes later)
    public static class s {
        public static LeagueSharp.Obj_AI_Hero myHero = LeagueSharp.ObjectManager.Player;
        public static LeagueSharp.Obj_SpawnPoint allySpawn = LeagueSharp.ObjectManager.Get<LeagueSharp.Obj_SpawnPoint>().FirstOrDefault(spawn => spawn.IsValid && spawn.Team == LeagueSharp.ObjectManager.Player.Team);
        public static LeagueSharp.Obj_SpawnPoint enemySpawn = LeagueSharp.ObjectManager.Get<LeagueSharp.Obj_SpawnPoint>().FirstOrDefault(spawn => spawn.IsValid && spawn.Team != LeagueSharp.ObjectManager.Player.Team);
        // position related
        public static double Distance(this SharpDX.Vector3 from, SharpDX.Vector3 to) { return System.Math.Sqrt(System.Math.Pow(to.X - from.X, 2) + System.Math.Pow(to.Y - from.Y, 2)); }
        public static double Rad(this SharpDX.Vector3 from, SharpDX.Vector3 to) {
            if (to.Y > from.Y) return System.Math.Atan2(to.Y - from.Y, to.X - from.X);
            else return System.Math.PI * 2 + System.Math.Atan2(to.Y - from.Y, to.X - from.X);
        }
        public static SharpDX.Vector3 Pos(this SharpDX.Vector3 from, SharpDX.Vector3 to, double range) {
            var rad = from.Rad(to);
            return new SharpDX.Vector3((float)(from.X + System.Math.Cos(rad) * range), from.Z, (float)(from.Y + System.Math.Sin(rad) * range));
        }
        public static bool CanSee(this SharpDX.Vector3 from, SharpDX.Vector3 to) { return from.Distance(to) < 1200 && LeagueSharp.NavMesh.LineOfSightTest(from, to); }
        public static bool CanSeeEx(this SharpDX.Vector3 from, SharpDX.Vector3 to) {
            #warning bugged;
            var toEx = from.Pos(to, (System.Math.Max(0, from.Distance(to) - 20)));
            return from.Distance(to) < 1200 && LeagueSharp.NavMesh.LineOfSightTest(from, toEx);
        }
        public static LeagueSharp.GameObjectTeam Side(this SharpDX.Vector3 unit) {
            var dist = allySpawn.Position.Distance(unit) - enemySpawn.Position.Distance(unit);
            if (dist > 1250) return enemySpawn.Team; else if (dist < -1250) return allySpawn.Team; else return LeagueSharp.GameObjectTeam.Neutral;
        }
        // buff related
        public static bool Buff(this LeagueSharp.Obj_AI_Base unit, string name) {
            foreach (var buff in unit.Buffs) {
                if (buff.IsValid && buff.IsActive && buff.Name == name) return true;
            }
            return false;
        }
        public static bool Buff(this LeagueSharp.Obj_AI_Base unit, System.Collections.Generic.List<string> list) {
            foreach (var buff in list) { if (myHero.Buff(buff)) return true; }
            return false;
        }
        // item+spell related
        public static bool CanUse(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.SpellSlot spell) { return unit.Spellbook.GetSpell(spell).State == LeagueSharp.SpellState.Ready; } // optimize it
        public static LeagueSharp.SpellSlot Item(this LeagueSharp.Obj_AI_Base unit, int id, bool usable = false) {
            var item = unit.InventoryItems.FirstOrDefault(slot => slot.Id == (LeagueSharp.ItemId)id && (!usable || unit.CanUse(slot.SpellSlot)));
            if (item != null) return item.SpellSlot; else return LeagueSharp.SpellSlot.Unknown;
        }
        public static LeagueSharp.SpellSlot Item(this LeagueSharp.Obj_AI_Base unit, System.Collections.Generic.List<int> list, bool usable = false) {
            foreach (var id in list) {
                var spell = unit.Item(id);
                if (spell != LeagueSharp.SpellSlot.Unknown) return spell;
            }
            return LeagueSharp.SpellSlot.Unknown;
        }
        public static int SmiteDamage(this LeagueSharp.Obj_AI_Hero unit) {
            var damage = 370 + unit.Level * 20;
            if (unit.Level > 4) damage = damage + (unit.Level - 4) * 10;
            if (unit.Level > 9) damage = damage + (unit.Level - 9) * 10;
            if (unit.Level > 14) damage = damage + (unit.Level - 14) * 10;
            return damage;
        }
        public static bool InRange(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.AttackableUnit target) { return unit.ServerPosition.Distance(target.Position) < unit.AttackRange + unit.BoundingRadius + target.BoundingRadius; }
        public static bool InRange(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.AttackableUnit target, LeagueSharp.SpellSlot spell) { return unit.ServerPosition.Distance(target.Position) < unit.Spellbook.GetSpell(spell).SData.CastRange; }
        // order related
        public static void Attack(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.AttackableUnit target) { unit.IssueOrder(LeagueSharp.GameObjectOrder.AttackUnit, target); }
        public static void MoveTo(this LeagueSharp.Obj_AI_Base unit, SharpDX.Vector3 pos, bool randomize = true) { unit.IssueOrder(LeagueSharp.GameObjectOrder.MoveTo, pos); }
        public static bool Cast(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.SpellSlot spell) { return (spell != LeagueSharp.SpellSlot.Unknown && unit.CanUse(spell) && unit.Spellbook.CastSpell(spell)); }
        public static bool Cast(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.SpellSlot spell, LeagueSharp.GameObject target) { return (spell != LeagueSharp.SpellSlot.Unknown && unit.CanUse(spell) && unit.Spellbook.CastSpell(spell, target)); }
        public static void Level(this LeagueSharp.Obj_AI_Base unit, System.Collections.Generic.List<LeagueSharp.SpellSlot> list) {
            int req_Q = 0, _Q = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.Q).Level;
            int req_W = 0, _W = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.W).Level;
            int req_E = 0, _E = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.W).Level;
            int req_R = 0, _R = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.R).Level;
            foreach (var spell in list) {
                switch (spell) {
                    case LeagueSharp.SpellSlot.Q:
                        req_Q += 1;
                        if (req_Q > _Q) unit.Spellbook.LevelSpell(spell);
                        break;
                    case LeagueSharp.SpellSlot.W:
                        req_W += 1;
                        if (req_W > _W) unit.Spellbook.LevelSpell(spell);
                        break;
                    case LeagueSharp.SpellSlot.E:
                        req_E += 1;
                        if (req_E > _E) unit.Spellbook.LevelSpell(spell);
                        break;
                    case LeagueSharp.SpellSlot.R:
                        req_R += 1;
                        if (req_R > _R) unit.Spellbook.LevelSpell(spell);
                        break;
                }
            }
        }
        // timer
        public static void SmartTick(System.Action cb) {
            float lastcall = 0;
            LeagueSharp.Game.OnUpdate += delegate {
                if (lastcall + 0.1 <= LeagueSharp.Game.ClockTime) {
                    lastcall = LeagueSharp.Game.ClockTime;
                    cb();
                }
            };
        }
    }
    // unit
    public class Unit {
        public static System.Collections.Generic.Dictionary<short, LeagueSharp.GameObject> Data;
        public LeagueSharp.GameObject unit;
        public string @class;
        public Unit(LeagueSharp.GameObject a, string req = null) {
            @class = Class(a);
            if (req != null && req != @class) throw new System.Exception("NotLib." + req + "constructed with NotLib." + @class);
            unit = a;
        }
        //
        public static string Class(LeagueSharp.GameObject unit) {
            var type = unit.Type;
            switch (type) {
                case LeagueSharp.GameObjectType.obj_GeneralParticleEmitter:
                case LeagueSharp.GameObjectType.obj_AI_Marker:
                case LeagueSharp.GameObjectType.FollowerObject: return "visual";
                case LeagueSharp.GameObjectType.obj_AI_Minion:
                    var name = unit.Name.ToLower();
                    if (name.Contains("minion")) return "minion";
                    else if (name.Contains("ward")) return "ward";
                    else if (name.Contains("buffplat") || name == "odinneutralguardian") return "point";
                    else if (name.Contains("shrine") || name.Contains("relic")) return "event";
                    else if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.SummonersRift && System.Text.RegularExpressions.Regex.IsMatch(name, @"\d+\.\d+")
                        && (name.Contains("baron") || name.Contains("dragon") || name.Contains("blue") || name.Contains("red") || name.Contains("crab")
                        || name.Contains("krug") || name.Contains("gromp") || name.Contains("wolf") || name.Contains("razor"))) return "creep";
                    else if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.TwistedTreeline && System.Text.RegularExpressions.Regex.IsMatch(name, @"\d+\.\d+")
                        && (name.Contains("wraith") || name.Contains("golem") || name.Contains("wolf") || name.Contains("spider"))) return "creep";
                    else {
                        var minion = unit as LeagueSharp.Obj_AI_Minion;
                        if (minion != null && !minion.IsTargetable) return "trap";
                        return "error";
                    }
                case LeagueSharp.GameObjectType.obj_AI_Turret: return "tower";
                case LeagueSharp.GameObjectType.obj_AI_Hero: return "player";
                case LeagueSharp.GameObjectType.obj_Shop: return "shop";
                case LeagueSharp.GameObjectType.obj_HQ: return "nexus";
                case LeagueSharp.GameObjectType.obj_BarracksDampener: return "inhibotor";
                case LeagueSharp.GameObjectType.obj_SpawnPoint: return "spawn";
                case LeagueSharp.GameObjectType.obj_Barracks: return "minionSpawn";
                case LeagueSharp.GameObjectType.NeutralMinionCamp: return "creepSpawn";
                case LeagueSharp.GameObjectType.obj_InfoPoint: return "event";
                case LeagueSharp.GameObjectType.Missile:
                case LeagueSharp.GameObjectType.MissileClient:
                case LeagueSharp.GameObjectType.obj_SpellMissile:
                case LeagueSharp.GameObjectType.obj_SpellCircleMissile:
                case LeagueSharp.GameObjectType.obj_SpellLineMissile: return "spell";
            }
            return "error";
        }
    }
    // creepSpawn
    public class CreepSpawn : Unit {
        // members
        new public LeagueSharp.NeutralMinionCamp unit;
        public int campNumber;
        public string type;
        public float spawn, respawn, dead;
        public CreepSpawn(LeagueSharp.GameObject a)
            : base(a, "creepSpawn") {
            unit = a as LeagueSharp.NeutralMinionCamp;
            campNumber = CampNumber();
            type = Type();
            spawn = Spawn() + 30; //bug with timers
            respawn = Respawn();
            dead = spawn;
            a.GetHashCode();
        }
        // methods
        public int CampNumber() {
            var result = System.Text.RegularExpressions.Regex.Match(unit.Name, @"\d+");
            if (!result.Success) return 0;
            else {
                int num = 0;
                if (int.TryParse(result.Value, out num)) return num; else return 0;
            }
        }
        public string Type() {
            if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.SummonersRift) {
                switch (campNumber) {
                    case 6: return "dragon";
                    case 12: return "nashor";
                    case 1:
                    case 7: return "blue";
                    case 4:
                    case 10: return "red";
                    case 15:
                    case 16: return "cancer";
                    case 2:
                    case 8: return "wolf";
                    case 3:
                    case 9: return "wraith";
                    case 5:
                    case 11: return "golem";
                    case 13:
                    case 14: return "wight";
                }
            }
            return "default";
        }
        public float Spawn() {
            if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.SummonersRift) {
                switch (type) {
                    case "dragon": return 150;
                    case "nashor": return 750;
                    case "cancer": return 150;
                }
            }
            return 120;
        }
        public float Respawn() {
            if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.SummonersRift) {
                switch (type) {
                    case "dragon": return 360;
                    case "nashor": return 420;
                    case "blue": return 300;
                    case "red": return 300;
                    case "cancer": return 180;
                }
            }
            return 100;
        }
        public System.Collections.Generic.IEnumerable<LeagueSharp.Obj_AI_Minion> Creeps(bool dead = false) { return LeagueSharp.ObjectManager.Get<LeagueSharp.Obj_AI_Minion>().Where(creep => creep.IsValid && creep.IsVisible && creep.IsDead == dead && creep.CampNumber == campNumber); }
        public float Health() { return Creeps().Sum(creep => creep.Health); }
        public LeagueSharp.Obj_AI_Minion BigCreep() {
            LeagueSharp.Obj_AI_Minion max = null;
            foreach (var creep in Creeps()) { if (max == null || creep.MaxHealth > max.MaxHealth) max = creep; }
            return max;
        }
        public bool Started() {
            foreach (LeagueSharp.Obj_AI_Minion creep in Creeps()) { if (creep.Health < creep.MaxHealth) return true; }
            return false;
        }
        public void Set(bool state) { if (state) dead = 0; else dead = LeagueSharp.Game.ClockTime + unit.Data<CreepSpawn>().respawn; }
        public float Get() { return System.Math.Max(0, dead - LeagueSharp.Game.ClockTime); }
        public void Refresh(bool force = false) {
            if (Creeps().Any()) Set(true);
            else if (Get() == 0 && (force || (!s.myHero.IsDead && unit.Data<CreepSpawn>().type != "cancer" && s.myHero.ServerPosition.CanSee(unit.Position)))) Set(false);
            else if (Creeps(true).Any()) Set(false);
        }
    }
    // creep
    public class Creep : Unit {
        // members
        new public static LeagueSharp.Obj_AI_Minion unit;
        public int campNumber;
        public Creep(LeagueSharp.Obj_AI_Minion a)
            : base(a, "creep") {
            unit = a;
            campNumber = a.CampNumber;
        }
        // methods
        public LeagueSharp.NeutralMinionCamp CreepSpawn() { return LeagueSharp.ObjectManager.Get<LeagueSharp.NeutralMinionCamp>().FirstOrDefault(creepSpawn => creepSpawn.IsValid && creepSpawn.Data<CreepSpawn>().campNumber == campNumber); }
    }
    // extension interface
    public static class Interface {
        public static System.Collections.Generic.Dictionary<int, Unit> data = new System.Collections.Generic.Dictionary<int, Unit>();
        public static T Data<T>(this LeagueSharp.GameObject unit) where T : Unit {
            if (!data.ContainsKey(unit.Index)) {
                if (typeof(T) == typeof(Unit)) data[unit.Index] = new Unit(unit);
                else if (typeof(T) == typeof(Creep)) data[unit.Index] = new Creep(unit as LeagueSharp.Obj_AI_Minion);
                else if (typeof(T) == typeof(CreepSpawn)) data[unit.Index] = new CreepSpawn(unit as LeagueSharp.NeutralMinionCamp);
            }
            return (T)data[unit.Index];
        }
    }
}
// abstract
namespace NotLib.Abstract {
    using NotLib.Ext;
    // base
    abstract public class Parent{
        abstract public void Reset();
        public Parent() { Reset(); }
    }
    // pot
    public class HealthPot: Parent {
        public System.Collections.Generic.List<int> id;
        public System.Collections.Generic.List<string> buff;
        public System.Func<bool> Need;
        public System.Action Logic;
        override public void Reset() {
            id = new System.Collections.Generic.List<int> { 2041, 2003, 2010, 2009 };
            buff = new System.Collections.Generic.List<string> { "ItemCrystalFlask", "RegenerationPotion", "ItemMiniRegenPotion" };
            Need = delegate { return s.myHero.Health / s.myHero.MaxHealth < 0.45; };
            Logic = delegate { if (Need()) s.myHero.Cast(s.myHero.Item(id)); };
        }
    }
    // red
    public class RedBuff: Parent {
        public string own,apply;
        override public void Reset(){
            own = "blessingofthelizardelder";
            apply = "blessingofthelizardelderslow";
        }
    }
    // spell attack
    public class SpellAttack: Parent {
        public System.Func<LeagueSharp.AttackableUnit, bool> T(LeagueSharp.SpellSlot spell) {
            var gs = s.myHero.Spellbook.GetSpell(spell);
            switch (gs.SData.TargettingType) {
                case LeagueSharp.SpellDataTargetType.Unit:
                    return delegate(LeagueSharp.AttackableUnit target) { return s.myHero.Cast(spell, target); };
                case LeagueSharp.SpellDataTargetType.SelfAoe:
                    return delegate(LeagueSharp.AttackableUnit target) { return s.myHero.Cast(spell); };
            }
            return delegate { return false; };
        }
        public System.Func<LeagueSharp.AttackableUnit, bool> Q, W, E, R;
        public System.Func<LeagueSharp.AttackableUnit, bool> Logic;
        override public void Reset() {
            Q = T(LeagueSharp.SpellSlot.Q);
            W = T(LeagueSharp.SpellSlot.W);
            E = T(LeagueSharp.SpellSlot.E);
            R = T(LeagueSharp.SpellSlot.R);
            Logic = delegate(LeagueSharp.AttackableUnit target) {return Q(target) || W(target) || E(target) || R(target);};
        }
    }
    // spell self
    public class SpellSelf : Parent {
        public System.Func<bool> T(LeagueSharp.SpellSlot spell) {
            var gs = s.myHero.Spellbook.GetSpell(spell);
            switch (gs.SData.TargettingType) {
                case LeagueSharp.SpellDataTargetType.SelfAoe:
                case LeagueSharp.SpellDataTargetType.Self:
                    return delegate { return s.myHero.Cast(spell); };
            }
            return delegate { return false; };
        }
        public System.Func<bool> Q, W, E, R;
        public System.Func<bool> Logic;
        override public void Reset() {
            Q = T(LeagueSharp.SpellSlot.Q);
            W = T(LeagueSharp.SpellSlot.W);
            E = T(LeagueSharp.SpellSlot.E);
            R = T(LeagueSharp.SpellSlot.R);
            Logic = delegate { return Q() || W() || E() || R(); };
        }
    }
    // jungle
    namespace Jungle {
        // spell
        public class Spell : NotLib.Abstract.SpellAttack {
            public System.Func<LeagueSharp.Obj_AI_Minion, LeagueSharp.NeutralMinionCamp, bool> Q_Worth, W_Worth, E_Worth, R_Worth;
            new public System.Func<LeagueSharp.Obj_AI_Minion, LeagueSharp.NeutralMinionCamp, bool> Logic;
            override public void Reset() {
                Logic = delegate(LeagueSharp.Obj_AI_Minion creep,LeagueSharp.NeutralMinionCamp creepSpawn) {
                    if (Q_Worth(creep,creepSpawn) && Q(creep)) return true;
                    else if (W_Worth(creep, creepSpawn) && W(creep)) return true;
                    else if (E_Worth(creep, creepSpawn) && E(creep)) return true;
                    else if (R_Worth(creep, creepSpawn) && R(creep)) return true;
                    return false;
                };
                Q = T(LeagueSharp.SpellSlot.Q);
                W = T(LeagueSharp.SpellSlot.W);
                E = T(LeagueSharp.SpellSlot.E);
                R = T(LeagueSharp.SpellSlot.R);
                Q_Worth = W_Worth = E_Worth = R_Worth = delegate { return false; };
            }
        }
        // channeling
        public class Channel : Parent {
            public System.Func<bool> Worth, State, Logic;
            override public void Reset() {
                Worth = delegate() { return true; };
                State = delegate() { return s.myHero.Spellbook.IsCastingSpell || s.myHero.Spellbook.IsChanneling; };
                Logic = delegate() { return Worth() && State(); };
            }
        }
        // jungle refresh
        public class Refresh : Parent {
            public static Refresh instance;
            public override void Reset() {
                if (instance != null) return;
                instance = this;
                LeagueSharp.Game.OnUpdate += delegate {
                    foreach (var creepSpawn in LeagueSharp.ObjectManager.Get<LeagueSharp.NeutralMinionCamp>()) { if (creepSpawn.IsValid) creepSpawn.Data<CreepSpawn>().Refresh(); }
                };
            }
        }
        // jungle navigator
        public class Nav : Parent {
            public System.Func<LeagueSharp.NeutralMinionCamp> CreepSpawnEx,CreepSpawn;
            override public void Reset() {
                CreepSpawnEx = delegate { return null; };
                CreepSpawn = delegate {
                    LeagueSharp.NeutralMinionCamp result = CreepSpawnEx();
                    if (result != null) return result;
                    var resultScore = 10000000.0;
                    foreach (var creepSpawn in LeagueSharp.ObjectManager.Get<LeagueSharp.NeutralMinionCamp>().Where(creepSpawn => creepSpawn.Position.Side() == s.myHero.Team)) {
                        var score = 0.0;
                        var get = creepSpawn.Data<CreepSpawn>().Get() * s.myHero.MoveSpeed;
                        var dist = s.myHero.ServerPosition.Distance(creepSpawn.Position);
                        if (creepSpawn.Data<CreepSpawn>().dead == creepSpawn.Data<CreepSpawn>().spawn || dist > get) score = dist; else score = dist + (get - dist) * 1.4;
                        if (score < resultScore) { result = creepSpawn; resultScore = score; }
                    }
                    return result;
                };
                

            }
        }
        // jungle target
        public class Target : Parent {
            public RedBuff redBuff;
            public System.Func<LeagueSharp.NeutralMinionCamp, bool> Cleave;
            public System.Func<LeagueSharp.NeutralMinionCamp, LeagueSharp.Obj_AI_Minion> Creep, CreepEx;
            override public void Reset() {
                redBuff = new RedBuff();
                Cleave = delegate { return false; };
                CreepEx = delegate { return null; };
                Creep = delegate(LeagueSharp.NeutralMinionCamp creepSpawn) {
                    var cleave = Cleave(creepSpawn);
                    LeagueSharp.Obj_AI_Minion target = CreepEx(creepSpawn);
                    if (target != null) return target;
                    var red = s.myHero.Buff(redBuff.own);
                    foreach (var creep in creepSpawn.Data<CreepSpawn>().Creeps()) {
                        if (target == null) target = creep;
                        else if (red && (target.Buff(redBuff.apply) || creep.Buff(redBuff.apply))) {
                            if (target.Buff(redBuff.apply) && !creep.Buff(redBuff.apply)) target = creep;
                        } else if (cleave && creep.MaxHealth > target.MaxHealth) target = creep;
                        else if (!cleave && creep.MaxHealth < target.MaxHealth) target = creep;
                        else if (creep.MaxHealth == target.MaxHealth && creep.NetworkId > target.NetworkId) target = creep;
                    }
                    return target;
                };
                
            }
        }
        // jungle killer
        public class Kill : Parent {
            public HealthPot pot;
            public Channel channel;
            public Spell spell;
            public System.Action<LeagueSharp.Obj_AI_Minion,LeagueSharp.NeutralMinionCamp> Logic;
            override public void Reset() {
                pot = new HealthPot();
                channel = new Channel();
                spell = new Spell();
                Logic = delegate(LeagueSharp.Obj_AI_Minion creep,LeagueSharp.NeutralMinionCamp creepSpawn) {
                    pot.Logic();
                    if (channel.Logic()) return;
                    spell.Logic(creep,creepSpawn);
                    s.myHero.Attack(creep);
                };
            }
        }
        // jungle move
        public class Move : Parent {
            public Channel channel;
            public System.Func<LeagueSharp.NeutralMinionCamp, bool> LogicEx,Logic;
            override public void Reset() {
                channel = new Channel();
                LogicEx = delegate(LeagueSharp.NeutralMinionCamp creepSpawn) { return false; };
                Logic = delegate(LeagueSharp.NeutralMinionCamp creepSpawn) {
                    if (channel.Logic() || LogicEx(creepSpawn)) return true;
                    else if (s.myHero.ServerPosition.Distance(creepSpawn.Position) < 50) return false;
                    s.myHero.MoveTo(creepSpawn.Position);
                    return true;
                };
            }
        }
        // jungle cycle
        public class Cycle : Parent {
            public Refresh refresh;
            public Nav nav;
            public Target target;
            public Kill kill;
            public Move move;
            public System.Func<bool> Logic;
            override public void Reset() {
                refresh = new Refresh();
                nav = new Nav();
                target = new Target();
                kill = new Kill();
                move = new Move();
                Logic = delegate {
                    var creepSpawn = (nav.CreepSpawnEx() ?? nav.CreepSpawn());
                    if (creepSpawn == null) return false;
                    var creep = target.Creep(creepSpawn);
                    if (creep != null) kill.Logic(creep, creepSpawn);
                    else move.Logic(creepSpawn);
                    return true;
                };
            }
        }
        // jungle cycle with heroes
        public class CycleEx : Cycle {
            public void Transform(string charName) {
                Reset();
                switch (charName) {
                    case "Warwick":
                        kill.spell.Q_Worth = delegate{return true;};
                        kill.spell.W_Worth = delegate(LeagueSharp.Obj_AI_Minion creep,LeagueSharp.NeutralMinionCamp creepSpawn) {return creepSpawn.Data<CreepSpawn>().Health() > s.myHero.SmiteDamage();};
                        break;
                    case "MasterYi":
                        kill.spell.Q_Worth = delegate { return true; };
                        kill.channel.Worth = delegate { return s.myHero.Health / s.myHero.MaxHealth < 1; };
                        move.channel.Worth = delegate { return s.myHero.Health / s.myHero.MaxHealth < 1; };
                        move.LogicEx = delegate { return s.myHero.Health / s.myHero.MaxHealth < 0.45 && s.myHero.Cast(LeagueSharp.SpellSlot.W); };
                        break;
                }
            }
            public CycleEx(string charName) { Transform(charName); }
        }
        // switcher
        public class Switch: CycleEx {
            public bool @switch = false;
            public int switchButton = 112; //F1
            public Switch():base(s.myHero.ChampionName) {
                LeagueSharp.Game.OnWndProc += delegate(LeagueSharp.WndEventArgs a) {
                    if (a.Msg == 256 && a.WParam == switchButton) @switch = !@switch;
                    if (a.Msg == 516 && a.WParam == 2) @switch = false;
                };
                s.SmartTick(delegate { if (@switch) Logic(); });
            }
        }
    }
}