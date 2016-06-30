var HOST = '192.168.1.11';
var PORT = 12345;

var SRVPORT = 12346;
var SRVHOST = '192.168.1.8';

var dgram = require('dgram');

var five = require("johnny-five");
var board = new five.Board();

var dgram = require('dgram');
var server = dgram.createSocket('udp4');
var client = dgram.createSocket('udp4');

board.on("ready", function() {
  //var sensor = new five.Sensor("A0");

var sensor = new five.Sensor({
  pin: "A0", 
  freq: 250, 
  threshold: 0.5
});
  
  // Scale the sensor's data from 0-1023 to 0-10 and log changes
  sensor.scale(0, 100).on("change", function() {
    	console.log(this.value);
  	var message = this.value.toString();
  	client.send(message, 0, message.length, PORT, HOST, function(err, bytes) {
     	//if (err) throw err;
    	//console.log('UDP message sent to ' + HOST +':'+ PORT);
      	//client.close();
		});
	});
	
server.on('listening', function () {
    var address = server.address();
    console.log('UDP Server listening on ' + address.address + ":" + address.port);
});

var pin = new five.Pin(1);
var pin5 = new five.Pin(5);
var pin9 = new five.Pin(9);

server.on('message', function (message, remote) {
	console.log("-" + message + "-");
	if (message == "hello")
	{
		HOST = remote.address;
		PORT = remote.port;
	}
	else if (message == 'fan-on')
	{
		pin.high();
	} else if (message == 'fan-off')
	{
		pin.low();
	} 
	else if (message == 'light-on')
	{
		pin5.high();
	} else if (message == 'light-off')
	{
		pin5.low();
	}
	else if (message == 'sound-on')
	{
		pin9.high();
	} else if (message == 'sound-off')
	{
		pin9.low();
	}	
});

server.bind(SRVPORT, SRVHOST);


});

