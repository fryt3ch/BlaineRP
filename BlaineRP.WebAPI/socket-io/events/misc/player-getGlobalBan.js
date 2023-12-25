exports = module.exports = (socket, dbPool) =>
{
  socket.on("Misc::Player::GetGlobalBan", async (data, ack) =>
  {
    var conn = null;

    try
    {
      conn = await dbPool.getConnection();

      var [result, fields] = await conn.query("SELECT * FROM global_blacklist WHERE SCID=? OR HWID=? LIMIT 1", [data.scid, data.hwid]);
  
      if (result.length == 0)
      {
        ack({ status: 1, });
      }
      else
      {
        var row = result[0];
    
        ack({ status: 1, data: row });
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