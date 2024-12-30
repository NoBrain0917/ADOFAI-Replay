[[한국어 문서]](https://github.com/NoBrain0917/ADOFAI-Replay)  
[[English Document]](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/english_doc.md)  
[[v1.0.0 vs v0.0.1]](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/compare.md)

# ADOFAI 回放模组

![replay](https://github.com/NoBrain0917/Replay/blob/master/Resource/adofai.gif?raw=true)

这是一个 [ADOFAI（冰与火之舞）](https://store.steampowered.com/app/977950/A_Dance_of_Fire_and_Ice/) 的游戏模组。
加入 ADOFAI.gg 社区以获取更多信息！https://discord.gg/TKdpbUUfUa

--- 

# [下载最新版本](https://github.com/NoBrain0917/Replay/releases)
回放模组使你可以保存你的游玩过程并随时随地地再次观看它。~~这类似于《守望先锋》的回放。~~
    
## 我该如何观看我的游玩回放？
默认情况下，你可以在你结束游玩关卡（通过关卡或是在关卡中失败）时按下 F9 或 F11 来观看你这次的游玩回放。
   
![save](https://github.com/NoBrain0917/Replay/blob/master/Resource/save.png?raw=true)

### 回放模组的大致操作方法
 - 在按下 F11（可以更改）时可以保存回放文件
 - 通过在标题界面进入回放列表，你可以预览已保存的回放文件。（或是按下 Ctrl + Shift + R）

### 关卡失败时回放
 - 按下 F9（可以更改）来查看关卡失败前 20 秒的回放
 - 你可以查看你失败的原因以及位置而**不用保存本次回放。**
 - 只在关卡中失败时生效

### 通常情况
 - 你可以通过按下 B 键来自由地移动摄像机（官方关卡不受支持）
     
![option](https://github.com/NoBrain0917/Replay/blob/master/Resource/option.png?raw=true)

## 保存回放时失败     
 - 问题在于找不到保存关卡文件的路径，请设置关卡保存路径
 - 或是你在保存回放文件时还停留在关卡开始的前 3 个轨道

## 在保存回放文件时出现了卡顿    
当回放文件转换为 JSON 格式并发送到服务器时，会出现延迟。
由于代码的特性，你在回放中按下的按键次数越多，卡顿就越大。
请打开 `<冰与火之舞路径>/Mods/Replay/ReplayOption.xml` 并将 `CanICollectReplayFile` 更改为 `2` 后保存。

![change](https://github.com/NoBrain0917/Replay/blob/master/Resource/change.png?raw=true)

---

## 支持的按键显示器
- AdofaiTweaks（由 PizzaLovers007 制作，支持 v2.5.4 或更新的版本）
- RainingKeys（由 파링 制作，支持 v0.3.0 或更新的版本）
- OttoKeyViewer（由 ChocoSwi 制作，支持 v1.2.1 或更新的版本）
- KeyViewer（由 C## 制作，支持 v3.4.0 或更新的版本）


## 支持的语言
- 한국어（韩语）
- English（英语）
- 日本語（日语）
- 简体中文

如果你对翻译感兴趣，请翻译 [这个部分](https://github.com/NoBrain0917/ADOFAI-Replay/blob/master/Replay/Languages/English.cs) 及将它提交到一个拉取请求或发送 DM 到 `᲼᲼#8850`

---

## 特别致谢
![sans](https://github.com/NoBrain0917/Replay/blob/master/Resource/specialtanks.gif?raw=true)
- ppapman（UI 设计及反馈）
- sjk（日语翻译）
- ChocoSwi
- 서재형
- kimkealean
- Luya
- SHADOW_SDW
