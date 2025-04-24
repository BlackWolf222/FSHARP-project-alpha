open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open WebSharper.AspNetCore
open Microsoft.Extensions.DependencyInjection
open WebSharper.Remoting
open WebSharper.Web
open WebSharper.Sitelets
open WebSharper
open Project_alpha.PdfMerger


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    // Add services to the container.
    builder.Services.AddWebSharper()
        .AddAuthentication("WebSharper")
        .AddCookie("WebSharper", fun options -> ())
    |> ignore

    builder.Services.AddControllers()

    let app = builder.Build()

    // Configure the HTTP request pipeline.
    if not (app.Environment.IsDevelopment()) then
        app.UseExceptionHandler("/Error")
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            .UseHsts()
        |> ignore
    
    app.UseHttpsRedirection()
#if DEBUG        
        .UseWebSharperScriptRedirect(startVite = true)
#endif
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseRouting()
        .UseWebSharper()
        .UseWebSharperRemoting()
        .UseEndpoints(fun endpoints ->
            endpoints.MapControllers() |> ignore
            endpoints.MapDefaultControllerRoute() |> ignore
    )
    |> ignore 
       
    app.Run()

    0 // Exit code
