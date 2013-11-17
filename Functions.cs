using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerDotNet
{
    /// <summary>
    /// Summary description for Functions
    /// </summary>
    public static class Functions
    {
        public static string GetIpAddress()
        {
            try
            {
                return (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]).GetString().Split(',')[0].Trim();
            }
            catch
            {
                return System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].GetString();
            }
        }
    }
}