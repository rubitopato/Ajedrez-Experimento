using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ajedrez
{
    internal abstract class Piece
    {
        public string Name { get; set; }
        public int Color { get; set; }
        protected bool RecalculateValidMoves { get; set; }
        public List<Tuple<int, int>>? ValidMoves { get; set; }
        public Tuple<int, int> Position { get; set; }
        public Image ImageControl { get; }
        protected bool hasMoved = false;


        public Piece(string name, Tuple<int, int> position, string imagePath)
        {
            this.Name = name;
            this.Color = name.StartsWith("White") ? 1 : 0;
            this.Position = position;
            this.RecalculateValidMoves = false;
            this.ImageControl = new Image
            {
                Width = 60,
                Height = 60,
                Stretch = Stretch.Uniform,
                Source = new BitmapImage(new Uri(imagePath))
            };
            //para decirle que la imagen esta manejada por esta clase, para que cuando se haga click en la imagen se pueda acceder a la pieza correspondiente
            this.ImageControl.Tag = this;
            this.ImageControl.DataContext = this;
        }

        public abstract void CalculateValidMoves(UniformGrid board);

        protected static Piece? GetPieceAt(Border border)
        {
            var img = border?.Child as Image;
            return img?.Tag as Piece;
        }

        protected List<Tuple<int, int>> GetContiniuosValidMovesInDirection(UniformGrid board, int rowDirection, int colDirection, int? limit = 10)
        {
            List<Tuple<int, int>> validMoves = new List<Tuple<int, int>>();
            int currentRow = this.Position.Item1 + rowDirection;
            int currentCol = this.Position.Item2 + colDirection;
            while (currentRow >= 0 && currentRow < board.Rows && currentCol >= 0 && currentCol < board.Columns && limit!=0)
            {
                int index = currentRow * board.Columns + currentCol;
                var border = board.Children[index] as Border;
                if (border?.Child == null)
                {
                    validMoves.Add(Tuple.Create(currentRow, currentCol));
                }
                else
                {

                    if (this is Pawn) break; //los peones no pueden capturar en linea recta, solo en diagonal

                    Piece? piece = GetPieceAt(border);
                    if (piece?.Color != this.Color)
                    {
                        validMoves.Add(Tuple.Create(currentRow, currentCol)); //pieza enemiga que se puede capturar
                    }
                    break; //despues de encontrar cualquier pieza no se puede seguir avanzando
                }
                currentRow += rowDirection;
                currentCol += colDirection;

                limit -= 1;
            }
            return validMoves;
        }
        public void Move(Tuple<int, int> NewPosition, UniformGrid board)
        {

            int LastIndex = this.Position.Item1 * board.Columns + this.Position.Item2;
            this.Position = NewPosition;
            int NewIndex = this.Position.Item1 * board.Columns + this.Position.Item2;

            if ( ((Border)board.Children[NewIndex]).Child != null)
            {
               ((Border)board.Children[NewIndex]).Child = null;
            }
            ((Border)board.Children[LastIndex]).Child = null;
            ((Border)board.Children[NewIndex]).Child = this.ImageControl;

            this.hasMoved = true;
        }

        public void CheckInvalidMoves(UniformGrid board, Piece king, string asm)
        {
            if (this.ValidMoves == null) return;
            List<Tuple<int,int>> ValidMovesAux = this.ValidMoves.ToList();
            foreach (var move in ValidMovesAux)
            {
                // Simula el movimiento
                List<Piece> boardState = BoardGenerator.CaptureBoardState(board);
                UniformGrid copyBoard = BoardGenerator.BuildGridFromState(8, 8, boardState, asm);
                var border = copyBoard.Children[this.Position.Item1 * board.Columns + this.Position.Item2] as Border;
                var img = border?.Child as Image;
                Piece p = img?.Tag as Piece;
                p.Move(move, copyBoard);
                if (KingStatusChecker.IsKingInCheck(king, copyBoard))
                {
                    this.ValidMoves.Remove(move);
                }
                
            }
        }

    }

    internal class Pawn : Piece
    {
        public Pawn(string name, Tuple<int, int> position, string imagePath) : base(name, position, imagePath) { }

        public override void CalculateValidMoves(UniformGrid board)
        {
            List<Tuple<int,int>> ValidNewPositions = new List<Tuple<int, int>>();
            int IndexOfCurrentPosition = this.Position.Item1 * board.Columns + this.Position.Item2;
            int PawnDirection = this.Color == 1 ? -1 : 1; // Si es blanco, se mueve hacia arriba

            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, PawnDirection, 0, hasMoved ? 1 : 2)); 

            if (this.Position.Item2 > 0 && ((Border)board.Children[IndexOfCurrentPosition + PawnDirection * board.Columns - 1]).Child != null)
            {
                Piece? piece = GetPieceAt((Border)board.Children[IndexOfCurrentPosition + PawnDirection * board.Columns - 1]);
                if (piece?.Color != this.Color)
                {
                    ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + PawnDirection, this.Position.Item2 - 1));
                }
            }

            if (this.Position.Item2 < board.Columns - 1 && ((Border)board.Children[IndexOfCurrentPosition + PawnDirection * board.Columns + 1]).Child != null)
            {
                Piece? piece = GetPieceAt((Border)board.Children[IndexOfCurrentPosition + PawnDirection * board.Columns + 1]);
                if (piece?.Color != this.Color)
                {
                    ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + PawnDirection, this.Position.Item2 + 1));
                }
            }

            this.ValidMoves = ValidNewPositions;
        }

        public void Promote(string newPieceType, UniformGrid board) { }

    }

    internal class Rook : Piece
    {
        public Rook(string name, Tuple<int, int> position, string imagePath) : base(name, position, imagePath) { }

        public override void CalculateValidMoves(UniformGrid board)
        {
            List<Tuple<int, int>> ValidNewPositions = new List<Tuple<int, int>>();

            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, 0)); //upwards
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, 0)); //downwards
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 0, 1)); //right
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 0, -1)); //left

            this.ValidMoves = ValidNewPositions;
        }
    }

    internal class Knight : Piece
    {
        public Knight(string name, Tuple<int, int> position, string imagePath) : base(name, position, imagePath) { }

        private List<Tuple<int, int>> GetKnightMoves()
        {
            List<Tuple<int, int>> ValidNewPositions = new List<Tuple<int, int>>();

            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 - 2, this.Position.Item2 - 1)); 
            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 - 1, this.Position.Item2 - 2));
            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 - 2, this.Position.Item2 + 1));
            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 - 1, this.Position.Item2 + 2)); 
            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + 1, this.Position.Item2 - 2)); 
            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + 2, this.Position.Item2 - 1)); 
            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + 2, this.Position.Item2 + 1)); 
            ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + 1, this.Position.Item2 + 2)); 

            return ValidNewPositions;
        }
        public override void CalculateValidMoves(UniformGrid board)
        {
            List<Tuple<int, int>> ValidNewPositions = GetKnightMoves();

            foreach (var pos in ValidNewPositions.ToArray())
            {
                if (pos.Item1 < 0 || pos.Item1 >= board.Rows || pos.Item2 < 0 || pos.Item2 >= board.Columns)
                {
                    ValidNewPositions.Remove(pos);
                }
                else if( ((Border)board.Children[pos.Item1 * board.Columns + pos.Item2]).Child != null)
                {
                    Piece? piece = GetPieceAt( (Border)board.Children[pos.Item1 * board.Columns + pos.Item2] );
                    if (piece?.Color == this.Color)
                    {
                        ValidNewPositions.Remove(pos);
                    }
                }
            }

            this.ValidMoves = ValidNewPositions;
        }
    }

    internal class Bishop : Piece
    {
        public Bishop(string name, Tuple<int, int> position, string imagePath) : base(name, position, imagePath) { }
        public override void CalculateValidMoves(UniformGrid board)
        {
            List<Tuple<int, int>> ValidNewPositions = new List<Tuple<int, int>>();

            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, -1)); //upleft
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, 1)); //upright
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, -1)); //downleft
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, 1)); //downright

            this.ValidMoves = ValidNewPositions;
        }
    }

    internal class Queen : Piece
    {
        public Queen(string name, Tuple<int, int> position, string imagePath) : base(name, position, imagePath) { }
        public override void CalculateValidMoves(UniformGrid board)
        {
            List<Tuple<int, int>> ValidNewPositions = new List<Tuple<int, int>>();

            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, 0)); //upwards
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, 0)); //downwards
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 0, 1)); //right
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 0, -1)); //left
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, -1)); //upleft
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, 1)); //upright
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, -1)); //downleft
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, 1)); //downright

            this.ValidMoves = ValidNewPositions;
        }
    }

    internal class King : Piece
    {
        public King(string name, Tuple<int, int> position, string imagePath) : base(name, position, imagePath) { }
        public override void CalculateValidMoves(UniformGrid board)
        {
            List<Tuple<int, int>> ValidNewPositions = new List<Tuple<int, int>>();

            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, 0, 1)); //upwards
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, 0, 1)); //downwards
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 0, 1, 1)); //right
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 0, -1, 1)); //left
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, -1, 1)); //upleft
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, -1, 1, 1)); //upright
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, -1, 1)); //downleft
            ValidNewPositions.AddRange(GetContiniuosValidMovesInDirection(board, 1, 1, 1)); //downright

            this.ValidMoves = ValidNewPositions;
        }

    }

    internal class PieceState
    {
        public string Name { get; init; }
        public int Color { get; init; }
        public int Row { get; init; }
        public int Col { get; init; }
        public string? ImageUri { get; init; }
    }

}

