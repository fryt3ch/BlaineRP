const bcrypt = require("bcrypt");

exports = module.exports = (socket, dbPool) =>
{
  socket.on("Auth::Account::Reg", async (data, ack) =>
  {
    var conn = null;

    try
    {
        conn = await dbPool.getConnection();

        var [result, fields] = await conn.query("SELECT * FROM accounts WHERE SCID=? OR Login=? OR Mail=? LIMIT 1;", [data.scid, data.login, data.mail]);
    
        if (result.length != 0)
            throw result[0].SCID == data.scid ? "SCIDExists" : result[0].Login == data.login ? "LoginExists" : result[0].Mail == data.mail ? "MailExists" : "";

        var passEnc = await bcrypt.hash(data.password, await bcrypt.genSalt(5));

        var curDate = new Date();

        [result, fields] = await conn.query("INSERT INTO accounts (SCID, HWID, Login, Password, Mail, RegDate, RegIP, LastIP, AdminLevel) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);", parseInt(data.scid), data.hwid, data.login, passEnc, data.mail, curDate, data.ip, data.ip, -1);

        var aData =
        {
          id : result.insertId,
          scid : data.scid,
          hwid : data.hwid,
          mail : data.mail,
          login : data.login,
          ip : data.ip,
          confirmHash : "",
          cancelledConfirmHashes : [],
        };

        ack({ status: 1, confirmHash: "", });

        socket.emit("Auth::Account::Confirmed", { aData: aData, });
    }
    catch (ex)
    {
        ack({ status: 0, error: ex, });
    }
    finally
    {
        if (conn != null)
            conn.release();
    }
  });
}