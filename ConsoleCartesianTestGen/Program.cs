using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestGenerator
{
    public class TupleWrapperInt : IEquatable<TupleWrapperInt>
    {
        public int[] Values { get; }

        public TupleWrapperInt(int[] values)
        {
            Values = values;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TupleWrapperInt);
        }

        public bool Equals(TupleWrapperInt other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (Values.Length != other?.Values.Length) return false;
            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i] != other.Values[i]) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Values.Length;
                for (int i = 0; i < Values.Length; i++)
                {
                    hash ^= Values[i].GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Join(" ", Values);
        }
    }

    public class Program
    {
        static Random random = new Random();
        //Функция для подсчёта количества элементов в декартовом произведении
        static long CartesianProductCount(int columnCount, int maxValuesPerColumn)
        {
            long result = 1;
            for (int i = 0; i < columnCount; i++)
            {
                result *= maxValuesPerColumn;
            }
            return result;
        }
        static List<List<TupleWrapperInt>> GenerateRandomTable(int sizeCategory, int columnCount, int maxValuesPerColumn)
        {
            Random random = new Random();            
            int rowsToRemove;

            switch (sizeCategory)
            {
                case 1: // Малая размерность (2-3 столбца, 3-6 строк)
                    columnCount = random.Next(2, 4);
                    maxValuesPerColumn = random.Next(3, 7);
                    rowsToRemove = random.Next(Math.Max(0, (int)(CartesianProductCount(columnCount, maxValuesPerColumn) * 0.2) - 1), (int)(CartesianProductCount(columnCount, maxValuesPerColumn) * 0.5)); // Удаляем от 20% до 50% строк
                    break;
                case 2: // Средний объем данных (3-4 столбца, 6-10 строк)
                    columnCount = random.Next(3, 5);
                    maxValuesPerColumn = random.Next(6, 11);
                    rowsToRemove = random.Next(Math.Max(0, (int)(CartesianProductCount(columnCount, maxValuesPerColumn) * 0.2) - 1), (int)(CartesianProductCount(columnCount, maxValuesPerColumn) * 0.5)); // Удаляем от 20% до 50% строк
                    break;
                case 3: // Большой объем данных (до 4 столбцов и 12 строк)
                    columnCount = random.Next(2, 5);
                    maxValuesPerColumn = Math.Min(12, random.Next(8, 13)); // Максимум 12 строк
                    rowsToRemove = random.Next(Math.Max(0, (int)(CartesianProductCount(columnCount, maxValuesPerColumn) * 0.2) - 1), (int)(CartesianProductCount(columnCount, maxValuesPerColumn) * 0.5)); // Удаляем от 20% до 50% строк
                    break;
                default:
                    throw new ArgumentException("sizeCategory должно быть 1, 2 или 3");
            }
            List<HashSet<int>> columnValues = Enumerable.Range(0, columnCount)
                .Select(i => GenerateRandomSet(random, maxValuesPerColumn)).ToList();

            List<TupleWrapperInt> cartesianProduct = CartesianProduct(columnValues);

            //int rowsToRemove = random.Next(0, cartesianProduct.Count / 2);
            var indicesToRemove = Enumerable.Range(0, cartesianProduct.Count)
                                            .OrderBy(x => random.Next())
                                            .Take(rowsToRemove)
                                            .ToList();

            //return cartesianProduct.Where((x, i) => !indicesToRemove.Contains(i)).ToList();
            List<TupleWrapperInt> reducedDataset = cartesianProduct.Where((x, i) => !indicesToRemove.Contains(i)).ToList();
            //Возвращаем список, содержащий полный и уменьшенный датасеты.
            return new List<List<TupleWrapperInt>>() { cartesianProduct, reducedDataset };
        }

        static HashSet<int> GenerateRandomSet(Random random, int maxValues)
        {
            int count = random.Next(1, maxValues + 1);
            return new HashSet<int>(Enumerable.Range(0, count).Select(_ => random.Next(1, 101)));
        }

        static List<TupleWrapperInt> CartesianProduct(List<HashSet<int>> sets)
        {
            if (sets == null || sets.Count == 0) return new List<TupleWrapperInt>();

            List<TupleWrapperInt> resultTuples = new List<TupleWrapperInt>();
            CartesianProductRecursive(sets, 0, new List<int>(), resultTuples);

            return resultTuples;
        }

        static void CartesianProductRecursive(List<HashSet<int>> sets, int index, List<int> current, List<TupleWrapperInt> result)
        {
            if (index == sets.Count)
            {
                result.Add(new TupleWrapperInt(current.ToArray()));
                return;
            }

            foreach (var value in sets[index])
            {
                current.Add(value);
                CartesianProductRecursive(sets, index + 1, current, result);
                current.RemoveAt(current.Count - 1);
            }
        }

        static int CountSetBits(ulong n)
        {
            int count = 0;
            while (n > 0)
            {
                count += (int)(n & 1);
                n >>= 1;
            }
            return count;
        }

        static void TestCountSetBits()
        {
            Console.WriteLine("Тесты метода CountSetBits:");

            TestCountSetBitsCase(0UL, 0);
            TestCountSetBitsCase(1UL, 1);
            TestCountSetBitsCase(2UL, 1);
            TestCountSetBitsCase(3UL, 2);
            TestCountSetBitsCase(15UL, 4);
            TestCountSetBitsCase(ulong.MaxValue, 64);
        }

        static void TestCountSetBitsCase(ulong input, int expected)
        {
            int result = CountSetBits(input);
            Console.WriteLine($"CountSetBits({input}) = {result} (Ожидалось: {expected}) - {(result == expected ? "Успешно" : "Ошибка")}");
        }

        static IEnumerable<ulong> GenerateNumbersByBitCount(int n)
        {
            if (n <= 0) throw new ArgumentException("n должно быть положительным целым числом.");

            ulong maxValue = (1UL << n) - 1;

            return GenerateNumbersByBitCountHelper(maxValue).OrderByDescending(i => CountSetBits(i)).ThenBy(a => a);
        }

        static IEnumerable<ulong> GenerateNumbersByBitCountHelper(ulong maxValue)
        {
            for (ulong i = 1; i <= maxValue; i++)
            {
                yield return i;
            }
        }

        static void TestGenerateNumbersByBitCount()
        {
            Console.WriteLine("\nТестирование GenerateNumbersByBitCount:");

            TestGenerateNumbersByBitCountCase(1, new ulong[] { 1 });
            TestGenerateNumbersByBitCountCase(2, new ulong[] { 3, 1, 2 });
            TestGenerateNumbersByBitCountCase(3, new ulong[] { 7, 3, 5, 6, 1, 2, 4 });
            TestGenerateNumbersByBitCountCase(4, new ulong[] { 15, 7, 11, 13, 14, 3, 5, 6, 9, 10, 12, 1, 2, 4, 8 });
        }

        static void TestGenerateNumbersByBitCountCase(int n, ulong[] expected)
        {
            ulong[] actual = GenerateNumbersByBitCount(n).ToArray();
            if (actual.Length != expected.Length)
            {
                Console.WriteLine($"GenerateNumbersByBitCount({n}): Разное количество элементов. Получено {actual.Length}, ожидалось {expected.Length} - Ошибка");
                return;
            }

            string message = $"GenerateNumbersByBitCount({n}): ";
            bool passed = true;
            for (int i = 0; i < actual.Length; i++)
            {
                if (CountSetBits(actual[i]) != CountSetBits(expected[i]) || actual[i] != expected[i])
                {
                    passed = false;
                    break;
                }
            }
            Console.WriteLine($"{message} - {(passed ? "Успешно" : "Ошибка")}");
        }

        static bool CheckDatasetSize(List<TupleWrapperInt> tuples)
        {
            if (tuples == null || tuples.Count == 0) return true;

            int columnCount = tuples[0].Values.Length;
            if (tuples.Any(t => t.Values.Length != columnCount))
            {
                throw new ArgumentException("Все кортежи должны иметь одинаковое количество столбцов.");
            }

            long datasetSize = tuples.Count;
            long productOfUniqueCounts = 1;

            for (int i = 0; i < columnCount; i++)
            {
                var uniqueValues = tuples.Select(t => t.Values[i]).Distinct().Count();
                productOfUniqueCounts *= uniqueValues;
                if (productOfUniqueCounts > long.MaxValue / uniqueValues)
                {
                    throw new OverflowException("Переполнение при вычислении произведения.");
                }
            }

            return datasetSize == productOfUniqueCounts;
        }

        static (ulong result, int result_size) FindMinimalFullSubset(List<TupleWrapperInt> source)
        {
            source = source.Distinct().ToList();

            if (CheckDatasetSize(source))
            {
                ulong fullSetRepresentation = (1UL << source.Count) - 1;
                return (fullSetRepresentation, source.Count);
            }

            List<TupleWrapperInt> subset = new List<TupleWrapperInt>();
            ulong result = 0;
            int result_size = 0;

            foreach (ulong i in GetSubSets(source.Count))
            {
                subset.Clear();

                for (int j = 0; j < source.Count; j++)
                {
                    if ((i & (1UL << j)) != 0)
                    {
                        subset.Add(source[j]);
                    }
                }

                if (CheckDatasetSize(subset))
                {
                    if (result != 0 && subset.Count == result_size)
                    {
                        throw new Exception("Найдено дублирующееся решение минимального размера.");
                    }

                    result = i;
                    result_size = subset.Count;

                    if (result != 0 && subset.Count < result_size)
                    {
                        break;
                    }
                }
                else if (result != 0 && subset.Count < result_size)
                {
                    break;
                }
            }

            return (result, result_size);
        }

        static IEnumerable<ulong> GetSubSets(int n)
        {
            for (ulong i = 1; i < (1UL << n); i++)
            {
                yield return i;
            }
        }
        static List<TupleWrapperInt> ReconstructSubset(List<TupleWrapperInt> source, ulong subsetRepresentation)
        {
            List<TupleWrapperInt> subset = new List<TupleWrapperInt>();
            for (int i = 0; i < source.Count; i++)
            {
                if ((subsetRepresentation & (1UL << i)) != 0)
                {
                    subset.Add(source[i]);
                }
            }
            return subset;
        }
        // Вспомогательная функция для сохранения датасета в файл
        static void SaveDatasetToFile(List<TupleWrapperInt> dataset, string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var tuple in dataset)
                {
                    writer.WriteLine(tuple);
                }
            }
        }
        // Новая функция для сохранения  списка  ошибочного датасета в файл
        static void SaveErrorDatasetsToFile(List<string> errorDatasets, string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var error in errorDatasets)
                {
                    writer.WriteLine(error);
                }
            }
        }
        static int DisplayMenu()
        {
            Console.WriteLine("Выберите размер таблицы:");
            Console.WriteLine("1. Малая размерность (2-3 столбца, 3-6 строк)");
            Console.WriteLine("2. Средний объем данных (3-4 столбца, 6-10 строк)");
            Console.WriteLine("3. Большой объем данных (до 4 столбцов и 12 строк)");
            Console.Write("Введите номер: ");
            int choice;
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 3)
            {
                Console.Write("Неверный ввод. Пожалуйста, введите номер от 1 до 3: ");
            }
            return choice;
        }
        static void Main(string[] args)
        {
            // Список для хранения ошибочных датасетов
            List<string> errorDatasets = new List<string>();

            // Тесты (можно закомментировать при генерации большого количества датасетов)
            Console.WriteLine("=====================");
            Console.WriteLine("Начало тестирования:");
            Console.WriteLine("=====================");
            TestCountSetBits();
            TestGenerateNumbersByBitCount();
            Console.WriteLine("=====================");
            Console.WriteLine("Тестирование завершено.");
            Console.WriteLine("=====================");

            // Генерация и вывод уникальных кортежей (можно закомментировать при генерации большого количества датасетов)
            Console.WriteLine("\nГенерация и вывод уникальных кортежей:");
            int columnCountUnique = 3; //Переименовали переменные
            int maxValuesPerColumnUnique = 6; //Переименовали переменные
            List<List<TupleWrapperInt>> uniqueTuplesList = GenerateRandomTable(1, columnCountUnique, maxValuesPerColumnUnique);
            TupleWrapperInt[] tuples = uniqueTuplesList[0].ToArray();
            HashSet<TupleWrapperInt> uniqueTuples = new HashSet<TupleWrapperInt>(tuples);

            Console.WriteLine("Уникальные кортежи, дубликаты удалены:");
            foreach (var tuple in uniqueTuples)
            {
                Console.WriteLine(tuple);
            }

            // Пример генерации чисел по количеству битов (можно закомментировать при генерации большого количества датасетов)
            Console.WriteLine("\nПример генерации чисел по количеству битов:");
            int n = 4;
            foreach (ulong num in GenerateNumbersByBitCount(n))
            {
                Console.WriteLine($"{num} (Количество единиц: {CountSetBits(num)})");
            }

            // Пример проверки размера датасета (можно закомментировать при генерации большого количества датасетов)
            Console.WriteLine("\nПример проверки размера датасета:");
            List<TupleWrapperInt> testTuples1 = new List<TupleWrapperInt>
    {
        new TupleWrapperInt(new int[] { 1, 2 }),
        new TupleWrapperInt(new int[] { 1, 3 }),
        new TupleWrapperInt(new int[] { 2, 2 }),
        new TupleWrapperInt(new int[] { 2, 3 })
    };
            List<TupleWrapperInt> testTuples2 = new List<TupleWrapperInt>
    {
        new TupleWrapperInt(new int[] { 1, 2 }),
        new TupleWrapperInt(new int[] { 1, 3 }),
        new TupleWrapperInt(new int[] { 2, 2 })
    };

            Console.WriteLine($"testTuples1: {CheckDatasetSize(testTuples1)}"); // должно быть true
            Console.WriteLine($"testTuples2: {CheckDatasetSize(testTuples2)}"); // должно быть false

            // Генерация 20 датасетов и сохранение в файлы
            Console.WriteLine("\nГенерация 20 датасетов:");
            for (int datasetIndex = 1; datasetIndex <= 20; datasetIndex++)
            {
                int sizeCategory = DisplayMenu();
                int columnCount;
                int maxValuesPerColumn;

                switch (sizeCategory)
                {
                    case 1:
                        columnCount = random.Next(2, 4);
                        maxValuesPerColumn = random.Next(3, 7);
                        break;
                    case 2:
                        columnCount = random.Next(3, 5);
                        maxValuesPerColumn = random.Next(6, 11);
                        break;
                    case 3:
                        columnCount = random.Next(2, 5);
                        maxValuesPerColumn = Math.Min(12, random.Next(8, 13));
                        break;
                    default:
                        throw new ArgumentException("Invalid sizeCategory");
                }
                List<List<TupleWrapperInt>> datasets = GenerateRandomTable(sizeCategory, columnCount, maxValuesPerColumn); //Исправлено: передаем три аргумента
                string baseFileName = $"dataset_{datasetIndex}";



                SaveDatasetToFile(datasets[0], baseFileName + ".in"); // Полный датасет
                SaveDatasetToFile(datasets[1], baseFileName + "_reduced.in"); // Датасет с удаленными строками

                try
                {
                    (ulong foundSubset, int size) = FindMinimalFullSubset(datasets[1]); // Обрабатываем reduced dataset
                    List<TupleWrapperInt> minimalSubset = ReconstructSubset(datasets[1], foundSubset);
                    SaveDatasetToFile(minimalSubset, baseFileName + ".ans"); // Сохраняем в .ans

                    Console.WriteLine($"\nДатасет {datasetIndex}:");
                    Console.WriteLine($"  Полный датасет сохранен в {baseFileName}.in");
                    Console.WriteLine($"  Уменьшенный датасет сохранен в {baseFileName}_reduced.in");
                    Console.WriteLine($"  Минимальное подмножество сохранено в {baseFileName}.ans (размер: {size})");
                    Console.WriteLine($"    Битовое представление подмножества: (размер: {size})");

                    //Вывод кортежей минимального подмножества в консоль (опционально):
                    Console.WriteLine("    Кортежи в минимальном подмножестве:");
                    foreach (var tuple in minimalSubset)
                    {
                        Console.WriteLine($"      {tuple}");
                    }


                }
                catch (Exception ex)
                {
                    errorDatasets.Add(datasetIndex.ToString()); // Добавляем только номер датасета
                    Console.WriteLine($"\nОшибка при обработке датасета {baseFileName}: {ex.Message}");
                }
            }
            // Вывод списка ошибочных датасетов
            if (errorDatasets.Any())
            {
                Console.WriteLine("\nОшибочные датасеты:");
                foreach (var error in errorDatasets)
                {
                    Console.WriteLine(error);
                }
                SaveErrorDatasetsToFile(errorDatasets, "error_datasets.txt");
            }
            else
            {
                Console.WriteLine("\nОшибочные датасеты отсутствуют.");
            }

            Console.WriteLine("\nГенерация и сохранение 20 датасетов завершены.");
        }        
    }
}
