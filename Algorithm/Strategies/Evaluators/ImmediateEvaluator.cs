using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourInARow.Strategies.Evaluators
{
    public class ImmediateEvaluator : IEvaluator
    {
        private static readonly int[] RowStrenghtsEvenPlayer = new int[]{
            2,1,2,1,2,1
        };

        private static readonly int[] RowStrenghtsOddPlayer = new int[]{
            1,2,1,2,1,2
        };

        private int EmptyPlaces;
        protected Dictionary<int, int[][]> StrengthsPerPlayer;
        private Dictionary<int, int[]> RowStrengthsPerPlayer;
        // potential improvement Idea: strength indicates, how close you are to getting 4 in a row, winnableLocations
        // would tell if it is possible to even get it. Init all false and everytime a strength is found for a cell, set to true
        // protected Dictionary<int, bool[][]> WinnableLocations;
        protected Board _board;

        public ImmediateEvaluator()
        {
            StrengthsPerPlayer = new Dictionary<int, int[][]>();
            RowStrengthsPerPlayer = new Dictionary<int, int[]>();
        }

        public double Evaluate(Board board)
        {
            _board = board;
            CountEmptyPlaces();
            int me = board.GetmyBotId();
            int you = board.GetOpponentId();

            RowStrengthsPerPlayer = new Dictionary<int, int[]>();
            if(EmptyPlaces % 2 == 0)
            {
                RowStrengthsPerPlayer.Add(me, RowStrenghtsEvenPlayer);
                RowStrengthsPerPlayer.Add(you, RowStrenghtsOddPlayer);
            }
            else
            {
                RowStrengthsPerPlayer.Add(me, RowStrenghtsOddPlayer);
                RowStrengthsPerPlayer.Add(you, RowStrenghtsEvenPlayer);
            }

            StrengthsPerPlayer = new Dictionary<int, int[][]>();
            InitStrengths(me);
            InitStrengths(you);
                        
            return QuadraticStrengths(me) - QuadraticStrengths(you);
        }

        private void CountEmptyPlaces()
        {
            // TODO optimize
            EmptyPlaces = 0;
            for (int row = 0; row < _board.RowCount(); row++)
            {
                for (int column = 0; column < _board.ColumnCount(); column++)
                {
                    if (_board.IsEmpty(row, column))
                        EmptyPlaces++;
                }
            }
        }

        private void InitStrengths(int player)
        {
            CreateStrengthsTable(player);
            FindHorizontalStrengths(player);
            FindDownwardDiagonalStrengths(player);
            FindUpwardDiagonalStrengths(player);
            //Vertical is not considered since it is easily blocked
            //Avoiding an easy win by opponent is done in the Minimax itself by scoring very bad when losing
        }

        protected int QuadraticStrengths(int player)
        {
            int total = 0;
            for (int row = 0; row < _board.RowCount(); row++)
            {
                for (int column = 0; column < _board.ColumnCount(); column++)
                {
                    int strength = StrengthsPerPlayer[player][row][column];
                    total += strength * strength * RowStrengthsPerPlayer[player][row];
                }
            }
            return total;
        }

        protected void CreateStrengthsTable(int player)
        {
            int[][] strengths = new int[_board.RowCount()][];
            for (int row = 0; row < strengths.Length; row++)
            {
                strengths[row] = new int[_board.ColumnCount()];
                for (int column = 0; column < strengths[row].Length; column++)
                {
                    strengths[row][column] = 0;
                }
            }

            StrengthsPerPlayer.Add(player, strengths);
        }

        protected void FindHorizontalStrengths(int player)
        {
            for (int row = 0; row < _board.RowCount(); row++)
            {
                for(int column = 0; column < _board.ColumnCount() - 3; column++)
                {
                    CheckStrengthInFourSpaceToTheRight(player, row, column);
                }
            }
        }

        protected void FindUpwardDiagonalStrengths(int player)
        {
            for (int row = _board.RowCount() - 1; row >= 3; row--)
            {
                for (int column = 0; column < _board.ColumnCount() - 3; column++)
                {
                    CheckStrengthInFourSpaceToTheUpperRight(player, row, column);
                }
            }
        }

        protected void FindDownwardDiagonalStrengths(int player)
        {
            for (int row = 0; row < _board.RowCount() - 3; row++)
            {
                for (int column = 0; column < _board.ColumnCount() - 3; column++)
                {
                    CheckStrengthInFourSpaceToTheDownRight(player, row, column);
                }
            }
        }
        
        /// <summary>
        /// Get the number of discs in a Four-space row to the right from and starting in the given location
        /// If 4 in a row is still possible for the player in this Four-space, the strength for all the cells in
        /// this Four-space is updated
        /// </summary>
        /// <param name="player"></param>
        /// <param name="row"></param>
        /// <param name="startColumn"></param>
        private void CheckStrengthInFourSpaceToTheRight(int player, int row, int startColumn)
        {
            int playerDiscs = 0;
            for (int column = startColumn; column < startColumn + 4; column++)
            {
                if (_board.GetPlayer(row, column) == player)
                {
                    playerDiscs++;
                }
                else if (!_board.IsEmpty(row, column))
                {
                    // otherPlayer claimed a disc here, no chance to get 4 in a row in this fourSpace
                    return;
                }
            }

            for (int column = startColumn; column < startColumn + 4; column++)
            {
                StrengthsPerPlayer[player][row][column] = Math.Max(StrengthsPerPlayer[player][row][column], playerDiscs);
            }
        }


        private void CheckStrengthInFourSpaceToTheUpperRight(int player, int startRow, int startColumn)
        {
            int playerDiscs = 0;
            for (int i = 0; i < 4; i++)
            {
                int row = startRow - i;
                int column = startColumn + i;
                if (_board.GetPlayer(row, column) == player)
                {
                    playerDiscs++;
                }
                else if (!_board.IsEmpty(row, column))
                {
                    // otherPlayer claimed a disc here, no chance to get 4 in a row in this fourSpace
                    return;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                int row = startRow - i;
                int column = startColumn + i;
                StrengthsPerPlayer[player][row][column] = Math.Max(StrengthsPerPlayer[player][row][column], playerDiscs);
            }
        }


        private void CheckStrengthInFourSpaceToTheDownRight(int player, int startRow, int startColumn)
        {
            int playerDiscs = 0;
            for (int i = 0; i < 4; i++)
            {
                int row = startRow + i;
                int column = startColumn + i;
                if (_board.GetPlayer(row, column) == player)
                {
                    playerDiscs++;
                }
                else if (!_board.IsEmpty(row, column))
                {
                    // otherPlayer claimed a disc here, no chance to get 4 in a row in this fourSpace
                    return;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                int row = startRow + i;
                int column = startColumn + i;
                StrengthsPerPlayer[player][row][column] = Math.Max(StrengthsPerPlayer[player][row][column], playerDiscs);
            }
        }

        public void InitializeHeuristics(Board board)
        {
            // not needed
        }

        public void UpdateHeuristicsAfterMove(int player, int row, int column)
        {
            // not needed
        }

        public void UpdateHeuristicsAfterUndo(int player, int row, int column)
        {
            // not needed
        }
    }
}
