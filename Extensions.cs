using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerDotNet
{
    /// <summary>
    /// Summary description for Extensions
    /// </summary>
    public static class Extensions
    {
        public static string GetString(this object value, string defaultValue = "")
        {
            if (value == null)
                return defaultValue;
            if (value == DBNull.Value)
                return defaultValue;
            try
            {
                return value.ToString();

            }
            catch
            {
                return defaultValue;
            }
        }

        public static T GetEnum<T>(this object value, T defaultValue) where T : struct
        {
            if (value == null)
                return defaultValue;
            if (value == DBNull.Value)
                return defaultValue;

            T result;
            if (Enum.TryParse<T>(value.ToString(), out result))
            {
                return result;
            }
            else
            {
                return defaultValue;  // you may want to throw exception here
            }
        }

        public static int GetInt(this object value, int defaultValue = 0)
        {
            if (value == null)
                return defaultValue;
            if (value == DBNull.Value)
                return defaultValue;
            try
            {
                return Convert.ToInt32(value);

            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool GetBool(this object value, bool defaultValue = false)
        {
            if (value == null)
                return defaultValue;
            if (value == DBNull.Value)
                return defaultValue;
            try
            {
                if (value == "true" || value == "True")
                    return true;

                if (value == "1")
                    return true;

                return Convert.ToBoolean(value);

            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime GetDate(this object value, DateTime defaultValue = new DateTime())
        {
            if (value == null)
                return defaultValue;
            if (value == DBNull.Value)
                return defaultValue;
            try
            {
                return Convert.ToDateTime(value);

            }
            catch
            {
                return defaultValue;
            }
        }

        //Goes together with PowerLib.js getEpoch, outputs the same value
        public static long GetEpoch(this DateTime value)
        {
            return (value.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static string NormalizeForUrl(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.NormalizeString();

                // Replaces all non-alphanumeric character by a space
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                char[] removeables = new char[] { '’', '\'', '"' };
                for (int i = 0; i < value.Length; i++)
                {
                    if (!removeables.Contains(value[i]))
                        builder.Append(char.IsLetterOrDigit(value[i]) ? value[i] : ' ');
                }

                value = builder.ToString();

                // Replace multiple spaces into a single dash
                value = System.Text.RegularExpressions.Regex.Replace(value, @"[ ]{1,}", @"-", System.Text.RegularExpressions.RegexOptions.None);

                value = value.TrimEnd('?', '.', ',', '-');
            }

            return value;
        }


        /// <summary>
        /// Strips the value from any non english character by replacing thoses with their english equivalent.
        /// </summary>
        /// <param name="value">The string to normalize.</param>
        /// <returns>A string where all characters are part of the basic english ANSI encoding.</returns>
        /// <seealso cref="http://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net"/>
        public static string NormalizeString(this string value)
        {
            string normalizedFormD = value.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < normalizedFormD.Length; i++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(normalizedFormD[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(normalizedFormD[i]);
                }
            }

            return builder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
        public static string StripFrom(this string value, int startPosition)
        {
            return startPosition > value.Length ? value : value.Remove(startPosition);
        }
        public static string StripTo(this string value, int endPosition)
        {
            return endPosition > value.Length ? "" : value.Remove(0, endPosition);
        }

        public static string Encrypt(this Object value)
        {
            try
            {
                return Crypt.EncryptString(value.GetString());
            }
            catch { return ""; }
        }

        public static string Decrypt(this Object value)
        {
            try
            {
                return Crypt.DecryptString(value.GetString());
            }
            catch { return ""; }
        }
    }
}