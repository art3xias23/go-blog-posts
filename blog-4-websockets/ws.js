const express = require('express');
const WebSocket = require('ws');
const http = require('http');

const app = express();
const server = http.createServer(app);
const wss = new WebSocket.Server({ server });

wss.on('connection', (ws) => {

	console.log("Client connected");

	ws.on("message", (message) => {
		console.log(`Received: ${message}`);

		ws.send(`Echo: ${message}`);
	});
});

wss.on("close", () =>{
	console.log("Client disconnected");
});

app.get("/", (req, res) =>{
	res.sendFile(__dirname+"/ws.html")
});

const port = 3000;
server.listen(port, () => {
	console.log(`sever started on localhost:${port}`);
});
