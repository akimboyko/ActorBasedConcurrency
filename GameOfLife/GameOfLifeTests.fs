namespace GameOfLife.Tests

module Tests =
    open System
    open Xunit
    open FsUnit.Xunit
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp
    open Akka.TestKit
    open Akka.TestKit.Xunit
    open GameOfLife.Domain

    type GameOfLifeTests () as kit =
        inherit TestKit()

        let system = kit.Sys
        let waitFor = TimeSpan.FromMilliseconds(100.0)
        let timeout = Nullable(waitFor)

        [<Fact>]
        member self.``Let's 'Spawn' cell and receive back 'Neighborhood' messages`` () =
            let cellRef = spawn system "Cell" <| cellActorCont

            cellRef <! Spawn(0, 0)

            self.ExpectMsg(Neighborhood(0, 0, Occupied),  timeout) |> ignore
            
            self.ExpectMsg(Neighborhood(+1, +1, Unknown), timeout) |> ignore
            self.ExpectMsg(Neighborhood(+1, +0, Unknown), timeout) |> ignore
            self.ExpectMsg(Neighborhood(+1, -1, Unknown), timeout) |> ignore
            self.ExpectMsg(Neighborhood(+0, -1, Unknown), timeout) |> ignore
            self.ExpectMsg(Neighborhood(-1, -1, Unknown), timeout) |> ignore
            self.ExpectMsg(Neighborhood(-1, +0, Unknown), timeout) |> ignore
            self.ExpectMsg(Neighborhood(-1, +1, Unknown), timeout) |> ignore
            self.ExpectMsg(Neighborhood(+0, +1, Unknown), timeout) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating none 'Neighborhood' messages will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating single 'Neighborhood' message with 'Occupied' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood(0, 0, Occupied)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating single 'Neighborhood' message with 'Unknown' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating two 'Neighborhood' messages all with 'Unknown' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating two 'Neighborhood' messages one with 'Unknown' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood(0, 0, Occupied)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating three 'Neighborhood' messages all with 'Unknown' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating three 'Neighborhood' messages one with 'Occupied' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! Neighborhood(0, 0, Occupied)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating four 'Neighborhood' messages will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! Neighborhood(0, 0, Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()