# Unity
Unity第三人称射击游戏相关代码

使用Unity内置IK系统，实现人物脚部触地，及上半身始终看向目标的动作。

FootIK和AimIK用于IK位置的计算，IkManager用于管理权重以及IK的最终执行，它们之间使用接口进行通讯。
