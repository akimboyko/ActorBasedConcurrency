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
                | :? string as msg -> printfn "Hello %s" msg
                | _ ->  failwith "unknown message"
            } 

echoServer <! "F#!"

Threading.Thread.Sleep 5000

system.Shutdown()