using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestGenerator
{
   
    // Класс для кортежа с переопределенными Equals и GetHashCode
    public class TupleWrapperInt : IEquatable<TupleWrapperInt>
{
        public int[] Values { get; }

        public TupleWrapperInt(int[] values)
        {
            Values = values;
        }
        //Обеспечивает сравнение объектов TupleWrapper<T> с другими объектами
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
            unchecked // Переполнение допустимо, просто происходит перенос
            {   
                int hash = Values.Length; // Начальное значение хэш-кода - длина массива

                // Цикл по всем значениям в массиве
                for (int i = 0; i < Values.Length; i++)
                {
                    // Побитовое исключающее ИЛИ (XOR) текущего хэш-кода с хэш-кодом элемента.
                    // Если элемент null, используется 0.
                    hash ^= Values[i].GetHashCode();
                }
                return hash;
            }
        }
        // Для отладки
        public override string ToString()
        {
            //return string.Join(" ", Values?.Select(v => v?.ToString()) ?? Enumerable.Empty<string>());
            return string.Join(" ", Values);
        }
    }

    public class Program
{
        // Генерирует случайную таблицу (неполное декартово произведение) с указанным количеством
        // столбцов и максимальным количеством значений в каждом столбце.
        static List<TupleWrapperInt> GenerateRandomTable(int columnCount, int maxValuesPerColumn)
        {
            Random random = new Random();
            List<HashSet<int>> columnValues = Enumerable.Range(0, columnCount)
                .Select(i => GenerateRandomSet(random, maxValuesPerColumn)).ToList();

            //CartesianProduct теперь возвращает List<TupleWrapper<int>>
            List<TupleWrapperInt> cartesianProduct = CartesianProduct(columnValues);

            int rowsToRemove = random.Next(0, cartesianProduct.Count / 2);
            var indicesToRemove = Enumerable.Range(0, cartesianProduct.Count)
                                            .OrderBy(x => random.Next())
                                            .Take(rowsToRemove)
                                            .ToList();

            return cartesianProduct.Where((x, i) => !indicesToRemove.Contains(i)).ToList();
        }
        // Генерирует множество случайных целых чисел.
        static HashSet<int> GenerateRandomSet(Random random, int maxValues)
        {
            int count = random.Next(1, maxValues + 1);
            return new HashSet<int>(Enumerable.Range(0, count).Select(_ => random.Next(1, 101)));
        }
        // Вычисляет декартово произведение множеств, используя рекурсивный подход.
        static List<TupleWrapperInt> CartesianProduct(List<HashSet<int>> sets)
        {
            if (sets == null || sets.Count == 0) return new List<TupleWrapperInt>();

            List<TupleWrapperInt> resultTuples = new List<TupleWrapperInt>();
            CartesianProductRecursive(sets, 0, new List<int>(), resultTuples);

            return resultTuples;
        }

        // Рекурсивная функция для вычисления декартова произведения.
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
        //Метод для подсчета единиц в ulong
        static int CountSetBits(ulong n)
        {
            int count = 0;
            while (n > 0)
            {
                count += (int)(n & 1); // Проверяем младший бит
                n >>= 1; // Сдвигаем число вправо на 1 бит
            }
            return count;
        }

        //Проверка метода CountSetBits
        static void TestCountSetBits()
        {
            Console.WriteLine("Тесты  метода CountSetBits:");

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

        //Метод для перебора чисел от 1 до 2^n - 1 в порядке убывания количества бит
        static IEnumerable<ulong> GenerateNumbersByBitCount(int n)
        {
            if (n <= 0) throw new ArgumentException("n должно быть положительным целым числом.");

            ulong maxValue = (ulong)(Math.Pow(2, n) - 1);
            return Enumerable.Range(0, (int)maxValue + 1)
                             .Select(i => (ulong)i)
                             .OrderByDescending(i => CountSetBits(i))
                             .Skip(1); // Пропускаем 0
        }
        static void Main(string[] args)
        {
            // Выполнение тестов
            Console.WriteLine("=====================");
            Console.WriteLine("Начало тестирования:");
            Console.WriteLine("=====================");
            TestCountSetBits();
            Console.WriteLine("=====================");
            Console.WriteLine("Тестирование завершено.");
            Console.WriteLine("=====================");


            // Генерация и вывод уникальных кортежей
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
            //Пример использования GenerateNumbersByBitCount
            Console.WriteLine("\nПример генерации чисел по количеству битов:");
            int n = 4; //числа до 2^4 -1 =15
            foreach (ulong num in GenerateNumbersByBitCount(n))
            {
                Console.WriteLine($"{num} (Количество единиц: {CountSetBits(num)})");
            }

        }
        
    }
}