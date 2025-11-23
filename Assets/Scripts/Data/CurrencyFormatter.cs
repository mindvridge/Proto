using System;
using System.Text;
using UnityEngine;

namespace HiddenGrowth.Data
{
    /// <summary>
    /// 재화 포맷터 - 한국어 단위 및 축약 표기 지원
    /// </summary>
    public static class CurrencyFormatter
    {
        #region Korean Units (한국어 단위)
        /// <summary>
        /// 한국어 숫자 단위 (4자리 단위)
        /// </summary>
        private static readonly string[] KoreanUnits = new string[]
        {
            "",         // 10^0   (일)
            "만",       // 10^4   (만)
            "억",       // 10^8   (억)
            "조",       // 10^12  (조)
            "경",       // 10^16  (경)
            "해",       // 10^20  (해)
            "자",       // 10^24  (자)
            "양",       // 10^28  (양)
            "구",       // 10^32  (구)
            "간",       // 10^36  (간)
            "정",       // 10^40  (정)
            "재",       // 10^44  (재)
            "극",       // 10^48  (극)
            "항하사",   // 10^52  (항하사)
            "아승기",   // 10^56  (아승기)
            "나유타",   // 10^60  (나유타)
            "불가사의", // 10^64  (불가사의)
            "무량대수", // 10^68  (무량대수)
            "겁",       // 10^72  (겁)
            "업",       // 10^76  (업)
            "무량",     // 10^80  (무량)
            "대수",     // 10^84  (대수)
            "무변",     // 10^88  (무변)
            "무등",     // 10^92  (무등)
            "무상",     // 10^96  (무상)
            "무진",     // 10^100 (무진)
            "무변대",   // 10^104 (무변대)
            "무량무변", // 10^108 (무량무변)
            "무량무등", // 10^112 (무량무등)
            "무량무상", // 10^116 (무량무상)
            "무량무진", // 10^120 (무량무진)
        };

        /// <summary>
        /// 한국어 단위 지수 (4자리 단위)
        /// </summary>
        private const int KOREAN_UNIT_STEP = 4;
        #endregion

        #region English/Short Units (영어 축약 단위)
        /// <summary>
        /// 영어 축약 단위 (3자리 단위)
        /// </summary>
        private static readonly string[] ShortUnits = new string[]
        {
            "",     // 10^0
            "K",    // 10^3   (Thousand)
            "M",    // 10^6   (Million)
            "B",    // 10^9   (Billion)
            "T",    // 10^12  (Trillion)
            "Qa",   // 10^15  (Quadrillion)
            "Qi",   // 10^18  (Quintillion)
            "Sx",   // 10^21  (Sextillion)
            "Sp",   // 10^24  (Septillion)
            "Oc",   // 10^27  (Octillion)
            "No",   // 10^30  (Nonillion)
            "Dc",   // 10^33  (Decillion)
            "UDc",  // 10^36  (Undecillion)
            "DDc",  // 10^39  (Duodecillion)
            "TDc",  // 10^42  (Tredecillion)
            "QaDc", // 10^45  (Quattuordecillion)
            "QiDc", // 10^48  (Quindecillion)
            "SxDc", // 10^51  (Sexdecillion)
            "SpDc", // 10^54  (Septendecillion)
            "OcDc", // 10^57  (Octodecillion)
            "NoDc", // 10^60  (Novemdecillion)
            "Vg",   // 10^63  (Vigintillion)
            "UVg",  // 10^66  (Unvigintillion)
            "DVg",  // 10^69  (Duovigintillion)
            "TVg",  // 10^72  (Trevigintillion)
            "QaVg", // 10^75  (Quattuorvigintillion)
            "QiVg", // 10^78  (Quinvigintillion)
            "SxVg", // 10^81  (Sexvigintillion)
            "SpVg", // 10^84  (Septenvigintillion)
            "OcVg", // 10^87  (Octovigintillion)
            "NoVg", // 10^90  (Novemvigintillion)
            "Tg",   // 10^93  (Trigintillion)
        };

        private const int SHORT_UNIT_STEP = 3;
        #endregion

