using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        const int N = 10_000_000;

        Console.WriteLine("Початок обчислень (без синхронізації)...");
        Stopwatch sw = Stopwatch.StartNew();

        // 1. Визначаємо кількість потоків (рівну кількості ядер процесора)
        int threadCount = Environment.ProcessorCount;

        // 2. Створюємо список задач, які будуть повертати число (long)
        List<Task<long>> tasks = new List<Task<long>>();

        // 3. Розраховуємо розмір блоку для одного потоку
        int batchSize = N / threadCount;

        for (int i = 0; i < threadCount; i++)
        {
            // Визначаємо межі діапазону для кожного потоку
            int start = i * batchSize + 1;

            // Останній потік має забрати "хвостик", якщо N не ділиться націло
            int end = (i == threadCount - 1) ? N : (start + batchSize - 1);

            // Створюємо і запускаємо задачу. 
            // Важливо: ми не передаємо посилання на спільні змінні, все локально.
            tasks.Add(Task.Run(() => CalculateRangeSteps(start, end)));
        }

        // 4. Чекаємо завершення всіх задач
        Task.WaitAll(tasks.ToArray());

        // 5. Агрегація (зведення) результатів
        // Оскільки потоки вже завершили роботу, ми просто сумуємо те, що вони повернули.
        // Тут немає гонки даних (Race Condition), тому синхронізація не потрібна.
        long totalSteps = 0;
        foreach (var task in tasks)
        {
            totalSteps += task.Result;
        }

        sw.Stop();

        double avgSteps = (double)totalSteps / N;

        Console.WriteLine($"Кількість потоків: {threadCount}");
        Console.WriteLine($"Обчислення завершено за {sw.ElapsedMilliseconds / 1000.0:F4} с");
        Console.WriteLine($"Середня кількість кроків до 1 для {N:N0} чисел = {avgSteps:F2}");
    }

    // Метод, що виконується в потоці. Він працює ТІЛЬКИ з своїм діапазоном.
    static long CalculateRangeSteps(int start, int end)
    {
        long localSum = 0;
        for (int i = start; i <= end; i++)
        {
            localSum += CollatzSteps(i);
        }
        return localSum; // Повертаємо результат, а не пишемо в глобальну змінну
    }

    // Логіка гіпотези Коллатца
    static int CollatzSteps(long n)
    {
        int steps = 0;
        while (n != 1)
        {
            if ((n & 1) == 0) // n парне
                n >>= 1;     // бітовий зсув (ділення на 2)
            else             // n непарне
                n = 3 * n + 1;
            steps++;
        }
        return steps;
    }
}