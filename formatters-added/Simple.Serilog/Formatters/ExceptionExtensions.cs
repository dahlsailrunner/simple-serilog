using System;
using System.Collections.Generic;

namespace Simple.Serilog.Formatters
{
    public static class ExceptionExtensions
    {
        public static CustomError ToCustomError(this Exception ex)
        {
            var rpError = new CustomError
            {
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Message = ex.Message,
                Data = new List<CustomDictEntry>()
            };

            var ts = ex.TargetSite;
            if (ts != null)
            {
                rpError.ModuleName = ts.Module.Name;
                rpError.DeclaringTypeName = ts.DeclaringType?.Name;
                rpError.TargetSiteName = ts.Name;
            }

            foreach (var dataKey in ex.Data.Keys)
            {
                if (ex.Data[dataKey] != null)
                {
                    rpError.Data.Add(new CustomDictEntry
                    {
                        Key = dataKey.ToString(),
                        Value = ex.Data[dataKey]?.ToString()
                    });
                }
            }

            if (ex.InnerException != null)
                rpError.InnerError = ex.InnerException.ToCustomError();

            return rpError;
        }

        public static string InnermostMessage(this Exception e)
        {
            while (true)
            {
                if (e.InnerException == null) return e.Message;
                e = e.InnerException;
            }
        }
    }
}
