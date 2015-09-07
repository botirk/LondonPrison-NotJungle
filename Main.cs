using System.Linq;


class _Main{
    static void Main(string[] args){
        System.Console.Clear();
        if (LeagueSharp.Game.Mode == LeagueSharp.GameMode.Running) new NotLib.Abstract.Jungle.Switch();
        else LeagueSharp.Game.OnStart += (a) => new NotLib.Abstract.Jungle.Switch();
    }
}