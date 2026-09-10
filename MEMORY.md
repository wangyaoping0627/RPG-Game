# RPG 项目记忆文件

> 本文件是长期记忆，平时不用读。只有用户明确说"读记忆/读memory"时才读取。
> **权威方案文档 = `D:\A工作文件\RPG实现方案.md`**（唯一方案，含全部设计 + 实现代码 + ✅/🟡/❌ 进度标记）。
> 内容 = 策划要点 + 项目结构 + 开发进度摘要。策划/实现有更新同步 RPG实现方案.md，本文件摘要跟进。

---

## 一、策划文档要点

### 项目概述
- 项目名：RPG（暂定）；类型：2D 俯视角动作 RPG
- 引擎：Unity 2022.3.62f3c1
- 核心玩法：打怪 → 爆装备 → 穿戴变强 → 挑战更强的怪
- 用途：秋招作品集 + 毕业论文

### 核心战斗循环
```
打怪 → 击杀掉装备 → 拾取进背包 → 穿戴变强 → 挑战更强的怪/Boss
```

**玩家属性**：HP（归零死亡，药水回复）、攻击力（基础+装备）、移动速度（基础+装备，Ctrl 加速跑）、经验/等级（升级提升属性并回满血）、武器范围（近战判定半径）、击退/硬直。

**操作**：WASD 移动；J/左键 攻击（冷却制）；Ctrl 加速跑；I 背包；C 角色装备面板；ESC 暂停；数字键 1-4 快捷栏用消耗品。

**战斗机制**：冷却制近战、动画事件驱动伤害判定（已有）；攻击者与被击者都有击退+硬直（已有）；伤害飘字（区分暴击/普通）；打击感=屏幕震动+顿帧+粒子特效。

### 装备与掉落
- 道具分类：装备（武器/头盔/护甲/靴子/饰品）、消耗品（生命/增益药水）、材料（合成/升级预留）
- 装备槽影响：武器→攻击力+武器范围；头盔/护甲→生命+防御；靴子→移速；饰品→特殊效果（暴击、吸血）
- 品质：普通白 #FFFFFF 1.0x 掉率50% ｜ 优秀蓝 #4A9EFF 1.3x 30% ｜ 稀有紫 #B048F0 1.6x 15% ｜ 传说金 #FFB000 2.0x 5%
- 掉落系统：每敌人一张掉落表（物品列表+掉率+数量范围），死亡随机抽取，尸体处生成掉落物，玩家走近自动拾取

### 敌人系统
- 状态机（已有，待扩展）：Stand →(玩家进检测范围) Chase →(进攻击范围) Attack →(动画结束) Stand
- 待扩展状态：Hit（受击硬直）、Death（死亡动画后销毁）、Patrol（巡逻，可选）
- 类型：普通小怪（低品质装备/材料）、精英怪（中高品质，有技能）、Boss（保底高品质，多技能）
- 刷怪：区域刷怪点（配置数量+冷却），Boss 区波次刷怪（可选），玩家离开后重置

### 背包与物品
- 背包 5x8=40 格网格布局，可堆叠；操作：使用/穿戴/丢弃/查看详情；满时提示"背包已满"
- 快捷栏：底部 4 格，可拖入消耗品，数字键 1-4 使用

### UI 系统
- HUD：血条（左上，红，当前/最大HP）、经验条（底部，蓝）、等级 Lv.X、快捷栏（底部居中 4 格）、小地图（可选，右上）
- 面板：背包(I)、角色面板(C，装备槽+属性总览)、暂停菜单(ESC)、主菜单
- 交互反馈：拾取提示"获得 [物品名]"、升级特效+音效+UI动画、装备槽高亮

### 存档系统
- 内容：玩家（等级/经验/HP/位置/朝向）、背包、已穿戴装备、游戏进度（击杀Boss/解锁区域）
- 技术：JSON 存 `Application.persistentDataPath`，预留 3 存档位，切换场景自动存档

### 音效系统
- 挥砍/命中、受击、拾取、升级、死亡音效；BGM：主菜单/战斗/Boss
- 技术：AudioManager 单例统一管理 BGM 和 SFX

### 开发阶段规划（5 阶段 24 模块）
1. **核心战斗循环**：①道具数据架构(ScriptableObject) ②掉落系统 ③拾取系统 ④背包系统 ⑤装备系统(动态改属性) ⑥伤害飘字
2. **内容扩展**：⑦多种敌人 ⑧刷怪系统 ⑨Boss ⑩消耗品 ⑪装备稀有度
3. **UI 完整化**：⑫背包UI(拖拽) ⑬装备栏UI ⑭HUD完善 ⑮主菜单 ⑯暂停菜单 ⑰拾取提示
4. **存档与系统完善**：⑱存档系统 ⑲地图扩展(多区域+安全区) ⑳音效系统 ㉑打击感优化
5. **收尾打磨**：㉒数值平衡 ㉓README(架构说明+技术亮点+截图) ㉔论文素材

### 毕业论文可选方向
- A：状态机+事件解耦 → "基于事件驱动的 2D ARPG 系统架构设计"
- B：装备/掉落系统 → "2D ARPG 中可扩展掉落系统的设计与实现"
- C：数据驱动设计 → "基于 ScriptableObject 的 Unity 游戏数据架构研究"

---

## 二、当前项目结构（截至记录时）

