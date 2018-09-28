﻿using Cauldron.Convertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_UWP

using System.Reflection;

#endif

namespace Cauldron
{
    /// <summary>
    /// Provides usefull extension methods
    /// </summary>
    public static class ExtensionsConvertions
    {
        private static CultureInfo cultureInfo;
        private static NumberFormatInfo numberFormatInfoEnUs;

        static ExtensionsConvertions()
        {
#if WINDOWS_UWP
            cultureInfo = new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.Languages[0]);
#else
            cultureInfo = CultureInfo.CurrentCulture;
#endif
            numberFormatInfoEnUs = new CultureInfo("en-US").NumberFormat;
        }

        /// <summary>
        /// Performs a cast between compatible reference types. If a convertion is not possible then
        /// null is returned. As a last resort it will use <see
        /// cref="System.Convert.ChangeType(object, Type)"/>.
        /// <para/>
        /// Tries to use the implicit and explicit operators if exists when convertion with 'as'
        /// returns null.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to convert to</typeparam>
        /// <param name="source">The object to convert</param>
        /// <returns>The object casted to <typeparamref name="T"/></returns>
        /// <example>
        /// In the following code example, the 'As' extension is used to convert a returned object
        /// via the implicit operator.
        /// <code>
        /// public interface IFoo
        /// {
        ///     string Name {get;}
        ///     string Description {get;}
        /// }
        ///
        /// public class Foo : IFoo
        /// {
        ///     public string Name {get; set;}
        ///     public string Description {get; set;}
        /// }
        ///
        /// public class Bar : BarBase
        /// {
        ///     private IFoo internalFoo;
        ///
        ///     private Bar(IFoo foo)
        ///     {
        ///         this.internalFoo = foo;
        ///     }
        ///
        ///     public void DoSomeStuff()
        ///     {
        ///     }
        ///
        ///     public void DoSomeOtherStuff()
        ///     {
        ///     }
        ///
        ///     public static implicit operator Bar(Foo value) =&gt; new Boo(value);
        ///     public static implicit operator Foo(Bar value) =&gt; value.internalFoo;
        /// }
        ///
        /// public class SomeOtherClass
        /// {
        ///     public IFoo GetFooFromSomewhere(string fooId) =&gt; new Foo { Name = "A Foo", Description = "This is the foo you are looking for." };
        /// }
        /// </code>
        /// The code can be called like following:
        /// <code>
        /// var bar = someOtherClassInstance.GetFooFromSomewhere("fooThatINeed").As&lt;Bar&gt;();
        /// </code>
        /// </example>
        public static T As<T>(this object source) where T : class
        {
            var result = source as T;

            if (result == null && source != null)
            {
                var method = ImplicitExplicitConvertionCache.Get(source.GetType(), typeof(T));
                if (method != null)
                    return method(source) as T;

#if NETFX_CORE
                if (typeof(T).GetTypeInfo().IsEnum && source != null)
#else
                if (typeof(T).IsEnum && source != null)
#endif
                    return Enum.Parse(typeof(T), source.ToString()) as T;

                return System.Convert.ChangeType(source, typeof(T)) as T;
            }

            return result;
        }

        /// <summary>
        /// Converts a type using the implicit or explicit operators. If both fails it will try to
        /// convert the value with <see cref="System.Convert.ChangeType(object, Type)"/>.
        /// </summary>
        /// <param name="source">The object to convert</param>
        /// <param name="sourceType">The type of the object to convert</param>
        /// <param name="targetType">The type to convert to</param>
        /// <returns>A new instance of <paramref name="targetType"/>.</returns>
        /// <example>
        /// <code>
        /// public class Bla
        /// {
        ///     public string Value { get; set; }
        ///
        ///     public static implicit operator Bla(string value) =&gt; new Bla { Value = value };
        ///
        ///     public static implicit operator Bla(int value) =&gt; new Bla { Value = value.ToString() };
        /// }
        /// </code>
        /// The code can be called like following:
        /// <code>
        /// var bar = "Test Test".As(typeof(string), typeof(Bla));
        /// </code>
        /// </example>
        public static object As(this object source, Type sourceType, Type targetType)
        {
            var method = ImplicitExplicitConvertionCache.Get(sourceType, targetType);
            if (method != null)
                return method(source);

#if NETFX_CORE
            if (targetType.GetTypeInfo().IsEnum && source != null)
#else
            if (targetType.IsEnum && source != null)
#endif
                return Enum.Parse(targetType, source.ToString());

