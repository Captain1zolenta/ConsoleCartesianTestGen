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
        static void Main(string[] args)
        {
            int columnCount = 3;
            int maxValuesPerColumn = 5;
            TupleWrapperInt[] tuples = GenerateRandomTable(columnCount, maxValuesPerColumn).ToArray();

            HashSet<TupleWrapperInt> uniqueTuples = new HashSet<TupleWrapperInt>(tuples);

            Console.WriteLine("Уникальные кортежи, дубликаты удалены:");
            foreach (var tuple in uniqueTuples)
            {
                Console.WriteLine(tuple);
            }
        }
        
    }
}