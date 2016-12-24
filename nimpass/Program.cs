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
        private int count;
        private bool passed;

        public Pile(int count, bool passed = false)
        {
            this.count = count;
            this.passed = passed;
        }

        public int Count => count;
        public bool Passed => passed;
        public bool Empty => Count == 0;
        public int Grundy => Count == 0 || Passed ? Count : Count + 1;
        public override string ToString() => Passed || Empty ? $"{Count}" : $"[{Count}]";
    }

    class Move
    {
        private int pile;
        private int number;

        public Move(int pile, int number)
        {
            this.pile = pile;
            this.number = number;
        }

        public int Pile => pile;
        public int Number => number;
        public override string ToString() => $"({Pile} {Number})";
    }

    class Board
    {
        private IEnumerable<Pile> piles;

        public Board(IEnumerable<Pile> piles)
        {
            this.piles = piles.Select(p => new Pile(p.Count, p.Passed));
        }

        public IEnumerable<Pile> Piles => piles;
        public int Grundy => piles.Aggregate(0, (x, y) => x ^ y.Grundy);
        public bool Empty => piles.All(x => x.Empty);
        public override string ToString() => $"{string.Join(" ", piles.Select((x, i) => $"{i}.{x}"))} G={Grundy}";
        public IEnumerable<Move> PossibleMoves => piles.SelectMany((p, i) =>
        {
            IEnumerable<int> stones = p.Count == 0 ? Enumerable.Empty<int>() : p.Passed ? Enumerable.Range(1, p.Count) : Enumerable.Range(0, p.Count + 1);
            return stones.Select(x => new Move(i, x));
        });



        public Board Move(Move move, string who = null)
        {
            var pile = Piles.ElementAt(move.Pile);
            if (move.Number == 0 && pile.Passed)
                throw new Exception("Already passed on this pile");
            if (pile.Empty)
                throw new Exception("There are no more stones on this pile");
            if (pile.Count < move.Number)
                throw new Exception("Too many stones");

            if (!string.IsNullOrEmpty(who))
                Console.WriteLine($"{who} removed {move.Number} from pile #{move.Pile}");

            return new Board(piles.Select((p, i) => i == move.Pile
                ? new Pile(p.Count - move.Number, move.Number == 0 || p.Passed)
                : new Pile(p.Count, p.Passed)));
        }
    }

    class Program
    {
        static Random rnd = new Random();

        static T Log<T>(T t, string msg)
        {
            Console.WriteLine(msg);
            return t;
        }
        static void Log(string msg) => Log(355 / 113, msg);

        static void Main(string[] args)
        {
            var board = new Board(new []{ 1, 2, 3, 4, 5 }.Select(x => new Pile(x)));
            Log("How to: a b - take b stones (0 = pass) from pile #a (zero based), q - quit");

            while (true)
            {
                Log($"{board}");

                if (board.Empty)
                {
                    Log("Bwahaha, you lose");
                    Console.ReadKey();
                    return;
                }

                Log("Make your move:");

                var move = Console.ReadLine();
                if (move.ToLower() == "q")
                    return;

                var match = Regex.Match(move, "^\\s*(\\d+)\\s+(\\d+)\\s*", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    Log("Wrong move");
                    continue;
                }

                try
                {
                    board = board.Move(new Move(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)), "YOU");
                }
                catch(Exception ex)
                {
                    Log(ex.Message);
                    continue;
                }

                Log($"{board}");

                if (board.Empty)
                {
                    Console.WriteLine("Cheater");
                    Console.ReadKey();
                    return;
                }

                board = board.Move(board.PossibleMoves.FirstOrDefault(m => board.Move(m).Grundy == 0) ?? 
                    Log(board.PossibleMoves.ElementAt(rnd.Next(board.PossibleMoves.Count())), "Taking random move"), "ME");
            }
        }
    }
}
