namespace ReplayLoader.Languages
{
    public class Simplified_Chinese : LocalizedText
    {
        public Simplified_Chinese()
        {
            pressToPlay = "请按任意键以开始回放";
            replayModText = "回放";
            replayingText = "正在回放中";
            cantFindPath = "路径不存在。";
            cantLoad = "读取回放文件失败。";
            cantSave = "保存失败";
            saveOptionTitle = "存储选项";
            saveEverytimeDied = "在关卡失败时自动保存回放文件（不建议使用）";
            saveEveryLevelComplete = "在通过关卡时自动保存回放文件";
            saveBySpecifiedKey = "在按下特定按键时保存回放文件";
            registerSpecifiedKeyText = "按下一个按键并将其注册为用于保存回放文件的特殊按键";
            progressTitle = "进程";
            levelLengthTitle = "关卡长度";
            playButtonTitle = "播放";
            saveSuccess = "已保存";
            replayListTitle = "回放列表";
            toMainTitle = "返回";
            keyviewerShowOption = "设置要在回放中显示的按键显示器（KeyViewer）";
            loading = "加载中……";
            replayMod = "回放";
            programming = "程序编写";
            uiDesign = "UI 设计";
            okText = "好的";
            noText = "算了吧";
            replayCollectMessage =
                "开发其它模组可能需要用到回放文件，<b>但这<u>不是必须的，可以被拒绝。</u></b> 回放文件需要以下信息：\n\n - 按下按键时行星与轨道的角度；轨道编号；当前按下的按键；按下按键的时机。\n - 在保存回放文件时开始与结束的轨道编号。\n - 关卡的曲师和歌曲名称、<b><u>保存关卡的路径（即使路径包含名字，也会按原样收集）</b></u>";
            
            
            levelDiff = "已保存的关卡数据和当前关卡数据不同";
            deathcamOption = "关卡失败时回放选项";
            changePath = "更改保存路径";
            currentSavePath = "当前保存路径：";
            replaySceneRPCTitle = "选择回放文件";
            agreed = "同意";
            noAgreed = "拒绝";
            saveWhen90P = "当在关卡进度大于等于 90% 失败时自动保存回放文件";
            replayCount20delete = "当回放文件数量超过 20 个时自动删除";

            disableOttoSave = "在开启自动播放时不自动保存回放文件";
            hideEffectInDeathcam = "隐藏特效";
            
            
            loadReplay = "加载回放文件";
            enterCodeTitle = "输入回放码";
            enterCodeHintText = "请输入：";
            notSupportOfficialLevel = "官方关卡并不受支持";
            reallyShareThisReplay = "你想共享这个回放文件吗？";
            cantShareBecauseLimitOver = "暂时无法共享——已共享的回放文件已超过上限（10 个），\n请先删除一些已共享的回放文件后重试。";
            reallyDeleteSharedReplay = "是否要删除这份回放文件？";
            reallyDeleteSharedReplayMoreMessage = "甚至连它的回放码也会被一并删除，你真的想要删除它吗？";
            downloadingText = "正在下载中……";
            uploadingText = "正在上传中……";
            successShareReplay = "上传完毕。\n回放码：";
            copySharedReplayCode = "复制回放码";
            failShareReplay = "上传失败。\n原因：";
            failDownloadReplay = "下载失败。\n原因：";
            sharedReplayCount = "已共享的回放文件数：";
            invalidReplayCode = "无效回放码";
            failDownloadReplayShort = "关卡下载失败";
            removeOnlyMyReplay = "你只能删除属于你自己的回放文件";
            preparing = "正在准备中……";
            autoUpdate = "自动更新";
            nextTimeUpdate = "下次再进行更新";
            newReplayVersion = "回放模组的新版本！";
            restartSoon = "一会儿后将重新启动……";
            japaneseTranslate = "日语翻译";
            replayOption = "回放选项";

            showInputTiming = "显示输入时机";
            saveRealComplete = "从头到尾完成关卡后自动保存回放文件（0% ~ 100%）";
            
            copyText = "复制错误信息";
            
            UnsetTextSetting();

        }
        
        
    }
}
