using System;
using System.Linq;
using OpenCvSharp;

// http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html
// http://www.hackerfactor.com/blog/index.php?/archives/529-Kind-of-Like-That.html
namespace ImgLib
{
    public class ImgProcessing : IDisposable
    {
        private const int ResizeMethod = 2; // Простой

        private bool _disposed;
        private Mat _srcImg;

        #region Constructors
        public ImgProcessing(string file)
        {
            _srcImg = new Mat(file);
        }
        #endregion

        public long Processing(Mode mode)
        {
            long hash;
            Mat coreImg;

            switch (mode)
            {
                case Mode.AHash:
                    coreImg = _srcImg;

                    // На основе среднего
                    hash = AHash(coreImg);
                    break;
                case Mode.PHash:
                    coreImg = _srcImg;

                    // pHash
                    hash = PHash(coreImg);
                    break;
                case Mode.DHash:
                    coreImg = _srcImg;

                    // pHash
                    hash = DHash(coreImg);
                    break;
                case Mode.CropAHash:
                    // Получим ядро изображения
                    coreImg = GetCrop(_srcImg);

                    // Расчитаем pHash
                    hash = AHash(coreImg);
                    break;
                case Mode.CropPHash:
                    // Получим ядро изображения
                    coreImg = GetCrop(_srcImg);

                    // Расчитаем pHash
                    hash = PHash(coreImg);
                    break;
                default:
                    // Получим ядро изображения
                    coreImg = GetCrop(_srcImg);

                    // Расчитаем pHash
                    hash = DHash(coreImg);
                    break;
            }

            return hash;
        }

        public static int GetHamDistanse(long hash1, long hash2)
        {
            var xor = hash1 ^ hash2;
            var bitChain = Convert.ToString(xor, 2).PadLeft(64, '0').ToCharArray();
            return bitChain.Count(x => x == '1');
        }

        /// <summary>
        /// aHash
        /// </summary>
        /// <param name="src">Изображение</param>
        /// <returns></returns>
        public long AHash(Mat src)
        {
            // Убрать цвет
            var grayImg = new Mat();
            Cv2.CvtColor(src, grayImg, ColorConversionCodes.BGR2GRAY);

            // Уменьшить размер
            var smallImg = ResizeGrayImg(grayImg, 8, 8,ResizeMethod);

            // Найти среднее
            var thresh = Cv2.Mean(smallImg).Val0;

            // Цепочка битов
            var bits = smallImg.Threshold(thresh, 255, ThresholdTypes.Binary);
            smallImg.Dispose();
            
            return GetHash(bits, (byte)255);
        }

        /// <summary>
        /// pHash
        /// </summary>
        /// <param name="src">Изображение</param>
        /// <returns></returns>
        public long PHash(Mat src)
        {
            // Убрать цвет
            var grayImg = new Mat();
            Cv2.CvtColor(src, grayImg, ColorConversionCodes.BGR2GRAY);
            // src.Dispose();

            // Уменьшить размер
            var smallImg = ResizeGrayImg(grayImg, 32, 32, ResizeMethod);
            grayImg.Dispose();

            // Запустить дискретное косинусное преобразование
            var prepImg = new Mat();
            smallImg.ConvertTo(prepImg, MatType.CV_32FC1, 1.0/255.0);
            smallImg.Dispose();

            var dctImg = new Mat();
            Cv2.Dct(prepImg, dctImg);
            prepImg.Dispose();

            // Сократить DCT
            var shortDct = new Mat(dctImg, new Range(0, 8), new Range(0, 8));
            dctImg.Dispose();

            // Вычислить среднее значение
            double avg = 0;
            for(var j = 0; j < 8; j++)
            for (var i = 0; i < 8; i++)
            {
                if (j == 0 && j == 0) continue;
                avg += shortDct.At<float>(j, i);
            }
            var thresh = avg / 63.0;

            // Ещё сократить DCT
            var bits = shortDct.Threshold(thresh, 255, ThresholdTypes.Binary);
            shortDct.Dispose();

            return GetHash(bits, (float)255);
        }

        /// <summary>
        /// dHash
        /// </summary>
        /// <param name="src">Изображение</param>
        /// <returns></returns>
        public long DHash(Mat src)
        {
            // Убрать цвет
            var grayImg = new Mat();
            Cv2.CvtColor(src, grayImg, ColorConversionCodes.BGR2GRAY);

            // Уменьшить размер
            var smallImg = ResizeGrayImg(grayImg, 9, 8, ResizeMethod);

            var bitChain = string.Empty;

            for (var j = 0; j < 8; j++)
            for (var i = 0; i < 8; i++)
            {
                if (smallImg.At<byte>(j, i) > smallImg.At<byte>(j, i + 1))
                    bitChain += "1";
                else
                    bitChain += "0";
            }

            smallImg.Dispose();

            var bits =  Convert.ToInt64(bitChain, 2);
            Console.WriteLine(bits);

            return Convert.ToInt64(bitChain, 2);;
        }

        /// <summary>
        /// Печать итоговой матрицы
        /// </summary>
        /// <param name="hash">Хеш-значение</param>
        public static void Print(long hash)
        {
            var bitChain = Convert.ToString(hash, 2).PadLeft(64, '0');
            var charArray = bitChain.ToCharArray();

            Console.WriteLine("--------");
            var cnt = 0;
            foreach (var chr in charArray)
            {
                Console.Write(chr == '1' ? "X" : " ");
                cnt++;

                if (cnt != 8) continue;

                Console.WriteLine();
                cnt = 0;
            }
            Console.WriteLine("--------");
        }

