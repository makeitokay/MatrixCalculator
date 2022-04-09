using System;

namespace MatrixCalculator.Exceptions
{
    public class MatrixSizesAreNotEqualException : MatrixException
    {
        public MatrixSizesAreNotEqualException() {}
        
        public MatrixSizesAreNotEqualException(string message) : base(message) {}
        
        public MatrixSizesAreNotEqualException(string message, Exception inner) : base(message, inner) {}
    }
}