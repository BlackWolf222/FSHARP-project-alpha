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
            let files = ev.Target?dataTransfer?files |> unbox<FileList>
            updateFiles files
        ))

        fileInput.AddEventListener("change", System.Action<Dom.Event>(fun _ ->
            updateFiles fileInput.Files
        ))

        mergeBtn.AddEventListener("click", System.Action<Dom.Event>(fun _ ->
            match selectedFiles with
            | Some files when files.Length > 0 ->
                let formData = new FormData()
                for i in 0 .. files.Length - 1 do
                    formData.Append("pdfs", files.[i])
                let xhr = new XMLHttpRequest()
                xhr.Open("POST", "/merge")
                xhr.ResponseType <- XMLHttpRequestResponseType.Blob
                xhr.OnLoad <- fun _ ->
                    if xhr.Status = 200 then
                        let blob = xhr.Response :?> Blob
                        let url = URL.CreateObjectURL(blob)

                        let a = JS.Document.CreateElement("a") :?> HTMLElement
                        a.SetAttribute("href", url)
                        a.SetAttribute("download", "merged.pdf")
                        a.Style.SetProperty("display", "none")
                        JS.Document.Body.AppendChild(a) |> ignore
                        a.Click()

                        URL.RevokeObjectURL(url)
                        JS.Document.Body.RemoveChild(a) |> ignore
                        
                        resultDiv.InnerText <- "PDFs merged successfully!"

                    else
                        resultDiv.InnerText <- "Error: " + xhr.StatusText
                xhr.Send(formData)
            | _ ->
                resultDiv.InnerText <- "Please select PDF files first."
        ))

        let fileDisplay =
            UploadedFiles.View.DocSeqCached(fun fileName ->
                Doc.Element "div" [] [text fileName]
            )

        Doc.RunById "fdisplay" fileDisplay

        Doc.Empty