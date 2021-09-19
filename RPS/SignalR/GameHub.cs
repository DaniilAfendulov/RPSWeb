using Microsoft.AspNetCore.SignalR;
using RPS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using RockPaperScissors;
using System;
using System.Text;

namespace RPS.SignalR
{
    public class GameHub : Hub
    {
        GameContext _gameContext;
        public GameHub(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        public async Task CreateGame(Player player, string[] gameArgs)
        {
            RPSGameWith2P rpsGame = null;
            try
            {
                rpsGame = new RPSGameWith2P(gameArgs);
            }
            catch (ArgumentException ex)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", ex.Message);
                return;
            }
            
            var game = new Game(player, rpsGame);
            await _gameContext.Games.AddAsync(game);
            await _gameContext.SaveChangesAsync();
        }

        public async Task JoinGame(int gameId, Player player)
        {
            var game = await _gameContext.Games.FindAsync(gameId);
            game.Players[1] = player;
            await _gameContext.SaveChangesAsync();
        }

        public async Task SendMove(int gameId, int  playerId, string moveString)
        {
            try
            {
                var game = await _gameContext.Games.FindAsync(gameId);
                if (game == null)
                {
                    throw new ArgumentException("game isn't find");
                }

                var id = Array.FindIndex(game.Players, 0, 2, p => p.Id == playerId);
                if (id == -1)
                {
                    throw new ArgumentException("player isn't find");
                }

                var rpsGame = game.RPSGameWith2P;
                Move move;
                if (!rpsGame.TryFindMove(moveString, out move))
                {
                    throw new ArgumentException("move isn't find");
                }

                if (id == 0) rpsGame.Move1 = move;
                if (id == 1) rpsGame.Move2 = move;

                await Clients.Caller.SendAsync("ReceiveGameMessage", "Your move: " + moveString);

                if (rpsGame.IsMovesSets())
                {
                    await SendResult(gameId);
                }

                await _gameContext.SaveChangesAsync();
            }
            catch (ArgumentException ex)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", ex.Message);
            }
        }

        public async Task SendResult(int gameId)
        {
            var game = await _gameContext.Games.FindAsync(gameId);
            var gameRpc = game.RPSGameWith2P;
            var ids = game.Players.Select(p => p.Id);
            if (ids != null)
            {
                try
                {
                    var result = gameRpc.FindResult();
                    var player1Name = game.Players[0].Name;
                    var player2Name = game.Players[1].Name;
                    StringBuilder msg = new StringBuilder();
                    msg.AppendLine(player1Name + " move is " + gameRpc.Move1.Name);
                    msg.AppendLine(player2Name + " move is " + gameRpc.Move2.Name);
                    switch (result)
                    {
                        case ClashResult.Win:
                            msg.AppendLine(player1Name + " Win!");
                            break;
                        case ClashResult.Lose:
                            msg.AppendLine(player2Name + " Win!");
                            break;
                        case ClashResult.Draw:
                            msg.AppendLine("Draw!");
                            break;
                    }
                    await SendMsgToGame(game, msg.ToString());
                    _gameContext.Games.Remove(game);
                    await _gameContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", ex.Message);
                    throw;
                }

            }
        }


        public async Task SendMsgToGameById(int gameId, string msg)
        {
            var game = await _gameContext.Games.FindAsync(gameId);
            await SendMsgToGame(game, msg);
        }
        public async Task SendMsgToGame(Game game, string msg)
        {
            var ids = game.Players.Select(p => p.Id);
            await Clients.Users((IReadOnlyList<string>)ids).SendAsync("ReceiveGameMessage", msg);
        }

    }
}
