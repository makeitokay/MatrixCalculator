using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using MatrixCalculator.Exceptions;

namespace MatrixCalculator
{
    /// <summary>
    /// Главный класс программы, осуществляющий всю логику ввода/вывода данных
    /// </summary>
    class Program
    {
        private static readonly List<Operation> RequiringMainMatrix = new()
        {
            Operation.MatrixTrace,
            Operation.MatrixTranspose,
            Operation.MatrixAddition,
            Operation.MatrixDifference,
            Operation.MatrixMultiplication,
            Operation.MatrixMultiplyingByNumber,
            Operation.MatrixDeterminant,
            Operation.SystemOfAlgebraicEquations,
            Operation.SwapMainAndAdditionalMatrix
        };
        
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

        private const string IntroductionText = 
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
                
                HandleOperation(userOperationInput, ref mainMatrix, ref additionalMatrix);
                
            } while (userOperationInput != Operation.ProgramExit);
        }

        /// <summary>
        /// Обработка операций.
        /// </summary>
        /// <param name="operation">Операция, которую необходимо обработать.</param>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        /// <param name="additionalMatrix">Ссылка на дополнительную матрицу.</param>
        static void HandleOperation(Operation operation, ref Matrix mainMatrix, ref Matrix additionalMatrix)
        {
            // TODO: вызывать AskAboutMatrixOverwrite() в этом методе (избавиться от повторяющегося кода)
            try
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
                        MatrixIoHelper.PrintMatrix(mainMatrix);
                        break;
                    case Operation.PrintAdditionalMatrix:
                        MatrixIoHelper.PrintMatrix(additionalMatrix);
                        break;
                    case Operation.SwapMainAndAdditionalMatrix:
                        (mainMatrix, additionalMatrix) = (additionalMatrix, mainMatrix);
                        MatrixIoHelper.PrintInfoMessage("Успешно! Основная и дополнительная матрицы поменялись местами.");
                        break;
                    case Operation.PrintOperations:
                        PrintOperations();
                        break;
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
                    default:
                        return;
                }
            }
            catch (OverflowException)
            {
                MatrixIoHelper.PrintInfoMessage("Вы работаете со слишком большими числами: вычислить такое программа пока не может.");
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
                MatrixIoHelper.PrintInfoMessage($"След матрицы = {mainMatrix.GetTrace()}");
            }
            catch (MatrixIsNotSquareException exception)
            {
                MatrixIoHelper.PrintInfoMessage(exception.Message);
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
            MatrixIoHelper.PrintMatrix(transposedMatrix, "Результат выполнения: транспонированная матрица");
            
            // Дополнительная матрица была нужна, чтобы была возможность записать в нее результат операции
            MatrixIoHelper.AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref transposedMatrix);
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
                MatrixIoHelper.PrintInfoMessage(exception.Message);
                return;
            }
            MatrixIoHelper.PrintMatrix(resultOfSum, "Результат выполнения: сумма основной и дополнительной матриц");
            MatrixIoHelper.AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfSum);
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
                MatrixIoHelper.PrintInfoMessage(exception.Message);
                return;
            }
            MatrixIoHelper.PrintMatrix(resultOfDifference, "Результат выполнения: разность основной и дополнительной матриц");
            MatrixIoHelper.AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfDifference);
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
                MatrixIoHelper.PrintInfoMessage(exception.Message);
                return;
            }
            MatrixIoHelper.PrintMatrix(resultOfMultiplication, "Результат выполнения: " +
                                                               "произведение основной и дополнительной матриц");
            MatrixIoHelper.AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfMultiplication);
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
            MatrixIoHelper.PrintMatrix(resultOfMultiply, "Результат выполнения: " + 
                                                         $"произведение основной матрицы и числа {value}");
            MatrixIoHelper.AskAboutMatrixOverwrite(ref mainMatrix, ref additionalMatrix, ref resultOfMultiply);
        }

        /// <summary>
        /// Обработчик операции вычисления определителя матрицы.
        /// </summary>
        /// <param name="mainMatrix">Ссылка на основную матрицу.</param>
        private static void HandleMatrixDeterminant(ref Matrix mainMatrix)
        {
            try
            {
                MatrixIoHelper.PrintInfoMessage($"Определитель матрицы = {mainMatrix.GetDeterminant()}");
            }
            catch (MatrixIsNotSquareException exception)
            {
                MatrixIoHelper.PrintInfoMessage(exception.Message);
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
                MatrixIoHelper.PrintInfoMessage(exception.Message);
                return;
            }
            if (solveCount == 0)
            {
                MatrixIoHelper.PrintInfoMessage("Данная система не имеет решений.");
                return;
            }
            if (double.IsPositiveInfinity(solveCount))
            {
                MatrixIoHelper.PrintInfoMessage("Данная система имеет бесконечно много решений.");
                return;
            }
            
            string resultText = "";
            for (int i = 0; i < solveResult.Length; i++)
                resultText += $"x{i + 1} = {solveResult[i]};" + (i != solveResult.Length - 1 ? "\n" : "");
            
            MatrixIoHelper.PrintInfoMessage($"Результат выполнения: решение системы линейных алгебраических уравнений\n{resultText}");
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
            int userChoiceInput = MatrixIoHelper.GetNumberInput(1, 3);
            switch (userChoiceInput)
            {
                case 1:
                    Console.WriteLine("Введите новое минимально генерируемое значение:");
                    MinRandomValue = MatrixIoHelper.GetNumberInput(maxValue: MaxRandomValue - 1);
                    MatrixIoHelper.PrintInfoMessage($"Изменения сохранены. Новое значение: {MinRandomValue}");
                    break;
                case 2:
                    Console.WriteLine("Введите новое максимально (НЕ ВКЛЮЧИТЕЛЬНО) генерируемое значение:");
                    MaxRandomValue = MatrixIoHelper.GetNumberInput(minValue: MinRandomValue + 1);
                    MatrixIoHelper.PrintInfoMessage($"Изменения сохранены. Новое значение: {MaxRandomValue}");
                    break;
                case 3:
                    IsFractionalUsed = !IsFractionalUsed;
                    MatrixIoHelper.PrintInfoMessage("Изменения сохранены. " +
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
            int userChoiceInput = MatrixIoHelper.GetNumberInput(1, 3);
            matrix = userChoiceInput switch
            {
                1 => MatrixIoHelper.GetMatrixFromConsole(),
                2 => MatrixIoHelper.GetMatrixFromFile(),
                3 => MatrixIoHelper.GetRandomMatrix(),
                _ => new Matrix()
            };
        }

        /// <summary>
        /// Печатает на экран список имеющихся операций.
        /// </summary>
        static void PrintOperations()
        {
            MatrixIoHelper.PrintInfoMessage("Список доступных операций (напишите только НОМЕР нужного пункта):");
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
        /// Обработчик ввода операции.
        /// </summary>
        /// <returns>Операция, которую необходимо выполнить.</returns>
        static Operation GetUserOperationInput()
        {
            int operationsLength = Enum.GetNames(typeof(Operation)).Length;

            Console.WriteLine("Введите номер нужного пункта главного меню " +
                              $"({(int) Operation.PrintOperations}, чтобы вывести список команд):");
            int userNumberInput = MatrixIoHelper.GetNumberInput(1, operationsLength);
            return (Operation) userNumberInput;
        }
    }
}