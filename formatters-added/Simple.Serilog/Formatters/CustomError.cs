using System.Collections.Generic;
using System.Text;

namespace Simple.Serilog.Formatters
{
    public class CustomError
    {
        public string ExceptionType { get; set; }
        public string ModuleName { get; set; }
        public string DeclaringTypeName { get; set; }
        public string TargetSiteName { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public List<CustomDictEntry> Data { get; set; }
        public CustomError InnerError { get; set; }

        public override string ToString()
        {
            return WriteError(new StringBuilder(), "");
        }

        private string WriteError(StringBuilder workInProgress, string prefix)
        {
            workInProgress.AppendLine($"{prefix}ExceptionType: {ExceptionType}");
            workInProgress.AppendLine($"{prefix}Message: {Message}");
            workInProgress.AppendLine($"{prefix}ModuleName: {ModuleName}");
            workInProgress.AppendLine($"{prefix}DeclaringTypeName: {DeclaringTypeName}");
            workInProgress.AppendLine($"{prefix}TargetSiteName: {TargetSiteName}");

            foreach (var item in Data)
                workInProgress.AppendLine($"{prefix}Data-{item.Key}: {item.Value}");

            workInProgress.AppendLine($"{prefix}StackTrace: {StackTrace}");
            if (InnerError != null)
                workInProgress.AppendLine($"{prefix}InnerError: {InnerError.WriteError(workInProgress, $"{prefix}\t")}");

            return workInProgress.ToString();
        }
    }
    public class CustomDictEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