        /// <summary>
        /// Преобразование в битовую строку и конвертация в число
        /// </summary>
        /// <typeparam name="T">byte или float</typeparam>
        /// <param name="matrix">Изображение</param>
        /// <param name="threshold">Порог</param>
        /// <returns></returns>
        private static long GetHash<T>(Mat matrix, T threshold) where T: struct
        {
            var bitChain = string.Empty;
            var value = threshold.ToString();

            for (var j = 0; j < 8; j++)
            for (var i = 0; i < 8; i++)
            {
                switch (threshold)
                {
                    case byte _:
                        bitChain += matrix.At<byte>(j, i) < byte.Parse(value) ? "1" : "0";
                        break;
                    case float _:
                        bitChain += matrix.At<float>(j, i) < float.Parse(value) ? "1" : "0";
                        break;
                }
            }

            return Convert.ToInt64(bitChain, 2);
        }

        /// <summary>
        /// Обрезка изображения на основе градиентов Щарра
        /// </summary>
        /// <returns></returns>
        private Mat GetCrop(Mat src)
        {
            using (var gradient = GetGradient(src))
            using (var binary = gradient.Threshold(127, 255F, ThresholdTypes.Binary))
            {
                var newY0 = 0;
                for (var i = 0; i < binary.Rows; i++)
                for (var j = 0; j < binary.Cols; j++)
                {
                    var pixel = binary.At<float>(i, j);

                    if (float.IsNaN(pixel)) continue;
                    if (Math.Abs(pixel - 255F) > float.Epsilon) continue;

                    newY0 = i;
                    goto exit_y0;
                }

                exit_y0:

                var newY1 = binary.Rows - 1;
                for (var i = binary.Rows - 1; i > 0; i--)
                for (var j = 0; j < binary.Cols; j++)
                {
                    var pixel = binary.At<float>(i, j);

                    if (float.IsNaN(pixel)) continue;
                    if (Math.Abs(pixel - 255F) > float.Epsilon) continue;

                    newY1 = i;
                    goto exit_y1;
                }

                exit_y1:

                var newX0 = 0;
                for (var j = 0; j < binary.Cols; j++)
                for (var i = 0; i < binary.Rows; i++)
                {
                    var pixel = binary.At<float>(i, j);

                    if (float.IsNaN(pixel)) continue;
                    if (Math.Abs(pixel - 255F) > float.Epsilon) continue;

                    newX0 = j;
                    goto exit_x0;
                }

                exit_x0:

                var newX1 = binary.Cols - 1;
                for (var j = binary.Cols - 1; j > 0; j--)
                for (var i = 0; i < binary.Rows; i++)
                {
                    var pixel = binary.At<float>(i, j);

                    if (float.IsNaN(pixel)) continue;
                    if (Math.Abs(pixel - 255F) > float.Epsilon) continue;

                    newX1 = j;
                    goto exit_x1;
                }

                exit_x1:

                return new Mat(_srcImg, new Range(newY0, newY1), new Range(newX0, newX1));
            }
        }

        /// <summary>
        /// Изменение размера изображения в оттенках серого
        /// </summary>
        /// <param name="grayImg">Mat-источник</param>
        /// <param name="newDimX">Размерность по горизонтали</param>
        /// <param name="newDimY">Размерность по вертикали</param>
        /// <param name="mode">1 - Нормальный режим, 2 - простой</param>
        /// <returns></returns>
        private static Mat ResizeGrayImg(Mat grayImg, int newDimX, int newDimY, int mode)
        {
            if (mode == 2)
            {
                var blockX = (int)Math.Round(1.0 * grayImg.Width / newDimX);
                var blockY = (int)Math.Round(1.0 * grayImg.Height / newDimY);

                var tmp = new byte[newDimY, newDimX];
            
                for (var j = 0; j < newDimY; j++)
                for (var i = 0; i < newDimX; i++)
                {
                    var sum = 0.0;

                    for (var y = j * blockY; y < ((j+1) * blockY) - 1; y++)
                    for (var x = i * blockX; x < ((i+1) * blockX) - 1; x++)
                    {
                        sum += grayImg.At<byte>(y, x);
                    }

                    tmp[j, i] = (byte)Math.Round(sum / (blockX * blockY));
                }

                return new Mat(newDimY, newDimX, MatType.CV_8UC1, tmp);
            }
            else
            {
                var smallImg = new Mat();
                Cv2.Resize(grayImg, smallImg, new Size(newDimX, newDimY));
                return smallImg;
            }
        }

        /// <summary>
        /// Карта градиентов на базе оператора Щарра
        /// </summary>
        /// <param name="src">Изображение</param>
        /// <returns></returns>
        private static Mat GetGradient(Mat src)
        {
            using (var preparedSrc = new Mat())
            {
                Cv2.CvtColor(src, preparedSrc, ColorConversionCodes.BGR2GRAY);

                using (var gradX = preparedSrc.Scharr(ddepth: MatType.CV_32F, xorder: 0, yorder: 1, scale: 1 / 4.0))
                using (var gradY = preparedSrc.Scharr(ddepth: MatType.CV_32F, xorder: 1, yorder: 0, scale: 1 / 4.0))
                {
                    var result = new Mat();
                    Cv2.Magnitude(gradX, gradY, result);

                    return result;
                }
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
 
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _srcImg.Dispose();
                    _srcImg = null;
                }

                _disposed = true;
            }
        }
 
        ~ImgProcessing()
        {
            Dispose (false);
        }
        #endregion
    }
}
