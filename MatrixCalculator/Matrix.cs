using System;
using MatrixCalculator.Exceptions;

namespace MatrixCalculator
{
    /// <summary>
    /// Класс матрицы; содержит все вычисления.
    /// </summary>
    public class Matrix
    {
        public int Rows, Columns;
        private decimal[,] _matrix;
        
        /// <summary>
        /// Конструктор матрицы.
        /// </summary>
        /// <param name="rows">Количество строк матрицы.</param>
        /// <param name="columns">Количество столбцов матрицы.</param>
        public Matrix(int rows = 0, int columns = 0)
        {
            Rows = rows;
            Columns = columns;
            _matrix = new decimal[Rows, Columns];
        }

        /// <summary>
        /// Индексатор для работы с значениями матрицы.
        /// </summary>
        /// <param name="rowIndex">Индекс строки.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        public decimal this[int rowIndex, int columnIndex]
        {
            get => _matrix[rowIndex, columnIndex];
            set => _matrix[rowIndex, columnIndex] = value;
        } 

        /// <summary>
        /// Сообщает, пустая ли матрица.
        /// </summary>
        /// <returns>true, если матрица пустая; иначе false.</returns>
        public bool IsEmpty() => _matrix.Length == 0;

        /// <summary>
        /// Сообщает, квадратная ли матрица.
        /// </summary>
        /// <returns>true, если матрица квадратная; иначе false.</returns>
        public bool IsSquare() => Rows == Columns;

        /// <summary>
        /// Сообщает, равны ли размеры матриц.
        /// </summary>
        /// <param name="matrix">Матрица, с которой необходимо произвести сравнение.</param>
        /// <returns>true, если размеры равны; иначе false.</returns>
        public bool SizeEquals(Matrix matrix) => Rows == matrix.Rows && Columns == matrix.Columns;

        /// <summary>
        /// Заполняет строку значениями.
        /// </summary>
        /// <param name="rowIndex">Индекс строки.</param>
        /// <param name="values">Массив значений.</param>
        public void SetRow(int rowIndex, decimal[] values)
        {
            for (var j = 0; j < Columns; j++)
                _matrix[rowIndex, j] = values[j];
        }

        /// <summary>
        /// Вычисляет след матрицы. Только для квадратных матриц.
        /// </summary>
        /// <returns>След матрицы.</returns>
        public decimal GetTrace()
        {
            if (!IsSquare())
                throw new MatrixIsNotSquareException("Вычислить след неквадратной матрицы невозможно.");
            
            decimal trace = 0;

            for (int i = 0; i < Math.Min(Rows, Columns); i++)
            {
                trace += this[i, i];
            }

            return trace;
        }

        /// <summary>
        /// Транспонирует матрицу.
        /// </summary>
        /// <returns>Транспонированная матрица.</returns>
        public Matrix Transpose()
        {
            Matrix resultMatrix = new Matrix(Columns, Rows);
            for (int j = 0; j < Columns; j++)
            {
                for (int i = 0; i < Rows; i++)
                    resultMatrix[j, i] = this[i, j];
            }

            return resultMatrix;
        }

        /// <summary>
        /// Переопределяет оператор сложения для класса матриц.
        /// </summary>
        /// <param name="firstMatrix">Первый операнд сложения.</param>
        /// <param name="secondMatrix">Второй операнд сложения.</param>
        /// <returns>Результирующая матрица.</returns>
        public static Matrix operator +(Matrix firstMatrix, Matrix secondMatrix)
        {
            if (!firstMatrix.SizeEquals(secondMatrix)) 
                throw new MatrixSizesAreNotEqualException("Матрицы должны быть одного размера.");

            Matrix resultMatrix = new Matrix(firstMatrix.Rows, firstMatrix.Columns);
            for (int i = 0; i < firstMatrix.Rows; i++)
            {
                for (int j = 0; j < firstMatrix.Columns; j++)
                    resultMatrix[i, j] = firstMatrix[i, j] + secondMatrix[i, j];
            }

            return resultMatrix;
        }
        
        /// <summary>
        /// Переопределяет оператор вычитания для класса матриц.
        /// </summary>
        /// <param name="firstMatrix">Первый операнд вычитания.</param>
        /// <param name="secondMatrix">Второй операнд вычитания.</param>
        /// <returns>Результирующая матрица.</returns>
        public static Matrix operator -(Matrix firstMatrix, Matrix secondMatrix)
        {
            if (!firstMatrix.SizeEquals(secondMatrix)) 
                throw new MatrixSizesAreNotEqualException("Матрицы должны быть одного размера.");;

            Matrix resultMatrix = new Matrix(firstMatrix.Rows, firstMatrix.Columns);
            for (int i = 0; i < firstMatrix.Rows; i++)
            {
                for (int j = 0; j < firstMatrix.Columns; j++)
                    resultMatrix[i, j] = firstMatrix[i, j] - secondMatrix[i, j];
            }

            return resultMatrix;
        }
        
        /// <summary>
        /// Переопределяет оператор умножения для класса матриц.
        /// </summary>
        /// <param name="firstMatrix">Первый операнд умножения.</param>
        /// <param name="secondMatrix">Второй операнд умножения.</param>
        /// <returns>Результирующая матрица.</returns>
        public static Matrix operator *(Matrix firstMatrix, Matrix secondMatrix)
        {
            if (firstMatrix.Columns != secondMatrix.Rows) 
                throw new MatrixMultiplicationIsNotPossibleException("Размеры матриц несовместимы с умножением.");

            Matrix resultMatrix = new Matrix(firstMatrix.Rows, secondMatrix.Columns);
            for (int i = 0; i < firstMatrix.Rows; i++)
            {
                for (int j = 0; j < secondMatrix.Columns; j++)
                {
                    resultMatrix[i, j] = 0;
                    for (int k = 0; k < secondMatrix.Rows; k++)
                        resultMatrix[i, j] += firstMatrix[i, k] * secondMatrix[k, j];
                }
            }

            return resultMatrix;
        }

