using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MiniExcelLibs.Attributes;

namespace Transport.Model
{    
    public class db_data_access
    {
       

    }
    public sealed class Shared
    {
        private Shared() { }

        public static bool ToBoolean(object o)
        {
            return ToBoolean(o, false);
        }

        public static bool ToBoolean(object o, bool Default)
        {
            if (o == null || o == DBNull.Value || o.ToString().Length == 0)
                return Default;

            try
            {
                return Convert.ToBoolean(o);
            }
            catch
            {
                return Default;
            }
        }

        public static int ToInt(object o)
        {
            return ToInt(o, 0);
        }

        public static int ToInt(object o, int Default)
        {
            return ToInt(o, Default, -1);
        }

        public static int ToInt(object o, int Default, int Err)
        {
            if (o == null || o == DBNull.Value || o.ToString().Length == 0)
                return Default;

            try
            {
                return Convert.ToInt32(o);
            }
            catch
            {
                return Err;
            }
        }

        public static short ToShort(object o)
        {
            return ToShort(o, 0);
        }

        public static short ToShort(object o, short Default)
        {
            return ToShort(o, Default, -1);
        }

        public static short ToShort(object o, short Default, short Err)
        {
            if (o == null || o == DBNull.Value || o.ToString().Length == 0)
                return Default;

            try
            {
                return Convert.ToInt16(o);
            }
            catch
            {
                return Err;
            }
        }

        public static decimal ToDecimal(object o)
        {
            return ToDecimal(o, 0);
        }

        public static decimal ToDecimal(object o, int Default)
        {
            return ToDecimal(o, Default, -1);
        }

        public static decimal ToDecimal(object o, int Default, int Err)
        {
            if (o == null || o == DBNull.Value || o.ToString().Length == 0)
                return Default;

            try
            {
                return Convert.ToDecimal(o);
            }
            catch
            {
                return Err;
            }
        }

        public static double ToDouble(object o)
        {
            return ToDouble(o, 0);
        }

        public static double ToDouble(object o, int Default)
        {
            return ToDouble(o, Default, -1);
        }

        public static double ToDouble(object o, int Default, int Err)
        {
            if (o == null || o == DBNull.Value || o.ToString().Length == 0)
                return Default;

            try
            {
                return Convert.ToDouble(o);
            }
            catch
            {
                return Err;
            }
        }

        public static char ToChar(object o)
        {
            return ToChar(o, '\0');
        }

        public static char ToChar(object o, char Default)
        {
            if (o == null || o == DBNull.Value)
                return Default;

            try
            {
                return Convert.ToChar(o);
            }
            catch //(Exception ex)
            {
                //				return ex.Message;
                return Default;
            }
        }

        public static string ToString(object o)
        {
            return ToString(o, "");
        }

        public static string ToString(object o, string Default)
        {
            if (o == null || o == DBNull.Value)
                return Default;

            try
            {
                return o.ToString();
            }
            catch //(Exception ex)
            {
                //				return ex.Message;
                return Default;
            }
        }
      
        /// <summary>
        /// Check and return formatted string
        /// </summary>
        /// <param name="sString">the string to be formatted</param>
        /// <param name="sFormat">the string format</param>
        public static string FormatString(object oString, object oFormat)
        {
            string sFormatted = "";

            try
            {
                if (oFormat != null && ToString(oFormat).Length > 0)
                    sFormatted = String.Format("{0:" + ToString(oFormat) + "}", oString);
                else
                    sFormatted = ToString(oString);
            }
            catch
            {
                sFormatted = ToString(oString);
            }

            return sFormatted;
        }

        /// <summary>
        /// Return the character of the given character value
        /// </summary>
        /// <param name="i">the character value</param>
        public static char Chr(int i)
        {
            return Convert.ToChar(i);
        }

        /// <summary>
        /// Return the character value of the given character
        /// </summary>
        /// <param name="ch">the character</param>
        public static int Asc(char ch)
        {
            return (int)Encoding.ASCII.GetBytes(new Char[1] { ch })[0];
        }

        /// <summary>
        /// Check for Boolean
        /// </summary>
        /// <param name="obj">the object to evaluate</param>
        public static bool IsBoolean(object obj)
        {
            try
            {
                if (obj == null)
                    return false;

                Convert.ToBoolean(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check for DateTime
        /// </summary>
        /// <param name="obj">the object to evaluate</param>
        public static bool IsDateTime(object obj)
        {
            try
            {
                if (obj == null)
                    return false;

                Convert.ToDateTime(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check for Decimal
        /// </summary>
        /// <param name="obj">the object to evaluate</param>
        public static bool IsDecimal(object obj)
        {
            try
            {
                if (obj == null)
                    return false;

                obj = obj.ToString().Replace("$", "");
                obj = obj.ToString().Replace(",", "");

                Convert.ToDouble(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check for Integer
        /// </summary>
        /// <param name="obj">the object to evaluate</param>
        public static bool IsInteger(object obj)
        {
            try
            {
                if (obj == null)
                    return false;

                obj = obj.ToString().Replace("$", "");
                obj = obj.ToString().Replace(",", "");
                //				obj = obj.ToString().Replace(".", "");

                Convert.ToInt32(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check for DateTime
        /// </summary>
        /// <param name="obj">the object to evaluate</param>
        public static bool IsPositiveInteger(object obj)
        {
            long iTemp;

            try
            {
                if (obj == null)
                    return false;

                obj = obj.ToString().Replace("$", "");
                obj = obj.ToString().Replace(",", "");
                obj = obj.ToString().Replace(".", "");

                iTemp = Convert.ToInt32(obj);

                if (iTemp < 0)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
