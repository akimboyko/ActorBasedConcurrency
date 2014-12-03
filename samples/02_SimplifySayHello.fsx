#time "on"
#load "Bootstrap.fsx"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

// #Simplify Actor
// There is a simpler way to define an Actor

let system = ActorSystem.Create("FSharp")

let echoServer = 
    spawn system "EchoServer"
    <| fun mailbox ->
            actor {
                let! message = mailbox.Receive()
                match box message with
                | :? string -> printfn "Hello %s" message
                | _ ->  failwith "unknown message"
            } 

echoServer <! "F#!"

system.Shutdown()