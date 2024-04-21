[<AutoOpen>]
module Styles

open Fss
open Fss.Static

// Colors
let dark_grey = "#1E2122"
let light_grey = "#E8E6E3"
let black = "#111213"
let white = "#D4E4DC"
let green = "2DBA4E"


let global_ = global' [
    BoxSizing.borderBox
]

let roboto_font = 
    fontFace "roboto"
        [ FontFace.Src.sources [ Fss.Types.FontFace.Truetype "/fonts/roboto.ttf" ]
          FontFace.FontWeight.normal
          FontFace.FontStyle.normal ]

let body = fss "body" [
    Overflow.hidden
    Color.hex light_grey
    FontFamily.value roboto_font
]

let nav_bar = fss "navbar" [
    GridArea.value "nav"
    BackgroundColor.hex dark_grey
    Display.flex
    FlexDirection.column
    Custom "gap" "10px"
    Width.value (pct 100)
]

let search_bar = fss "search_bar" [
    GridArea.value "head"
    BackgroundColor.hex green
]

let app_container = fss "app_container" [
    Display.grid
    GridTemplateAreas.value [
        ["head"; "head"]
        ["nav"; "main"]
    ]
    GridTemplateRows.value ([ px 70; vh 100 ] : LengthUnit list)
    GridTemplateColumns.value ([ pct 15; vw 86] : LengthUnit list)
    Margin.value (px -8, px 0, px 0, px -8)
]

let channel_list = fss "channel_list" [
    Cursor.pointer
    FontSize.larger
    Width.value (pct 100)
    Padding.value (px 10)
    Display.flex
    JustifyContent.center
    Hover [
        BackgroundColor.hex green
        Width.value (pct 100)
    ] 
]

let channel_info = fss "channel_info" [
    Display.flex
    AlignItems.center
    JustifyContent.spaceBetween
]

let channel= fss "channel" [
    GridArea.value "main"
    BackgroundColor.hex black
    Padding.value (px 20)
]

let messages = fss "messages" [
    Display.flex
    FlexDirection.column
    Custom "row-gap" "10px"
    Width.value (pct 90)
]

let message_info = fss "message_info" [
    Display.flex
    Custom "gap" "10px"

    !> (Selector.Tag Fss.Types.Html.Div) [
        Display.flex
        Custom "gap" "20px"
        AlignItems.center

        FirstChild [
            FontWeight.bold
        ]

        LastChild [
            FontSize.smaller
        ]
    ]
]

let message = fss "message" [
    BackgroundColor.hex black
    Padding.value (px 20)
    Display.flex
    FlexDirection.column
    Custom "gap" "10px"
]

let chat = fss "chat" [
    Display.flex
    ColumnGap.value (px 10)
    AlignItems.end'
]

let label = fss "label" [
    Display.flex
    FlexDirection.column
]

let button = fss "button" [
    Height.minContent
    Padding.value (px 5)
]
















