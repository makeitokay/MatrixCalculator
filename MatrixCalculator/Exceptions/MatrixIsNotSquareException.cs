using System;

namespace MatrixCalculator.Exceptions
{
    public class MatrixIsNotSquareException : MatrixException
    {
        public MatrixIsNotSquareException() {}
        
        public MatrixIsNotSquareException(string message) : base(message) {}
        
        public MatrixIsNotSquareException(string message, Exception inner) : base(message, inner) {}
    }
}