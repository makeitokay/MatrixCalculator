using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using MatrixCalculator.Exceptions;

namespace MatrixCalculator
{
    /// <summary>
    /// Главный класс программы, осуществляющий всю логику ввода/вывода данных
    /// </summary>
    class Program
    {
        // Разбиение операций на категории
        private static readonly List<Operation> CalculationOperations = new()
        {
            Operation.MatrixTrace,
            Operation.MatrixTranspose,
            Operation.MatrixAddition,
            Operation.MatrixDifference,
            Operation.MatrixMultiplication,
            Operation.MatrixMultiplyingByNumber,
            Operation.MatrixDeterminant,
            Operation.SystemOfAlgebraicEquations,
        };
        
        private static readonly List<Operation> RequiringMainMatrix = 
            CalculationOperations.Concat(new []{ Operation.SwapMainAndAdditionalMatrix }).ToList();

        private static readonly List<Operation> RequiringAdditionalMatrix = new()
        {
            Operation.MatrixAddition,
            Operation.MatrixDifference,
            Operation.MatrixMultiplication,
            Operation.SwapMainAndAdditionalMatrix
        };

        // Параметры для рандомной генерации чисел
        public static int MinRandomValue = -100;
        public static int MaxRandomValue = 100;
        public static bool IsFractionalUsed = false;

        // Ограничение на кол-во строк и столбцов
        public static int MaximumRowsCount = 10;
        public static int MaximumColumnsCount = MaximumRowsCount;

        public const string IntroductionText = 
            "=== Калькулятор матриц (beta v0.0.1) ===\n" + 
            "Программа может хранить две матрицы - основную (которую можно использовать для " +
            "выполнения унарных операций по типу нахождения определителя и т.д.) и дополнительную " +
            "(которая используется СПРАВА (!) в операциях умножения, сложения, разности матриц).\n" +
            "Вы можете вводить матрицы самостоятельно через консоль, через файл ИЛИ программа может" +
            " сама сгенерировать матрицу нужного вам размера.\n" +
            "Есть стандартные параметры для генерации чисел: минимальное значение -100, " +
            "максимальное 100 (не вкл.); дробные числа НЕ генерируются. Вы сможете изменить эти параметры.\n" +
            "Если вы вводите слишком большое или слишком маленькое число при вводе матрицы и " +
            "программа не обрабатывает его, попробуйте сузить диапазон вводимых значений " +
            "(размеры ограничены типом decimal, а именно от -79228162514264337593543950335 до " +
            "79228162514264337593543950335).\n" +
            "Разделитель целой и дробной части всех чисел - ЗАПЯТАЯ!\n" +
            "В процессе работы с программой вам будет предложено некоторое меню действий, которые " +
            "можно осуществить. Чтобы выбрать действие, напишите НОМЕР нужного пункта меню.\n";

        /// <summary>
        /// Входная точка в программу, главный цикл ввода операции и ее последующей обработки.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Нужно, чтобы разделитель целой и дробной части был запятой
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
            
            Console.Write(IntroductionText);   
            
            // Хранение основной и дополнительной матриц в этих переменных.
            Matrix mainMatrix = new Matrix();
            Matrix additionalMatrix = new Matrix();

            Operation userOperationInput;
            PrintOperations();
            do
            {
                userOperationInput = GetUserOperationInput();
                
                // Если операция требует матрицу, ее необходимо сначала ввести.
                if (RequiringMainMatrix.Contains(userOperationInput) && mainMatrix.IsEmpty())
                {
                    Console.WriteLine("Для выполнения этой операции необходимо ввести основную матрицу.");
                    GetMatrixInput(out mainMatrix);
                }

                if (RequiringAdditionalMatrix.Contains(userOperationInput) && additionalMatrix.IsEmpty())
                {
                    Console.WriteLine("Для выполнения этой операции необходимо ввести дополнительную матрицу.");
                    GetMatrixInput(out additionalMatrix);
                }
                
                // Отдельно обрабатываются операции, требующие вычислений.
                if (CalculationOperations.Contains(userOperationInput))
                    HandleCalculateOperation(userOperationInput, ref mainMatrix, ref additionalMatrix);
                else
                    HandleNonCalculateOperation(userOperationInput, ref mainMatrix, ref additionalMatrix);
                
            } while (userOperationInput != Operation.ProgramExit);
        }

