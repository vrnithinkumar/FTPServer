//#r @"/Users/vr/WorkSpace/GitHub/FTPServer/FTPServerLib/bin/Debug/netstandard2.0/FTPServerLib.dll"
#I __SOURCE_DIRECTORY__
#r @"./FTPServerLib/bin/Debug/netstandard2.0/FTPServerLib.dll"
open FTPServerLib.Main
open FTPServerLib.ClientHelpers

let serverAsync = 
    async {
       StartServer() 
    }
let asyncClient =
    async {
       CreateClient() 
    }

Async.Start serverAsync
Async.Start asyncClient