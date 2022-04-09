namespace MatrixCalculator
{
    // Все возможные операции для выполнения
    public enum Operation
    {
        MatrixTrace = 1,
        MatrixTranspose,
        MatrixAddition,
        MatrixDifference,
        MatrixMultiplication,
        MatrixMultiplyingByNumber,
        MatrixDeterminant,
        SystemOfAlgebraicEquations,

        MainMatrixInput,
        AdditionalMatrixInput,
        SwapMainAndAdditionalMatrix,
        PrintMainMatrix,
        PrintAdditionalMatrix,
        ChangeRandomGeneration,

        PrintOperations,

        ProgramExit
    }
}