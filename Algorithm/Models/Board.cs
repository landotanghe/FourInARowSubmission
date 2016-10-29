using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourInARow
{
    public class Board
    {
        public const int NoDisc = 0;
        private int[][] _boardArray;
        private int _mybotId;
        private int _opponentId;

        public Board() { }

        public Board(Board board, int me, int opponent)
        {
            _mybotId = me;
            _opponentId = opponent;

            var rowCount = board.RowCount();
            var columnCount = board.ColumnCount();

            _boardArray = new int[rowCount][];
            for (int row = 0; row < rowCount; row++)
            {
                _boardArray[row] = new int[columnCount];
                for (int column = 0; column < columnCount; column++)
                {
                    _boardArray[row][column] = board._boardArray[row][column];
                }
            }
        }

        public Board Rotate()
        {
            var rotation = new Board(ColumnCount(), RowCount());
            for(int myRow = 0; myRow < RowCount(); myRow++)
            {
                for(int myCol = 0; myCol < ColumnCount(); myCol++)
                {
                    rotation._boardArray[myCol][myRow] = _boardArray[myRow][myCol];
                }
            }
            return rotation;
        }
        
        public Board(int rowCount, int columnCount)
        {
            _boardArray = new int[rowCount][];
            for(int row = 0; row < rowCount; row++)
            {
                _boardArray[row] = new int[columnCount];
                for(int column = 0; column < columnCount; column++)
                {
                    _boardArray[row][column] = NoDisc;
                }
            }
        }
        
        public bool IsValidLocation(Location location)
        {
            return location.Row > 0
                && location.Column > 0 && location.Column < LastColumn() //strange ordering because it is the most optimal one (game will evolve around center/bottom)
                && location.Row < LastRow();
        }

        public int GetmyBotId()
        {
            return _mybotId;
        }

        public int GetOpponentId()
        {
            return _opponentId;
        }

        public void SetMyBotId(int myBotId)
        {
            _mybotId = myBotId;

            if (_mybotId == 1)
                _opponentId = 2;
            else
                _opponentId = 1;
        }

        public void Update(int[][] boardArray)
        {
            _boardArray = boardArray;
        }

        public int ColumnCount()
        {
            return _boardArray[0].Length;
        }

        public int LastColumn()
        {
            return _boardArray[0].Length - 1;
        }

        public int RowCount()
        {
            return _boardArray.Length;
        }

        public int LastRow()
        {
            return _boardArray.Length - 1;
        }
        
        public bool IsMe(int row, int column)
        {
            return _boardArray[row][column] == _mybotId;
        }

        public bool IsPlayer(int player, int row, int column)
        {
            return _boardArray[row][column] == player;
        }

        public int GetPlayer(int row, int column)
        {
            return _boardArray[row][column];
        }

        public bool IsEmpty(int row, int column)
        {
            return _boardArray[row][column] == NoDisc;
        }

        public bool IsEmpty(Location location)
        {
            return _boardArray[location.Row][location.Column] == NoDisc;
        }

        public IEnumerable<int> GetOpenColumns()
        {
            for(int column = 0; column < ColumnCount(); column++)
            {
                if (_boardArray[0][column] == NoDisc)
                    yield return column;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="player"></param>
        /// <returns>row of dropped disk</returns>
        public int DropDisc(int column, int player)
        {
            var rowToFill = GetRowOfTopDiscInColumn(column) - 1;
            _boardArray[rowToFill][column] = player;
            return rowToFill;
        }
        
        public bool HasFourInARow(int player, int row, int column)
        {
            return _boardArray[row][column] == player &&
                (GetPotentialHorizontalLength(player, row, column) >= 4
                || GetPotentialVerticalLength(player, row, column) >= 4
                || GetPotentialDownwardDiagonalLength(player, row, column) >= 4
                || GetPotentialUpwardDiagonalLength(player, row, column) >= 4);
        }

        public int GetPotentialLength(int player, int row, int column)
        {
            var maxLength = GetPotentialHorizontalLength(player, row, column);
            maxLength = Math.Max(maxLength, GetPotentialVerticalLength(player, row, column));
            maxLength = Math.Max(maxLength, GetPotentialDownwardDiagonalLength(player, row, column));
            maxLength = Math.Max(maxLength, GetPotentialUpwardDiagonalLength(player, row, column));

            return maxLength;
        }

        public int GetPotentialLength(int player, Location location)
        {
            return GetPotentialLength(player, location.Row, location.Column);
        }
        
        /// <summary>
        ///  How long would the horizontal be if the player inserted a disk at the given cell
        /// </summary>
        /// <param name="player"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int GetPotentialHorizontalLength(int player, int row, int column)
        {
            int leftMost = GetWestLineEnd(player, row, column);
            int rightMost = GetEastLineEnd(player, row, column);

            return 1 + rightMost - leftMost;
        }
        
        /// <summary>
        /// How long would the vertical be if the player inserted a disk at the given cell
        /// </summary>
        /// <param name="player"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int GetPotentialVerticalLength(int player, int row, int column)
        {
            int highestRow = GetNorthLineEnd(player, row, column);
            int lowestRow = GetSouthLineEnd(player, row, column);

            return 1 + lowestRow - highestRow;
        }
        
        private int GetPotentialDownwardDiagonalLength(int player, int row, int column)
        {
            var startLocation = GetNorthWestLineEnd(player, row, column);
            var endLocation = GetSouthEastLineEnd(player, row, column);

            return 1 + endLocation.Row - startLocation.Row;
        }
        
        private int GetPotentialUpwardDiagonalLength(int player, int row, int column)
        {
            var startLocation = GetSouthWestLineEnd(player, row, column);
            var endLocation = GetNorthEastLineEnd(player, row, column);

            var length = 1 + startLocation.Row - endLocation.Row;
            return length;
        }
        
#region lineEndings
        /// <summary>
        /// Find the row where the northward line for the given player starting from the given cell ends
        /// Assumes the given cell already belongs to player(thus usable for calculating potential lengths)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public int GetNorthLineEnd(int player, int row, int column)
        {
            int highestRow = row;
            while (highestRow > 0 && _boardArray[highestRow - 1][column] == player)
            {
                highestRow--;
            }

            return highestRow;
        }

        public Location GetNorthEastLineEnd(int player, int row, int column)
        {
            var endLocation = new Location
            {
                Row = row,
                Column = column
            };
            while (endLocation.Column < ColumnCount() - 1 && endLocation.Row > 0
                && _boardArray[endLocation.Row - 1][endLocation.Column + 1] == player)
            {
                endLocation.Column++;
                endLocation.Row--;
            }

            return endLocation;
        }

        public int GetEastLineEnd(int player, int row, int column)
        {
            int rightMost = column;
            while (rightMost < ColumnCount() - 1 && _boardArray[row][rightMost + 1] == player)
            {
                rightMost++;
            }

            return rightMost;
        }

        public Location GetSouthEastLineEnd(int player, int row, int column)
        {
            var endLocation = new Location
            {
                Row = row,
                Column = column
            };
            while (endLocation.Column < ColumnCount() - 1 && endLocation.Row < RowCount() - 1
                && _boardArray[endLocation.Row + 1][endLocation.Column + 1] == player)
            {
                endLocation.Column++;
                endLocation.Row++;
            }

            return endLocation;
        }

        public int GetSouthLineEnd(int player, int row, int column)
        {
            int lowestRow = row;
            while (lowestRow + 1 < RowCount() && _boardArray[lowestRow + 1][column] == player)
            {
                lowestRow++;
            }

            return lowestRow;
        }
        
        public Location GetSouthWestLineEnd(int player, int row, int column)
        {
            var startLocation = new Location
            {
                Row = row,
                Column = column
            };
            while (startLocation.Column > 0 && startLocation.Row < RowCount() - 1
                && _boardArray[startLocation.Row + 1][startLocation.Column - 1] == player)
            {
                startLocation.Column--;
                startLocation.Row++;
            }

            return startLocation;
        }

        public int GetWestLineEnd(int player, int row, int column)
        {
            int leftMost = column;
            while (leftMost > 0 && _boardArray[row][leftMost - 1] == player)
            {
                leftMost--;
            }

            return leftMost;
        }
        
        public Location GetNorthWestLineEnd(int player, int row, int column)
        {
            var startLocation = new Location
            {
                Row = row,
                Column = column
            };
            while (startLocation.Column > 0 && startLocation.Row > 0
                && _boardArray[startLocation.Row - 1][startLocation.Column - 1] == player)
            {
                startLocation.Column--;
                startLocation.Row--;
            }

            return startLocation;
        }
        #endregion

        public int RemoveTopDisc(int column)
        {
            var rowToEmpty = GetRowOfTopDiscInColumn(column);
            _boardArray[rowToEmpty][column] = NoDisc;
            return rowToEmpty;
        }

        public int GetRowOfTopDiscInColumn(int column)
        {
            int row = LastRow();
            if(_boardArray[row][column] == NoDisc)
            {
                return RowCount();
            }
            else
            {
                while(row > 0 && _boardArray[row - 1][column] != NoDisc)
                {
                    row--; 
                }
                return row;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < RowCount(); i++)
            {
                for (int j = 0; j < ColumnCount(); j++)
                {
                    sb.Append(_boardArray[i][j]).Append(" ");
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}