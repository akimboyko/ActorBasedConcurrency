namespace GameOfLife.Properties

module GameOfLifeProperties =
    open System
    open Xunit
    open FsUnit.Xunit
    open FsCheck
    open FsCheck.Fluent
    open Akka.FSharp
    open Akka.TestKit.Xunit
    open GameOfLife.Domain

    type GameOfLifeProperties () as kit =
        inherit TestKit()

        let system = kit.Sys
        let waitFor = TimeSpan.FromMilliseconds(100.0)
        let timeout = Nullable(waitFor)

        let shouldReceiveSpawn statuses =
            let nMessages = statuses |> Array.length
            let overAllStatus = 
                statuses |>
                Array.fold(fun state overall -> 
                            if state = Occupied then Occupied
                            else if overall = Occupied then Occupied
                            else Unknown) Unknown

            nMessages = 3 || (nMessages = 2 && overAllStatus = Occupied)

        [<Fact>]
        member self.``'Spawn' cell and receive back all 'Neighborhood' messages`` () =
            Spec.ForAny<int * int>(fun (x, y) ->
                let cellRef = spawn system "Cell" <| cellActorCont

                cellRef <! Spawn(x, y)

                self.ExpectMsg(Neighborhood(x, y, Occupied),  timeout) |> ignore
            
                self.ExpectMsg(Neighborhood(x + 1, y + 1, Unknown), timeout) |> ignore
                self.ExpectMsg(Neighborhood(x + 1, y + 0, Unknown), timeout) |> ignore
                self.ExpectMsg(Neighborhood(x + 1, y - 1, Unknown), timeout) |> ignore
                self.ExpectMsg(Neighborhood(x + 0, y - 1, Unknown), timeout) |> ignore
                self.ExpectMsg(Neighborhood(x - 1, y - 1, Unknown), timeout) |> ignore
                self.ExpectMsg(Neighborhood(x - 1, y + 0, Unknown), timeout) |> ignore
                self.ExpectMsg(Neighborhood(x - 1, y + 1, Unknown), timeout) |> ignore
                self.ExpectMsg(Neighborhood(x + 0, y + 1, Unknown), timeout) |> ignore

                self.ExpectNoMsg(waitFor)
                
                true).QuickCheckThrowOnFailure()

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating 'Neighborhood' messages and receive result`` () =
            Spec.ForAny<int * int * CellStatus[]>(fun (x, y, statuses) ->
                let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

                for status in statuses do
                    aggregateRef <! Neighborhood(x, y, status)

                aggregateRef <! AggregationCompleted

                if shouldReceiveSpawn statuses then
                    self.ExpectMsg(Spawn(x, y), timeout) |> ignore
                    
                self.ExpectNoMsg(waitFor)

                true).QuickCheckThrowOnFailure()

            system.Shutdown()