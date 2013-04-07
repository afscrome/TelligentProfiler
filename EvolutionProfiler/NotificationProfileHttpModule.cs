using StackExchange.Profiling;
using System;
using System.Web;

namespace Telligent.Evolution.Profiler
{
	/// <summary>
	/// Helper module to log asp.net pipeline stages 
	/// </summary>
	public class NotificationProfileHttpModule : IHttpModule
	{
		private static readonly object _notificationStepKey = new object();

		public void Init(HttpApplication context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			context.AcquireRequestState += BeginPipelineStage;
			context.AuthenticateRequest += BeginPipelineStage;
			context.AuthorizeRequest += BeginPipelineStage;
			context.LogRequest += BeginPipelineStage;
			context.MapRequestHandler += BeginPipelineStage;
			context.ReleaseRequestState += BeginPipelineStage;
			context.PreRequestHandlerExecute += BeginPipelineStage;
			context.ResolveRequestCache += BeginPipelineStage;
			context.UpdateRequestCache += BeginPipelineStage;

			context.PostAcquireRequestState += EndPipelineStage;
			context.PostAuthenticateRequest += EndPipelineStage;
			context.PostAuthorizeRequest += EndPipelineStage;
			context.PostLogRequest += EndPipelineStage;
			context.PostMapRequestHandler += EndPipelineStage;
			context.PostReleaseRequestState += EndPipelineStage;
			context.PostRequestHandlerExecute += EndPipelineStage;
			context.PostResolveRequestCache += EndPipelineStage;
			context.PostUpdateRequestCache += EndPipelineStage;
		}


		private void BeginPipelineStage(object sender, EventArgs e)
		{
			if (MiniProfiler.Current == null)
				return;

			var app = (HttpApplication)sender;
			var context = app.Context;
			context.Items[_notificationStepKey] = MiniProfiler.Current.Step("[aspnet] HttpApplication: " + context.CurrentNotification.ToString());
		}

		private void EndPipelineStage(object sender, EventArgs e)
		{
			if (MiniProfiler.Current == null)
				return;

			var app = (HttpApplication)sender;
			var step = (IDisposable)app.Context.Items[_notificationStepKey];
			step.Dispose();
		}

		public void Dispose()
		{
		}
	}

}
