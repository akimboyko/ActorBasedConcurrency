namespace GameOfLife

module Main =
    open System
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp

    let cellActorCont = 
        let cont (mailbox : Actor<obj>) =
            let rec loop() =
                actor { 
                    let! message = mailbox.Receive()
                    let sender = mailbox.Sender()

                    match message with
                    | :? string as msg when msg = "ping" -> 
                            sender <! "pong"
                            return! loop()
                    | _ ->  failwith "unknown message"
                }

            loop()

        cont