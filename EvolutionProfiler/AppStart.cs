using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using System.Web;
using Telligent.Evolution.Profiler;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStart), "Start")]
namespace Telligent.Evolution.Profiler
{
	public static class PreApplicationStart
	{
		/// <summary>
		/// Dynamicaly registers HTTP Modules for MiniProfiler logging
		/// as well as initalising MiniProfiler itself
		/// </summary>
		public static void Start()
		{
			DynamicModuleUtility.RegisterModule(typeof(ProfilerHttpModule));
			DynamicModuleUtility.RegisterModule(typeof(NotificationProfileHttpModule));
			DynamicModuleUtility.RegisterModule(typeof(PageStageProfileHttpModule));
			MiniProfilerHelper.Init();
		}
	}
}