
# Project Alpha - PDF Merger

**Project Alpha**
The F# PDF Merger is a specialized tool designed to combine multiple PDF documents into a single cohesive file.
This application leverages the functional programming capabilities of F# to provide a clean, efficient solution for PDF merging tasks that are common in both professional and academic environments.

**Try-live link:**
https://blackwolf222.github.io/FSHARP-project-alpha

---

## Motivation

This project was born from the need to efficiently combine multiple PDF documents, which is a frequent requirement in both academic and business settings.
I found these kinds of applications particularly useful for:

- Academic Work: Combining research papers, lecture notes, or assignment components
- Professional Documentation: Merging reports, contracts, or presentation materials
- Publishing: Assembling chapters or sections into complete documents
- Administrative Tasks: Combining forms, records, or documentation

The F# functional programming paradigm provides a concise and robust framework for handling document operations, making the PDF merging process more reliable and straightforward.

---

## Features

- Upload multiple PDF files
- Merge them into a single document
- Download the resulting PDF
- Fast and server-side processing using `PdfSharpCore`

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or later with F# and ASP.NET workloads
- Node.js (if using NPM-based client tooling)
- [WebSharper](https://websharper.com/)

---

### Build Instructions

1. **Clone the repository:**

   ```bash
   git clone https://github.com/your-username/project-alpha.git
   cd project-alpha
   ```

2. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

3. **Build the project:**

   ```bash
   dotnet build
   ```

4. **Run the server:**

   ```bash
   dotnet run --project Project_alpha
   ```

   Or simply press **F5** in Visual Studio.

---

### Usage

- Navigate to the website
- Select and upload multiple PDF files
- Click "Merge"
- Download your merged PDF!

---

## Project Structure

```
Project_alpha/
│
├── Client/                 # Client-side WebSharper code
├── Server.fs              # Main server-side logic
├── Webserver.fs           # Sitelet definition and routing
├── Startup.fs             # ASP.NET pipeline configuration
├── PdfMergeHandler.fs     # PDF merging logic using PdfSharpCore
├── wwwroot/               # Static assets
```

---

## Dependencies

- [WebSharper.UI](https://websharper.com/docs/ui)
- [PdfSharpCore](https://github.com/ststeiger/PdfSharpCore)
- ASP.NET Core
- F# (.NET 7+)

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## Author

Created by Beödők Levente. Suggestions and PRs welcome!
