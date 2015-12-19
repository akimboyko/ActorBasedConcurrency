// This sample intended to run on Mono at Docker

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

[<EntryPoint>]
let main argv = 
    let system = ActorSystem.Create("FSharp")

    let echoServer = 
        spawn system "EchoServer"
        <| fun mailbox ->
                actor {
                    let! message = mailbox.Receive()
                    match box message with
                    | :? string as msg -> printfn "Hello %s" msg
                    | _ ->  failwith "unknown message"
                } 

    echoServer <! "F# and Mono running as Docker container!"

    Threading.Thread.Sleep 1000

    system.Shutdown()

    0