        #region Korean Format (한국어 포맷)
        /// <summary>
        /// 한국어 단위로 포맷 (예: 1억 2345만)
        /// </summary>
        public static string FormatKorean(BigNumber value, int maxUnits = 2)
        {
            if (value.IsZero) return "0";
            if (value.IsNegative) return "-" + FormatKorean(-value, maxUnits);

            // 작은 숫자는 그대로 표시
            if (value.Exponent < 4)
            {
                return value.ToLong().ToString("N0");
            }

            StringBuilder sb = new StringBuilder();
            int unitsDisplayed = 0;
            int currentExp = value.Exponent;

            // 전체 값을 double로 변환 (근사치)
            double fullValue = value.ToDouble();

            // 각 단위별로 분해
            for (int unitIndex = KoreanUnits.Length - 1; unitIndex >= 0 && unitsDisplayed < maxUnits; unitIndex--)
            {
                int unitExp = unitIndex * KOREAN_UNIT_STEP;

                if (currentExp < unitExp) continue;

                double unitValue = Math.Pow(10, unitExp);
                long unitAmount;

                if (unitExp > 15)
                {
                    // 큰 수는 BigNumber에서 직접 계산
                    BigNumber unitBig = BigNumber.PowerOf10(unitExp);
                    BigNumber divided = value / unitBig;
                    unitAmount = divided.ToLong() % 10000;
                }
                else
                {
                    unitAmount = (long)(fullValue / unitValue) % 10000;
                }

                if (unitAmount > 0 || (unitsDisplayed == 0 && unitIndex == 0))
                {
                    if (sb.Length > 0) sb.Append(" ");

                    if (unitIndex == 0)
                    {
                        sb.Append(unitAmount.ToString("N0"));
                    }
                    else
                    {
                        sb.Append(unitAmount);
                        sb.Append(KoreanUnits[unitIndex]);
                    }

                    unitsDisplayed++;
                }
            }

            return sb.Length > 0 ? sb.ToString() : "0";
        }

        /// <summary>
        /// 한국어 단위로 포맷 (간단 버전, 가장 큰 단위만)
        /// 예: 1.23억, 4.56조
        /// </summary>
        public static string FormatKoreanSimple(BigNumber value, int decimalPlaces = 2)
        {
            if (value.IsZero) return "0";
            if (value.IsNegative) return "-" + FormatKoreanSimple(-value, decimalPlaces);

            // 작은 숫자는 그대로 표시
            if (value.Exponent < 4)
            {
                return value.ToLong().ToString("N0");
            }

            // 단위 인덱스 계산
            int unitIndex = value.Exponent / KOREAN_UNIT_STEP;
            if (unitIndex >= KoreanUnits.Length)
            {
                unitIndex = KoreanUnits.Length - 1;
            }

            // 해당 단위로 나눈 값
            int unitExp = unitIndex * KOREAN_UNIT_STEP;
            BigNumber unitValue = BigNumber.PowerOf10(unitExp);
            BigNumber displayValue = value / unitValue;

            string format = decimalPlaces > 0 ? $"F{decimalPlaces}" : "F0";
            double displayDouble = displayValue.ToDouble();

            // 정수 부분이 4자리 이상이면 다음 단위로
            if (displayDouble >= 10000 && unitIndex < KoreanUnits.Length - 1)
            {
                unitIndex++;
                unitExp = unitIndex * KOREAN_UNIT_STEP;
                unitValue = BigNumber.PowerOf10(unitExp);
                displayValue = value / unitValue;
                displayDouble = displayValue.ToDouble();
            }

            return $"{displayDouble.ToString(format)}{KoreanUnits[unitIndex]}";
        }

        /// <summary>
        /// 한국어 전체 표기 (예: 1억 2345만 6789)
        /// </summary>
        public static string FormatKoreanFull(BigNumber value)
        {
            return FormatKorean(value, int.MaxValue);
        }
        #endregion

        #region Short Format (축약 포맷)
        /// <summary>
        /// 축약 표기로 포맷 (예: 1.23M, 4.56B)
        /// </summary>
        public static string FormatShort(BigNumber value, int decimalPlaces = 2)
        {
            if (value.IsZero) return "0";
            if (value.IsNegative) return "-" + FormatShort(-value, decimalPlaces);

            // 작은 숫자는 그대로 표시
            if (value.Exponent < 3)
            {
                return value.ToLong().ToString("N0");
            }

            // 단위 인덱스 계산
            int unitIndex = value.Exponent / SHORT_UNIT_STEP;
            if (unitIndex >= ShortUnits.Length)
            {
                // 단위를 초과하면 과학적 표기법
                return $"{value.Mantissa:F2}e{value.Exponent}";
            }

            // 해당 단위로 나눈 값
            int unitExp = unitIndex * SHORT_UNIT_STEP;
            BigNumber unitValue = BigNumber.PowerOf10(unitExp);
            BigNumber displayValue = value / unitValue;

            double displayDouble = displayValue.ToDouble();
            string format = decimalPlaces > 0 ? $"F{decimalPlaces}" : "F0";

            return $"{displayDouble.ToString(format)}{ShortUnits[unitIndex]}";
        }
        #endregion

