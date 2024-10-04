// A client for the capitalization server. After connecting, every line
// sent to the server will come back capitalized.
//
// Use interactively:
//
//   node capitalizeclient.js 10.0.1.40
//
// Or pipe in a file to be capitalized:
//
//   node capitalizeclient.js 10.0.1.40 < myfile

const readline = require('node:readline');
const net = require('net');
// import { net } from 'net'
// import { readline } from 'readline'

const client = new net.Socket()
// Connects to server
client.connect(59898, process.argv[2] ?? "localhost", () => {
    console.log("Connected to server")
})
client.on("data", (data) => {
    console.log(data.toString("utf-8"))
})

const reader = readline.createInterface({ input: process.stdin })
reader.on("line", (line) => {
    // Sends to server over socket - on other side, server should
    // listen and then send calculated bill back
    client.write(`${line}\n`)
})
reader.on("close", () => {
    client.end()
})