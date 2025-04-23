namespace Project_alpha

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.UI.Html
open Microsoft.AspNetCore.Http
open PdfSharpCore.Pdf
open PdfSharpCore.Pdf.IO
open System.IO
open System
open System.Threading.Tasks
open WebSharper.AspNetCore.Sitelets
open Project_alpha.Client

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/merged">] Merge

module PdfMergeHandler =
    let mergePdfs (files: IFormFileCollection) : Result<byte[], string> =
        try
            if Seq.isEmpty files then
                Error "No files were provided"
            else
                use outputDocument = new PdfDocument()
                
                for file in files do
                    use ms = new MemoryStream()
                    file.OpenReadStream().CopyTo(ms)
                    ms.Position <- 0L
                    use inputDocument = PdfReader.Open(ms, PdfDocumentOpenMode.Import)
                    for i in 0 .. inputDocument.PageCount - 1 do
                        outputDocument.AddPage(inputDocument.Pages.[i]) |> ignore

                use outStream = new MemoryStream()
                outputDocument.Save(outStream, closeStream=false)
                Ok (outStream.ToArray())
        with ex ->
            Error ex.Message

module Server =
    open Microsoft.AspNetCore.Builder
    open Microsoft.Extensions.DependencyInjection

    let mergeHandler : SiteletHttpHandler =
        fun (next: SiteletHttpFunc) (ctx: HttpContext) ->
            task {
                if ctx.Request.Method = "POST" then
                    let files = ctx.Request.Form.Files
                    match PdfMergeHandler.mergePdfs files with
                    | Ok pdfBytes ->
                        ctx.Response.ContentType <- "application/pdf"
                        ctx.Response.ContentLength <- Nullable(int64 pdfBytes.Length)
                        do! ctx.Response.Body.WriteAsync(pdfBytes, 0, pdfBytes.Length)
                        return Some ctx
                    | Error msg ->
                        ctx.Response.StatusCode <- 400
                        do! ctx.Response.WriteAsync(msg)
                        return Some ctx
                else
                    ctx.Response.StatusCode <- 405
                    do! ctx.Response.WriteAsync("Only POST method is supported")
                    return Some ctx
            }

    [<Website>]
    let Main : Sitelet<EndPoint> =
        Application.MultiPage(fun ctx endpoint ->
        match endpoint with
        | Home -> 
            Content.Page(
                Title = "PDF Merger",
                Body = [ div [] [Client.Main()] ]
            )
        | _ -> Content.Text "Unknown endpoint"
    )