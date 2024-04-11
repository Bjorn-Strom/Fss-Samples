open System
open Giraffe
open Giraffe.Htmx
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

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

    let toggleTodo guid =
        todos <- List.map (fun x -> if x.Id = guid then { x with Done = not x.Done } else x) todos

    addTodo (createTodo "Purchase groceries" false)
    addTodo (createTodo "Change tires on car." true)

module Views =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Htmx

    let todoList () = 
        ul [] 
            <| List.map (fun (todo: Domain.Todo) ->
                let styles =
                    if todo.Done then
                        $"{styles.counter} {styles.counterDone}"
                    else
                        styles.counter
                li [ _class styles; _hxPut $"/todo/{todo.Id}"; _hxTrigger HxTrigger.Click; _hxSwap HxSwap.OuterHtml; _hxTarget "closest ul" ] [ str todo.Title ]) Fakabase.todos

    let numTodos () = str $"Add #{List.length Fakabase.todos + 1}"

    let createTodo =
        fun _ (ctx: HttpContext) ->
            task {
                let title =
                    ctx.GetFormValue("title")
                    |> Option.defaultValue ""

                Fakabase.addTodo (Domain.createTodo title false)

                return! ctx.WriteHtmlViewAsync(todoList ())
            }

    let updateTodo guid : HttpHandler =
        fun _ (ctx: HttpContext) ->
            task {
                Fakabase.toggleTodo guid
                return! ctx.WriteHtmlViewAsync(todoList ())
            }

    let index () =
        html [] [
            head [] [
                title [] [ str "Todo" ]
                link [ _rel "preconnect"; _href "https://fonts.gstatic.com" ]
                link [ _rel "stylesheet"; _href "https://fonts.googleapis.com/css2?family=Roboto" ]
                link [ _rel "stylesheet"; _href "/css/styles.css" ]
                link [ _rel "stylesheet"; _href "/css/fonts.css" ]
                script [ _src "https://unpkg.com/htmx.org@1.9.11"; _integrity "sha384-0gxUXCCR8yv9FM2b+U3FDbsKthCI66oH5IA9fHppQq9DDMHuMauqq1ZHBpJxQ0J0"; _crossorigin "anonymous"] []
            ]
            body [] [
                div [ _class styles.container ]
                    [
                        h2 [ _class styles.header ] [ str "TODO" ]
                        div [ _id "todo-list"; _hxGet "/todos"; _hxTrigger HxTrigger.Load; _hxSwap HxSwap.InnerHtml ] [ ]
                        form [ _hxPost "/todo"; _hxTarget "#todo-list"; _hxSwap HxSwap.InnerHtml; (_hxOnHxEvent HxEvent.AfterRequest "if(event.detail.successful) this.reset()") ] [
                            input [ _class styles.input
                                    _name "title"
                                    _id "title"
                                    _type "text"
                                    _placeholder "What needs to be done?"
                                    _required ]
                            button [ _class styles.button; _type "Submit"; _hxTrigger "todoCreated from:body"; _hxGet "/num-todos"; _hxSwap HxSwap.InnerHtml; _hxTarget "this" ] [
                                numTodos ()
                            ]
                        ]
                    ]
            ]
        ]

let routes =
    choose [ 
        POST >=> route "/todo" >=> withHxTrigger "todoCreated" >=> Views.createTodo
        PUT >=> routef "/todo/%O" (fun guid -> Views.updateTodo guid)
        GET >=> choose [
            route "/" >=> htmlView (Views.index ()) 
            route "/todos" >=> warbler (fun _ -> htmlView (Views.todoList ()))
            route "/num-todos" >=> warbler (fun _ -> htmlView (Views.numTodos ()))
        ]
    ]

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffe routes
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