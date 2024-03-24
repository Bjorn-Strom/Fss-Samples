[<AutoOpen>]
module Styles

open Fss
open Fss.Static

// Colors
let private blue = hex "0d6efd"
let private darkBlue = hex "01398D"
// Fonts
let private textFont = FontFamily.value "Roboto"

// TODO: Kan vi hente navn rett ut fra variabelen?
let globalBoxSizing = global' [
    BoxSizing.borderBox
    BackgroundColor.whiteSmoke
]

let container = fss "container" [
    Display.flex
    FlexDirection.column
    Padding.value (rem 0., rem 1.5)
    textFont
]

let header = fss "header" [
    Color.value blue
]

let private formStyle =
    [ Display.inlineBlock
      Padding.value (px 10, px 15)
      FontSize.value (px 18)
      BorderRadius.value (px 0) ]

let button = fss "button" [
    yield! formStyle
    Border.none
    BackgroundColor.value blue
    Color.white
    Width.value (em 10.)
    Hover [
        Cursor.pointer
        BackgroundColor.value darkBlue 
    ]
]

let input = fss "input" [
    yield! formStyle
    Label "InputClass"
    BorderWidth.thin
    MarginRight.value (px 25)
    Width.value (px 400) 
]

let fadeAnimation = keyframes "fadeAnimation" [ 
    frame 0 [
        Opacity.value 0. 
        Transform.value [ Transform.translateY <| px 20 ] 
    ]
    frame 100 [
        Opacity.value 1.  
        Transform.value [ Transform.translateY <| px 0 ]
    ] 
]

let indexCounter = counterStyle "indexCounter" [ CounterLabel "indexCounter" ]

// TODO: Deal med animasjoner
let counter =
    fss "counter" [
        CounterIncrement.value "indexCounter"
        FontSize.value (px 20)
        AnimationName.value (fst (snd fadeAnimation))
        AnimationDuration.value (sec 0.4)
        AnimationTimingFunction.ease
        ListStyleType.none
        Before [ Color.hex "48f"
                 Content.counter ("indexCounter", ". ") ]
    ]

let droidSerifFont =
    fontFaces
        "DroidSerifFontFace"
        [ [ FontFace.Src.truetype("https://rawgit.com/google/fonts/master/ufl/ubuntu/Ubuntu-Bold.ttf")
            FontFace.FontWeight.bold
            FontFace.FontStyle.normal ]
          [ FontFace.Src.truetype("https://rawgit.com/google/fonts/master/ufl/ubuntumono/UbuntuMono-Italic.ttf")
            FontFace.FontWeight.normal
            FontFace.FontStyle.normal ] ]

let modernaFont =
    fontFace
        "modernaFontFace"
        [ FontFace.Src.sources
              [ Fss.Types.FontFace.Woff2 "https://s3-us-west-2.amazonaws.com/s.cdpn.io/133207/moderna_-webfont.woff2"
                Fss.Types.FontFace.Woff "https://s3-us-west-2.amazonaws.com/s.cdpn.io/133207/moderna_-webfont.woff"
                Fss.Types.FontFace.Truetype "https://s3-us-west-2.amazonaws.com/s.cdpn.io/133207/moderna_-webfont.ttf"
                Fss.Types.FontFace.Svg "https://s3-us-west-2.amazonaws.com/s.cdpn.io/133207/moderna_-webfont.svg" ]
          FontFace.FontWeight.normal
          FontFace.FontStyle.normal ]

let droidSerif =
    fss "droidSerif" [
          Label "Droid Serif"
          FontFamily.value "DroidSerifFontFace" ]

let droidSerifBold =
    fss "droidSerifBold" [ 
          Label "Droid Serif Bold"
          FontFamily.value "DroidSerifFontFace"
          FontWeight.bold ]

let moderna =
    fss "moderna" [
          Label "Moderna"
          FontFamily.value "modernaFontFace" ]
