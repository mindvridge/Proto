using System;
using UnityEngine;

namespace HiddenGrowth.Data
{
    /// <summary>
    /// 무한대에 가까운 큰 숫자를 처리하는 클래스
    /// 가수(mantissa) + 지수(exponent) 방식 사용
    /// 예: 1.5e100 = mantissa:1.5, exponent:100
    /// </summary>
    [Serializable]
    public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>
    {
        #region Constants
        public static readonly BigNumber Zero = new BigNumber(0, 0);
        public static readonly BigNumber One = new BigNumber(1, 0);
        public static readonly BigNumber Ten = new BigNumber(1, 1);
        public static readonly BigNumber Hundred = new BigNumber(1, 2);
        public static readonly BigNumber Thousand = new BigNumber(1, 3);
        public static readonly BigNumber MaxValue = new BigNumber(9.999999, int.MaxValue);

        private const double MANTISSA_MIN = 1.0;
        private const double MANTISSA_MAX = 10.0;
        private const double EPSILON = 1e-10;
        #endregion

        #region Fields
        [SerializeField] private double mantissa;  // 1.0 ~ 9.999999
        [SerializeField] private int exponent;     // 10의 지수
        #endregion

        #region Properties
        public double Mantissa => mantissa;
        public int Exponent => exponent;

        /// <summary>
        /// 0인지 확인
        /// </summary>
        public bool IsZero => Math.Abs(mantissa) < EPSILON;

        /// <summary>
        /// 음수인지 확인
        /// </summary>
        public bool IsNegative => mantissa < 0;

        /// <summary>
        /// long 범위 내인지 확인
        /// </summary>
        public bool FitsInLong => exponent < 19;

        /// <summary>
        /// double로 변환 가능한지 확인
        /// </summary>
        public bool FitsInDouble => exponent < 308;
        #endregion

        #region Constructors
        public BigNumber(double value)
        {
            if (value == 0 || double.IsNaN(value))
            {
                mantissa = 0;
                exponent = 0;
                return;
            }

            if (double.IsInfinity(value))
            {
                mantissa = value > 0 ? 9.999999 : -9.999999;
                exponent = int.MaxValue;
                return;
            }

            bool isNegative = value < 0;
            value = Math.Abs(value);

            exponent = (int)Math.Floor(Math.Log10(value));
            mantissa = value / Math.Pow(10, exponent);

            if (isNegative) mantissa = -mantissa;

            Normalize();
        }

        public BigNumber(double mantissa, int exponent)
        {
            this.mantissa = mantissa;
            this.exponent = exponent;
            Normalize();
        }

        public BigNumber(long value) : this((double)value) { }
        #endregion

        #region Normalization
        /// <summary>
        /// 정규화: mantissa를 1.0 ~ 9.999999 범위로 조정
        /// </summary>
        private void Normalize()
        {
            if (Math.Abs(mantissa) < EPSILON)
            {
                mantissa = 0;
                exponent = 0;
                return;
            }

            bool isNegative = mantissa < 0;
            mantissa = Math.Abs(mantissa);

            while (mantissa >= MANTISSA_MAX && exponent < int.MaxValue)
            {
                mantissa /= 10;
                exponent++;
            }

            while (mantissa < MANTISSA_MIN && mantissa > 0 && exponent > int.MinValue)
            {
                mantissa *= 10;
                exponent--;
            }

            // 정밀도 유지
            mantissa = Math.Round(mantissa, 9);

            if (isNegative) mantissa = -mantissa;
        }
        #endregion

        #region Arithmetic Operations
        /// <summary>
        /// 덧셈
        /// </summary>
        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.IsZero) return b;
            if (b.IsZero) return a;

            // 지수 차이가 너무 크면 큰 쪽 반환
            int expDiff = a.exponent - b.exponent;
            if (expDiff > 15) return a;
            if (expDiff < -15) return b;

