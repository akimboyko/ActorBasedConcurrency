namespace GameOfLife.Tests

module Tests =
    open Xunit
    open FsUnit.Xunit
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp
    open Akka.TestKit
    open Akka.TestKit.Xunit
    open GameOfLife.Main

    type GameOfLifeTests () as kit =
        inherit TestKit()

        let system = kit.Sys
        let timeoutInMs = 1000

        [<Fact>]
        member self.``Let's play single iteration of ping-pong`` () =
            let cellRef = spawn system "TestKit" <| cellActorCont

            let future = cellRef <? "ping"

            let reply = Async.RunSynchronously(future, timeoutInMs)

            reply |> should equal "pong"
            
        [<Fact>]
        member self.``Let's play 100 iterations of ping-pong`` () =
            let cellRef = spawn system "TestKit" <| cellActorCont

            for _ in [1..100] do
                let future = cellRef <? "ping"

                let reply = Async.RunSynchronously(future, timeoutInMs)

                reply |> should equal "pong"