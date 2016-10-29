using FourInARow.Strategies.Evaluators;
using System;
using System.Linq;

namespace FourInARow.Strategies
{
    public class AlphaBetaStrategyWithOrdering : IStrategy
    {
        private int _depth;

        private Board _board;
        private IEvaluator _evaluator;
        private int _me;
        private int _opponent;

        public AlphaBetaStrategyWithOrdering(IEvaluator evaluator, int depth = 1)
        {
            _evaluator = evaluator;
            _depth = depth;
        }
        
        public int NextMove(Board board)
        {
            _board = board;
            _me = _board.GetmyBotId();
            _opponent = _board.GetOpponentId();

            _evaluator.InitializeHeuristics(board);

            return FindBestMoveForMe();
        }

        private int FindBestMoveForMe()
        {
            int bestColumn = 0;
            double highestScore = Double.NegativeInfinity;
            double alpha = Double.NegativeInfinity;
            double beta = Double.PositiveInfinity;

            foreach (var column in _board.GetOpenColumns())
            {
                using (var move = ExploreMove(_me, column))
                {
                    if (IsWin(_me, move.Row, move.Column))
                        return column;

                    double score = FindBestScoreForOpponent(alpha, beta, currentDepth: 0);

                    // _me(Max) will at least get this score
                    if (alpha < score)
                        alpha = score;

                    if (highestScore < score)
                    {
                        highestScore = score;
                        bestColumn = column;
                    }
                }
            }
            return bestColumn;
        }
        
        private double FindBestScoreForMe(double alpha, double beta, int currentDepth)
        {
            if (currentDepth == _depth)
            {
                return _evaluator.Evaluate(_board);
            }
            if (! _board.GetOpenColumns().Any())
            {
                return 0;
            }

            double highestScore = Double.NegativeInfinity;
            foreach (var column in _board.GetOpenColumns().OrderByDescending(col => 3 - col))// central columns first
            {
                using (var move = ExploreMove(_me, column))
                {
                    if (IsWin(_me, move.Row, move.Column))
                        return Double.MaxValue;

                    double score = FindBestScoreForOpponent(alpha, beta, currentDepth);
                    
                    // _me(Max) will at least get this score
                    if (alpha < score)
                        alpha = score;

                    // _opponent(Min) won't pick anything that is higher than the lowest branch he can asure
                    if (beta <= score)
                        return score;

                    if (highestScore < score)
                        highestScore = score;
                }
            }
            return highestScore;
        }

        private double FindBestScoreForOpponent(double alpha, double beta, int currentDepth)
        {
            double lowestScore = Double.PositiveInfinity;
            if (!_board.GetOpenColumns().Any())
            {
                return 0;
            }
            foreach (var column in _board.GetOpenColumns())
            {
                using (var move = ExploreMove(_opponent, column))
                {
                    if (IsWin(_opponent, move.Row, move.Column))
                        return Double.MinValue;

                    double score = FindBestScoreForMe(alpha, beta, currentDepth + 1);

                    // _opponent(Min) can at least get under this score
                    if (score < beta)
                        beta = score;

                    // _me(Max) won't pick a move that's lower than another branch he already assured
                    if (score <= alpha)
                        return score;

                    if (score < lowestScore)
                        lowestScore = score;
                }
            }
            return lowestScore;
        }

        private MoveExplorer ExploreMove(int player, int column)
        {
            return new MoveExplorer(this, player, column);
        }

        private class MoveExplorer : IDisposable
        {
            private Board _board;
            private IEvaluator _evaluator;
            private int _player;

            public int Column
            {
                get; private set;
            }

            public int Row
            {
                get; private set;
            }

            public MoveExplorer(AlphaBetaStrategyWithOrdering minimaxStrategy, int player, int column)
            {
                _board = minimaxStrategy._board;
                _evaluator = minimaxStrategy._evaluator;
                _player = player;
                Column = column;

                Move();
            }

            public void Dispose()
            {
                Undo();
            }

            private void Move()
            {
                Row = _board.DropDisc(Column, _player);
                _evaluator.UpdateHeuristicsAfterMove(_player, Row, Column);
            }

            private void Undo()
            {
                _board.RemoveTopDisc(Column);
                _evaluator.UpdateHeuristicsAfterUndo(_player, Row, Column);
            }
        }

        private bool IsWin(int player, int row, int column)
        {
            return _board.IsPlayer(player, row, column)
                && _board.GetPotentialLength(player, row, column) >= 4;
        }
    }
}
