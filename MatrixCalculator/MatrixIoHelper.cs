using System;
using System.IO;

namespace MatrixCalculator
{
    public static class MatrixIoHelper
    {
        // Ограничение на кол-во строк и столбцов
        private const int MaximumRowsCount = 10;
        private const int MaximumColumnsCount = MaximumRowsCount;
        
        /// <summary>
        /// Спрашивает у пользователя целое число в необходимом диапазоне.
        /// </summary>
        /// <param name="minValue">Минимально допустимое значение.</param>
        /// <param name="maxValue">Максимально допустимое значение.</param>
        /// <returns>Результат ввода: число.</returns>
        public static int GetNumberInput(int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            int result;
            do
            {
                while (!int.TryParse(Console.ReadLine(), out result))
                    Console.WriteLine("Некорректный ввод: пожалуйста, введите корректное целое число.");
                if (result < minValue)
                    Console.WriteLine($"Число не должно быть меньше {minValue}.");
                if (result > maxValue)
                    Console.WriteLine($"Число не должно быть больше {maxValue}.");
            } while (!(result >= minValue && result <= maxValue));

            return result;
        }
        
        /// <summary>
        /// Обработчик ввода матрицы с консоли.
        /// </summary>
        /// <returns>Матрица, введеная с консоли.</returns>
        public static Matrix GetMatrixFromConsole()
        {
            Matrix resultMatrix = GetEmptyMatrix();

            var rows = resultMatrix.Rows;
            var columns = resultMatrix.Columns;

            Console.WriteLine($"Сейчас вам необходимо ввести {rows} строк в формате {columns} чисел, " +
                              $"разделенных через пробел.");
            Console.WriteLine("Например, ввод строки из трех элементов выглядел бы так: 1 2 3.");
            Console.WriteLine("Разделитель целой и дробной части всех чисел - ЗАПЯТАЯ!");

            for (var i = 0; i < rows; i++)
            {
                Console.Write($"Строка №{i + 1}. ");
                decimal[] row = GetMatrixRowFromConsole(columns);
                resultMatrix.SetRow(i, row);
            }

            return resultMatrix;
        }
        
        /// <summary>
        /// Обработчик ввода строки, состояющей из чисел.
        /// </summary>
        /// <param name="length">Необходимое количество чисел в строке.</param>
        /// <returns>Массив чисел, введеных в строке.</returns>
        private static decimal[] GetMatrixRowFromConsole(int length)
        {
            decimal[] result;
            Console.WriteLine($"Введите строку, состоящую из {length} чисел, разделенных через пробел:");
            while (!ParseStringOfNumbers(Console.ReadLine(), out result) || result.Length != length)
                Console.WriteLine("Пожалуйста, повторите ввод этой строки: она не соответствует требованиям.");
            return result;
        }
        
        /// <summary>
        /// Парсит строку чисел, преобразует ее в массив чисел.
        /// </summary>
        /// <param name="row">Строка чисел.</param>
        /// <param name="result">Массив, в который необходимо записать результат.</param>
        /// <returns>true или false в зависимости от успешности парсинга.</returns>
        private static bool ParseStringOfNumbers(string row, out decimal[] result)
        {
            var arrayOfRowElements = row?.Split(" ");
            if (arrayOfRowElements == null)
            {
                result = Array.Empty<decimal>();
                return false;
            }

            result = new decimal[arrayOfRowElements.Length];
            for (var i = 0; i < arrayOfRowElements.Length; i++)
            {
                if (!decimal.TryParse(arrayOfRowElements[i], out var element))
                {
                    PrintInfoMessage($"Невозможно преобразовать `{arrayOfRowElements[i]}` в число.");
                    return false;
                }

                result[i] = element;
            }

            return true;
        }
        
