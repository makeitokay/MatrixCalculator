using System;

namespace MatrixCalculator.Exceptions
{
    public class MatrixCramerMethodIsNotPossibleException : MatrixException
    {
        public MatrixCramerMethodIsNotPossibleException() {}
        
        public MatrixCramerMethodIsNotPossibleException(string message) : base(message) {}
        
        public MatrixCramerMethodIsNotPossibleException(string message, Exception inner) : base(message, inner) {}
    }
}