        #region Scientific Format (과학적 표기법)
        /// <summary>
        /// 과학적 표기법 포맷 (예: 1.23e10)
        /// </summary>
        public static string FormatScientific(BigNumber value, int decimalPlaces = 2)
        {
            if (value.IsZero) return "0";

            string format = $"F{decimalPlaces}";
            return $"{value.Mantissa.ToString(format)}e{value.Exponent}";
        }

        /// <summary>
        /// 과학적 표기법 (지수 위첨자) (예: 1.23×10¹⁰)
        /// </summary>
        public static string FormatScientificSuperscript(BigNumber value, int decimalPlaces = 2)
        {
            if (value.IsZero) return "0";

            string format = $"F{decimalPlaces}";
            string expStr = ToSuperscript(value.Exponent);
            return $"{value.Mantissa.ToString(format)}×10{expStr}";
        }

        private static string ToSuperscript(int number)
        {
            string[] superscripts = { "⁰", "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹" };
            string result = "";

            if (number < 0)
            {
                result = "⁻";
                number = -number;
            }

            foreach (char c in number.ToString())
            {
                result += superscripts[c - '0'];
            }

            return result;
        }
        #endregion

        #region Custom Format (커스텀 포맷)
        /// <summary>
        /// 지정된 포맷 타입으로 변환
        /// </summary>
        public static string Format(BigNumber value, CurrencyFormatType formatType, int decimalPlaces = 2)
        {
            return formatType switch
            {
                CurrencyFormatType.Korean => FormatKorean(value),
                CurrencyFormatType.KoreanSimple => FormatKoreanSimple(value, decimalPlaces),
                CurrencyFormatType.KoreanFull => FormatKoreanFull(value),
                CurrencyFormatType.Short => FormatShort(value, decimalPlaces),
                CurrencyFormatType.Scientific => FormatScientific(value, decimalPlaces),
                CurrencyFormatType.ScientificSuperscript => FormatScientificSuperscript(value, decimalPlaces),
                CurrencyFormatType.Full => FormatFull(value),
                _ => value.ToString()
            };
        }

        /// <summary>
        /// 전체 숫자 표시 (작은 수만)
        /// </summary>
        public static string FormatFull(BigNumber value)
        {
            if (value.Exponent > 15)
            {
                return FormatScientific(value);
            }
            return value.ToDouble().ToString("N0");
        }
        #endregion

        #region Percentage Format (퍼센트 포맷)
        /// <summary>
        /// 퍼센트로 포맷 (0.1 -> 10%)
        /// </summary>
        public static string FormatPercent(BigNumber value, int decimalPlaces = 1)
        {
            double percent = value.ToDouble() * 100;
            string format = $"F{decimalPlaces}";
            return $"{percent.ToString(format)}%";
        }

        /// <summary>
        /// 퍼센트로 포맷 (BigNumber 비율)
        /// </summary>
        public static string FormatPercent(BigNumber current, BigNumber max, int decimalPlaces = 1)
        {
            if (max.IsZero) return "0%";
            BigNumber ratio = current / max;
            return FormatPercent(ratio, decimalPlaces);
        }
        #endregion

        #region Time Format (시간 포맷)
        /// <summary>
        /// 초를 시간 형식으로 포맷 (예: 1:23:45)
        /// </summary>
        public static string FormatTime(BigNumber seconds)
        {
            long totalSeconds = seconds.ToLong();

            if (totalSeconds < 0) return "0:00";
            if (totalSeconds < 60) return $"0:{totalSeconds:D2}";
            if (totalSeconds < 3600)
            {
                long min = totalSeconds / 60;
                long sec = totalSeconds % 60;
                return $"{min}:{sec:D2}";
            }

            long hours = totalSeconds / 3600;
            long mins = (totalSeconds % 3600) / 60;
            long secs = totalSeconds % 60;

            if (hours < 100)
            {
                return $"{hours}:{mins:D2}:{secs:D2}";
            }

            // 100시간 이상
            return $"{FormatKoreanSimple(new BigNumber(hours), 0)}시간";
        }
        #endregion

