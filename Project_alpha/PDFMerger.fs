namespace Project_alpha

open System
open System.IO
open WebSharper
open WebSharper.Sitelets
open PdfSharpCore.Pdf
open PdfSharpCore.Pdf.IO

[<Remote>]
type IPDFMerger =
    abstract member UploadFile : string * string -> Async<unit>
    abstract member MergePDFs  : list<string> -> Async<string>

type PDFMergerService() =
    interface IPDFMerger with
        member this.UploadFile(fileName: string, base64: string) = async {
            let uploads = "wwwroot/uploads"
            Directory.CreateDirectory(uploads) |> ignore
            let filePath = Path.Combine(uploads, fileName)
            let base64Data = base64.Substring(base64.IndexOf(',') + 1)
            let bytes = Convert.FromBase64String(base64Data)
            File.WriteAllBytes(filePath, bytes)
        }

        member this.MergePDFs(pdfFiles: list<string>) = async {
            let uploadFolder = "wwwroot/uploads"
            let mergedFolder = "wwwroot/merged"
            Directory.CreateDirectory(mergedFolder) |> ignore

            use outputDocument = new PdfDocument()

            for file in pdfFiles do
                let path = Path.Combine(uploadFolder, file)
                use inputDocument = PdfReader.Open(path, PdfDocumentOpenMode.Import)
                for i in 0 .. inputDocument.PageCount - 1 do
                    outputDocument.AddPage(inputDocument.Pages.[i]) |> ignore

            let ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            let mergedFileName = $"merged_{ts}.pdf"
            let outputPath = Path.Combine(mergedFolder, mergedFileName)
            outputDocument.Save(outputPath)

            return $"/merged/{mergedFileName}"
        }