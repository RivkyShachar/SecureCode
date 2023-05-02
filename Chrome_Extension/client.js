

/*import net from "net"

var net = require('net');


var HOST = '20.100.2.62';
var PORT = '5555';

var socket = new net.Socket();

socket.connect(PORT, HOST, function () {
	console.log('CONNECTED TO: ' + HOST + ':' + PORT);
	// Write a message to the socket as soon as the client is connected, the server will   receive it as message from the client 
	socket.write('@!>\n\r');
	socket.write('RIG,test,test,3.1\m\r');

});*/



// Add a 'data' event handler for the client socket
// data is what the server sent to this socket
socket.on('data', function (data) {
	console.log('data recieved: ' + data);
	// Close the client socket completely
	//    client.destroy();
});

socket.on('error', function (exception) {
	console.log('Exception:');
	console.log(exception);
});


socket.on('drain', function () {
	console.log("drain!");
});

socket.on('timeout', function () {
	console.log("timeout!");
});

// Add a 'close' event handler for the client socket
socket.on('close', function () {
	console.log('Connection closed');
});