        /// <summary>
        /// Обработка операций, не требующих вычислений.
        /// </summary>
        /// <param name="operation">Операция, которую необходимо обработать.</param>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        static void HandleNonCalculateOperation(Operation operation, ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            switch (operation)
            {
                case Operation.ChangeRandomGeneration:
                    RandomGenerationInput();
                    break;
                case Operation.MainMatrixInput:
                    GetMatrixInput(out mainMatrix);
                    break;
                case Operation.AdditionalMatrixInput:
                    GetMatrixInput(out additionalMatrix);
                    break;
                case Operation.PrintMainMatrix:
                    PrintMatrix(mainMatrix);
                    break;
                case Operation.PrintAdditionalMatrix:
                    PrintMatrix(additionalMatrix);
                    break;
                case Operation.SwapMainAndAdditionalMatrix:
                    (mainMatrix, additionalMatrix) = (additionalMatrix, mainMatrix);
                    PrintInfoMessage("Успешно! Основная и дополнительная матрицы поменялись местами.");
                    break;
                case Operation.PrintOperations:
                    PrintOperations();
                    break;
            }
        }

        /// <summary>
        /// Обработка операций, требующих вычислений.
        /// </summary>
        /// <param name="operation">Операция, которую необходимо обработать.</param>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        static void HandleCalculateOperation(Operation operation, ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            // try-catch необходим, чтобы не возникло ошибки переполнения при вычислениях.
            try
            {
                switch (operation)
                {
                    case Operation.MatrixTrace:
                        HandleMatrixTrace(ref mainMatrix);
                        break;
                    case Operation.MatrixTranspose:
                        HandleMatrixTranspose(ref mainMatrix, ref additionalMatrix);
                        break;
                    case Operation.MatrixAddition:
                        HandleMatrixAddition(ref mainMatrix, ref additionalMatrix);
                        break;
                    case Operation.MatrixDifference:
                        HandleMatrixDifference(ref mainMatrix, ref additionalMatrix);
                        break;
                    case Operation.MatrixMultiplication:
                        HandleMatrixMultiplication(ref mainMatrix, ref additionalMatrix);
                        break;
                    case Operation.MatrixMultiplyingByNumber:
                        HandleMatrixMultiplyingByNumber(ref mainMatrix, ref additionalMatrix);
                        break;
                    case Operation.MatrixDeterminant:
                        HandleMatrixDeterminant(ref mainMatrix);
                        break;
                    case Operation.SystemOfAlgebraicEquations:
                        HandleSystemOfAlgebraicEquations(ref mainMatrix);
                        break;
                }
            }
            catch (OverflowException)
            {
                PrintInfoMessage("Вы работаете со слишком большими числами: вычислить такое программа пока не может.");
            }
        }

        /// <summary>
        /// Обработчик операции вычисления следа матрицы.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        private static void HandleMatrixTrace(ref Matrix mainMatrix)
        {
            try
            {
                PrintInfoMessage($"След матрицы = {mainMatrix.GetTrace()}");
            }
            catch (MatrixIsNotSquareException exception)
            {
                PrintInfoMessage(exception.Message);
            }
        }

        /// <summary>
        /// Обработчик операции транспонирования матрицы.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        private static void HandleMatrixTranspose(ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            Matrix transposedMatrix = mainMatrix.Transpose();
            PrintMatrix(transposedMatrix, "Результат выполнения: транспонированная матрица");
            
            // Дополнительная матрица была нужна, чтобы была возможность записать в нее результат операции
            AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref transposedMatrix);
        }

        /// <summary>
        /// Обработчик операции сложения матриц.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        private static void HandleMatrixAddition(ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            Matrix resultOfSum;
            try
            {
                resultOfSum = mainMatrix + additionalMatrix;
            }
            catch (MatrixSizesAreNotEqualException exception)
            {
                PrintInfoMessage(exception.Message);
                return;
            }
            PrintMatrix(resultOfSum, "Результат выполнения: сумма основной и дополнительной матриц");
            AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfSum);
        }

        /// <summary>
        /// Обработчик операции вычитания матриц.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        private static void HandleMatrixDifference(ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            Matrix resultOfDifference;
            try
            {
                resultOfDifference = mainMatrix - additionalMatrix;
            }
            catch (MatrixSizesAreNotEqualException exception)
            {
                PrintInfoMessage(exception.Message);
                return;
            }
            PrintMatrix(resultOfDifference, "Результат выполнения: разность основной и дополнительной матриц");
            AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfDifference);
        }

        /// <summary>
        /// Обработчик операции умножения матриц.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        private static void HandleMatrixMultiplication(ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            Matrix resultOfMultiplication;
            try
            {
                resultOfMultiplication = mainMatrix * additionalMatrix;
            }
            catch (MatrixMultiplicationIsNotPossibleException exception)
            {
                PrintInfoMessage(exception.Message);
                return;
            }
            PrintMatrix(resultOfMultiplication, "Результат выполнения: " +
                                                "произведение основной и дополнительной матриц");
            AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfMultiplication);
        }

        /// <summary>
        /// Обработчик операции умножения матрицы на число.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        private static void HandleMatrixMultiplyingByNumber(ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            Console.WriteLine("Введите число, на которое нужно умножить матрицу: ");
            decimal value;
            while (!decimal.TryParse(Console.ReadLine(), out value))
                Console.WriteLine("Некорректный ввод: попробуйте ввести еще раз.");
            Matrix resultOfMultiply = mainMatrix * value;
            PrintMatrix(resultOfMultiply, "Результат выполнения: " + 
                                          $"произведение основной матрицы и числа {value}");
            AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfMultiply);
        }

        /// <summary>
        /// Обработчик операции вычисления определителя матрицы.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        private static void HandleMatrixDeterminant(ref Matrix mainMatrix)
        {
            try
            {
                PrintInfoMessage($"Определитель матрицы = {mainMatrix.GetDeterminant()}");
            }
            catch (MatrixIsNotSquareException exception)
            {
                PrintInfoMessage(exception.Message);
            }
        }

        /// <summary>
        /// Обработчик операции решения СЛАУ методом Крамера.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        private static void HandleSystemOfAlgebraicEquations(ref Matrix mainMatrix)
        {
            double solveCount;
            decimal[] solveResult;
            try
            {
                solveCount = mainMatrix.CramerMethod(out solveResult);
            }
            catch (MatrixCramerMethodIsNotPossibleException exception)
            {
                PrintInfoMessage(exception.Message);
                return;
            }
            if (solveCount == 0)
            {
                PrintInfoMessage("Данная система не имеет решений.");
                return;
            }
            if (double.IsPositiveInfinity(solveCount))
            {
                PrintInfoMessage("Данная система имеет бесконечно много решений.");
                return;
            }
            
            string resultText = "";
            for (int i = 0; i < solveResult.Length; i++)
                resultText += $"x{i + 1} = {solveResult[i]};" + (i != solveResult.Length - 1 ? "\n" : "");
            
            PrintInfoMessage($"Результат выполнения: решение системы линейных алгебраических уравнений\n{resultText}");
        }

        /// <summary>
        /// Метод, позволяющий пользователю перезаписать результат какой-либо операции вместо основной или
        /// дополнительной матрицы.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        /// <param name="resultMatrix">Ссылка на матрицу, которая должна быть записана.</param>
        private static void AskAboutMatrixOverwrite(ref Matrix mainMatrix, ref Matrix additionalMatrix,
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
        /// Обработчик изменения параметров рандомной генерации чисел.
        /// </summary>
        static void RandomGenerationInput()
        {
            Console.WriteLine("=== Изменение параметров рандомной генерации чисел ===");
            Console.WriteLine("Выберите номер нужного пункта:\n" +
                              $"1. Изменить минимальное значение элементов матрицы (сейчас {MinRandomValue})\n" +
                              $"2. Изменить максимальное значение элементов матрицы (сейчас {MaxRandomValue})\n" +
                              "3. Выберите этот пункт, чтобы изменить разрешение генерировать дробные числа на " +
                              $"противоположное (сейчас {IsFractionalUsedString()}).");
            Console.WriteLine("Учтите, что выбрать минимальное и максимальное значение вы можете только в диапазоне " +
                              "от -2147483648 до 2147483647.");
            Console.WriteLine("Также не забывайте, что минимум не может быть больше максимума и максимум не может " +
                              "быть меньше минимума! Программа, в случае некорректного ввода, об этом сообщит.");
            int userChoiceInput = GetNumberInput(1, 3);
            switch (userChoiceInput)
            {
                case 1:
                    Console.WriteLine("Введите новое минимально генерируемое значение:");
                    MinRandomValue = GetNumberInput(maxValue: MaxRandomValue - 1);
                    PrintInfoMessage($"Изменения сохранены. Новое значение: {MinRandomValue}");
                    break;
                case 2:
                    Console.WriteLine("Введите новое максимально (НЕ ВКЛЮЧИТЕЛЬНО) генерируемое значение:");
                    MaxRandomValue = GetNumberInput(minValue: MinRandomValue + 1);
                    PrintInfoMessage($"Изменения сохранены. Новое значение: {MaxRandomValue}");
                    break;
                case 3:
                    IsFractionalUsed = !IsFractionalUsed;
                    PrintInfoMessage($"Изменения сохранены. " +
                                     $"Теперь генерировать дробные значения {IsFractionalUsedString()}");
                    break;
            }
        }

        /// <summary>
        /// Переводит bool-значение разрешения использовать дробные числа в строку.
        /// </summary>
        /// <returns>Строка "разрешено"/"запрещено" в зависимости от `IsFractionalUsed`.</returns>
        static string IsFractionalUsedString() => IsFractionalUsed ? "разрешено" : "запрещено";

        /// <summary>
        /// Главный метод для обработки ввода матрицы; спрашивает у пользователя способ ввода.
        /// </summary>
        /// <param name="matrix">Матрица, в которую будет записан результат ввода.</param>
        static void GetMatrixInput(out Matrix matrix)
        {
            Console.WriteLine("=== Ввод матрицы ===");
            Console.WriteLine("Выберите номер нужного пункта:\n" +
                              "1. Ввод матрицы через консоль\n" +
                              "2. Ввод матрицы из файла\n" +
                              "3. Рандомная генерация матрицы");
            int userChoiceInput = GetNumberInput(1, 3);
            matrix = userChoiceInput switch
            {
                1 => GetMatrixFromConsole(),
                2 => GetMatrixFromFile(),
                3 => GetRandomMatrix(),
                _ => new Matrix()
            };
        }

        /// <summary>
        /// Спрашивает у пользователя, если это необходимо, кол-во строк и столбцов и возвращает пустую матрицу с
        /// указанными параметрами.
        /// </summary>
        /// <param name="rows">Количество строк (необязательно).</param>
        /// <param name="columns">Количество столбцов (необязательно).</param>
        /// <returns>Пустая матрица, имеющая `rows` строк и `columns` столбцов.</returns>
        static Matrix GetEmptyMatrix(int rows = 0, int columns = 0)
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
        /// Обработчик ввода матрицы с консоли.
        /// </summary>
        /// <returns>Матрица, введеная с консоли.</returns>
        static Matrix GetMatrixFromConsole()
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
        static decimal[] GetMatrixRowFromConsole(int length)
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
        static bool ParseStringOfNumbers(string? row, out decimal[] result)
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
        static Matrix GetMatrixFromFile()
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
        static bool GetMatrixValuesFromString(string values, out decimal[][] matrixValues)
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
        /// Обработчик генерации рандомной матрицы.
        /// </summary>
        /// <returns>Рандомно сгенерированная матрица.</returns>
        static Matrix GetRandomMatrix()
        {
            Random randomInstance = new Random();

            var resultMatrix = GetEmptyMatrix();
            for (int i = 0; i < resultMatrix.Rows; i++)
            {
                decimal[] row = new decimal[resultMatrix.Columns];
                for (int j = 0; j < resultMatrix.Columns; j++)
                {
                    bool isFractional = IsFractionalUsed && randomInstance.NextDouble() > 0.5d;
                    if (isFractional)
                        row[j] = (decimal) Math.Round(randomInstance.NextDouble() * (MaxRandomValue - MinRandomValue)
                                                      + MinRandomValue, 2);
                    else
                        row[j] = randomInstance.Next(MinRandomValue, MaxRandomValue);
                }

                resultMatrix.SetRow(i, row);
            }
            
            PrintMatrix(resultMatrix, "Матрица была успешно сгенерирована.");

            return resultMatrix;
        }

        /// <summary>
        /// Спрашивает у пользователя целое число в необходимом диапазоне.
        /// </summary>
        /// <param name="minValue">Минимально допустимое значение.</param>
        /// <param name="maxValue">Максимально допустимое значение.</param>
        /// <returns>Результат ввода: число.</returns>
        static int GetNumberInput(int minValue = int.MinValue, int maxValue = int.MaxValue)
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
        /// Печатает на экран список имеющихся операций.
        /// </summary>
        static void PrintOperations()
        {
            PrintInfoMessage("Список доступных операций (напишите только НОМЕР нужного пункта):");
            Console.WriteLine("1. Вычислить след матрицы\n" +
                              "2. Транспонировать матрицу\n" +
                              "3. Добавить к матрице другую матрицу\n" +
                              "4. Вычесть из матрицы другую матрицу\n" +
                              "5. Умножить матрицу на другую матрицу\n" +
                              "6. Умножить матрицу на число\n" +
                              "7. Вычислить определитель матрицы\n" +
                              "8. Решить систему линейных алгебраических уравнений МЕТОДОМ КРАМЕРА\n\n" +
                              "9. Ввести основную матрицу\n" +
                              "10. Ввести дополнительную матрицу\n" +
                              "11. Поменять основную и дополнительную матрицу местами\n" +
                              "12. Вывести на экран основную матрицу\n" +
                              "13. Вывести на экран дополнительную матрицу\n" +
                              "14. Изменить параметры генерации рандомных чисел\n\n" +
                              "15. ВЫВЕСТИ НА ЭКРАН СПИСОК ОПЕРАЦИЙ\n\n" +
                              "16. Завершить работу программы");
        }

        /// <summary>
        /// Печатает матрицу с дополнительным сообщением.
        /// </summary>
        /// <param name="matrix">Матрица, которую нужно напечатать</param>
        /// <param name="infoMessage">Дополнительное сообщение</param>
        public static void PrintMatrix(Matrix matrix, String infoMessage = "")
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
        private static void PrintInfoMessage(String infoMessage)
        {
            Console.WriteLine("=====");
            Console.WriteLine(infoMessage);
            Console.WriteLine("=====");
        }

        /// <summary>
        /// Обработчик ввода операции.
        /// </summary>
        /// <returns>Операция, которую необходимо выполнить.</returns>
        static Operation GetUserOperationInput()
        {
            int operationsLength = Enum.GetNames(typeof(Operation)).Length;

            Console.WriteLine("Введите номер нужного пункта главного меню " +
                              $"({(int) Operation.PrintOperations}, чтобы вывести список команд):");
            int userNumberInput = GetNumberInput(1, operationsLength);
            return (Operation) userNumberInput;
        }
    }
}