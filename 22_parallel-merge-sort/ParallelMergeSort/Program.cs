﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;

/*

Napište paralelní implementaci třídicího algoritmu MergeSort (třídění sléváním). Při implementaci použijte základní funkce pro vytváření vláken (třída Thread). Podívejte se zejména na metody Start, Join, Abort, Sleep a Interrupt. Vaše aplikace dostane na standardním vstupu libovolný počet čísel (každé číslo zapsané v dekadickém formátu na samostatném řádku), přičemž data jsou zakončena právě jedním prázdným řádkem. Po načtení celého vstupu (který se vejde do paměti) váš program setřídí všechna data a vypíše je na standardní výstup (opět každé na samostatný řádek). Všechna čísla se vejdou do 32-bitového integeru se znaménkem. Můžete také předpokládat, že do paměti se data vejdou nejméně 2× (máte dostatek místa na jedno pomocné pole pro MergeSort). Pokud se ve vstupních datech objeví jakákoli chyba (špatně nafomátované číslo, předčasné ukončení vstupu apod.), program vypíše na standardní výstup řetězec "Format Error" a skončí.

Váš program dostane právě jeden argument, což je celé číslo T v rozsahu 1 až 256. Číslo T udává, kolik vláken by měla aplikace celkem využívat (nezapomeňte, že hlavní vlákno se do toho také počítá). Pokud je tedy T=1, nesmí váš program vytvářet žádná další vlákna. Pokud je např. T=4, vytvoří váš program právě 3 další vlákna. Výjimku z výše uvedeného pravidla tvoří situace, pokud je načtených dat ostře méně než 2*T. V takovém případě se nevyplatí úlohu paralelizovat a k setřídění použijte jednovláknovou variantu (jako by T bylo rovno 1). Pokud je argument T zadán chybně (např. mimo rozsah) nebo váš program dostane jiný počet argumentů, rovnou vypíše na standardní výstup "Argument Error" a skončí.

Příklad: program.exe 4
std. vstup
    7
    8
    6
    10
    9
    3
    1
    2
    5
    4
    -- prázdný řádek --
std. výstup
    1
    2
    3
    4
    5
    6
    7
    8
    9
    10
*/

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ParaMergeTests")]

namespace ParallelMergeSort {

    public class ParallelMergeSort<T> where T : IComparable<T> {

        private class PieceOfArray {

            T[] array;
            public int From;
            public int Length;

            public PieceOfArray(T[] array, int from, int length)
            {
                this.array = array;
                this.From = from;
                this.Length = length;
            }

            public void Start()
            {
                Array.Sort(array, From, Length);
            }

        }

        T[] array;

        public ParallelMergeSort(T[] array)
        {
            this.array = array;
        }

        public T[] Sort(int nThreads = 2)
        {
            if (nThreads < 1)
                throw new ArgumentOutOfRangeException("At least 1 thread is needed to proceed.");

            int howManyItemsPerThread = array.Length / nThreads;
            if (howManyItemsPerThread <= 0)
                return array;

            if (array.Length < 2 * nThreads)
                nThreads = 1; // Not enough items for more threads

            Thread[] threads = new Thread[nThreads];
            threads[0] = Thread.CurrentThread;

            List<PieceOfArray> pieces = new List<PieceOfArray>(nThreads);
            pieces.Add(new PieceOfArray(array, 0, howManyItemsPerThread));

            // Creating more Threads
            for (int i = 1; i < nThreads; i++)
            {
                pieces.Add(new PieceOfArray(array, i * howManyItemsPerThread,
                    i == nThreads - 1 ? array.Length - i * howManyItemsPerThread : howManyItemsPerThread
                ));
                threads[i] = new Thread(pieces[i].Start);
                threads[i].Start();
            }

            // Run first piece with default Thread
            pieces[0].Start();

            for (int i = 1; i < nThreads; i++)
                threads[i].Join();

            T[] temp = new T[array.Length];

            // Merge two consecutive pieces until there is just one remaining
            // [less operations then merging everything with the first piece -> around 11% less for 4 pieces]
            while (pieces.Count > 1)
            {
                int curr = 0;
                while (curr < pieces.Count - 1)
                {
                    merge(array, temp,
                        pieces[curr].From, pieces[curr].Length,
                        pieces[curr + 1].From, pieces[curr + 1].Length
                    );
                    pieces[curr].Length += pieces[curr + 1].Length;
                    pieces.RemoveAt(curr + 1);
                    curr++;
                }
                
                // Swap array pointers to simulate copying
                T[] tempPointer = array;
                array = temp;
                temp = tempPointer;
            }

            // Merge all other pieces with the first one [more operations]
            //for (int i = 1; i < nThreads; i++)
            //{
            //    merge(array, temp,
            //        pieces[0].From, pieces[0].Length,
            //        pieces[i].From, pieces[i].Length
            //    );
            //    pieces[0].Length += pieces[i].Length;
            //}

            return array;
        }

        void merge(T[] array, T[] temp, int left, int leftLength, int right, int rightLength)
        {
            int leftIndex = left;
            int rightIndex = right;
            int tempIndex = left;

            while (leftIndex < left + leftLength && rightIndex < right + rightLength)
                temp[tempIndex++] = array[leftIndex].CompareTo(array[rightIndex]) <= 0
                    ? array[leftIndex++]
                    : array[rightIndex++];

            while (leftIndex < left + leftLength)
                temp[tempIndex++] = array[leftIndex++];

            while (rightIndex < right + rightLength)
                temp[tempIndex++] = array[rightIndex++];

            // Can use this instead of array swapping [but slower]
            //for (int i = left; i < right + rightLength; i++)
            //    array[i] = temp[i];
        }

    }

    public class Program {

        static int[] ReadInput(TextReader input)
        {
            List<int> list = new List<int>();

            string ln;
            while ((ln = input.ReadLine()) != null && ln != "")
                list.Add(int.Parse(ln));

            return list.ToArray();
        }

        static void WriteOutput(int[] array, TextWriter output)
        {
            for (int i = 0; i < array.Length; i++)
                output.WriteLine(array[i]);
        }

        public static void Run(string[] args, TextReader input, TextWriter output)
        {
            if (args.Length != 1)
                throw new ArgumentException();

            int nThreads;
            if (int.TryParse(args[0], out nThreads) == false)
                throw new ArgumentException();

            if ((nThreads >= 1 && nThreads <= 256) == false)
                throw new ArgumentException();

            int[] array = ReadInput(input);

            // actual sorting
            var parallelMergeSort = new ParallelMergeSort<int>(array);
            array = parallelMergeSort.Sort(nThreads);

            WriteOutput(array, output);
        }

        static void generateFileWithRandomNumbers(string filename, int count)
        {
            var r = new Random();
            var o = new StreamWriter(File.Create(filename));

            for (int i = 0; i < count; i++)
                o.WriteLine(r.Next());

            o.Close();
        }

        static void Main(string[] args)
        {
            try
            {
                Run(args, System.Console.In, System.Console.Out);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Argument Error");
            }
            catch (FormatException)
            {
                Console.WriteLine("Format Error");
            }
        }
    }
}
