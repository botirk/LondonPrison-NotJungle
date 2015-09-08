using System.Linq;
// extensions
namespace NotLib.Ext {
    // static methods (several will be transformed into classes later)
    public static class s {
        public static Player myHero = LeagueSharp.ObjectManager.Player.Data<Player>();
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
    }
    // object
    public class Object {
        // members
        public LeagueSharp.GameObject @ref;
        public string @class;
        public Object(LeagueSharp.GameObject a) {
            @ref = a;
            @class = Class(a); 
        }
        // methods
        public static string Class(LeagueSharp.GameObject unit) {
            var type = unit.Type;
            switch (type) {
                case LeagueSharp.GameObjectType.obj_GeneralParticleEmitter:
                case LeagueSharp.GameObjectType.obj_AI_Marker:
                case LeagueSharp.GameObjectType.FollowerObject: return "visual";
                case LeagueSharp.GameObjectType.obj_AI_Minion:
                    var minion = (unit as LeagueSharp.Obj_AI_Minion);
                    var name = unit.Name.ToLower();
                    if (minion.CampNumber != 0) return "creep"; //L#
                    else if (name.Contains("minion")) return "minion";
                    else if (name.Contains("ward")) return "ward";
                    else if (name.Contains("buffplat") || name == "odinneutralguardian") return "point";
                    else if (name.Contains("shrine") || name.Contains("relic")) return "event";
                    else if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.SummonersRift && System.Text.RegularExpressions.Regex.IsMatch(name, @"\d+\.\d+")
                        && (name.Contains("baron") || name.Contains("dragon") || name.Contains("blue") || name.Contains("red") || name.Contains("crab")
                        || name.Contains("krug") || name.Contains("gromp") || name.Contains("wolf") || name.Contains("razor"))) return "creep";
                    else if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.TwistedTreeline && System.Text.RegularExpressions.Regex.IsMatch(name, @"\d+\.\d+")
                        && (name.Contains("wraith") || name.Contains("golem") || name.Contains("wolf") || name.Contains("spider"))) return "creep";
                    else if (!minion.IsTargetable) return "trap";
                    else return "error";
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
                case LeagueSharp.GameObjectType.obj_Turret:
                case LeagueSharp.GameObjectType.obj_Levelsizer:
                case LeagueSharp.GameObjectType.obj_NavPoint:
                case LeagueSharp.GameObjectType.LevelPropSpawnerPoint:
                case LeagueSharp.GameObjectType.LevelPropGameObject:
                case LeagueSharp.GameObjectType.GrassObject:
                case LeagueSharp.GameObjectType.obj_Lake:
                case LeagueSharp.GameObjectType.obj_LampBulb:
                case LeagueSharp.GameObjectType.DrawFX: return "useless";
            }
            return "error";
        }
    }
    // unit
    public class Unit : Object {
        // members
        new public LeagueSharp.Obj_AI_Base @ref;
        public Unit(LeagueSharp.Obj_AI_Base a): base(a) {
            @ref = a;
        }
        // methods
        public bool InRange(Unit target) { return @ref.ServerPosition.Distance(target.@ref.Position) < @ref.AttackRange + @ref.BoundingRadius + target.@ref.BoundingRadius; }
        public bool InRange(Unit target, LeagueSharp.SpellSlot spell) { return @ref.ServerPosition.Distance(target.@ref.Position) < @ref.Spellbook.GetSpell(spell).SData.CastRange; }
        public bool Buff(string name) {
            foreach (var buff in @ref.Buffs) {
                if (buff.IsValid && buff.IsActive && buff.Name == name) return true;
            }
            return false;
        }
        public bool Buff(System.Collections.Generic.List<string> list) {
            foreach (var buff in list) if (Buff(buff)) return true;
            return false;
        }
        public bool CanUse(LeagueSharp.SpellSlot spell) { return @ref.Spellbook.GetSpell(spell).State == LeagueSharp.SpellState.Ready; } // optimize it
        public LeagueSharp.SpellSlot Item(int id, bool usable = false) {
            var item = @ref.InventoryItems.FirstOrDefault(slot => slot.Id == (LeagueSharp.ItemId)id && (!usable || CanUse(slot.SpellSlot)));
            if (item != null) return item.SpellSlot; else return LeagueSharp.SpellSlot.Unknown;
        }
        public LeagueSharp.SpellSlot Item(System.Collections.Generic.List<int> list, bool usable = false) {
            foreach (var id in list) {
                var spell = Item(id);
                if (spell != LeagueSharp.SpellSlot.Unknown) return spell;
            }
            return LeagueSharp.SpellSlot.Unknown;
        }
    }
    // player
    public class Player: Unit {
        // members
        new public LeagueSharp.Obj_AI_Hero @ref;
        public static Player myHero = LeagueSharp.ObjectManager.Player.Data<Player>();
        public Player(LeagueSharp.Obj_AI_Hero a): base(a) {
            @ref = a;
        }
        // methods
        public int SmiteDamage() {
            var damage = 370 + @ref.Level * 20;
            if (@ref.Level > 4) damage = damage + (@ref.Level - 4) * 10;
            if (@ref.Level > 9) damage = damage + (@ref.Level - 9) * 10;
            if (@ref.Level > 14) damage = damage + (@ref.Level - 14) * 10;
            return damage;
        }
        public void Level(System.Collections.Generic.List<LeagueSharp.SpellSlot> list) {
            int req_Q = 0, _Q = @ref.Spellbook.GetSpell(LeagueSharp.SpellSlot.Q).Level;
            int req_W = 0, _W = @ref.Spellbook.GetSpell(LeagueSharp.SpellSlot.W).Level;
            int req_E = 0, _E = @ref.Spellbook.GetSpell(LeagueSharp.SpellSlot.W).Level;
            int req_R = 0, _R = @ref.Spellbook.GetSpell(LeagueSharp.SpellSlot.R).Level;
            foreach (var spell in list) {
                switch (spell) {
                    case LeagueSharp.SpellSlot.Q:
                        req_Q += 1;
                        if (req_Q > _Q) @ref.Spellbook.LevelSpell(spell);
                        break;
                    case LeagueSharp.SpellSlot.W:
                        req_W += 1;
                        if (req_W > _W) @ref.Spellbook.LevelSpell(spell);
                        break;
                    case LeagueSharp.SpellSlot.E:
                        req_E += 1;
                        if (req_E > _E) @ref.Spellbook.LevelSpell(spell);
                        break;
                    case LeagueSharp.SpellSlot.R:
                        req_R += 1;
                        if (req_R > _R) @ref.Spellbook.LevelSpell(spell);
                        break;
                }
            }
        }
        public void Attack(Unit target) { @ref.IssueOrder(LeagueSharp.GameObjectOrder.AttackUnit, target.@ref); }
        public void MoveTo(Object target) { @ref.IssueOrder(LeagueSharp.GameObjectOrder.MoveTo, target.@ref.Position); }
        public bool Cast(LeagueSharp.SpellSlot spell) { return (spell != LeagueSharp.SpellSlot.Unknown && CanUse(spell) && @ref.Spellbook.CastSpell(spell)); }
        public bool Cast(LeagueSharp.SpellSlot spell, LeagueSharp.GameObject target) { return (spell != LeagueSharp.SpellSlot.Unknown && CanUse(spell) && @ref.Spellbook.CastSpell(spell, target)); }
    }
    // creepSpawn
    public class CreepSpawn : Object {
        // members
        new public LeagueSharp.NeutralMinionCamp @ref;
        public int campNumber;
        public float spawn, respawn, dead;
        public CreepSpawn(LeagueSharp.NeutralMinionCamp a): base(a) {
            @ref = a;
            campNumber = CampNumber();
            spawn = Spawn() + 30; //bug with timers
            respawn = Respawn();
            dead = spawn;
            a.GetHashCode();
        }
        // methods
        public int CampNumber() {
            var result = System.Text.RegularExpressions.Regex.Match(@ref.Name, @"\d+");
            if (!result.Success) return 0;
            else {
                int num = 0;
                if (int.TryParse(result.Value, out num)) return num; else return 0;
            }
        }
        public bool Test(int id, LeagueSharp.GameMapId map = LeagueSharp.GameMapId.SummonersRift) { return LeagueSharp.Game.MapId == map && campNumber == id; }
        public bool Test(int[] id, LeagueSharp.GameMapId map = LeagueSharp.GameMapId.SummonersRift) { return LeagueSharp.Game.MapId == map && id.Contains(campNumber); }
        public bool Dragon() { return Test(6); }
        public bool Nashor() { return Test(12); }
        public bool Blue() { return Test(new[] { 1, 7 }); }
        public bool Red() { return Test(new[] { 4, 10 }); }
        public bool Cancer() { return Test(new[] { 15, 16 }); }
        public bool Wolf() { return Test(new[] { 2, 8 }); }
        public bool Wraith() { return Test(new[] { 3, 9 }); }
        public bool Golem() { return Test(new[] { 5, 11 }); }
        public bool Wight() { return Test(new[] { 13, 14 }); }
        public float Spawn() {
            if (Dragon()) return 150;
            else if (Nashor()) return 750;
            else if (Cancer()) return 150;
            else return 120;
        }
        public float Respawn() {
            if (Dragon()) return 360;
            else if (Nashor()) return 420;
            else if (Cancer()) return 180;
            else if (Blue() || Red()) return 300;
            else return 100;
        }
        public System.Collections.Generic.IEnumerable<Creep> Creeps(bool dead = false) { return Interface.data.Values.OfType<Creep>().Where(c => c.@ref.IsVisible && c.@ref.IsDead == dead && c.@ref.CampNumber == campNumber); }
        public float Health() { return Creeps().Sum(creep => creep.@ref.Health); }
        public Creep BigCreep() {
            Creep max = null;
            foreach (var c in Creeps()) { if (max == null || c.@ref.MaxHealth > max.@ref.MaxHealth) max = c; }
            return max;
        }
        public bool Started() {
            foreach (var c in Creeps()) { if (c.@ref.Health < c.@ref.MaxHealth) return true; }
            return false;
        }
        public void Set(bool state) { if (state) dead = 0; else dead = LeagueSharp.Game.ClockTime + respawn; }
        public float Get() { return System.Math.Max(0, dead - LeagueSharp.Game.ClockTime); }
        public void Refresh(bool force = false) {
            if (Creeps().Any()) Set(true);
            else if (Get() == 0 && force) Set(false);
            else if (Get() == 0 && !s.myHero.@ref.IsDead && !Cancer() && s.myHero.@ref.ServerPosition.CanSee(@ref.Position))
                Timer.Once((h) => { if (s.myHero.@ref.ServerPosition.CanSee(@ref.Position)) Refresh(true); }).Cooldown(LeagueSharp.Game.Ping/1000+0.05f).Start();
            else if (Creeps(true).Any()) Set(false);
        }
    }
    // creep
    public class Creep : Unit {
        // members
        new public LeagueSharp.Obj_AI_Minion @ref;
        public int campNumber;
        public Creep(LeagueSharp.Obj_AI_Minion a): base(a) {
            @ref = a;
            campNumber = a.CampNumber;
        }
        // methods
        public CreepSpawn CreepSpawn() { return Interface.data.Values.OfType<CreepSpawn>().FirstOrDefault(cs => cs.campNumber == campNumber); }
    }
    // extension interface
    public static class Interface {
        public static System.Collections.Generic.Dictionary<int, Object> data = new System.Collections.Generic.Dictionary<int, Object>();
        public static void Add(Object unit) { data.Add(unit.@ref.Index, unit); }
        public static void Add(LeagueSharp.GameObject unit) {
            var @class = Unit.Class(unit);
            if (@class == "useless") return;
            else if (@class == "creep") Add(new Creep(unit as LeagueSharp.Obj_AI_Minion));
            else if (@class == "creepSpawn") Add(new CreepSpawn(unit as LeagueSharp.NeutralMinionCamp));
            else if (@class == "player") Add(new Player(unit as LeagueSharp.Obj_AI_Hero));
            else if (unit is LeagueSharp.Obj_AI_Base) Add(new Unit(unit as LeagueSharp.Obj_AI_Base));
            else data.Add(unit.Index, new Object(unit));
        }
        public static void Remove(Unit unit) { Remove(unit.@ref); }
        public static void Remove(LeagueSharp.GameObject unit) { data.Remove(unit.Index); }
        static Interface() {
            LeagueSharp.GameObject.OnCreate += (obj, a) => Add(obj);
            LeagueSharp.GameObject.OnDelete += (obj, a) => Remove(obj);
            foreach (var obj in LeagueSharp.ObjectManager.Get<LeagueSharp.GameObject>()) Add(obj);
        }
        // extension
        public static T Data<T>(this LeagueSharp.GameObject unit) where T : Object {return (T)data[unit.Index];}
    }
    // timer
    public static class Timer {
        public class Handle {
            public bool later = true;
            public float lastcall, cooldown = 0;
            public System.Action<Handle> callback = delegate { };
            public void Disable() { list.Remove(this); }
            public Handle Start() { later = false; return this; }
            public Handle Stop() { later = true; return this; }
            public Handle Cooldown(float v) { cooldown = v; return this; }
            public Handle Callback(System.Action<Handle> @in) { callback = @in; return this; }
        }
        public static System.Collections.Generic.List<Handle> list = new System.Collections.Generic.List<Handle>();
        public static Handle Add(System.Action<Handle> callback) { var h = new Handle(); list.Add(h); return h.Callback(callback); }
        public static Handle Once(System.Action<Handle> callback) {return Add(delegate(Handle h) { callback(h); h.Disable(); });}
        static Timer() {
            LeagueSharp.Game.OnUpdate += delegate {
                for (int i = list.Count; i-- > 0; ) {
                    var h = list[i];
                    if (!h.later && h.lastcall + h.cooldown <= LeagueSharp.Game.ClockTime) {
                        h.lastcall = LeagueSharp.Game.ClockTime;
                        h.callback(h);
                    }
                }
            };
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
            Need = delegate { return s.myHero.@ref.Health / s.myHero.@ref.MaxHealth < 0.45; };
            Logic = delegate { if (Need()) s.myHero.Cast(s.myHero.Item(id)); };
        }
    }
    // spell attack
    public class SpellAttack: Parent {
        public System.Func<LeagueSharp.AttackableUnit, bool> T(LeagueSharp.SpellSlot spell) {
            var gs = s.myHero.@ref.Spellbook.GetSpell(spell);
            switch (gs.SData.TargettingType) {
                case LeagueSharp.SpellDataTargetType.Unit:
                    return delegate(LeagueSharp.AttackableUnit target) { return s.myHero.Cast(spell, target); };
                case LeagueSharp.SpellDataTargetType.Self:
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
            var gs = s.myHero.@ref.Spellbook.GetSpell(spell);
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
        // red
        public class RedBuff : Parent {
            public string own, apply;
            override public void Reset() {
                own = "blessingofthelizardelder";
                apply = "blessingofthelizardelderslow";
            }
        }
        // spell
        public class Spell : SpellAttack {
            public System.Func<Creep,CreepSpawn, bool> Q_Worth, W_Worth, E_Worth, R_Worth;
            new public System.Func<Creep,CreepSpawn, bool> Logic;
            override public void Reset() {
                Logic = (c,cs) => {
                    if (Q_Worth(c, cs) && Q(c.@ref)) return true;
                    else if (W_Worth(c, cs) && W(c.@ref)) return true;
                    else if (E_Worth(c, cs) && E(c.@ref)) return true;
                    else if (R_Worth(c, cs) && R(c.@ref)) return true;
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
                State = delegate() { return s.myHero.@ref.Spellbook.IsCastingSpell || s.myHero.@ref.Spellbook.IsChanneling; };
                Logic = delegate() { return Worth() && State(); };
            }
        }
        // smite
        public class Smite : Parent {
            public System.Func<Creep,CreepSpawn, bool> WorthStart,Worth, WorthEx,Logic;
            public LeagueSharp.SpellSlot spell;
            public static LeagueSharp.SpellSlot Find() {
                if (s.myHero.@ref.Spellbook.GetSpell(LeagueSharp.SpellSlot.Summoner1).Name.ToLower().Contains("smite")) return LeagueSharp.SpellSlot.Summoner1;
                else if (s.myHero.@ref.Spellbook.GetSpell(LeagueSharp.SpellSlot.Summoner2).Name.ToLower().Contains("smite")) return LeagueSharp.SpellSlot.Summoner2;
                else return LeagueSharp.SpellSlot.Unknown;
            }
            override public void Reset() {
                WorthStart = (c,cs) => {
                    return cs.Started() && c.@ref.MaxHealth > s.myHero.SmiteDamage() * 2; // not started or too small
                };
                Worth = (c, cs) => {
                    if (c.@ref.Health > s.myHero.SmiteDamage()) return false; // cant smitesteal
                    else return (cs.Dragon() || cs.Nashor() || cs.Blue() || cs.Red());
                };
                WorthEx = (c, cs) => { return false; };
                Logic = (c,cs) => {
                    if (c.@ref.CampNumber == 0 || s.myHero.@ref.ServerPosition.Distance(c.@ref.ServerPosition) > 850) return false;
                    else return WorthStart(c, cs) && (WorthEx(c, cs) || Worth(c, cs)) && s.myHero.Cast(spell, c.@ref); 
                };
                spell = Find();
            }
        }
        // smite active
        public class SmiteActive : Parent {
            public Smite smite;
            public Timer.Handle timer;
            public override void Reset() {
                smite = new Smite();
                smite.WorthEx = (c,cs) => {
                    return (cs.Wight() || cs.Golem()) &&
                        !LeagueSharp.ObjectManager.Get<LeagueSharp.Obj_AI_Hero>().Any(player => player.IsValid && !player.IsMe && !player.IsDead && player.ServerPosition.Distance(s.myHero.@ref.ServerPosition) < 1500);
                };
                if (timer != null) timer.Disable();
                timer = Timer.Add(delegate {
                    if (!s.myHero.@ref.IsDead)
                        foreach (var c in Interface.data.Values.OfType<Creep>()) { smite.Logic(c,c.CreepSpawn()); };
                }).Start();
            }
        }
        // refresh
        public class RefreshActive : Parent {
            public static RefreshActive instance;
            public Timer.Handle timer;
            public override void Reset() {
                if (instance != null) return;
                instance = this;
                if (timer != null) timer.Disable();
                timer = Timer.Add(delegate {
                    if (!s.myHero.@ref.IsDead)
                        foreach (var cs in Interface.data.Values.OfType<CreepSpawn>()) cs.Refresh();
                }).Start();
            }
        }
        // navigator
        public class Nav : Parent {
            public System.Func<bool> Fast;
            public System.Func<System.Collections.Generic.IEnumerable<CreepSpawn>> Candidates;
            public System.Func<CreepSpawn> CreepSpawn,CreepSpawnEx;
            override public void Reset() {
                Fast = delegate { return false; };
                Candidates = () => Interface.data.Values.OfType<CreepSpawn>().Where(ics => ics.@ref.Position.Side() == s.myHero.@ref.Team);
                CreepSpawn = delegate {
                    CreepSpawn result = CreepSpawnEx(); if (result != null) return result;
                    var fast = Fast();
                    var candidates = Candidates();
                    var resultScore = double.MaxValue;
                    foreach (var cs in candidates) {
                        // score
                        var score = 0d;
                        var get = cs.Get() * s.myHero.@ref.MoveSpeed;
                        var dist = s.myHero.@ref.ServerPosition.Distance(cs.@ref.Position);
                        if (cs.dead == cs.spawn || dist > get) score = dist; else score = dist + (get - dist) * 1.4;
                        if (cs.Started()) { if (cs.respawn > 200) score -= 3300; else score -= 2000; } else if (LeagueSharp.Game.MapId == LeagueSharp.GameMapId.SummonersRift) {
                            if (s.myHero.@ref.Level == 1) { if ((!fast && cs.Golem()) || (fast && cs.Wight())) score = score - 3300; } else if (s.myHero.@ref.Level >= 3 && fast) {
                                if (cs.Wolf() && (candidates.First((ics) => ics.Blue()).Get() < 3200 / s.myHero.@ref.MoveSpeed || candidates.First((ics) => ics.Wight()).Get() < 3200 / s.myHero.@ref.MoveSpeed)) score += 3300;
                                else if (cs.Wraith() && (candidates.First((ics) => ics.Red()).Get() < 3200 / s.myHero.@ref.MoveSpeed || candidates.First((ics) => ics.Golem()).Get() < 3200 / s.myHero.@ref.MoveSpeed)) score += 3300;
                            }
                        }
                        if (score < resultScore) { result = cs; resultScore = score; }
                    }
                    return result;
                };
                CreepSpawnEx = delegate { return null; };
            }
        }
        // target
        public class Target : Parent {
            public RedBuff redBuff;
            public System.Func<CreepSpawn, bool> Cleave;
            public System.Func<CreepSpawn, Creep> Creep, CreepEx;
            override public void Reset() {
                redBuff = new RedBuff();
                Cleave = (c) => false;
                CreepEx = (cs) => null;
                Creep = (cs) => {
                    var target = CreepEx(cs); if (target != null) return target;
                    var cleave = Cleave(cs);
                    var red = s.myHero.Buff(redBuff.own);
                    foreach (var c in cs.Creeps()) {
                        if (target == null) target = c;
                        else if (red && (target.Buff(redBuff.apply) || c.Buff(redBuff.apply))) {
                            if (target.Buff(redBuff.apply) && !c.Buff(redBuff.apply)) target = c;
                        } else if (cleave && c.@ref.MaxHealth > target.@ref.MaxHealth) target = c;
                        else if (!cleave && c.@ref.MaxHealth < target.@ref.MaxHealth) target = c;
                        else if (c.@ref.MaxHealth == target.@ref.MaxHealth && c.@ref.NetworkId > target.@ref.NetworkId) target = c;
                    }
                    return target;
                };
                
            }
        }
        // killer
        public class Kill : Parent {
            public HealthPot pot;
            public Channel channel;
            public Spell spell;
            public System.Action<Creep,CreepSpawn> Logic;
            override public void Reset() {
                pot = new HealthPot();
                channel = new Channel();
                spell = new Spell();
                Logic = (c,cs) => {
                    pot.Logic();
                    if (channel.Logic()) return;
                    spell.Logic(c,cs);
                    s.myHero.Attack(c);
                };
            }
        }
        // move
        public class Move : Parent {
            public Channel channel;
            public System.Func<CreepSpawn, bool> LogicEx,Logic;
            override public void Reset() {
                channel = new Channel();
                LogicEx = (cs) => false;
                Logic = (cs) => {
                    if (channel.Logic() || LogicEx(cs)) return true;
                    else if (s.myHero.@ref.ServerPosition.Distance(cs.@ref.Position) < 50) return false;
                    s.myHero.MoveTo(cs);
                    return true;
                };
            }
        }
        // cycle
        public class Cycle : Parent {
            public Nav nav;
            public Target target;
            public Kill kill;
            public Move move;
            public RefreshActive refresh;
            public SmiteActive smite;
            public System.Func<bool> Logic;
            override public void Reset() {
                nav = new Nav();
                target = new Target();
                kill = new Kill();
                move = new Move();
                refresh = new RefreshActive();
                smite = new SmiteActive();
                Logic = delegate {
                    var creepSpawn = nav.CreepSpawn();
                    if (LeagueSharp.Hud.SelectedUnit != null && LeagueSharp.Hud.SelectedUnit.Data<Object>().@class == "creep") creepSpawn = LeagueSharp.Hud.SelectedUnit.Data<Creep>().CreepSpawn();
                    if (creepSpawn == null) return false;
                    var creep = target.Creep(creepSpawn);
                    if (creep != null) kill.Logic(creep, creepSpawn);
                    else move.Logic(creepSpawn);
                    return true;
                };
            }
        }
        // cycle with heroes
        public class CycleEx : Cycle {
            public void Transform(string charName) {
                Reset();
                switch (charName) {
                    case "Warwick":
                        kill.spell.Q_Worth = (c, cs) => { return true; };
                        kill.spell.W_Worth = (c, cs) => { return s.myHero.InRange(c) && cs.Health() > s.myHero.SmiteDamage(); };
                        break;
                    case "MasterYi":
                        kill.spell.Q_Worth = (c, cs) => { return true; };
                        kill.spell.E_Worth = new CycleEx("Warwick").kill.spell.W_Worth;
                        move.channel.Worth = kill.channel.Worth = () => { return s.myHero.@ref.Health / s.myHero.@ref.MaxHealth < 1; };
                        move.LogicEx = (cs) => { return s.myHero.@ref.Health / s.myHero.@ref.MaxHealth < 0.45 && s.myHero.Cast(LeagueSharp.SpellSlot.W); };
                        break;
                }
            }
            public CycleEx(string charName) { Transform(charName); }
        }
        // switcher
        public class Switch: CycleEx {
            public uint switchButton; 
            public Timer.Handle timer;
            public void GuiFull() {
                var menu = new LeagueSharp.Common.Menu("jungler slack", "jungler slack", true);
                // farm button
                var f1 = new LeagueSharp.Common.KeyBind(switchButton, LeagueSharp.Common.KeyBindType.Toggle, false);
                var farmTick = new LeagueSharp.Common.MenuItem("farm", "farm");
                farmTick.SetValue(f1);
                farmTick.ValueChanged += (h, a) => timer.later = !a.GetNewValue<LeagueSharp.Common.KeyBind>().Active;
                farmTick.DontSave();
                menu.AddItem(farmTick);
                // farm button disabler
                LeagueSharp.Game.OnWndProc += (a) => {
                    if (a.Msg == 516 && a.WParam == 2) {
                        f1.Active = false;
                        farmTick.SetValue(f1);
                    }
                };
                // smite button
                var smiteTick = new LeagueSharp.Common.MenuItem("smite", "smite",true).SetValue(true);
                smiteTick.ValueChanged += (h, a) => smite.timer.later = !a.GetNewValue<bool>();
                menu.AddItem(smiteTick);
                // fast button
                var fastTick = new LeagueSharp.Common.MenuItem("fast", "fast",true).SetValue(true);
                nav.Fast = () => fastTick.GetValue<bool>();
                menu.AddItem(fastTick);
                // leveling
                var recordMenu = new LeagueSharp.Common.Menu("records", "records");
                menu.AddSubMenu(recordMenu);
                LeagueSharp.Obj_AI_Hero.OnLevelUp += (unit, a) => { if (unit.IsMe) {
                    var me = unit as LeagueSharp.Obj_AI_Hero;
                    if (me.Level == 3 || me.Level == 6 || me.Level == 9 || me.Level == 11 || me.Level == 16) {
                        var record = new LeagueSharp.Common.MenuItem(me.Level.ToString(), "lvl " + me.Level + " : " + System.TimeSpan.FromSeconds(LeagueSharp.Game.ClockTime).ToString(@"mm\:ss"));
                        record.DontSave();
                        recordMenu.AddItem(record);
                    }
                }};
                // leveling sated
                LeagueSharp.Obj_AI_Hero.OnBuffAdd += (unit,a) => {if (unit.IsMe){
                    #warning l# bug
                    //System.Console.WriteLine(a.Buff.Name);
                }};
                // leveling init
                var startRecord = new LeagueSharp.Common.MenuItem("start", "start : " + System.TimeSpan.FromSeconds(LeagueSharp.Game.ClockTime).ToString(@"mm\:ss"));
                startRecord.DontSave();
                recordMenu.AddItem(startRecord);
                // fin
                menu.AddToMainMenu();
            }
            public void GuiLess() {
                LeagueSharp.Game.OnWndProc += delegate(LeagueSharp.WndEventArgs a) {
                    if (a.Msg == 256 && a.WParam == switchButton) timer.later = !timer.later; //F1
                    if (a.Msg == 516 && a.WParam == 2) timer.later = true; // RIGHT CLICK
                };
            }
            public Switch():base(s.myHero.@ref.ChampionName) {
                switchButton = 112; // F1
                timer = Timer.Add(delegate { Logic(); }).Cooldown(0.1f);
                GuiFull();
            }
        }
    }
}