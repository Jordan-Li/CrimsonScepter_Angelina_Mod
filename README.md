# 杀戮尖塔2：安洁莉娜角色Mod

> 环境依赖：本Mod基于BaseLib开发，游玩此Mod前务必先安装BaseLib。

## 项目概览与进度

### 项目概览

- 这是一个基于C#语言编写的杀戮尖塔2的角色mod，角色为《明日方舟》游戏中的安洁莉娜。
- 作者：Bilibili-绯红权杖攻略组

### 开发进度

- [x] 游戏主题卡牌的设计与制作（目前卡池60张左右，后续会继续增加）
- [x] 角色模型与Spine动画的绑定
- [ ] 专享的遗物、药水等比较少
- [ ] 卡牌卡图（卡图当前均为beta版手绘测试卡图，需要为爱发电的画师大大）
- [ ] ……

## 安洁莉娜的背景与机制设计

> 以下部分适合想要快速了解安洁莉娜背景和机制的玩家。

安洁莉娜是一名能够操控重力的信使，根据这一角色背景我们给安洁莉娜设计了两大主要的机制，分别是**失衡**与**寄送**.

### 失衡（Imbalance) 与 失重 (Weightless)

- **失衡**是参考了终末地战斗设计时产生的概念。在安洁莉娜的卡牌中，有大量能够基于地方**失衡**的效果。**失衡**的游戏设计为：当**失衡**累计达到敌方生命上限的一半时（最多累计到100），会清除所有**失衡**，造成**失衡**一半的伤害，并使敌方进入失重状态，持续三回合。
- 失重则是**失衡**爆发后的一种状态，在失重状态下，敌方受到的**失衡**效果会直接变为失去对应的生命，并且在失重状态下敌方受到的伤害翻倍。
- 总的来说是一个带有打断效果的延迟收益效果，越强大的敌人就越难打断，打断本身有三回合的固定CD，所以也没办法一直打断敌人。如何让对方尽可能快的进入**失衡**，也是使用安洁莉娜攻略尖塔的一个重要挑战。

### 寄送（Delivery）

- **寄送**是安洁丽娜作为一个信使必不可少的技能。**寄送**可以将玩家的手牌送出去，并且玩家会在下一个回合的抽牌之前，收到送回来的卡牌。
- **寄送**的效果可以理解成弃牌与保留的叠加，被**寄送**的卡牌在本回合暂时消耗。配合失衡这一机制的加伤窗口期，可以将高攻击的牌**寄送**到下回合再打出。
- 除此之外**寄送**还有一个衍生的机制**送达**。当某张有**送达**词条的牌因**寄送**回到手中时，会执行词条后对应的效果。

### 飞行

- 安洁莉娜都会操控重力，那**飞行**自然是信手拈来。在本Mod中，我们沿用了杀戮尖塔1中关于**飞行**的设定：即直接受到的攻击伤害减半，受到攻击时减少一层。
- 安洁莉娜既可以给自己施加**飞行**效果，也可以给敌方施加**飞行**效果。这个**飞行**效果构成了安洁莉娜的防御核心。当敌方因为被攻击退出**浮空**（出于对原作的尊重，**振翅**和**飞行**都属于**浮空**状态）状态时，会触发初始遗物**绯红权杖**的效果，敌方会获得部分**失衡**。

### 法术伤害 & 法术格挡

- 安洁莉娜本身就是一名法师，造成的伤害当然是法术伤害。 **法术**是一个前置词条，当卡牌描述为造成法术伤害或者获得法术格挡时，这个效果就不会受到力量、敏捷、易伤、虚弱等Buff的影响，取而代之的是其会受到集中的影响。
- 法术伤害不是直接攻击，不会受到飞行的减伤，也不会消耗对方飞行的层数，本Mod中有许多能力牌的效果会和法术进行联动。
- 尝试去平衡手牌中法术伤害和普通伤害的数量，来对敌方的飞行状态进行击落也是爬塔时需要考虑的点。

## 项目文件结构

```text

CrimsonScepter_Angelina_Mod_AI/
├── CrimsonScepter_Angelina_Mod.csproj          # C# 项目文件
├── CrimsonScepter_Angelina_Mod.sln             # 解决方案文件
├── CrimsonScepter_Angelina_Mod.json            # Mod 清单
├── project.godot                               # Godot 项目入口
├── export_presets.cfg                          # Godot 导出配置
├── Directory.Build.props                       # 本地构建参数
├── Sts2PathDiscovery.props                     # STS2 路径发现配置
├── spine_godot_extension.gdextension           # Spine GDExtension 配置
├── README.md                                   # 当前说明文档
├── CrimsonScepter_Angelina_Mod/                # 资源目录（res://CrimsonScepter_Angelina_Mod）
│   ├── mod_image.png                           # Mod 展示图
│   ├── images/
│   │   ├── card_portraits/                     # 卡图资源
│   │   │   ├── beta/                           # 当前测试卡图
│   │   │   └── big/                            # 预留大图目录
│   │   ├── charui/                             # 角色 UI 图：头像、选人图、能量图、地图标记
│   │   ├── powers/
│   │   │   └── big/                            # Power 大图目录
│   │   ├── relics/
│   │   │   └── big/                            # Relic 大图目录
│   │   └── screens/
│   │       └── char_select_bg_angelina.png     # 选人背景图
│   ├── localization/
│   │   ├── eng/                                # 英文本地化
│   │   └── zhs/                                # 中文本地化
│   ├── scenes/
│   │   ├── combat/
│   │   │   └── energy_counters/
│   │   │       └── angelina_energy_counter.tscn
│   │   ├── creature_visuals/
│   │   │   └── angelina.tscn
│   │   ├── merchant/
│   │   │   └── characters/
│   │   │       └── angelina_merchant.tscn
│   │   ├── rest_site/
│   │   │   └── characters/
│   │   │       └── angelina_rest_site.tscn
│   │   └── screens/
│   │       └── char_select/
│   │           └── char_select_bg_angelina.tscn
│   └── spine_data/                             # Spine 原始数据与 Godot 资源封装
│       ├── build_char_291_aglina.atlas
│       ├── build_char_291_aglina.png
│       ├── build_char_291_aglina.skel
│       ├── build_char_291_aglina.tres
│       ├── build_char_291_aglina_rest.atlas
│       ├── build_char_291_aglina_rest.png
│       ├── build_char_291_aglina_rest.skel
│       └── build_char_291_aglina_rest.tres
├── CrimsonScepter_Angelina_ModCode/            # 核心 C# 代码
│   ├── MainFile.cs                             # Mod 初始化入口
│   ├── Abstracts/                              # 卡牌 / Power / Relic / Potion 基类
│   ├── Cards/                                  # 所有卡牌实现
│   ├── Character/                              # 角色本体与卡池 / 遗物池 / 药水池
│   ├── Extensions/                             # 路径与字符串扩展
│   ├── Helpers/                                # 辅助逻辑
│   ├── Potions/                                # 药水目录（当前为空）
│   ├── Powers/                                 # 所有 Power 实现
│   ├── Relics/                                 # 所有遗物实现
│   └── Scripts/                                # Godot 场景绑定脚本
│       ├── AngelinaMerchantCharacter.cs
│       └── AngelinaRestSiteCharacter.cs
├── .godot/                                     # Godot 自动生成目录
├── bin/                                        # 编译 / 发布输出
└── windows/                                    # 导出运行时相关目录

```


