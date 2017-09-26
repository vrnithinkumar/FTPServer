namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

module ServerHelpers =
    let StartServer() =
        let port = 11000
        let ipHostInfo = Dns.Resolve(Dns.GetHostName())
        let localIPAddress = ipHostInfo.AddressList.[0]
        let localEndPoint = new IPEndPoint(localIPAddress, port)
        printfn "ip Host : %A" ipHostInfo
        printfn "ip IP Address: %A" localIPAddress
        printfn "ip End Point: %A" localEndPoint

open ServerHelpers
