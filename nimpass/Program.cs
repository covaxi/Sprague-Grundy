using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nimpass
{
    class Pile
    {
        public int Count { get; set; }
        public bool Passed { get; set; }
        public override string ToString() => Passed || Empty ? $"{Count}" : $"[{Count}]";
        public bool Empty => Count == 0;
        public int ToInt => Count == 0 || Passed ? Count : Count + 1;
    }

    class Move
    {
        public int Pile { get; set; }
        public int Number { get; set; }
        public override string ToString() => $"({Pile} {Number}";
    }

    class Board
    {
        private List<Pile> piles;
        public List<Pile> Piles => piles;
        public int Grundy => piles.Aggregate(0, (x, y) => x ^ y.ToInt);
        public bool Empty => piles.All(x => x.Empty);
        public override string ToString() => $"{string.Join(" ", piles.Select((x, i) => $"{i}.{x}"))} G={Grundy}";
        public IEnumerable<Move> PossibleMoves => piles.SelectMany((p, i) =>
        {
            IEnumerable<int> stones = p.Count == 0 ? Enumerable.Empty<int>() : p.Passed ? Enumerable.Range(1, p.Count) : Enumerable.Range(0, p.Count + 1);
            return stones.Select(x => new Move { Pile = i, Number = x });
        });

        public Board(IEnumerable<Pile> piles)
        {
            this.piles = new List<Pile>(piles.Select(p => new Pile { Count = p.Count, Passed = p.Passed }));
        }

        public Board Move(Move move, string who = null)
        {
            if (move.Number == 0 && Piles[move.Pile].Passed || Piles[move.Pile].Empty)
                throw new Exception("Already passed on this pile");
            if (Piles[move.Pile].Count < move.Number)
                throw new Exception("Too many stones");

            if (!string.IsNullOrEmpty(who))
                Console.WriteLine($"{who} removed {move.Number} from pile #{move.Pile}");

            return new Board(piles.Select((p, i) => i == move.Pile
                ? new Pile { Count = p.Count - move.Number, Passed = move.Number == 0 || p.Passed }
                : new Pile { Count = p.Count, Passed = p.Passed }));
        }
    }

    class Program
    {
        static Random rnd = new Random();

        static void Main(string[] args)
        {
            var board = new Board(new []{ 1, 2, 3, 4, 5 }.Select(x => new Pile { Count = x, Passed = false }));
            Console.WriteLine("How to do a move: a b - take b stones (0 = pass) from pile #a (zero based), q - quit");

            while (true)
            {
                Console.WriteLine(board);
                if (board.Empty)
                {
                    Console.WriteLine("Bwahaha, you lose");
                    Console.ReadKey();
                    return;
                }

                Console.Write($"Make your move: ");
                
                var move = Console.ReadLine();
                if (move.ToLower() == "q")
                    return;

                var match = Regex.Match(move, "^\\s*(\\d+)\\s+(\\d+)\\s*", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    Console.WriteLine("Wrong move");
                    continue;
                }

                var pile = int.Parse(match.Groups[1].Value);
                var num = int.Parse(match.Groups[2].Value);

                try
                {
                    board = board.Move(new Move { Pile = pile, Number = num }, "YOU");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
                Console.WriteLine(board);

                if (board.Empty)
                {
                    Console.WriteLine("Cheater");
                    Console.ReadKey();
                    return;
                }

                var myMove = board.PossibleMoves.FirstOrDefault(m =>
                {
                    return board.Move(m).Grundy == 0;
                });

                if (myMove == null) 
                {
                    Console.WriteLine("Taking random move");
                    myMove = board.PossibleMoves.First();
                }

                board = board.Move(myMove, "ME");
            }
        }
    }
}
