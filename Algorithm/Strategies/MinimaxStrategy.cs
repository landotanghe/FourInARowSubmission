using FourInARow.Strategies.Evaluators;
using System;
using System.Linq;

namespace FourInARow.Strategies
{
    public class MinimaxStrategy : IStrategy
    {
        private int _depth;
        
        private Board _board;
        private IEvaluator _evaluator;
        private int _me;
        private int _opponent;

        public MinimaxStrategy(IEvaluator evaluator, int depth = 1)
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
            foreach (var column in _board.GetOpenColumns())
            {
                using (var move = ExploreMove(_me, column))
                {
                    if (IsWin(_me, move.Row, move.Column))
                        return column;

                    double score = FindBestScoreForOpponent(currentDepth: 0);
                    if (highestScore < score)
                    {
                        highestScore = score;
                        bestColumn = column;
                    }
                }
            }
            return bestColumn;
        }


        private double FindBestScoreForMe(int currentDepth)
        {
            if (currentDepth == _depth)
            {
                return _evaluator.Evaluate(_board);
            }
            if (!_board.GetOpenColumns().Any())
            {
                return 0;
            }

            double highestScore = Double.NegativeInfinity;
            foreach(var column in _board.GetOpenColumns())
            {
                using (var move = ExploreMove(_me, column))
                {
                    if (IsWin(_me, move.Row, move.Column))
                        return Double.MaxValue;
                    
                    double score = FindBestScoreForOpponent(currentDepth);

                    if (highestScore < score)
                        highestScore = score;
                }                    
            }
            return highestScore;
        }
        
        private double FindBestScoreForOpponent(int currentDepth)
        {
            double lowestScore = Double.PositiveInfinity;
            if (!_board.GetOpenColumns().Any())
            {
                return 0;
            }
            foreach (var column in _board.GetOpenColumns())
            {
                using(var move = ExploreMove(_opponent, column))
                {
                    if (IsWin(_opponent, move.Row, move.Column))
                        return Double.MinValue;

                    double score = FindBestScoreForMe(currentDepth + 1);
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

            public MoveExplorer(MinimaxStrategy minimaxStrategy, int player, int column)
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
