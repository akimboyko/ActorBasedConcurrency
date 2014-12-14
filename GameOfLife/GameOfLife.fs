namespace GameOfLife

module Domain =
    open System
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp

    type Command =
    | Spawn of int * int

    type CellStatus =
    | Occupied
    | Unknown

    type Message =
    | Neighborhood of int * int * CellStatus
    | AggregationCompleted

    let poisonPill = PoisonPill.Instance

    let cellActorCont = 
        let cont (mailbox : Actor<Command>) =
            let rec loop() =
                actor { 
                    let! command = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let self = mailbox.Self

                    match command with
                    | Spawn(x, y) -> 
                        sender <! Neighborhood(x, y, Occupied)
                            
                        sender <! Neighborhood(x + 1, y + 1, Unknown)
                        sender <! Neighborhood(x + 1, y + 0, Unknown)
                        sender <! Neighborhood(x + 1, y - 1, Unknown)
                        sender <! Neighborhood(x + 0, y - 1, Unknown)
                        sender <! Neighborhood(x - 1, y - 1, Unknown)
                        sender <! Neighborhood(x - 1, y + 0, Unknown)
                        sender <! Neighborhood(x - 1, y + 1, Unknown)
                        sender <! Neighborhood(x + 0, y + 1, Unknown)
                            
                        self <! poisonPill
                }

            loop()

        cont

    let aggregateActorCont = 
        let cont (mailbox : Actor<Message>) =
            let aggregate currentStatus status =
                if status = Occupied then Occupied
                else if currentStatus = Occupied then Occupied
                else Unknown

            let rec loop n position currentStatus =
                actor { 
                    let! message = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let self = mailbox.Self

                    match message with
                    | Neighborhood(x, y, status) -> 
                        let n = n + 1
                        let status = aggregate currentStatus status 
                        let position = Some(x, y)
                             
                        return! loop n position status
                    | AggregationCompleted ->
                        match position with
                        | Some(x, y) ->
                            if n = 2 && currentStatus = Occupied then
                                sender <! Spawn (x, y)
                            else if n = 3 then
                                sender <! Spawn (x, y)
                        | None -> ()

                        self <! poisonPill
                }

            loop 0 None Unknown

        cont