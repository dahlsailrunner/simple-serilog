using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Simple.Serilog.Filters
{
    public class TrackPerformanceFilter : IActionFilter
    {
        private PerfTracker _tracker;
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor cad)) return;

            //if (context.RouteData.Values.Keys.Count <= 2)  // no route parameters
            //{
                _tracker = new PerfTracker($"{cad.ControllerName}-{cad.ActionName}");
            //}
            //else // has route parameters - add them to the log entry
            //{
            //    var routeValues = new StringBuilder();
            //    foreach (var key in context.RouteData.Values.Keys)
            //    {
            //        routeValues.Append($"{key}-{context.RouteData.Values[key]};");
            //    }
            //    _tracker = new PerfTracker($"{cad.ControllerName}-{cad.ActionName}",
            //        "routeData", routeValues.ToString());
            //}
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _tracker?.Stop();
        }
    }
}
