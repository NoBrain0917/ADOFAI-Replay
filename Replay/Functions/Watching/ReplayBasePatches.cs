﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using Discord;
using HarmonyLib;
using Newgrounds;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using Replay.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Replay.Functions.Menu
{
    [HarmonyPatch]
    public class ReplayBasePatches
    {
        private static bool _forceMove;
        private static bool _forcePlay;
        private static bool _forceColor;
        private static bool _replayPlanetHit;
        private static bool _beforeNoStopModEnabled;
        private static bool _dontDie;
        private static bool _paused;
        private static int _index;
        private static UnityModManager.ModEntry _noStopModModEntry;
        

        private static List<Tween> _requestedHold = new List<Tween>();
        
        internal static bool _progressDisplayerCancel;
        internal static ReplayInfo _playingReplayInfo;

        // Disable NoStopMod if installed
        // Not Used

        /*
        private static void DisableNoStopMod()
        {
            if(!Replay.IsUsingNoStopMod) return;
            
            _noStopModModEntry ??= UnityModManager.FindMod("NoStopMod");
            
            if (_noStopModModEntry == null) return;
            _beforeNoStopModEnabled = _noStopModModEntry.Active;
            if (_beforeNoStopModEnabled)
                _noStopModModEntry.OnToggle(_noStopModModEntry, false);
            
            TestGUI.var1 = _index;
        }*/


        // reset all data
        public static void Reset()
        {
            if (_forceMove)
            {
                _forceMove = false;
                return;
            }

            foreach (var h in _requestedHold)
                h.Kill();
            _requestedHold.Clear();

            _playingReplayInfo = null;
            _index = 0;
            _dontDie = false;
            Cursor.visible = true;

            ReplayFreeCameraPatches._freeCameraMode = true;
            WatchReplay.IsPlaying = false;

            GCS.customLevelPaths = null;
            GCS.standaloneLevelMode = false;
            GCS.checkpointNum = 0;
            ReplayUI.Instance.InGameUI.SetActive(false);

            if (scrController.instance != null)
            {
                scrController.instance.errorMeter.UpdateLayout(Persistence.GetHitErrorMeterSize(),
                    Persistence.GetHitErrorMeterShape());
            }

            KeyboradHook.OnEndInputs();

            /*
            if (!Replay.IsUsingNoStopMod) return;
            if (_noStopModModEntry == null) return;
            if (_beforeNoStopModEnabled)
                _noStopModModEntry.OnToggle(_noStopModModEntry, true);
            */
        }

        // Detect if index range is exceeded
        private static bool CheckInvalidIndex()
        {
            return _playingReplayInfo.Tiles.Length - 1 < _index || _index < 0;
        }

        // Planet angle when pressed by player
        private static double GetAngle()
        {
            if (CheckInvalidIndex())
                return Math.PI;

            var planet = scrController.instance.chosenplanet;
            var tileInfo = _playingReplayInfo.Tiles[_index];
            return tileInfo.HitAngleRatio + planet.targetExitAngle;
        }

        // Hit the planet in replay
        private static void ReplayHit()
        {
            if (CheckInvalidIndex())
                return;
            _replayPlanetHit = true;

            var r = _playingReplayInfo.Tiles[_index];
            var angle = GetAngle();

            scrController.instance.consecMultipressCounter = 0;
            if (_playingReplayInfo.Tiles[_index].SeqID == scrController.instance.currentSeqID &&
                !scrController.instance.currFloor.midSpin)
            {
                scrController.instance.chosenplanet.angle = angle;
                scrController.instance.chosenplanet.cachedAngle = angle;
            }

            scrController.instance.noFailInfiniteMargin = r.NoFailHit;
            scrController.instance.Hit();
            scrController.instance.noFailInfiniteMargin = false;

            var k = _playingReplayInfo.Tiles[_index].Key;
            KeyboradHook.OnKeyPressed(k);
            var tween = DOVirtual.DelayedCall(
                _playingReplayInfo.Tiles[_index].HeldTime / (GCS.currentSpeedTrial / _playingReplayInfo.Speed),
                () => { KeyboradHook.OnKeyReleased(k); });
            tween.OnComplete(() => { _requestedHold.Remove(tween); });
            tween.OnKill(() => { KeyboradHook.OnKeyReleased(k); });
            _index++;
        }


        // replay play
        public static void Start(ReplayInfo replayInfo)
        {
            WatchReplay.IsLoading = true;
            WatchReplay.IsPlaying = true;
            WatchReplay.IsPlanetDied = false;
            _forcePlay = true;
            _playingReplayInfo = replayInfo;
            _index = 0;
            Cursor.visible = true;

            GCS.currentSpeedTrial = replayInfo.Speed;
            GCS.nextSpeedRun = replayInfo.Speed;
            WatchReplay.PatchedPitch = GCS.currentSpeedTrial;

            //DisableNoStopMod();
        }

        // Restart the replay from that point
        public static void ReplayStartAt(int seqID)
        {
            var floors = scrLevelMaker.instance.listFloors;
            if (seqID > floors[_playingReplayInfo.EndTile].seqID) seqID = floors[_playingReplayInfo.EndTile].seqID;
            if (seqID < floors[_playingReplayInfo.StartTile].seqID) seqID = floors[_playingReplayInfo.StartTile].seqID;

            _forceMove = true;
            _index = _playingReplayInfo.Tiles.ToList().FindIndex(x => x.SeqID == seqID);

            foreach (var h in _requestedHold)
                h.Kill();
            _requestedHold.Clear();

            WatchReplay.RestartLevelAt(seqID);
        }

        // when is the hit timing
        private static bool IsHitNow()
        {
            if (scrController.instance == null) return false;
            if (scrController.instance.currentState != States.PlayerControl)
                return false;
            if (WatchReplay.IsPaused) return false;

            if (CheckInvalidIndex())
                return false;
            

            var planet = scrController.instance.chosenplanet;

            if (scrController.instance.currentSeqID > _playingReplayInfo.Tiles[_index].SeqID) return true;

            var angle = GetAngle();
            var angleOverd = (scrController.instance.isCW && planet.angle >= angle) ||
                             (!scrController.instance.isCW && planet.angle <= angle);
            var validTile = scrController.instance.currentSeqID == _playingReplayInfo.Tiles[_index].SeqID;

            if (scrController.instance.currFloor.freeroam)
            {
                var nextTileInfoVailed = ReplayUtils.GetSafeList(_playingReplayInfo.Tiles, _index + 1).SeqID ==
                                         scrController.instance.currFloor.seqID;
                if (angleOverd && validTile && nextTileInfoVailed)
                    return true;
            }
            

            return scrController.isGameWorld && angleOverd && validTile;
        }


        [HarmonyPatch(typeof(PauseMenu), "RefreshLayout")]
        [HarmonyPostfix]
        public static void DisableOtherButtonsPatch(ref GeneralPauseButton[] ___pauseButtons)
        {
            if (!WatchReplay.IsPlaying) return;

            ___pauseButtons = ___pauseButtons.Where(b =>
            {
                if (b is PauseButton button)
                {
                    return !(button.rdString == "pauseMenu.restart" || button.rdString == "pauseMenu.practice" ||
                             button.rdString == "pauseMenu.settings" || button.rdString == "pauseMenu.next" ||
                             button.rdString == "pauseMenu.previous");
                }

                return false;
            }).ToArray();

        }


        [HarmonyPatch(typeof(scrController), "Awake")]
        [HarmonyPostfix]
        public static void SetStartAtPatch()
        {


            if (!WatchReplay.IsPlaying) return;
            if (!_playingReplayInfo.IsOfficialLevel) return;
            GCS.checkpointNum = WatchReplay.OfficialStartAt;
            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;
        }


        [HarmonyPatch(typeof(CustomLevel), "Play")]
        [HarmonyPrefix]
        public static void ForceCustomSeqIDPlayPatch(ref int seqID)
        {
            if (!WatchReplay.IsPlaying) return;
            if (!_forcePlay) return;

            _forcePlay = false;
            seqID = _playingReplayInfo.StartTile;
            GCS.checkpointNum = _playingReplayInfo.StartTile;
            scrController.instance.currentSeqID = _playingReplayInfo.StartTile;
        }


        [HarmonyPatch(typeof(scrController), "Start")]
        [HarmonyPrefix]
        public static void ForceOfficialSeqIDPlayPatch()
        {
            _paused = scrController.instance.paused;
            
            if (!WatchReplay.IsPlaying) return;
            if (!scrController.isGameWorld) return;
            if (!_playingReplayInfo.IsOfficialLevel) return;
            if (!_forcePlay) return;
            _forcePlay = false;

            GCS.checkpointNum = _playingReplayInfo.StartTile;
            scrController.instance.currentSeqID = _playingReplayInfo.StartTile;
        }


        [HarmonyPatch(typeof(scrController), "QuitToMainMenu")]
        [HarmonyPrefix]
        public static bool QuitToMainMenuInReplayingPatch(ref bool ___exitingToMainMenu)
        {
            if (!WatchReplay.IsPlaying) return true;
            Reset();
            
            scrController.instance.paused = true;
            scrController.instance.enabled = false;
            scrController.instance.paused = true;
            
            WatchReplay.DisableAllEffects();

            ___exitingToMainMenu = true;
            RDUtils.SetGarbageCollectionEnabled(true);
            ADOBase.audioManager.StopLoadingMP3File();

            GCS.standaloneLevelMode = false;
            GCS.worldEntrance = null;
            GCS.checkpointNum = 0;
            GCS.currentSpeedTrial = 1f;

            scrController.deaths = 0;
            scrConductor.instance.hasSongStarted = false;
            scrConductor.instance.KillAllSounds();
            scrConductor.instance.song.Stop();


            Time.timeScale = 1;
            ReplayUIUtils.DoSwipe(() => { SceneManager.LoadScene("scnReplayIntro"); });
            _progressDisplayerCancel = true;
            scrUIController.instance.WipeToBlack(WipeDirection.StartsFromLeft);

            return false;
        }


        [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
        [HarmonyPrefix]
        public static bool CancelProgressDisplayerPatch()
        {
            if (_progressDisplayerCancel)
            {
                _progressDisplayerCancel = false;
                return false;
            }

            return true;
        }


        [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]
        [HarmonyPrefix]
        public static bool DisableSwipePatch()
        {
            if (!WatchReplay.IsPlaying) return true;
            if (!scrController.isGameWorld) return true;
            if (scrLevelMaker.instance.listFloors.Count - 1 == scrController.instance.currentSeqID)
                return false;
            return true;
        }


        [HarmonyPatch(typeof(scrController), "TogglePauseGame")]
        [HarmonyPrefix]
        public static bool TogglePauseGamePatch(ref bool __result, int ___frameStart, ref int ___lastTogglePauseFrame)
        {
            if (!WatchReplay.IsPlaying) return true;
            var controller = scrController.instance;
            AudioManager.pauseGameSounds = false;
            if (GCS.standaloneLevelMode && (Time.frameCount - ___frameStart < 4 ||
                                            (ADOBase.customLevel != null && ADOBase.customLevel.isLoading)))
            {
                __result = _paused;
                return false;
            }

            if (!ADOBase.isEditingLevel && scrUIController.instance.transitionPanel.gameObject.activeSelf)
            {
                __result = _paused;
                return false;
            }
            
            if (GCS.d_boothDisablePossibleMessUpButtons || GCS.webVersion)
                controller.QuitToMainMenu();

            _paused = !_paused;

            controller.paused = !WatchReplay.IsPlaying && _paused;
            ___lastTogglePauseFrame = 0;
            controller.audioPaused = _paused;
            controller.enabled = WatchReplay.IsPlaying || !_paused;
            Time.timeScale = (_paused ? 0f : 1f);
            
            if (scnEditor.instance == null || GCS.standaloneLevelMode)
            {
                if (_paused)
                {
                    controller.takeScreenshot.ShowPauseMenu(false);
                }
                else
                {
                    typeof(scrController).GetMethod("CheckForAudioOutputChange", AccessTools.all)
                        .Invoke(controller, new object[]{});
                    controller.pauseMenu.Hide();
                }
            }

            __result = _paused;
            return false;
        }

        [HarmonyPatch(typeof(scrPlanet), "Update_RefreshAngles")]
        [HarmonyPrefix]
        public static bool PauseAngle()
        {
            if(!WatchReplay.IsPlaying) return true;
            if (WatchReplay.IsPlanetDied)
            {
                scrController.instance.audioPaused = true;
                return false;
            }
            return true;
        }
        
        

        [HarmonyPatch(typeof(scrPressToStart), "ShowText")]
        [HarmonyPostfix]
        public static void ReplayShowTextPatch(Text ___text)
        {
            if (!WatchReplay.IsPlaying) return;
            var hash = _playingReplayInfo.IsOfficialLevel
                ? 0
                : (CustomLevel.instance.levelData.isOldLevel
                    ? CustomLevel.instance.levelData.pathData.GetHashCode()
                    : string.Join("", CustomLevel.instance.levelData.angleData).GetHashCode());


            if (hash != _playingReplayInfo.PathDataHash)
            {
                Reset();
            
                WatchReplay.DisableAllEffects();
                
                RDUtils.SetGarbageCollectionEnabled(true);
                ADOBase.audioManager.StopLoadingMP3File();

                GCS.standaloneLevelMode = false;
                GCS.worldEntrance = null;
                GCS.checkpointNum = 0;
                GCS.currentSpeedTrial = 1f;

                if (scrController.instance.pauseMenu.gameObject.activeSelf)
                    scrController.instance.TogglePauseGame();

                scrController.deaths = 0;
                scrController.instance.enabled = false;
                scrController.instance.paused = true;
                scrConductor.instance.hasSongStarted = false;
                scrConductor.instance.KillAllSounds();
                scrConductor.instance.song.Stop();
                
                _progressDisplayerCancel = true;
                scrUIController.instance.WipeToBlack(WipeDirection.StartsFromLeft);
                
                SceneManager.LoadScene("scnReplayIntro");

                GlobalLanguage.OK = Replay.CurrentLang.okText;
                GlobalLanguage.No = Replay.CurrentLang.noText;
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayModText, Replay.CurrentLang.levelDiff,
                    () => { }, null, RDString.language);
                ReplayViewingTool.UpdateLayout();
                return;
            }

            if (CheckInvalidIndex())
            {
                Replay.Log("invalid replay index "+_index);
                ReplayStartAt(_playingReplayInfo.StartTile);
                return;
            }

            
            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;
            
            Cursor.visible = true;

            FixReplayBugsPatches.SetTileGlow();

            var planet = scrController.instance.chosenplanet;
            WatchReplay.SetPlanetColor(planet, _playingReplayInfo);
            WatchReplay.SetPlanetColor(planet.other, _playingReplayInfo);

            scrUIController.instance.txtCountdown.GetComponent<scrCountdown>().CancelGo();
            ReplayViewingTool.Init(_playingReplayInfo);

            scrConductor.instance.song.volume = 1;
            scrUIController.instance.difficultyContainer.gameObject.SetActive(false);
            GCS.difficulty = _playingReplayInfo.Difficulty;

            ___text.text = Replay.CurrentLang.pressToPlay;
            WatchReplay.IsLoading = false;
            WatchReplay.IsPlanetDied = false;

            if (WatchReplay.IsPaused)
            {
                Time.timeScale = 0;
                scrController.instance.audioPaused = true;
            }
            else
            {
                Time.timeScale = 1;
                scrController.instance.audioPaused = false;
            }
        }
        
        
        [HarmonyPatch(typeof(scrController), "Start_Rewind")]
        [HarmonyPostfix]
        public static void PlanetColorChangePatch()
        {
            if (!WatchReplay.IsPlaying) return;

            var planet = scrController.instance.chosenplanet;
            WatchReplay.SetPlanetColor(planet, _playingReplayInfo);
            WatchReplay.SetPlanetColor(planet.other, _playingReplayInfo);

            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;
        }


        [HarmonyPatch(typeof(scrController), "FailAction")]
        [HarmonyPrefix]
        public static bool NoFailPatch()
        {
            if (!WatchReplay.IsPlaying) return true;
            if (_playingReplayInfo.EndTile - 1 > scrController.instance.currentSeqID) return false;

            WatchReplay.IsPlanetDied = true;
            scrController.instance.audioPaused = true;
            Time.timeScale = 0;
            scrController.instance.ChangeState(States.None);
            return false;
        }


        [HarmonyPatch(typeof(ActivityManager), "UpdateActivity")]
        [HarmonyPrefix]
        public static void SetDiscordActivityPatch(ref Activity activity)
        {
            if (!WatchReplay.IsPlaying) return;
            if (activity.Assets.LargeImage == "planets_icon_stars")
            {
                activity.Details = Replay.CurrentLang.replayModText;
                if (_playingReplayInfo.IsOfficialLevel)
                    activity.State = Replay.CurrentLang.replayingText + ": " + _playingReplayInfo.SongName;
                else
                    activity.State = Replay.CurrentLang.replayingText + ": " + _playingReplayInfo.ArtistName + " - " +
                                     _playingReplayInfo.SongName;
            }
        }


        [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
        [HarmonyPrefix]
        public static void WipeToBlackInReplayingPatch()
        {
            if (!WatchReplay.IsPlaying) return;
            Reset();
        }
        
        [HarmonyPatch(typeof(scrUIController), "WipeFromBlack")]
        [HarmonyPrefix]
        public static void WipeFromBlackkInReplayingPatch()
        {
            if (!WatchReplay.IsPlaying) return;
            scrUIController.wipeDirection = WipeDirection.StartsFromRight;
        }


        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        [HarmonyPrefix]
        public static void ResetSceneInReplayingPatch()
        {
            if (!WatchReplay.IsPlaying) return;
            Reset();
        }
        


        [HarmonyPatch(typeof(scrController), "Hit")]
        [HarmonyPrefix]
        public static bool IgnoreKeystrokesPatch()
        {
            if (!WatchReplay.IsPlaying ||
                scrController.instance.currentState != States.PlayerControl) return true;

            scrController.instance.keyTimes.Clear();
            if (scrController.instance.currFloor.midSpin) return true;
            if (!_replayPlanetHit)
                return false;
            _replayPlanetHit = false;
            return true;
        }
        

        [HarmonyPatch(typeof(scrPlanet), "SwitchChosen")]
        [HarmonyPrefix]
        public static void SetForceAnglePatch(scrPlanet __instance)
        {
            if (!WatchReplay.IsPlaying) return;
            if (scrController.instance.midspinInfiniteMargin || scrController.instance.currFloor.midSpin) return;
            var angle = GetAngle();

            __instance.angle = angle;
            __instance.cachedAngle = angle;
        }
        
        


        [HarmonyPatch(typeof(scrConductor), "Update")]
        [HarmonyPrefix]
        public static void ReplayPlanetUpdatePatch()
        {
            if (!WatchReplay.IsPlaying) return;
            if (_playingReplayInfo == null) return;

            if (scrLevelMaker.instance.listFloors.Count > 0)
                ReplayViewingTool.UpdateTime();

            if (CheckInvalidIndex()) return;

            var num0 = 10;
            while (IsHitNow() && !WatchReplay.IsLoading && WatchReplay.IsPlaying && num0 > 0)
            {
                if (WatchReplay.IsLoading || _forceMove || _forcePlay || WatchReplay.IsPaused || !WatchReplay.IsPlaying || scrController.instance == null)
                    break;

                if (CheckInvalidIndex())
                    break;

                ReplayHit();
                num0--;

            }
        }
    }
}