const express = require('express');
const app = express();
const port = 3000;

let counter = 0;

app.get('/sse', (req, res) => {
	res.setHeader('Content-Type', 'text/event-stream');
	res.setHeader('Cache-Control', 'no-cache');
	res.setHeader('Connection', 'keep-alive');

	const intervalId = setInterval(() => {
		counter++;
		res.write(`data: ${counter}\n\n`);
	}, 1000)

	req.on('close', () => {
		clearInterval(intervalId);
	});
})

app.get('/', (req, res) =>{
	res.sendFile(__dirname + '/sse.html');
} )

app.listen(port, () => {
	console.log(`server is running on http://localhost:${port}`);
});
