using FourInARow.Strategies;
using FourInARow.Strategies.Evaluators;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FourInARow
{
    public class Session
    {
        // TODO optimize when loading board: only scan top of non-empty columns
        public void Run()
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(512)));
            string line;
            
            Board board = new Board();

            //IStrategy strategy = new RandomizedStrategy();
            IStrategy strategy = new AlphaBetaStrategy(new ImmediateEvaluator(), 3); //new MinimaxStrategy(new SimpleEvaluator());

            while ((line = Console.ReadLine()) != null)
            {
                if (line == String.Empty) continue;

                var parts = line.Split(' ');

                switch (parts[0])
                {
                    case "settings":
                        switch (parts[1])
                        {
                            case "your_botid":
                                var myBotId = int.Parse(parts[2]);
                                board.SetMyBotId(myBotId);
                                break;
                        }
                        break;
                    case "update":
                        switch (parts[1])
                        {
                            case "game":
                                switch (parts[2])
                                {
                                    case "field":
                                        var boardArray = parts[3].Split(';')
                                            .Select(x => x.Split(',')
                                                .Select(int.Parse).ToArray())
                                            .ToArray();
                                        board.Update(boardArray);
                                    break;
                                }
                            break;
                        }
                        break;
                    case "action":
                        var move = strategy.NextMove(board);
                        Console.WriteLine("place_disc {0}", move);
                        break;
                }
            }
        }
    }
}