            // 지수를 맞추고 덧셈
            double adjustedA = a.mantissa * Math.Pow(10, a.exponent);
            double adjustedB = b.mantissa * Math.Pow(10, b.exponent);

            if (a.exponent > 15 || b.exponent > 15)
            {
                // 큰 수는 지수를 맞춰서 계산
                int maxExp = Math.Max(a.exponent, b.exponent);
                adjustedA = a.mantissa * Math.Pow(10, a.exponent - maxExp);
                adjustedB = b.mantissa * Math.Pow(10, b.exponent - maxExp);
                return new BigNumber(adjustedA + adjustedB, maxExp);
            }

            return new BigNumber(adjustedA + adjustedB);
        }

        /// <summary>
        /// 뺄셈
        /// </summary>
        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            return a + new BigNumber(-b.mantissa, b.exponent);
        }

        /// <summary>
        /// 곱셈
        /// </summary>
        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            if (a.IsZero || b.IsZero) return Zero;

            double newMantissa = a.mantissa * b.mantissa;
            int newExponent = a.exponent + b.exponent;

            return new BigNumber(newMantissa, newExponent);
        }

        /// <summary>
        /// 나눗셈
        /// </summary>
        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b.IsZero)
            {
                Debug.LogError("[BigNumber] Division by zero!");
                return Zero;
            }
            if (a.IsZero) return Zero;

            double newMantissa = a.mantissa / b.mantissa;
            int newExponent = a.exponent - b.exponent;

            return new BigNumber(newMantissa, newExponent);
        }

        /// <summary>
        /// double과 곱셈
        /// </summary>
        public static BigNumber operator *(BigNumber a, double b)
        {
            return a * new BigNumber(b);
        }

        public static BigNumber operator *(double a, BigNumber b)
        {
            return new BigNumber(a) * b;
        }

        /// <summary>
        /// double과 나눗셈
        /// </summary>
        public static BigNumber operator /(BigNumber a, double b)
        {
            return a / new BigNumber(b);
        }

        /// <summary>
        /// long과 연산
        /// </summary>
        public static BigNumber operator +(BigNumber a, long b) => a + new BigNumber(b);
        public static BigNumber operator +(long a, BigNumber b) => new BigNumber(a) + b;
        public static BigNumber operator -(BigNumber a, long b) => a - new BigNumber(b);
        public static BigNumber operator *(BigNumber a, long b) => a * new BigNumber(b);
        public static BigNumber operator /(BigNumber a, long b) => a / new BigNumber(b);

        /// <summary>
        /// 단항 마이너스
        /// </summary>
        public static BigNumber operator -(BigNumber a)
        {
            return new BigNumber(-a.mantissa, a.exponent);
        }

        /// <summary>
        /// 증가/감소
        /// </summary>
        public static BigNumber operator ++(BigNumber a) => a + One;
        public static BigNumber operator --(BigNumber a) => a - One;
        #endregion

        #region Comparison Operations
        public static bool operator ==(BigNumber a, BigNumber b)
        {
            if (a.IsZero && b.IsZero) return true;
            return a.exponent == b.exponent && Math.Abs(a.mantissa - b.mantissa) < EPSILON;
        }

        public static bool operator !=(BigNumber a, BigNumber b) => !(a == b);

        public static bool operator >(BigNumber a, BigNumber b)
        {
            if (a.IsNegative != b.IsNegative)
                return !a.IsNegative;

            if (a.exponent != b.exponent)
                return a.IsNegative ? a.exponent < b.exponent : a.exponent > b.exponent;

            return a.IsNegative ? a.mantissa < b.mantissa : a.mantissa > b.mantissa;
        }

        public static bool operator <(BigNumber a, BigNumber b) => b > a;
        public static bool operator >=(BigNumber a, BigNumber b) => a > b || a == b;
        public static bool operator <=(BigNumber a, BigNumber b) => a < b || a == b;

        public int CompareTo(BigNumber other)
        {
            if (this > other) return 1;
            if (this < other) return -1;
            return 0;
        }
        #endregion

        #region Mathematical Functions
        /// <summary>
        /// 제곱
        /// </summary>
        public BigNumber Pow(int power)
        {
            if (power == 0) return One;
            if (power == 1) return this;
            if (IsZero) return Zero;

            double newMantissa = Math.Pow(mantissa, power);
            int newExponent = exponent * power;

            return new BigNumber(newMantissa, newExponent);
        }

        /// <summary>
        /// 제곱근
        /// </summary>
        public BigNumber Sqrt()
        {
            if (IsZero) return Zero;
            if (IsNegative)
            {
                Debug.LogError("[BigNumber] Cannot sqrt negative number!");
                return Zero;
            }

            double adjustedMantissa = mantissa;
            int adjustedExponent = exponent;

            // 지수가 홀수면 조정
            if (adjustedExponent % 2 != 0)
            {
                adjustedMantissa *= 10;
                adjustedExponent--;
            }

            return new BigNumber(Math.Sqrt(adjustedMantissa), adjustedExponent / 2);
        }

        /// <summary>
        /// 로그10
        /// </summary>
        public double Log10()
        {
            if (IsZero || IsNegative) return double.NegativeInfinity;
            return Math.Log10(mantissa) + exponent;
        }

        /// <summary>
        /// 자연로그
        /// </summary>
        public double Log()
        {
            return Log10() * Math.Log(10);
        }

        /// <summary>
        /// 절대값
        /// </summary>
        public BigNumber Abs()
        {
            return new BigNumber(Math.Abs(mantissa), exponent);
        }

        /// <summary>
        /// 내림
        /// </summary>
        public BigNumber Floor()
        {
            if (exponent < 0) return IsNegative ? new BigNumber(-1) : Zero;
            if (exponent > 15) return this;

            double value = ToDouble();
            return new BigNumber(Math.Floor(value));
        }

        /// <summary>
        /// 올림
        /// </summary>
        public BigNumber Ceiling()
        {
            if (exponent < 0) return IsNegative ? Zero : One;
            if (exponent > 15) return this;

            double value = ToDouble();
            return new BigNumber(Math.Ceiling(value));
        }

        /// <summary>
        /// 최대값
        /// </summary>
        public static BigNumber Max(BigNumber a, BigNumber b) => a > b ? a : b;

        /// <summary>
        /// 최소값
        /// </summary>
        public static BigNumber Min(BigNumber a, BigNumber b) => a < b ? a : b;

        /// <summary>
        /// 범위 제한
        /// </summary>
        public static BigNumber Clamp(BigNumber value, BigNumber min, BigNumber max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 선형 보간
        /// </summary>
        public static BigNumber Lerp(BigNumber a, BigNumber b, float t)
        {
            t = Mathf.Clamp01(t);
            return a + (b - a) * t;
        }

        /// <summary>
        /// 퍼센트 계산
        /// </summary>
        public BigNumber Percent(float percent)
        {
            return this * (percent / 100f);
        }
        #endregion

        #region Conversion
        /// <summary>
        /// double로 변환
        /// </summary>
        public double ToDouble()
        {
            if (IsZero) return 0;
            if (exponent > 307) return mantissa > 0 ? double.MaxValue : double.MinValue;
            if (exponent < -307) return 0;

            return mantissa * Math.Pow(10, exponent);
        }

        /// <summary>
        /// long으로 변환
        /// </summary>
        public long ToLong()
        {
            if (IsZero) return 0;
            if (exponent > 18) return mantissa > 0 ? long.MaxValue : long.MinValue;
            if (exponent < 0) return 0;

            return (long)(mantissa * Math.Pow(10, exponent));
        }

        /// <summary>
        /// float로 변환
        /// </summary>
        public float ToFloat()
        {
            return (float)ToDouble();
        }

        /// <summary>
        /// 암시적 변환
        /// </summary>
        public static implicit operator BigNumber(long value) => new BigNumber(value);
        public static implicit operator BigNumber(int value) => new BigNumber(value);
        public static implicit operator BigNumber(double value) => new BigNumber(value);
        public static implicit operator BigNumber(float value) => new BigNumber(value);

        /// <summary>
        /// 명시적 변환
        /// </summary>
        public static explicit operator long(BigNumber value) => value.ToLong();
        public static explicit operator double(BigNumber value) => value.ToDouble();
        public static explicit operator float(BigNumber value) => value.ToFloat();
        #endregion

        #region String Conversion
        /// <summary>
        /// 기본 문자열 변환 (과학적 표기법)
        /// </summary>
        public override string ToString()
        {
            if (IsZero) return "0";

            if (exponent < 6 && exponent >= 0)
            {
                return ToDouble().ToString("N0");
            }

            return $"{mantissa:F2}e{exponent}";
        }

        /// <summary>
        /// 지정된 자릿수로 문자열 변환
        /// </summary>
        public string ToString(int decimalPlaces)
        {
            if (IsZero) return "0";

            if (exponent < 6 && exponent >= 0)
            {
                return ToDouble().ToString($"N{decimalPlaces}");
            }

            string format = $"F{decimalPlaces}";
            return $"{mantissa.ToString(format)}e{exponent}";
        }

        /// <summary>
        /// 한국어 단위로 변환 (CurrencyFormatter 사용 권장)
        /// </summary>
        public string ToKoreanString()
        {
            return CurrencyFormatter.FormatKorean(this);
        }

        /// <summary>
        /// 축약 표기 (K, M, B, T, ...)
        /// </summary>
        public string ToShortString()
        {
            return CurrencyFormatter.FormatShort(this);
        }
        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            if (obj is BigNumber other)
                return this == other;
            return false;
        }

        public bool Equals(BigNumber other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(mantissa, exponent);
        }
        #endregion

        #region Serialization
        /// <summary>
        /// JSON 직렬화용 문자열
        /// </summary>
        public string Serialize()
        {
            return $"{mantissa}:{exponent}";
        }

        /// <summary>
        /// JSON 역직렬화
        /// </summary>
        public static BigNumber Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data)) return Zero;

            string[] parts = data.Split(':');
            if (parts.Length != 2) return Zero;

            if (double.TryParse(parts[0], out double m) &&
                int.TryParse(parts[1], out int e))
            {
                return new BigNumber(m, e);
            }

            return Zero;
        }
        #endregion

        #region Utility
        /// <summary>
        /// 10의 거듭제곱 생성
        /// </summary>
        public static BigNumber PowerOf10(int exponent)
        {
            return new BigNumber(1, exponent);
        }

        /// <summary>
        /// 랜덤 BigNumber 생성 (범위 내)
        /// </summary>
        public static BigNumber Random(BigNumber min, BigNumber max)
        {
            if (min >= max) return min;

            float t = UnityEngine.Random.value;
            return Lerp(min, max, t);
        }

        /// <summary>
        /// 문자열에서 파싱
        /// </summary>
        public static BigNumber Parse(string value)
        {
            if (string.IsNullOrEmpty(value)) return Zero;

            // 과학적 표기법 처리
            if (value.Contains("e") || value.Contains("E"))
            {
                if (double.TryParse(value, out double d))
                    return new BigNumber(d);
            }

            // 일반 숫자
            if (double.TryParse(value.Replace(",", ""), out double result))
                return new BigNumber(result);

            return Zero;
        }

        /// <summary>
        /// 파싱 시도
        /// </summary>
        public static bool TryParse(string value, out BigNumber result)
        {
            result = Parse(value);
            return !result.IsZero || value == "0";
        }
        #endregion
    }
}