        /// <summary>
        /// Обработчик ввода матрицы из файла.
        /// </summary>
        /// <returns>Матрица, введеная из файла.</returns>
        public static Matrix GetMatrixFromFile()
        {
            Console.WriteLine("Ваш файл должен содержать не более 10 строк, в каждой из которых находится не более " +
                              "10 чисел, разделенных через пробел.\nНикаких других символов в файле быть не должно.");
            Console.WriteLine("Введите полный путь к файлу (или относительный, если файл лежит в папке с " +
                              "скомпилированной программой - папка net5.0):");
            
            string filePath = Console.ReadLine();
            decimal[][] matrixFromFile = Array.Empty<decimal[]>();
            do
            {
                while (!File.Exists(filePath))
                {
                    PrintInfoMessage("Файл не найден. Введите путь к файлу ещё раз.");
                    filePath = Console.ReadLine();
                }

                bool readResult;
                try
                {
                    StreamReader streamReader = new StreamReader(filePath);
                    var fileContent = streamReader.ReadToEnd();
                    readResult = GetMatrixValuesFromString(fileContent, out matrixFromFile);
                    streamReader.Close();
                }
                catch (IOException)
                {
                    PrintInfoMessage("Невозможно считать данные с этого файла.");
                    readResult = false;
                }

                if (readResult) break;
                Console.WriteLine("Исправьте файл. После исправления нажмите на любую клавишу - попытка повторится.");
                Console.ReadKey();
                Console.WriteLine();
            } while (true);

            PrintInfoMessage("Матрица успешно прочитана из файла.");
            Matrix resultMatrix = GetEmptyMatrix(matrixFromFile.Length, matrixFromFile[0].Length);
            for (var i = 0; i < resultMatrix.Rows; i++)
                resultMatrix.SetRow(i, matrixFromFile[i]);

            return resultMatrix;
        }
        