            return System.Convert.ChangeType(source, targetType);
        }

        /// <summary>
        /// Converts a type using the implicit or explicit operators. If both fails it will try to
        /// convert the value with <see cref="System.Convert.ChangeType(object, Type)"/>.
        /// </summary>
        /// <param name="source">The object to convert</param>
        /// <param name="targetType">The type to convert to</param>
        /// <returns>A new instance of <paramref name="targetType"/>.</returns>
        /// <example>
        /// <code>
        /// public class Bla
        /// {
        ///     public string Value { get; set; }
        ///
        ///     public static implicit operator Bla(string value) =&gt; new Bla { Value = value };
        ///
        ///     public static implicit operator Bla(int value) =&gt; new Bla { Value = value.ToString() };
        /// }
        /// </code>
        /// The code can be called like following:
        /// <code>
        /// var bar = "Test Test".As(typeof(Bla));
        /// </code>
        /// </example>
        public static object As(this object source, Type targetType)
        {
            if (source != null)
            {
                var method = ImplicitExplicitConvertionCache.Get(source.GetType(), targetType);
                if (method != null)
                    return method(source);

#if NETFX_CORE
                if (targetType.GetTypeInfo().IsEnum && source != null)
#else
                if (targetType.IsEnum && source != null)
#endif
                    return Enum.Parse(targetType, source.ToString());

                return System.Convert.ChangeType(source, targetType);
            }

            return null;
        }

        /// <summary>
        /// Converts a <see cref="IDictionary{TKey, TValue}"/> to a <see
        /// cref="ReadOnlyDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to wrap.</param>
        /// <returns>A new instance of <see cref="ReadOnlyDictionary{TKey, TValue}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null</exception>
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Converts a string to the type defined by <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to convert the string to</typeparam>
        /// <param name="value">The string value to convert</param>
        /// <returns>The converted value</returns>
        public static T Convert<T>(this string value) =>
            (T)value.Convert(typeof(T), cultureInfo.NumberFormat);

        /// <summary>
        /// Converts a string to the type defined by <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to convert the string to</typeparam>
        /// <param name="value">The string value to convert</param>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="value"/>.
        /// </param>
        /// <returns>The converted value</returns>
        public static T Convert<T>(this string value, NumberFormatInfo numberformat) =>
            (T)value.Convert(typeof(T), numberformat);

        /// <summary>
        /// Converts a string to the type defined by <paramref name="targetType"/>
        /// </summary>
        /// <param name="value">The string value to convert</param>
        /// <param name="targetType">The type to convert the string to</param>
        /// <returns>The converted value</returns>
        public static object Convert(this string value, Type targetType) =>
            value.Convert(targetType, cultureInfo.NumberFormat);

        /// <summary>
        /// Converts a string to the type defined by <paramref name="targetType"/>
        /// </summary>
        /// <param name="value">The string value to convert</param>
        /// <param name="targetType">The type to convert the string to</param>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="value"/>.
        /// </param>
        /// <returns>The converted value</returns>
        public static object Convert(this string value, Type targetType, NumberFormatInfo numberformat)
        {
            /*
             TypeConverter does not exist in UWP that is why we built our own
             */

#if WINDOWS_UWP || NETCORE
            if ((targetType.IsNullable() || !targetType.GetTypeInfo().IsValueType) && string.IsNullOrEmpty(value))
#else
            if ((targetType.IsNullable() || !targetType.IsValueType) && string.IsNullOrEmpty(value))
#endif
                return null;

            if (string.IsNullOrEmpty(value)) /* This is for value types ... This will always return false for non value types*/
                return targetType.GetDefaultInstance();

            if (targetType.IsNullable())
                targetType = Nullable.GetUnderlyingType(targetType);

#if WINDOWS_UWP || NETCORE
            if (targetType.GetTypeInfo().IsEnum)
#else
            if (targetType.IsEnum)
#endif
            {
                if (value.All(char.IsDigit))
                    return Enum.ToObject(targetType, value.ToLong());
                else
                    return Enum.Parse(targetType, value);
            }

