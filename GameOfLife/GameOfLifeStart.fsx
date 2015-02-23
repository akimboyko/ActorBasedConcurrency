#time "on"
#load "..\samples\Bootstrap.fsx"
#load "GameOfLife.fs"

open System
open System.Text
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open GameOfLife.Domain

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
            log-config-on-start : on
            stdout-loglevel : DEBUG
            loglevel : DEBUG
            loggers = [""Akka.Event.StandardOutLogger""]
            debug {
                autoreceive = on
                lifecycle = on
                event-stream = on
            }
        }")

let system = ActorSystem.Create("GameOfLife", configuration)
let eventStream = system.EventStream

let output = new StringBuilder()

let coordinatorRef  = spawn system "Coordinator" <| coordinatorActorCont
let collectorRef    = spawn system "Collector"   <| collectorActorCont
let displayRef      = spawn system "Display"     <| generationDisplayActorCont(output)

eventStream.Subscribe(displayRef, typedefof<Event>) |> ignore

coordinatorRef.Tell(Spawn(1, 0),    collectorRef)
coordinatorRef.Tell(Spawn(1, 1),    collectorRef)
coordinatorRef.Tell(Spawn(1, 2),    collectorRef)
coordinatorRef.Tell(SpawnCompleted, collectorRef)

Threading.Thread.Sleep(2000)

system.Shutdown()

printf "%s" (output.ToString())