- Less overhead during connection as When an HTTP connection is established it's associated with handshakes and
    other overheads whereas websockets aren't.
- Websockets send minimal headers, whereas HTTP headers could be plentiful,
    which increases the data sent with the request.
- Websockets send frames and not bodies of information which again reduces the
    information sent with the request.

    - Example of websocket frame
[FIN][RSV1][RSV2][RSV3][Opcode][Mask][Payload length][Payload data]

    - Example of HTTP Requst 
GET /path/resource HTTP/1.1
Host: example.com
User-Agent: Mozilla/5.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Connection: keep-alive
Upgrade-Insecure-Requests: 1

    - Example of http response
HTTP/1.1 200 OK
Date: Mon, 23 Jul 2021 16:00:00 GMT
Server: Apache/2.4.1 (Unix)
Last-Modified: Wed, 22 Jul 2021 19:15:56 GMT
Content-Length: 44
Content-Type: text/html
Connection: Closed

<html><body>Hello, World!</body></html>
