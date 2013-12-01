using System;
using System.Collections.Generic;
using System.Text;

namespace ads_permutations
{
    class Permutations
    {

        int factorial(int n) {
            if (n == 0)
                return 1;

            return n * factorial(n - 1);
        }

        /// <summary>
        /// Generate order of permutation (zero-based)
        /// </summary>
        /// <param name="order">Permutation's order</param>
        /// <param name="length">Permutation's length</param>
        /// <returns>Permutation</returns>
        int[] orderToPermutation(int order, int length) {
            var permutation = new int[length];
            
            // generate identity
            for (int i = 0; i < length; i++)
                permutation[i] = i;

            for (int i = 0; i < length; i++)
            {
                // how far will the current item be swapped
                int step = order / factorial(length - 1 - i);

                var temp = permutation[i];
                permutation[i] = permutation[i + step];
                permutation[i + step] = temp;

                order = order % factorial(length - 1 - i);
            }

            return permutation;
        }

        /// <summary>
        /// Calculates order for given permutation (zero-based)
        /// </summary>
        /// <param name="permutation">Permutation</param>
        /// <returns>Order of permutation</returns>
        int permutationToOrder(int[] permutation) {
            int order = 0;
            int length = permutation.Length;

            // Permutation for generating inverse swappings
            var swapping = new int[length];
            // Positions of items in swapping for instant location 
            var positions = new int[length];
            
            // Generate identities
            for (int i = 0; i < length; i++)
            {
                positions[i] = i;
                swapping[i] = i;
            }

            for (int i = 0; i < length - 1; i++)
            {
                order += Math.Abs(i - positions[permutation[i]]) * factorial(length - 1 - i);

                int actual = permutation[i];

                // make inverse transposition
                int current = swapping[i];
                swapping[i] = swapping[positions[permutation[i]]];
                swapping[positions[permutation[i]]] = current;
                
                // correct items positions
                int temp = positions[current];
                positions[current] = positions[actual];
                positions[actual] = temp;
            }

            return order;
        }

        public Permutations()
        {

            int n = 6;
            for (int i = 0; i < factorial(n); i++)
            {

                var perm = orderToPermutation(i, n);
                Console.Write(i + ":: ");
                Console.Write(String.Join("", perm));

                int k = permutationToOrder(perm);
                Console.Write(" : ");
                Console.Write(k);

                Console.Write(" : ");
                Console.Write(i - k);

                Console.WriteLine();
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
        }
    }
}