### 工程
- 位置：`D:\A工作文件\RPG`（Unity 工程根，解决方案 RPG.sln；旧 2DGame.sln 已删）
- 引擎版本：Unity 2022.3.62f3c1（团结引擎生态，含 cn.tuanjie.codely.bridge 1.0.76 + TJGenerators）
- 架构文档：工程根 `README.md`（目录分层/数据流/事件表/关键约定）；**唯一权威实现方案 = `D:\A工作文件\RPG实现方案.md`**（含全部设计 + 实现代码 + 进度状态），开发/改动先看它

### 场景（Assets/Scenes）
- `MainScene.unity`（主游戏场景）
- `Start.unity`（开始场景）

### 脚本目录（Assets/Scripts，按模块分层，共 36 个）
- `Core/`：StatsManager（属性单例）、Main（入口）、SaveSystem（存档）、SaveController（存档挂载点）、AudioManager（音频单例）
- `Player/`：PlayerMove、PlayerCombat、PlayerHealth
- `Enemy/`：EnemyMovement（状态机，含 SetLocked 外部锁）、EnemyCombat、EnemyHealth、BossController（Boss 技能）
- `Items/`（掉落拾取数据链）：ItemData(SO)、ItemStack、Inventory、EquipmentManager、EnemyLoot（留在 Items 因其与掉落链耦合）、PickupSpawner、PickupItem
- `Combat/`：CombatCalculator、DamageText、DamageTextSpawner、HitstopController、CameraShake
- `UI/`：StatsUI、ExpController、MainMenu、PauseMenu、InventoryUI、EquipmentUI、HotbarUI、PickupToast
- `World/`：LoadScene、MountainCollidersEnter、MountainCollidersExit、EnemySpawner（刷怪点）

### 美术资源
- Assets/Spirits：动画文件夹、动画贴图、地面、建筑
- Assets/Sources/2D资源包/Tiny Swords：素材包（tiny swords 2D 素材）

### 插件
- Assets/插件/AVProVideo：视频插件（含 Demos/Docs/Editor/Extensions/Runtime，另有 Timeline/UnityUI/VisualEffectGraph 扩展）

### 关键包（Packages/manifest.json）
- com.unity.cinemachine 2.10.6、com.unity.feature.2d 2.0.1、com.unity.textmeshpro 3.0.7、com.unity.timeline 1.7.7、com.unity.ugui 1.0.0、visualscripting、2D 全套（Sprite/Tilemap/Animation/IK/PixelPerfect/PsdImporter）
- cn.tuanjie.ai.generators（TJGenerators 本地包）、cn.tuanjie.codely.bridge 1.0.76

### 状态备注（进度快照，权威详见 `D:\A工作文件\RPG实现方案.md` 第十八节）
- ✅ 已完成：
  - 战斗手感全套：攻击状态机(前摇/判定/后摇)、判定框Hitbox、输入缓存、攻击锁移动、无敌帧+受击闪烁、击退EaseOut曲线、伤害飘字/顿帧/震屏、伤害公式(攻防减伤/暴击/浮动)
  - 敌人：5状态机(Stand/Chase/Attack/HitStagger/Death)、追击攻击、受击硬直、死亡动画销毁、血条UI
  - 掉落链：ItemData(SO)、品质系统、掉落表(EnemyLoot)、动态掉落物(PickupSpawner)、自动拾取(PickupItem)、背包40格(Inventory)、装备穿脱+增量写回(EquipmentManager)
  - 玩家：移动/加速/朝向、经验升级(StatsManager)、经验条+等级UI(ExpController)
  - 场景：Start/MainScene、场景切换淡入淡出(LoadScene的Scene类)、山体碰撞边界
- ✅ 本轮已完成：数值基线(damage=10/maxHealth=100/weaponRange=1.5，升级+3/+20)、weaponRange 生效(判定框 radius 跟随)、装备不再堆叠、物品资产迁移到 `Assets/Resources/Items/`
- 🔵 代码已就绪待 Unity 接线(11 个)：MainMenu、PauseMenu、InventoryUI、EquipmentUI、HotbarUI、PickupToast、EnemySpawner(刷怪点)、BossController(Boss 技能)、SaveSystem(存档)、SaveController(存档挂载点)、AudioManager(音频)；音效接线已内建到 PlayerCombat/PlayerHealth/PickupItem/StatsManager/EnemyHealth；**接线步骤见 `D:\A工作文件\RPG_Unity接线清单.md`**
- 🟡 简陋待打磨：玩家死亡(直接SetActive false)、连击(连按重播)、血条(纯文本)、属性面板(StatsUI仅3属性)、装备资产(Resources/Items 有12个示例)、README(缺截图)
- ❌ 未实现：Unity 接线(搭UI/挂组件)、多种敌人(场景仅1只 HP=20)、敌人预制体化、音频资源生成、README补全+截图+视频
- 关键实现约定（沿用）：
  - **UI 全部由用户自建**；拾取反馈靠订阅 `Inventory.OnChanged`，属性刷新靠订阅 `StatsManager.OnStatsChanged`/`EquipmentManager.OnChanged`
  - **创建道具 = 手动**：右键 Create > RPG/Item 新建 SO 资产；**必须放在 `Assets/Resources/Items/`**（存档靠 `Resources.LoadAll` 查回，已有 12 个示例资产）
  - 敌人挂 `EnemyLoot` 组件 + 掉落表即可掉落
  - **代码缺口**：`EquipmentManager.Equip` 不自动移出背包(穿戴前需 `Inventory.RemoveAt`)、`Unequip` 不自动放回背包(脱下后需 `Inventory.Add`)
