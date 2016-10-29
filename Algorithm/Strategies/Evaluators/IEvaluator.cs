using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourInARow.Strategies.Evaluators
{
    public interface IEvaluator
    {
        void InitializeHeuristics(Board board);

        void UpdateHeuristicsAfterMove(int player, int row, int column);

        void UpdateHeuristicsAfterUndo(int player, int row, int column);

        /// <summary>
        /// How good is the board is it is 'me'-players turn
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        double Evaluate(Board board);
    }
}
