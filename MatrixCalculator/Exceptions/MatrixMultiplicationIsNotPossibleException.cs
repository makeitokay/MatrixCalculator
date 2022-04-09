using System;

namespace MatrixCalculator.Exceptions
{
    public class MatrixMultiplicationIsNotPossibleException : MatrixException
    {
        public MatrixMultiplicationIsNotPossibleException() {}
        
        public MatrixMultiplicationIsNotPossibleException(string message) : base(message) {}
        
        public MatrixMultiplicationIsNotPossibleException(string message, Exception inner) : base(message, inner) {}
    }
}