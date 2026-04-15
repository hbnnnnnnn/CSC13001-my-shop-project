using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Models;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Presentation.Login;
using CSC13001_my_shop_project.Presentation.OrderList;
using CSC13001_my_shop_project.Presentation.Products;
using CSC13001_my_shop_project.Presentation.ServerConfiguration;
using CSC13001_my_shop_project.Services;
using CSC13001_my_shop_project.Services.Endpoints;
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
                            services.AddTransient<AuthTokenHandler>();
                            services
                                .AddHttpClient(
                                    "BackendApi",
                                    client =>
                                    {
                                        var baseUrl =
                                            context.Configuration["ApiClient:Url"]
                                            ?? "http://localhost:4000";
                                        client.BaseAddress = new Uri(baseUrl);
                                        client.DefaultRequestHeaders.Add(
                                            "Accept",
                                            "application/json"
                                        );
                                    }
                                )
                                .AddHttpMessageHandler<AuthTokenHandler>();

#if DEBUG
                            services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                        }
                    )
                    .ConfigureServices(
                        (context, services) =>
                        {
                            services.AddSingleton<NavigationStateStore>();
                            services.AddTransient<ProductDetailViewModel>();
                            services.AddSingleton<AppStateService>();

                            // API services
                            services.AddSingleton<GraphqlService>();
                            services.AddSingleton<AuthService>();
                        }
                    )
                    .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
        AppHost = Host;

        NavigateOnStartup();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        var shellMap = new ViewMap();

        views.Register(
            shellMap,
            new ViewMap<DashboardPage, DashboardViewModel>(),
            new ViewMap<OrderListPage, OrderListViewModel>(),
            new ViewMap<ProductsPage, ProductsViewModel>(),
            new DataViewMap<ProductDetailPage, ProductDetailViewModel, ProductDetailArgs>(),
                new ViewMap<OrderListPage, OrderListViewModel>(),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<ServerConfigurationPage, ServerConfigurationViewModel>()
        );

        routes.Register(
            new RouteMap(
                "",
                View: shellMap,
                Nested:
                [
                    new RouteMap(
                        "Login",
                        View: views.FindByViewModel<LoginViewModel>(),
                        IsDefault: true
                    ),
                    new RouteMap("Dashboard", View: views.FindByViewModel<DashboardViewModel>()),
                    new RouteMap("Orders", View: views.FindByViewModel<OrderListViewModel>()),
                    new RouteMap(
                        "ServerConfiguration",
                        View: views.FindByViewModel<ServerConfigurationViewModel>()
                    ),
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

    private async void NavigateOnStartup()
    {
        var authService = Host?.Services.GetRequiredService<AuthService>();
        var appState = Host?.Services.GetRequiredService<AppStateService>();

        // If the user has a stored token ("Remember Me"), validate it
        if (authService?.IsLoggedIn == true)
        {
            var account = await authService.GetCurrentAccountAsync();
            if (account is not null)
            {
                if (appState?.LastPage is string lastPage)
                {
                    WeakReferenceMessenger.Default.Send(new NavigateToPageMessage(lastPage));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Dashboard"));
                }

                return;
            }
        }

        // No valid token — show login
        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Login"));
    }
}
