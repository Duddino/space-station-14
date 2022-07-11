using Content.Server.Players;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;

using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.GameTicking.Commands
{
    sealed class RespawnCommand : IConsoleCommand
    {
        public string Command => "respawn";
        public string Description => "Respawns a player, kicking them back to the lobby.";
        public string Help => "respawn [player]";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player as IPlayerSession;
            if (args.Length > 1)
            {
                shell.WriteLine("Must provide <= 1 argument.");
                return;
            }

            var playerMgr = IoCManager.Resolve<IPlayerManager>();
            var ticker = EntitySystem.Get<GameTicker>();

            NetUserId userId;
            if (args.Length == 0)
            {
                if (player == null)
                {
                    shell.WriteLine("If not a player, an argument must be given.");
                    return;
                }

                userId = player.UserId;
            }
            else if (!playerMgr.TryGetUserId(args[0], out userId))
            {
                shell.WriteLine("Unknown player");
                return;
            }

            if (!playerMgr.TryGetSessionById(userId, out var targetPlayer))
            {
                if (!playerMgr.TryGetPlayerData(userId, out var data))
                {
                    shell.WriteLine("Unknown player");
                    return;
                }

                data.ContentData()?.WipeMind();
                shell.WriteLine("Player is not currently online, but they will respawn if they come back online");
                return;
            }

            ticker.Respawn(targetPlayer);
        }
    }

    [AnyCommand]
    public sealed class RespawnWhenGhost : IConsoleCommand
    {
	public string Command => "respawnghost";
	public string Description => "Respawns when ghost";
	public string Help => $"Usage: {Command}";
	
	
	public void Execute(IConsoleShell shell, string argStr, string[] args)
	{
	    var player = shell.Player as IPlayerSession;
            if (args.Length != 0)
            {
                shell.WriteLine("Must not provide any arguments.");
                return;
            }

            var playerMgr = IoCManager.Resolve<IPlayerManager>();
            var ticker = EntitySystem.Get<GameTicker>();
	    
	    if (!ticker.LobbyEnabled)
	    {
		shell.WriteLine("Lobby is not enabled");
		return;
	    }

	    if (ticker.RespawnTime < 0f)
	    {
		shell.WriteLine("Respawning is not enabled");
		return;
	    }

	    if(player == null)
	    {
		shell.WriteLine("Not a player");
		return;
	    }

	    // Not sure why it doesn't work
	    var mind = PlayerDataExt.ContentData(player)?.Mind;//player.ContentData?.Mind;
	    
	    //	    if (mind?.TimeOfDeath == null)
	    if(mind == null)
	    {
		shell.WriteLine("Character is not dead.");
		return;
	    }
	    string? name = mind.CharacterName;
	    if(name == null)
	    {
		shell.WriteLine("No character found");
		return;
	    }
	    PlayerDataExt.ContentData(player)?.UsedCharacters?.Add(name);
	    ticker.Respawn(player);
        }
	
    }
}
