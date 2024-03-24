open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.ViewEngine

open GeneratedStyles

module Domain =
    type Todo = { Id: Guid; Title: string; Done: bool }

    let createTodo title ``done`` =
        { Id = Guid.NewGuid()
          Title = title
          Done = ``done`` }

module Fakabase =
    open Domain
    let mutable todos: Todo list = []
    let addTodo (todo: Todo) = todos <- todos @ [ todo ]

    let updateTodo todo todos =
        List.map (fun x -> if x.Id = todo.Id then todo else x) todos

    addTodo (createTodo "Purchase groceries" false)
    addTodo (createTodo "Change tires on car." true)

module Views =
    let indexView () =
        html [] [
            head [] [
                title [] [ str "Todo" ]
                link [ _rel "preconnect"; _href "https://fonts.gstatic.com" ]
                link [ _rel "stylesheet"; _href "https://fonts.googleapis.com/css2?family=Roboto" ]
                link [ _rel "stylesheet"; _href "/css/index.css" ]
                link [ _rel "stylesheet"; _href "/css/fonts.css" ]
            ]
            body [] [
                div [ _class styles.container ]
                    [
                        h2 [ _class styles.header ] [ str "TODO" ]
                        ul []
                            <| List.map (fun (todo: Domain.Todo) -> li [ _class styles.counter] [ str todo.Title ]) Fakabase.todos
                        form [ _method "POST"; _action "/todo" ] [
                            input [ _class styles.input
                                    _name "title"
                                    _id "title"
                                    _type "text"
                                    _placeholder "What needs to be done?"
                                    _required ]
                            button [
                                _class styles.button
                                _type "Submit"
                            ] [
                                str $"Add #{List.length Fakabase.todos + 1}"
                            ]
                        ]
                    ]
            ]
        ]

module TodoControllers =
    let createTodo =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let title =
                    ctx.GetFormValue("title")
                    |> Option.defaultValue ""

                let newTodo =
                    Fakabase.addTodo (Domain.createTodo title false)

                return! ctx.WriteHtmlViewAsync(Views.indexView ())
            }

let apiRoutes =
    choose [ POST
             >=> route "/todo"
             >=> TodoControllers.createTodo ]

let uiRoutes =
    choose [ GET
             >=> route "/"
             >=> htmlView (Views.indexView ()) ]

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffe <| choose [ apiRoutes; uiRoutes ]
    app.UseStaticFiles() |> ignore

let configureServices (services: IServiceCollection) = services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0