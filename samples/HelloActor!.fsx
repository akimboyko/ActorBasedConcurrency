// https://github.com/akkadotnet/akka.net/wiki/FSharp-API
#time "on"
#load "Bootstrap.fsx"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

type SomeActorMessages =
    | Greet of string
    | Hi

type SomeActor() =    
    inherit Actor()

    override x.OnReceive message =
        match message with
        | :? SomeActorMessages as m ->  
            match m with
            | Greet(name) -> Console.WriteLine("Hello {0}",name)
            | Hi -> Console.WriteLine("Hello from F#!")
        | _ -> failwith "unknown message"

let system =  
    ConfigurationFactory.Default() 
    |> System.create "FSharpActors"

let actor = 
    spawn system "MyActor"
    <| fun mailbox ->
        let rec again name =
            actor {
                let! message = mailbox.Receive()
                match message with
                | Greet(n) when n = name ->
                    printfn "Hello again, %s" name
                    return! again name
                | Greet(n) -> 
                    printfn "Hello %s" n
                    return! again n
                | Hi -> 
                    printfn "Hello from F#!"
                    return! again name }
        and loop() =
            actor {
                let! message = mailbox.Receive()
                match message with
                | Greet(name) -> 
                    printfn "Hello %s" name
                    return! again name
                | Hi ->
                    printfn "Hello from F#!"
                    return! loop() } 
        loop()

actor <! Greet "Aaron"
actor <! Hi
actor <! Greet "Roger"
actor <! Hi
actor <! Greet "Håkan"
actor <! Hi
actor <! Greet "Jeremie"