using StackExchange.Profiling;
using System;
using System.Web;
using System.Web.UI;
using Telligent.Common.Diagnostics.Tracing;

namespace Telligent.Evolution.Profiler
{
	/// <summary>
	/// HTTP Module to start MiniProfiler tracing, and inject the results
	/// into an ASP.Net page.
	/// </summary>
	public class ProfilerHttpModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			context.BeginRequest += context_BeginRequest;
			context.PostAuthenticateRequest += context_PostAuthenticateRequest;
			context.PostMapRequestHandler += context_PostMapRequestHandler;
			context.EndRequest += context_EndRequest;
		}

		private void context_BeginRequest(object sender, EventArgs e)
		{
			if (!MiniProfilerHelper.ProfilingEnabled())
				return;

			if (!TracePoints.Enabled)
				throw new InvalidOperationException("Tracing must be enabled in diagnostics.config to profile Telligent Evolution");

			MiniProfiler.Start();
		}

		private void context_PostAuthenticateRequest(object sender, EventArgs e)
		{
			//If the current user can't view profiling results, let's stop profiling here
			if (MiniProfilerHelper.IsTracing && !MiniProfilerHelper.AuthoriseResults(HttpContext.Current.Request))
				MiniProfiler.Stop(discardResults: true);
		}

		private void context_PostMapRequestHandler(object sender, EventArgs e)
		{
			var page = HttpContext.Current.Handler as Page;

			if (page != null)
				page.PreRender += page_PreRender;
		}


		private void page_PreRender(object sender, EventArgs e)
		{
			if (!MiniProfilerHelper.IsTracing)
				return;

			var page = HttpContext.Current.Handler as Page;
			if (page != null)
			{
				page.ClientScript.RegisterStartupScript(this.GetType(), "miniprofilerscript", MiniProfiler.RenderIncludes().ToString());
				page.ClientScript.RegisterStartupScript(this.GetType(), "miniprofilerstyles", CssAdditions);
			}

		}

		private void context_EndRequest(object sender, EventArgs e)
		{
			MiniProfiler.Stop();
		}

		public void Dispose()
		{

		}

		const string CssAdditions = @"
<style>
.profiler-results .profiler-popup .timings .label {
max-width: 100%;
}
</style>";
	}

}
