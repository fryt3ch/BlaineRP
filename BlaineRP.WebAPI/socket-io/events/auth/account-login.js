const bcrypt = require("bcrypt");

exports = module.exports = (socket, dbPool) =>
{
  socket.on("Auth::Account::Login", async (data, ack) =>
  {
    var conn = null;

    try
    {
        conn = await dbPool.getConnection();

        var [result, fields] = await conn.query("SELECT * FROM accounts WHERE SCID=? LIMIT 1;", [data.scid, data.login]);
    
        if (result.length == 0)
            throw "NoAccountFound";
        
        var row = result[0];

        if (row.Login != data.login)
            throw "WrongLogin";

        var passVerifyRes = await bcrypt.compare(data.password, row.Password);

        if (passVerifyRes)
        {
            var row = result[0];

            // if no 2fa
  
            var aData =
            {
              id : row.ID,
              hwid : row.HWID,
              mail : row.Mail,
              regDate : row.RegDate.getTime() / 1000,
              regIp : row.RegIP,
              lastIp : row.LastIP,
              adminLvl : row.AdminLevel,
              bcoins : row.BCoins,
            };

            ack({ status: 1, aData: aData, });
        }
        else
        {
            throw "WrongPassword";
        }
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