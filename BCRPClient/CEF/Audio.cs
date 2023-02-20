using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    public class Audio : Events.Script
    {
        public static RAGE.Ui.HtmlWindow Window { get; private set; }

        private static string[] DefaultStreamFormats = new string[] { "mp3" };

        public static List<Data> AllAudios { get; private set; } = new List<Data>();

        public class Data
        {
            public string Id { get; set; }

            public TrackTypes CurrentTrackType { get; set; }

            public string ExtraData { get; set; }

            public GameEntity Entity { get; set; }

            public Vector3 Position { get; set; }

            public float Range { get; set; }

            public bool IsPlaying { get; set; }

            public float BaseVolume { get; set; }

            public float Duration { get; set; }

            public DateTime StartTime { get; set; }

            public bool Exists => AllAudios.Contains(this);

            public Data(string Id, GameEntity Entity, float Range, float BaseVolume = 1f) : this(Id, BaseVolume)
            {
                this.Entity = Entity;
                this.Range = Range;
            }

            public Data(string Id, Vector3 Position, float Range, float BaseVolume = 1f) : this(Id, BaseVolume)
            {
                this.Position = Position;
                this.Range = Range;
            }

            public Data(string Id, float BaseVolume = 1f)
            {
                GetById(Id)?.Destroy();

                this.Id = Id;
                this.BaseVolume = BaseVolume;

                StartTime = DateTime.MinValue;

                AllAudios.Add(this);
            }

            public void Play(TrackTypes trackType, float volume = 1f, bool loop = false, int pos = 0)
            {
                var url = Urls.GetValueOrDefault(trackType);

                if (url == null)
                    return;

                CurrentTrackType = trackType;

                IsPlaying = true;

                if (IsTrackStream(CurrentTrackType))
                {
                    Duration = float.PositiveInfinity;

                    Window.ExecuteJs("playAudioStream", Id, url, volume, DefaultStreamFormats);
                }
                else
                {
                    Window.ExecuteJs("playAudio", Id, url, volume, loop, pos);
                }
            }

            public void Stop()
            {
                if (!IsPlaying)
                    return;

                IsPlaying = false;

                Window.ExecuteJs("stopAudio", Id);
            }

            public void Destroy()
            {
                Stop();

                AllAudios.Remove(this);
            }

            public void SetPaused(bool state)
            {
                IsPlaying = state;

                Window.ExecuteJs("setAudioPaused", Id, IsPlaying);
            }

            public void SetVolume(float volume)
            {
                Window.ExecuteJs("setAudioVolume", Id, volume);
            }

            public void SetPos(float seconds)
            {
                Window.ExecuteJs("setAudioPos", Id, seconds);
            }

            public static Data GetById(string id) => AllAudios.Where(x => x.Id == id).FirstOrDefault();

            public static List<Data> GetAllByEntity(GameEntity gEntity) => AllAudios.Where(x => x.Entity == gEntity).ToList();

            public void RequestTrackUpdate(TrackTypes trackType)
            {
                CurrentTrackType = trackType;

                IsPlaying = false;
            }

            public float GetActualPosSeconds()
            {
                if (StartTime == DateTime.MinValue)
                    return float.MinValue;

                return (float)Utils.GetServerTime().Subtract(StartTime).TotalSeconds;
            }
        }

        public enum TrackTypes
        {
            None = 0,

            Auth1, Auth2, Auth3,

            RadioRetroFM, RadioEuropaPlus, RadioClassicFM, RadioRecord, RadioSynthwave,
        }

        private static Dictionary<TrackTypes, string> Urls { get; set; } = new Dictionary<TrackTypes, string>()
        {
            { TrackTypes.Auth1, "http://fishbotserver.ddns.net:1234/audio/auth_1.mp3" },
            { TrackTypes.Auth2, "http://fishbotserver.ddns.net:1234/audio/auth_2.mp3" },
            { TrackTypes.Auth3, "http://fishbotserver.ddns.net:1234/audio/auth_3.mp3" },

            { TrackTypes.RadioRetroFM, "https://retro.hostingradio.ru:8014/retro320.mp3" }, // https://retro.hostingradio.ru:8043/retro256.mp3
            { TrackTypes.RadioEuropaPlus, "https://ep256.hostingradio.ru:8052/europaplus256.mp3" },
            { TrackTypes.RadioClassicFM, "https://media-ssl.musicradio.com/ClassicFM" },
            { TrackTypes.RadioRecord, "https://radiorecord.hostingradio.ru/rr_main96.aacp" },
            { TrackTypes.RadioSynthwave, "https://radiorecord.hostingradio.ru/synth96.aacp" },
        };

        public static bool IsTrackStream(TrackTypes trackType) => trackType >= TrackTypes.RadioRetroFM;

        public Audio()
        {
            Window = new RAGE.Ui.HtmlWindow("package://audio//index.html");

            Events.Add("Audio::Finished", async (args) =>
            {
                var audioData = Data.GetById((string)args[0]);

                if (audioData == null)
                    return;

                if (audioData.Id == "AUTH_AUDIO_PL")
                {
                    await RAGE.Game.Invoker.WaitAsync(1500);

                    if (!audioData.Exists)
                        return;

                    var authAudios = Enum.GetValues(typeof(TrackTypes)).Cast<TrackTypes>().Where(x => x.ToString().StartsWith("Auth")).ToList();

                    var nextTrackIdx = authAudios.IndexOf(audioData.CurrentTrackType) + 1;

                    if (nextTrackIdx < 0 || nextTrackIdx >= authAudios.Count)
                        nextTrackIdx = 0;

                    audioData.Play(authAudios[nextTrackIdx], audioData.BaseVolume, false, 0);
                }
            });

            Events.Add("Audio::Error", (args) =>
            {
                var audioId = (string)args[0];
                var errorMsg = (string)args[1];
            });

            Events.Add("Audio::Loaded", (args) =>
            {
                var audioData = Data.GetById((string)args[0]);

                if (audioData == null)
                    return;

                var duration = (float)Convert.ToDecimal(args[1]);

                audioData.Duration = duration < 0 ? float.PositiveInfinity : duration;

                var actualPos = audioData.GetActualPosSeconds();

                if (actualPos <= 0f)
                    return;

                audioData.SetPos(actualPos);
            });

            (new AsyncTask(() =>
            {
                var userVolume = Settings.Audio.SoundVolume / 100f;

                for (int i = 0; i < AllAudios.Count; i++)
                {
                    var audio = AllAudios[i];

                    if (audio.CurrentTrackType == TrackTypes.None)
                        continue;

                    if (audio.Entity != null)
                    {
                        var dist = 0f;

                        if (audio.Entity is Vehicle veh)
                        {
                            if (!veh.GetIsEngineRunning())
                            {
                                dist = float.MaxValue;
                            }
                            else if (Player.LocalPlayer.Vehicle == veh)
                            {
                                audio.BaseVolume = 1f;

                                dist = 0f;
                            }
                            else
                            {
                                audio.BaseVolume = 0.25f;

                                dist = Player.LocalPlayer.Position.DistanceTo(RAGE.Game.Entity.GetEntityCoords(veh.Handle, false));
                            }
                        }
                        else
                        {
                            dist = Player.LocalPlayer.Position.DistanceTo(RAGE.Game.Entity.GetEntityCoords(audio.Entity.Handle, false));
                        }

                        UpdateAudioState(audio, dist, userVolume);
                    }
                    else if (audio.Position != null)
                    {
                        var dist = Player.LocalPlayer.Position.DistanceTo(audio.Position);

                        UpdateAudioState(audio, dist, userVolume);
                    }
                }
            }, 250, true, 1000)).Run();


            Events.AddDataHandler("AUD", (entity, value, oldValue) =>
            {
                if (entity is GameEntity gEntity)
                {
                    if (!gEntity.Exists)
                        return;

                    var audioId = GetEntityAudioId(gEntity, "D");

                    var currentAudio = AllAudios.Where(x => x.Id == audioId).FirstOrDefault();

                    if (value == null)
                    {
                        currentAudio?.Destroy();
                    }
                    else
                    {
                        TrackTypes trackType; float range, baseVolume; long startTimestamp; string trackData;

                        ParseAudioData(((string)value).Split('&'), out trackType, out range, out baseVolume, out startTimestamp, out trackData);

                        if (currentAudio != null)
                        {
                            if (currentAudio.CurrentTrackType != trackType || currentAudio.ExtraData != trackData)
                            {
                                currentAudio.ExtraData = trackData;

                                currentAudio.RequestTrackUpdate(trackType);
                            }

                            currentAudio.Range = range;

                            currentAudio.BaseVolume = baseVolume;
                        }
                        else
                        {
                            var audio = new Data(audioId, gEntity, range, baseVolume)
                            {
                                CurrentTrackType = trackType,

                                StartTime = DateTimeOffset.FromUnixTimeSeconds(startTimestamp).DateTime,

                                ExtraData = trackData,
                            };
                        }
                    }
                }
            });
        }

        public static void StopAll()
        {
            AllAudios.Clear();

            Window.ExecuteJs("stopAllAudio();");
        }

        public static void UpdateAudioState(Data audio, float dist, float userVolume)
        {
            if (dist > audio.Range + 15f)
            {
                audio.Stop();
            }
            else
            {
                var newVolume = (audio.BaseVolume - (dist / audio.Range) * audio.BaseVolume) * userVolume;

                if (!audio.IsPlaying)
                {
                    audio.Play(audio.CurrentTrackType, newVolume, false, 0);
                }
                else
                {
                    audio.SetVolume(newVolume);
                }
            }
        }

        public static void StartAuthPlaylist()
        {
            var authAudios = Enum.GetValues(typeof(TrackTypes)).Cast<TrackTypes>().Where(x => x.ToString().StartsWith("Auth")).ToList();

            var audioData = new Data("AUTH_AUDIO_PL", 0.5f);

            audioData.Play(authAudios[Utils.Random.Next(0, authAudios.Count)], audioData.BaseVolume, false, 0);
        }

        public static void StopAuthPlaylist()
        {
            var audioData = Data.GetById("AUTH_AUDIO_PL");

            audioData?.Destroy();
        }

        public static void OnEntityStreamIn(GameEntity gEntity)
        {
            var audioData = gEntity.GetSharedData<string>("AUD")?.Split('&');

            if (audioData == null)
                return;

            TrackTypes trackType; float range, baseVolume; long startTimestamp; string trackData;

            ParseAudioData(audioData, out trackType, out range, out baseVolume, out startTimestamp, out trackData);

            var audio = new Data(GetEntityAudioId(gEntity, "D"), gEntity, range, baseVolume)
            {
                CurrentTrackType = trackType,

                StartTime = DateTimeOffset.FromUnixTimeSeconds(startTimestamp).DateTime,

                ExtraData = trackData,
            };
        }

        public static void OnEntityStreamOut(GameEntity gEntity)
        {
            var allAudio = CEF.Audio.Data.GetAllByEntity(gEntity);

            foreach (var x in allAudio)
            {
                x.Destroy();
            }
        }

        public static void ParseAudioData(string[] audioData, out TrackTypes trackType, out float range, out float baseVolume, out long startTimestamp, out string trackData)
        {
            trackType = (TrackTypes)int.Parse(audioData[0]);

            range = float.Parse(audioData[1]);
            baseVolume = float.Parse(audioData[2]);

            startTimestamp = long.Parse(audioData[3]);

            trackData = audioData.Length > 4 ? audioData[4] : null;
        }

        public static string GetEntityAudioId(GameEntity gEntity, string tag) => $"ENT_{tag}_{gEntity.Handle}";
    }
}
