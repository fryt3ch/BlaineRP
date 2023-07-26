using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Fractions
{
    public abstract partial class Fraction
    {
        [Script(int.MaxValue)]
        public class Events
        {
            public Events()
            {
                RAGE.Events.Add("Player::SCF", (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (args == null || args.Length < 1)
                    {
                        var lastFraction = pData.CurrentFraction;

                        if (lastFraction != null)
                        {
                            lastFraction.OnEndMembership();

                            pData.CurrentFraction = null;
                        }
                    }
                    else
                    {
                        var fraction = (FractionTypes)(int)args[0];

                        var fData = Fraction.Get(fraction);

                        var lastFraction = pData.CurrentFraction;

                        if (lastFraction != null)
                        {
                            lastFraction.OnEndMembership();
                        }

                        pData.CurrentFraction = fData;

                        fData.OnStartMembership(args.Skip(1).ToArray());
                    }
                });

                RAGE.Events.Add("Fraction::UMS", (args) =>
                {
                    if (Fraction.AllMembers == null)
                        return;

                    var cid = Utils.Convert.ToUInt32(args[0]);

                    var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                    if (mData == null)
                        return;

                    mData.SubStatus = (byte)(int)args[1];

                    FractionMenu.UpdateMember(cid, "status", mData.SubStatus);
                });

                RAGE.Events.Add("Fraction::UMO", (args) =>
                {
                    if (Fraction.AllMembers == null)
                        return;

                    var cid = Utils.Convert.ToUInt32(args[0]);

                    var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                    if (mData == null)
                        return;

                    mData.IsOnline = (bool)args[1];

                    if (mData.IsOnline)
                        mData.LastSeenDate = Core.ServerTime;

                    FractionMenu.UpdateMember(cid, "circle", mData.Rank + 1);

                    if (args.Length > 2)
                    {
                        mData.SubStatus = (byte)(int)args[2];

                        FractionMenu.UpdateMember(cid, "status", mData.SubStatus);
                    }
                });

                RAGE.Events.Add("Fraction::UMR", (args) =>
                {
                    if (Fraction.AllMembers == null)
                        return;

                    var cid = Utils.Convert.ToUInt32(args[0]);

                    var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                    if (mData == null)
                        return;

                    mData.Rank = (byte)(int)args[1];

                    FractionMenu.UpdateMember(cid, "pos", mData.Rank + 1);
                });

                RAGE.Events.Add("Fraction::UM", (args) =>
                {
                    if (Fraction.AllMembers == null)
                        return;

                    var cid = Utils.Convert.ToUInt32(args[0]);

                    if (args.Length == 1)
                    {
                        var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                        if (Fraction.AllMembers.Remove(cid))
                        {
                            FractionMenu.RemoveMember(cid);
                        }
                    }
                    else
                    {
                        var mData = new MemberData() { IsOnline = true, Name = (string)args[1], Rank = (byte)(int)args[2], SubStatus = (byte)(int)args[3], LastSeenDate = DateTimeOffset.FromUnixTimeSeconds(Utils.Convert.ToInt64(args[4])).DateTime };

                        if (Fraction.AllMembers.TryAdd(cid, mData))
                        {
                            FractionMenu.AddMember(cid, mData);
                        }
                    }
                });

                RAGE.Events.Add("Fraction::UVEHMR", (args) =>
                {
                    if (Fraction.AllVehicles == null)
                        return;

                    var vid = Utils.Convert.ToUInt32(args[0]);
                    var newMinRank = (byte)(int)args[1];

                    var vData = Fraction.AllVehicles.GetValueOrDefault(vid);

                    if (vData == null)
                        return;

                    vData.MinRank = newMinRank;

                    FractionMenu.UpdateVehicleInfo(vid, "access", newMinRank + 1);
                });

                RAGE.Events.Add("Fraction::NEWSC", (args) =>
                {
                    if (Fraction.NewsData == null)
                        return;

                    var idx = (int)args[0];

                    if (args.Length > 1)
                    {
                        var text = (string)args[1];

                        if (Fraction.NewsData.All.TryAdd(idx, text))
                        {
                            FractionMenu.AddNews(idx, text);
                        }
                        else
                        {
                            Fraction.NewsData.All[idx] = text;

                            FractionMenu.UpdateNews(idx, text);
                        }
                    }
                    else
                    {
                        if (Fraction.NewsData.All.Remove(idx))
                        {
                            if (Fraction.NewsData.PinnedId == idx)
                                Fraction.NewsData.PinnedId = -1;

                            FractionMenu.DeleteNews(idx);
                        }
                    }
                });

                RAGE.Events.Add("Fraction::NEWSP", (args) =>
                {
                    if (Fraction.NewsData == null)
                        return;

                    var idx = (int)args[0];

                    if (idx < 0)
                    {
                        if (Fraction.NewsData.PinnedId < 0)
                            return;

                        Fraction.NewsData.PinnedId = -1;

                        FractionMenu.PinNews(-1);
                    }
                    else
                    {
                        if (!Fraction.NewsData.All.ContainsKey(idx))
                            return;


                        Fraction.NewsData.PinnedId = idx;

                        FractionMenu.PinNews(idx);
                    }
                });
            }
        }
    }
}