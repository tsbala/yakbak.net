var http = require('http');
var yakbak = require('yakbak');

console.log(__dirname);
http.createServer(yakbak('http://google.co.uk', {
	dirname: __dirname + '/tapes'
})).listen(3000);