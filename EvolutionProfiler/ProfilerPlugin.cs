using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Telligent.DynamicConfiguration.Components;
using Telligent.DynamicConfiguration.Controls;
using Telligent.Evolution.Extensibility.Version1;

namespace Telligent.Evolution.Profiler
{
	public class ProfilerPlugin : ISingletonPlugin, IConfigurablePlugin, ITranslatablePlugin
	{
		private const string TrivialDurationKey = "TrivialDuration";
		private const string MaxTracesToShowKey = "MaxTraces";
		private const string IgnoredPathsKey = "IgnoredPaths";
		private const string AllowedUsernamesKey = "AllowedUsernames";
		private const string AllowedRolesKey = "AllowedRoles";

		public static IEnumerable<string> AllowedUserNames { get; private set; }
		public static IEnumerable<string> AllowedRoles { get; private set; }
		public static IEnumerable<string> DefaultExcludePaths { get; private set; }

		private ITranslatablePluginController _translations { get; set; }

		static ProfilerPlugin()
		{
			AllowedUserNames = AllowedRoles = new string[0];
			DefaultExcludePaths = new[] {
				"/WebResource.axd",
				"/ScriptResource.axd",
				"/cfs-file.ashx",
				"/cfs-filesystemfile.ashx",
				"/resized-image.ashx",
				"/callback.ashx",
				"/controlpanel/images/",
				"/controlPanel/style",
				"/controlpanel/utility/",
				"/tiny_mce/",
				"/utility/jquery/",
				"/utility/profiler",
				"/utility/images/",
				"/filestorage/",
				"socket.ashx",
			};
		}

		public string Name { get { return GetTranslation("Name"); } }
		public string Description { get { return GetTranslation("Description"); } }

		public void Initialize()
		{
		}

		public void Update(IPluginConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			AllowedUserNames = GetValuesFromMultiLineString(configuration.GetString(AllowedUsernamesKey));
			AllowedRoles = GetValuesFromMultiLineString(configuration.GetString(AllowedRolesKey));

			if (HostingEnvironment.IsHosted)
			{
				MiniProfiler.Settings.TrivialDurationThresholdMilliseconds = Convert.ToDecimal(configuration.GetDouble(TrivialDurationKey));
				MiniProfiler.Settings.PopupMaxTracesToShow = configuration.GetInt(MaxTracesToShowKey);
				//TODO
				//MiniProfiler.Settings.PopupRenderPosition =_current.PopupPosition;
				MiniProfiler.Settings.IgnoredPaths = GetValuesFromMultiLineString(configuration.GetString(IgnoredPathsKey))
					.ToArray();
			}

		}

		public PropertyGroup[] ConfigurationOptions
		{
			get
			{
				var config = new PropertyGroup("config", GetTranslation("Config"), 0);
				AddProperty(config, TrivialDurationKey, PropertyType.Double, "2.0");
				AddProperty(config, MaxTracesToShowKey, PropertyType.Int, "15");
				var ignoredPaths = AddProperty(config, IgnoredPathsKey, PropertyType.String, String.Join("\r\n", DefaultExcludePaths));
				ignoredPaths.ControlType = typeof(MultilineStringControl);

				var access = new PropertyGroup("access", GetTranslation("Access"), 1);
				var allowedUsernames = AddProperty(access, AllowedUsernamesKey, PropertyType.String, "");
				allowedUsernames.ControlType = typeof(MultilineStringControl);
				var allowedRoles = AddProperty(access, AllowedRolesKey, PropertyType.String, "Administrators");
				allowedRoles.ControlType = typeof(MultilineStringControl);

				return new[] { config, access };
			}
		}

		private string GetTranslation(string key)
		{
			// Workaround for translations causing a plugin to fail on app startup
			try
			{
				return _translations.GetLanguageResourceValue(key);
			}
			catch (HttpException ex)
			{
				//Check the error is "Request is not available in this context", 
				if (ex.ErrorCode == -2147467259) //0x80004005
					return String.Empty;
				else
					throw;
			}
		}

		public Translation[] DefaultTranslations
		{
			get
			{
				var en = new Translation("en-us");
				en.Set("Name", "Performance Profiler");
				en.Set("Description", "Provides a popup on each page showing what actions took place on ");

				en.Set("Config", "Configuration");
				en.Set("TrivialDuration", "Trivial Duration");
				en.Set("TrivialDuration_Desc", "Activites under this length of time (in milliseconds) will be hidden by default.");
				en.Set("MaxTraces", "Max Traces");
				en.Set("MaxTraces_Desc", "Maximum number of Traces");
				en.Set("IgnoredPaths", "Ignored Paths");
				en.Set("IgnoredPaths_Desc", "Any paths.  Enter each one on a new line");

				en.Set("Access", "Access");
				en.Set("AllowedUsernames", "Allowed Usernames");
				en.Set("AllowedUsernames_Desc", "These users will see the performance results. Enter each on a new line.");
				en.Set("AllowedRoles", "Allowed Roles");
				en.Set("AllowedRoles_Desc", "Any user in these roles will be see performance results. Enter each on a new line.");

				return new[] { en };
			}
		}

		public void SetController(ITranslatablePluginController controller)
		{
			_translations = controller;
		}

		private static IEnumerable<string> GetValuesFromMultiLineString(string s)
		{
			return s.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim());
		}

		public Property AddProperty(PropertyGroup group, string id, PropertyType type, string defaultValue)
		{
			if (group== null)
				throw new ArgumentNullException("group");

			var property = new Property(id, GetTranslation(id), type, group.Properties.Count, defaultValue);
			property.DescriptionText = GetTranslation(id + "_Desc");
			group.Properties.Add(property);
			return property;
		}
	}
}
