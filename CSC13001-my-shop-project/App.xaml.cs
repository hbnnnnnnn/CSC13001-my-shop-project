using System.Diagnostics.CodeAnalysis;
using CSC13001_my_shop_project.Models;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Presentation.Products;
using CSC13001_my_shop_project.Services;
using Uno.Extensions.Navigation;
using CSC13001_my_shop_project.Presentation.OrderList;
using Uno.Resizetizer;

namespace CSC13001_my_shop_project;

public partial class App : Application
{
    /// <summary>Host after launch; used by shell view model to resolve <c>INavigator</c>.</summary>
    internal static IHost? AppHost { get; private set; }
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    [SuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context."
    )]
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host =>
                host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                    .UseLogging(
                        configure: (context, logBuilder) =>
                        {
                            // Configure log levels for different categories of logging
                            logBuilder
                                .SetMinimumLevel(
                                    context.HostingEnvironment.IsDevelopment()
                                        ? LogLevel.Information
                                        : LogLevel.Warning
                                )
                                // Default filters for core Uno Platform namespaces
                                .CoreLogLevel(LogLevel.Warning);

                            // Uno Platform namespace filter groups
                            // Uncomment individual methods to see more detailed logging
                            //// Generic Xaml events
                            //logBuilder.XamlLogLevel(LogLevel.Debug);
                            //// Layout specific messages
                            //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                            //// Storage messages
                            //logBuilder.StorageLogLevel(LogLevel.Debug);
                            //// Binding related messages
                            //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                            //// Binder memory references tracking
                            //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                            //// DevServer and HotReload related
                            //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                            //// Debug JS interop
                            //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);
                        },
                        enableUnoLogging: true
                    )
                    .UseConfiguration(configure: configBuilder =>
                        configBuilder.EmbeddedSource<App>().Section<AppConfig>()
                    )
                    // Enable localization (see appsettings.json for supported languages)
                    .UseLocalization()
                    .UseHttp(
                        (context, services) =>
                        {
#if DEBUG
                            // DelegatingHandler will be automatically injected
                            services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                        }
                    )
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<NavigationStateStore>();
                        services.AddTransient<ProductDetailViewModel>();
                    })
                    .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
        AppHost = Host;
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<DashboardPage, DashboardViewModel>(),
            new ViewMap<ProductsPage, ProductsViewModel>(),
            new DataViewMap<ProductDetailPage, ProductDetailViewModel, ProductDetailArgs>(),
                new ViewMap<OrderListPage, OrderListViewModel>()
        );

        routes.Register(
            new RouteMap(
                "",
                View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new RouteMap("Dashboard", View: views.FindByViewModel<DashboardViewModel>(), IsDefault: true),
                    new RouteMap(
                        "Products",
                        View: views.FindByViewModel<ProductsViewModel>(),
                        Nested:
                        [
                            new RouteMap(
                                "ProductDetail",
                                View: views.FindByViewModel<ProductDetailViewModel>()
                            ),
                        ]
                    ),
                    new RouteMap("Orders", View: views.FindByViewModel<OrderListViewModel>()),
                ]
            )
        );
    }
}
