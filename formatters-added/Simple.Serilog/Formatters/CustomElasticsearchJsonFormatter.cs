// based on https://github.com/serilog/serilog-sinks-elasticsearch/blob/dev/src/Serilog.Formatting.Elasticsearch/ElasticsearchJsonFormatter.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elasticsearch.Net;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace Simple.Serilog.Formatters
{
    public class CustomElasticsearchJsonFormatter : DefaultJsonFormatter
    {
        readonly IElasticsearchSerializer _serializer;
        readonly bool _inlineFields;

        /// <summary>
        /// Render message property name
        /// </summary>
        public const string RenderedMessagePropertyName = "@message";

        /// <summary>
        /// Message template property name
        /// </summary>
        public const string MessageTemplatePropertyName = "messageTemplate";
        
        /// <summary>
        /// Level property name
        /// </summary>
        public const string LevelPropertyName = "level";

        /// <summary>
        /// Timestamp property name
        /// </summary>
        public const string TimestampPropertyName = "@timestamp";

        /// <summary>
        /// Construct a CustomElasticsearchJsonFormatter.
        /// </summary>
        /// <param name="omitEnclosingObject">If true, the properties of the event will be written to
        /// the output without enclosing braces. Otherwise, if false, each event will be written as a well-formed
        /// JSON object.</param>
        /// <param name="closingDelimiter">A string that will be written after each log event is formatted.
        /// If null, <see cref="Environment.NewLine"/> will be used. Ignored if <paramref name="omitEnclosingObject"/>
        /// is true.</param>
        /// <param name="renderMessage">If true, the message will be rendered and written to the output as a
        /// property named RenderedMessage.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="serializer">Inject a serializer to force objects to be serialized over being ToString()</param>
        /// <param name="inlineFields">When set to true values will be written at the root of the json document</param>
        /// <param name="renderMessageTemplate">If true, the message template will be rendered and written to the output as a
        /// property named RenderedMessageTemplate.</param>
        public CustomElasticsearchJsonFormatter(
            bool omitEnclosingObject = false,
            string closingDelimiter = null,
            bool renderMessage = true,
            IFormatProvider formatProvider = null,
            IElasticsearchSerializer serializer = null,
            bool inlineFields = false,
            bool renderMessageTemplate = true)
            : base(omitEnclosingObject, closingDelimiter, renderMessage, formatProvider, renderMessageTemplate)
        {
            _serializer = serializer;
            _inlineFields = inlineFields;
        }

        /// <summary>
        /// Writes out individual renderings of attached properties
        /// </summary>
        protected override void WriteRenderings(IGrouping<string, PropertyToken>[] tokensWithFormat,
            IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write(",\"{0}\":{{", "renderings");
            WriteRenderingsValues(tokensWithFormat, properties, output);
            output.Write("}");
        }

        /// <summary>
        /// Writes out the attached properties
        /// </summary>
        protected override void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties,
            TextWriter output)
        {
            if (!_inlineFields)
                output.Write(",\"{0}\":{{", "fields");
            else
                output.Write(",");

            WritePropertiesValues(properties, output);

            if (!_inlineFields)
                output.Write("}");
        }

        /// <summary>
        /// Escape the name of the Property before calling ElasticSearch
        /// </summary>
        protected override void WriteDictionary(IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> elements,
            TextWriter output)
        {
            var escaped = elements.ToDictionary(e => DotEscapeFieldName(e.Key), e => e.Value);

            base.WriteDictionary(escaped, output);
        }

        /// <summary>
        /// Escape the name of the Property before calling ElasticSearch
        /// </summary>
        protected override void WriteJsonProperty(string name, object value, ref string precedingDelimiter,
            TextWriter output)
        {
            var propertiesToOmit = new List<string> { "SourceContext", "EventId" };
            if (propertiesToOmit.Any(a => a == name))
                return;

            if (string.Equals(name, "HttpContext", StringComparison.InvariantCultureIgnoreCase))
            {
                if (value is StructureValue sv)
                    foreach (var prop in sv.Properties)
                    {
                        name = DotEscapeFieldName(prop.Name);
                        base.WriteJsonProperty(name, prop.Value, ref precedingDelimiter, output);
                    }
            }
            else
            {
                name = DotEscapeFieldName(name);
                base.WriteJsonProperty(name, value, ref precedingDelimiter, output);
            }            
        }

        /// <summary>
        /// Escapes Dots in Strings and does nothing to objects
        /// </summary>
        protected virtual ScalarValue DotEscapeFieldName(ScalarValue value)
        {
            return value.Value is string s ? new ScalarValue(DotEscapeFieldName(s)) : value;
        }

        /// <summary>
        /// Dots are not allowed in Field Names, replaces '.' with '/'
        /// https://github.com/elastic/elasticsearch/issues/14594
        /// </summary>
        protected virtual string DotEscapeFieldName(string value)
        {
            if (value == null) return null;

            return value.Replace('.', '/');
        }

        /// <summary>
        /// Writes out the attached exception
        /// </summary>
        protected override void WriteException(Exception exception, ref string delim, TextWriter output)
        {
            WriteJsonProperty("exception", exception.ToCustomError(), ref delim, output);

            output.Write(delim);
            output.Write("\"");
            output.Write("@exceptionMessage");
            output.Write("\":");
            output.Write($"\"{exception.InnermostMessage()}\"");
        }                

        /// <summary>
        /// (Optionally) writes out the rendered message
        /// </summary>
        protected override void WriteRenderedMessage(string message, ref string delim, TextWriter output)
        {
            WriteJsonProperty(RenderedMessagePropertyName, message, ref delim, output);
        }

        /// <summary>
        /// Writes out the message template for the logevent.
        /// </summary>
        protected override void WriteMessageTemplate(string template, ref string delim, TextWriter output)
        {
            WriteJsonProperty(MessageTemplatePropertyName, template, ref delim, output);
        }

        /// <summary>
        /// Writes out the log level
        /// </summary>
        protected override void WriteLevel(LogEventLevel level, ref string delim, TextWriter output)
        {
            var stringLevel = Enum.GetName(typeof(LogEventLevel), level);
            WriteJsonProperty(LevelPropertyName, stringLevel, ref delim, output);
        }

        /// <summary>
        /// Writes out the log timestamp
        /// </summary>
        protected override void WriteTimestamp(DateTimeOffset timestamp, ref string delim, TextWriter output)
        {
            WriteJsonProperty(TimestampPropertyName, timestamp, ref delim, output);
        }

        /// <summary>
        /// Allows a subclass to write out objects that have no configured literal writer.
        /// </summary>
        /// <param name="value">The value to be written as a json construct</param>
        /// <param name="output">The writer to write on</param>
        protected override void WriteLiteralValue(object value, TextWriter output)
        {
            if (_serializer != null)
            {
                string jsonString = _serializer.SerializeToString(value, SerializationFormatting.None);
                output.Write(jsonString);
                return;
            }

            base.WriteLiteralValue(value, output);
        }
    }
}


