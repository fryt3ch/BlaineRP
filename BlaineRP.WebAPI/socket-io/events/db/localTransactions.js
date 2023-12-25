exports = module.exports = (socket, dbPool) =>
{
  socket.on("Db::LocalTransactions", async (data, ack) =>
  {
    var conn = null;

    try
    {
      conn = await dbPool.getConnection();

      await conn.beginTransaction();

      for (var i = 0; i < data.list.length; i++)
      {
        await conn.query(data.list[i].cmd, data.list[i].args);
      }

      await conn.commit();

      ack({ status: 1, });
    }
    catch (ex)
    {
        if (conn != null)
            await conn.rollback();

      ack({ status: 0, error: ex, });
    }
    finally
    {
      if (conn != null)
        conn.release();
    }
  });
}