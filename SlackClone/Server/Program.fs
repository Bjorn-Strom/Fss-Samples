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
    type Message = {
        Id: Guid
        User: string
        Sent: DateTime
        Edited: DateTime
        Message: string
    }

    type Channel = {
        Id: Guid
        Name: string
        Messages: Message list
    }

    type Workspace = {
        Id: Guid
        DefaultChannel: Channel
        Name: string
    }


module Fakabase =
    open Domain
    let mutable channels: Channel list = []

    let create_message message user = 
        {
            Id = Guid.NewGuid()
            User = user
            Sent = DateTime.Now
            Edited = DateTime.Now
            Message = message
        }

    let create_channel name messages =
        { Id = Guid.NewGuid() 
          Name = name
          Messages = messages
        }

    let add_channel (channel: Channel) = channels <- channels @ [ channel ]

    let update_channel (channel: Channel) =
        channels <- List.map (fun (x: Channel) -> if x.Id = channel.Id then channel else x) channels

    add_channel (create_channel "General" [ create_message "Welcome!" "Per"; create_message "Thank you!" "Pål"])
    add_channel (create_channel "Random" [])

    let add_message message user channel_id =
        let message_to_add = create_message message user
        let current_channel = List.find (fun (c: Domain.Channel) -> c.Id = channel_id) channels
        update_channel { current_channel with Messages = current_channel.Messages@[message_to_add] }

    let count_users_in_channel channel =
        channel.Messages
        |> List.map (fun m -> m.User)
        |> List.distinct 
        |> List.length

    let workspace = {
        Id = Guid.NewGuid()
        Name = "Workspace"
        DefaultChannel = List.head channels
    }

module Views =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Htmx

    let page _body =
        html [] [
            head [] [
                title [] [ str "Taut" ]
                link [ _rel "stylesheet"; _href "/css/styles.css" ]
                script [ _src "https://unpkg.com/htmx.org@1.9.11"; _integrity "sha384-0gxUXCCR8yv9FM2b+U3FDbsKthCI66oH5IA9fHppQq9DDMHuMauqq1ZHBpJxQ0J0"; _crossorigin "anonymous"] []
            ]
            body [ _class Styles.body ] _body
        ]

    let channel_list_item (channel: Domain.Channel) =
        div [ _class Styles.channel_list; _hxGet $"/channel/{channel.Id}"; _hxTrigger HxTrigger.Click; _hxTarget "#channel"; _hxSwap HxSwap.OuterHtml ]
            [
                span [] [ str channel.Name ]
            ]

    let channel_list () =
        nav [ _class Styles.navbar ]
            (List.map channel_list_item Fakabase.channels)

    let search_bar () =
        header [ _class Styles.search_bar ] [
        ]

    let message (message: Domain.Message) =
        div [ _class Styles.message ] [
            div [ _class Styles.message_info ] [
                div []
                    [
                        str message.User
                    ]
                div []
                    [
                        str $"""{message.Sent.ToString("HH:mm dd-MM-yyyy")}"""
                    ]

            ]
            div [] [
                str message.Message
            ]
        ]
    
    let messages (channel_id: Guid) = 
        let current_channel = List.tryFind (fun (c: Domain.Channel) -> c.Id = channel_id) Fakabase.channels
        match current_channel with
        | Some channel -> 
            div [ _id "messages"; _class Styles.messages ] (List.map message channel.Messages)
        | None ->
            div [ _id "messages"; _class Styles.messages ] []


    let channel (channel_id: Guid) =
        let current_channel = List.tryFind (fun (c: Domain.Channel) -> c.Id = channel_id) Fakabase.channels
        div [ _id "channel"; _class Styles.channel; _hxGet $"/messages/{channel_id}"; _hxTrigger (HxTrigger.Every "5s"); _hxSwap HxSwap.InnerHtml; _hxTarget "#messages" ] [
            match current_channel with
            | Some channel ->
                div [ _class Styles.channel_info ]
                    [
                        h1 [] [
                            str channel.Name
                        ]
                        span [] [
                            str $"{Fakabase.count_users_in_channel channel} members" 
                        ]
                    ]
                div [ _id "messages"; _class Styles.messages ] (List.map message channel.Messages)
                form [ _class Styles.chat; _hxPost $"/chat/{channel_id}"; _hxTarget "#channel"; _hxSwap HxSwap.OuterHtml;(_hxOnHxEvent HxEvent.AfterRequest "if(event.detail.successfull) this.reset()") ] [
                    label [ _class Styles.label ] [
                        str "Name"
                        input [ _name "name"
                                _id "name"
                                _type "text"
                                _placeholder "Enter your name"
                                _required ]
                    ]
                    label [ _class Styles.label ] [
                        str "Message"
                        input [ _name "message"
                                _id "message"
                                _type "text"
                                _placeholder "Enter message"
                                _required ]
                    ]
                    button [ _type "submit"; _class Styles.button ] [ str "Send" ]
                ]
            | None ->
                h1 [] [
                    str "No channel found"
                ]
        ]

    let app (id: Guid) =
        page [
            div [ _class Styles.app_container ] [
                search_bar ()
                div [ _hxGet "/channel-list"; _hxTrigger HxTrigger.Load; _hxSwap HxSwap.OuterHtml ] []
                div [ _hxGet $"/channel/{id}"; _hxTrigger HxTrigger.Load; _hxSwap HxSwap.OuterHtml ] []
            ]
        ]

module Actions =
    let create_message id =
        fun next (ctx: HttpContext) -> 
            task {
                let message = ctx.GetFormValue("message") |> Option.defaultValue ""
                let user = ctx.GetFormValue("name") |> Option.defaultValue ""

                Fakabase.add_message message user id
                |> ignore
                
                return! (ctx.WriteHtmlViewAsync (Views.channel id))
            }

let routes =
    choose [ 
        POST >=> routef "/chat/%O" (fun id -> Actions.create_message id)
        //PUT >=> routef "/todo/%O" (fun guid -> Views.updateTodo guid)
        GET >=> choose [
            route "/" >=> warbler (fun _ -> htmlView (Views.app Fakabase.workspace.DefaultChannel.Id))

            route "/channel-list" >=> warbler (fun _ -> htmlView (Views.channel_list ()))
            routef "/channel/%O" (fun id -> htmlView (Views.channel id))
            routef "/messages/%O" (fun id -> htmlView (Views.messages id))

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
