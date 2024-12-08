using System;
using System.Collections.Generic;
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
        static List<TupleWrapperInt> GenerateRandomTable(int columnCount, int maxValuesPerColumn)
        {
            Random random = new Random();
            List<HashSet<int>> columnValues = Enumerable.Range(0, columnCount)
                .Select(i => GenerateRandomSet(random, maxValuesPerColumn)).ToList();

            List<TupleWrapperInt> cartesianProduct = CartesianProduct(columnValues);

            int rowsToRemove = random.Next(0, cartesianProduct.Count / 2);
            var indicesToRemove = Enumerable.Range(0, cartesianProduct.Count)
                                            .OrderBy(x => random.Next())
                                            .Take(rowsToRemove)
                                            .ToList();

            return cartesianProduct.Where((x, i) => !indicesToRemove.Contains(i)).ToList();
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

        static void Main(string[] args)
        {
            Console.WriteLine("=====================");
            Console.WriteLine("Начало тестирования:");
            Console.WriteLine("=====================");
            TestCountSetBits();
            TestGenerateNumbersByBitCount();
            Console.WriteLine("=====================");
            Console.WriteLine("Тестирование завершено.");
            Console.WriteLine("=====================");

            Console.WriteLine("\nГенерация и вывод уникальных кортежей:");
            int columnCount = 3;
            int maxValuesPerColumn = 6;
            TupleWrapperInt[] tuples = GenerateRandomTable(columnCount, maxValuesPerColumn).ToArray();
            HashSet<TupleWrapperInt> uniqueTuples = new HashSet<TupleWrapperInt>(tuples);

            Console.WriteLine("Уникальные кортежи, дубликаты удалены:");
            foreach (var tuple in uniqueTuples)
            {
                Console.WriteLine(tuple);
            }

            Console.WriteLine("\nПример генерации чисел по количеству битов:");
            int n = 4;
            foreach (ulong num in GenerateNumbersByBitCount(n))
            {
                Console.WriteLine($"{num} (Количество единиц: {CountSetBits(num)})");
            }

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

            Console.WriteLine($"testTuples1: {CheckDatasetSize(testTuples1)}");
            Console.WriteLine($"testTuples2: {CheckDatasetSize(testTuples2)}");

            Console.WriteLine("\nПример поиска минимального полного подмножества:");
            List<TupleWrapperInt> sourceData = GenerateRandomTable(2, 3);
            Console.WriteLine("\nИсходный датасет:");
            foreach (var item in sourceData) Console.WriteLine(item);
            
            try
            {
                (ulong foundSubset, int size) = FindMinimalFullSubset(sourceData);
                Console.WriteLine($"\nНайден минимальный полный подмножество, Размер: {size}");

                List<TupleWrapperInt> minimalSubset = ReconstructSubset(sourceData, foundSubset);

                Console.WriteLine("\nКортежи в минимальном подмножестве:");
                foreach (var tuple in minimalSubset)
                {
                    Console.WriteLine(tuple);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
            }
        }
        /*
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
        }*/
    }
}
