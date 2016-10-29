
using System;

namespace FourInARow.Strategies.Evaluators
{
    public class SimpleEvaluator : IEvaluator
    {
        private  Board _board;
        private int _me;
        private int _opponent;

        private int[][] _mePotentialLengths;
        private int[][] _opponentPotentialLenghts;
        private int[] _freeRowIndices;

        public void InitializeHeuristics(Board board)
        {
            _board = board;
            _me = _board.GetmyBotId();
            _opponent = _board.GetOpponentId();

            _freeRowIndices = CalculateFreeRowIndices();

            _mePotentialLengths = CreatePotentialLengthsTable(_me);
            _opponentPotentialLenghts = CreatePotentialLengthsTable(_opponent);

        }

        private int[] CalculateFreeRowIndices()
        {
            var freeRowIndices = new int[_board.ColumnCount()];
            for (int column = 0; column < _board.ColumnCount(); column++)
            {
                int row = _board.RowCount() - 1;
                while (row >= 0 && !_board.IsEmpty(row, column))
                {
                    row--;
                }
                freeRowIndices[column] = row;
            }
            return freeRowIndices;
        }

        public void UpdateHeuristicsAfterMove(int player, int row, int column)
        {
            _freeRowIndices[column]--;
            _mePotentialLengths[row][column] = 0;
            _opponentPotentialLenghts[row][column] = 0;
            UpdatePotentialLengthsAtEmptyLineEnds(player, row, column);
        }

        public void UpdateHeuristicsAfterUndo(int player, int row, int column)
        {
            _freeRowIndices[column]++;
            _mePotentialLengths[row][column] = _board.GetPotentialLength(_me, row, column);
            _opponentPotentialLenghts[row][column] = _board.GetPotentialLength(_opponent, row, column);
            UpdatePotentialLengthsAtEmptyLineEnds(player, row, column);
        }

        /// <summary>
        /// Find all empty cells at the end of the lines starting from the given cell for the given player.
        /// Recalculate the potential lengths at these empty cells.
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        private void UpdatePotentialLengthsAtEmptyLineEnds(int player, int row, int column)
        {
            var potentialLengths = player == _me ? _mePotentialLengths : _opponentPotentialLenghts;

            // north and south are special: guaranteed no empty cells in south, so no update needed there
            // north is guaranteed to be empty (if it exists) so always it.
            var northRow = row - 1;
            if (northRow > 0)
            {
                var north = new Location
                {
                    Row = northRow,
                    Column = column
                };
                var length = _board.GetPotentialLength(player, north);
                SetPotentialLength(potentialLengths, north, length);
            }

            var northEast = _board.GetNorthEastLineEnd(player, row, column).North().East();
            var southWest = _board.GetSouthWestLineEnd(player, row, column).South().West();
            if (_board.IsValidLocation(northEast) && _board.IsEmpty(northEast))
            {
                var length = _board.GetPotentialLength(player, northEast);
                SetPotentialLength(potentialLengths, northEast, length);
            }
            if (_board.IsValidLocation(southWest) && _board.IsEmpty(southWest))
            {
                var length = _board.GetPotentialLength(player, southWest);
                SetPotentialLength(potentialLengths, southWest, length);
            }

            var northWest = _board.GetNorthWestLineEnd(player, row, column).North().West();
            var southEast = _board.GetSouthEastLineEnd(player, row, column).South().East();
            if (_board.IsValidLocation(northWest) && _board.IsEmpty(northWest))
            {
                var length = _board.GetPotentialLength(player, northWest);
                SetPotentialLength(potentialLengths, northWest, length);
            }
            if (_board.IsValidLocation(southEast) && _board.IsEmpty(southEast))
            {
                var length = _board.GetPotentialLength(player, southEast);
                SetPotentialLength(potentialLengths, southEast, length);
            }

            var eastCol = _board.GetEastLineEnd(player, row, column) + 1;
            var westCol = _board.GetWestLineEnd(player, row, column) - 1;
            if (eastCol < _board.ColumnCount() && _board.IsEmpty(row, eastCol))
            {
                var east = new Location
                {
                    Row = row,
                    Column = eastCol
                };
                var length = _board.GetPotentialLength(player, east);
                SetPotentialLength(potentialLengths, east, length);
            }
            if (westCol > 0 && _board.IsEmpty(row, westCol))
            {
                var west = new Location
                {
                    Row = row,
                    Column = westCol
                };
                var length = _board.GetPotentialLength(player, west);
                SetPotentialLength(potentialLengths, west, length);
            }
        }

        private void SetPotentialLength(int[][] potentialLengths, Location location, int length)
        {
            potentialLengths[location.Row][location.Column] = length;
        }

        /// <summary>
        /// Create a table and initialize it with the potential lengths for the empty cells
        /// In order to limit the cells calculated,in every column we start from the lowest empty cell,
        /// then work our way upwards, as long as the cell has a neighbour directly SE or SW  with a disc
        /// This pruning is done, because GetPotentialLength() might be heavy to calculate.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int[][] CreatePotentialLengthsTable(int player)
        {
            // TODO 1's or 0s: 0 when cell belongs to specific player, 1 or higher when empty
            var potentialLengths = new int[_board.RowCount()][];
            for (int row = 0; row < potentialLengths.Length; row++)
            {
                potentialLengths[row] = new int[_board.ColumnCount()];
                for (int column = 0; column < potentialLengths[row].Length; column++)
                {
                    potentialLengths[row][column] = 0;
                }
            }
            var lastRow = _board.LastRow();

            // first column
            {
                int row = _freeRowIndices[0];
                while (row >= 0
                    && (row == lastRow || _board.IsEmpty(row + 1, 1)))
                {
                    potentialLengths[row][0] = _board.GetPotentialLength(player, row, 0);
                    row--;
                }
            }

            // middle columns
            for (int column = 1; column < _board.LastColumn(); column++)
            {
                int row = _freeRowIndices[column];
                while (row >= 0
                    && (row == lastRow || !_board.IsEmpty(row + 1, column - 1) || _board.IsEmpty(row + 1, column + 1)))
                {
                    potentialLengths[row][column] = _board.GetPotentialLength(player, row, column);
                    row--;
                }
            }

            // last column
            {
                int lastColumn = _board.LastColumn();
                int row = _freeRowIndices[lastColumn];
                while (row >= 0
                && (row == lastRow || _board.IsEmpty(row + 1, lastColumn - 1)))
                {
                    potentialLengths[row][lastColumn] = _board.GetPotentialLength(player, row, lastColumn);
                    row--;
                }
            }

            return potentialLengths;
        }

        public double Evaluate(Board board)
        {
            double score = 0.0;
            for(int column = 0; column < _freeRowIndices.Length; column++)
            {

                for(int row = _freeRowIndices[column] + 1; row < _board.RowCount(); row++)
                {
                    // centrality of unempty cell
                    var centrality = Math.Abs(_board.ColumnCount() / 2.0 - column);
                    if (_board.IsPlayer(_me, row, column))
                    {
                        score += centrality;
                    }
                    else
                    {
                        score -= centrality;
                    }
                }

                for (int row = 0 ; row < _freeRowIndices[column]; row++)
                {
                    var myLength = _mePotentialLengths[row][column];
                    var opponentLength = _opponentPotentialLenghts[row][column];
                    // potential lengths of empty cells
                    score += 10 * (myLength * myLength - opponentLength * opponentLength);
                }
            }

            return score;
        }
    }
}
