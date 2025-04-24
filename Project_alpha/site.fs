namespace Project_alpha

open WebSharper
open WebSharper.Sitelets

module Site =
    type EndPoint = | Home

    [<Website>]
    let Main =
        Sitelet.Infer (fun _ action ->
            match action with
            | Home -> Content.Text "OK"
        )