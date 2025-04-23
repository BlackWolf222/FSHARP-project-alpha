open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open WebSharper.AspNetCore
open Project_alpha

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    // Add services to the container.
    builder.Services.AddWebSharper()
        .AddAuthentication("WebSharper")
        .AddCookie("WebSharper", fun options -> ())
    |> ignore

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
        //Enable if you want to make RPC calls to server
        .UseRouting()
        .UseEndpoints(fun endpoints ->
            endpoints.MapPost("/merge", Func<HttpContext, Task>(fun ctx ->
                task {
                    let files = ctx.Request.Form.Files
                    match Project_alpha.PdfMergeHandler.mergePdfs files with
                    | Ok pdfBytes ->
                        ctx.Response.ContentType <- "application/pdf"
                        ctx.Response.ContentLength <- Nullable(int64 pdfBytes.Length)
                        do! ctx.Response.Body.WriteAsync(pdfBytes, 0, pdfBytes.Length)
                    | Error msg ->
                        ctx.Response.StatusCode <- 400
                        do! ctx.Response.WriteAsync(msg)
                }
            )) |> ignore
        )
    |> ignore 
       
    app.Run()

    0 // Exit code
