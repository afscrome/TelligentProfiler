The Evolution Profiler utility provides you with information about how the page was
loaded, as well as how long each part took to load. 

This is perfect for troubleshooting cases where specific pages are taking longer to
load than expected as it will allow you to narrow down exactly where the performance
issue is located.

This tool may be of less help if you’re experiencing site wide performance issues
– it will help if there is one particular cause on every page that’s being slow, but
will not help in the case of a load related issue where everything is running slow.

The profiling functionality is powered by http://miniprofiler.com/


================================
        USING THE ADDON
================================

After a page has loaded, a list of timings will be shown in the top left of the screen,
each one showing how long it has taken to load a resource used by the current page.
Clicking on one of these these timings will open a popup showing a hierarchy of the
operations taken to that resource.

By default, performance results will be shown to all users accessign the community from
the server itself, as well as Administrators.  This can be customised by configuring the
Performance Profiler plugin.


================================
          INSTALLATION
================================

    1. Copy the contents of the /web/ folder into your community.

    2. Go to the Manage Plugins Page in your community (Control Panel > Site
       Administration > > Manage Plugins)

    3. Enable the "Performance Profiler" plugin

================================
    BUILDING THE SOURCE CODE
================================

    1. Copy the following files from your Telligent Evolution website's /bin/ directory
       to the /References/ folder
          * Telligent.Caching.dll
          * Telligent.Common.dll
          * Telligent.Evolution.Components.dll

    2. Open the EvolutionProfiler.sln solution in Visual Studio and Compile

Output will be copied to the /Deploy/ folder

================================
        TROUBLESHOOTING
================================

Some users may find that the profiler popup fails to load after installing the plugin
due to the MiniProfiler resources from /utility/profiler returning 404s.  I'm not
entirely sure what is causing, however it appears to be a bug in .net 4.0 as I have yet
to see this issue occur on a machine with .net 4.5 installed.

As a workaround for this issue, register the following handler in the <handlers /> section of your community's web.config.

    <add name="MiniProfiler"
         path="utility/profiler/*"
         verb="*"
         type="System.Web.Routing.UrlRoutingModule"
         resourceType="Unspecified"
         preCondition="integratedMode"
         />
