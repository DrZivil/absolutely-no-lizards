var app = require('express')();
var express = require('express');
var http = require('http').Server(app);
var io = require('socket.io')(http);

//console.log(app);
app.use(express.static(__dirname + '/public'));

app.get('/', function(req, res){
    res.sendFile(__dirname + '/index.html');
});
app.get('/test', function(req, res){
    res.sendFile(__dirname + '/test.html');
});

// io.on('connection', function(socket){
//     console.log('a user connected');
// });

http.listen(3000, function(){
    console.log('listening on *:3000');
});

io.on('connection', function(socket){
    socket.on('count', function(msg){
        console.log("counting", msg);
        io.emit('count', msg);
    });

    socket.on('client connected', function(msg){
        console.log("Client has connected", msg);
        io.emit('unity connected', msg);
    });

    socket.on('curr energy', function(msg){
        io.emit('curr energy', msg);
    });
	socket.on('curr energy pic', function(msg){
	    //console.log('test: ', msg);
        io.emit('test', msg);
    });
    socket.on('energy up', function(msg){
        io.emit('energy up', msg);
    });
    socket.on('energy down', function(msg){
        io.emit('energy down', msg);
    });
});