module Tests

open System
open Xunit
open FTPServerLib.ServerHelpers
[<Fact>]
let ``My test`` () =
    StartServer()
    Assert.True(true)
