using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Telligent.Common.Diagnostics.Tracing;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Version1;

namespace Telligent.Evolution.Profiler
{
	public static class MiniProfilerHelper
	{
		public static void Init()
		{
			if (!HostingEnvironment.IsHosted)
				throw new InvalidOperationException("Cannot start profiling unless running under ASP.Net");

			TracePoints.Starting += TracePoints_Starting;
			TracePoints.Completed += TracePoints_Completed;

			MiniProfiler.Settings.PopupShowTimeWithChildren = true;
			MiniProfiler.Settings.RouteBasePath = "~/utility/profiler";
			MiniProfiler.Settings.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
			MiniProfiler.Settings.Results_Authorize = MiniProfiler.Settings.Results_List_Authorize = AuthoriseResults;

			StackExchange.Profiling.UI.MiniProfilerHandler.RegisterRoutes();
		}

		/// <summary>
		/// Returns whether the current request is being traced
		/// </summary>
		public static bool IsTracing
		{
			get { return HttpContext.Current != null && MiniProfiler.Current != null; }
		}

		public static bool ProfilingEnabled()
		{
			var plugin = PluginManager.GetSingleton<ProfilerPlugin>();
			return plugin != null && PluginManager.IsEnabled(plugin);
		}

		/// <summary>
		/// Helper method used by Mini Profiler to determine whether the
		/// trace should be accessible by the current user.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static bool AuthoriseResults(HttpRequest request)
		{
			if (request == null)
				throw new ArgumentNullException("request");

			var user = PublicApi.Users.AccessingUser;
			return ProfilingEnabled()
				&& (
					request.IsLocal
					|| ProfilerPlugin.AllowedUserNames.Contains(user.Username, StringComparer.OrdinalIgnoreCase)
					|| ProfilerPlugin.AllowedRoles.Any(x => PublicApi.RoleUsers.IsUserInRoles(user.Username, new[] { x }))
				);
		}

		private static readonly object _stepsKey = new object();
		/// <summary>
		/// Records the stack of Profiler Steps relating to Trace Points
		/// </summary>
		private static Stack<IDisposable> ProfilerSteps
		{
			get
			{
				var stack = HttpContext.Current.Items[_stepsKey] as Stack<IDisposable>;
				if (stack == null)
				{
					stack = new Stack<IDisposable>();
					HttpContext.Current.Items[_stepsKey] = stack;
				}
				return stack;
			}
		}

		/// <summary>
		/// Adds a Profiler step for the starting tracepoint
		/// </summary>
		private static void TracePoints_Starting(TraceInfo traceInfo, EventArgs e)
		{
			if (IsTracing)
			{
				var step = MiniProfiler.Current.Step(TranslateDescription(traceInfo.Description));
				ProfilerSteps.Push(step);
			}
		}

		/// <summary>
		/// Stops the Mini Profiler step for the completed tracepoint.
		/// </summary>
		private static void TracePoints_Completed(TraceInfo traceInfo, EventArgs e)
		{
			if (IsTracing)
			{
				var entry = ProfilerSteps.Pop();
				entry.Dispose();
			}
		}

		/// <summary>
		///     Simplify some trace point descriptions for Mini Profiler display to make it easier to read since
		///     Mini Profiler typically truncates to the first 100 characters or so.
		/// </summary>
		/// <remarks>
		///     Translate:
		///         [scripted-content-fragments] rendering nvelocity script to control hierarchy: Friendship List (p:133a73e034ea4500939f774b78d3d683:424eb7d9138d417b994b64bff44bf274:Content)
		///         [scripted-content-fragments] rendering nvelocity script to string: Blog - Post List (f:dd895400883f4f43beca9fd9376a55b1:424eb7d9138d417b994b64bff44bf274:subview-perform-query.vm)
		///         [scripted-content-fragments] rendering nvelocity script to string: Blog - Post List (f:dd895400883f4f43beca9fd9376a55b1:424eb7d9138d417b994b64bff44bf274:Header)
		///     to
		///         [widget] Friendship List (p:133a73e034ea4500939f774b78d3d683:424eb7d9138d417b994b64bff44bf274:Content)
		///         [executed-file] subview-perform-query.vm of Blog - Post List (f:dd895400883f4f43beca9fd9376a55b1:424eb7d9138d417b994b64bff44bf274:subview-perform-query.vm)
		///         [widget-header] subview-perform-query.vm of Blog - Post List (f:dd895400883f4f43beca9fd9376a55b1:424eb7d9138d417b994b64bff44bf274:Header)
		/// </remarks>
		private static string TranslateDescription(string description)
		{
			const string prefix = "[scripted-content-fragments] ";
			const string widgetPrefix = "rendering nvelocity script to control hierarchy: ";
			const string viewPrefix = "rendering nvelocity script to string:";

			if (description.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				description = description.Substring(prefix.Length);
				if (description.StartsWith(widgetPrefix, StringComparison.OrdinalIgnoreCase))
				{
					description = "[widget] " + description.Substring(widgetPrefix.Length);
				}
				else if (description.StartsWith(viewPrefix, StringComparison.OrdinalIgnoreCase))
				{
					description = description.Substring(viewPrefix.Length);
					var viewName = description.Substring(description.LastIndexOf(':') + 1).TrimEnd(')');
					return viewName == "Header"
						? "[widget-header] " + description
						: String.Concat("[executed-file] ", viewName, " of ", description);
				}

			}
			return description;
		}
	}
}
