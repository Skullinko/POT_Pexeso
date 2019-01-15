using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Pexeso.ChatLibrary;
using Pexeso.ChatLibrary.Model;
using Pexeso.ChatLibrary.PexesoService;
using Pexeso.Database.Model;
using Game = Pexeso.Database.Model.Game;

namespace Pexeso.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PexesoService : IPexesoService
    {
        public PexesoContext Db { get; set; }

        private static IPexesoClient ClientCallback => OperationContext.Current.GetCallbackChannel<IPexesoClient>();
        public Dictionary<string, PlayingClient> ConnectedPlayers { get; set; }
        private readonly Random _availablePlayerRandom;

        public Dictionary<int, Game> AllGames { get; set; }
        private int _internalGameId;
        private readonly string _gameCardsChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly Random _gameBoardRandom;

        public PexesoService()
        {
            Db = new PexesoContext();

            ConnectedPlayers = new Dictionary<string, PlayingClient>();
            _availablePlayerRandom = new Random();

            AllGames = new Dictionary<int, Game>();
            _gameBoardRandom = new Random();
        }

        ~PexesoService()
        {
            Db.Dispose();
        }

        public bool Register(string nick)
        {
            if (ConnectedPlayers.ContainsKey(nick))
                return false;

            ConnectedPlayers.Add(nick, new PlayingClient(ClientCallback));

            return true;
        }

        public void Unregister(string nick)
        {
            if (ConnectedPlayers[nick].IsPlaying)
            {
                var game = AllGames.First(pair => pair.Value.MyMatches.ContainsKey(nick)).Value;

                if (game != null)
                {
                    var otherMatch = game.MyMatches.Select(pair => pair.Value).First(match => match.PlayerNick != nick);

                    ConnectedPlayers[otherMatch.PlayerNick].Client.GameCancel();
                    ConnectedPlayers[otherMatch.PlayerNick].IsPlaying = false;
                    SendMessageToAll(new TextMessage { SenderNick = otherMatch.PlayerNick, Type = MessageType.PlayerFinishedMessage });

                    AllGames.Remove(game.Id);
                }
            }

            ConnectedPlayers.Remove(nick);
        }
        
        public bool GetRandomAvailablePlayer(string nick, out string otherNick)
        {
            var list = ConnectedPlayers.Where(pair => pair.Key != nick && pair.Value.IsPlaying == false).Select(pair => pair.Key).ToList();

            if (list.Count < 1)
            {
                otherNick = "";
                return false;
            }

            otherNick = list[_availablePlayerRandom.Next(list.Count)];
            while (otherNick == nick)
            {
                otherNick = list[_availablePlayerRandom.Next(list.Count)];
            }

            return true;
        }

        public void InvitePlayer(InvitationMessage message)
        {
            try
            {
                if (!ConnectedPlayers[message.ReceiverNick].Client.AcceptInvitation(message))
                    return;

                message.GameId = _internalGameId++;
                message.GameCards = GenerateGameCards(message.GameSize);

                ConnectedPlayers[message.SenderNick].Client.ReceiveGame(message, true);
                ConnectedPlayers[message.ReceiverNick].Client.ReceiveGame(message, false);

                AllGames.Add(message.GameId, CreateGame(message));

                ConnectedPlayers[message.SenderNick].IsPlaying = true;
                SendMessageToAll(new TextMessage {SenderNick = message.SenderNick, Type = MessageType.PlayerStartedMessage});
                ConnectedPlayers[message.ReceiverNick].IsPlaying = true;
                SendMessageToAll(new TextMessage {SenderNick = message.ReceiverNick, Type = MessageType.PlayerStartedMessage});
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public void CardRevealed(GameMessage message)
        {
            try
            {
                AllGames[message.GameId].MyMatches[message.SenderNick].NumberOfMoves++;
                ConnectedPlayers[message.ReceiverNick].Client.RevealCard(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);

                ConnectedPlayers[message.SenderNick].Client.GameCancel();
            }
        }

        public void ReceiveGameData(GameMessage message)
        {
            try
            {
                var game = EvaluateGame(message);

                AllGames.Remove(message.GameId);

                ConnectedPlayers[message.SenderNick].IsPlaying = false;
                SendMessageToAll(new TextMessage { SenderNick = message.SenderNick, Type = MessageType.PlayerFinishedMessage });
                ConnectedPlayers[message.ReceiverNick].IsPlaying = false;
                SendMessageToAll(new TextMessage { SenderNick = message.ReceiverNick, Type = MessageType.PlayerFinishedMessage });

                SaveGame(game);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ReceiveGameCancel(GameMessage message)
        {
            try
            {
                ConnectedPlayers[message.SenderNick].Client.GameCancel();
                ConnectedPlayers[message.ReceiverNick].Client.GameCancel();

                AllGames.Remove(message.GameId);

                ConnectedPlayers[message.SenderNick].IsPlaying = false;
                SendMessageToAll(new TextMessage { SenderNick = message.SenderNick, Type = MessageType.PlayerFinishedMessage });
                ConnectedPlayers[message.ReceiverNick].IsPlaying = false;
                SendMessageToAll(new TextMessage { SenderNick = message.ReceiverNick, Type = MessageType.PlayerFinishedMessage });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SendMessageToAll(TextMessage message)
        {
            var listOfPlayers = ConnectedPlayers.Values.ToList();

            foreach (var player in listOfPlayers)
            {
                try
                {
                    player.Client.ReceiveMessage(message);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private List<char> GenerateGameCards(GameSize gameSize)
        {
            var numberOfCards = gameSize.Height() * gameSize.Width();

            var index = _gameBoardRandom.Next(_gameCardsChars.Length);

            var result = new List<char>(numberOfCards);
            for (var i = 0; i < numberOfCards / 2; i++)
            {
                var c = _gameCardsChars[(index + i) % _gameCardsChars.Length];
                result.Add(c);
                result.Add(c);
            }

            return new Queue<char>(result.OrderBy(i => _gameBoardRandom.Next())).ToList();
        }

        private Game CreateGame(InvitationMessage message)
        {
            var newGame = new Game
            {
                GameSize = message.GameSize,
                Duration = DateTime.Now.TimeOfDay
            };

            newGame.MyMatches.Add(message.SenderNick, new Match {PlayerNick = message.SenderNick});
            newGame.MyMatches.Add(message.ReceiverNick, new Match {PlayerNick = message.ReceiverNick});

            return newGame;
        }

        private Game EvaluateGame(GameMessage message)
        {
            var game = AllGames[message.GameId];
            game.Duration = DateTime.Now.TimeOfDay - game.Duration;

            if (message.SenderNick == message.WinnerNick)
            {
                game.MyMatches[message.SenderNick].Result = MatchResult.Win;
                game.MyMatches[message.ReceiverNick].Result = MatchResult.Lose;
            }
            else if (message.ReceiverNick == message.WinnerNick)
            {
                game.MyMatches[message.SenderNick].Result = MatchResult.Lose;
                game.MyMatches[message.ReceiverNick].Result = MatchResult.Win;
            }
            else
            {
                game.MyMatches[message.SenderNick].Result = MatchResult.Draw;
                game.MyMatches[message.ReceiverNick].Result = MatchResult.Draw;
            }

            return game;
        }

        private void SaveGame(Game game)
        {
            Db.Games.Add(game);
            var gameId = game.Id;

            foreach (var match in game.MyMatches.Values)
            {
                match.GameId = gameId;
                Db.Matches.Add(match);
            }

            Db.SaveChangesAsync();
        }
    }
}