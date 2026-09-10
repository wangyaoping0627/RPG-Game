# 项目介绍

一个 2D 俯视角动作 RPG，用 Unity开发。

## 脚本说明

### Core

StatsManager 是玩家所有属性的存放处，做成跨场景常驻的单例。血量、攻击、防御、移速、暴击率、武器范围都记在这里。装备和升级往里写，战斗代码只负责读。

Main 是入口，目前只设了一下目标帧率。

SaveSystem 负责存档，把玩家属性、背包和已穿戴的装备序列化成 JSON，存到 persistentDataPath。物品不存对象引用，而是存 ItemData 的 id 字符串，读档时从 Resources/Items 反查回来。

SaveController 是存档的挂载点。SaveSystem 是静态类，需要一个挂场景的组件来触发它，所以用它负责进场景时自动读档、按键存读档，以及退出游戏时保存。

AudioManager 统一管理背景音乐和音效，同样是常驻单例。别的脚本直接调它的静态方法放声音，音频字段空着就静默跳过，不会影响运行。

### Player

PlayerMove 处理移动输入、朝向翻转和受击击退。攻击的前摇和判定帧期间会锁住移动，让每一次出手都有代价。击退用的是一条衰减曲线，而不是给个瞬间速度再直接归零。

PlayerCombat 是攻击状态机，把一次攻击拆成前摇、判定、后摇三段，由动画事件推动。伤害判定挂在攻击点下的圆形触发器上，同一次攻击不会重复打到同一个敌人。另外还做了输入缓存，后摇期间按攻击键，收招后会自动接上下一刀。

PlayerHealth 管理玩家血量，对外只留一个 TakeDamage 入口。受击后会有半秒无敌时间和红色闪烁，避免被敌人连着打到死。

### Enemy

EnemyMovement 是敌人的状态机，有待机、追击、攻击、受击硬直、死亡五个状态。另外提供一个 SetLocked 方法，给 Boss 技能在释放期间锁住移动用。

EnemyCombat 管敌人的攻击判定，由动画事件触发，检查完距离再调用玩家的 TakeDamage。

EnemyHealth 是敌人的受击入口，负责扣血、更新血条、进入硬直或者死亡。死亡时会广播事件、触发掉落、通知状态机播放死亡动画，最后销毁自己和头顶的血条。

BossController 给 Boss 加技能，按固定间隔循环释放冲刺、砸地范围伤害和召唤小怪。血量掉到一半以下进入狂暴，移速和攻击频率都会提升。

### Items

ItemData 是道具的 ScriptableObject，定义一件装备或者消耗品的全部静态数据。在 Project 窗口右键 Create 里的 RPG/Item 就能新建，装备的各类加成会自动乘上品质倍率。

ItemStack 是背包里的一格，记着道具数据和数量。

Inventory 是背包数据，静态类，四十格上限。消耗品和材料可以堆叠，装备不堆叠，每件各占一格。每次增删都会广播 OnChanged，界面订阅它刷新。

EquipmentManager 管五个装备槽的穿戴和脱下。它用增量法把装备加成写回 StatsManager：先记录当前装备的总加成，穿脱时只写新旧差值。这样不会破坏升级时直接改属性的那套逻辑。

EnemyLoot 挂在敌人身上配掉落表，每一项有物品、掉率和数量范围，敌人死亡时按表随机抽取。

PickupSpawner 是静态类，在尸体位置直接拼一个带精灵和触发器的物体当掉落物，不需要预制体。物品没配图标时用品质颜色的方块兜底。

PickupItem 是掉落物上的拾取逻辑，玩家走过去自动进背包，背包满了就不捡。

### Combat

CombatCalculator 是伤害计算工具，处理攻防减伤、暴击和正负百分之十的浮动。减伤用的是防御力除以防御力加一百的曲线，保证堆防御的收益递减，不会出现无敌的情况。

DamageText 和 DamageTextSpawner 负责伤害飘字。前者控制数字上浮和淡出，暴击时字号更大、颜色更亮；后者把敌人的世界坐标转成界面坐标再生成。

HitstopController 是顿帧，命中瞬间把时间放慢到四分之一，恢复时用不受 timeScale 影响的计时。

CameraShake 挂在主相机上，命中时短暂抖动，暴击和击杀的幅度更大。

### UI

