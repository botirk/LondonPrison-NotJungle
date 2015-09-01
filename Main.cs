class _Main{
    static void Main(string[] args){
        var bot = new INotJungle();
        bot.Clear = delegate(LeagueSharp.Obj_AI_Minion creep, LeagueSharp.NeutralMinionCamp creepSpawn) {
            if (NotLib.myHero.InRange(creep)) {
                NotLib.myHero.Cast(LeagueSharp.SpellSlot.Q, creep);
            }
            NotLib.myHero.Attack(creep);
        };
        bot.LevelMe = new System.Collections.Generic.List<LeagueSharp.SpellSlot>{LeagueSharp.SpellSlot.Q};
    }
}