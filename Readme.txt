================================
        ABOUT THE ADDON
================================

The Evolution Profiler addon provides metrics about how long various operations performed
in the rendering of a page take to load.  This information can be used to track down
performance issues.

The profiling functionality is powered by http://miniprofiler.com/


================================
        USING THE ADDON
================================

After a page has loaded, a list of timings will be shown in the top left of the screen
showing how long it has taken to load a number of resources on your page. Clicking on
these timings will open a popup showing a hierarchy of the operations taken to load 


The addon can be configured via the Profiler.config file in your website root:

    localOnly - Profiler results will only be shown when viewing from a local browser

    trivialDuration - actions taking less than this time will be hidden in the trace
        window unless you click on the "Show Trivial" link

    excludePaths - a comma seperated lists of paths to exclude from profiling

    popupPosition (Left or Right) - Sets where the profiler metrics will be displayed

    maxTracesToShow - the maximum number or traces to show at any one time


================================
          INSTALLATION
================================

    1. Copy the contents of the /web/ folder into your community.

    2. Configure the diagnostics.config as required

    3. Open your community in your web browser.  PRofiling results should be displayed
       on the left or right hand side of the page.


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