        /// <summary>
        /// Парсит строку матрицы, преобразуя ее в зубчатый массив числовых значений.
        /// </summary>
        /// <param name="values">Строка, содержащая матрицу.</param>
        /// <param name="matrixValues">Массив, в который необходимо записать результат.</param>
        /// <returns>true или false в зависимости от успешности парсинга.</returns>
        private static bool GetMatrixValuesFromString(string values, out decimal[][] matrixValues)
        {
            string[] fileRows = values.Split(Environment.NewLine);
            if (fileRows.Length == 0)
            {
                matrixValues = Array.Empty<decimal[]>();
                PrintInfoMessage("На вход поступила пустая строка.");
                return false;
            }

            if (fileRows.Length > MaximumRowsCount)
            {
                matrixValues = Array.Empty<decimal[]>();
                PrintInfoMessage($"Количество строк в файле не должно превышать {MaximumRowsCount}.");
                return false;
            }

            matrixValues = new decimal[fileRows.Length][];
            int columns = fileRows[0].Split(" ").Length;
            for (var i = 0; i < fileRows.Length; i++)
            {
                if (!ParseStringOfNumbers(fileRows[i], out matrixValues[i]))
                    return false;

                if (matrixValues[i].Length != columns)
                {
                    PrintInfoMessage($"Строка №1 и строка №{i + 1} не совпадают по размеру " +
                                     "(разное количество чисел в строках).");
                    return false;
                }

                if (matrixValues[i].Length > MaximumColumnsCount)
                {
                    PrintInfoMessage($"Количество столбцов в файле не должно превышать {MaximumColumnsCount}.");
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Спрашивает у пользователя, если это необходимо, кол-во строк и столбцов и возвращает пустую матрицу с
        /// указанными параметрами.
        /// </summary>
        /// <param name="rows">Количество строк (необязательно).</param>
        /// <param name="columns">Количество столбцов (необязательно).</param>
        /// <returns>Пустая матрица, имеющая `rows` строк и `columns` столбцов.</returns>
        private static Matrix GetEmptyMatrix(int rows = 0, int columns = 0)
        {
            if (rows != 0)
                Console.WriteLine($"Число строк: {rows}");
            else
            {
                Console.WriteLine("Введите число строк матрицы (не более 10):");
                rows = GetNumberInput(1, MaximumRowsCount);
            }

            if (columns != 0)
                Console.WriteLine($"Число столбцов: {columns}");
            else
            {
                Console.WriteLine("Введите число столбцов матрицы (не более 10):");
                columns = GetNumberInput(1, MaximumColumnsCount);
            }

            return new Matrix(rows, columns);
        }

        /// <summary>
        /// Обработчик генерации рандомной матрицы.
        /// </summary>
        /// <returns>Рандомно сгенерированная матрица.</returns>
        public static Matrix GetRandomMatrix()
        {
            Random randomInstance = new Random();

            var resultMatrix = GetEmptyMatrix();
            for (int i = 0; i < resultMatrix.Rows; i++)
            {
                decimal[] row = new decimal[resultMatrix.Columns];
                for (int j = 0; j < resultMatrix.Columns; j++)
                {
                    bool isFractional = Program.IsFractionalUsed && randomInstance.NextDouble() > 0.5d;
                    if (isFractional)
                        row[j] = (decimal) Math.Round(
                            randomInstance.NextDouble() * (Program.MaxRandomValue - Program.MinRandomValue)
                                                      + Program.MinRandomValue, 2);
                    else
                        row[j] = randomInstance.Next(Program.MinRandomValue, Program.MaxRandomValue);
                }

                resultMatrix.SetRow(i, row);
            }
            
            PrintMatrix(resultMatrix, "Матрица была успешно сгенерирована.");

            return resultMatrix;
        }
        
        /// <summary>
        /// Метод, позволяющий пользователю перезаписать результат какой-либо операции вместо основной или
        /// дополнительной матрицы.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        /// <param name="resultMatrix">Ссылка на матрицу, которая должна быть записана.</param>
        public static void AskAboutMatrixOverwrite(ref Matrix mainMatrix, ref Matrix additionalMatrix,
            ref Matrix resultMatrix)
        {
            Console.WriteLine("Вы можете использовать матрицу, полученную в результате выполнения операции, " +
                              "в качестве основной или дополнительной. Выберите нужный пункт меню:\n" +
                              "1. Записать результат вместо основной матрицы.\n" +
                              "2. Записать результат вместо дополнительной матрицы.\n" +
                              "3. Не сохранять результат и вернуться в главное меню.");
            int userChoiceInput = GetNumberInput(1, 3);
            switch (userChoiceInput)
            {
                case 1:
                    mainMatrix = resultMatrix;
                    PrintInfoMessage("Вместо основной матрицы был записан результат последней выполненной операции.");
                    break;
                case 2:
                    additionalMatrix = resultMatrix;
                    PrintInfoMessage("Вместо дополнительной матрицы был записан результат " +
                                     "последней выполненной операции.");
                    break;
                case 3:
                    PrintInfoMessage("Вы в главном меню.");
                    break;
            }
        }
        
        /// <summary>
        /// Печатает матрицу с дополнительным сообщением.
        /// </summary>
        /// <param name="matrix">Матрица, которую нужно напечатать</param>
        /// <param name="infoMessage">Дополнительное сообщение</param>
        public static void PrintMatrix(Matrix matrix, string infoMessage = "")
        {
            if (matrix.IsEmpty())
            {
                PrintInfoMessage("Матрица пустая!");
                return;
            }

            Console.WriteLine("======");
            if (infoMessage != "")
                Console.WriteLine(infoMessage);
            for (var i = 0; i < matrix.Rows; i++)
            {
                for (var j = 0; j < matrix.Columns; j++)
                    Console.Write(matrix[i, j] + (j == matrix.Columns - 1 ? "\n" : "\t"));
            }

            Console.WriteLine("======");
        }

        /// <summary>
        /// Печатает информационное сообщение.
        /// </summary>
        /// <param name="infoMessage">Сообщение, которое нужно напечатать.</param>
        public static void PrintInfoMessage(String infoMessage)
        {
            Console.WriteLine("=====");
            Console.WriteLine(infoMessage);
            Console.WriteLine("=====");
        }
    }
}