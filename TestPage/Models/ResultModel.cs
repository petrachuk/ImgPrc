using System;
using System.Linq;

namespace TestPage.Models
{
    public class ResultModel
    {
        public long Img1Hash { get; set; }
        public long Img2Hash { get; set; }
        public long Img1PHash { get; set; }
        public long Img2PHash { get; set; }
        public long Img1Magic { get; set; }
        public long Img2Magic { get; set; }

        public long TimeHash { get; set; }
        public long TimePHash { get; set; }
        public long TimeMagic { get; set; }

        public string DiffrenseHash => Convert.ToString(Img1Hash ^ Img2Hash, 2).PadLeft(64, '0');
        public string DiffrensePHash => Convert.ToString(Img1PHash ^ Img2PHash, 2).PadLeft(64, '0');
        public string DiffrenseMagic => Convert.ToString(Img1Magic ^ Img2Magic, 2).PadLeft(64, '0');

        public int HammingDistanseHash => DiffrenseHash.ToCharArray().Count(x => x == '1');
        public int HammingDistansePHash => DiffrensePHash.ToCharArray().Count(x => x == '1');
        public int HammingDistanseMagic => DiffrenseMagic.ToCharArray().Count(x => x == '1');

        public string HashSummary => Grade(HammingDistanseHash);
        public string PHashSummary => Grade(HammingDistansePHash);
        public string MagicSummary => Grade(HammingDistanseMagic);

        private static string Grade(int hash)
        {
            if (hash == 0) return "Одинаковые (100%)";
            if (hash <= 5) return $"Очень похожи ({100 - (hash / 64.0) * 100}%)";
            return hash <= 10 ? $"Похожи  {100 - (hash / 64.0) * 100}%" : $"Разные  {100 - (hash / 64.0) * 100}%";
        }
    }
}
