namespace Project_alpha

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.UI.Html

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    // ListModel to store the uploaded file names
    let UploadedFiles = ListModel.Create (fun (s: string) -> s) []

    // Function to handle file selection
    let HandleFileSelect (files: FileList) =
        UploadedFiles.Clear()
        for i in 0 .. files.Length - 1 do
            let file = files.[i]
            UploadedFiles.Add(file.Name)

    [<SPAEntryPoint>]
    let Main () : Doc =
        let dropzone = JS.Document.GetElementById("drop") :?> HTMLElement
        let fileInput = JS.Document.GetElementById("myFile") :?> HTMLInputElement
        let fileselect = JS.Document.GetElementById("File") :?> HTMLInputElement
        let mergeBtn = JS.Document.GetElementById("mergeBtn") :?> HTMLButtonElement
        let resultDiv = JS.Document.GetElementById("result") :?> HTMLElement

        let mutable selectedFiles : FileList option = None

        let preventDefault (e: Dom.Event) =
            e.PreventDefault()
            e.StopPropagation()

        let updateFiles (files: FileList) =
            selectedFiles <- Some files
            UploadedFiles.Clear()
            for i in 0 .. files.Length - 1 do
                let file = files.[i]
                UploadedFiles.Add(file.Name)

        dropzone.AddEventListener("dragover", System.Action<Dom.Event>(preventDefault))
        dropzone.AddEventListener("dragenter", System.Action<Dom.Event>(preventDefault))

        dropzone.AddEventListener("drop", System.Action<Dom.Event>(fun ev ->
            preventDefault ev
            let files = ev?dataTransfer?files |> unbox<FileList> // Dynamically access dataTransfer and files
            updateFiles files
        ))

        fileInput.AddEventListener("change", System.Action<Dom.Event>(fun _ ->
            updateFiles fileInput.Files
        ))

        fileselect.AddEventListener("change", System.Action<Dom.Event>(fun _ ->
            updateFiles fileselect.Files
        ))

        mergeBtn.AddEventListener("click", System.Action<Dom.Event>(fun _ ->
            match selectedFiles with
            | Some files when files.Length > 0 ->
                let filePromises =
                    [ for i in 0 .. files.Length - 1 ->
                        async {
                            let file = files.[i]
                            let! arrayBuffer = file.ArrayBuffer() |> Promise.AsAsync
                            let uint8Array = new Uint8Array(arrayBuffer) 
                            let byteArray = Array.init uint8Array.Length (fun i -> uint8Array.Get(i)) // Use Get() to access elements
                            return file.Name, byteArray
                        }
                    ]
                async {
                    resultDiv.InnerText <- "Merging PDFs, please wait..."
                    let! fileData = Async.Parallel filePromises
                    let! mergedPdf = PdfMerger.MergePdfs (fileData |> Array.toList)
                    
                    // Convert the byte[] to Uint8Array
                    let uint8Array = new Uint8Array(mergedPdf)
                    
                    // Create a Blob from the Uint8Array
                    let blob = new Blob([| uint8Array :> ArrayBufferView |], BlobPropertyBag(Type = "application/pdf"))
                    let url = URL.CreateObjectURL(blob)

                    // Create an anchor element and trigger the download
                    let a = JS.Document.CreateElement("a") :?> HTMLElement
                    a.SetAttribute("href", url)
                    a.SetAttribute("download", "merged.pdf")
                    a.Style.SetProperty("display", "none")
                    JS.Document.Body.AppendChild(a) |> ignore
                    
                    a.Click()

                    URL.RevokeObjectURL(url)
                    JS.Document.Body.RemoveChild(a) |> ignore

                    resultDiv.InnerText <- "PDFs merged successfully!"
                }
                |> Async.Start
            | _ ->
                resultDiv.InnerText <- "Please select PDF files first."
        ))

        let fileDisplay =
            UploadedFiles.View.DocSeqCached(fun fileName ->
                Doc.Element "div" [] [text fileName]
            )

        Doc.RunById "fdisplay" fileDisplay

        Doc.Empty