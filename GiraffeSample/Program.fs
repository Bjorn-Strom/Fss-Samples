open System
open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection

open Fss

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
    module Styling =
        // Colors
        let private blue = hex "0d6efd"
        let private darkBlue = hex "01398D"
        // Fonts
        let private textFont = FontFamily.value "Roboto"
        // Elements
        let containerClass, container =
            fss [ Display.flex
                  FlexDirection.column
                  Padding.value (rem 0., rem 1.5)
                  textFont ]

        let headerClass, header = fss [ Color.value blue ]

        let fadeAnimationName, fadeAnimation =
            keyframes [ frame
                            0
                            [ Opacity.value 0.
                              Transform.value [ Transform.translateY <| px 20 ] ]
                        frame
                            100
                            [ Opacity.value 1.
                              Transform.value [ Transform.translateY <| px 0 ] ] ]

        let todoClass, indexCounters =
            let counterName, counter = counterStyle [ CounterLabel "indexCounter" ]

            let indexCounterName, indexCounter =
                fss [ CounterIncrement.value counterName
                      FontSize.value (px 20)
                      AnimationName.value fadeAnimationName
                      AnimationDuration.value (sec 0.4)
                      AnimationTimingFunction.ease
                      ListStyleType.none
                      Before [ Color.hex "48f"
                               Content.counter (counterName, ". ") ] ]

            indexCounterName, [ counter; indexCounter ]

        let private formStyle =
            [ Display.inlineBlock
              Padding.value (px 10, px 15)
              FontSize.value (px 18)
              BorderRadius.value (px 0) ]

        let buttonClass, button =
            fss [ yield! formStyle
                  Border.none
                  BackgroundColor.value blue
                  Color.white
                  Width.value (em 10.)
                  Hover [ Cursor.pointer
                          BackgroundColor.value darkBlue ] ]

        let inputClass, input =
            fss [ yield! formStyle
                  Label "InputClass"
                  BorderWidth.thin
                  MarginRight.value (px 25)
                  Width.value (px 400) ]

    let indexView () =
        html [] [
            head [] [
                title [] [ str "Todo" ]
                Styling.container
                Styling.header
                Styling.fadeAnimation
                yield! Styling.indexCounters
                Styling.button
                Styling.input
                link [ _rel "preconnect"; _href "https://fonts.gstatic.com" ]
                link [ _rel "stylesheet"; _href "https://fonts.googleapis.com/css2?family=Roboto" ]
            ]
            body [] [
                div [ _class Styling.containerClass ]
                    [
                        h2 [ _class Styling.headerClass ] [ str "TODO" ]
                        ul []
                            <| List.map (fun (todo: Domain.Todo) -> li [ _class Styling.todoClass ] [ str todo.Title ]) Fakabase.todos
                        form [ _method "POST"; _action "/todo" ] [
                            input [ _class Styling.inputClass
                                    _name "title"
                                    _id "title"
                                    _type "text"
                                    _placeholder "What needs to be done?"
                                    _required ]
                            button [
                                _class Styling.buttonClass
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