using StackExchange.Profiling;
using System;
using System.Web;
using System.Web.UI;

namespace Telligent.Evolution.Profiler
{
	/// <summary>
	/// HTTP Module to record phases of the ASP.Net page lifecycle 
	/// </summary>
	public class PageStageProfileHttpModule : IHttpModule
	{
		private static readonly object _notificationStepKey = new object();

		public void Init(HttpApplication context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			context.PostMapRequestHandler += context_PostMapRequestHandler;
		}

		private static void context_PostMapRequestHandler(object sender, EventArgs e)
		{
			var app = (HttpApplication)sender;
			var page = app.Context.Handler as Page;

			if (page == null)
				return;

			page.PreInit += page_Init;
			page.PreLoad+= page_Load;
			page.PreRender += page_PreRender;

			page.InitComplete += EndPipelineStage;
			page.LoadComplete += EndPipelineStage;
			page.PreRenderComplete += EndPipelineStage;
		}

		private static void page_Init(object sender, EventArgs e) { BeginPipelineStage(sender, "Page: Init"); }
		private static void page_Load(object sender, EventArgs e) { BeginPipelineStage(sender, "Page: Load"); }
		private static void page_PreRender(object sender, EventArgs e) { BeginPipelineStage(sender, "Page: PreRender"); }

		private static void BeginPipelineStage(object sender, string stage)
		{
			var page = (Page)sender;
			if (MiniProfilerHelper.IsTracing)
				page.Items[_notificationStepKey] = MiniProfiler.Current.Step("[aspnet] " + stage);
		}

		private static void EndPipelineStage(object sender, EventArgs e)
		{
			if (!MiniProfilerHelper.IsTracing)
				return;

			var page = (Page)sender;
			var step = (IDisposable)page.Items[_notificationStepKey];
			step.Dispose();
		}

		public void Dispose()
		{
		}
	}

}