        #region Comparison Format (비교 포맷)
        /// <summary>
        /// 변화량 포맷 (예: +1.5만, -3.2억)
        /// </summary>
        public static string FormatChange(BigNumber value, CurrencyFormatType formatType = CurrencyFormatType.KoreanSimple)
        {
            if (value.IsZero) return "0";

            string prefix = value.IsNegative ? "" : "+";
            return prefix + Format(value, formatType);
        }

        /// <summary>
        /// 비율 변화 포맷 (예: +15%, -3.2%)
        /// </summary>
        public static string FormatPercentChange(float ratio, int decimalPlaces = 1)
        {
            string prefix = ratio >= 0 ? "+" : "";
            string format = $"F{decimalPlaces}";
            return $"{prefix}{(ratio * 100).ToString(format)}%";
        }
        #endregion

        #region Unit Information (단위 정보)
        /// <summary>
        /// 특정 지수의 한국어 단위 가져오기
        /// </summary>
        public static string GetKoreanUnit(int exponent)
        {
            int unitIndex = exponent / KOREAN_UNIT_STEP;
            if (unitIndex < 0 || unitIndex >= KoreanUnits.Length)
                return "";
            return KoreanUnits[unitIndex];
        }

        /// <summary>
        /// 특정 지수의 영어 단위 가져오기
        /// </summary>
        public static string GetShortUnit(int exponent)
        {
            int unitIndex = exponent / SHORT_UNIT_STEP;
            if (unitIndex < 0 || unitIndex >= ShortUnits.Length)
                return "";
            return ShortUnits[unitIndex];
        }

        /// <summary>
        /// 한국어 단위 목록 가져오기
        /// </summary>
        public static string[] GetKoreanUnits() => KoreanUnits;

        /// <summary>
        /// 영어 단위 목록 가져오기
        /// </summary>
        public static string[] GetShortUnits() => ShortUnits;
        #endregion

        #region Color Formatting (컬러 포맷)
        /// <summary>
        /// 변화량에 따른 색상 태그 추가 (TextMeshPro용)
        /// </summary>
        public static string FormatWithColor(BigNumber value, CurrencyFormatType formatType = CurrencyFormatType.KoreanSimple)
        {
            string formatted = Format(value, formatType);

            if (value.IsZero)
                return $"<color=#FFFFFF>{formatted}</color>";
            if (value.IsNegative)
                return $"<color=#FF4444>{formatted}</color>";
            return $"<color=#44FF44>+{formatted}</color>";
        }

        /// <summary>
        /// 골드 색상으로 포맷
        /// </summary>
        public static string FormatGold(BigNumber value, CurrencyFormatType formatType = CurrencyFormatType.KoreanSimple)
        {
            string formatted = Format(value, formatType);
            return $"<color=#FFD700>{formatted}</color>";
        }

        /// <summary>
        /// 데미지 색상으로 포맷
        /// </summary>
        public static string FormatDamage(BigNumber value, bool isCritical = false, CurrencyFormatType formatType = CurrencyFormatType.KoreanSimple)
        {
            string formatted = Format(value, formatType);

            if (isCritical)
                return $"<color=#FF8800><size=120%>{formatted}!</size></color>";
            return $"<color=#FFFFFF>{formatted}</color>";
        }
        #endregion
    }

    /// <summary>
    /// 재화 포맷 타입
    /// </summary>
    public enum CurrencyFormatType
    {
        /// <summary>한국어 단위 (1억 2345만)</summary>
        Korean,
        /// <summary>한국어 간단 (1.23억)</summary>
        KoreanSimple,
        /// <summary>한국어 전체 (1억 2345만 6789)</summary>
        KoreanFull,
        /// <summary>축약 (1.23M)</summary>
        Short,
        /// <summary>과학적 표기법 (1.23e8)</summary>
        Scientific,
        /// <summary>과학적 위첨자 (1.23×10⁸)</summary>
        ScientificSuperscript,
        /// <summary>전체 숫자</summary>
        Full
    }
}
