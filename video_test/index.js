var app = require('express')();
var express = require('express');
var http = require('http').Server(app);
var io = require('socket.io')(http);

//console.log(app);
app.use(express.static(__dirname + '/public'));

app.get('/', function(req, res){
    res.sendFile(__dirname + '/index.html');
});

io.on('connection', function(socket){
    console.log('a user connected');
});

http.listen(3000, function(){
    console.log('listening on *:3000');
});

io.on('connection', function(socket){
    socket.on('curr energy', function(msg){
        io.emit('curr energy', msg);
    });
    socket.on('energy up', function(msg){
        io.emit('energy up', msg);
    });
    socket.on('energy down', function(msg){
        io.emit('energy down', msg);
    });
});