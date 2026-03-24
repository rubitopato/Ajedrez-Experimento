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
        public bool hasMoved = false;


        public Piece(string name, Tuple<int, int> position, string imagePath)
        {
            this.Name = name;
            this.Color = name.Contains("White") ? 1 : 0;
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
        public void Move(Tuple<int, int> NewPosition, UniformGrid board, string asm, bool showPromotionUI = true)
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

            if (this is Pawn pawn)
            {
                int distance = Math.Abs(NewIndex - LastIndex);
                if (distance == 16)
                {
                    pawn.hasJustMovedTwo = true;
                }

                if (((Border)board.Children[NewIndex]).Child == null && (NewIndex - LastIndex) % 8 != 0)//comer al paso
                {
                    int direction = this.Color == 1 ? 8 : -8;
                    ((Border)board.Children[NewIndex + direction]).Child = null;
                }

                int promotionRow = this.Color == 1 ? 0 : board.Rows - 1;
                if (this.Position.Item1 == promotionRow)
                {
                    pawn.Promote(board, asm, showPromotionUI);
                }
            }

            if (this is King king)
            {
                int distance = Math.Abs(NewIndex - LastIndex);
                if (distance == 2) //castling
                {
                    if (NewIndex == 62) //castling corto blanco
                    {
                        var rook = GetPieceAt((Border)board.Children[63]) as Rook;
                        if (rook != null)
                        {
                            rook.Position = Tuple.Create(7, 5);
                            ((Border)board.Children[63]).Child = null;
                            ((Border)board.Children[61]).Child = rook.ImageControl;
                            rook.hasMoved = true;
                        }
                    }
                    else if (NewIndex == 58) //castling largo blanco
                    {
                        var rook = GetPieceAt((Border)board.Children[56]) as Rook;
                        if (rook != null)
                        {
                            rook.Position = Tuple.Create(7, 3);
                            ((Border)board.Children[56]).Child = null;
                            ((Border)board.Children[59]).Child = rook.ImageControl;
                            rook.hasMoved = true;
                        }
                    }
                    else if (NewIndex == 6) //castling negro corto
                    {
                        var rook = GetPieceAt((Border)board.Children[7]) as Rook;
                        if (rook != null)
                        {
                            rook.Position = Tuple.Create(0, 5);
                            ((Border)board.Children[7]).Child = null;
                            ((Border)board.Children[5]).Child = rook.ImageControl;
                            rook.hasMoved = true;
                        }

                    }
                    else if (NewIndex == 2) //castling negro largo
                    {
                        var rook = GetPieceAt((Border)board.Children[0]) as Rook;
                        if (rook != null)
                        {
                            rook.Position = Tuple.Create(0, 3);
                            ((Border)board.Children[0]).Child = null;
                            ((Border)board.Children[3]).Child = rook.ImageControl;
                            rook.hasMoved = true;
                        }
                    }
                }
            }

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
                // When simulating moves we don't want to show UI dialogs like promotion choices;
                // promote silently (default to Queen) in simulations.
                p.Move(move, copyBoard, asm, false);
                if (KingStatusChecker.IsKingInCheck(p is King ? p : king, copyBoard))
                {
                    this.ValidMoves.Remove(move);
                }
                
            }
        }

    }

    internal class Pawn : Piece
    {
        public bool hasJustMovedTwo { get; set; }
        public Pawn(string name, Tuple<int, int> position, string imagePath) : base(name, position, imagePath) 
        { 
            this.hasJustMovedTwo = false;
        }

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

            if ( ((Border)board.Children[IndexOfCurrentPosition + 1]).Child != null)
            {
                Piece? piece = GetPieceAt( (Border)board.Children[IndexOfCurrentPosition + 1] );
                if (piece is Pawn adjacentPawn && adjacentPawn.Color != this.Color && adjacentPawn.hasJustMovedTwo)
                {
                    ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + PawnDirection, this.Position.Item2 + 1));
                }
            }

            if (((Border)board.Children[IndexOfCurrentPosition - 1]).Child != null)
            {
                Piece? piece = GetPieceAt((Border)board.Children[IndexOfCurrentPosition - 1]);
                if (piece is Pawn adjacentPawn && adjacentPawn.Color != this.Color && adjacentPawn.hasJustMovedTwo)
                {
                    ValidNewPositions.Add(Tuple.Create(this.Position.Item1 + PawnDirection, this.Position.Item2 - 1));
                }
            }

            this.ValidMoves = ValidNewPositions;
        }

        public void Promote(UniformGrid board, string asm, bool showDialog = true)
        {

            if (!showDialog)
            {
                // Silent promotion during simulations: default to Queen
                string colorName = this.Color == 1 ? "blanco" : "negro";
                string[] parts = this.Name.Split('_');
                string index = parts.Length > 2 ? parts[2] : "0";
                var newName = $"New_{(this.Color==1?"White":"Black")}_Queen_{index}";
                var newPiece = new Queen(newName, this.Position, $"pack://application:,,,/{asm};component/Images/reina_{colorName}.png");
                int idx = this.Position.Item1 * board.Columns + this.Position.Item2;
                ((Border)board.Children[idx]).Child = newPiece.ImageControl;
                return;
            }

            var win = new PromotionWindow(this.Color, asm);
            var result = win.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(win.SelectedPieceType))
            {
                string selected = win.SelectedPieceType; // "Queen", "Rook", "Bishop", "Knight"
                string colorName = this.Color == 1 ? "blanco" : "negro";
                string[] parts = this.Name.Split('_');
                string index = parts.Length > 2 ? parts[2] : "0";
                var colorPrefix = this.Color == 1 ? "White" : "Black";
                Piece newPiece = selected switch
                {
                    "Queen" => new Queen($"{colorPrefix}_Queen_New_{index}", this.Position, $"pack://application:,,,/{asm};component/Images/reina_{colorName}.png"),
                    "Rook" => new Rook($"{colorPrefix}_Rook_New_{index}", this.Position, $"pack://application:,,,/{asm};component/Images/torre_{colorName}.png"),
                    "Bishop" => new Bishop($"{colorPrefix}_Bishop_New_{index}", this.Position, $"pack://application:,,,/{asm};component/Images/alfil_{colorName}.png"),
                    "Knight" => new Knight($"{colorPrefix}_Knight_New_{index}", this.Position, $"pack://application:,,,/{asm};component/Images/caballo_{colorName}.png"),
                    _ => new Queen($"{colorPrefix}_Queen_New_{index}", this.Position, $"pack://application:,,,/{asm};component/Images/reina_{colorName}.png"),
                };

                int idx = this.Position.Item1 * board.Columns + this.Position.Item2;
                ((Border)board.Children[idx]).Child = null;
                ((Border)board.Children[idx]).Child = newPiece.ImageControl;
            }
            return;
        }

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

            if (KingStatusChecker.IsKingInCheck(this, board)) return; //no se puede enrocar si el rey esta en jaque

            if (this.Color == 1 && !this.hasMoved) //castling blanco
            {
                Dictionary<Tuple<int, string>, List<Tuple<int, int>>> opponentsValidMoves = null;
                //castling corto
                if ( ((Border)board.Children[61]).Child == null && ((Border)board.Children[62]).Child == null )
                {
                    Piece? rook = GetPieceAt((Border)board.Children[63]);
                    if (rook is Rook r && !r.hasMoved)
                    {
                        opponentsValidMoves = KingStatusChecker.CalculateAllValidMovesForColor(0, board);
                        if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(7, 5))) && !opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(7, 6))))
                        {
                            if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(7, 6))))
                            {
                                this.ValidMoves.Add(Tuple.Create(7, 6));
                            }
                        }
                    }
                }
                //castling largo
                if (((Border)board.Children[57]).Child == null && ((Border)board.Children[58]).Child == null && ((Border)board.Children[59]).Child == null )
                {
                    Piece? rook = GetPieceAt((Border)board.Children[56]);
                    if (rook is Rook r && !r.hasMoved)
                    {
                        if (opponentsValidMoves == null) opponentsValidMoves = KingStatusChecker.CalculateAllValidMovesForColor(0, board);
                        if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(7, 3))) && !opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(7, 2))))
                        {
                            if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(7, 2))))
                            {
                                this.ValidMoves.Add(Tuple.Create(7, 2));
                            }
                        }
                    }
                }
            }
            else if(this.Color == 0 && !this.hasMoved) //castling negro
            {
                Dictionary<Tuple<int, string>, List<Tuple<int, int>>> opponentsValidMoves = null;
                //castling corto
                if (((Border)board.Children[5]).Child == null && ((Border)board.Children[6]).Child == null)
                {
                    Piece? rook = GetPieceAt((Border)board.Children[7]);
                    if (rook is Rook r && !r.hasMoved)
                    {
                        opponentsValidMoves = KingStatusChecker.CalculateAllValidMovesForColor(1, board);
                        if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(0, 5))) && !opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(0, 6))))
                        {
                            if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(0, 6))))
                            {
                                this.ValidMoves.Add(Tuple.Create(0, 6));
                            }
                        }
                    }
                }
                //castling largo
                if (((Border)board.Children[1]).Child == null && ((Border)board.Children[2]).Child == null && ((Border)board.Children[3]).Child == null)
                {
                    Piece? rook = GetPieceAt((Border)board.Children[0]);
                    if (rook is Rook r && !r.hasMoved)
                    {
                        if (opponentsValidMoves == null) opponentsValidMoves = KingStatusChecker.CalculateAllValidMovesForColor(1, board);
                        if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(0, 3))) && !opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(0, 2))))
                        {
                            if (!opponentsValidMoves.Values.Any(moves => moves.Contains(Tuple.Create(0, 2))))
                            {
                                this.ValidMoves.Add(Tuple.Create(0, 2));
                            }
                        }
                    }
                }
            }
        }

    }

}

