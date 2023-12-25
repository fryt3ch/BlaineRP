exports = module.exports = (socket, dbPool) =>
{
  socket.on("Auth::Account::GetId", async (data, ack) =>
  {
    var conn = null;

    try
    {
      conn = await dbPool.getConnection();

      var [result, fields] = await conn.query("SELECT ID FROM accounts WHERE SCID=? LIMIT 1;", [data.scid]);
  
      if (result.length == 0)
      {
        ack({ status: 1, });
      }
      else
      {
        var row = result[0];
    
        ack({ status: 1, aid: row.ID });
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