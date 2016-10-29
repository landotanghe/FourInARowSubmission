using System;
using System.Linq;

namespace FourInARow.Strategies
{
    public class RandomizedStrategy : IStrategy
    {
        public int NextMove(Board board)
        {
            //TODO: write your code to choose best move on current board
            Random r = new Random();
            var validColumns = board.GetOpenColumns().ToArray();
            var i = r.Next(validColumns.Count());

            return validColumns[i];
        }
    }

}