// Copy of the reactjs.org todo list.
module App

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fss
open Fss.Fable

// Colors
let blue = hex "0d6efd"
let darkBlue = hex "01398D"

// Font
let textFont = FontFamily.value "Roboto"
let container =
    fss
        [
            Display.flex
            FlexDirection.column
            Padding.value(rem 0., rem 1.5)
            textFont
        ]
let header = fss [ Color.value blue ]
let todoStyle =
    let fadeInAnimation =
        keyframes
            [
                frame 0
                    [
                        Opacity.value 0.
                        Transform.value [ Transform.translateY <| px 20 ]
                    ]
                frame 100
                    [
                        Opacity.value 1.
                        Transform.value [ Transform.translateY <| px 0 ]
                    ]
            ]
    let indexCounter = counterStyle []
    fss
        [
            CounterIncrement.value indexCounter
            FontSize.value (px 20)
            AnimationName.value fadeInAnimation
            AnimationDuration.value (sec 0.4)
            AnimationTimingFunction.ease
            ListStyleType.none
            Before
                [
                    Color.hex "48f"
                    Content.counter(indexCounter,". ")
                ]
        ]
let formStyle =
    [
        Display.inlineBlock
        Padding.value(px 10, px 15)
        FontSize.value (px 18);
        BorderRadius.value (px 0)
    ]
let buttonStyle =
    fss
        [
            yield! formStyle
            Border.none
            BackgroundColor.value blue
            Color.white
            Width.value (em 10.)
            Hover
                [
                    Cursor.pointer
                    BackgroundColor.value darkBlue
                ]
        ]
let inputStyle =
    fss
        [
            yield! formStyle
            BorderRadius.value (px 0)
            BorderWidth.thin
            MarginRight.value (px 25)
            Width.value (px 400)
        ]

type Model = {
    Input: string
    Todos: string list
}

type Msg =
    | SetInput of string
    | AddTodo of string

let init() = {
    Input = ""
    Todos = []
}

let update (msg: Msg) (model: Model): Model =
    match msg with
    | SetInput input -> { model with Input = input }
    | AddTodo todo ->
        { model with
            Todos = model.Todos @ [todo]
            Input = "" }

let render (model: Model) (dispatch: Msg -> unit) =
    div [ ClassName container ]
        [
            h2 [ ClassName header ] [ str "TODO" ]
            ul [] <| List.map (fun todo -> li [ ClassName todoStyle] [ str todo ]) model.Todos
            div []
                [
                    input
                        [
                            ClassName inputStyle
                            Placeholder "What needs to be done?"
                            Value model.Input
                            OnChange (fun e -> e.Value |> SetInput |> dispatch)
                        ]
                    button
                        [
                            ClassName buttonStyle
                            OnClick (fun _ -> model.Input |> AddTodo |> dispatch)
                        ]
                        [ str $"Add #{List.length model.Todos}" ]
                ]
        ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run
