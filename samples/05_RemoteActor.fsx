﻿#time "on"
#load "Bootstrap.fsx"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

// #Remote Actor
// Actor is not only a concurrency model, it can also be used for distributed computing.
// This example builds an EchoServer using an Actor.
// Then it creates a client to access the Akka URL.
// The usage is the same as with a normal Actor.

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
            log-config-on-start : on
            stdout-loglevel : DEBUG
            loglevel : ERROR
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }

            remote {
                helios.tcp {
                    port = 8777
                    hostname = localhost
                }
            }
        }")

let system = ActorSystem.Create("RemoteFSharp", configuration)

let echoServer = 
    spawn system "EchoServer"
    <| fun mailbox ->
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? string -> 
                        printfn "super!"
                        sender <! sprintf "Hello %s remote" message
                | _ ->  failwith "unknown message"
            } 

let echoClient = system.ActorSelection(
                            "akka.tcp://RemoteFSharp@localhost:8777/user/EchoServer")

let task = echoClient <? "F#!"

let response = Async.RunSynchronously (task, 1000)

printfn "Reply from remote %s" (string(response))

system.Shutdown()