namespace GameOfLife.Tests

module Tests =
    open System
    open System.Text
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
        let waitFor = TimeSpan.FromMilliseconds(250.0)
        let timeout = Nullable(waitFor)

        let verify = Swensen.Unquote.Assertions.test

        [<Fact>]
        member self.``Let's 'Spawn' cell and receive back 'Neighborhood' messages`` () =
            let cellRef = spawn system "Cell" <| cellActorCont

            cellRef <! Spawn(0, 0)

            self.ExpectMsgAllOf(waitFor,
                                Neighborhood((0, 0), Occupied), 
                                Neighborhood((+1, +1), Unknown),
                                Neighborhood((+1, +0), Unknown),
                                Neighborhood((+1, -1), Unknown),
                                Neighborhood((+0, -1), Unknown),
                                Neighborhood((-1, -1), Unknown),
                                Neighborhood((-1, +0), Unknown),
                                Neighborhood((-1, +1), Unknown),
                                Neighborhood((+0, +1), Unknown)) |> ignore

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

            aggregateRef <! Neighborhood((0, 0), Occupied)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating single 'Neighborhood' message with 'Unknown' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating two 'Neighborhood' messages all with 'Unknown' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating one 'Neighborhood' messages with 'Unknown' status and one 'Occupied' will remove cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Occupied)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating two 'Neighborhood' messages with 'Unknown' status and one 'Occupied' will keep cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Occupied)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating three 'Neighborhood' messages all with 'Unknown' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating three 'Neighborhood' messages one with 'Occupied' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Occupied)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating four 'Neighborhood' messages will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Coordinator spawns cells and they sends back 'Neighborhood' messages``() =
            let coordinatorRef = spawn system "Coordinator" <| coordinatorActorCont

            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(3, 3)
            coordinatorRef <! SpawnCompleted

            self.ExpectMsg(AggregationStarted (9 * 2) (*, timeout *)) |> ignore

            self.ExpectMsgAllOf(waitFor, 
                                Neighborhood((0 + 0, 0 + 0), Occupied),
                                Neighborhood((0 + 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 1, 0 + 0), Unknown), 
                                Neighborhood((0 + 1, 0 - 1), Unknown), 
                                Neighborhood((0 + 0, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 + 0), Unknown), 
                                Neighborhood((0 - 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 0, 0 + 1), Unknown), 

                                Neighborhood((3 + 0, 3 + 0), Occupied),
                                Neighborhood((3 + 1, 3 + 1), Unknown), 
                                Neighborhood((3 + 1, 3 + 0), Unknown), 
                                Neighborhood((3 + 1, 3 - 1), Unknown), 
                                Neighborhood((3 + 0, 3 - 1), Unknown), 
                                Neighborhood((3 - 1, 3 - 1), Unknown), 
                                Neighborhood((3 - 1, 3 + 0), Unknown), 
                                Neighborhood((3 - 1, 3 + 1), Unknown), 
                                Neighborhood((3 + 0, 3 + 1), Unknown)) |> ignore 

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Coordinator spawns only unique cells``() =
            let coordinatorRef = spawn system "Coordinator" <| coordinatorActorCont

            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! SpawnCompleted

            self.ExpectMsg(AggregationStarted 9 (*, timeout *)) |> ignore

            self.ExpectMsgAllOf(waitFor,
                                Neighborhood((0 + 0, 0 + 0), Occupied),
                                Neighborhood((0 + 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 1, 0 + 0), Unknown), 
                                Neighborhood((0 + 1, 0 - 1), Unknown), 
                                Neighborhood((0 + 0, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 + 0), Unknown), 
                                Neighborhood((0 - 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 0, 0 + 1), Unknown)) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Coordinator publish to event stream``() =
            let coordinatorRef = spawn system "Coordinator" <| coordinatorActorCont

            system.EventStream.Subscribe(self.TestActor, typedefof<Event>) |> ignore

            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! SpawnCompleted

            self.ExpectMsg(AggregationStarted 9 (*, timeout *)) |> ignore

            self.ExpectMsgAllOf(waitFor,
                                [|
                                    Generation 0                           :> obj
                                    LivingCell (0, 0)                      :> obj
                                    Neighborhood((0 + 0, 0 + 0), Occupied) :> obj
                                    Neighborhood((0 + 1, 0 + 1), Unknown)  :> obj 
                                    Neighborhood((0 + 1, 0 + 0), Unknown)  :> obj 
                                    Neighborhood((0 + 1, 0 - 1), Unknown)  :> obj 
                                    Neighborhood((0 + 0, 0 - 1), Unknown)  :> obj 
                                    Neighborhood((0 - 1, 0 - 1), Unknown)  :> obj 
                                    Neighborhood((0 - 1, 0 + 0), Unknown)  :> obj 
                                    Neighborhood((0 - 1, 0 + 1), Unknown)  :> obj 
                                    Neighborhood((0 + 0, 0 + 1), Unknown)  :> obj                                    
                                |]) |> ignore
            
            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates no 'Neighborhood' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 0

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates single 'Neighborhood' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 1
            collectorRef <! Neighborhood((0, 0), Occupied)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates two times same 'Neighborhood' one with 'Occupied' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 2
            collectorRef <! Neighborhood((0, 0), Occupied)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates two times same 'Neighborhood' both 'Unknow' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 2
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates three times same 'Neighborhood' one with 'Occupied' and spawns back new cell``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 3
            collectorRef <! Neighborhood((0, 0), Occupied)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates three times same 'Neighborhood' both 'Unknow' and spawns back new cell``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 3
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates four times same 'Neighborhood' one with 'Occupied' and keep cell``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 4
            collectorRef <! Neighborhood((0, 0), Occupied)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates four times same 'Neighborhood' both 'Unknow' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 4
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates four times 'Unknow' and one 'Occupied' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 5
            collectorRef <! Neighborhood((0, 0), Occupied)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Expected empty generation`` () =
            let output = new StringBuilder()
            let generationDisplayRef = spawn system "GenerationDisplay" <| generationDisplayActorCont(output)

            let eventStream = system.EventStream
            eventStream.Subscribe(generationDisplayRef, typedefof<Event>) |> ignore

            eventStream.Publish(Generation(0))

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

            let results = output.ToString()

            verify <@ results = "Generation 0" + Environment.NewLine @>

        [<Fact>]
        member self.``Expected generation of one cell`` () =
            let output = new StringBuilder()
            let generationDisplayRef = spawn system "GenerationDisplay" <| generationDisplayActorCont(output)

            let eventStream = system.EventStream
            eventStream.Subscribe(generationDisplayRef, typedefof<Event>) |> ignore

            eventStream.Publish(Generation(0))
            eventStream.Publish(LivingCell(1, 1))
            eventStream.Publish(Generation(1))

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

            let results = output.ToString()

            verify <@ results = 
                        ("Generation 0" + Environment.NewLine +
                         "Generation 1" + Environment.NewLine + 
                         "#" + Environment.NewLine)
                    @>

        [<Fact>]
        member self.``Expected generation of three cells`` () =
            let output = new StringBuilder()
            let generationDisplayRef = spawn system "GenerationDisplay" <| generationDisplayActorCont(output)

            let eventStream = system.EventStream
            eventStream.Subscribe(generationDisplayRef, typedefof<Event>) |> ignore

            eventStream.Publish(Generation(0))
            eventStream.Publish(LivingCell(0, 1))
            eventStream.Publish(LivingCell(1, 1))
            eventStream.Publish(LivingCell(2, 1))
            eventStream.Publish(Generation(1))

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

            let results = output.ToString()

            verify <@ results = 
                        ("Generation 0" + Environment.NewLine +
                         "Generation 1" + Environment.NewLine + 
                         "###" + Environment.NewLine)
                    @>