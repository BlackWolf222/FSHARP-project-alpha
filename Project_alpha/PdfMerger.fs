namespace Project_alpha
open System
open System.IO
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open PdfSharpCore.Pdf
open PdfSharpCore.Pdf.IO

module PdfMerger =

    let readPages (stream: Stream) =
        try
            use doc = PdfReader.Open(stream, PdfDocumentOpenMode.Import)
            [ for i in 0 .. doc.Pages.Count - 1 -> doc.Pages.[i] ]
        with ex ->
            failwithf "Failed to read pages from PDF: %s" ex.Message

    let createDocFromPages pages =
        if List.isEmpty pages then
            failwith "No pages to merge into the PDF."
        let targetDoc = new PdfDocument()
        pages |> List.iter (fun p -> targetDoc.Pages.Add(p) |> ignore)
        use ms = new MemoryStream()
        targetDoc.Save(ms, false)
        ms.ToArray()

    [<Rpc>]
    let MergePdfs (files: list<string * byte[]>) : Async<byte[]> =
        async {
            if List.isEmpty files then
                failwith "No files provided for merging."

            let allPages =
                files
                |> List.collect (fun (name, bytes) ->
                    try
                        use ms = new MemoryStream(bytes)
                        readPages ms
                    with ex ->
                        failwithf "Failed to read pages from file '%s': %s" name ex.Message
                )

            if List.isEmpty allPages then
                failwith "No pages found in the provided files."

            let mergedPdf = createDocFromPages allPages
            return mergedPdf
        }
