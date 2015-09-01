using System.Linq;

public static class NotLib {
    public static LeagueSharp.Obj_AI_Hero myHero = LeagueSharp.ObjectManager.Player;
    public static LeagueSharp.Obj_SpawnPoint allySpawn = LeagueSharp.ObjectManager.Get<LeagueSharp.Obj_SpawnPoint>().FirstOrDefault(spawn => spawn.IsValid && spawn.Team == LeagueSharp.ObjectManager.Player.Team);
    public static LeagueSharp.Obj_SpawnPoint enemySpawn = LeagueSharp.ObjectManager.Get<LeagueSharp.Obj_SpawnPoint>().FirstOrDefault(spawn => spawn.IsValid && spawn.Team != LeagueSharp.ObjectManager.Player.Team);
    // position related
    public static double Distance(this SharpDX.Vector3 from, SharpDX.Vector3 to) { return System.Math.Sqrt(System.Math.Pow(to.X - from.X, 2) + System.Math.Pow(to.Y - from.Y, 2)); }
    public static double Rad(this SharpDX.Vector3 from, SharpDX.Vector3 to) {
        if (to.Y > from.Y) return System.Math.Atan2(to.Y - from.Y, to.X - from.X); 
        else return System.Math.PI*2+System.Math.Atan2(to.Y - from.Y, to.X - from.X);
    }
    public static SharpDX.Vector3 Pos(this SharpDX.Vector3 from, SharpDX.Vector3 to, double range) {
        var rad = from.Rad(to);
        return new SharpDX.Vector3((float)(from.X + System.Math.Cos(rad) * range),from.Z,(float)(from.Y + System.Math.Sin(rad) * range));
    }
    public static bool CanSee(this SharpDX.Vector3 from, SharpDX.Vector3 to) { return from.Distance(to) < 1200 && LeagueSharp.NavMesh.LineOfSightTest(from,to); }
    public static bool CanSeeEx(this SharpDX.Vector3 from, SharpDX.Vector3 to) {
        var toEx = from.Pos(to,(System.Math.Max(0,from.Distance(to) - 20)));
        return from.Distance(to) < 1200 && LeagueSharp.NavMesh.LineOfSightTest(from,toEx); 
    }
    public static LeagueSharp.GameObjectTeam Side(this SharpDX.Vector3 unit) {
        var dist = NotLib.allySpawn.Position.Distance(unit) - NotLib.enemySpawn.Position.Distance(unit);
        if (dist > 1250) return NotLib.enemySpawn.Team; else if (dist < -1250) return NotLib.allySpawn.Team; else return LeagueSharp.GameObjectTeam.Neutral;
    }
    // buff related
    public static bool Buff(this LeagueSharp.Obj_AI_Base unit,string name) {
        foreach (var buff in unit.Buffs) {
            if (buff.IsValid && buff.IsActive && buff.Name == name) return true;
        }
        return false;
    }
    public static bool Buff(this LeagueSharp.Obj_AI_Base unit, System.Collections.Generic.List<string> list) {
        foreach (var buff in list) {if (myHero.Buff(buff)) return true;}
        return false;
    }
    public static bool Buff_HealthPot(this LeagueSharp.Obj_AI_Base unit){ return unit.Buff(new System.Collections.Generic.List<string>{"ItemCrystalFlask", "RegenerationPotion", "ItemMiniRegenPotion"}); }
    public static bool Buff_Red(this LeagueSharp.Obj_AI_Base unit) { return unit.Buff("blessingofthelizardelder"); }
    public static bool Buff_RedSlow(this LeagueSharp.Obj_AI_Base unit) { return unit.Buff("blessingofthelizardelderslow"); }
    // item+spell related
    public static bool CanUse(this LeagueSharp.SpellSlot spell, LeagueSharp.Obj_AI_Base unit) {return unit.Spellbook.GetSpell(spell).State == LeagueSharp.SpellState.Ready;} // optimize it
    public static LeagueSharp.SpellSlot Item(this LeagueSharp.Obj_AI_Base unit, int id ,bool usable = false) {
        var item = unit.InventoryItems.FirstOrDefault(slot => slot.Id == (LeagueSharp.ItemId)id && (!usable || slot.SpellSlot.CanUse(unit)));
        if (item != null) return item.SpellSlot; else return LeagueSharp.SpellSlot.Unknown;
    }
    public static LeagueSharp.SpellSlot Item(this LeagueSharp.Obj_AI_Base unit, System.Collections.Generic.List<int> list, bool usable = false) {
        foreach (var id in list) {
            var spell = unit.Item(id);
            if (spell != LeagueSharp.SpellSlot.Unknown) return spell;
        }
        return LeagueSharp.SpellSlot.Unknown;
    }
    public static LeagueSharp.SpellSlot Item_HealthPot(this LeagueSharp.Obj_AI_Base unit) { return unit.Item(new System.Collections.Generic.List<int> { 2041, 2003, 2010, 2009 }); }
    // order related
    public static bool InRange(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.AttackableUnit target) { return unit.ServerPosition.Distance(target.Position) < unit.AttackRange + unit.BoundingRadius + target.BoundingRadius; }
    public static void Attack(this LeagueSharp.Obj_AI_Base unit,LeagueSharp.AttackableUnit target) { unit.IssueOrder(LeagueSharp.GameObjectOrder.AttackUnit,target); }
    public static void MoveTo(this LeagueSharp.Obj_AI_Base unit, SharpDX.Vector3 pos,bool randomize = true) { unit.IssueOrder(LeagueSharp.GameObjectOrder.MoveTo, pos); }
    public static void Cast(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.SpellSlot spell) { if (spell != LeagueSharp.SpellSlot.Unknown && unit.Spellbook.GetSpell(spell).State == LeagueSharp.SpellState.Ready) unit.Spellbook.CastSpell(spell); }
    public static void Cast(this LeagueSharp.Obj_AI_Base unit, LeagueSharp.SpellSlot spell, LeagueSharp.GameObject target) { if (spell != LeagueSharp.SpellSlot.Unknown && unit.Spellbook.GetSpell(spell).State == LeagueSharp.SpellState.Ready) unit.Spellbook.CastSpell(spell, target); }
    public static void Level(this LeagueSharp.Obj_AI_Base unit, System.Collections.Generic.List<LeagueSharp.SpellSlot> list) {
        int req_Q = 0, _Q = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.Q).Level;
        int req_W = 0, _W = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.W).Level;
        int req_E = 0, _E = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.W).Level;
        int req_R = 0, _R = unit.Spellbook.GetSpell(LeagueSharp.SpellSlot.R).Level;
        foreach (var spell in list) {
            switch (spell){
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
    // unit
    public class Unit {
        public static System.Collections.Generic.Dictionary<short,LeagueSharp.GameObject> Data;
        public LeagueSharp.GameObject unit;
        public string @class;
        public Unit(LeagueSharp.GameObject a,string req) {
            @class = Class(a);
            if (@class != req) throw new System.Exception("NotLib."+req+"constructed with NotLib."+@class);
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
    public class CreepSpawn:Unit {
        public static System.Collections.Generic.Dictionary<int, NotLib.CreepSpawn> data = new System.Collections.Generic.Dictionary<int, NotLib.CreepSpawn>();
        new public LeagueSharp.NeutralMinionCamp unit;
        public int campNumber;
        public string type;
        public float spawn,respawn,dead;
        public CreepSpawn(LeagueSharp.NeutralMinionCamp a): base(a,"creepSpawn") {
            unit = a;
            campNumber = CampNumber();
            type = Type();
            spawn = Spawn()+30; //bug with timers
            respawn = Respawn();
            dead = spawn;
            a.GetHashCode();
        }
        //
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
        public LeagueSharp.Obj_AI_Minion BigCreep() {
            LeagueSharp.Obj_AI_Minion max = null;
            foreach (var creep in Creeps()) { if (max == null || creep.MaxHealth > max.MaxHealth) max = creep; }
            return max;
        }
        public bool Started() {
            foreach (LeagueSharp.Obj_AI_Minion creep in Creeps()) { if (creep.Health < creep.MaxHealth) return true; }
            return false;
        }
        public void Set(bool state) { if (state) dead = 0; else dead = LeagueSharp.Game.ClockTime + unit.Data().respawn; }
        public float Get() { return System.Math.Max(0, dead - LeagueSharp.Game.ClockTime); }
        public void Refresh(bool force = false) {
            if (Creeps().Any()) Set(true); 
            else if (Get() == 0 && (force || (!myHero.IsDead && unit.Data().type != "cancer" && myHero.ServerPosition.CanSee(unit.Position)))) Set(false); 
            else if (Creeps(true).Any()) Set(false);
        }
    }
    public static NotLib.CreepSpawn Data(this LeagueSharp.NeutralMinionCamp unit){
        if (!CreepSpawn.data.ContainsKey(unit.Index)) CreepSpawn.data[unit.Index] = new NotLib.CreepSpawn(unit);
        return CreepSpawn.data[unit.Index];
    }
    // creep
    public class Creep:Unit {
        public static System.Collections.Generic.Dictionary<int, NotLib.Creep> data = new System.Collections.Generic.Dictionary<int, NotLib.Creep>();
        new public static LeagueSharp.Obj_AI_Minion unit;
        public int campNumber;
        public Creep(LeagueSharp.Obj_AI_Minion a):base(a,"creep") {
            unit = a;
            campNumber = a.CampNumber;
        }
        //
        public LeagueSharp.NeutralMinionCamp CreepSpawn() { return LeagueSharp.ObjectManager.Get<LeagueSharp.NeutralMinionCamp>().FirstOrDefault(creepSpawn => creepSpawn.Data().campNumber == campNumber); }
    }
    public static NotLib.Creep Data(this LeagueSharp.Obj_AI_Minion unit){
        if (!Creep.data.ContainsKey(unit.Index)) Creep.data[unit.Index] = new NotLib.Creep(unit);
        return Creep.data[unit.Index];
    }
}