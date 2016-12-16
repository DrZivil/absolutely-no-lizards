var app = require('express')();
var express = require('express');
var http = require('http').Server(app);
var io = require('socket.io')(http);

//console.log(app);
app.use(express.static(__dirname + '/public'));

app.get('/', function(req, res){
    res.sendFile(__dirname + '/index.html');
});

app.get('/gamemaster', function(req, res) {
    res.sendFile(__dirname + '/gamemaster.html');
});
app.get('/leaderboard', function(req, res) {
    res.sendFile(__dirname + '/leaderboard.html');
});

// io.on('connection', function(socket) {
//     console.log('a user connected');
// });

http.listen(3000, function(){
    console.log('listening on *:3000');
});

io.on('connection', function(socket){
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
    socket.on('update', function (msg) {
        console.log("update Values");
        io.emit('update', msg);
    });
    socket.on('reset game', function () {
        console.log("reset game");
        io.emit('reset game');
    });
    socket.on('start game', function () {
        console.log("start game");
        io.emit('start game');
    });
    socket.on('game over', function () {
        io.emit('game over');
    });
});