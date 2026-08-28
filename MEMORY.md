# RPG 项目记忆文件

> 本文件是长期记忆，平时不用读。只有用户明确说"读记忆/读memory"时才读取。
> 内容 = 策划文档要点 + 当前项目结构。源文档：`D:\A工作文件\RPG游戏策划文档.md`（如策划有更新，需同步本文件）。

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
- 位置：`D:\A工作文件\RPG`（Unity 工程根，解决方案 RPG.sln；另有 2DGame.sln 旧文件）
- 引擎版本：Unity 2022.3.62f3c1（团结引擎生态，含 cn.tuanjie.codely.bridge 1.0.76 + TJGenerators）

### 场景（Assets/Scenes）
- `MainScene.unity`（主游戏场景）
- `Start.unity`（开始场景）

### 现有脚本（Assets/Scripts，共 17 个，无子目录）
- 玩家：PlayerMove（移动）、PlayerCombat（攻击）、PlayerHealth（生命）、StatsManager（属性）、ExpController（经验）
- 敌人：EnemyMovement（状态机/追击）、EnemyCombat（攻击）、EnemyHealth（生命）
- 战斗：CombatCalculator（伤害计算）、DamageText / DamageTextSpawner（伤害飘字）、HitstopController（顿帧）、CameraShake（屏幕震动）
- 场景/其他：LoadScene（场景加载）、Main（主入口）、StatsUI、MountainCollidersEnter/Exit（区域触发器）

### 美术资源
- Assets/Spirits：动画文件夹、动画贴图、地面、建筑
- Assets/Sources/2D资源包/Tiny Swords：素材包（tiny swords 2D 素材）

### 插件
- Assets/插件/AVProVideo：视频插件（含 Demos/Docs/Editor/Extensions/Runtime，另有 Timeline/UnityUI/VisualEffectGraph 扩展）

### 关键包（Packages/manifest.json）
- com.unity.cinemachine 2.10.6、com.unity.feature.2d 2.0.1、com.unity.textmeshpro 3.0.7、com.unity.timeline 1.7.7、com.unity.ugui 1.0.0、visualscripting、2D 全套（Sprite/Tilemap/Animation/IK/PixelPerfect/PsdImporter）
- cn.tuanjie.ai.generators（TJGenerators 本地包）、cn.tuanjie.codely.bridge 1.0.76

### 状态备注
- 已有：移动/攻击（动画事件驱动）/击退硬直/敌人状态机/经验升级/伤害飘字/顿帧/震屏
- 未做（第一阶段缺口）：道具数据架构(SO)、掉落表、拾取、背包、装备穿戴