            if (targetType == typeof(string)) return value;
            if (targetType == typeof(int)) return value.ToInteger(numberformat);
            if (targetType == typeof(uint)) return value.ToUInteger(numberformat);
            if (targetType == typeof(long)) return value.ToLong(numberformat);
            if (targetType == typeof(ulong)) return value.ToULong(numberformat);
            if (targetType == typeof(byte)) return value == "" ? (byte)0 : (byte)value[0];
            if (targetType == typeof(sbyte)) return value == "" ? (sbyte)0 : (sbyte)value[0];
            if (targetType == typeof(float)) return value.ToFloat(numberformat);
            if (targetType == typeof(double)) return value.ToDouble(numberformat);
            if (targetType == typeof(decimal)) return value.ToDecimal(numberformat);
            if (targetType == typeof(bool)) return value.ToBool();
            if (targetType == typeof(char)) return value == "" ? (char)0 : value[0];
            if (targetType == typeof(short)) return value.ToShort(numberformat);
            if (targetType == typeof(ushort)) return value.ToUShort(numberformat);
            if (targetType == typeof(IntPtr)) return (IntPtr)value.ToInteger(numberformat);
            if (targetType == typeof(UIntPtr)) return (UIntPtr)value.ToUInteger(numberformat);
            if (targetType == typeof(DateTime)) return DateTime.Parse(value);
            if (targetType == typeof(DateTimeOffset)) return DateTimeOffset.Parse(value);
            if (targetType == typeof(TimeSpan)) return TimeSpan.Parse(value);
            if (targetType == typeof(Guid)) return Guid.Parse(value);

            var op = ImplicitExplicitConvertionCache.Get(typeof(string), targetType);
            if (op != null)
                return op(value);

            return targetType.GetDefaultInstance();
        }

        /// <summary>
        /// Converts a string from a encoding to another encoding
        /// </summary>
        /// <param name="source">The string to convert</param>
        /// <param name="from">The source strings encoding</param>
        /// <param name="to">The target encoding</param>
        /// <returns>The converted string</returns>
        public static string Convert(this string source, Encodings from, Encodings to)
        {
            var toEncoding = ToEncoding(to);
            return toEncoding.GetString(source.ConvertToBytes(from, to));
        }

        /// <summary>
        /// Converts a string represented by a byte array from a encoding to another encoding
        /// </summary>
        /// <param name="source">The string to convert</param>
        /// <param name="from">The source strings encoding</param>
        /// <param name="to">The target encoding</param>
        /// <returns>The converted string</returns>
        public static string Convert(this byte[] source, Encodings from, Encodings to)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var fromEncoding = ToEncoding(from);
            var toEncoding = ToEncoding(to);