        /// <summary>
        /// Переопределяет оператор умножения (на число) для класса матриц.
        /// </summary>
        /// <param name="matrix">Умножаемая матрица.</param>
        /// <param name="value">Число, на которое матрица умножается.</param>
        /// <returns>Результирующая матрица.</returns>
        public static Matrix operator *(Matrix matrix, decimal value)
        {
            Matrix resultMatrix = new Matrix(matrix.Rows, matrix.Columns);

            for (int i = 0; i < resultMatrix.Rows; i++)
            {
                for (int j = 0; j < resultMatrix.Columns; j++)
                    resultMatrix[i, j] = matrix[i, j] * value;
            }

            return resultMatrix;
        }

        /// <summary>
        /// Возвращает матрицу, необходимую для вычисления дополнительного минора (вычеркивает одну строку и столбец).
        /// </summary>
        /// <param name="rowIndex">Индекс вычеркиваемой строки.</param>
        /// <param name="columnIndex">Индекс вычеркиваемого столбца.</param>
        /// <returns>Матрица без строки `rowIndex` и без столбца `columnIndex`.</returns>
        public Matrix GetMinorMatrix(int rowIndex, int columnIndex)
        {
            Matrix resultMatrix = new Matrix(Rows - 1, Columns - 1);
            int currentRow = 0;
            int currentColumn = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (i == rowIndex || j == columnIndex)
                        continue;
                    resultMatrix[currentRow, currentColumn] = this[i, j];
                    currentColumn += 1;
                    if (currentColumn != Columns - 1) 
                        continue;
                    currentColumn = 0;
                    currentRow += 1;
                }
            }

            return resultMatrix;
        }

        /// <summary>
        /// Вычисляет определитель матрицы.
        /// </summary>
        /// <returns>Определитель матрицы.</returns>
        public decimal GetDeterminant()
        {
            if (!IsSquare())
                throw new MatrixIsNotSquareException("Определитель существует только для квадратных матриц.");

            if (Rows == 1)
                return this[0, 0];
            
            decimal determinant = 0;
            int currentSign = 1;

            for (int i = 0; i < Rows; i++)
            {
                Matrix minorMatrix = GetMinorMatrix(0, i);
                determinant += currentSign * this[0, i] * minorMatrix.GetDeterminant();
                currentSign = -currentSign;
            }

            return determinant;
        }

        /// <summary>
        /// Заменяет или удаляет столбец матрицы.
        /// </summary>
        /// <param name="columnIndex">Индекс изменяемого столбца.</param>
        /// <param name="columnValues">Массив значений, на которые необходимо заменить (необязательно).</param>
        /// <returns>Матрица с замененным или удаленным столбцом.</returns>
        public Matrix ReplaceColumn(int columnIndex, decimal[] columnValues = null)
        {
            int resultColumns = columnValues == null ? Columns - 1 : Columns;
            Matrix resultMatrix = new Matrix(Rows, resultColumns);

            for (int i = 0; i < Rows; i++)
            {
                int currentColumn = 0;
                for (int j = 0; j < Columns; j++)
                {
                    if (j != columnIndex)
                    {
                        resultMatrix[i, currentColumn] = this[i, j];
                        currentColumn += 1;
                        continue;
                    }

                    if (columnValues == null)
                        continue;
                    
                    resultMatrix[i, currentColumn] = columnValues[i];
                    currentColumn += 1;
                }
            }

            return resultMatrix;

        }

        /// <summary>
        /// Возвращает столбец матрицы.
        /// </summary>
        /// <param name="columnIndex">Индекс необходимого столбца.</param>
        /// <returns>Массив значений столбца.</returns>
        public decimal[] GetColumn(int columnIndex)
        {
            decimal[] result = new decimal[Columns];

            for (int i = 0; i < Rows; i++)
            {
                result[i] = this[i, columnIndex];
            }

            return result;
        }

        /// <summary>
        /// Решение СЛАУ по методу Крамера.
        /// </summary>
        /// <param name="solveResult">Массив, в который необходимо записать решение СЛАУ.</param>
        /// <returns>Количество решений: 0, 1 или бесконечность.</returns>
        public double CramerMethod(out decimal[] solveResult)
        {
            if (Rows != Columns - 1)
                throw new MatrixCramerMethodIsNotPossibleException(
                    "Решить такую СЛАУ методом Крамера невозможно, т.к. определителя для " + 
                    "неквадратных матриц не существует.\nМатрица должна состоять из " +
                    "n x n коэффициентов неизвестных и столбца свободных членов.");
            
            solveResult = new decimal[Rows];

            Matrix coefficientsMatrix = ReplaceColumn(Columns - 1);
            decimal mainDeterminant = coefficientsMatrix.GetDeterminant();

            decimal[] constantCoefficients = GetColumn(Columns - 1);

            for (int i = 0; i < Columns - 1; i++)
            {
                Matrix matrix = coefficientsMatrix.ReplaceColumn(i, constantCoefficients);
                decimal deltaDeterminant = matrix.GetDeterminant();
                if (mainDeterminant == 0)
                {
                    if (deltaDeterminant != 0)
                        return 0d;
                    continue;
                }
                
                solveResult[i] = Math.Round(deltaDeterminant / mainDeterminant, 2);
            }
            
            return mainDeterminant != 0 ? 1d : double.PositiveInfinity;
        }
    }
}