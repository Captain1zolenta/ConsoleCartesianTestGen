using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestGenerator
{
    class Program
    {
        // Класс для кортежа с переопределенными Equals и GetHashCode
        public class TupleWrapper<T> : IEquatable<TupleWrapper<T>>
        {
            public T[] Values { get; }

            public TupleWrapper(T[] values)
            {
                Values = values;
            }
            //Обеспечивает сравнение объектов TupleWrapper<T> с другими объектами
            public override bool Equals(object obj)
            {
                return Equals(obj as TupleWrapper<T>);
            }

            public bool Equals(TupleWrapper<T> other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                if (Values.Length != other?.Values.Length) return false;

                for (int i = 0; i < Values.Length; i++)
                {                    
                    if (!object.Equals(Values[i], other.Values[i])) return false;
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
                        hash ^= Values[i]?.GetHashCode() ?? 0;
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
        static void Main(string[] args)
        {
            string[] values = { "A", "B", null };
            TupleWrapper<string> myTuple = new TupleWrapper<string>(values);

            Console.WriteLine(myTuple);
            // Генерируем 20 таблиц
            for (int i = 1; i <= 20; i++)
            {
                // Генерируем случайную таблицу
                var data = GenerateRandomTable();

                // Получаем уникальные значения из каждой колонки
                var uniqueValues = new List<HashSet<string>>();
                foreach (var column in data[0].Keys)
                {
                    var uniqueColumnValues = new HashSet<string>(data.Select(row => row[column]));
                    uniqueValues.Add(uniqueColumnValues);
                }

                // Генерируем декартово произведение уникальных значений
                var cartesianProduct = CartesianProduct(uniqueValues);

                // Проверяем наличие строк из декартова произведения в исходной таблице
                var result = cartesianProduct.Select(row => data.Any(originalRow => originalRow.Values.SequenceEqual(row))).ToList();
            }
        }
        static List<Dictionary<string, string>> GenerateRandomTable()
        {
            
            var random = new Random();
            // Случайное количество столбцов (от 2 до 3).
            int columnCount = random.Next(2, 4);
            // Случайное количество строк (от 3 до 6).
            int rowCount = random.Next(3, 7);
            // Создаем пустой список для хранения таблицы (списка словарей).
            var data = new List<Dictionary<string, string>>();
            // Цикл по строкам.
            for (int i = 0; i < rowCount; i++)
            {
                // Создаем словарь для текущей строки.
                var row = new Dictionary<string, string>();

                // Цикл по столбцам.
                for (int j = 0; j < columnCount; j++)
                {
                    // Генерация случайной буквы от 'A' до 'Z' (включительно).                     
                    char randomChar = (char)random.Next('A', 'Z' + 1);

                    // Добавление столбца со случайной буквой в словарь строки.
                    row[$"Column{j + 1}"] = randomChar.ToString();
                }
                // Добавление сформированной строки в список данных таблицы
                data.Add(row);
            }

            // Возвращаем сгенерированную таблицу.
            return data;
        }
        static List<List<string>> CartesianProduct(List<HashSet<string>> sets)
        {
            var result = new List<List<string>>(); // Создаем список для хранения результатов (каждая комбинация - это список строк).
            CartesianProductRecursive(sets, 0, new List<string>(), result); // Вызываем рекурсивную функцию для вычисления декартова произведения.
            return result; // Возвращаем список всех комбинаций.
        }
        static void CartesianProductRecursive(List<HashSet<string>> sets, int index, List<string> current, List<List<string>> result)
        {
            if (index == sets.Count) // Базовый случай рекурсии: если мы обработали все множества
            {
                result.Add(new List<string>(current)); // Добавляем текущую комбинацию в список результатов.  Создаем копию current, чтобы избежать изменений в дальнейшем.
                return;
            }

            foreach (var value in sets[index]) // Перебираем все значения в текущем множестве
            {
                current.Add(value); // Добавляем текущее значение в текущую комбинацию
                CartesianProductRecursive(sets, index + 1, current, result); // Рекурсивный вызов для следующего множества
                current.RemoveAt(current.Count - 1); // Удаляем добавленное значение, чтобы подготовиться к следующей итерации.  Это "откат" для перебора всех комбинаций.
            }
        }
    }
}