ExpController 订阅敌人死亡事件，给玩家加经验，同时刷新经验条和等级文字。

StatsUI 是早期做的属性面板，按 OpenMenu 键开关，只显示血上限、攻击和移速三项。

MainMenu 是开始场景的菜单，两个按钮分别进游戏和退出。

PauseMenu 按 ESC 开关暂停面板，暂停时把 timeScale 设成零。

InventoryUI 是背包面板，订阅 Inventory.OnChanged 刷新格子。点装备会先把物品移出背包再穿戴，点消耗品会回血并扣掉一个。打开面板时游戏暂停。

EquipmentUI 是装备面板，显示五个槽位和属性总览。点某个槽位会把装备脱下来放回背包。

HotbarUI 是底部快捷栏，数字键使用绑定的消耗品，会从背包里扣掉一个。

PickupToast 订阅背包变化，拾取到东西时在屏幕中间短暂显示物品名。

### World

LoadScene 里的 Scene 类负责场景切换。它做了一层黑屏遮罩，在屏幕全黑的时候完成后台加载和场景激活，玩家看不到场景创建时的卡顿。

MountainCollidersEnter 和 MountainCollidersExit 处理山体碰撞区域的进出。

EnemySpawner 是刷怪点，开局刷满指定数量，每只敌人死亡后等冷却时间补刷一只。

## 关联

整个项目的骨架是一条数据链，从敌人死亡一直走到玩家变强：

1. 敌人血量归零，EnemyHealth 广播死亡事件，ExpController 收到后给玩家加经验
2. 同一个地方调用 EnemyLoot，按掉落表抽出这次要掉的东西
3. PickupSpawner 在尸体位置生成掉落物
4. 玩家走过去，PickupItem 触发自动拾取，物品进入 Inventory
5. Inventory 广播 OnChanged，背包面板和拾取提示刷新
6. 玩家点一下格子，EquipmentManager 把装备穿上
7. 装备加成写回 StatsManager，广播 OnStatsChanged
8. 血条、经验条和属性面板刷新，PlayerCombat 与 PlayerMove 读到新的攻击力和移速

项目里主要用到的事件有这几个：

- EnemyHealth.OnAnyEnemyDeath，任意敌人死亡时触发并带上经验值，ExpController 订阅它加经验
- EnemyHealth.OnDeath，单个敌人实例的死亡事件，刷怪点订阅它安排补刷
- StatsManager.OnStatsChanged，属性变化时触发，血条和属性面板订阅
- Inventory.OnChanged，背包增删时触发，背包面板、快捷栏和拾取提示订阅
- EquipmentManager.OnChanged，装备槽发生变化时触发，装备面板订阅

事件是主要的解耦手段。逻辑层只管广播，界面层自己订阅，所以以后加新界面不需要动逻辑代码。需要注意的是，订阅了事件的对象要在 OnDestroy 里退订，写法可以参考 ExpController 和 PlayerHealth。

除了事件，还有几条贯穿全局的约定：

属性只有 StatsManager 一个出口。玩家的所有数值都放在那里，装备和升级通过它写回，战斗代码只读不写。

单例有两种形态。需要挂在场景对象上的用 MonoBehaviour 单例，比如 StatsManager；纯数据管理器用静态类，比如 Inventory 和 EquipmentManager，它们没有场景对象，将来做存档时自己序列化内部状态。

掉落物不用预制体。PickupSpawner 在运行时拼装，美术资源到位后只改它一处。

道具数值只写进 ItemData 资产。装备的最终加成等于基础值乘品质倍率，调数值不用碰代码。

## 开发进度

已经能跑通的：移动、攻击、击退和硬直、敌人状态机、经验升级、伤害飘字、顿帧、震屏、掉落、拾取，以及背包和装备的数据逻辑。攻击手感做得比较完整，包含前摇判定后摇的拆分、判定框、输入缓存、无敌帧和击退曲线。

正在做的：背包和装备的界面、快捷栏、拾取提示、多种敌人、Boss、刷怪点、存档和音效。

## 未来规划

近期的目标是把一套十分钟能玩完的完整流程做出来。玩家从出生点出发，一路打小怪、捡装备成长起来，最后挑战 Boss 通关，同时把存档和音效补齐。

想扩展的方向：更丰富的敌人类型和精英怪、装备的合成与升级、分成几个区域的更大一点的关卡。
