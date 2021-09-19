using RockPaperScissors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPS.Models
{
    public class Game
    {
        public int Id { get; }
        public Player[] Players { get; set;  } 
        public RPSGameWith2P RPSGameWith2P { get; set;  }

        public Game(Player player1, RPSGameWith2P game)
        {
            Id = player1.Id;
            Players = new Player[2];
            Players[0] = player1;
            RPSGameWith2P = game;
        }
    }
}
