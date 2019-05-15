using System;
using ImgLib;

namespace ImgPrc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const Mode mode = Mode.CropPHash;

            using (var proc1 = new ImgProcessing(@"test1.jpg"))
            using (var proc2 = new ImgProcessing(@"test2.jpg"))
             {
                var hash1 = proc1.Processing(mode);
                var hash2 = proc2.Processing(mode);

                // Вывод результатов
                ImgProcessing.Print(hash1);
                Console.WriteLine(Convert.ToString(hash1, 2).PadLeft(64, '0'));
                Console.WriteLine(Convert.ToString(hash1, 16));

                // Вывод результатов
                ImgProcessing.Print(hash2);
                Console.WriteLine(Convert.ToString(hash2, 2).PadLeft(64, '0'));
                Console.WriteLine(Convert.ToString(hash2, 16));

                Console.WriteLine("\nHamDistanse: " + ImgProcessing.GetHamDistanse(hash1, hash2));
            }
        }
    }
}