            return toEncoding.GetString(Encoding.Convert(fromEncoding, toEncoding, source));
        }

        /// <summary>
        /// Converts the readable escaped chars in a string to its equivalent char. This includes
        /// simple-escape-sequences such as \' \" \\ \0 \a \b \f \n \r \t \v
        /// </summary>
        /// <param name="source">The source string to convert</param>
        /// <returns>The converted string</returns>
        public static string ConvertEscapeSequences(this string source)
        {
            var readableEscapedChars = new string[] { @"\'", @"\""", @"\\", @"\0", @"\a", @"\b", @"\f", @"\n", @"\r", @"\t", @"\v" };
            var escapedChars = new string[] { "\'", "\"", "\\", "\0", "\a", "\b", "\f", "\n", "\r", "\t", "\v" };

            // we are explicitly not using string replace on this to lessen the GC load on such an
            // easy thing like replacement

            var result = new char[source.Length];
            int indexer = 0;

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == '\\' && i + 1 < result.Length)
                {
                    var nextChar = source[i + 1];

                    switch (nextChar)
                    {
                        case '\'': result[indexer] = '\''; i++; break;
                        case '"': result[indexer] = '"'; i++; break;
                        case '\\': result[indexer] = '\\'; i++; break;
                        case '0': result[indexer] = '\0'; i++; break;
                        case 'a': result[indexer] = '\a'; i++; break;
                        case 'b': result[indexer] = '\b'; i++; break;
                        case 'f': result[indexer] = '\f'; i++; break;
                        case 'n': result[indexer] = '\n'; i++; break;
                        case 'r': result[indexer] = '\r'; i++; break;
                        case 't': result[indexer] = '\t'; i++; break;
                        case 'v': result[indexer] = '\v'; i++; break;
                        default: result[indexer] = nextChar; break;
                    }
                }
                else
                    result[indexer] = source[i];

                indexer++;
            }

            return new string(result, 0, indexer);
        }

        /// <summary>
        /// Converts a string from a encoding to another encoding
        /// </summary>
        /// <param name="source">The string to convert</param>
        /// <param name="from">The source strings encoding</param>
        /// <param name="to">The target encoding</param>
        /// <returns>The converted string</returns>
        public static byte[] ConvertToBytes(this string source, Encodings from, Encodings to)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var fromEncoding = ToEncoding(from);
            var toEncoding = ToEncoding(to);

            return Encoding.Convert(fromEncoding, toEncoding, fromEncoding.GetBytes(source));
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable"/> to an array
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable"/> to convert</param>
        /// <param name="elementType">The element type contained in the <see cref="IEnumerable"/></param>
        /// <returns>An array of <paramref name="elementType"/></returns>
        /// <exception cref="ArgumentNullException">source is null</exception>
        public static object ToArray(this IEnumerable source, Type elementType)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            #region Count the elements in the source

            int sourceCount = 0;
            var collection = source as ICollection;
            if (collection != null)
                sourceCount = collection.Count;
            else
            {
                var enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                    sourceCount++;
                (enumerator as IDisposable)?.Dispose();
            }

            #endregion Count the elements in the source

            var result = Array.CreateInstance(elementType, sourceCount);
            var index = 0;

            foreach (var item in source)
                result.SetValue(item, index++);

            return result;
        }

        /// <summary>
        /// Converts a string to its equivalent string representation that is encoded with base-64 digits.
        /// </summary>
        /// <param name="source">The string to convert</param>
        /// <returns>The string representation, in base 64</returns>
        /// <exception cref="ArgumentNullException">source string is null</exception>
        public static string ToBase64String(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var b = Encoding.UTF8.GetBytes(source);
            return System.Convert.ToBase64String(b);
        }

        /// <summary>
        /// Converts a string to bool.
        /// <para/>
        /// This will first try to compare the string to "true" and then to "false". If both fails
        /// then it will use <see cref="bool.TryParse(string, out bool)"/> to parse the string to
        /// bool. If that also fails then the string will be compared to "1".
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>Returns true if the string is equivalent to true; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        public static bool ToBool(this string target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                return true;
            else if (target.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (target.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                return true;
            else if (target.Equals("N", StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (bool.TryParse(target, out bool val))
                return val;

            if (target == "1")
                return true;

            return false;
        }

        /// <summary>
        /// Converts the value to a byte If convertion fails the value will always be 0
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>The byte value of the string</returns>
        public static byte ToByte(this string target)
        {
            if (byte.TryParse(target, out byte value))
                return value;

            return 0;
        }

        /// <summary>
        /// Converts the value to a byte array
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>The <paramref name="target"/> as an array of bytes</returns>
        public static byte[] ToBytes(this uint target) => BitConverter.GetBytes(target);

        /// <summary>
        /// Converts the value to a byte array
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>The <paramref name="target"/> as an array of bytes</returns>
        public static byte[] ToBytes(this int target) => BitConverter.GetBytes(target);

        /// <summary>
        /// Converts the value to a byte array
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>The <paramref name="target"/> as an array of bytes</returns>
        public static byte[] ToBytes(this long target) => BitConverter.GetBytes(target);

        /// <summary>
        /// Converts the value to a byte array
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>The <paramref name="target"/> as an array of bytes</returns>
        public static byte[] ToBytes(this double target) => BitConverter.GetBytes(target);

        /// <summary>
        /// Converts the value to a byte array
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>The <paramref name="target"/> as an array of bytes</returns>
        public static byte[] ToBytes(this float target) => BitConverter.GetBytes(target);

        /// <summary>
        /// Converts a <see cref="Stream"/> to <see cref="byte"/> array. If the stream is not
        /// seekable, then this will use <see cref="StreamReader.ReadToEndAsync"/> to get the stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to convert</param>
        /// <returns>An array of bytes</returns>
        /// <exception cref="ArgumentNullException">
        /// Parameter <paramref name="stream"/> is null
        /// </exception>
        public static async Task<byte[]> ToBytesAsync(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanSeek)
            {
                using (var reader = new StreamReader(stream))
                {
                    var content = await reader.ReadToEndAsync();
                    stream.Dispose();

                    using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        return memoryStream.ToArray();
                    }
                }
            }

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.SetLength(stream.Length);
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Converts the value to a char If convertion fails the value will always be '\0'
        /// </summary>
        /// <param name="target">The value to convert</param>
        /// <returns>The char value of the string</returns>
        public static char ToChar(this string target)
        {
            if (char.TryParse(target, out char value))
                return value;

            return '\0';
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="decimal"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a decimal that represents the converted string</returns>
        public static decimal ToDecimal(this string target) =>
            target.ToDecimal(cultureInfo.NumberFormat);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="decimal"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a decimal that represents the converted string</returns>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        public static decimal ToDecimal(this string target, NumberFormatInfo numberformat)
        {
            if (decimal.TryParse(target, NumberStyles.Any, numberformat, out decimal result))
                return result;

            return decimal.MinusOne;
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="decimal"/> using the en-US
        /// number format
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a decimal that represents the converted string</returns>
        public static decimal ToDecimalUS(this string target) =>
            target.ToDecimal(numberFormatInfoEnUs);

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to
        /// its double-precision floating-point number equivalent. if the string content is "nan"
        /// then a <see cref="double.NaN"/> is returned.
        /// </summary>
        /// <param name="target">A string that contains a number to convert.</param>
        /// <returns>
        /// A double-precision floating-point number that is equivalent to the numeric value or
        /// symbol specified in <paramref name="target"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        public static double ToDouble(this string target) => target.ToDouble(cultureInfo.NumberFormat);

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes at a
        /// specified position in a byte array.
        /// </summary>
        /// <param name="target">The byte array to convert to its double representation</param>
        /// <returns>The double-precision floating point number value of the byte array</returns>
        public static double ToDouble(this byte[] target) => BitConverter.ToDouble(target, 0);

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to
        /// its double-precision floating-point number equivalent. if the string content is "nan"
        /// then a <see cref="double.NaN"/> is returned.
        /// </summary>
        /// <param name="target">A string that contains a number to convert.</param>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        /// <returns>
        /// A double-precision floating-point number that is equivalent to the numeric value or
        /// symbol specified in <paramref name="target"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        public static double ToDouble(this string target, NumberFormatInfo numberformat)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.Equals("nan", StringComparison.CurrentCultureIgnoreCase))
                return double.NaN;

            try
            {
                return double.Parse(target, NumberStyles.Any, numberformat);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts the string representation of a number in the en-US format to its
        /// double-precision floating-point number equivalent. if the string content is "nan" then a
        /// <see cref="double.NaN"/> is returned.
        /// </summary>
        /// <param name="target">A string that contains a number to convert.</param>
        /// <returns>
        /// A double-precision floating-point number that is equivalent to the numeric value or
        /// symbol specified in <paramref name="target"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        public static double ToDoubleUS(this string target) => target.ToDouble(numberFormatInfoEnUs);

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific
        /// format to its single-precision floating-point number equivalent. if the string content is
        /// "nan" then a <see cref="double.NaN"/> is returned.
        /// </summary>
        /// <param name="target">A string that contains a number to convert.</param>
        /// <returns>
        /// A single-precision floating-point number equivalent to the numeric value or symbol
        /// specified in <paramref name="target"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        public static float ToFloat(this string target) => target.ToFloat(cultureInfo.NumberFormat);

        /// <summary>
        /// Returns a single-precision floating point number converted from four bytes at a specified
        /// position in a byte array.
        /// </summary>
        /// <param name="target">The byte array to convert</param>
        /// <returns>The single-precision floating point number representaion of the byte array</returns>
        public static float ToFloat(this byte[] target) => BitConverter.ToSingle(target, 0);

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific
        /// format to its single-precision floating-point number equivalent. if the string content is
        /// "nan" then a <see cref="double.NaN"/> is returned.
        /// </summary>
        /// <param name="target">A string that contains a number to convert.</param>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        /// <returns>
        /// A single-precision floating-point number equivalent to the numeric value or symbol
        /// specified in <paramref name="target"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        public static float ToFloat(this string target, NumberFormatInfo numberformat)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.Equals("nan", StringComparison.CurrentCultureIgnoreCase))
                return float.NaN;

            try
            {
                return float.Parse(target, NumberStyles.Any, numberformat);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts the string representation of a number in a the en-US format format to its
        /// single-precision floating-point number equivalent. if the string content is "nan" then a
        /// <see cref="double.NaN"/> is returned.
        /// </summary>
        /// <param name="target">A string that contains a number to convert.</param>
        /// <returns>
        /// A single-precision floating-point number equivalent to the numeric value or symbol
        /// specified in <paramref name="target"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        public static float ToFloatUS(this string target) => target.ToFloat(numberFormatInfoEnUs);

        /// <summary>
        /// Returns a 32-bit signed integer converted from four bytes at a specified position in a
        /// byte array.
        /// </summary>
        /// <param name="target">An array of bytes.</param>
        /// <returns>A 32-bit signed integer formed by four bytes</returns>
        public static int ToInteger(this byte[] target) => BitConverter.ToInt32(target, 0);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="int"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns an int that represents the converted string</returns>
        public static int ToInteger(this string target) => target.ToInteger(cultureInfo.NumberFormat);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="int"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns an int that represents the converted string</returns>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        public static int ToInteger(this string target, NumberFormatInfo numberformat)
        {
            if (int.TryParse(target, NumberStyles.Any, numberformat, out int result))
                return result;

            return int.MinValue;
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="int"/> using the en-US number format
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns an int that represents the converted string</returns>
        public static int ToIntegerUS(this string target) => target.ToInteger(numberFormatInfoEnUs);

        /// <summary>
        /// Returns a long converted from eight bytes.
        /// </summary>
        /// <param name="target">An array of bytes.</param>
        /// <returns>A long formed by eight bytes</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="target"/> has less than 8 bytes</exception>
        public static long ToLong(this byte[] target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.Length < 8)
                throw new ArgumentException("The byte array has less than 8 bytes");

            return BitConverter.ToInt64(target, 0);
        }

        /// <summary>
        /// Tries to convert a <see cref="object"/> to an <see cref="long"/>
        /// </summary>
        /// <param name="target">The object to convert</param>
        /// <returns>Returns a long that represents the converted object</returns>
        public static long ToLong(this object target)
        {
            if (target == null)
                return -1;

            return target.ToString().ToInteger();
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="long"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a long that represents the converted string</returns>
        public static long ToLong(this string target) => target.ToLong(cultureInfo.NumberFormat);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="long"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a long that represents the converted string</returns>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        public static long ToLong(this string target, NumberFormatInfo numberformat)
        {
            long result;

            if (long.TryParse(target, NumberStyles.Any, numberformat, out result))
                return result;

            return -1;
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="long"/> using the en-US number format
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a long that represents the converted string</returns>
        public static long ToLongUS(this string target) => target.ToLong(numberFormatInfoEnUs);

        /// <summary>
        /// Converts a <see cref="DateTime"/> to ordinal date.
        /// An ordinal date is a calendar date typically consisting of a year and a day of year ranging between 1 and 366 (starting on January 1).
        /// </summary>
        /// <param name="date">The date to convert</param>
        /// <returns>A ordinal formatted string representation of the date</returns>
        public static string ToOrdinalDate(this DateTime date) => $"{date.ToString("yyyy")}{ date.DayOfYear.ToString("000")}";

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="short"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a short that represents the converted string</returns>
        public static short ToShort(this string target) =>
            target.ToShort(cultureInfo.NumberFormat);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="short"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a short that represents the converted string</returns>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        public static short ToShort(this string target, NumberFormatInfo numberformat)
        {
            if (short.TryParse(target, NumberStyles.Any, numberformat, out short result))
                return result;

            return short.MinValue;
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="short"/> using the en-US number format
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a short that represents the converted string</returns>
        public static short ToShortUS(this string target) =>
            target.ToShort(numberFormatInfoEnUs);

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. This also takes into account that <paramref
        /// name="source"/> can be an inline text for the TextBlock. <see
        /// cref="CultureInfo.CurrentCulture"/> is used as <see cref="IFormatProvider"/>
        /// </summary>
        /// <param name="source">The formatted string</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>
        /// A copy of format in which the format items have been replaced by the string
        /// representation of the corresponding objects in <paramref name="args"/>.
        /// </returns>
        public static string ToString(this string source, params object[] args) =>
            source.ToString(cultureInfo, args);

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. This also takes into account that <paramref
        /// name="source"/> can be an inline text for the TextBlock
        /// </summary>
        /// <param name="source">The formatted string</param>
        /// <param name="provider">An object that supplies culture-specific formatting information</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>
        /// A copy of format in which the format items have been replaced by the string
        /// representation of the corresponding objects in <paramref name="args"/>.
        /// </returns>
        public static string ToString(this string source, IFormatProvider provider, params object[] args)
        {
            if (source.StartsWith("<Inline>") && source.EndsWith("</Inline>"))
            {
                var legalArgs = new object[args.Length];
                // we only do this for strings, nothing else
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] is string)
                        legalArgs[i] = WebUtility.HtmlEncode(args[i].ToString());
                    else
                        legalArgs[i] = args[i];
                }
                return string.Format(source, legalArgs);
            }

            return string.Format(provider, source, args);
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="uint"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns an uint that represents the converted string</returns>
        public static uint ToUInteger(this string target) => target.ToUInteger(cultureInfo.NumberFormat);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="uint"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns an uint that represents the converted string</returns>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        public static uint ToUInteger(this string target, NumberFormatInfo numberformat)
        {
            if (uint.TryParse(target, NumberStyles.Any, numberformat, out uint result))
                return result;

            return uint.MinValue;
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="uint"/> using the en-US number format
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns an uint that represents the converted string</returns>
        public static uint ToUIntegerUS(this string target) => target.ToUInteger(numberFormatInfoEnUs);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="ulong"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a ulong that represents the converted string</returns>
        public static ulong ToULong(this string target) =>
            target.ToULong(cultureInfo.NumberFormat);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="ulong"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a ulong that represents the converted string</returns>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        public static ulong ToULong(this string target, NumberFormatInfo numberformat)
        {
            if (ulong.TryParse(target, NumberStyles.Any, numberformat, out ulong result))
                return result;

            return ulong.MinValue;
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="ulong"/> using the en-US number format
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a ulong that represents the converted string</returns>
        public static ulong ToULongUS(this string target) =>
            target.ToULong(numberFormatInfoEnUs);

        /// <summary>
        /// Returns a 16-bit signed integer converted from four bytes at a specified position in a
        /// byte array.
        /// </summary>
        /// <param name="target">An array of bytes.</param>
        /// <returns>A 16-bit signed integer formed by four bytes</returns>
        public static ushort ToUShort(this byte[] target) => BitConverter.ToUInt16(target, 0);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="ushort"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a ushort that represents the converted string</returns>
        public static ushort ToUShort(this string target) =>
            target.ToUShort(cultureInfo.NumberFormat);

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="ushort"/>
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a ushort that represents the converted string</returns>
        /// <param name="numberformat">
        /// An object that supplies culture-specific formatting information about <paramref name="target"/>.
        /// </param>
        public static ushort ToUShort(this string target, NumberFormatInfo numberformat)
        {
            if (ushort.TryParse(target, NumberStyles.Any, numberformat, out ushort result))
                return result;

            return ushort.MinValue;
        }

        /// <summary>
        /// Tries to convert a <see cref="string"/> to an <see cref="ushort"/> using the en-US number format
        /// </summary>
        /// <param name="target">The string to convert</param>
        /// <returns>Returns a ushort that represents the converted string</returns>
        public static ushort ToUShortUS(this string target) => target.ToUShort(numberFormatInfoEnUs);

        private static Encoding ToEncoding(Encodings encoding)
        {
            switch (encoding)
            {
                case Encodings.ASCII: return Encoding.ASCII;
                case Encodings.BigEndianUnicode: return Encoding.BigEndianUnicode;
                case Encodings.Unicode: return Encoding.Unicode;
                case Encodings.UTF32: return Encoding.UTF32;
                case Encodings.UTF7: return Encoding.UTF7;
                case Encodings.UTF8: return Encoding.UTF8;
                case Encodings.EBCDIC_IBM037: return Encoding.GetEncoding("IBM037");
                case Encodings.EBCDIC_IBM01047: return Encoding.GetEncoding("IBM01047");
                case Encodings.EBCDIC_IBM01140: return Encoding.GetEncoding("IBM01140");
                case Encodings.EBCDIC_IBM01141: return Encoding.GetEncoding("IBM01141");
                case Encodings.ANSI: return Encoding.GetEncoding("ISO-8859-1");
            }

            throw new NotImplementedException("Unknown encoding");
        }
    }
}