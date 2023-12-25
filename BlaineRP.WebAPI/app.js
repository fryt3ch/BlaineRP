var mysql = require('mysql2/promise'); 

const http = require('http');
const socket = require('socket.io');

const eventAccountGetId = require('./socket-io/events/auth/account-getId');
const eventAccountLogin = require('./socket-io/events/auth/account-login');
const eventAccountReg = require('./socket-io/events/auth/account-reg');
const eventMiscPlayerGetGlobalBan = require('./socket-io/events/misc/player-getGlobalBan');
const eventDbLocalTransactions = require('./socket-io/events/db/localTransactions');

const host = "localhost";
const port = 7777;

const mainDbPool = mysql.createPool({
  connectionLimit: 10,
  host: "localhost",
  user: "root",
  database: "blainerp_main",
  password: "root",
});

const server1DbPool = mysql.createPool({
  connectionLimit: 5,
  host: "localhost",
  user: "root",
  database: "blainerp_server_1",
  password: "root",
});

const server = http.createServer();

const io = socket(server,
{
  transports: ["websocket"],
});

const User = class
{
  constructor(id, password, dbPool)
  {
    this.id = id;
    this.password = password;
    this.dbPool = dbPool;
  }

  isConnected() { return this.socket != null; }

  isAuthed() { return isConnected() || this.authSocket != null; }

  setAsConnected(socket, time)
  {
    if (socket != null)
    {
      this.socket = socket;

      this.lastConnectedTime = time;
    }
    else
    {
      this.socket = null;

      this.lastDisconnectedTime = time;
    }

    if (this.authSocket != null)
      this.authSocket = null;
  }

  setAsAuthed(socket, time)
  {
    if (socket != null)
    {
      this.authSocket = socket;
    }
    else
    {
      this.authSocket = null;
    }
  }
};

const serverUsers =
[
  new User("brp-server-1", "63c209c3-3505-443a-b234-91e3046e2894", server1DbPool),
];

function getUserBy(predicate) { return serverUsers.find(predicate) }

function getUserById(id) { return getUserBy(x => x.id == id) }

function getUserBySocketId(id) { return getUserBy(x => x.socket.id == id); }

io.use((socket, next) =>
{
  var user = getUserById(socket.handshake.auth.user);

  if (user == null || user.password != socket.handshake.auth.password || user.isConnected())
  {
    next(new Error("Authentication error"));

    return;
  }

  user.setAsAuthed(socket, new Date());

  console.log(`User \"${user.id}\" just authed!`);

  next();
});

io.on("connection", (socket) =>
{
  var user = getUserBy(x => x.authSocket.id == socket.id);

  if (user == null)
  {
    socket.disconnect();

    return;
  }

  user.setAsConnected(socket, new Date());

  console.log(`User \"${user.id}\" just connected!`);

  socket.once("disconnect", () =>
  {
    var user = getUserBySocketId(socket.id);

    if (user == null)
    {
      socket.disconnect();

      return;
    }

    user.setAsConnected(null, new Date());

    console.log(`User \"${user.id}\" just disconnected!`);
  });

  eventAccountGetId(socket, mainDbPool);
  eventAccountReg(socket, mainDbPool);
  eventAccountLogin(socket, mainDbPool);
  eventMiscPlayerGetGlobalBan(socket, mainDbPool);

  eventDbLocalTransactions(socket, user.dbPool);
});

server.listen(port, host, () =>
{
  console.log(`Server is running on http://${host}:${port}`);
});