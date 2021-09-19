using RockPaperScissors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPS.Models
{
    public class Player
    {
        public int Id { get; }
        public string Name { get; set; }
        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
